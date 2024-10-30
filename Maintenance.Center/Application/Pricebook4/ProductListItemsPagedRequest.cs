using Data.Common.Contracts;
using Data.Definitions.Pricebook4.View;
using MediatR;

namespace Application.Pricebook4
{

    public record class ProductListItemsPagedRequest(int PageSize, int Page, string? SortColumn, string? SearchTerm) : IRequest<IEnumerable<ProductListItem>>;

    public class ProductListItemsPagedRequestHandler : IRequestHandler<ProductListItemsPagedRequest, IEnumerable<ProductListItem>>
    {
        private readonly IAsyncQuery<IEnumerable<ProductListItem>, ProductListItemsPagedRequest> _query;

        public ProductListItemsPagedRequestHandler(IAsyncQuery<IEnumerable<ProductListItem>, ProductListItemsPagedRequest> query)
        {
            _query = query;
        }

        public async Task<IEnumerable<ProductListItem>> Handle(ProductListItemsPagedRequest request, CancellationToken cancellationToken)
        {
            return await _query.ExecuteAsync(
                parameter: request,
                cancellationToken: cancellationToken);
        }
    }
}
