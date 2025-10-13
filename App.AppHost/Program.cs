var builder = DistributedApplication.CreateBuilder(args);

// Postgres
var pgVersion = builder.Configuration["Postgres:Version"] ?? "16";
var pgPassword = builder.AddParameter("postgres-password", secret: true);
var postgres = builder.AddPostgres("postgres", password: pgPassword)
    .WithImageTag(pgVersion)
    .WithDataVolume("pgdata")
    .WithHostPort(55432);

var appDb = postgres.AddDatabase("appdb");

// API
builder.AddProject<Projects.App_Api>("App")
       .WithReference(appDb)
       .WaitFor(appDb);

// Frontend
builder.AddExecutable(
        name: "frontend",
        command: "npm",
        workingDirectory: "../frontend",
        args: ["run", "dev"]
    )
    .WithHttpEndpoint(port: 5173, name: "frontend", isProxied: false);

builder.Build().Run();
