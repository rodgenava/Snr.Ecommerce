using Core.ControlledUpdates;
using Data.Common.Contracts;

namespace Application.Repositories.ControlledUpdates
{
    public interface ICampaignRepository : IAsyncRepository<Guid, Campaign>
    {
    }
}
