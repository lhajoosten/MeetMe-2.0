namespace MeetMe.Domain.Common
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }
}