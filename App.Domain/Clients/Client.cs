using App.Domain.Common;
using App.Domain.Common.Abstractions;

namespace App.Domain.Clients;

public sealed class Client : IAuditableEntity
{
    // --- Key ------------------------------------------------------------------
    public Guid Id { get; private set; }

    // --- Core Fields ----------------------------------------------------------
    
    // Company name is required, if First Name and Last Name are left blank
    public string? CompanyName { get; private set; }   
    public string? FirstName { get; private set; }
    public string? MiddleName { get; private set; }
    public string? LastName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public Address? Address { get; private set; }
    public string? Note { get; private set; }

    // --- Auditing ------------------------------------------------------------
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }

    // TODO: add projects when that domain is ready
    // public ICollection<Project> Projects { get; } = new List<Project>();
    private readonly bool _suppressTouch;
    
    // --- Constructors --------------------------------------------------------
    private Client() { } // EF
    public Client(
        string? email = null,
        string? companyName = null,
        string? firstName = null,
        string? lastName = null,
        string? middleName = null,
        string? phone = null,
        Address? address = null,
        string? note = null)
    {
        Id = Guid.CreateVersion7();

        _suppressTouch = true;

        SetContactInfoInternal(companyName, firstName, lastName, middleName, email, phone);
        SetAddressInternal(address);
        SetNoteInternal(note);

        var now = DateTimeOffset.UtcNow;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;

        _suppressTouch = false;
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
        var changed = SetContactInfoInternal(companyName, firstName, lastName, middleName, email, phone);
        if (changed) Touch();
    }

    public void SetAddress(Address? address)
    {
        EnsureNotDeleted();
        var changed = SetAddressInternal(address);
        if (changed) Touch();
    }

    public void SetNote(string? note)
    {
        EnsureNotDeleted();
        var changed = SetNoteInternal(note);
        if (changed) Touch();
    }

    public void SoftDelete()
    {
        if (DeletedAtUtc is not null) return;
        DeletedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DeletedAtUtc.Value;
    }

    public void Restore()
    {
        if (DeletedAtUtc is null) return;
        DeletedAtUtc = null;
        Touch();
    }

    // --- Core Setters ---------------------------------------------------------
    private bool SetContactInfoInternal(
        string? companyName,
        string? firstName,
        string? lastName,
        string? middleName,
        string? email,
        string? phone)
    {
        var newEmail   = string.IsNullOrWhiteSpace(email) ? null : email.ToNormalizedEmail();
        var newCompany = string.IsNullOrWhiteSpace(companyName) ? null : companyName.ToNormalizedName();
        var newFirst   = string.IsNullOrWhiteSpace(firstName)   ? null : firstName.ToNormalizedName();
        var newLast    = string.IsNullOrWhiteSpace(lastName)    ? null : lastName.ToNormalizedName();
        var newMiddle  = string.IsNullOrWhiteSpace(middleName)  ? null : middleName.ToNormalizedName();
        var newPhone   = phone.ToNormalizedPhone();

        // Rule: if both names are blank, CompanyName must be provided.
        if (newFirst is null && newLast is null && newCompany is null)
            throw new InvalidOperationException("CompanyName is required when both FirstName and LastName are blank.");

        var changed = false;

        if (newCompany != CompanyName) { CompanyName = newCompany; changed = true; }
        if (newFirst   != FirstName)   { FirstName   = newFirst;   changed = true; }
        if (newLast    != LastName)    { LastName    = newLast;    changed = true; }
        if (newMiddle  != MiddleName)  { MiddleName  = newMiddle;  changed = true; }
        if (newEmail   != Email)       { Email       = newEmail;   changed = true; }
        if (newPhone   != Phone)       { Phone       = newPhone;   changed = true; }

        return changed;
    }

    private bool SetAddressInternal(Address? address)
    {
        if (address is null)
        {
            if (Address is null) return false;
            Address = null;
            return true;
        }

        var line1 = address.Line1.ToNormalizedAddressLine();
        var line2 = address.Line2.ToNormalizedAddressLine();
        var city = address.City.ToNormalizedCity();
        var state = address.State.ToNormalizedState();
        var postalCode = address.PostalCode.ToNormalizedPostal();

        var newAddress = new Address(line1, line2, city, state, postalCode);

        if (Address == newAddress) return false;

        Address = newAddress;
        return true;
    }

    private bool SetNoteInternal(string? note)
    {
        var normalized = note.ToNormalizedNote();
        if (normalized == Note) return false;

        Note = normalized;
        return true;
    }

    // --- Helpers --------------------------------------------------------------
    private void EnsureNotDeleted()
    {
        if (DeletedAtUtc is not null)
            throw new InvalidOperationException("Cannot mutate a soft-deleted client.");
    }

    private void Touch()
    {
        if (_suppressTouch) return;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}