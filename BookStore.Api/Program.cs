using BookStore.Models.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "AllowReactClient";

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("BookStore"));

// Register every IValidator<T> in the assembly. Each validator lives in its own
// feature slice file, so adding validation to a feature is a local change.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins(
                  "http://localhost:56789", // Vite dev server (this repo)
                  "http://localhost:5173",
                  "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Seed the in-memory store at startup so there is data to work with immediately.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.Seed(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
        options.WithTitle("BookStore API"));
}
else
{
    // Skip HTTPS redirect in Development so the React dev-server proxy can talk
    // to the API over plain HTTP without hitting a 307 redirect.
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
