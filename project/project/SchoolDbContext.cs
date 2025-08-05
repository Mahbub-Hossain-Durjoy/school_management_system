using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project
{
    public class SchoolDbContext : DbContext
    {
        private readonly string _connectionString;

        public SchoolDbContext ()
        {
            _connectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=CSharpB20;User ID=csharpb20;Password=123456;TrustServerCertificate=True;";
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                 .HasOne(x => x.Teacher)
                 .WithOne(y => y.User)
                 .HasForeignKey<Teacher>(z => z.UserId);
            

            modelBuilder.Entity<Grade>()
                .HasOne(x=> x.Student)
                .WithMany(y=> y.Grades)
                .HasForeignKey(z => z.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Grade>()
                .HasOne(x => x.Assignment)
                .WithMany(y=> y.Grades)
                .HasForeignKey(z=> z.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<ClassRoom> Classes { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Grade> Grades {  get; set; }

    }
}
