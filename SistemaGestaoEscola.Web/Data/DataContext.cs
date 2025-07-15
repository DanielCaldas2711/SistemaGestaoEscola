using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data.Entities;

namespace SistemaGestaoEscola.Web.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseDisciplines> CourseDisciplines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Subject>()
                .HasIndex(s => s.Code)
                .IsUnique();

            modelBuilder.Entity<Course>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<CourseDisciplines>()
                .HasOne(cd => cd.Course)
                .WithMany(c => c.CourseDisciplines)
                .HasForeignKey(cd => cd.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CourseDisciplines>()
                .HasOne(cd => cd.Subject)
                .WithMany(s => s.CourseDisciplines)
                .HasForeignKey(cd => cd.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
