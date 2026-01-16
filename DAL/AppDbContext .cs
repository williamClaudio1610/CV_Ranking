using CV_Ranking.Models;
using Microsoft.EntityFrameworkCore;

namespace CVMatching.Infrastructure.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

	public DbSet<Vaga> JobVacancies { get; set; }
	public DbSet<CandidatoCV> Candidates { get; set; }
	public DbSet<ResultadoMatching> ResultadoMatchings { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Vaga>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
			entity.Property(e => e.Descricao).IsRequired();
		});

		modelBuilder.Entity<CandidatoCV>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.NomeFicheiro).IsRequired().HasMaxLength(255);
			entity.HasOne(e => e.Vaga)
				  .WithMany(j => j.Candidatos)
				  .HasForeignKey(e => e.VagaID)
				  .OnDelete(DeleteBehavior.Cascade);
		});

		modelBuilder.Entity<ResultadoMatching>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.HasOne(e => e.Vaga)
				  .WithMany()
				  .HasForeignKey(e => e.VagaID)
				  .OnDelete(DeleteBehavior.Cascade);
			entity.HasOne(e => e.CandidatoCV)
				  .WithMany(c => c.ResultadoMatching)
				  .HasForeignKey(e => e.CandidatoCVID)
				  .OnDelete(DeleteBehavior.Cascade);
		});
	}
}