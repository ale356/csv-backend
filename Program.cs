using csvBackEnd.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env.
DotNetEnv.Env.Load();

// Add services to the container.
builder.Services.AddControllers();

// Retrieve the connection string from .env or appsettings.json.
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Register DbContext with PostgreSQL configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure CORS to allow requests from your frontend application.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNetlify",
        builder =>
        {
            builder.WithOrigins("https://ale356-csv-frontend.netlify.app")
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Configure Swagger for API documentation if needed.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS.
app.UseCors("AllowNetlify");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
