using App.Domain.Common;
using App.Domain.Common.Abstractions;

namespace App.Domain.Clients;

public sealed class Client : IAuditableEntity, ISoftDeletable
{
    // --- Key ------------------------------------------------------------------
    public Guid Id { get; private set; }

    // --- Core Fields ----------------------------------------------------------
    public string? Name          { get; private set; }
    // TODO: Might need to break this out into its own table
    public string? ContactFirst  { get; private set; }
    public string? ContactMiddle { get; private set; }
    public string? ContactLast   { get; private set; }
    public string? Email       { get; private set; }
    public string? Phone       { get; private set; }
    public Address? Address    { get; private set; }
    public string? Note        { get; private set; }

    // --- Auditing ----------------------------------------------------------
    public DateTimeOffset CreatedAtUtc  { get; private set; }
    public DateTimeOffset UpdatedAtUtc  { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public Guid? CreatedById            { get; private set; }
    public Guid? UpdatedById            { get; private set; }
    public Guid? DeletedById            { get; private set; }
    public bool IsDeleted => DeletedAtUtc.HasValue;
    
    // --- Constructors --------------------------------------------------------
    private Client() { } // EF

    private Client(
        string? name,
        string? contactFirst,
        string? contactMiddle,
        string? contactLast,
        string? email,
        string? phone,
        Address? address,
        string? note)
    {
        Id = Guid.CreateVersion7();
        SetContactInfoInternal(name, contactFirst, contactMiddle, contactLast, email, phone);
        SetAddressInternal(address);
        SetNoteInternal(note);
    }

    /// <summary>Minimal seed path: pass only what you have from CSV (company + contact names).</summary>
    public static Client Seed(string clientName, string? contactFirst, string? contactLast) =>
        new(
            name: clientName,
            contactFirst: contactFirst,
            contactMiddle: null,
            contactLast: contactLast,
            email: null,
            phone: null,
            address: null,
            note: null
        );

    // --- Public Mutators ------------------------------------------------------
    public void ChangeContactInfo(
        string? name,
        string? firstName,
        string? lastName,
        string? middleName,
        string? email,
        string? phone)
    {
        EnsureNotDeleted();
        SetContactInfoInternal(name, firstName, lastName, middleName, email, phone);
    }

    public void SetAddress(Address? address)
    {
        EnsureNotDeleted();
        SetAddressInternal(address);
    }

    public void SetNote(string? note)
    {
        EnsureNotDeleted();
        SetNoteInternal(note);
    }
    
    // --- Internals ------------------------------------------------------------
    private void SetContactInfoInternal(
        string? name,
        string? contactFirst,
        string? contactMiddle,
        string? contactLast,
        string? email,
        string? phone)
    {
        var newCompany = NullIfBlank(name);
        var newFirst   = contactFirst?.ToNormalizedName();
        var newMiddle  = contactMiddle?.ToNormalizedName();
        var newLast    = contactLast?.ToNormalizedName();
        var newEmail   = string.IsNullOrWhiteSpace(email) ? null : email.ToNormalizedEmail();
        var newPhone   = phone.ToNormalizedPhone();

        // Rule: if both names are blank, company must be provided.
        if (newFirst is null && newLast is null && newCompany is null)
            throw new InvalidOperationException("CompanyName is required when both FirstName and LastName are blank.");

        if (Name         != newCompany) Name         = newCompany;
        if (ContactFirst != newFirst)   ContactFirst = newFirst;
        if (ContactMiddle!= newMiddle)  ContactMiddle= newMiddle;
        if (ContactLast  != newLast)    ContactLast  = newLast;
        if (Email        != newEmail)   Email        = newEmail;
        if (Phone        != newPhone)   Phone        = newPhone;
    }

    private void SetAddressInternal(Address? address)
    {
        if (address is null)
        {
            if (Address is null) return;
            Address = null;
            return;
        }

        var line1      = address.Line1.ToNormalizedAddressLine();
        var line2      = address.Line2.ToNormalizedAddressLine();
        var city       = address.City.ToNormalizedCity();
        var state      = address.State.ToNormalizedState();
        var postalCode = address.PostalCode.ToNormalizedPostal();

        var newAddress = new Address(line1, line2, city, state, postalCode);
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