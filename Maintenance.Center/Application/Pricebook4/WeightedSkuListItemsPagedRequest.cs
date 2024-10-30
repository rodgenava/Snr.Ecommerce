using Data.Common.Contracts;
using Data.Definitions.Pricebook4.View;
using MediatR;

namespace Application.Pricebook4
{
    public record class WeightedSkuListItemsPagedRequest(int PageSize, int Page, string? SortColumn, string? SearchTerm) : IRequest<IEnumerable<WeightedSkuListItem>>;

    public class WeightedSkuListItemsPagedRequestHandler : IRequestHandler<WeightedSkuListItemsPagedRequest, IEnumerable<WeightedSkuListItem>>
    {
        private readonly IAsyncQuery<IEnumerable<WeightedSkuListItem>, WeightedSkuListItemsPagedRequest> _query;

        public WeightedSkuListItemsPagedRequestHandler(IAsyncQuery<IEnumerable<WeightedSkuListItem>, WeightedSkuListItemsPagedRequest> query)
        {
            _query = query;
        }

        public async Task<IEnumerable<WeightedSkuListItem>> Handle(WeightedSkuListItemsPagedRequest request, CancellationToken cancellationToken)
        {
            return await _query.ExecuteAsync(
                parameter: request,
                cancellationToken: cancellationToken);
        }
    }
}
