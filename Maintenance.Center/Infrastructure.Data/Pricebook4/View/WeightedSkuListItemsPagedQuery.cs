using Application.Pricebook4;
using Data.Common.Contracts;
using Data.Definitions.Pricebook4.View;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data.Common;

namespace Infrastructure.Data.Pricebook4.View
{
    public class WeightedSkuListItemsPagedQuery : IAsyncQuery<IEnumerable<WeightedSkuListItem>, WeightedSkuListItemsPagedRequest>
    {
        private readonly ISqlProvider _provider;
        private readonly ISqlCaller _caller;
        private readonly int _commandTimeout;

        public WeightedSkuListItemsPagedQuery(string connection, int commandTimeout)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connection));
            _commandTimeout = commandTimeout;

        }
        public async Task<IEnumerable<WeightedSkuListItem>> ExecuteAsync(WeightedSkuListItemsPagedRequest parameter, CancellationToken token)
        {
            string query = "With OrderedSet As (Select Row_Number() Over (Order By {PART_SORT_ORDER}) As ResultOrdinal,WI.Sku,MD.DepartmentName,MD.SubdepartmentName,MD.ClassName,MD.SubclassName,M.[Description],M.SkuTypeCode,WI.AverageWeight From [SNR_ECOMMERCE].[dbo].[WeightedItems] WI Join Masterlist M On WI.Sku = M.Sku Join MasterlistDepartments MD On M.Sku = MD.Sku {PART_INTERNAL_SEARCH}) Select OS.Sku,OS.DepartmentName,OS.SubdepartmentName,OS.ClassName,OS.SubclassName,OS.[Description],OS.SkuTypeCode,OS.AverageWeight From OrderedSet OS ";

            bool notHasSearchQuery = string.IsNullOrWhiteSpace(parameter.SearchTerm);

            query = query.Replace("{PART_SORT_ORDER}", (parameter.SortColumn ?? "").ToLower() switch
            {
                "sort:sku:asc" => "WI.Sku Asc",
                "sort:sku:desc" => "WI.Sku Desc",
                "sort:description:asc" => "M.[Description] Asc",
                "sort:description:desc" => "M.[Description] Desc",
                _ => "WI.Sku Asc"
            })
                .Replace("{PART_INTERNAL_SEARCH}", notHasSearchQuery ?
                    "" :
                    "Where Cast(WI.Sku as Varchar(15)) Like '%' + @Search + '%' Or M.[Description] Like '%' + @Search + '%' Or MD.DepartmentName Like '%' + @Search + '%' Or MD.SubdepartmentName Like '%' + @Search + '%' Or MD.ClassName Like '%' + @Search + '%' Or MD.SubclassName Like '%' + @Search + '%' ")
                + $"Where (OS.ResultOrdinal >= {(parameter.Page * parameter.PageSize) - parameter.PageSize + 1} And OS.ResultOrdinal <= {parameter.Page * parameter.PageSize}) ";

            using DbCommand command = _provider.CreateCommand(query);

            command.CommandTimeout = _commandTimeout;

            if (!notHasSearchQuery)
            {
                command.Parameters.Add(_provider.CreateInputParameter("@Search", parameter.SearchTerm!, System.Data.DbType.String));
            }

            IEnumerable<DataHolder> items = await _caller.GetAsync(new ReflectionDataMapper<DataHolder>(), command, token);

            return from item in items
                   select new WeightedSkuListItem(
                       Sku: item.Sku,
                       Department: item.DepartmentName,
                       Subdepartment: item.SubdepartmentName,
                       Class: item.ClassName,
                       Subclass: item.SubclassName,
                       Description: item.Description,
                       SkuTypeCode: item.SkuTypeCode,
                       AverageWeight: item.AverageWeight);
        }

        private class DataHolder
        {
            public int Sku { get; set; }
            public string? DepartmentName { get; set; }
            public string? SubdepartmentName { get; set; }
            public string? ClassName { get; set; }
            public string? SubclassName { get; set; }
            public string? Description { get; set; }
            public string? SkuTypeCode { get; set; }
            public decimal AverageWeight { get; set; }
        }

    }
}
