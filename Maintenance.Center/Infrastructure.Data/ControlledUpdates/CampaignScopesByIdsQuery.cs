using Application.ControlledUpdates;
using Core.ControlledUpdates;
using Data.Common.Contracts;
using Data.Sql.Provider;
using Data.Sql;
using Data.Sql.Mapping;
using System.Data.Common;
using System.Data;

namespace Infrastructure.Data.ControlledUpdates
{
    public class CampaignScopesByIdsQuery : IAsyncQuery<IEnumerable<Scope>, IEnumerable<CampaignScope>>
    {
        private readonly ISqlProvider _provider;

        private readonly ISqlCaller _caller;

        private readonly int _commandTimeout;

        public CampaignScopesByIdsQuery(string connectionString, int commandTimeout)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connectionString));
            _commandTimeout = commandTimeout;
        }

        public async Task<IEnumerable<Scope>> ExecuteAsync(IEnumerable<CampaignScope> parameter, CancellationToken token)
        {
            var uniqueScopeIds = parameter.Distinct().ToArray();

            if (uniqueScopeIds.Any())
            {
                using DbCommand command = _provider.CreateCommand(
                    commandString: $"Select Id From [ControlledUpdates].[Scopes] Where Id In ({string.Join(",", from item in uniqueScopeIds select $"'{item.Id}'" )})",
                    commandType: CommandType.Text);
                
                command.CommandTimeout = _commandTimeout;

                IEnumerable<DataHolder> items = await _caller.GetAsync(
                    dataMapper: new ReflectionDataMapper<DataHolder>(),
                    command: command,
                    cancellationToken: token);

                return from item in items
                       select new Scope(Id: item.Id);
            }

            return Enumerable.Empty<Scope>();
        }

        private class DataHolder
        {
            public Guid Id { get; set; }
        }
    }
}
