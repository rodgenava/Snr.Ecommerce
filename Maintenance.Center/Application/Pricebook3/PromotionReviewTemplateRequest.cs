using Application.Writing;
using Data.Common.Contracts;
using Data.Definitions.Pricebook3;
using MediatR;

namespace Application.Pricebook3
{
    public record class PromotionReviewTemplateRequest : IRequest<byte[]>;

    public class PromotionReviewTemplateRequestHandler : IRequestHandler<PromotionReviewTemplateRequest, byte[]>
    {
        private readonly IStreamlinedCollectionWriter<PromotionReviewTemplateItem> _writer;
        private readonly IAsyncDataSourceIterator<PromotionReviewTemplateItem> _iterator;

        public PromotionReviewTemplateRequestHandler(IStreamlinedCollectionWriter<PromotionReviewTemplateItem> writer, IAsyncDataSourceIterator<PromotionReviewTemplateItem> iterator)
        {
            _writer = writer;
            _iterator = iterator;
        }

        public async Task<byte[]> Handle(PromotionReviewTemplateRequest request, CancellationToken cancellationToken)
        {
            var templateStream = await _writer.WriteAsync(_iterator, cancellationToken);

            try
            {
                return ((MemoryStream)templateStream).ToArray();
            }
            finally
            {
                templateStream.Dispose();
            }
        }
    }
}
