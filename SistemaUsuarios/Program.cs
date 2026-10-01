using Microsoft.EntityFrameworkCore;
using SistemaUsuarios.Components;
using SistemaUsuarios.Data;
using SistemaUsuarios.Repositories;
using SistemaUsuarios.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Cadena de conexión SQLite: se normaliza para que el archivo usuarios.db se
// cree siempre en la raíz de contenido de la aplicación.
// ---------------------------------------------------------------------------
var cadenaConexion = ConfiguracionBaseDatos.NormalizarCadenaConexion(
    builder.Configuration.GetConnectionString(ConfiguracionBaseDatos.NombreCadenaConexion),
    builder.Environment.ContentRootPath);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Entity Framework Core: fábrica de contextos (patrón recomendado en Blazor Server).
builder.Services.AddDbContextFactory<ApplicationDbContext>(opciones =>
    opciones.UseSqlite(cadenaConexion));

// Capa de acceso a datos y capa de servicios (inyección de dependencias).
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ITipoUsuarioRepository, TipoUsuarioRepository>();
builder.Services.AddScoped<ITipoUsuarioService, TipoUsuarioService>();

var app = builder.Build();

// Aplica las migraciones de Entity Framework Core y crea usuarios.db si no existe.
await DbInicializador.InicializarAsync(app.Services, app.Logger);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    // La redirección a HTTPS solo se habilita fuera de desarrollo para evitar el
    // aviso "Failed to determine the https port for redirect" cuando la aplicación
    // se ejecuta localmente sobre HTTP.
    app.UseHttpsRedirection();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

