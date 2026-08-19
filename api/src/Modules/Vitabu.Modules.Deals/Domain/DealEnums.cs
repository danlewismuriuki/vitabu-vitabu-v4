namespace Vitabu.Modules.Deals.Domain;

public enum InterestStatus
{
    Pending,
    Accepted,
    Waitlisted,
    Declined,
    Cancelled,
    Completed,
    Disputed
}

public enum HandoffMode
{
    Meetup,
    PickupMtaani
}
