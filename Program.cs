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

// Database - PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(
		builder.Configuration.GetConnectionString("PostgreSQL")
	)
);

//CONFIGURA??O CORS PARA FRONTEND SEPARADO
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy.WithOrigins(
			"http://localhost:3000",      // Se usar Live Server padr?o
			"http://localhost:5500",      // VS Code Live Server
			"http://127.0.0.1:3000",
			"http://127.0.0.1:5500",
			"http://localhost:8080",      // Outras portas comuns
			"file://"                      // Abrir HTML direto no navegador
		)
		.AllowAnyMethod()
		.AllowAnyHeader()
		.AllowCredentials();
	});

	// Permitir qualquer origem
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});

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

// Criar pasta para CVs em wwwroot/cvs
var cvFolder = app.Configuration["CvStorage:Folder"] ?? "wwwroot/cvs";
var cvFullPath = Path.Combine(app.Environment.ContentRootPath, cvFolder);
Directory.CreateDirectory(cvFullPath);

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

// Habilitar arquivos est?ticos
app.UseStaticFiles();

// Rota padr?o para index.html
app.MapGet("/", () => Results.Redirect("/index.html"));

app.Run();