using CV_Ranking.DAL;
using CV_Ranking.Services;
using CVMatching.Infrastructure.Data;
using CVMatching.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerGen;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new()
	{
		Title = "CV Matching API",
		Version = "v1",
		Description = "API para matching de CVs com vagas usando ML.NET"
	});
});

// Database - usando SQLite para MVP (troque para PostgreSQL em produção)
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlite(
		builder.Configuration.GetConnectionString("DefaultConnection")
		?? "Data Source=cvmatching.db"
	)
);

// Repositories
builder.Services.AddScoped<IVagaRepository, VagaRepository>();
builder.Services.AddScoped<ICandidatoRepository, CandidateRepository>();
builder.Services.AddScoped<IMatchingResultRepository, ResultadoMatchingRepository>();

// Services
builder.Services.AddSingleton<IPdfTextExtractor, PdfTextExtractor>();
builder.Services.AddSingleton<INormalizarTexto, Normalizartexto>();
builder.Services.AddSingleton<IMatchingEngine, MatchingEngine>();

// CORS (opcional, para testes frontend)
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});

var app = builder.Build();

// Criar banco de dados automaticamente
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "CV Matching API v1");
		c.RoutePrefix = string.Empty; // Swagger na raiz
	});
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();