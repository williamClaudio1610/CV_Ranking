using CV_Ranking.Models;

namespace CV_Ranking.DAL
{


	public interface IPdfTextExtractor
	{
		string ExtractText(Stream pdfStream);
	}

	public interface INormalizarTexto
	{
		string Normalizar(string text);
	}

	public interface IMatchingEngine
	{
		List<(int candidateId, float score)> CalculateMatches(string jobDescription, List<(int id, string text)> candidates);
	}

	public interface IVagaRepository
	{
		Task<Vaga> CreateAsync(Vaga vaga);
		Task<Vaga?> GetByIdAsync(int id);
		Task<List<Vaga>> GetAllAsync();
	}

	public interface ICandidatoRepository
	{
		Task<CandidatoCV> CreateAsync(CandidatoCV candidate);
		Task<List<CandidatoCV>> GetByJobIdAsync(int jobId);
	}

	public interface IMatchingResultRepository
	{
		Task<List<ResultadoMatching>> SaveResultsAsync(List<ResultadoMatching> results);
		Task<List<ResultadoMatching>> GetByJobIdAsync(int jobId);
	}
}
