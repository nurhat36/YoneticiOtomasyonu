using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using YoneticiOtomasyonu.Models;
using YoneticiOtomasyonu.Models;

namespace YoneticiOtomasyonu.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tablolar
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Income> Incomes { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<WorkTask> WorkTasks { get; set; }
        public DbSet<ComplaintImage> ComplaintImages { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<MeetingAttendance> MeetingAttendances { get; set; }
        public DbSet<UserBuildingRole> UserBuildingRoles { get; internal set; }
        public DbSet<DuesSetting> DuesSettings { get; set; }
        public DbSet<UserDebt> UserDebts { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ComplaintImage>()
    .HasOne(ci => ci.Complaint)
    .WithMany(c => c.Images)
    .HasForeignKey(ci => ci.ComplaintId)
    .OnDelete(DeleteBehavior.Cascade);


            // WorkTask ilişkileri
            builder.Entity<WorkTask>()
                .HasOne(t => t.Complaint)
                .WithMany(c => c.WorkTasks)
                .HasForeignKey(t => t.ComplaintId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WorkTask>()
                .HasOne(t => t.Building)
                .WithMany(b => b.WorkTasks)
                .HasForeignKey(t => t.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WorkTask>()
                .HasOne(t => t.CreatedBy)
                .WithMany()
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WorkTask>()
                .HasOne(t => t.AssignedTo)
                .WithMany()
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserProfile ilişkisi
            builder.Entity<UserProfile>()
                .HasOne(up => up.IdentityUser)
                .WithOne()
                .HasForeignKey<UserProfile>(up => up.IdentityUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // MeetingAttendance ilişkileri
            builder.Entity<MeetingAttendance>()
                .HasKey(ma => new { ma.MeetingId, ma.UserId });

            builder.Entity<MeetingAttendance>()
                .HasOne(ma => ma.Meeting)
                .WithMany(m => m.Attendances)
                .HasForeignKey(ma => ma.MeetingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MeetingAttendance>()
                .HasOne(ma => ma.User)
                .WithMany()
                .HasForeignKey(ma => ma.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Meeting ilişkileri
            builder.Entity<Meeting>()
                .HasOne(m => m.Building)
                .WithMany(b => b.Meetings)
                .HasForeignKey(m => m.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Meeting>()
                .HasOne(m => m.OrganizedBy)
                .WithMany()
                .HasForeignKey(m => m.OrganizedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Diğer varsayılan değerler
            builder.Entity<Unit>()
                .Property(u => u.IsOccupied)
                .HasDefaultValue(false);

            builder.Entity<Complaint>()
                .Property(c => c.Status)
                .HasDefaultValue("Beklemede");

            builder.Entity<WorkTask>()
                .Property(t => t.Status)
                .HasDefaultValue("Beklemede");

            builder.Entity<WorkTask>()
                .Property(t => t.Priority)
                .HasDefaultValue("Orta");
            builder.Entity<Building>()
                .HasMany(b => b.UserRoles)
                .WithOne(ubr => ubr.Building)
                .HasForeignKey(ubr => ubr.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Building creator ilişkisi
            builder.Entity<Building>()
                .HasOne(b => b.CreatorUser)
                .WithMany()
                .HasForeignKey(b => b.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserBuildingRole atayan kullanıcı ilişkisi
            builder.Entity<UserBuildingRole>()
                .HasOne(ubr => ubr.AssignedByUser)
                .WithMany()
                .HasForeignKey(ubr => ubr.AssignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<DuesSetting>()
        .HasOne(ds => ds.Building)
        .WithMany(b => b.DuesSettings)
        .HasForeignKey(ds => ds.BuildingId)
        .OnDelete(DeleteBehavior.Cascade);

            // UserDebt - Building ilişkisi
            builder.Entity<UserDebt>()
                .HasOne(ud => ud.Building)
                .WithMany(b => b.UserDebts)
                .HasForeignKey(ud => ud.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserDebt - User ilişkisi
            builder.Entity<UserDebt>()
                .HasOne(ud => ud.User)
                .WithMany(u => u.UserDebts)
                .HasForeignKey(ud => ud.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Type alanı için max uzunluk kontrolü
            builder.Entity<UserDebt>()
                .Property(ud => ud.Type)
                .HasMaxLength(50);

            // Tablo isimlendirmeleri
            builder.Entity<Building>().ToTable("Buildings");
            builder.Entity<Unit>().ToTable("Units");
            builder.Entity<Income>().ToTable("Incomes");
            builder.Entity<Expense>().ToTable("Expenses");
            builder.Entity<Complaint>().ToTable("Complaints");
            builder.Entity<WorkTask>().ToTable("WorkTasks");
            builder.Entity<Announcement>().ToTable("Announcements");
            builder.Entity<UserProfile>().ToTable("UserProfiles");
            builder.Entity<Document>().ToTable("Documents");
            builder.Entity<Meeting>().ToTable("Meetings");
            builder.Entity<MeetingAttendance>().ToTable("MeetingAttendances");

            // Identity tablo isimlendirmeleri (opsiyonel)
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        }
    }
}