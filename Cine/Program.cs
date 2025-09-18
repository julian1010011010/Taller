using Cine;
using Cine.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Vincular configuración OpenAI desde appsettings
builder.Services.Configure<OpenAiSettings>(builder.Configuration.GetSection("OpenAi"));

// HttpClients
builder.Services.AddHttpClient<CallApiMovies>();

// OpenAI como proveedor IA
builder.Services.AddHttpClient<OpenAiMovieGuesser>();
builder.Services.AddScoped<IAiMovieGuesser, OpenAiMovieGuesser>();

builder.Services.AddScoped<IMovieIdentifier, MovieIdentifier>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
