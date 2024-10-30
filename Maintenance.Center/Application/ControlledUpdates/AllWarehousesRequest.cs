using Data.Common.Contracts;
using Data.Definitions.ControlledUpdates;
using MediatR;

namespace Application.ControlledUpdates
{
    public record class AllWarehousesRequest() : IRequest<IEnumerable<Warehouse>>;

    public class AllWarehousesRequestHandler : IRequestHandler<AllWarehousesRequest, IEnumerable<Warehouse>>
    {
        private readonly IAsyncQuery<IEnumerable<Warehouse>> _query;

        public AllWarehousesRequestHandler(IAsyncQuery<IEnumerable<Warehouse>> query)
        {
            _query = query;
        }

        public async Task<IEnumerable<Warehouse>> Handle(AllWarehousesRequest request, CancellationToken cancellationToken)
        {
            return await _query.ExecuteAsync(cancellationToken);
        }
    }
}