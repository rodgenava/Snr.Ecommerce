using Application.Pricebook4;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Pricebook4Controller : ControllerBase
    {
        private readonly IMediator _mediator;

        public Pricebook4Controller(IMediator mediator)
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
        [Route("WeightedItems")]
        public async Task<IActionResult> WeightedItems([FromQuery] RecordQueryOptions options, CancellationToken token)
        {
            return Ok(await _mediator.Send(
               request: new WeightedSkuListItemsPagedRequest(
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
                fileDownloadName: "Pricebook 4 Template.xlsx");
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
                fileDownloadName: "Pricebook 4 Promotions Template.xlsx");
        }

        [HttpGet]
        [Route("WeightedItems/Template")]
        public async Task<IActionResult> WeightedItemsTemplate(CancellationToken token)
        {
            byte[] templateBytes = await _mediator.Send(
                request: new WeightedSkuUpdateTemplateRequest(),
                cancellationToken: token);

            return File(
                fileContents: templateBytes,
                contentType: System.Net.Mime.MediaTypeNames.Application.Octet,
                fileDownloadName: "Pricebook 4 Weighted Items Template.xlsx");
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
                request: new UpdatePromotionsFromTemplateRequest(InputFile: stream), 
                cancellationToken: token);

            return Ok();
        }

        [HttpPost]
        [Route("WeightedItems/Update")]
        [RequestSizeLimit(bytes: 60_000_000)]
        public async Task<IActionResult> UpdateWeightedItems(IFormFile file, CancellationToken token)
        {
            using Stream stream = file.OpenReadStream();

            await _mediator.Send(
                request: new UpdateWeightedSkuFromTemplateRequest(InputFile: stream),
                cancellationToken: token);

            return Ok();
        }
    }
}
