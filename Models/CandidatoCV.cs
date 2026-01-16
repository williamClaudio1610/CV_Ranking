namespace CV_Ranking.Models
{
	public class CandidatoCV
	{
		public int Id { get; set; }
		public string NomeFicheiro { get; set; } = string.Empty;
		public string RawText { get; set; } = string.Empty;
		public string NormalizedText { get; set; } = string.Empty;
		public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
		public int VagaID { get; set; }
		public Vaga? Vaga { get; set; }
		public List<ResultadoMatching> ResultadoMatching { get; set; } = new();

	}
}