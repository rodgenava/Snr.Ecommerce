using Data.Common.Contracts;

namespace Core.ControlledUpdates
{
    public class CampaignCreatedDataEvent : DataEvent
    {
        public Guid Id { get; }
        public Description Description { get; }
        public Duration Duration { get; }
        public IEnumerable<Warehouse> Warehouses { get; }
        public IEnumerable<Scope> Scopes { get; }

        public CampaignCreatedDataEvent(Guid id, Description description, Duration duration, IEnumerable<Warehouse> warehouses, IEnumerable<Scope> scopes)
        {
            Id = id;
            Description = description;
            Duration = duration;
            Warehouses = warehouses;
            Scopes = scopes;
        }
    }

    public class CampaignCancelledDataEvent : DataEvent
    {
        public Guid Id { get; }

        public CampaignCancelledDataEvent(Guid id)
        {
            Id = id;
        }
    }

    public class CampaignDurationChangedDataEvent : DataEvent
    {
        public Guid Id { get; }
        public Duration Duration { get; }

        public CampaignDurationChangedDataEvent(Guid id, Duration duration)
        {
            Id = id;
            Duration = duration;
        }
    }

    public class CampaignWarehousesChangedDataEvent : DataEvent
    {
        public Guid Id { get; }
        public IEnumerable<Warehouse> Warehouses { get; }

        public CampaignWarehousesChangedDataEvent(Guid id, IEnumerable<Warehouse> warehouses)
        {
            Id = id;
            Warehouses = warehouses;
        }
    }

    public class CampaignScopesChangedDataEvent : DataEvent
    {
        public Guid Id { get; }
        public IEnumerable<Scope> Scopes { get; }

        public CampaignScopesChangedDataEvent(Guid id, IEnumerable<Scope> scopes)
        {
            Id = id;
            Scopes = scopes;
        }
    }

    public class Campaign : IDataEventSource
    {
        #region Factories

        public static Campaign New(Description description, Duration duration, IEnumerable<Warehouse> warehouses, IEnumerable<Scope> scopes)
        {
            if (!warehouses.Any())
            {
                throw new DomainLogicException("Cannot create a campaign with empty warehouse.");
            }

            if (!scopes.Any())
            {
                throw new DomainLogicException("Cannot create a campaign with empty scope.");
            }

            var campaign = new Campaign(
                id: Guid.NewGuid(),
                description: description,
                duration: duration,
                warehouses: warehouses,
                scopes: scopes);

            campaign._events.Add(new CampaignCreatedDataEvent(
                id: campaign.Id,
                description: campaign.Description,
                duration: campaign.Duration,
                warehouses: campaign.Warehouses,
                scopes: campaign.Scopes));

            return campaign;
        }

        public static Campaign Existing(Guid id, Description description, Duration duration, IEnumerable<Warehouse> warehouses, IEnumerable<Scope> scopes)
        {
            return new Campaign(
                id: id,
                description: description,
                duration: duration,
                warehouses: warehouses,
                scopes: scopes);
        }

        #endregion

        #region Fields

        private readonly List<DataEvent> _events = new();
        private List<Warehouse> _warehouses;
        private List<Scope> _scopes;

        public Guid Id { get; }
        public Description Description { get; private set; }
        public Duration Duration { get; private set; }
        public IEnumerable<Warehouse> Warehouses => _warehouses;
        public IEnumerable<Scope> Scopes => _scopes;
        public bool IsCancelled { get; private set; }

        public bool IsInEffect => Duration.Begin < DateOnly.FromDateTime(DateTime.Now);

        public bool HasEnded => Duration.End < DateOnly.FromDateTime(DateTime.Now);

        #endregion

        #region Constructor

        private Campaign(Guid id, Description description, Duration duration, IEnumerable<Warehouse> warehouses, IEnumerable<Scope> scopes)
        {
            Id = id;
            Description = description;
            Duration = duration;
            _warehouses = warehouses.ToList();
            _scopes = scopes.ToList();
        }

        #endregion

        public void ChangeDuration(Duration duration)
        {
            if (HasEnded)
            {
                throw new DomainLogicException("Cannot change duration of past campaign.");
            }

            bool campaignAlreadyInEffect = IsInEffect;

            bool newDurationBeginEarlierThanCampaignBegin = Duration.Begin > duration.Begin;

            if (campaignAlreadyInEffect && newDurationBeginEarlierThanCampaignBegin)
            {
                throw new DomainLogicException("Cannot set Campain duration earlier because it already took effect.");
            }

            Duration = duration;

            _events.Add(new CampaignDurationChangedDataEvent(id: Id, duration: Duration));
        }

        public void ChangeWarehouses(IEnumerable<Warehouse> warehouses)
        {
            if (HasEnded)
            {
                throw new DomainLogicException("Cannot modify a campaign once ended.");
            }

            if (!warehouses.Any())
            {
                throw new DomainLogicException("Cannot set campaign warehouses to empty.");
            }

            _warehouses = warehouses.ToList();

            _events.Add(new CampaignWarehousesChangedDataEvent(id: Id, warehouses: Warehouses));
        }

        public void ChangeScope(IEnumerable<Scope> scopes)
        {
            if (HasEnded)
            {
                throw new DomainLogicException("Cannot modify a campaign once ended.");
            }

            if (!scopes.Any())
            {
                throw new DomainLogicException("Cannot set campaign scopes to empty.");
            }

            _scopes = scopes.ToList();

            _events.Add(new CampaignScopesChangedDataEvent(id: Id, scopes: Scopes));
        }

        public void Cancel()
        {
            if (IsCancelled || HasEnded)
            {
                throw new DomainLogicException("This campaign is already cancled or ended.");
            }

            IsCancelled = true;

            _events.Add(new CampaignCancelledDataEvent(id: Id));
        }

        public IEnumerable<DataEvent> ReleaseEvents()
        {
            IReadOnlyList<DataEvent> copy = _events.ToList();

            _events.Clear();

            return copy;
        }
    }
}