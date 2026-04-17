using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// 1. Configuración de Base de Datos
builder.Services.AddDbContext<TareasDbContext>(opt =>
    opt.UseSqlite("Data Source=tareas.db"));

// 2. AGREGAR SERVICIO DE CORS (Indispensable para Angular)
builder.Services.AddCors();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 3. HABILITAR CORS (Debe ir antes de los endpoints)
app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader());

app.UseHttpsRedirection();

// --- ENDPOINTS DEL CRUD ---

// 1. GET - Obtener todas las tareas (Con filtros para los botones del Front)
app.MapGet("/api/tareas", async (bool? completada, TareasDbContext db) =>
{
    var query = db.Tareas.AsQueryable();

    if (completada.HasValue)
    {
        query = query.Where(t => t.Completed == completada.Value);
    }

    var resultado = await query.ToListAsync();

    // Devolvemos la lista (aunque esté vacía) para que Angular no de error
    return Results.Ok(resultado);
});

// 2. GET - Obtener una sola tarea por ID
app.MapGet("/api/tareas/{id}", async (int id, TareasDbContext db) =>
{
    var tarea = await db.Tareas.FindAsync(id);
    return tarea is not null ? Results.Ok(tarea) : Results.NotFound(new { mensaje = "Tarea no encontrada." });
});

// 3. POST - Crear una nueva tarea
app.MapPost("/api/tareas", async ([FromBody] Todo nuevaTarea, TareasDbContext db) =>
{
    db.Tareas.Add(nuevaTarea);
    await db.SaveChangesAsync();
    return Results.Created($"/api/tareas/{nuevaTarea.Id}", nuevaTarea);
});

// 4. PUT - Actualizar una tarea (marcar como completada o cambiar texto)
app.MapPut("/api/tareas/{id}", async (int id, [FromBody] Todo tareaActualizada, TareasDbContext db) =>
{
    var tareaOriginal = await db.Tareas.FindAsync(id);
    if (tareaOriginal is null) return Results.NotFound(new { mensaje = "No se puede actualizar, tarea no encontrada." });

    tareaOriginal.TaskDescription = tareaActualizada.TaskDescription;
    tareaOriginal.Completed = tareaActualizada.Completed;

    await db.SaveChangesAsync();
    return Results.Ok(tareaOriginal);
});

// 5. DELETE - Borrar una tarea
app.MapDelete("/api/tareas/{id}", async (int id, TareasDbContext db) =>
{
    var tarea = await db.Tareas.FindAsync(id);
    if (tarea is null) return Results.NotFound(new { mensaje = "No se puede borrar, tarea no encontrada." });

    db.Tareas.Remove(tarea);
    await db.SaveChangesAsync();
    return Results.Ok(new { mensaje = $"Tarea {id} eliminada correctamente." });
});

app.Run();

// --- MODELO ---
public class Todo
{
    public int Id { get; set; }
    [JsonPropertyName("todo")]
    public string TaskDescription { get; set; } = string.Empty;
    [JsonPropertyName("completed")]
    public bool Completed { get; set; }
}

public class TareasDbContext : DbContext
{
    public TareasDbContext(DbContextOptions<TareasDbContext> options) : base(options) { }
    public DbSet<Todo> Tareas => Set<Todo>();
}