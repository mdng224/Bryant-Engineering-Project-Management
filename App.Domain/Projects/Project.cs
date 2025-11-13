using System.Text.RegularExpressions;
using App.Domain.Clients;
using App.Domain.Common;
using App.Domain.Common.Abstractions;

namespace App.Domain.Projects;

public sealed class Project : IAuditableEntity, ISoftDeletable
{
    // --- Key ------------------------------------------------------------------
    public Guid Id { get; private set; }
    
    // --- Core Fields ----------------------------------------------------------
    public string Name   { get; private set; }
    public string Code   { get; private set; } // legacy (e.g., "01-2632")
    public int Year        { get; private init; }
    public int Number      { get; private init; }
    public string NewCode  => $"{Year}-{Number}";
    public string Manager  { get; private set; }
    public string Type     { get; private set; }
    public string Location  { get; private set; }
    
    // 🔗 FKs + navigation
    public Guid ClientId { get; private set; }
    public Client Client { get; private set; } = null!;

    public Guid ScopeId { get; private set; }
    public Scope Scope  { get; private set; }
    
    // --- Auditing ----------------------------------------------------------
    public DateTimeOffset CreatedAtUtc  { get; private set; }
    public DateTimeOffset UpdatedAtUtc  { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public Guid? CreatedById            { get; private set; }
    public Guid? UpdatedById            { get; private set; }
    public Guid? DeletedById            { get; private set; }
    public bool IsDeleted => DeletedAtUtc.HasValue;
    
    // --- Constructors --------------------------------------------------------
    private Project(
        Guid clientId,
        Guid scopeId,
        string name,
        string code,
        int year,
        int number,
        string manager,
        string type,
        string location,
        DateTimeOffset? deletedAtUtc,
        Guid? deletedById)
    {
        Id = Guid.CreateVersion7();
        ClientId = clientId;
        ScopeId = scopeId;
        Name = name.Trim();
        Code = NormalizeLegacyCode(code);
        Year = year;
        Number = number;
        Manager = manager.Trim();
        Type = type.Trim();
        Location = location;
        DeletedAtUtc = deletedAtUtc;
        DeletedById = deletedById;
    }

    // --- Factory --------------------------------------------------------------
    public static Project Seed(
        Guid clientId,
        Guid scopeId,
        string name,
        string projectCode,
        string manager,
        string status,
        string location,
        string type,
        DateTimeOffset deletedNow)
    {
        if (clientId == Guid.Empty)  throw new ArgumentException("ClientId is required.", nameof(clientId));
        if (scopeId  == Guid.Empty)  throw new ArgumentException("ScopeId is required.",  nameof(scopeId));
        if (string.IsNullOrWhiteSpace(name))        throw new ArgumentException("Project name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(projectCode)) throw new ArgumentException("Project code is required.", nameof(projectCode));

        var parts = ParseLegacyCode(projectCode)
                    ?? throw new ArgumentException($"Invalid project code '{projectCode}'", nameof(projectCode));

        var isClosed = string.Equals(status?.Trim(), "CLOSED", StringComparison.OrdinalIgnoreCase);

        return new Project(
            clientId:    clientId,
            scopeId:     scopeId,
            name:        name,
            code:        projectCode,
            year:        parts.year,
            number:      parts.number,
            manager:     manager,
            type:        type,
            location:    location,
            deletedAtUtc: isClosed ? deletedNow : null,
            deletedById : isClosed ? Guid.Empty : null
        );
    }
    
    public bool Restore()
    {
        if (!IsDeleted)
            return false;
        
        DeletedAtUtc = null;
        DeletedById = null;
        return true;
    }
    
    // --- Helpers --------------------------------------------------------------
    private static (int year, int number)? ParseLegacyCode(string? rawProjectCode)
    {
        if (string.IsNullOrWhiteSpace(rawProjectCode))
            return null;
        
        var s = NormalizeLegacyCode(rawProjectCode);

        // Accept "01-2632", "2023-15032", etc.
        var match = Regex.Match(s, @"^\s*(\d{2,4})\s*[-–—]\s*(\d+)\s*$");
        if (!match.Success)
            return null;

        var yearPart = match.Groups[1].Value;
        var number   = int.Parse(match.Groups[2].Value);
        var year = yearPart.Length == 2
            ? (int.Parse(yearPart) <= 24 ? 2000 : 1900) + int.Parse(yearPart)
            : int.Parse(yearPart);

        return (year, number);
    }

    private static string NormalizeLegacyCode(string s) => s.Trim().Replace('—', '-').Replace('–', '-');
}