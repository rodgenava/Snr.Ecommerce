using Application.Writing;
using Data.Common.Contracts;
using Data.Definitions.Pricebook5;
using MediatR;

namespace Application.Pricebook5
{
    public record class UpdateProductsFromTemplateRequest(Stream InputFile) : IRequest;

    public class UpdateProductsFromTemplateRequestHandler : IRequestHandler<UpdateProductsFromTemplateRequest>
    {
        private readonly ICollectionReader<ProductUpdateTemplateItem, Stream> _reader;
        private readonly IAsyncDataWriter<IEnumerable<ProductUpdateTemplateItem>> _writer;

        public UpdateProductsFromTemplateRequestHandler(ICollectionReader<ProductUpdateTemplateItem, Stream> reader, IAsyncDataWriter<IEnumerable<ProductUpdateTemplateItem>> writer)
        {
            _reader = reader;
            _writer = writer;
        }

        public async Task<Unit> Handle(UpdateProductsFromTemplateRequest request, CancellationToken cancellationToken)
        {
            await _writer.WriteAsync(_reader.ReadFrom(request.InputFile), cancellationToken);

            return Unit.Value;
        }
    }
}
