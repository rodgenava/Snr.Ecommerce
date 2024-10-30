using Application.Writing;
using Data.Common.Contracts;
using Data.Definitions.Pricebook2;
using MediatR;

namespace Application.Pricebook2
{
    public record class UpdateMetromartBuyXTakeYConfigurationFromTemplateRequest(Stream InputFile) : IRequest;

    public class UpdateMetromartBuyXTakeYConfigurationFromTemplateRequestHandler : IRequestHandler<UpdateMetromartBuyXTakeYConfigurationFromTemplateRequest>
    {
        private static readonly TimeOnly _zeroHour = new TimeOnly(hour: 0, minute: 0, second: 0);
        private readonly ICollectionReaderV2<MetromartBuyXTakeYConfigurationTemplateItem, Stream> _reader;
        private readonly IAsyncDataWriter<IEnumerable<MetromartBuyXTakeYConfigurationTemplateItem>> _writer;

        public UpdateMetromartBuyXTakeYConfigurationFromTemplateRequestHandler(ICollectionReaderV2<MetromartBuyXTakeYConfigurationTemplateItem, Stream> reader, IAsyncDataWriter<IEnumerable<MetromartBuyXTakeYConfigurationTemplateItem>> writer)
        {
            _reader = reader;
            _writer = writer;
        }

        public async Task<Unit> Handle(UpdateMetromartBuyXTakeYConfigurationFromTemplateRequest request, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.Now;

            Func<MetromartBuyXTakeYConfigurationTemplateItem, (bool IsValid, string Message)> specification = item =>
            {
                item.Deconstruct(
                    out _,
                    out int sku,
                    out _,
                    out DateOnly begin,
                    out DateOnly end,
                    out decimal buyQuantity,
                    out decimal takeQuantity);

                DateTime beginDate = begin.ToDateTime(_zeroHour);

                if((beginDate - now).TotalHours < 24)
                {
                    return (IsValid: false, Message: "Buy X Take Y event must be submitted by at least 24 hours prior.");
                }

                if(begin >= end)
                {
                    return (IsValid: false, Message: "Begin and end dates must be different dates with end date later than begin date.");
                }

                if(buyQuantity <= 0)
                {
                    return (IsValid: false, Message: $"Buy quantity ({buyQuantity}) must be greater than zero.");
                }

                if (takeQuantity <= 0)
                {
                    return (IsValid: false, Message: $"Take quantity ({takeQuantity}) must be greater than zero.");
                }

                return (IsValid: true, Message: "");
            };

            IEnumerable<MetromartBuyXTakeYConfigurationTemplateItem> items = _reader.ReadFrom(
                collection: request.InputFile,
                specification: specification,
                throwIfSpecificationNotMet: true);

            await _writer.WriteAsync(
                data: items,
                cancellationToken: cancellationToken);

            return Unit.Value;
        }
    }
}
