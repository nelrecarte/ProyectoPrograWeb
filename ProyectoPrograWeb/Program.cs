using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ProyectoQ3Backend.Middlewares;
using ProyectoQ3Backend.Models;
using ProyectoQ3Backend.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var projectId = builder.Configuration["Firebase:ProjectId"]
                ?? throw new InvalidOperationException("Falta Firebase:ProjectId en appsettings.json");
var credentialsPath = builder.Configuration["Firebase:CredentialsPath"]
                      ?? throw new InvalidOperationException("Falta Firebase:CredentialsPath en appsettings.json");

FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile(credentialsPath),
    ProjectId = projectId
});

// ---------------------------------------------------------------------------
// Servicios
// ---------------------------------------------------------------------------

/**
 * Una sola instancia para toda la vida de ejecucion de la app
 */
builder.Services.AddSingleton<FirebaseService>();

builder.Services.AddHttpClient<FirebaseAuthClient>();

// Autenticacion y usuarios
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();

// Dominio de ApagonYa
builder.Services.AddScoped<ZoneService>();
builder.Services.AddScoped<TechnicianService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<ResolutionService>();
builder.Services.AddScoped<StatisticsService>();
builder.Services.AddScoped<SeedService>();

// Actividad semanal de clase (UserHub), no forma parte de ApagonYa
builder.Services.AddScoped<NoteService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

// ---------------------------------------------------------------------------
// Autenticacion: los ID token los emite Firebase, aqui solo se validan
// ---------------------------------------------------------------------------

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://securetoken.google.com/{projectId}";

        // Sin el mapeo automatico los claims llegan con el nombre que Firebase les da
        // ("role", "user_id", "email"), que es mucho mas facil de razonar.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{projectId}",
            ValidateAudience = true,
            ValidAudience = projectId,
            ValidateLifetime = true,

            // El rol viaja como custom claim "role" de Firebase. Al declararlo aca,
            // [Authorize(Roles = ...)] funciona directo.
            RoleClaimType = "role",
            NameClaimType = "user_id"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloAdministrador", policy => policy.RequireRole(Roles.Administrador));
    options.AddPolicy("SoloTecnico", policy => policy.RequireRole(Roles.Tecnico));
    options.AddPolicy("SoloCiudadano", policy => policy.RequireRole(Roles.Ciudadano));
    options.AddPolicy("GestionDeReportes", policy => policy.RequireRole(Roles.Administrador, Roles.Tecnico));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// Pipeline
// ---------------------------------------------------------------------------

// Va de primero para poder atrapar lo que truene mas adelante.
app.UseErrorHandling();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("ApagonYa API")
            .WithPreferredScheme("Bearer")
            .WithHttpBearerAuthentication(bearer =>
            {
                bearer.Token = "";
            });
    });
}

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
