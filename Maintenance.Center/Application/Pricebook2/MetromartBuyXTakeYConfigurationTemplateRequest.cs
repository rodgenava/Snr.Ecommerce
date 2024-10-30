using Application.Writing;
using Data.Common.Contracts;
using Data.Definitions.Pricebook2;
using MediatR;

namespace Application.Pricebook2
{
    public record MetromartBuyXTakeYConfigurationTemplateRequest : IRequest<byte[]>;

    public class MetromartBuyXTakeYConfigurationTemplateRequestHandler : IRequestHandler<MetromartBuyXTakeYConfigurationTemplateRequest, byte[]>
    {
        private readonly IStreamlinedCollectionWriterV2<MetromartBuyXTakeYConfigurationTemplateItem> _writer;
        private readonly IAsyncEnumerableQuery<MetromartBuyXTakeYConfigurationTemplateItem> _query;

        public MetromartBuyXTakeYConfigurationTemplateRequestHandler(IStreamlinedCollectionWriterV2<MetromartBuyXTakeYConfigurationTemplateItem> writer, IAsyncEnumerableQuery<MetromartBuyXTakeYConfigurationTemplateItem> query)
        {
            _writer = writer;
            _query = query;
        }

        public async Task<byte[]> Handle(MetromartBuyXTakeYConfigurationTemplateRequest request, CancellationToken cancellationToken)
        {
            using var memoryStream = new MemoryStream();

            await _writer.WriteAsync(
                stream: memoryStream,
                query: _query,
                cancellationToken: cancellationToken);
            
            return memoryStream.ToArray();
        }
    }
}
