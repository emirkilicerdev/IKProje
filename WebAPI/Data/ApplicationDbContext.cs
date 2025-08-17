using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet'ler
        public DbSet<UserModel> Users { get; set; }
        public DbSet<RoleModel> Roles { get; set; }
        public DbSet<UserRoleModel> UserRoles { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveTypeModel> LeaveTypes { get; set; }
        public DbSet<LeaveStatusModel> LeaveStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureUserModel(modelBuilder);
            ConfigureRoleModel(modelBuilder);
            ConfigureUserRoleModel(modelBuilder);
            ConfigureLeaveRequestModel(modelBuilder);
            ConfigureLeaveTypeModel(modelBuilder);
            ConfigureLeaveStatusModel(modelBuilder);
        }

        private void ConfigureUserModel(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<UserModel>();

            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();

            // Kullanıcı silindiğinde izinleri de silinsin
            entity.HasMany(u => u.LeaveRequests)
                  .WithOne(lr => lr.User)
                  .HasForeignKey(lr => lr.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureRoleModel(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<RoleModel>();

            entity.HasIndex(r => r.Name).IsUnique();
        }

        private void ConfigureUserRoleModel(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<UserRoleModel>();

            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(ur => ur.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(ur => ur.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureLeaveRequestModel(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<LeaveRequest>();

            entity.HasKey(lr => lr.Id);


            entity.HasOne(lr => lr.LeaveType)
                  .WithMany(t => t.LeaveRequests)
                  .HasForeignKey(lr => lr.LeaveTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(lr => lr.LeaveStatus)
                  .WithMany(s => s.LeaveRequests)
                  .HasForeignKey(lr => lr.LeaveStatusId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(lr => lr.StartDate)
                  .IsRequired();

            entity.Property(lr => lr.EndDate)
                  .IsRequired();

            entity.Property(lr => lr.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");
        }

        private void ConfigureLeaveTypeModel(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<LeaveTypeModel>();

            entity.HasKey(t => t.Id);

            entity.Property(t => t.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.HasMany(t => t.LeaveRequests)
                  .WithOne(lr => lr.LeaveType)
                  .HasForeignKey(lr => lr.LeaveTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureLeaveStatusModel(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<LeaveStatusModel>();

            entity.HasKey(s => s.Id);

            entity.Property(s => s.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.HasMany(s => s.LeaveRequests)
                  .WithOne(lr => lr.LeaveStatus)
                  .HasForeignKey(lr => lr.LeaveStatusId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
