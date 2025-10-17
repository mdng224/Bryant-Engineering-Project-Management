using App.Domain.Common;

namespace App.Domain.Clients;

public sealed class Client : IAuditableEntity
{
    // --- Key ------------------------------------------------------------------
    public Guid Id { get; private set; }

    // --- Core Fields ----------------------------------------------------------
    public string? CompanyName { get; private set; }   // optional: Will retroactively be set
    public string? ContactName { get; private set; }   // optional: Will retroactively be set
    public string Email { get; private set; } = null!;
    public string? Phone { get; private set; }

    public string? AddressLine1 { get; private set; }
    public string? AddressLine2 { get; private set; }
    public string? City { get; private set; }
    public string? StateOrProvince { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Country { get; private set; }
    public string? Note { get; private set; }

    // --- Auditing ------------------------------------------------------------
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }

    // TODO: Add client contact domain
    // public ICollection<ClientContact> Contacts { get; } = new List<ClientContact>();

    // TODO: add projects when that domain is ready
    // public ICollection<Project> Projects { get; } = new List<Project>();

    // --- Constructors --------------------------------------------------------
    private Client() { } // EF
    public Client(
        string email,
        string? companyName = null,
        string? contactName = null,
        string? phone = null,
        string? addressLine1 = null,
        string? addressLine2 = null,
        string? city = null,
        string? stateOrProvince = null,
        string? postalCode = null,
        string? country = null,
        string? note = null)
    {
        Id = Guid.CreateVersion7();

        // Required
        Email = Guard.AgainstNullOrWhiteSpace(email, nameof(email)).ToNormalizedEmail();

        // Optional: null if empty/whitespace
        CompanyName = string.IsNullOrWhiteSpace(companyName) ? null : companyName.ToNormalizedName();
        ContactName = string.IsNullOrWhiteSpace(contactName) ? null : contactName.ToNormalizedName();
        Phone = phone.ToNormalizedPhone();

        AddressLine1 = addressLine1.ToNormalizedAddressLine();
        AddressLine2 = addressLine2.ToNormalizedAddressLine();
        City = city.ToNormalizedCity();
        StateOrProvince = stateOrProvince.ToNormalizedState();
        PostalCode = postalCode.ToNormalizedPostal();
        Country = country.ToNormalizedCountry();

        Note = note.ToNormalizedNote();

        var now = DateTimeOffset.UtcNow;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    // --- Mutators (domain intent) ---------------------------------------------
    public void ChangeContactInfo(string companyName, string contactName, string email, string? phone)
    {
        EnsureNotDeleted();
        var newEmail = Guard.AgainstNullOrWhiteSpace(email, nameof(email)).ToNormalizedEmail();

        var newCompany = string.IsNullOrWhiteSpace(companyName) ? null : companyName.ToNormalizedName();
        var newContact = string.IsNullOrWhiteSpace(contactName) ? null : contactName.ToNormalizedName();
        var newPhone = phone.ToNormalizedPhone();

        bool changed = false;

        if (newCompany != CompanyName) { CompanyName = newCompany; changed = true; }
        if (newContact != ContactName) { ContactName = newContact; changed = true; }
        if (newEmail != Email) { Email = newEmail; changed = true; }
        if (newPhone != Phone) { Phone = newPhone; changed = true; }

        if (changed) Touch();
    }

    public void ChangeAddress(
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? stateOrProvince,
        string? postalCode,
        string? country)
    {
        EnsureNotDeleted();

        var a1 = addressLine1.ToNormalizedAddressLine();
        var a2 = addressLine2.ToNormalizedAddressLine();
        var c = city.ToNormalizedCity();
        var s = stateOrProvince.ToNormalizedState();
        var p = postalCode.ToNormalizedPostal();
        var co = country.ToNormalizedCountry();

        var changed = false;

        if (a1 != AddressLine1) { AddressLine1 = a1; changed = true; }
        if (a2 != AddressLine2) { AddressLine2 = a2; changed = true; }
        if (c != City) { City = c; changed = true; }
        if (s != StateOrProvince) { StateOrProvince = s; changed = true; }
        if (p != PostalCode) { PostalCode = p; changed = true; }
        if (co != Country) { Country = co; changed = true; }

        if (changed) Touch();
    }

    public void SetNote(string? note)
    {
        EnsureNotDeleted();

        var normalized = note.ToNormalizedNote();
        if (normalized == Note) return;

        Note = normalized;
        Touch();
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

    // --- Helpers --------------------------------------------------------------
    private void EnsureNotDeleted()
    {
        if (DeletedAtUtc is not null)
            throw new InvalidOperationException("Cannot mutate a soft-deleted client.");
    }
    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}