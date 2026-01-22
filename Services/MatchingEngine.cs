using CV_Ranking.DAL;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace CV_Ranking.Services
{
	public class MatchingEngine : IMatchingEngine
	{
		private readonly MLContext _mlContext;

		public MatchingEngine()
		{
			_mlContext = new MLContext(seed: 0);
		}
		public List<(int candidateId, float score)> CalculateMatches(string jobDescription, List<(int id, string text)> candidates)
		{
			if (candidates.Count == 0)
				return new List<(int, float)>();

			// Preparar dados para processamento
			var allTexts = new List<TextData>
		{
			new TextData { Id = -1, Text = jobDescription } // Job como referência
        };

			allTexts.AddRange(candidates.Select(c => new TextData { Id = c.id, Text = c.text }));

			var dataView = _mlContext.Data.LoadFromEnumerable(allTexts);

			// Pipeline de featurização com TF-IDF
			var pipeline = _mlContext.Transforms.Text.FeaturizeText(
				outputColumnName: "Features",
				inputColumnName: nameof(TextData.Text));

			var model = pipeline.Fit(dataView);
			var transformedData = model.Transform(dataView);

			// Extrair vetores de features
			var features = _mlContext.Data.CreateEnumerable<TextFeatures>(
				transformedData,
				reuseRowObject: false).ToList();

			var jobVector = features[0].Features;
			var candidateVectors = features.Skip(1).ToList();

			// Calcular similaridade de cosseno
			var results = new List<(int candidateId, float score)>();

			for (int i = 0; i < candidateVectors.Count; i++)
			{
				var similarity = CosineSimilaridade(jobVector, candidateVectors[i].Features);
				results.Add((candidates[i].id, similarity));
			}

			// Ordenar por score descendente
			return results.OrderByDescending(r => r.score).ToList();
		}

		private float CosineSimilaridade(float[] vectorA, float[] vectorB)
		{
			if (vectorA.Length != vectorB.Length)
				return 0f;

			float dotProduct = 0f;
			float magnitudeA = 0f;
			float magnitudeB = 0f;

			for (int i = 0; i < vectorA.Length; i++)
			{
				dotProduct += vectorA[i] * vectorB[i];
				magnitudeA += vectorA[i] * vectorA[i];
				magnitudeB += vectorB[i] * vectorB[i];
			}

			magnitudeA = (float)Math.Sqrt(magnitudeA);
			magnitudeB = (float)Math.Sqrt(magnitudeB);

			if (magnitudeA == 0 || magnitudeB == 0)
				return 0f;

			return dotProduct / (magnitudeA * magnitudeB);
		}

		public class TextData
		{
			public int Id { get; set; }
			public string Text { get; set; } = string.Empty;
		}

		public class TextFeatures
		{
			[VectorType]
			public float[] Features { get; set; } = Array.Empty<float>();
		}
	}
}

