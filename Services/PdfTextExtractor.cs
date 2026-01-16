using UglyToad.PdfPig;
using System.Text;
using CV_Ranking.DAL;

namespace CV_Ranking.Services
{
	public class PdfTextExtractor : IPdfTextExtractor
	{
		public string ExtractText(Stream pdfStream)
		{
			try { 
			using var document = PdfDocument.Open(pdfStream);
			var textBuilder = new StringBuilder();

			foreach (var page in document.GetPages())
			{
				var text = page.Text;
				textBuilder.AppendLine(page.Text);
			}
			return textBuilder.ToString();
				}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Erro ao extrair texto do PDF: {ex.Message}", ex);

			}
		}
	}
}
