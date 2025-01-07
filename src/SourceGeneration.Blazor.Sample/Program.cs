using SourceGeneration.Blazor;
using SourceGeneration.Blazor.Sample.States;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<MyState>();
builder.Services.AddBlazorState();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<SourceGeneration.Blazor.Sample.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
