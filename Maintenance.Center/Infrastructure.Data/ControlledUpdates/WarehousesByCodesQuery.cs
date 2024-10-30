using Application.ControlledUpdates;
using Core.ControlledUpdates;
using Data.Common.Contracts;
using Data.Sql.Provider;
using Data.Sql;
using Data.Sql.Mapping;
using System.Data.Common;
using System.Data;
using System.Text;

namespace Infrastructure.Data.ControlledUpdates
{
    public class WarehousesByCodesQuery :  IAsyncQuery<IEnumerable<Warehouse>, IEnumerable<WarehouseCode>>
    {
        private readonly ISqlProvider _provider;

        private readonly ISqlCaller _caller;

        private readonly int _commandTimeout;

        public WarehousesByCodesQuery(string connectionString, int commandTimeout)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connectionString));
            _commandTimeout = commandTimeout;
        }

        public async Task<IEnumerable<Warehouse>> ExecuteAsync(IEnumerable<WarehouseCode> parameter, CancellationToken token)
        {
            var uniqueWarehouseCodes = parameter.Distinct().ToArray();

            if (uniqueWarehouseCodes.Any())
            {
                int length = uniqueWarehouseCodes.Length;

                StringBuilder bobTheBuilder = new("Select Code From [ControlledUpdates].[Warehouses] Where Code In (");

                using DbCommand command = _provider.CreateCommand(
                    commandString: string.Empty,
                    commandType: CommandType.Text);
                
                command.CommandTimeout = _commandTimeout;

                string parameterName = string.Empty;

                for (int i = 0; i < length; ++i)
                {
                    parameterName = $"@Code{i}";

                    bobTheBuilder.Append(parameterName);

                    command.Parameters.Add(
                        value: _provider.CreateInputParameter(
                            parameterName: parameterName,
                            value: uniqueWarehouseCodes[i].Code,
                            dbType: DbType.String));

                    if (i < length - 1)
                    {
                        bobTheBuilder.Append(value: ',');
                    }
                }

                bobTheBuilder.Append(')');

                command.CommandText = bobTheBuilder.ToString();

                IEnumerable<DataHolder> items = await _caller.GetAsync(
                    dataMapper: new ReflectionDataMapper<DataHolder>(),
                    command: command,
                    cancellationToken: token);

                return from item in items
                       where !string.IsNullOrWhiteSpace(item.Code)
                       select new Warehouse(Code: item.Code!);
            }

            return Enumerable.Empty<Warehouse>();
        }

        private class DataHolder
        {
            public string? Code { get; set; }
        }
    }
}
