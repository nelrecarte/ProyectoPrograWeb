using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

// ---- Firebase ----
var credPath = builder.Configuration["Firebase:CredentialsPath"];
var projectId = builder.Configuration["Firebase:ProjectId"];

FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile(credPath),
    ProjectId = projectId
});

var firestoreDb = new FirestoreDbBuilder
{
    ProjectId = projectId,
    CredentialsPath = credPath
}.Build();

builder.Services.AddSingleton(firestoreDb);
// ------------------

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();