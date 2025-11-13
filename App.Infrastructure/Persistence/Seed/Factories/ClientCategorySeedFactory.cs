using App.Domain.Clients;
using App.Infrastructure.Persistence.Seed.Configurations;

namespace App.Infrastructure.Persistence.Seed.Factories;

internal static class ClientCategorySeedFactory
{
    public static IEnumerable<ClientCategory> All =>
    [
        new(id: ClientCategoryIds.IndividualPrivate,         name: "Individual/Private"),
        new(id: ClientCategoryIds.PrivateSectorOrganization, name: "Private-Sector Organization"),
        new(id: ClientCategoryIds.GovernmentPublicAgency,    name: "Government / Public Agency"),
        new(id: ClientCategoryIds.NonProfitInstitutional,    name: "Non-Profit / Institutional"),
        new(id: ClientCategoryIds.EducationHealth,           name: "Education & Health"),
        new(id: ClientCategoryIds.LandRealEstate,            name: "Land / Real Estate"),
        new(id: ClientCategoryIds.UtilityInfrastructure,     name: "Utility & Infrastructure"),
        new(id: ClientCategoryIds.FinancialLegalEntity,      name: "Financial / Legal Entity"),
        new(id: ClientCategoryIds.SpecialPurposeTemporary,   name: "Special Purpose / Temporary"),
        new(id: ClientCategoryIds.OtherMiscellaneous,        name: "Other / Miscellaneous")
    ];
}