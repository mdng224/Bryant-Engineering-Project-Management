using App.Domain.Employees;
using static App.Infrastructure.Persistence.Seed.Configurations.PositionIds;

namespace App.Infrastructure.Persistence.Seed;

internal static class PositionSeedFactory
{
    public static IEnumerable<Position> All =>
    [
        Position.CreateSeed(id: PrincipalInCharge,        name: "Principal-In-Charge",        code: "PIC",           requiresLicense: true),
        Position.CreateSeed(id: ProjectEngineer,          name: "Project Engineer",           code: "PE",            requiresLicense: true),
        Position.CreateSeed(id: ProfessionalLandSurveyor, name: "Professional Land Surveyor", code: "PLS",           requiresLicense: true),
        Position.CreateSeed(id: LandSurveyorInTraining,   name: "Land Surveyor In Training",  code: "LSIT",          requiresLicense: true),
        Position.CreateSeed(id: SurveyPartyChief,         name: "Survey Party Chief",         code: "SPC",           requiresLicense: false),
        Position.CreateSeed(id: EngineeringIntern,        name: "Engineering Intern",         code: "Eng Intern",    requiresLicense: false),
        Position.CreateSeed(id: Rodman,                   name: "Rodman",                     code: "Rodman",        requiresLicense: false),
        Position.CreateSeed(id: OfficeManager,            name: "Office Manager",             code: "OfficeMgr",     requiresLicense: false),
        Position.CreateSeed(id: DraftingTechnician,       name: "Drafting Technician",        code: "Draft Tech",    requiresLicense: false),
        Position.CreateSeed(id: EngineeringTechnician,    name: "Engineering Technician",     code: "Eng Tech",      requiresLicense: false),
        Position.CreateSeed(id: SeniorDraftingTechnician, name: "Senior Drafting Technician", code: "Sr Draft Tech", requiresLicense: false),
        Position.CreateSeed(id: EngineerInTraining,       name: "Engineer-In-Training",       code: "EIT",           requiresLicense: false),
        Position.CreateSeed(id: RemotePilotInCommand,     name: "Remote Pilot In Command",    code: "Remote PIC",    requiresLicense: false)
    ];
}