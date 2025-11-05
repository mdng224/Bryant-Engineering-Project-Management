using App.Domain.Common;
using App.Domain.Common.Abstractions;

namespace App.Domain.Clients;

public sealed class Client : IAuditableEntity, ISoftDeletable
{
    // --- Key ------------------------------------------------------------------
    public Guid Id { get; private set; }

    // --- Core Fields ----------------------------------------------------------
    
    // Company name is required if FirstName and LastName are both blank
    public string? CompanyName { get; private set; }
    public string? FirstName   { get; private set; }
    public string? MiddleName  { get; private set; }
    public string? LastName    { get; private set; }
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

    public Client(
        string? email       = null,
        string? companyName = null,
        string? firstName   = null,
        string? lastName    = null,
        string? middleName  = null,
        string? phone       = null,
        Address? address    = null,
        string? note        = null)
    {
        Id = Guid.CreateVersion7();

        // normalize + apply rule via core setters
        SetContactInfoInternal(companyName, firstName, lastName, middleName, email, phone);
        SetAddressInternal(address);
        SetNoteInternal(note);
    }

    // --- Public Mutators ------------------------------------------------------
    public void ChangeContactInfo(
        string? companyName,
        string? firstName,
        string? lastName,
        string? middleName,
        string? email,
        string? phone)
    {
        EnsureNotDeleted();
        SetContactInfoInternal(companyName, firstName, lastName, middleName, email, phone);
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

    // --- Core Setters ---------------------------------------------------------
    private void SetContactInfoInternal(
        string? companyName,
        string? firstName,
        string? lastName,
        string? middleName,
        string? email,
        string? phone)
    {
        var newEmail   = string.IsNullOrWhiteSpace(email)       ? null : email.ToNormalizedEmail();
        var newCompany = string.IsNullOrWhiteSpace(companyName) ? null : companyName.ToNormalizedName();
        var newFirst   = string.IsNullOrWhiteSpace(firstName)   ? null : firstName.ToNormalizedName();
        var newLast    = string.IsNullOrWhiteSpace(lastName)    ? null : lastName.ToNormalizedName();
        var newMiddle  = string.IsNullOrWhiteSpace(middleName)  ? null : middleName.ToNormalizedName();
        var newPhone   = phone.ToNormalizedPhone();

        // Rule: if both names are blank, CompanyName must be provided.
        if (newFirst is null && newLast is null && newCompany is null)
            throw new InvalidOperationException("CompanyName is required when both FirstName and LastName are blank.");

        if (newCompany != CompanyName) CompanyName = newCompany;
        if (newFirst   != FirstName)   FirstName   = newFirst;
        if (newLast    != LastName)    LastName    = newLast;
        if (newMiddle  != MiddleName)  MiddleName  = newMiddle;
        if (newEmail   != Email)       Email       = newEmail;
        if (newPhone   != Phone)       Phone       = newPhone;
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
}