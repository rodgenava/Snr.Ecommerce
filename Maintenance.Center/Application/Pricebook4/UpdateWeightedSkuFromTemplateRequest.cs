using Application.Writing;
using Data.Common.Contracts;
using Data.Definitions.Pricebook4;
using MediatR;

namespace Application.Pricebook4
{
    public record class UpdateWeightedSkuFromTemplateRequest(Stream InputFile) : IRequest;

    public class UpdateWeightedSkuFromTemplateRequestHandler : IRequestHandler<UpdateWeightedSkuFromTemplateRequest>
    {
        private readonly ICollectionReader<WeightedSkuUpdateTemplateItem, Stream> _reader;
        private readonly IAsyncDataWriter<IEnumerable<WeightedSkuUpdateTemplateItem>> _writer;

        public UpdateWeightedSkuFromTemplateRequestHandler(
            ICollectionReader<WeightedSkuUpdateTemplateItem, Stream> reader,
            IAsyncDataWriter<IEnumerable<WeightedSkuUpdateTemplateItem>> writer)
        {
            _reader = reader;
            _writer = writer;
        }

        public async Task<Unit> Handle(UpdateWeightedSkuFromTemplateRequest request, CancellationToken cancellationToken)
        {
            await _writer.WriteAsync(_reader.ReadFrom(request.InputFile), cancellationToken);

            return Unit.Value;
        }
    }
}
