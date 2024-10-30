namespace Core.ControlledUpdates
{
    public record Duration
    {
        public DateOnly Begin { get; }
        public DateOnly End { get; }

        public Duration(DateOnly begin, DateOnly end)
        {
            if(begin > end)
            {
                throw new DomainLogicException(
                    message: "Begin date should be sooner than or same as end date.",
                    innerException: new ArgumentOutOfRangeException(paramName: nameof(begin)));
            }

            Begin = begin;
            End = end;
        }

        public Duration Extend(int days)
        {
            var end = End.AddDays(days);

            if(Begin > end)
            {
                throw new DomainLogicException("End date should be later than or same as begin date.");
            }

            return new Duration(Begin, end);
        }

        public Duration Extend(DateOnly until)
        {
            if (Begin > until)
            {
                throw new DomainLogicException("End date should be later than or same as begin date.");
            }

            return new Duration(Begin, until);
        }
    }
}