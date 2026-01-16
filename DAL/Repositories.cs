using CV_Ranking.DAL;
using CV_Ranking.Models;
using CVMatching.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CVMatching.Infrastructure.Repositories;

public class VagaRepository : IVagaRepository
{
	private readonly AppDbContext _context;

	public VagaRepository(AppDbContext context)
	{
		_context = context;
	}

	public async Task<Vaga> CreateAsync(Vaga vacancy)
	{
		_context.JobVacancies.Add(vacancy);
		await _context.SaveChangesAsync();
		return vacancy;
	}

	public async Task<Vaga?> GetByIdAsync(int id)
	{
		return await _context.JobVacancies
			.Include(j => j.Candidatos)
			.FirstOrDefaultAsync(j => j.Id == id);
	}

	public async Task<List<Vaga>> GetAllAsync()
	{
		return await _context.JobVacancies.ToListAsync();
	}
}

public class CandidateRepository : ICandidatoRepository
{
	private readonly AppDbContext _context;

	public CandidateRepository(AppDbContext context)
	{
		_context = context;
	}

	public async Task<CandidatoCV> CreateAsync(CandidatoCV candidate)
	{
		_context.Candidates.Add(candidate);
		await _context.SaveChangesAsync();
		return candidate;
	}

	public async Task<List<CandidatoCV>> GetByJobIdAsync(int jobId)
	{
		return await _context.Candidates
			.Where(c => c.VagaID == jobId)
			.ToListAsync();
	}
}

public class ResultadoMatchingRepository : IMatchingResultRepository
{
	private readonly AppDbContext _context;

	public ResultadoMatchingRepository(AppDbContext context)
	{
		_context = context;
	}

	public async Task<List<ResultadoMatching>> SaveResultsAsync(List<ResultadoMatching> results)
	{
		_context.ResultadoMatchings.AddRange(results);
		await _context.SaveChangesAsync();
		return results;
	}

	public async Task<List<ResultadoMatching>> GetByJobIdAsync(int jobId)
	{
		return await _context.ResultadoMatchings
			.Include(m => m.CandidatoCV)
			.Where(m => m.VagaID == jobId)
			.OrderBy(m => m.Ranking)
			.ToListAsync();
	}
}