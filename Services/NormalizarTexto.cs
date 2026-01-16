using CV_Ranking.DAL;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace CV_Ranking.Services
{
	public class Normalizartexto : INormalizarTexto
	{
		public string Normalizar(string texto)
		{
			if (string.IsNullOrWhiteSpace(texto))
				return string.Empty;

			// Converter para minúsculas
			texto = texto.ToLowerInvariant();

			// Remover caracteres especiais, mantendo letras, números e espaços
			texto = Regex.Replace(texto, @"[^a-z0-9\s]", " ");

			// Remover múltiplos espaços
			texto = Regex.Replace(texto, @"\s+", " ");

			// Remover espaços no início e fim
			texto = texto.Trim();

			// Remover stop words comuns (simplificado)
			var stopWords = new HashSet<string>
			{
				"o", "a", "de", "da", "do", "para", "com", "em", "e",
				"os", "as", "dos", "das", "um", "uma", "the", "and", "or", "in", "on", "at"
			};

			var words = texto.Split(' ')
			.Where(w => !string.IsNullOrWhiteSpace(w) && !stopWords.Contains(w) && w.Length > 2)
			.ToList();

			return string.Join(" ", words);
		}
	}
}
