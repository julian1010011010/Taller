using Cine;
using Cine.Services;

var builder = WebApplication.CreateBuilder(args);

// Registrar servicios antes de Build()
builder.Services.AddControllers();

// HttpClients
builder.Services.AddHttpClient<CallApiMovies>();
builder.Services.AddHttpClient<OpenAiMovieGuesser>();

// Servicios de IA y orquestador
builder.Services.AddScoped<IAiMovieGuesser, OpenAiMovieGuesser>();
builder.Services.AddScoped<IMovieIdentifier, MovieIdentifier>();

var app = builder.Build();

// Middlewares
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
