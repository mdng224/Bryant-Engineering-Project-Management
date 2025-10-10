using App.Api.Extensions;
using App.Application;
using App.Infrastructure;
using App.Infrastructure.Data;
using App.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddApi(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
app.MigrateDatabase<AppDbContext>();
app.UseApi();

app.Run();
