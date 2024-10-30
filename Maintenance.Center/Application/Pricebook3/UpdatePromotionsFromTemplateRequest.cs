using Application.Writing;
using Data.Common.Contracts;
using Data.Definitions.Pricebook3;
using MediatR;

namespace Application.Pricebook3
{
    public record class UpdatePromotionsFromTemplateRequest(Stream InputFile) : IRequest;

    public class UpdatePromotionsFromTemplateRequestHandler : IRequestHandler<UpdatePromotionsFromTemplateRequest>
    {
        private readonly ICollectionReader<PromotionUpdateItem, Stream> _reader;
        private readonly IAsyncDataWriter<IEnumerable<PromotionUpdateItem>> _writer;

        public UpdatePromotionsFromTemplateRequestHandler(
            ICollectionReader<PromotionUpdateItem, Stream> reader,
            IAsyncDataWriter<IEnumerable<PromotionUpdateItem>> writer)
        {
            _reader = reader;
            _writer = writer;
        }

        public async Task<Unit> Handle(UpdatePromotionsFromTemplateRequest request, CancellationToken cancellationToken)
        {
            await _writer.WriteAsync(_reader.ReadFrom(request.InputFile), cancellationToken);

            return Unit.Value;
        }
    }
}
