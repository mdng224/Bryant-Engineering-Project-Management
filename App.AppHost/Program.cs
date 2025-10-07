var builder = DistributedApplication.CreateBuilder(args);

// Postgres container
var pgVersion = builder.Configuration["Postgres:Version"] ?? "16";
//var pgUser = builder.Configuration["Postgres:User"] ?? Environment.GetEnvironmentVariable("POSTGRES_USER");
//var pgPassword = builder.Configuration["Postgres:Password"] ?? Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

var postgres = builder.AddPostgres("postgres")
    .WithImageTag(pgVersion)
    .WithDataVolume("pgdata")
    .WithHostPort(55432);

var appDb = postgres.AddDatabase("appdb");

// API
builder.AddProject<Projects.App_Api>("App")
       .WithReference(appDb)
       .WaitFor(appDb)
       .WithHttpEndpoint(port: 5000, name: "api");

// --- Vue dev server (Vite) ---
builder.AddExecutable(
        name: "frontend",
        command: "npm",
        workingDirectory: "../frontend",
        args: ["run", "dev"]
    )
    .WithHttpEndpoint(port: 5173, name: "frontend", isProxied: false);

builder.Build().Run();
