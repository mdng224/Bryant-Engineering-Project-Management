using App.Domain.Clients;
using App.Infrastructure.Persistence.Seed.Configurations;

namespace App.Infrastructure.Persistence.Seed.Factories;

internal static class ClientTypeSeedFactory
{
    public static IEnumerable<ClientType> All =>
    [
        // ---------------------------------------------------------------------
        // INDIVIDUAL / PRIVATE
        // ---------------------------------------------------------------------
        new(id: ClientTypeIds.Individual,
            name: "Individual / Private",
            description: "Homeowner, private property owner",
            categoryId: ClientCategoryIds.IndividualPrivate),

        new(id: ClientTypeIds.JointIndividuals,
            name: "Private-Sector Organization",
            description: "Married couple, co-owners",
            categoryId: ClientCategoryIds.IndividualPrivate),

        new(id: ClientTypeIds.EstateTrust,
            name: "Government / Public Agency",
            description: "\"Estate of John Doe\", family trust",
            categoryId: ClientCategoryIds.IndividualPrivate),

        new(id: ClientTypeIds.SoleProprietor,
            name: "Non-Profit / Institutional",
            description: "Independent business owner under own name",
            categoryId: ClientCategoryIds.IndividualPrivate),

        // ---------------------------------------------------------------------
        // PRIVATE-SECTOR ORGANIZATION
        // ---------------------------------------------------------------------
        new(id: ClientTypeIds.Corporation,
            name: "Corporation (Inc.)",
            description: "\"ABC Development, Inc.\"",
            categoryId: ClientCategoryIds.PrivateSectorOrganization),

        new(id: ClientTypeIds.LimitedLiabilityCompany,
            name: "Limited Liability Company (LLC)",
            description: "\"Smith Builders, LLC\"",
            categoryId: ClientCategoryIds.PrivateSectorOrganization),

        new(id: ClientTypeIds.Partnership,
            name: "Partnership (LP / GP)",
            description: "\"Greenfield Partners LP\"",
            categoryId: ClientCategoryIds.PrivateSectorOrganization),

        new(id: ClientTypeIds.DeveloperBuilder,
            name: "Developer / Builder",
            description: "Real-estate developer or home builder",
            categoryId: ClientCategoryIds.PrivateSectorOrganization),

        new(id: ClientTypeIds.ConsultantProfessionalFirm,
            name: "Consultant / Professional Firm",
            description: "Engineer, surveyor, architect",
            categoryId: ClientCategoryIds.PrivateSectorOrganization),

        new(id: ClientTypeIds.UtilityProviderPrivate,
            name: "Utility Provider (Private)",
            description: "Atmos Energy, Spectrum",
            categoryId: ClientCategoryIds.PrivateSectorOrganization),

        new(id: ClientTypeIds.PropertyManagementCompany,
            name: "Property Management Company",
            description: "Manages apartments, retail centers",
            categoryId: ClientCategoryIds.PrivateSectorOrganization),

        // ---------------------------------------------------------------------
        // GOVERNMENT / PUBLIC AGENCY
        // ---------------------------------------------------------------------
        new(id: ClientTypeIds.FederalAgency,
            name: "Federal Agency",
            description: "USACE, USDA, EPA, FEMA",
            categoryId: ClientCategoryIds.GovernmentPublicAgency),

        new(id: ClientTypeIds.StateAgency,
            name: "State Agency",
            description: "KYTC, KDOW, KDEP",
            categoryId: ClientCategoryIds.GovernmentPublicAgency),

        new(id: ClientTypeIds.CountyGovernment,
            name: "County Government",
            description: "Fiscal Court, Public Works, Road Dept",
            categoryId: ClientCategoryIds.GovernmentPublicAgency),

        new(id: ClientTypeIds.CityMunicipalGovernment,
            name: "City / Municipal Government",
            description: "City of Owensboro",
            categoryId: ClientCategoryIds.GovernmentPublicAgency),

        new(id: ClientTypeIds.RegionalAuthoritySpecialDistrict,
            name: "Regional Authority / Special District",
            description: "Daviess County Water District",
            categoryId: ClientCategoryIds.GovernmentPublicAgency),

        new(id: ClientTypeIds.SchoolDistrictBoardOfEducation,
            name: "School District / Board of Education",
            description: "Owensboro Public Schools",
            categoryId: ClientCategoryIds.GovernmentPublicAgency),

        new(id: ClientTypeIds.UniversityCollegePublic,
            name: "University / College (Public)",
            description: "WKU, KCTCS campus",
            categoryId: ClientCategoryIds.GovernmentPublicAgency),

        // ---------------------------------------------------------------------
        // NON-PROFIT / INSTITUTIONAL
        // ---------------------------------------------------------------------
        new(id: ClientTypeIds.ReligiousOrganizationChurch,
            name: "Religious Organization / Church",
            description: "St. Joseph Catholic Church",
            categoryId: ClientCategoryIds.NonProfitInstitutional),

        new(id: ClientTypeIds.NonProfitOrganization,
            name: "Non-Profit Organization",
            description: "Habitat for Humanity, United Way",
            categoryId: ClientCategoryIds.NonProfitInstitutional),

        new(id: ClientTypeIds.CommunityAssociationHoa,
            name: "Community Association / HOA",
            description: "\"Rolling Hills HOA\"",
            categoryId: ClientCategoryIds.NonProfitInstitutional),

        new(id: ClientTypeIds.CivicCulturalInstitution,
            name: "Civic / Cultural Institution",
            description: "Public library, museum, arts council",
            categoryId: ClientCategoryIds.NonProfitInstitutional),

        new(id: ClientTypeIds.FoundationCharitableTrust,
            name: "Foundation / Charitable Trust",
            description: "Community Foundation of Owensboro",
            categoryId: ClientCategoryIds.NonProfitInstitutional),

        // ---------------------------------------------------------------------
        // EDUCATION & HEALTH
        // ---------------------------------------------------------------------
        new(id: ClientTypeIds.PrivateSchoolAcademy,
            name: "Private School / Academy",
            description: "Christian Academy, Montessori",
            categoryId: ClientCategoryIds.EducationHealth),

        new(id: ClientTypeIds.UniversityCollegePrivate,
            name: "University / College (Private)",
            description: "Brescia University",
            categoryId: ClientCategoryIds.EducationHealth),

        new(id: ClientTypeIds.HospitalMedicalCenter,
            name: "Hospital / Medical Center",
            description: "Owensboro Health",
            categoryId: ClientCategoryIds.EducationHealth),

        new(id: ClientTypeIds.ClinicHealthcareProvider,
            name: "Clinic / Healthcare Provider",
            description: "Local medical or dental clinic",
            categoryId: ClientCategoryIds.EducationHealth),

        new(id: ClientTypeIds.SeniorLivingAssistedCare,
            name: "Senior Living / Assisted Care",
            description: "Charter Senior Living",
            categoryId: ClientCategoryIds.EducationHealth),

        // ---------------------------------------------------------------------
        // LAND / REAL ESTATE
        // ---------------------------------------------------------------------
        new(id: ClientTypeIds.FarmAgriculturalOperation,
            name: "Farm / Agricultural Operation",
            description: "Grain or livestock farm",
            categoryId: ClientCategoryIds.LandRealEstate),

        new(id: ClientTypeIds.LandholdingInvestmentCompany,
            name: "Landholding / Investment Company",
            description: "\"Green Acre Holdings, LLC\"",
            categoryId: ClientCategoryIds.LandRealEstate),

        new(id: ClientTypeIds.RealEstateInvestmentTrust,
            name: "Real Estate Investment Trust (REIT)",
            description: "Large property portfolio",
            categoryId: ClientCategoryIds.LandRealEstate),

        new(id: ClientTypeIds.PropertyDeveloperSubdivision,
            name: "Property Developer (Subdivision)",
            description: "\"Deer Valley Subdivision, LLC\"",
            categoryId: ClientCategoryIds.LandRealEstate),

        new(id: ClientTypeIds.PropertyManagementLeasingCompany,
            name: "Property Management / Leasing Co.",
            description: "\"Bluegrass Realty Mgmt.\"",
            categoryId: ClientCategoryIds.LandRealEstate),

        // ---------------------------------------------------------------------
        // UTILITY & INFRASTRUCTURE
        // ---------------------------------------------------------------------
        new(id: ClientTypeIds.WaterSewerDistrict,
            name: "Water / Sewer District",
            description: "Regional Water Resource Agency",
            categoryId: ClientCategoryIds.UtilityInfrastructure),

        new(id: ClientTypeIds.ElectricCooperative,
            name: "Electric Cooperative",
            description: "Warren RECC",
            categoryId: ClientCategoryIds.UtilityInfrastructure),

        new(id: ClientTypeIds.TelecommunicationsProvider,
            name: "Telecommunications Provider",
            description: "AT&T, Comcast",
            categoryId: ClientCategoryIds.UtilityInfrastructure),

        new(id: ClientTypeIds.TransportationAgency,
            name: "Transportation Agency",
            description: "KYTC, City Transit",
            categoryId: ClientCategoryIds.UtilityInfrastructure),

        new(id: ClientTypeIds.EnergyCompany,
            name: "Energy Company (Public or Private)",
            description: "Atmos, TVA, solar developer",
            categoryId: ClientCategoryIds.UtilityInfrastructure),

        // ---------------------------------------------------------------------
        // SPECIAL PURPOSE / TEMPORARY
        // ---------------------------------------------------------------------
        new(id: ClientTypeIds.JointVenture,
            name: "Joint Venture (JV)",
            description: "Two firms partnering on project",
            categoryId: ClientCategoryIds.SpecialPurposeTemporary),

        new(id: ClientTypeIds.ProjectLlcSpv,
            name: "Project LLC / SPV",
            description: "Single-project legal entity",
            categoryId: ClientCategoryIds.SpecialPurposeTemporary),

        new(id: ClientTypeIds.ReceiverCourtAppointee,
            name: "Receiver / Court Appointee",
            description: "Bankruptcy or litigation case",
            categoryId: ClientCategoryIds.SpecialPurposeTemporary),

        new(id: ClientTypeIds.BankruptcyTrustee,
            name: "Bankruptcy Trustee",
            description: "Manages distressed property",
            categoryId: ClientCategoryIds.SpecialPurposeTemporary),

        new(id: ClientTypeIds.EstateAdministrator,
            name: "Estate Administrator",
            description: "Executor handling property",
            categoryId: ClientCategoryIds.SpecialPurposeTemporary),

        // ---------------------------------------------------------------------
        // OTHER / MISCELLANEOUS
        // ---------------------------------------------------------------------
        new(id: ClientTypeIds.CivicClubFraternalOrg,
            name: "Civic Club / Fraternal Org",
            description: "Rotary, Kiwanis",
            categoryId: ClientCategoryIds.OtherMiscellaneous),

        new(id: ClientTypeIds.PoliticalOrganization,
            name: "Political Organization",
            description: "PAC, campaign committee",
            categoryId: ClientCategoryIds.OtherMiscellaneous),

        new(id: ClientTypeIds.UnknownFailedDbExport,
            name: "Unknown / Failed DB Export",
            description: "Failed BEi Access DB Export 2025",
            categoryId: ClientCategoryIds.OtherMiscellaneous),

        new(id: ClientTypeIds.UnknownToBeClassified,
            name: "Unknown / To Be Classified",
            description: "Placeholder for initial data import",
            categoryId: ClientCategoryIds.OtherMiscellaneous)
    ];
}