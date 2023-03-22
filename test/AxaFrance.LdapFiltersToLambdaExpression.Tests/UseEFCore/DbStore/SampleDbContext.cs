using Microsoft.EntityFrameworkCore;

namespace AxaFrance.LdapFiltersToLambdaExpression.Tests.UseEFCore.DbStore;

public partial class SampleDbContext : DbContext
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Model> Models { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");

        modelBuilder.Entity<Model>(
            entity =>
            {
                entity.HasKey(e => e.DistinguishedName);
                entity.ToTable("Model");

                entity.Property(e => e.DistinguishedName).HasColumnName("distinguishedname");
                entity.Property(e => e.Sn).HasColumnName("sn");
                entity.Property(e => e.Civility).HasColumnName("civility");
                entity.Property(e => e.Job).HasColumnName("job");
                entity.Property(e => e.GivenName).HasColumnName("givenname");
                entity.Property(e => e.Version).HasColumnName("version");
            });

        this.OnModelCreatingPartial(modelBuilder);
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
