using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registra serviço do AppDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TasksDB")
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/tasks", async (AppDbContext db) => await db.Tasks.ToListAsync());

app.MapGet("/tasks/{id}", async (int id, AppDbContext db) =>
    await db.Tasks.FindAsync(id) is Task task 
        ? Results.Ok(task) 
        : Results.NotFound()
);

app.MapGet("/tasks/finished", async (AppDbContext db) => 
    await db.Tasks.Where(x => x.IsFinished).ToListAsync()
);

app.MapPost("/tasks", async (Task task, AppDbContext db) =>
{
    db.Tasks.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
});

app.MapPost("/tasks/add-multiple", async (List<Task> tasks, AppDbContext db) =>
{
    foreach (var task in tasks)
    {
        db.Tasks.Add(task);
    }
    await db.SaveChangesAsync();
    return Results.Created($"/tasks", tasks);
});

app.MapPut("/tasks/{id}", async (int id, Task updatedTask, AppDbContext db) => 
{ 
    var task = await db.Tasks.FindAsync(id);

    if (task is null)
        return Results.NotFound();

    task.Name = updatedTask.Name;
    task.IsFinished = updatedTask.IsFinished;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/tasks/{id}", async(int id, AppDbContext db) =>
{
    var taskToDelete = await db.Tasks.FindAsync(id);

    if (taskToDelete is not Task task)
        return Results.NotFound();

    db.Tasks.Remove(taskToDelete);
    await db.SaveChangesAsync();
    return Results.Ok(taskToDelete);
});

app.Run();

class Task
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsFinished { get; set; }
}

// Mapeia a entidade Task
class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
            
    }

    public DbSet<Task> Tasks => Set<Task>();
}
