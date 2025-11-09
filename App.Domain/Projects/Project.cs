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
    public string Name     { get; private set; }
    public string Code     { get; private set; }    // legacy (e.g., "01-2632")
    public int Year        { get; private init; }
    public int Number      { get; private init; }
    public string NewCode  => $"{Year}-{Number}";
    public string Scope    { get; private set; }
    public string Manager  { get; private set; }
    
    // TODO: May need to create a project type
    public string Type     { get; private set; }
    
    // TODO: Ask andy if we need this to be a specific address or just a location string
    public Address? Address { get; private set; }  // Seed will put everything in Line1 for now
    
    // 🔗 FK + navigation
    public Guid ClientId { get; private set; }
    public Client Client { get; private set; } = null!;
    
    // --- Auditing ----------------------------------------------------------
    public DateTimeOffset CreatedAtUtc  { get; private set; }
    public DateTimeOffset UpdatedAtUtc  { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public Guid? CreatedById            { get; private set; }
    public Guid? UpdatedById            { get; private set; }
    public Guid? DeletedById            { get; private set; }
    public bool IsDeleted => DeletedAtUtc.HasValue;
    
    // --- Constructors --------------------------------------------------------
    private Project(Guid clientId, string name, string code, int year, int number, string scope, string manager,
        string type, DateTimeOffset? deletedAtUtc, Guid? deletedById)
    {
        Id = Guid.CreateVersion7();
        ClientId = clientId;
        Name = name.Trim();
        Code = NormalizeLegacyCode(code);
        Year = year;
        Number = number;
        Scope = scope.Trim();
        Manager = manager.Trim();
        Type = type.Trim();
        DeletedAtUtc = deletedAtUtc;
        DeletedById = deletedById;
    } // Seed constructor

    // --- Factory --------------------------------------------------------------
    public static Project Seed(Guid clientId, string name, string projectCode, string scope, string manager,
        string status, string location, string type, DateTimeOffset deletedNow)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required.", nameof(name));

        var isClosed = string.Equals(status.Trim(), "CLOSED", StringComparison.OrdinalIgnoreCase);
        var (year, number) = ParseLegacyCode(projectCode)
                             ?? throw new ArgumentException($"Invalid project code '{projectCode}'", nameof(projectCode));

        var project = new Project(
            clientId: clientId,
            name:     name,
            code:     projectCode,
            year:     year,
            number:   number,
            scope:    scope.Trim(),
            manager:  manager,
            type:     type,
            deletedAtUtc: isClosed ? deletedNow : null,
            deletedById:  isClosed ? Guid.Empty : null
        )
        {
            Address = new Address(
                Line1: location.Trim(),
                Line2: null,
                City: null,
                State: null,
                PostalCode: null)
        };

        return project;
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
        int year;
        
        if (yearPart.Length == 2)
        {
            // Assume "00"–"24" = 2000–2024, else treat as 1900s
            var y = int.Parse(yearPart);
            year = (y <= 24 ? 2000 : 1900) + y;
        }
        else
        {
            year = int.Parse(yearPart);
        }

        return (year, number);
    }

    private static string NormalizeLegacyCode(string s) => s.Trim().Replace('—', '-').Replace('–', '-');
}