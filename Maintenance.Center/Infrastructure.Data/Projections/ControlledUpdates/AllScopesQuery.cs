using Data.Common.Contracts;
using Data.Definitions.ControlledUpdates;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data;
using System.Data.Common;

namespace Infrastructure.Data.Projections.ControlledUpdates
{
    public class AllScopesQuery : IAsyncQuery<IEnumerable<Scope>>
    {
        private readonly ISqlProvider _provider;

        private readonly ISqlCaller _caller;

        private readonly int _commandTimeout;

        public AllScopesQuery(string connectionString, int commandTimeout)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connectionString));
            _commandTimeout = commandTimeout;
        }

        public async Task<IEnumerable<Scope>> ExecuteAsync(CancellationToken token)
        {
            using DbCommand command = _provider.CreateCommand(
                commandString: "Select Id,[Description] From [ControlledUpdates].[Scopes]",
                commandType: CommandType.Text);

            command.CommandTimeout = _commandTimeout;

            IEnumerable<DataHolder> items = await _caller.GetAsync(
                dataMapper: new ReflectionDataMapper<DataHolder>(),
                command: command,
                cancellationToken: token);

            return from item in items
                   select new Scope(
                       Id: item.Id,
                       Description: item.Description);
        }

        private class DataHolder
        {
            public Guid Id { get; set; }
            public string Description { get; set; } = string.Empty;
        }
    }
}
