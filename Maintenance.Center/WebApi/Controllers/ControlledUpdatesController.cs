using Application.ControlledUpdates;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/controlled-updates")]
    public class ControlledUpdatesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ControlledUpdatesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("warehouses")]
        public async Task<IActionResult> Warehouses(CancellationToken token)
        {
            var data = await _mediator.Send(
                   request: new AllWarehousesRequest(),
                   cancellationToken: token);

            return Ok(data);
        }

        [HttpGet]
        [Route("scopes")]
        public async Task<IActionResult> Scopes(CancellationToken token)
        {
            var data = await _mediator.Send(
                   request: new AllScopesRequest(),
                   cancellationToken: token);

            return Ok(data);
        }

        [HttpPost]
        [Route("campaign")]
        public async Task<IActionResult> NewCampaign(NewCampaignBindingModel model, CancellationToken token)
        {
            var data = await _mediator.Send(
                   request: new NewCampaignRequest(
                       Description: model.Description,
                       Begin: model.Begin,
                       End: model.End,
                       Warehouses: from item in model.Warehouses
                                   select new WarehouseCode(item),
                       Scopes: from item in model.Scopes
                               select new CampaignScope(item)),
                   cancellationToken: token);

            return Ok(data);
        }
    }
}