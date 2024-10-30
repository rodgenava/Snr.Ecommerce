using Data.Common.Contracts;
using Data.Definitions.Pricebook2.View;
using MediatR;

namespace Application.Pricebook2
{
    public record MetromartBuyXTakeYConfigurationListItemsPagedRequest(int PageSize, int Page, string? SortColumn, string? SearchTerm) : IRequest<IEnumerable<MetromartBuyXTakeYConfigurationListItem>>;

    public class MetromartBuyXTakeYConfigurationListItemsPagedRequestHandler : IRequestHandler<MetromartBuyXTakeYConfigurationListItemsPagedRequest, IEnumerable<MetromartBuyXTakeYConfigurationListItem>>
    {
        private readonly IAsyncQuery<IEnumerable<MetromartBuyXTakeYConfigurationListItem>, MetromartBuyXTakeYConfigurationListItemsPagedRequest> _query;

        public MetromartBuyXTakeYConfigurationListItemsPagedRequestHandler(IAsyncQuery<IEnumerable<MetromartBuyXTakeYConfigurationListItem>, MetromartBuyXTakeYConfigurationListItemsPagedRequest> query)
        {
            _query = query;
        }

        public async Task<IEnumerable<MetromartBuyXTakeYConfigurationListItem>> Handle(MetromartBuyXTakeYConfigurationListItemsPagedRequest request, CancellationToken cancellationToken)
        {
            return await _query.ExecuteAsync(request, cancellationToken);
        }
    }
}