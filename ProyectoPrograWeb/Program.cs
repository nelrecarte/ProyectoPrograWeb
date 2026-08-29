using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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

/**
 * Una sola instancia para toda la vida de ejecucion de la app
 */
builder.Services.AddSingleton<FirebaseService>();

builder.Services.AddHttpClient<FirebaseAuthClient>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<NoteService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://securetoken.google.com/{projectId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{projectId}",
            ValidateAudience = true,
            ValidAudience = projectId,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("UserHub API")
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
