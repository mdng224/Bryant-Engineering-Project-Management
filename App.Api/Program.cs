using App.Api.Extensions;
using App.Application;
using App.Infrastructure;
using App.Infrastructure.Persistence;
using App.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddApi(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
await app.MigrateDatabaseAsync<AppDbContext>();
app.UseApi();
await app.RunAsync();