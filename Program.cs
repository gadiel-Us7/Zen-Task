using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// --- BASE DE DATOS (En memoria para pruebas) --
List<Todo> tareasDb = new()
{
    new Todo { Id = 1, TaskDescription = "Aprender Minimal APIs en .NET 10", Completed = false },
    new Todo { Id = 2, TaskDescription = "Configurar el CRUD en Postman", Completed = true }
};

// --- ENDPOINTS DEL CRUD ---

// 1. GET - Obtener todas las tareas
app.MapGet("/api/tareas", ([FromQuery] bool? completada) =>
{
    var resultado = completada.HasValue
        ? tareasDb.Where(t => t.Completed == completada.Value).ToList()
        : tareasDb;

    return resultado.Any()
        ? Results.Ok(resultado)
        : Results.Ok(new { mensaje = "No hay tareas en la lista." });
});

// 2. GET - Obtener una sola tarea por ID
app.MapGet("/api/tareas/{id}", (int id) =>
{
    var tarea = tareasDb.FirstOrDefault(t => t.Id == id);
    return tarea is not null ? Results.Ok(tarea) : Results.NotFound(new { mensaje = "Tarea no encontrada." });
});

// 3. POST - Crear una nueva tarea
app.MapPost("/api/tareas", ([FromBody] Todo nuevaTarea) =>
{
    // ID automáticamente
    nuevaTarea.Id = tareasDb.Count > 0 ? tareasDb.Max(t => t.Id) + 1 : 1;
    tareasDb.Add(nuevaTarea);

    return Results.Created($"/api/tareas/{nuevaTarea.Id}", nuevaTarea);
});

// 4. PUT - Actualizar una tarea (marcar como completada o cambiar texto)
app.MapPut("/api/tareas/{id}", (int id, [FromBody] Todo tareaActualizada) =>
{
    var tareaOriginal = tareasDb.FirstOrDefault(t => t.Id == id);
    if (tareaOriginal is null) return Results.NotFound(new { mensaje = "No se puede actualizar, tarea no encontrada." });

    tareaOriginal.TaskDescription = tareaActualizada.TaskDescription;
    tareaOriginal.Completed = tareaActualizada.Completed;

    return Results.Ok(tareaOriginal);
});

// 5. DELETE - Borrar una tarea
app.MapDelete("/api/tareas/{id}", (int id) =>
{
    var tarea = tareasDb.FirstOrDefault(t => t.Id == id);
    if (tarea is null) return Results.NotFound(new { mensaje = "No se puede borrar, tarea no encontrada." });

    tareasDb.Remove(tarea);
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