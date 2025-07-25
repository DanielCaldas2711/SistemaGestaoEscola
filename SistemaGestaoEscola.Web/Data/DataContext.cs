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
        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassStudents> ClassStudents { get; set; }

        public DbSet<ClassProfessors> ClassProfessors { get; set; }

        public DbSet<StudentGrades> StudentGrades { get; set; }

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
            
            modelBuilder.Entity<Class>()
                .HasOne(c => c.Course)
                .WithMany()
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<ClassStudents>()
                .HasOne(cs => cs.Class)
                .WithMany(c => c.Students)
                .HasForeignKey(cs => cs.ClassId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<ClassStudents>()
                .HasOne(cs => cs.Student)
                .WithMany()
                .HasForeignKey(cs => cs.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClassProfessors>()
                .HasOne(cs => cs.Class)
                .WithMany(c => c.Professors)
                .HasForeignKey(cs => cs.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClassProfessors>()
               .HasOne(cs => cs.Professor)
               .WithMany()
               .HasForeignKey(cs => cs.ProfessorId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClassProfessors>()
                .HasOne(cs => cs.Subject)
                .WithMany(c => c.Professors)
                .HasForeignKey(cs => cs.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentGrades>()
                .HasOne(sg => sg.ClassStudents)
                .WithMany(s => s.StudentGrades)
                .HasForeignKey(sg => sg.ClassStudentsId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentGrades>()
                .HasOne(sg => sg.Subject)
                .WithMany(s => s.StudentGrades)
                .HasForeignKey(sg => sg.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
