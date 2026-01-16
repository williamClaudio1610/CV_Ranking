using CV_Ranking.DAL;
using CV_Ranking.Models;
using Microsoft.AspNetCore.Mvc;

namespace CV_Ranking.Controllers
{

	[ApiController]
	[Route("api/[controller]")]
	public class VagaController : ControllerBase
	{
		private readonly IVagaRepository _vagarepo;
		public VagaController(IVagaRepository vagaRepo)
		{
			_vagarepo = vagaRepo;
		}

		[HttpPost]
		public async Task<IActionResult> CriarVaga([FromBody] CriarVagaDTO dto)
		{
			var job = new Vaga
			{
				Titulo = dto.Titulo,
				Descricao = dto.Descricao
			};

			var created = await _vagarepo.CreateAsync(job);
			return CreatedAtAction(nameof(GetJob), new { id = created.Id }, created);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetJob(int id)
		{
			var job = await _vagarepo.GetByIdAsync(id);
			if (job == null)
				return NotFound();

			return Ok(job);
		}

		[HttpGet]
		public async Task<IActionResult> GetAllJobs()
		{
			var jobs = await _vagarepo.GetAllAsync();
			return Ok(jobs);
		}
	}
}
