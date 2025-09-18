using Cine;

var builder = WebApplication.CreateBuilder(args);

// Registrar servicios antes de Build()
builder.Services.AddControllers();
builder.Services.AddHttpClient<CallApiMovies>();

var app = builder.Build();

// Middlewares
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
