using Application.Writing;
using Data.Common.Contracts;
using Data.Definitions.Pricebook4;
using MediatR;

namespace Application.Pricebook4
{
    public record class ProductsUpdateTemplateRequest : IRequest<byte[]>;

    public class ProductUpdateTemplateRequestHandler : IRequestHandler<ProductsUpdateTemplateRequest, byte[]>
    {
        private readonly IStreamlinedCollectionWriter<ProductUpdateTemplateItem> _writer;
        private readonly IAsyncDataSourceIterator<ProductUpdateTemplateItem> _iterator;

        public ProductUpdateTemplateRequestHandler(IStreamlinedCollectionWriter<ProductUpdateTemplateItem> writer, IAsyncDataSourceIterator<ProductUpdateTemplateItem> iterator)
        {
            _writer = writer;
            _iterator = iterator;
        }

        public async Task<byte[]> Handle(ProductsUpdateTemplateRequest request, CancellationToken cancellationToken)
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
