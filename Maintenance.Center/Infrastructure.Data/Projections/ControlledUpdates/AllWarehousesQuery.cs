using Data.Common.Contracts;
using Data.Definitions.ControlledUpdates;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data;
using System.Data.Common;

namespace Infrastructure.Data.Projections.ControlledUpdates
{
    public class AllWarehousesQuery : IAsyncQuery<IEnumerable<Warehouse>>
    {
        private readonly ISqlProvider _provider;

        private readonly ISqlCaller _caller;

        private readonly int _commandTimeout;

        public AllWarehousesQuery(string connectionString, int commandTimeout)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connectionString));
            _commandTimeout = commandTimeout;
        }

        public async Task<IEnumerable<Warehouse>> ExecuteAsync(CancellationToken token)
        {
            using DbCommand command = _provider.CreateCommand(
                commandString: "Select Code,[Description] From [ControlledUpdates].[Warehouses]",
                commandType: CommandType.Text);

            command.CommandTimeout = _commandTimeout;

            IEnumerable<DataHolder> items = await _caller.GetAsync(
                dataMapper: new ReflectionDataMapper<DataHolder>(),
                command: command,
                cancellationToken: token);

            return from item in items
                   where !string.IsNullOrWhiteSpace(item.Code)
                   select new Warehouse(
                       Code: item.Code!,
                       Description: item.Description);
        }

        private class DataHolder
        {
            public string? Code { get; set; }
            public string Description { get; set; } = string.Empty;
        }
    }
}
