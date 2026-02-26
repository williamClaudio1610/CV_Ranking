using CV_Ranking.DAL;
using CV_Ranking.Models;
using Microsoft.AspNetCore.Mvc;

namespace CV_Ranking.Controllers
{
	[ApiController]
	[Route("api/vagas/{vagaid}/candidatos")]
	public class CandidatosController : ControllerBase
	{
		private readonly ICandidatoRepository _candidateRepo;
		private readonly IVagaRepository _jobRepo;
		private readonly IPdfTextExtractor _pdfExtractor;
		private readonly INormalizarTexto _normalizer;
		private readonly IWebHostEnvironment _env;
		private readonly IConfiguration _config;

		public CandidatosController(
			ICandidatoRepository candidateRepo,
			IVagaRepository jobRepo,
			IPdfTextExtractor pdfExtractor,
			INormalizarTexto normalizer,
			IWebHostEnvironment env,
			IConfiguration config)
		{
			_candidateRepo = candidateRepo;
			_jobRepo = jobRepo;
			_pdfExtractor = pdfExtractor;
			_normalizer = normalizer;
			_env = env;
			_config = config;
		}

		[HttpPost]
		public async Task<IActionResult> UploadCV(int vagaid, IFormFile file)
		{
			var job = await _jobRepo.GetByIdAsync(vagaid);
			if (job == null)
				return NotFound("Vaga não encontrada");

			if (file == null || file.Length == 0)
				return BadRequest("Arquivo inválido");

			if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
				return BadRequest("Apenas arquivos PDF são aceitos");

			// Pasta para guardar CVs em wwwroot/cvs
			var cvFolder = _config["CvStorage:Folder"] ?? "wwwroot/cvs";
			var fullPath = Path.Combine(_env.ContentRootPath, cvFolder);
			Directory.CreateDirectory(fullPath);

			// Nome único para o ficheiro
			var extension = Path.GetExtension(file.FileName);
			var uniqueName = $"{Guid.NewGuid():N}{extension}";
			var relativePath = Path.Combine("cvs", uniqueName).Replace('\\', '/');
			var filePath = Path.Combine(fullPath, uniqueName);

			// Guardar ficheiro em disco
			using (var fileStream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(fileStream);
			}

			// Extrair texto do PDF (abre o ficheiro guardado)
			string rawText;
			string normalizedText;
			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				rawText = _pdfExtractor.ExtractText(stream);
				normalizedText = _normalizer.Normalizar(rawText);
			}

			var candidate = new CandidatoCV
			{
				VagaID = vagaid,
				NomeFicheiro = file.FileName,
				CaminhoFicheiro = relativePath,
				RawText = rawText,
				NormalizedText = normalizedText
			};

			var created = await _candidateRepo.CreateAsync(candidate);
			return CreatedAtAction(nameof(GetCandidates), new { vagaid }, created);
		}

		[HttpGet]
		public async Task<IActionResult> GetCandidates(int jobId)
		{
			var candidates = await _candidateRepo.GetByJobIdAsync(jobId);
			return Ok(candidates);
		}
	}
}
