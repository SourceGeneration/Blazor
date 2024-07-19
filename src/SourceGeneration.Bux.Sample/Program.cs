using SourceGeneration.Blazor;
using SourceGeneration.Blux.Sample.States;
using SourceGeneration.ChangeTracking;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddScoped(sp => ChangeTrackingProxyFactory.Create(new MyState()));
builder.Services.AddBlazorStatily();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<SourceGeneration.Blux.Sample.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
