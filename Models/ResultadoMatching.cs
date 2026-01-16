namespace CV_Ranking.Models
{
	public class ResultadoMatching
	{
		public int Id { get; set; }
		public int VagaID { get; set; }
		public Vaga? Vaga { get; set; }
		public int CandidatoCVID { get; set; }
		public CandidatoCV? CandidatoCV { get; set; }
		public float ScoreCompativel { get; set; }
		public int Ranking { get; set; }
		public DateTime Calculado { get; set; } = DateTime.UtcNow;
	}

	//DTO
	public class MatchingResultadoDto
	{
		public int CandidatoID { get; set; }
		public string FileName { get; set; } = string.Empty;
		public float Score { get; set; }
		public int Ranking { get; set; }
	}

	public class RankingResponseDTO
	{
		public int JobVacancyId { get; set; }
		public string JobTitle { get; set; } = string.Empty;
		public List<MatchingResultadoDto> Candidatos { get; set; } = new();
		public DateTime ProcessedAt { get; set; }
	}
}