using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using webapi_binaryintellect.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AppDb");

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));

var securityScheme = new OpenApiSecurityScheme()
{
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JSON Web Token based security"
};

var securityReq = new OpenApiSecurityRequirement()
{
    {
        new OpenApiSecurityScheme { Reference = new OpenApiReference{ Type=ReferenceType.SecurityScheme, Id="Bearer" }},
        new string[] {}
    }
};

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "Minimal API", Version = "v1" });
    o.AddSecurityDefinition("Bearer", securityScheme);
    o.AddSecurityRequirement(securityReq);
});



builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))

    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.MapGet("/minimalapi/employees",
[Authorize] (AppDbContext db) =>
{
    return Results.Ok(db.Employees.ToList());
});

app.MapGet("/minimalapi/employees/{id}",
[Authorize] (AppDbContext db, int id) =>
{
    return Results.Ok(db.Employees.Find(id));
});

app.MapPost("/minimalapi/employees",
[Authorize] (AppDbContext db, Employee emp) =>
{
    db.Employees.Add(emp);
    db.SaveChanges();
    return Results.Created($"/minimalapi/employees/{emp.EmployeeID}", emp);
});

app.MapPut("/minimalapi/employees/{id}",
[Authorize] (AppDbContext db, int id, Employee emp) =>
{
    db.Employees.Update(emp);
    db.SaveChanges();
    return Results.NoContent();
});

app.MapDelete("/minimalapi/employees/{id}",
[Authorize] (AppDbContext db, int id) =>
{
    var emp = db.Employees.Find(id);
    db.Remove(emp);
    db.SaveChanges();
    return Results.NoContent();
});

app.MapPost("/minimalapi/security/getToken",
[AllowAnonymous] (AppUser user) =>
{
    if (user.UserName == "sekhar" && user.Password == "abcdef")
    {
        //issue token 
        var issuer = builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["Jwt:Audience"];
        var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

        //token generation
        var token = new JwtSecurityToken(issuer: issuer, audience: audience, signingCredentials: credentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        var stringToken = tokenHandler.WriteToken(token);

        return Results.Ok(stringToken);
    }
    return Results.Unauthorized();
});

app.Run();