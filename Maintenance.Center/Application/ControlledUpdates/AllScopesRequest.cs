using Data.Common.Contracts;
using Data.Definitions.ControlledUpdates;
using MediatR;

namespace Application.ControlledUpdates
{
    public record class AllScopesRequest() : IRequest<IEnumerable<Scope>>;

    public class AllScopesRequestHandler : IRequestHandler<AllScopesRequest, IEnumerable<Scope>>
    {
        private readonly IAsyncQuery<IEnumerable<Scope>> _query;

        public AllScopesRequestHandler(IAsyncQuery<IEnumerable<Scope>> query)
        {
            _query = query;
        }

        public async Task<IEnumerable<Scope>> Handle(AllScopesRequest request, CancellationToken cancellationToken)
        {
            return await _query.ExecuteAsync(cancellationToken);
        }
    }
}