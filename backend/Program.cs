using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Configuración de Base de Datos (Cambiar ruta por sql server local de su PC)
builder.Services.AddDbContext<TareasDbContext>(opt =>
{
    opt.UseSqlServer("Server=DESKTOP-V7G3G1I\\SQLEXPRESS;Database=TareasDB;Trusted_Connection=True;TrustServerCertificate=True;");
});

// 2. SERVICIO DE CORS
builder.Services.AddCors();

var app = builder.Build();

// ACTIVAR SWAGGER
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tareas API v1");
    c.RoutePrefix = "swagger"; // ← esto asegura que funcione en /swagger
});

// 3. HABILITAR CORS
app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader());

app.UseHttpsRedirection();

// --- ENDPOINTS DEL CRUD ---

// 1. GET - Obtener todas las tareas
app.MapGet("/api/tareas", async (bool? completada, TareasDbContext db) =>
{
    var query = db.Tareas.AsQueryable();

    if (completada.HasValue)
    {
        query = query.Where(t => t.Completed == completada.Value);
    }

    var resultado = await query.ToListAsync();
    return Results.Ok(resultado);
});

// 2. GET - Obtener una tarea por ID
app.MapGet("/api/tareas/{id}", async (int id, TareasDbContext db) =>
{
    var tarea = await db.Tareas.FindAsync(id);
    return tarea is not null
        ? Results.Ok(tarea)
        : Results.NotFound(new { mensaje = "Tarea no encontrada." });
});

// 3. POST - Crear tarea
app.MapPost("/api/tareas", async ([FromBody] Todo nuevaTarea, TareasDbContext db) =>
{
    db.Tareas.Add(nuevaTarea);
    await db.SaveChangesAsync();
    return Results.Created($"/api/tareas/{nuevaTarea.Id}", nuevaTarea);
});

// 4. PUT - Actualizar tarea
app.MapPut("/api/tareas/{id}", async (int id, [FromBody] Todo tareaActualizada, TareasDbContext db) =>
{
    var tareaOriginal = await db.Tareas.FindAsync(id);

    if (tareaOriginal is null)
        return Results.NotFound(new { mensaje = "No se puede actualizar, tarea no encontrada." });

    tareaOriginal.Title = tareaActualizada.Title;
    tareaOriginal.Due = tareaActualizada.Due;
    tareaOriginal.Time = tareaActualizada.Time;
    tareaOriginal.Category = tareaActualizada.Category;
    tareaOriginal.Completed = tareaActualizada.Completed;

    await db.SaveChangesAsync();
    return Results.Ok(tareaOriginal);
});

// 5. DELETE - Eliminar tarea
app.MapDelete("/api/tareas/{id}", async (int id, TareasDbContext db) =>
{
    var tarea = await db.Tareas.FindAsync(id);

    if (tarea is null)
        return Results.NotFound(new { mensaje = "No se puede borrar, tarea no encontrada." });

    db.Tareas.Remove(tarea);
    await db.SaveChangesAsync();

    return Results.Ok(new { mensaje = $"Tarea {id} eliminada correctamente." });
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TareasDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

// --- MODELO ---
public class Todo
{
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("due")]
    public string Due { get; set; } = string.Empty;

    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("completed")]
    public bool Completed { get; set; }
}

// --- DB CONTEXT ---
public class TareasDbContext : DbContext
{
    public TareasDbContext(DbContextOptions<TareasDbContext> options) : base(options) { }

    public DbSet<Todo> Tareas => Set<Todo>();
}