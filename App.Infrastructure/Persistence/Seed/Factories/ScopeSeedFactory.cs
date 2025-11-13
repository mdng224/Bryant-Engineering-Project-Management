using App.Domain.Projects;
using App.Infrastructure.Persistence.Seed.Configurations;

namespace App.Infrastructure.Persistence.Seed.Factories;

internal static class ScopeSeedFactory
{
    public static IReadOnlyList<Scope> All =>
    [
        new(id: ScopeIds.Unknown,            name: "UNKNOWN",                  description: "Placeholder for missing/unclear scope"),
        new(id: ScopeIds.AlleyClosure,       name: "ALLEY CLOSURE",            description: null),
        new(id: ScopeIds.Alta,               name: "ALTA",                     description: null),
        new(id: ScopeIds.Annexation,         name: "ANNEXATION",               description: null),
        new(id: ScopeIds.AsBuilt,            name: "AS-BUILT",                 description: null),
        new(id: ScopeIds.BoundarySurvey,     name: "BOUNDARY SURVEY",          description: null),
        new(id: ScopeIds.BwgSiteDevPlan,     name: "BWG SITE DEV PLAN",        description: null),
        new(id: ScopeIds.ConcreteTesting,    name: "CONCRETE TESTING",         description: null),
        new(id: ScopeIds.ConstStaking,       name: "CONST STAKING",            description: null),
        new(id: ScopeIds.Cup,                name: "CUP",                      description: "Conditional Use Permit Application"),
        new(id: ScopeIds.DevPlan,            name: "DEVPLAN",                  description: null),
        new(id: ScopeIds.ElevCertificate,    name: "ELEV CERTIFICATE",         description: "Elevation Certificate"),
        new(id: ScopeIds.Fdp,                name: "FDP",                      description: "Final Development Plan"),
        new(id: ScopeIds.FinalPlat,          name: "FINAL PLAT",               description: null),
        new(id: ScopeIds.FindSetCorners,     name: "FIND/SET CORNERS",         description: null),
        new(id: ScopeIds.GradingModel,       name: "GRADING MODEL",            description: null),
        new(id: ScopeIds.LandfillSurvey,     name: "LANDFILL SURVEY",          description: null),
        new(id: ScopeIds.LandscapePlan,      name: "LANDSCAPE PLAN",           description: null),
        new(id: ScopeIds.Le,                 name: "LE",                       description: "Lateral Extension (Louisville MSD)"),
        new(id: ScopeIds.Loma,               name: "LOMA",                     description: "Letter of Map Amendment"),
        new(id: ScopeIds.MajorSubPlat,       name: "MAJOR SUB PLAT",           description: null),
        new(id: ScopeIds.MajorMinorSubPlat,  name: "MAJOR/MINOR SUB PLAT",     description: null),
        new(id: ScopeIds.MinorSubPlat,       name: "MINOR SUB PLAT",           description: null),
        new(id: ScopeIds.MortgageInspection, name: "MORTGAGE INSPECTION",      description: null),
        new(id: ScopeIds.PlotPlan,           name: "PLOT PLAN",                description: null),
        new(id: ScopeIds.PublicFacilityPlan, name: "PUBLIC FACILITY PLAN",     description: null),
        new(id: ScopeIds.Retracement,        name: "RETRACEMENT",              description: null),
        new(id: ScopeIds.Rezoning,           name: "REZONING",                 description: null),
        new(id: ScopeIds.Scp,                name: "SCP",                      description: "Stream Construction Permit Application"),
        new(id: ScopeIds.SitePlan,           name: "SITE PLAN",                description: null),
        new(id: ScopeIds.SoilTesting,        name: "SOIL TESTING",             description: null),
        new(id: ScopeIds.Topo,               name: "TOPO",                     description: null),
        new(id: ScopeIds.Variance,           name: "VARIANCE",                 description: null)
    ];
}
