using Application.Repositories.ControlledUpdates;
using Core.ControlledUpdates;
using Data.Common.Contracts;
using MediatR;

namespace Application.ControlledUpdates
{
    public record NewCampaignRequest(string Description, DateOnly Begin, DateOnly End, IEnumerable<WarehouseCode> Warehouses, IEnumerable<CampaignScope> Scopes) : IRequest;

    public class NewCampaignRequestHandler : IRequestHandler<NewCampaignRequest>
    {
        private readonly IAsyncQuery<IEnumerable<Warehouse>, IEnumerable<WarehouseCode>> _warehousesByCodesQuery;
        private readonly IAsyncQuery<IEnumerable<Scope>, IEnumerable<CampaignScope>> _campaignScopesByIdsQuery;
        private readonly ICampaignRepository _repository;

        public NewCampaignRequestHandler(IAsyncQuery<IEnumerable<Warehouse>, IEnumerable<WarehouseCode>> warehousesByCodesQuery, IAsyncQuery<IEnumerable<Scope>, IEnumerable<CampaignScope>> campaignScopesByIdsQuery, ICampaignRepository repository)
        {
            _warehousesByCodesQuery = warehousesByCodesQuery;
            _campaignScopesByIdsQuery = campaignScopesByIdsQuery;
            _repository = repository;
        }

        public async Task<Unit> Handle(NewCampaignRequest request, CancellationToken cancellationToken)
        {
            IEnumerable<Warehouse> warehouses = await _warehousesByCodesQuery.ExecuteAsync(
                parameter: request.Warehouses,
                cancellationToken: cancellationToken);

            IEnumerable<Scope> scopes = await _campaignScopesByIdsQuery.ExecuteAsync(
                parameter: request.Scopes,
                cancellationToken: cancellationToken);

            var campaign = Campaign.New(
                description: new Description(Value: request.Description),
                duration: new Duration(
                    begin: request.Begin,
                    end: request.End),
                warehouses: warehouses,
                scopes: scopes);

            await _repository.SaveAsync(
                item: campaign,
                cancellationToken: cancellationToken);

            return Unit.Value;
        }
    }
}