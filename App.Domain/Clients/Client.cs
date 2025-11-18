using App.Domain.Common;
using App.Domain.Common.Abstractions;
using App.Domain.Projects;

namespace App.Domain.Clients;

public sealed class Client : IAuditableEntity, ISoftDeletable
{
    // --- Constructors --------------------------------------------------------
    private Client() { } // EF

    public Client(
        string name,
        string? namePrefix,
        string firstName,
        string? middleName,
        string lastName,
        string? nameSuffix,
        string? email,
        string? phone,
        Address? address,
        string? note,
        Guid? categoryId,
        Guid? typeId,
        string? projectCode // legacy optional
    )
    {
        Id = Guid.CreateVersion7();
        SetContactInfoInternal(name, namePrefix, firstName, middleName, lastName, nameSuffix, email, phone);
        ApplyAddress(address);
        SetNoteInternal(note);
        CategoryId = categoryId;
        TypeId = typeId;
        ProjectCode = projectCode?.Trim();
    }
    
    // --- Properties -----------------------------------------------------------
    public Guid Id { get; private set; }
    // Company / person
    public string Name        { get; private set; } = null!;  // company or household name (client_name)
    public string? NamePrefix { get; private set; }  // name_prefix (e.g., Mr., Ms., Dr.)
    public string FirstName { get; private set; } = null!;  // first_name
    public string? MiddleName { get; private set; }
    public string LastName { get; private set; } = null!;  // last_name
    public string? NameSuffix { get; private set; }  // name_suffix (e.g., Jr., III)
    
    // Contact
    public string? Email { get; private set; }       // email
    public string? Phone { get; private set; }       // phone
    public Address? Address      { get; private set; }
    public string? Note          { get; private set; }
    
    // Lookups
    public Guid? CategoryId { get; private set; }  // from "CLIENT CATEGORY" lookup
    public Guid? TypeId     { get; private set; }  // from "CLIENT TYPE" lookup

    // Legacy linkage (optional)
    public string? ProjectCode { get; private set; }     // legacy: how clients/projects were linked (maybe null)

    // 🔁 Navigation (optional for EF, useful for domain logic)
    private readonly List<Project> _projects = [];
    public IReadOnlyCollection<Project> Projects => _projects;

    // --- Auditing ----------------------------------------------------------
    public DateTimeOffset CreatedAtUtc  { get; private set; }
    public DateTimeOffset UpdatedAtUtc  { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public Guid? CreatedById            { get; private set; }
    public Guid? UpdatedById            { get; private set; }
    public Guid? DeletedById            { get; private set; }
    public bool IsDeleted => DeletedAtUtc.HasValue;
    
    /// <summary>
    /// CSV-aligned seed method. Pass the raw values from your parsed CSV;
    /// normalize/lookups happen inside.
    /// </summary>
    public static Client Seed(
        string clientName,
        string? namePrefix,
        string firstName,
        string? middleName,
        string lastName,
        string? nameSuffix,
        string? email,
        string? phone,
        string? line1,
        string? line2,
        string? city,
        string? state,
        string? postalCode,
        string? note,
        Guid? clientCategoryId,
        Guid? clientTypeId,
        string? legacyProjectCode = null
    )
    {
        var address = CreateAddressOrNull(line1, line2, city, state, postalCode);

        return new Client(
            name:           clientName,
            namePrefix:     namePrefix,
            firstName:      firstName,
            middleName:     middleName,
            lastName:       lastName,
            nameSuffix:     nameSuffix,
            email:          email,
            phone:          phone,
            address:        address,
            note:           note,
            categoryId: clientCategoryId,
            typeId:     clientTypeId,
            projectCode:    legacyProjectCode
        );
    }

    // --- Public Mutators ------------------------------------------------------
    public void SetAddress(Address? address)
    {
        EnsureNotDeleted();
        ApplyAddress(address);
    }

    public void SetLegacyProjectCode(string? code)
    {
        EnsureNotDeleted();
        var normalized = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        if (ProjectCode != normalized) ProjectCode = normalized;
    }

    public void SetNote(string? note)
    {
        EnsureNotDeleted();
        SetNoteInternal(note);
    }
    
    // --- Internals ------------------------------------------------------------
    private void SetContactInfoInternal(
        string name,
        string? namePrefix,
        string firstName,
        string? middleName,
        string lastName,
        string? nameSuffix,
        string? email,
        string? phone)
    {
        // Company / client name
        var newClientName = NullIfBlank(name);
        if (newClientName is null)
            throw new InvalidOperationException("Client name is required.");

        // First / last names
        var newFirst = firstName.ToNormalizedName();
        if (string.IsNullOrWhiteSpace(newFirst))
            throw new InvalidOperationException("First name is required.");

        var newLast = lastName.ToNormalizedName();
        if (string.IsNullOrWhiteSpace(newLast))
            throw new InvalidOperationException("Last name is required.");
        
        var newPrefix    = NullIfBlank(namePrefix);
        var newSuffix    = NullIfBlank(nameSuffix);
        var newEmail     = string.IsNullOrWhiteSpace(email) ? null : email.ToNormalizedEmail();
        var newPhone     = phone.ToNormalizedPhone();
        var newMiddle    = middleName?.ToNormalizedName();
        
        if (Name       != newClientName) Name       = newClientName;
        if (NamePrefix != newPrefix)     NamePrefix = newPrefix;
        if (FirstName  != newFirst)      FirstName  = newFirst;
        if (MiddleName != newMiddle)     MiddleName = newMiddle;
        if (LastName   != newLast)       LastName   = newLast;
        if (NameSuffix != newSuffix)     NameSuffix = newSuffix;
        if (Email      != newEmail)      Email      = newEmail;
        if (Phone      != newPhone)      Phone      = newPhone;
    }

    private static Address? CreateAddressOrNull(
        string? line1,
        string? line2,
        string? city,
        string? state,
        string? postalCode)
    {
        // If everything is blank => no address
        if (string.IsNullOrWhiteSpace(line1)
            && string.IsNullOrWhiteSpace(line2)
            && string.IsNullOrWhiteSpace(city)
            && string.IsNullOrWhiteSpace(state)
            && string.IsNullOrWhiteSpace(postalCode))
        {
            return null;
        }

        // All-or-nothing: if *any* address info is provided,
        // then all required fields (Line1, City, State, PostalCode) must be present.
        if (string.IsNullOrWhiteSpace(line1)
            || string.IsNullOrWhiteSpace(city)
            || string.IsNullOrWhiteSpace(state)
            || string.IsNullOrWhiteSpace(postalCode))
        {
            throw new InvalidOperationException(
                "Address must be fully provided (line1, city, state, postal code) or not provided at all."
            );
        }
        
        if (state.Trim().Length != 2)
            throw new InvalidOperationException("State must be a 2-letter code (e.g., KY).");

        // Now safe to normalize (we know required ones are not null/whitespace)
        var normalizedLine1      = line1.ToNormalizedAddressLine();
        var normalizedLine2      = line2.ToNormalizedAddressLine();
        var normalizedCity       = city.ToNormalizedCity();
        var normalizedState      = state.ToNormalizedState();
        var normalizedPostalCode = postalCode.ToNormalizedPostal();

        return new Address(
            normalizedLine1!,
            normalizedLine2,
            normalizedCity!,
            normalizedState!,
            normalizedPostalCode!
        );
    }

    private void ApplyAddress(Address? address)
    {
        if (address is null)
        {
            if (Address is null) return;
            Address = null;
            return;
        }

        // Re-run through the same normalization + all-or-nothing logic.
        // This will throw if someone tries to pass a partial address.
        var newAddress = CreateAddressOrNull(
            address.Line1,
            address.Line2,
            address.City,
            address.State,
            address.PostalCode
        );

        if (Address == newAddress) return;
        Address = newAddress;
    }

    private void SetNoteInternal(string? note)
    {
        var normalized = note.ToNormalizedNote();
        if (normalized == Note) return;
        Note = normalized;
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