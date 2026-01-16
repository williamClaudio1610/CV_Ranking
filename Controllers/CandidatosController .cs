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

		public CandidatosController(
			ICandidatoRepository candidateRepo,
			IVagaRepository  jobRepo,
			IPdfTextExtractor pdfExtractor,
			INormalizarTexto normalizer)
		{
			_candidateRepo = candidateRepo;
			_jobRepo = jobRepo;
			_pdfExtractor = pdfExtractor;
			_normalizer = normalizer;
		}

		[HttpPost]
		public async Task<IActionResult> UploadCV( int vagaid, IFormFile file)
		{
			var job = await _jobRepo.GetByIdAsync(vagaid);
			if (job == null)
				return NotFound("Vaga não encontrada");

			if (file == null || file.Length == 0)
				return BadRequest("Arquivo inválido");

			if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
				return BadRequest("Apenas arquivos PDF são aceitos");

			using var stream = file.OpenReadStream();
			var rawText = _pdfExtractor.ExtractText(stream);
			var normalizedText = _normalizer.Normalizar(rawText);

			var candidate = new CandidatoCV
			{
				VagaID = vagaid,
				NomeFicheiro = file.FileName,
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
