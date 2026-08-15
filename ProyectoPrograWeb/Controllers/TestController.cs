using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;

namespace ProyectoPrograWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly FirestoreDb _db;
    public TestController(FirestoreDb db) => _db = db;

    [HttpGet("firebase-check/{integrante}")]
    public async Task<IActionResult> Check(string integrante, [FromQuery] string? nombre)
    {
        var doc = _db.Collection("test").Document(integrante);
        await doc.SetAsync(new Dictionary<string, object>
        {
            { "status", "ok" },
            { "integrante", nombre ?? integrante },
            { "maquina", Environment.MachineName },
            { "timestamp", Timestamp.GetCurrentTimestamp() }
        });

        var snap = await doc.GetSnapshotAsync();
        return Ok(snap.ToDictionary());
    }
}