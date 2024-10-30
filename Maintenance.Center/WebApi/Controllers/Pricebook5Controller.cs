using Application.Pricebook5;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Pricebook5Controller : ControllerBase
    {
        private readonly IMediator _mediator;

        public Pricebook5Controller(IMediator mediator)
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
        [Route("Promotions")]
        public async Task<IActionResult> Promotions([FromQuery] RecordQueryOptions options, CancellationToken token)
        {
            return Ok(await _mediator.Send(
                request: new PromotionListItemsPagedRequest(
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
                fileDownloadName: "Pricebook 5 Template.xlsx");
        }

        [HttpGet]
        [Route("Promotions/Template")]
        public async Task<IActionResult> PromotionsTemplate(CancellationToken token)
        {
            byte[] templateBytes = await _mediator.Send(
                request: new PromotionReviewTemplateRequest(),
                cancellationToken: token);

            return File(
                fileContents: templateBytes,
                contentType: System.Net.Mime.MediaTypeNames.Application.Octet,
                fileDownloadName: "Pricebook 5 Promotions Template.xlsx");
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
        [Route("Promotions/Update")]
        [RequestSizeLimit(bytes: 60_000_000)]
        public async Task<IActionResult> UpdatePromotions(IFormFile file, CancellationToken token)
        {
            using Stream stream = file.OpenReadStream();

            await _mediator.Send(
                request: new UpdatePromotionsFromTemplateRequest(InputFile: 
                stream), cancellationToken: token);

            return Ok();
        }
    }
}