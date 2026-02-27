using VisioCall.Server.Hubs;
using VisioCall.Server.Services;
using VisioCall.Shared.Constants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

builder.Services.AddSingleton<UserTrackingService>();

// Allow connections from any origin for local dev
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

app.UseCors();
app.MapHub<CallHub>(HubRoutes.CallHub);

app.MapGet("/", () => "VisioCall SignalR Server is running.");

app.Run();
