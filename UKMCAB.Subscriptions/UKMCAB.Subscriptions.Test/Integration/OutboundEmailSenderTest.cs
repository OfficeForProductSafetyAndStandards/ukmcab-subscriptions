using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;

namespace UKMCAB.Subscriptions.Test.Integration;
public class OutboundEmailSenderTest
{
    [Test, Explicit]
    public async Task SendEmailTest()
    {
        var subj = new OutboundEmailSender(Bootstrap.Configuration["GovUkNotifyApiKey"] ?? throw new Exception("GovUkNotifyApiKey is null"), OutboundEmailSenderMode.Send);
        await subj.SendAsync(new Core.Domain.Emails.EmailDefinition("bbc4db80-8b9b-45f4-bb24-33cc180843c8", "kris.dyson@beis.gov.uk", 
            new Dictionary<string, string>
            {
                ["confirm_link"] = "https://localhost:7061/subscriptions/subscribe/cab/confirm?token=uGilquH6-j5qfAv4OjSxA9kPg3nePD19y3RrkP3AL9Hw87Wr9jqGeOC8GTvJy5fu4BH5j4Wc4TOUjG2hp4vEnvrKZUqHPJcFzE1YthpTE1j6tT3RReKTtG_M-uKuiVymzSe9bT1maAAbZwUYZmFUDjPBm3n4jRBDr_FvefyV8MMZHbQmo6C9ApzZYqH4QW38sY4Igl3ZwYNFIxMe9hb5Cw"
            },
            new Dictionary<string, string> ())).ConfigureAwait(false);

        Assert.That(subj.Requests.Count, Is.EqualTo(1));
    }
}
