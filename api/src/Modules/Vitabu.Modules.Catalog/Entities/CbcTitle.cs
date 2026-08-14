namespace Vitabu.Modules.Catalog.Entities;

public sealed class CbcTitle
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public string MaterialType { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}
