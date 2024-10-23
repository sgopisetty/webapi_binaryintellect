using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AppDb");

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o=> o.SwaggerDoc("v1", new OpenApiInfo { Title = "Minimal API", Version = "v1" }));


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.MapGet("/minimalapi/employees",
(AppDbContext db) =>
{
    return Results.Ok(db.Employees.ToList());
});

app.MapGet("/minimalapi/employees/{id}",
(AppDbContext db, int id) =>
{
    return Results.Ok(db.Employees.Find(id));
});

app.MapPost("/minimalapi/employees",
(AppDbContext db, Employee emp) =>
{
    db.Employees.Add(emp);
    db.SaveChanges();
    return Results.Created($"/minimalapi/employees/{emp.EmployeeID}", emp);
});

app.MapPut("/minimalapi/employees/{id}",
(AppDbContext db, int id, Employee emp) =>
{
    db.Employees.Update(emp);
    db.SaveChanges();
    return Results.NoContent();
});

app.MapDelete("/minimalapi/employees/{id}",
(AppDbContext db, int id) =>
{
    var emp = db.Employees.Find(id);
    db.Remove(emp);
    db.SaveChanges();
    return Results.NoContent();
});

app.Run();