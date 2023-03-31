using UKMCAB.Subscriptions.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using UKMCAB.Subscriptions.Core.Abstract;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSubscriptionServices(new SubscriptionServicesCoreOptions("https://ukmcab-dev.beis.gov.uk", "todo"));
var app = builder.Build();

var sub = app.Services.GetRequiredService<ISubscriptionService>();

