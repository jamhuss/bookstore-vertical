using BookStore.Models.Data;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "AllowReactClient";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("BookStore"));

builder.Services
   .AddFastEndpoints()
     .SwaggerDocument(o =>
     {
         o.AutoTagPathSegmentIndex = 2; // "books", "orders" instead of "api"
     });


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
   app.UseFastEndpoints()
    .UseSwaggerGen();
}
else
{
    // Skip HTTPS redirect in Development so the React dev-server proxy can talk
    // to the API over plain HTTP without hitting a 307 redirect.
    app.UseFastEndpoints();
    app.UseHttpsRedirection();
}


app.UseCors(CorsPolicy);


app.Run();
