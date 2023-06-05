using DataStorageService.Api.Middlewares;
using DataStorageService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddServices();

var app = builder.Build();

// Apply ExceptionHandleMiddleware middleware to handle exceptions.
app.UseMiddleware<ExceptionHandleMiddleware>();

// Configure the HTTP request pipeline.
// Depending on the environment (Development or non-Development),
// Swagger and Swagger UI are included for API documentation.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply HTTPS redirection.
app.UseHttpsRedirection();

// Apply authorization.
app.UseAuthorization();

// Apply routing controllers.
app.MapControllers();

// Start processing HTTP requests.
app.Run();
