using App.Domain.Common;
using App.Domain.Common.Abstractions;

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

    public Address? Address { get; private set; }
    public string? Note { get; private set; }

    // --- Auditing ------------------------------------------------------------
    public DateTimeOffset CreatedAtUtc { get; }
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
        Address? address = null,
        string? note = null)
    {
        Id = Guid.CreateVersion7();

        // Required
        Email = Guard.AgainstNullOrWhiteSpace(email, nameof(email)).ToNormalizedEmail();

        // Optional: null if empty/whitespace
        CompanyName = string.IsNullOrWhiteSpace(companyName) ? null : companyName.ToNormalizedName();
        ContactName = string.IsNullOrWhiteSpace(contactName) ? null : contactName.ToNormalizedName();
        Phone = phone.ToNormalizedPhone();
        Address = new Address(address?.Line1, address?.Line2, address?.City, address?.State, address?.PostalCode);
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

        var changed = false;

        if (newCompany != CompanyName) { CompanyName = newCompany; changed = true; }
        if (newContact != ContactName) { ContactName = newContact; changed = true; }
        if (newEmail != Email) { Email = newEmail; changed = true; }
        if (newPhone != Phone) { Phone = newPhone; changed = true; }

        if (changed) Touch();
    }

    public void SetAddress(Address? address)
    {
        EnsureNotDeleted();

        // if null — clear the address
        switch (address)
        {
            case null when Address is null:
                return;
            case null when Address is not null:
                Address = null;
                Touch();
                return;
        }

        // Normalize input values
        var line1 = address!.Line1.ToNormalizedAddressLine();
        var line2 = address.Line2.ToNormalizedAddressLine();
        var city = address.City.ToNormalizedCity();
        var state = address.State.ToNormalizedState();
        var postalCode = address.PostalCode.ToNormalizedPostal();

        // Construct a new normalized address value object
        var newAddress = new Address(line1, line2, city, state, postalCode);

        // Only update if something actually changed
        if (Address == newAddress)
            return;

        Address = newAddress;
        Touch();
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