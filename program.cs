using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDb>(o =>
    o.UseSqlite("Data Source=todo.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
    db.Database.EnsureCreated();
}

// ===== CRUD =====

app.MapGet("/api/todo", async (TodoDb db) =>
    await db.Todos.ToListAsync());

app.MapGet("/api/todo/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id) is ToDoItem t ? Results.Ok(t) : Results.NotFound());

app.MapPost("/api/todo", async (ToDoItem item, TodoDb db) =>
{
    db.Todos.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/api/todo/{item.Id}", item);
});

app.MapPut("/api/todo/{id}", async (int id, ToDoItem input, TodoDb db) =>
{
    var item = await db.Todos.FindAsync(id);
    if (item == null) return Results.NotFound();

    item.Title = input.Title;
    item.Description = input.Description;
    item.Tags = input.Tags;

    await db.SaveChangesAsync();
    return Results.Ok(item);
});

app.MapDelete("/api/todo/{id}", async (int id, TodoDb db) =>
{
    var item = await db.Todos.FindAsync(id);
    if (item == null) return Results.NotFound();

    db.Todos.Remove(item);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();
