using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Backend.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
}));

builder.Services.AddSingleton<Backend.Services.MongoDbService>();
builder.Services.AddSingleton<Backend.Services.CounterService>();
builder.Services.AddScoped<IProductRepository, Backend.Repositories.MongoProductRepository>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("MyPolicy");


// Serve static files from wwwroot (so /admin/index.html is available)
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Simple route to serve admin UI
app.MapGet("/admin", context =>
{
    context.Response.Redirect("/admin/index.html");
    return Task.CompletedTask;
});

// No EF Core migrations; persistencia via MongoDB

app.Run();
