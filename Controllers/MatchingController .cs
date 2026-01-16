using CV_Ranking.DAL;
using CV_Ranking.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/matching")]
public class MatchingController : ControllerBase
{
	private readonly IVagaRepository _jobRepo;
	private readonly ICandidatoRepository _candidateRepo;
	private readonly IMatchingResultRepository _matchingRepo;
	private readonly IMatchingEngine _matchingEngine;
	private readonly INormalizarTexto _normalizer;

	public MatchingController(
		IVagaRepository jobRepo,
		ICandidatoRepository candidateRepo,
		IMatchingResultRepository matchingRepo,
		IMatchingEngine matchingEngine,
		INormalizarTexto normalizer)
	{
		_jobRepo = jobRepo;
		_candidateRepo = candidateRepo;
		_matchingRepo = matchingRepo;
		_matchingEngine = matchingEngine;
		_normalizer = normalizer;
	}

	[HttpPost("{vagaId}")]
	public async Task<IActionResult> ProcessMatching(int jobId)
	{
		var job = await _jobRepo.GetByIdAsync(jobId);
		if (job == null)
			return NotFound("Vaga não encontrada");

		var candidates = await _candidateRepo.GetByJobIdAsync(jobId);
		if (candidates.Count == 0)
			return BadRequest("Nenhum candidato encontrado para esta vaga");

		var normalizedJobDesc = _normalizer.Normalizar(job.Descricao);
		var candidateTexts = candidates
			.Select(c => (c.Id, c.NormalizedText))
			.ToList();

		var scores = _matchingEngine.CalculateMatches(normalizedJobDesc, candidateTexts);

		var results = scores.Select((s, index) => new ResultadoMatching
		{
			VagaID = jobId,
			CandidatoCVID = s.candidateId,
			ScoreCompativel = s.score,
			Ranking = index + 1
		}).ToList();

		await _matchingRepo.SaveResultsAsync(results);

		return Ok(new { message = "Matching processado com sucesso", totalCandidates = results.Count });
	}

	[HttpGet("{jobId}/ranking")]
	public async Task<IActionResult> GetRanking(int jobId)
	{
		var job = await _jobRepo.GetByIdAsync(jobId);
		if (job == null)
			return NotFound("Vaga não encontrada");

		var results = await _matchingRepo.GetByJobIdAsync(jobId);
		if (results.Count == 0)
			return NotFound("Nenhum resultado de matching encontrado. Execute o matching primeiro.");

		var response = new RankingResponseDTO
		{
			JobVacancyId = jobId,
			JobTitle = job.Titulo,
			ProcessedAt = results.Max(r => r.Calculado),
			Candidatos = results.Select(r => new MatchingResultadoDto
			{
				CandidatoID = r.CandidatoCVID,
				FileName = r.CandidatoCV?.NomeFicheiro ?? "Unknown",
				Score = r.ScoreCompativel,
				Ranking = r.Ranking
			}).ToList()
		};

		return Ok(response);
	}
}