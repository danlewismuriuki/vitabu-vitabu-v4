namespace Vitabu.Modules.Deals.Domain;

public enum InterestStatus
{
    Pending,
    Accepted,
    Waitlisted,
    Declined,
    Cancelled,
    Completed
}

public enum HandoffMode
{
    Meetup,
    PickupMtaani
}
