namespace Vitabu.Modules.Listings.Domain;

public enum ListingIntent
{
    Sale,
    Free,
    Exchange
}

public enum BookCondition
{
    LikeNew,
    Good,
    Fair,
    WritingInside
}

public enum ListingStatus
{
    Draft,
    Active,
    Reserved,
    Sold,
    Given,
    Exchanged,
    Paused,
    Hidden
}
