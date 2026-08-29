using System.Net.Http.Json;
using System.Text.Json;

namespace ProyectoQ3Backend.Services;

public class FirebaseAuthClient
{
    private const string BaseUrl = "https://identitytoolkit.googleapis.com/v1/accounts";

    private readonly HttpClient _http;
    private readonly string _apiKey;

    public FirebaseAuthClient(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _apiKey = configuration["Firebase:ApiKey"]
                  ?? throw new InvalidOperationException(
                      "Falta Firebase:ApiKey en appsettings.json. Es la Web API Key de la consola de Firebase.");
    }

    public Task<FirebaseAuthResult> SignUpAsync(string email, string password) =>
        SendAsync("signUp", email, password);

    public Task<FirebaseAuthResult> SignInAsync(string email, string password) =>
        SendAsync("signInWithPassword", email, password);

    private async Task<FirebaseAuthResult> SendAsync(string action, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new FirebaseAuthException(
                "Firebase:ApiKey esta vacia. Pedirle a Nelson la Web API Key de la consola.");

        var response = await _http.PostAsJsonAsync(
            $"{BaseUrl}:{action}?key={_apiKey}",
            new { email, password, returnSecureToken = true });

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!response.IsSuccessStatusCode)
            throw new FirebaseAuthException(TranslateError(body));

        return new FirebaseAuthResult
        {
            IdToken = body.GetProperty("idToken").GetString()!,
            LocalId = body.GetProperty("localId").GetString()!,
            Email = body.GetProperty("email").GetString()!
        };
    }

    private static string TranslateError(JsonElement body)
    {
        var code = body.TryGetProperty("error", out var error)
                   && error.TryGetProperty("message", out var message)
            ? message.GetString() ?? string.Empty
            : string.Empty;

        var key = code.Split(':')[0].Trim();

        return key switch
        {
            "EMAIL_EXISTS" => "Ya existe una cuenta con ese correo",
            "INVALID_EMAIL" => "El correo no tiene un formato valido",
            "WEAK_PASSWORD" => "La contrasena debe tener al menos 6 caracteres",
            "EMAIL_NOT_FOUND" or "INVALID_PASSWORD" or "INVALID_LOGIN_CREDENTIALS"
                => "Correo o contrasena incorrectos",
            "USER_DISABLED" => "La cuenta esta deshabilitada",
            "TOO_MANY_ATTEMPTS_TRY_LATER" => "Demasiados intentos, intentar mas tarde",
            "OPERATION_NOT_ALLOWED"
                => "El proveedor Email/Password no esta activado en la consola de Firebase",
            "" => "Firebase rechazo la peticion sin dar un motivo",
            _ => $"Error de Firebase: {key}"
        };
    }
}

public class FirebaseAuthResult
{
    public string IdToken { get; set; } = string.Empty;
    public string LocalId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class FirebaseAuthException : Exception
{
    public FirebaseAuthException(string message) : base(message) { }
}
