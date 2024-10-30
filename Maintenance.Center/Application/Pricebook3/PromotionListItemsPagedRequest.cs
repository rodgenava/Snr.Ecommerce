using Data.Common.Contracts;
using Data.Definitions.Pricebook3.View;
using MediatR;

namespace Application.Pricebook3
{
    public record class PromotionListItemsPagedRequest(int PageSize, int Page, string? SortColumn, string? SearchTerm) : IRequest<IEnumerable<PromotionListItem>>;

    public class PromotionListItemsPagedRequestHandler : IRequestHandler<PromotionListItemsPagedRequest, IEnumerable<PromotionListItem>>
    {
        private readonly IAsyncQuery<IEnumerable<PromotionListItem>, PromotionListItemsPagedRequest> _query;

        public PromotionListItemsPagedRequestHandler(IAsyncQuery<IEnumerable<PromotionListItem>, PromotionListItemsPagedRequest> query)
        {
            _query = query;
        }

        public async Task<IEnumerable<PromotionListItem>> Handle(PromotionListItemsPagedRequest request, CancellationToken cancellationToken)
        {
            return await _query.ExecuteAsync(request, cancellationToken);
        }
    }
}
