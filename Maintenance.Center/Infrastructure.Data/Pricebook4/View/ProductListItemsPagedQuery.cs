using Application.Pricebook4;
using Data.Common.Contracts;
using Data.Definitions.Pricebook4.View;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data.Common;

namespace Infrastructure.Data.Pricebook4.View
{
    public class ProductListItemsPagedQuery : IAsyncQuery<IEnumerable<ProductListItem>,ProductListItemsPagedRequest>
    {
        private readonly ISqlProvider _provider;
        private readonly ISqlCaller _caller;
        private readonly int _commandTimeout;

        public ProductListItemsPagedQuery(string connection, int commandTimeout)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connection));
            _commandTimeout = commandTimeout;

        }
        public async Task<IEnumerable<ProductListItem>> ExecuteAsync(ProductListItemsPagedRequest parameter, CancellationToken token)
        {
            string query = "With OrderedSet As (Select Row_Number() Over (Order By {PART_SORT_ORDER}) As ResultOrdinal,P4I.Sku,MD.DepartmentName,MD.SubdepartmentName,MD.ClassName,MD.SubclassName,M.[Description],M.SkuTypeCode,M.[Status] From (Select Sku From Pricebook4Items Group By Sku) P4I Join Masterlist M On P4I.Sku = M.Sku Join MasterlistDepartments MD On M.Sku = MD.Sku {PART_INTERNAL_SEARCH}) Select OS.Sku,OS.DepartmentName,OS.SubdepartmentName,OS.ClassName,OS.SubclassName,OS.[Description],OS.SkuTypeCode,OS.[Status] From OrderedSet OS ";

            bool notHasSearchQuery = string.IsNullOrWhiteSpace(parameter.SearchTerm);

            query = query.Replace("{PART_SORT_ORDER}", (parameter.SortColumn ?? "").ToLower() switch
            {
                "sort:sku:asc" => "P4I.Sku Asc",
                "sort:sku:desc" => "P4I.Sku Desc",
                "sort:description:asc" => "M.[Description] Asc",
                "sort:description:desc" => "M.[Description] Desc",
                _ => "M.Sku Asc"
            })
                .Replace("{PART_INTERNAL_SEARCH}", notHasSearchQuery ?
                    "" :
                    "Where CAST(P4I.Sku as varchar(15)) Like '%' + @Search + '%' Or M.[Description] Like '%' + @Search + '%' Or MD.DepartmentName Like '%' + @Search + '%' Or MD.SubdepartmentName Like '%' + @Search + '%' Or MD.ClassName Like '%' + @Search + '%' Or MD.SubclassName Like '%' + @Search + '%' ")
                + $"Where (OS.ResultOrdinal >= {(parameter.Page * parameter.PageSize) - parameter.PageSize + 1} And OS.ResultOrdinal <= {parameter.Page * parameter.PageSize}) ";

            using DbCommand command = _provider.CreateCommand(query);

            command.CommandTimeout = _commandTimeout;

            if (!notHasSearchQuery)
            {
                command.Parameters.Add(_provider.CreateInputParameter("@Search", parameter.SearchTerm!, System.Data.DbType.String));
            }

            IEnumerable<DataHolder> items = await _caller.GetAsync(new ReflectionDataMapper<DataHolder>(), command, token);

            return from item in items
                   select new ProductListItem(
                       Sku: item.Sku,
                       Department: item.DepartmentName,
                       Subdepartment: item.SubdepartmentName,
                       Class: item.ClassName,
                       Subclass: item.SubclassName,
                       Description: item.Description,
                       SkuTypeCode: item.SkuTypeCode,
                       MmsStatus: item.Status);
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
            public string? Status { get; set; }
        }
    }
}
