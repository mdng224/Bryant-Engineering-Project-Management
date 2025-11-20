using App.Domain.Common;
using App.Domain.Common.Abstractions;
using App.Domain.Contacts;
using App.Domain.Projects;

namespace App.Domain.Clients;

public sealed class Client : IAuditableEntity, ISoftDeletable
{
    // --- Constructors --------------------------------------------------------
    private Client() { } // EF

    public Client(
        string name,
        Guid? categoryId,
        Guid? typeId,
        string? projectCode // legacy optional
    )
    {
        Id   = Guid.CreateVersion7();
        Name = SetNameInternal(name);
        CategoryId = categoryId;
        TypeId     = typeId;
        ProjectCode = projectCode?.Trim();
    }
    
    // --- Properties -----------------------------------------------------------
    public Guid Id { get; private set; }
    // Company / person
    public string Name        { get; private set; } = null!;  // company or household name (client_name)
    
    // Lookups
    public Guid? CategoryId { get; private set; }  // from "CLIENT CATEGORY" lookup
    public Guid? TypeId     { get; private set; }  // from "CLIENT TYPE" lookup

    // Legacy linkage (optional)
    public string? ProjectCode { get; private set; }     // legacy: how clients/projects were linked (maybe null)

    // 🔁 Navigation
    private readonly List<Project> _projects = [];
    public IReadOnlyCollection<Project> Projects => _projects;
    private readonly List<Contact> _contacts = [];
    public IReadOnlyCollection<Contact> Contacts => _contacts;

    // --- Auditing ----------------------------------------------------------
    public DateTimeOffset CreatedAtUtc  { get; private set; }
    public DateTimeOffset UpdatedAtUtc  { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public Guid? CreatedById            { get; private set; }
    public Guid? UpdatedById            { get; private set; }
    public Guid? DeletedById            { get; private set; }
    public bool IsDeleted => DeletedAtUtc.HasValue;
    
    // --- Factory (no person fields here) --------------------------------------
    public static Client Seed(
        string clientName,
        Guid? clientCategoryId,
        Guid? clientTypeId,
        string? legacyProjectCode = null)
    {

        return new Client(
            name:        clientName,
            categoryId:  clientCategoryId,
            typeId:      clientTypeId,
            projectCode: legacyProjectCode
        );
    }

    // --- Public Mutators ------------------------------------------------------
    public void SetLegacyProjectCode(string? code)
    {
        EnsureNotDeleted();
        var normalized = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        if (ProjectCode != normalized) ProjectCode = normalized;
    }
    
    // --- Internals ------------------------------------------------------------
    private string SetNameInternal(string name)
    {
        var newClientName = NullIfBlank(name);
        if (newClientName is null)
            throw new InvalidOperationException("Client name is required.");

        if (Name != newClientName)
            Name = newClientName;

        return Name;
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
    private void EnsureNotDeleted()
    {
        if (DeletedAtUtc is not null)
            throw new InvalidOperationException("Cannot mutate a soft-deleted client.");
    }

    private static string? NullIfBlank(string? s)  => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}