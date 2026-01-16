namespace CV_Ranking.Models
{
	public class Vaga
	{
		public int Id { get; set; }
		public string Titulo { get; set; }
		public string Descricao { get; set; }
		public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
		public List<CandidatoCV> Candidatos { get; set; } = new();
	}


	// DTOs
	public class CriarVagaDTO
	{
		public string Titulo { get; set; } = string.Empty;
		public string Descricao { get; set; } = string.Empty;
	}
}
