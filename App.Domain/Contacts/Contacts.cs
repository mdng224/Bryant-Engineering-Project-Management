using App.Domain.Common;
using App.Domain.Common.Abstractions;
using App.Domain.Clients;

namespace App.Domain.Contacts;

public sealed class Contact : IAuditableEntity, ISoftDeletable
{
    // EF
    private Contact() { }

    public Contact(
        Guid? clientId,
        string? title,                // Title
        string firstName,             // First Name
        string? middleName,           // Middle Name
        string lastName,              // Last Name
        string? suffix,               // Suffix
        string? company,              // Company
        string? department,           // Department
        string? jobTitle,             // Job Title
        string? businessStreet1,      // Business Street
        string? businessStreet2,      // Business Street 2
        string? businessStreet3,      // Business Street 3
        string? businessCity,         // Business City
        string? businessState,        // Business State
        string? businessPostalCode,   // Business Postal Code
        string? businessCountry,      // Business Country/Region
        string? businessPhone,        // Business Phone
        string? mobilePhone,          // Mobile Phone
        PrimaryPhoneKind primaryPhone,// Primary Phone kind
        string? emailAddress,         // E-mail Address
        string? webPage,              // Web Page
        bool isPrimaryForClient)
    {
        Id = Guid.CreateVersion7();

        SetName(title, firstName, middleName, lastName, suffix);
        SetCompanyInfo(company, department, jobTitle);
        SetBusinessAddress(
            businessStreet1,
            businessStreet2,
            businessStreet3,
            businessCity,
            businessState,
            businessPostalCode,
            businessCountry);
        SetPhones(businessPhone, mobilePhone, primaryPhone);
        SetContactInfo(emailAddress, webPage);

        ClientId           = clientId;
        IsPrimaryForClient = isPrimaryForClient;
    }

    // --- Keys / FKs ----------------------------------------------------------
    public Guid Id { get; private set; }

    public Guid? ClientId { get; private set; }
    public Client? Client { get; private set; }

    // --- Person name ---------------------------------------------------------
    public string? NamePrefix { get; private set; } // Title
    public string  FirstName  { get; private set; } = null!;
    public string? MiddleName { get; private set; }
    public string  LastName   { get; private set; } = null!;
    public string? NameSuffix { get; private set; }

    // --- Company / role ------------------------------------------------------
    public string? Company    { get; private set; }
    public string? Department { get; private set; }
    public string? JobTitle   { get; private set; }

    // --- Business address ----------------------------------------------------
    public Address? Address       { get; private set; }
    public string?  Country { get; private set; }

    // --- Phones / contact info ----------------------------------------------
    public string? BusinessPhone   { get; private set; }
    public string? MobilePhone     { get; private set; }
    public PrimaryPhoneKind PrimaryPhone { get; private set; }

    public string? Email   { get; private set; }
    public string? WebPage { get; private set; }

    /// <summary>
    /// Whether this is the primary contact for its client (if any).
    /// </summary>
    public bool IsPrimaryForClient { get; private set; }

    // --- Auditing / soft delete ---------------------------------------------
    public DateTimeOffset CreatedAtUtc  { get; private set; }
    public DateTimeOffset UpdatedAtUtc  { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public Guid? CreatedById            { get; private set; }
    public Guid? UpdatedById            { get; private set; }
    public Guid? DeletedById            { get; private set; }
    public bool IsDeleted => DeletedAtUtc.HasValue;

    // ========================================================================
    // SEED: from CSV row (raw Outlook export)
    // ========================================================================
    /// <summary>
    /// CSV-aligned seed method. Pass the raw values from your parsed CSV; 
    /// normalization happens inside.
    /// </summary>
    public static Contact Seed(
        Guid? clientId,
        string? title,                // Title
        string? firstName,            // First Name
        string? middleName,           // Middle Name
        string? lastName,             // Last Name
        string? suffix,               // Suffix
        string? company,              // Company
        string? department,           // Department
        string? jobTitle,             // Job Title
        string? businessStreet1,      // Business Street
        string? businessStreet2,      // Business Street 2
        string? businessStreet3,      // Business Street 3
        string? businessCity,         // Business City
        string? businessState,        // Business State
        string? businessPostalCode,   // Business Postal Code
        string? businessCountry,      // Business Country/Region
        string? businessPhone,        // Business Phone
        string? mobilePhone,          // Mobile Phone
        string? primaryPhone,         // Primary Phone (value in CSV)
        string? emailAddress,         // E-mail Address
        string? webPage               // Web Page
    )
    {
        // Fallback so we don't blow up if someone sends blanks.
        var safeFirst = firstName ?? string.Empty;
        var safeLast  = lastName  ?? string.Empty;

        var primaryKind = DeterminePrimaryPhoneKind(
            businessPhone,
            mobilePhone,
            primaryPhone);

        return new Contact(
            clientId:          clientId,
            title:             title,
            firstName:         safeFirst,
            middleName:        middleName,
            lastName:          safeLast,
            suffix:            suffix,
            company:           company,
            department:        department,
            jobTitle:          jobTitle,
            businessStreet1:   businessStreet1,
            businessStreet2:   businessStreet2,
            businessStreet3:   businessStreet3,
            businessCity:      businessCity,
            businessState:     businessState,
            businessPostalCode: businessPostalCode,
            businessCountry:   businessCountry,
            businessPhone:     businessPhone,
            mobilePhone:       mobilePhone,
            primaryPhone:      primaryKind,
            emailAddress:      emailAddress,
            webPage:           webPage,
            isPrimaryForClient: false // will be managed in app/UI
        );
    }

    private static PrimaryPhoneKind DeterminePrimaryPhoneKind(
        string? businessPhone,
        string? mobilePhone,
        string? primaryPhoneValue)
    {
        var normalizedBusiness = businessPhone.ToNormalizedPhone();
        var normalizedMobile   = mobilePhone.ToNormalizedPhone();
        var normalizedPrimary  = primaryPhoneValue.ToNormalizedPhone();

        if (normalizedPrimary is null)
            return PrimaryPhoneKind.Unknown;

        if (normalizedBusiness is not null && normalizedPrimary == normalizedBusiness)
            return PrimaryPhoneKind.Business;

        if (normalizedMobile is not null && normalizedPrimary == normalizedMobile)
            return PrimaryPhoneKind.Mobile;

        return PrimaryPhoneKind.Unknown;
    }

    // --- Mutators ------------------------------------------------------------
    public void SetClient(Guid? clientId)
    {
        EnsureNotDeleted();
        ClientId = clientId;
    }

    public void SetName(
        string? namePrefix,
        string firstName,
        string? middleName,
        string lastName,
        string? nameSuffix)
    {
        EnsureNotDeleted();

        var newFirst = firstName.ToNormalizedName();
        if (string.IsNullOrWhiteSpace(newFirst))
            throw new InvalidOperationException("First name is required.");

        var newLast = lastName.ToNormalizedName();
        if (string.IsNullOrWhiteSpace(newLast))
            throw new InvalidOperationException("Last name is required.");

        var newPrefix = NullIfBlank(namePrefix);
        var newMiddle = middleName?.ToNormalizedName();
        var newSuffix = NullIfBlank(nameSuffix);

        if (FirstName  != newFirst)  FirstName  = newFirst;
        if (LastName   != newLast)   LastName   = newLast;
        if (NamePrefix != newPrefix) NamePrefix = newPrefix;
        if (MiddleName != newMiddle) MiddleName = newMiddle;
        if (NameSuffix != newSuffix) NameSuffix = newSuffix;
    }

    public void SetCompanyInfo(string? company, string? department, string? jobTitle)
    {
        EnsureNotDeleted();

        var newCompany    = NullIfBlank(company);
        var newDepartment = NullIfBlank(department);
        var newJobTitle   = NullIfBlank(jobTitle);

        if (Company    != newCompany)    Company    = newCompany;
        if (Department != newDepartment) Department = newDepartment;
        if (JobTitle   != newJobTitle)   JobTitle   = newJobTitle;
    }

    public void SetBusinessAddress(
        string? street1,
        string? street2,
        string? street3,
        string? city,
        string? state,
        string? postalCode,
        string? countryRegion)
    {
        EnsureNotDeleted();

        var combinedLine2 = CombineLines(street2, street3);
        var address = CreateBusinessAddressOrNull(
            street1,
            combinedLine2,
            city,
            state,
            postalCode);

        if (Address != address)
            Address = address;

        var normalizedCountry = NullIfBlank(countryRegion);
        if (Country != normalizedCountry)
            Country = normalizedCountry;
    }

    public void SetPhones(string? businessPhone, string? mobilePhone, PrimaryPhoneKind primaryPhone)
    {
        EnsureNotDeleted();

        var newBusiness = businessPhone.ToNormalizedPhone();
        var newMobile   = mobilePhone.ToNormalizedPhone();

        if (BusinessPhone != newBusiness) BusinessPhone = newBusiness;
        if (MobilePhone   != newMobile)   MobilePhone   = newMobile;

        PrimaryPhone = primaryPhone;
    }

    public void SetContactInfo(string? email, string? webPage)
    {
        EnsureNotDeleted();

        var newEmail   = string.IsNullOrWhiteSpace(email) ? null : email.ToNormalizedEmail();
        var newWebPage = string.IsNullOrWhiteSpace(webPage) ? null : webPage.Trim();

        if (Email   != newEmail)   Email   = newEmail;
        if (WebPage != newWebPage) WebPage = newWebPage;
    }

    public void SetPrimaryForClient(bool isPrimary)
    {
        EnsureNotDeleted();
        IsPrimaryForClient = isPrimary;
    }

    public void SoftDelete(Guid deletedById)
    {
        if (IsDeleted)
            return;

        if (IsPrimaryForClient && ClientId is not null)
            throw new InvalidOperationException(
                "Cannot soft-delete the primary contact for a client. Reassign primary first.");

        DeletedAtUtc = DateTimeOffset.UtcNow;
        DeletedById  = deletedById;
    }

    public bool Restore()
    {
        if (!IsDeleted)
            return false;

        DeletedAtUtc = null;
        DeletedById  = null;
        return true;
    }

    // --- Helpers -------------------------------------------------------------
    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot mutate a soft-deleted contact.");
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string? CombineLines(string? line2, string? line3)
    {
        var l2 = NullIfBlank(line2);
        var l3 = NullIfBlank(line3);

        if (l2 is null && l3 is null) return null;
        if (l2 is null) return l3;
        if (l3 is null) return l2;

        return $"{l2}; {l3}";
    }

    private static Address? CreateBusinessAddressOrNull(
        string? line1,
        string? line2,
        string? city,
        string? state,
        string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(line1)
            && string.IsNullOrWhiteSpace(line2)
            && string.IsNullOrWhiteSpace(city)
            && string.IsNullOrWhiteSpace(state)
            && string.IsNullOrWhiteSpace(postalCode))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(line1)
            || string.IsNullOrWhiteSpace(city)
            || string.IsNullOrWhiteSpace(state)
            || string.IsNullOrWhiteSpace(postalCode))
        {
            throw new InvalidOperationException(
                "Business address must be fully provided (line1, city, state, postal code) or not provided at all.");
        }

        if (state.Trim().Length != 2)
            throw new InvalidOperationException("State must be a 2-letter code (e.g., KY).");

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
}
