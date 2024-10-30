using Application.Pricebook2;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Pricebook2Controller : ControllerBase
    {
        private readonly IMediator _mediator;

        public Pricebook2Controller(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("Products")]
        public async Task<IActionResult> Items([FromQuery] RecordQueryOptions options, CancellationToken token)
        {
            return Ok(await _mediator.Send(
                request: new ProductListItemsPagedRequest(
                    PageSize: options.PageSize,
                    Page: options.Page,
                    SortColumn: options.SortColumn,
                    SearchTerm: options.SearchTerm),
                cancellationToken: token));
        }

        [HttpGet]
        [Route("Metromart/BuyXTakeYs")]
        public async Task<IActionResult> MetromartBuyXTakeYs([FromQuery] RecordQueryOptions options, CancellationToken token)
        {
            return Ok(await _mediator.Send(
               request: new MetromartBuyXTakeYConfigurationListItemsPagedRequest(
                   PageSize: options.PageSize,
                   Page: options.Page,
                   SortColumn: options.SortColumn,
                   SearchTerm: options.SearchTerm),
               cancellationToken: token));
        }

        [HttpGet]
        [Route("PriceChanges")]
        public async Task<IActionResult> PricingHistories([FromQuery] RecordQueryOptions options, CancellationToken token)
        {
            return Ok(await _mediator.Send(
               request: new PricingChangeListItemsPagedRequest(
                   PageSize: options.PageSize,
                   Page: options.Page,
                   SortColumn: options.SortColumn,
                   SearchTerm: options.SearchTerm),
               cancellationToken: token));
        }

        [HttpGet]
        [Route("SkuConfiguration/Template")]
        public async Task<IActionResult> Template(CancellationToken token)
        {
            byte[] templateBytes = await _mediator.Send(
                request: new ProductsUpdateTemplateRequest(),
                cancellationToken: token);

            return File(
                fileContents: templateBytes,
                contentType: System.Net.Mime.MediaTypeNames.Application.Octet,
                fileDownloadName: "Pricebook 2 Template.xlsx");
        }

        [HttpGet]
        [Route("Metromart/BuyXTakeYs/Template")]
        public async Task<IActionResult> MetromartBuyXTakeYTemplate(CancellationToken token)
        {
            byte[] templateBytes = await _mediator.Send(
                request: new MetromartBuyXTakeYConfigurationTemplateRequest(),
                cancellationToken: token);

            return File(
                fileContents: templateBytes,
                contentType: System.Net.Mime.MediaTypeNames.Application.Octet,
                fileDownloadName: "Pricebook 2 Metromart Buy X Take Y Template.xlsx");
        }

        [HttpPost]
        [Route("SkuConfiguration/Update")]
        [RequestSizeLimit(bytes: 60_000_000)]
        public async Task<IActionResult> Update(IFormFile file, CancellationToken token)
        {
            using Stream stream = file.OpenReadStream();

            await _mediator.Send(
                request: new UpdateProductsFromTemplateRequest(InputFile: stream),
                cancellationToken: token);

            return Ok();
        }

        [HttpPost]
        [Route("Metromart/BuyXTakeYs/Update")]
        [RequestSizeLimit(bytes: 60_000_000)]
        public async Task<IActionResult> MetromartBuyXTakeYUpdate(IFormFile file, CancellationToken token)
        {
            using Stream stream = file.OpenReadStream();

            await _mediator.Send(
                request: new UpdateMetromartBuyXTakeYConfigurationFromTemplateRequest(InputFile: stream),
                cancellationToken: token);

            return Ok();
        }
    }
}