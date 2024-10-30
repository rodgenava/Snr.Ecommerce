using Application.Pricebook2;
using Data.Common.Contracts;
using Data.Definitions.Pricebook2.View;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data.Common;

namespace Infrastructure.Data.Pricebook2.View
{
    public class ProductListItemsPagedQuery : IAsyncQuery<IEnumerable<ProductListItem>, ProductListItemsPagedRequest>
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
            string query = "With Pricebook2Skus As ( Select Sku From Pricebook2Items Group By Sku ), OrderedSet As ( Select Row_Number() Over ( Order By {PART_SORT_ORDER} ) As ResultOrdinal, M.Sku, M.[Description], M.SkuTypeCode, M.[Status], MD.ClassName, MD.SubclassName, MD.DepartmentName, MD.SubdepartmentName From Masterlist M Join MasterlistDepartments MD On M.Sku = MD.Sku Join Pricebook2Skus P2S On M.Sku = P2S.Sku {PART_INTERNAL_SEARCH} ) Select OS.Sku, OS.[Description], OS.SkuTypeCode, OS.[Status], OS.ClassName, OS.SubclassName, OS.DepartmentName, OS.SubdepartmentName From OrderedSet OS ";

            bool notHasSearchQuery = string.IsNullOrWhiteSpace(parameter.SearchTerm);

            query = query.Replace("{PART_SORT_ORDER}", (parameter.SortColumn ?? "").ToLower() switch
            {
                "sort:sku:asc" => "M.Sku Asc",
                "sort:sku:desc" => "M.Sku Desc",
                "sort:description:asc" => "M.[Description] Asc",
                "sort:description:desc" => "M.[Description] Desc",
                _ => "M.Sku Asc"
            })
                .Replace("{PART_INTERNAL_SEARCH}", notHasSearchQuery ? 
                    "" : 
                    "Where CAST(M.Sku as varchar(15)) Like '%' + @Search + '%' Or M.[Description] Like '%' + @Search + '%' Or MD.ClassName Like '%' + @Search + '%' Or MD.DepartmentName Like '%' + @Search + '%' ")
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
                       Description: item.Description,
                       Class: item.ClassName,
                       Subclass: item.SubclassName,
                       Department: item.DepartmentName,
                       Subdepartment: item.SubdepartmentName,
                       SkuTypeCode: item.SkuTypeCode,
                       MmsStatus: item.Status);
        }
        private class DataHolder
        {
            public int Sku { get; set; }
            public string? Description { get; set; }
            public string? ClassName { get; set; }
            public string? SubclassName { get; set; }
            public string? DepartmentName { get; set; }
            public string? SubdepartmentName { get; set; }
            public string? SkuTypeCode { get; set; }
            public string? Status { get; set; }
        }
    }
}
