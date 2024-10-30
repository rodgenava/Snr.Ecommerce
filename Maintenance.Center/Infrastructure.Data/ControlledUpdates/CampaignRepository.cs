using Application.Repositories.ControlledUpdates;
using Core.ControlledUpdates;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data;
using System.Data.Common;

namespace Infrastructure.Data.ControlledUpdates
{
    public class CampaignRepository : ICampaignRepository
    {
        private readonly ISqlProvider _provider;

        private readonly ISqlCaller _caller;

        private readonly int _commandTimeout;

        public CampaignRepository(string connectionString, int commandTimeout)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connectionString));
            _commandTimeout = commandTimeout;
        }

        public async Task<Campaign?> FindAsync(Guid key, CancellationToken token)
        {
            SqlTransaction transaction = _caller.CreateScopedTransaction(IsolationLevel.ReadCommitted);

            DbCommand command = _provider.CreateCommand(
                commandString: $"Select Id,[Description],[Begin],[End] From [ControlledUpdates].[Campaigns] Where Id='{key}'",
                commandType: CommandType.Text);

            command.CommandTimeout = _commandTimeout;

            try
            {
                IEnumerable<CampaignDataHolder> campaignDataHolders = await transaction.GetAsync(
                    dataMapper: new ReflectionDataMapper<CampaignDataHolder>(),
                    command: command,
                    token: token);

                CampaignDataHolder? campaignData = campaignDataHolders.FirstOrDefault();

                if(campaignData == null)
                {
                    return null;
                }

                command.CommandText = $"Select WarehouseCode From [ControlledUpdates].[CampaignWarehouses] Where Campaign='{key}'";

                IEnumerable<WarehouseDataHolder> warehouseDataHolders = await transaction.GetAsync(
                    dataMapper: new ReflectionDataMapper<WarehouseDataHolder>(),
                    command: command,
                    token: token);

                command.CommandText = $"Select Scope From [ControlledUpdates].[CampaignScopes] Where Campaign='{key}'";

                IEnumerable<ScopeDataHolder> scopeDataHolders = await transaction.GetAsync(
                    dataMapper: new ReflectionDataMapper<ScopeDataHolder>(),
                    command: command,
                    token: token);

                await transaction.CommitAsync(token);

                return Campaign.Existing(
                    id: campaignData.Id,
                    description: new Description(Value: campaignData.Description),
                    duration: new Duration(
                        begin: DateOnly.FromDateTime(campaignData.Begin),
                        end: DateOnly.FromDateTime(campaignData.End)),
                    warehouses: from item in warehouseDataHolders
                                select new Warehouse(Code: item.Code),
                    scopes: from item in scopeDataHolders
                            select new Scope(Id: item.Id));
            }
            finally
            {
                transaction.Dispose();
                command.Dispose();
            }
        }

        private class CampaignDataHolder
        {
            public Guid Id { get; set; }
            public string Description { get; set; } = string.Empty;
            public DateTime Begin { get; set; }
            public DateTime End { get; set; }
        }

        private class WarehouseDataHolder
        {
            public string Code { get; set; } = string.Empty;
        }

        private class ScopeDataHolder
        {
            public Guid Id { get; set; }
        }

        public async Task SaveAsync(Campaign item, CancellationToken token)
        {
            var events = item.ReleaseEvents();

            SqlTransaction transaction = _caller.CreateScopedTransaction(IsolationLevel.ReadCommitted);

            try
            {
                foreach (var @event in events)
                {
                    switch (@event)
                    {
                        case CampaignCreatedDataEvent ccde:
                            await WriteEvent(ccde, transaction, token);
                            break;
                        case CampaignDurationChangedDataEvent cdcde:
                            await WriteEvent(cdcde, transaction, token);
                            break;
                        case CampaignWarehousesChangedDataEvent cwcde:
                            await WriteEvent(cwcde, transaction, token);
                            break;
                        case CampaignScopesChangedDataEvent cscde:
                            await WriteEvent(cscde, transaction, token);
                            break;
                        case CampaignCancelledDataEvent ccde:
                            await WriteEvent(ccde, transaction, token);
                            break;
                    }
                }

                await transaction.CommitAsync(token);
            }
            catch
            {
                await transaction.RollbackAsync(token);

                throw;
            }
            finally
            {
                transaction.Dispose();
            }
        }

        private async Task WriteEvent(CampaignCreatedDataEvent @event, SqlTransaction transaction, CancellationToken token)
        {
            Guid id = @event.Id;

            using DbCommand command = _provider.CreateCommand(
                commandString: $"Insert Into [ControlledUpdates].[Campaigns](Id,[Description],[Begin],[End]) Values ('{id}',@Description,'{@event.Duration.Begin:yyyy-MM-dd}','{@event.Duration.End:yyyy-MM-dd}')",
                commandType: CommandType.Text);

            command.Parameters.Add(_provider.CreateInputParameter(
                parameterName: "@Description",
                value: @event.Description.Value,
                dbType: DbType.String));

            command.CommandTimeout = _commandTimeout;

            await transaction.ExecuteNonQueryAsync(command, token);

            foreach(var warehouse in @event.Warehouses)
            {
                command.Parameters.Clear();

                command.CommandText = $"Insert Into [ControlledUpdates].[CampaignWarehouses](Campaign,WarehouseCode) Values ('{id}',@WarehouseCode)";

                command.Parameters.Add(_provider.CreateInputParameter(
                    parameterName: "@WarehouseCode",
                    value: warehouse.Code,
                    dbType: DbType.String));

                await transaction.ExecuteNonQueryAsync(command, token);
            }

            command.Parameters.Clear();

            foreach (var scope in @event.Scopes)
            {
                command.CommandText = $"Insert Into [ControlledUpdates].[CampaignScopes](Campaign,Scope) Values ('{id}','{scope.Id}')";

                await transaction.ExecuteNonQueryAsync(command, token);
            }
        }
        
        private async Task WriteEvent(CampaignDurationChangedDataEvent @event, SqlTransaction transaction, CancellationToken token)
        {
            using DbCommand command = _provider.CreateCommand(
                commandString: $"Update [ControlledUpdates].[Campaigns] Set [Begin]='{@event.Duration.Begin:yyyy-MM-dd}',[End]='{@event.Duration.End:yyyy-MM-dd}' Where Id='{@event.Id}')",
                commandType: CommandType.Text);

            command.CommandTimeout = _commandTimeout;

            await transaction.ExecuteNonQueryAsync(command, token);
        }
        
        private async Task WriteEvent(CampaignWarehousesChangedDataEvent @event, SqlTransaction transaction, CancellationToken token)
        {
            Guid id = @event.Id;

            using DbCommand command = _provider.CreateCommand(
               commandString: $"Delete From [ControlledUpdates].[CampaignWarehouses] Where Campaign='{id}')",
               commandType: CommandType.Text);

            command.CommandTimeout = _commandTimeout;

            await transaction.ExecuteNonQueryAsync(command, token);

            foreach(var warehouse in @event.Warehouses)
            {
                command.Parameters.Clear();

                command.CommandText = $"Insert Into [ControlledUpdates].[CampaignWarehouses](Campaign,WarehouseCode) Values ('{id}',@WarehouseCode)";

                command.Parameters.Add(_provider.CreateInputParameter(
                    parameterName: "@WarehouseCode",
                    value: warehouse.Code,
                    dbType: DbType.String));

                await transaction.ExecuteNonQueryAsync(command, token);
            }
        }
        
        private async Task WriteEvent(CampaignScopesChangedDataEvent @event, SqlTransaction transaction, CancellationToken token)
        {
            Guid id = @event.Id;

            using DbCommand command = _provider.CreateCommand(
               commandString: $"Delete From [ControlledUpdates].[CampaignScopes] Where Campaign='{id}')",
               commandType: CommandType.Text);

            command.CommandTimeout = _commandTimeout;


            await transaction.ExecuteNonQueryAsync(command, token);
            foreach (var scope in @event.Scopes)
            {
                command.CommandText = $"Insert Into [ControlledUpdates].[CampaignScopes](Campaign,Scope) Values ('{id}','{scope.Id}')";

                await transaction.ExecuteNonQueryAsync(command, token);
            }
        }

        private async Task WriteEvent(CampaignCancelledDataEvent @event, SqlTransaction transaction, CancellationToken token)
        {
            using DbCommand command = _provider.CreateCommand(
                commandString: $"Update [ControlledUpdates].[Campaigns] Set Cancelled=1 Where Id='{@event.Id}')",
                commandType: CommandType.Text);

            command.CommandTimeout = _commandTimeout;

            await transaction.ExecuteNonQueryAsync(command, token);
        }
    }
}
