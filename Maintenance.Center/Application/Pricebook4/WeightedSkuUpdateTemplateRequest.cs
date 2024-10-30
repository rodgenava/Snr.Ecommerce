using Application.Writing;
using Data.Common.Contracts;
using Data.Definitions.Pricebook4;
using MediatR;

namespace Application.Pricebook4
{
    public record class WeightedSkuUpdateTemplateRequest : IRequest<byte[]>;

    public class WeightedSkuUpdateTemplateRequestHandler : IRequestHandler<WeightedSkuUpdateTemplateRequest, byte[]>
    {
        private readonly IStreamlinedCollectionWriter<WeightedSkuUpdateTemplateItem> _writer;
        private readonly IAsyncDataSourceIterator<WeightedSkuUpdateTemplateItem> _iterator;

        public WeightedSkuUpdateTemplateRequestHandler(IStreamlinedCollectionWriter<WeightedSkuUpdateTemplateItem> writer, IAsyncDataSourceIterator<WeightedSkuUpdateTemplateItem> iterator)
        {
            _writer = writer;
            _iterator = iterator;
        }

        public async Task<byte[]> Handle(WeightedSkuUpdateTemplateRequest request, CancellationToken cancellationToken)
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
