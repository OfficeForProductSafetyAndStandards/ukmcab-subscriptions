using Azure.Data.Tables;
using UKMCAB.Subscriptions.Core.Common;
using UKMCAB.Subscriptions.Core.Domain;

namespace UKMCAB.Subscriptions.Core.Data;

public interface IBlockedEmailsRepository : IRepository
{
    Task BlockAsync(EmailAddress emailAddress);
    Task<bool> IsBlockedAsync(EmailAddress emailAddress);
    Task UnblockAsync(EmailAddress emailAddress);
}

public class BlockedEmailsRepository : Repository, IBlockedEmailsRepository
{
    public BlockedEmailsRepository(AzureDataConnectionString dataConnectionString) : base(dataConnectionString, $"{SubscriptionsCoreServicesOptions.TableNamePrefix}blockedemail") { }
    public async Task<bool> IsBlockedAsync(EmailAddress emailAddress) => await GetAsync<TableEntity>(new Keys(string.Empty, emailAddress)) != null;
    public async Task BlockAsync(EmailAddress emailAddress) => await UpsertAsync(new TableEntity(string.Empty, emailAddress));
    public async Task UnblockAsync(EmailAddress emailAddress) => await DeleteAsync(new Keys(string.Empty, emailAddress));
}

