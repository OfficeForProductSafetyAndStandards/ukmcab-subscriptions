# Guidance for developing the email subscriptions functionality

## Overview
The core package will be hosted inside two different contexts:
- The UKMCAB ASP.NET Core web app
- The UKMCAB Azure Function

As such, this package exposes two main services that cater for the overall functionality of the package:
- `ISubscriptionService`
- `ISubscriptionEngine`

### ISubscriptionService
`ISubscriptionService` is purposed for the UKMCAB web application and allows users to:
	- subscribe to searches
	- subscribe to cab updates
	- check whether the email address is subscribed to a given CAB or search
	- unsubscribe from searches/cab-updates

### ISubscriptionEngine
`ISubscriptionEngine` is purposed for the Azure functions app.  It contains three methods that will be called in parallel and whose run-time will be continuous (until CancellationToken.IsCancellationRequested==true).
- ProcessSearchSubscribersAsync \
This will be called once-per-hour. It will process any search changes and aggregate a list of changes that will eventually be notified to the subscriber.
- ProcessCabSubscribersAsync\
This method will be continuous. It looks for CAB updated messages on the Azure Storage Queue that tells it that a CAB has changed.
- ProcessSendEmailNotificationsAsync
This method will be continuous. It looks for emails that are due to be sent and sends them when they are due.

## CAB updates
### How it works:
- The user subscribes to CAB updates by entering their email address on the front-end. The front-end will call `ISubscriptionService.SubscribeAsync(string emailAddress, Guid cabId, Frequency frequency)`,
supplying the email address, the CAB id and the Frequency of requested emails.
- A snapshot of the CAB record is persisted, along with the email address, CAB id, frequency and the UTC datetime of the subscription request (`ISubscriptionRepository`)

_That's the end of the CAB updates subscription process_

#### How updates are sent:
- The Azure Function host is running the following method continuously: `ISubscriptionEngine.ProcessCabSubscribersAsync(CancellationToken cancellationToken)`
- Someone changes a CAB on the UKMCAB front-end.  The front-end posts to an Azure Storage Queue a `CabUpdateMessage` that contains `CabId` and `Name`.
- `ISubscriptionEngine.ProcessCabSubscribersAsync` picks up the message and persists an email in `IOutboxRepository`; if there's already an item for this subscription in
`IOutboxRepository` then that one is overritten with the latest email; Messages can stay in the outbox for days until the email is actually due (according to frequency rules)
- `ISubscriptionEngine.ProcessSendEmailNotificationsAsync` is continuously running.  It looks for emails in the outbox that are due, by looking at the frequency of the
associated subscription and the date of the last email sent on this subscription (or date the subscription was created) and then sends the email.  
It also records the sent-email in `ISentNotificationRepository`


## NOTES
Development will be entirely test-driven and use integration-fakes in-place of real implementations that talk to real services.
- _There will be no email sent_
- _No need to interact with a real web api from UKMCAB_

# TDD Fakes
- `ICabUpdatesReceiver`: `FakeCabUpdatesReceiver`\
	Use this to fake the behaviour of an Azure Storage Queue
- `IDateTimeProvider`: `FakeDateTimeProvider`\
	Use this to fake the progression of time, so you can test the Frequency rules
- `ICabSearchService`: `FakeCabSearchService`\
	Rather than trying to interact with the real UKMCAB service to do a real search, just inject what search results you expect here
