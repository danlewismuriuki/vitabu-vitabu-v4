using Microsoft.EntityFrameworkCore;
using Vitabu.Core.Exceptions;
using Vitabu.Modules.Catalog.Contracts;
using Vitabu.Modules.Catalog.Entities;
using Vitabu.Modules.Catalog.Persistence;

namespace Vitabu.Modules.Catalog.Services;

public interface ICatalogReadService
{
    Task<CbcTitlePage> SearchTitlesAsync(SearchCbcTitlesQuery query, CancellationToken ct = default);
    Task<SchoolPage> ListSchoolsAsync(string? city, int page, int pageSize, CancellationToken ct = default);
    Task<SchoolDetail> GetSchoolAsync(Guid id, CancellationToken ct = default);
}

public interface ISchoolWriteService
{
    Task<SchoolDetail> CreateAsync(CreateSchoolRequest request, CancellationToken ct = default);
}

public sealed class CatalogReadService(ICatalogDbContext catalogDb) : ICatalogReadService
{
    public async Task<CbcTitlePage> SearchTitlesAsync(SearchCbcTitlesQuery query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var q = catalogDb.CbcTitles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim().ToLowerInvariant();
            q = q.Where(t =>
                t.Title.ToLower().Contains(term) ||
                t.Subject.ToLower().Contains(term) ||
                t.Code.ToLower().Contains(term) ||
                t.Grade.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(query.Grade))
        {
            var grade = query.Grade.Trim();
            q = q.Where(t => t.Grade == grade);
        }

        if (!string.IsNullOrWhiteSpace(query.Subject))
        {
            var subject = query.Subject.Trim();
            q = q.Where(t => t.Subject == subject);
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(t => t.Grade)
            .ThenBy(t => t.Subject)
            .ThenBy(t => t.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new CbcTitleCard(
                t.Id,
                t.Code,
                t.Title,
                t.Grade,
                t.Subject,
                t.Term,
                t.MaterialType,
                t.Language))
            .ToListAsync(ct);

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new CbcTitlePage(items, page, pageSize, total, totalPages);
    }

    public async Task<SchoolPage> ListSchoolsAsync(
        string? city,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var q = catalogDb.Schools.AsNoTracking().Where(s => s.IsVerified);
        if (!string.IsNullOrWhiteSpace(city))
        {
            var c = city.Trim();
            q = q.Where(s => s.City == c);
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(s => s.City)
            .ThenBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SchoolCard(s.Id, s.Name, s.City, s.ContactName, s.IsVerified))
            .ToListAsync(ct);

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new SchoolPage(items, page, pageSize, total, totalPages);
    }

    public async Task<SchoolDetail> GetSchoolAsync(Guid id, CancellationToken ct = default)
    {
        var school = await catalogDb.Schools.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.IsVerified, ct)
            ?? throw NotFoundException.For("school", id);

        return ToDetail(school);
    }

    internal static SchoolDetail ToDetail(School school) =>
        new(
            school.Id,
            school.Name,
            school.City,
            school.ContactName,
            school.ContactPhoneE164,
            school.ContactEmail,
            school.IsVerified,
            school.Notes,
            school.CreatedAtUtc);
}

public sealed class SchoolWriteService(ICatalogDbContext catalogDb) : ISchoolWriteService
{
    public async Task<SchoolDetail> CreateAsync(CreateSchoolRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.City))
        {
            var errors = new Dictionary<string, string[]>();
            if (string.IsNullOrWhiteSpace(request.Name)) errors["name"] = ["Required."];
            if (string.IsNullOrWhiteSpace(request.City)) errors["city"] = ["Required."];
            throw new ValidationException("Name and city are required.", errors);
        }

        var school = new School
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            City = request.City.Trim(),
            ContactName = string.IsNullOrWhiteSpace(request.ContactName) ? null : request.ContactName.Trim(),
            ContactPhoneE164 = string.IsNullOrWhiteSpace(request.ContactPhoneE164)
                ? null
                : request.ContactPhoneE164.Trim(),
            ContactEmail = string.IsNullOrWhiteSpace(request.ContactEmail) ? null : request.ContactEmail.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            IsVerified = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        catalogDb.Schools.Add(school);
        await catalogDb.SaveChangesAsync(ct);
        return CatalogReadService.ToDetail(school);
    }
}
