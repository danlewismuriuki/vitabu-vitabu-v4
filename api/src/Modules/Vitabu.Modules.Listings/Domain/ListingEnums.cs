namespace Vitabu.Modules.Listings.Domain;

public enum ListingIntent
{
    Sale,
    Free,
    Exchange,
    DonateSchool
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
    Donated,
    Paused,
    Hidden
}
