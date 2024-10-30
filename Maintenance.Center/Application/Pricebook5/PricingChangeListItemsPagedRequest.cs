using Data.Common.Contracts;
using Data.Definitions.Pricebook5.View;
using MediatR;

namespace Application.Pricebook5
{
    public record class PricingChangeListItemsPagedRequest(int PageSize, int Page, string? SortColumn, string? SearchTerm) : IRequest<IEnumerable<PricingHistoryListItem>>;

    public class PricingChangeListItemsPagedRequestHandler : IRequestHandler<PricingChangeListItemsPagedRequest, IEnumerable<PricingHistoryListItem>>
    {
        private readonly IAsyncQuery<IEnumerable<PricingHistoryListItem>, PricingChangeListItemsPagedRequest> _query;

        public PricingChangeListItemsPagedRequestHandler(IAsyncQuery<IEnumerable<PricingHistoryListItem>, PricingChangeListItemsPagedRequest> query)
        {
            _query = query;
        }

        public async Task<IEnumerable<PricingHistoryListItem>> Handle(PricingChangeListItemsPagedRequest request, CancellationToken cancellationToken)
        {
            return await _query.ExecuteAsync(request, cancellationToken);
        }
    }
}
