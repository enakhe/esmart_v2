using ESMART.Domain.Entities.Configuration;
using ESMART.Domain.Entities.Data;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Entities.Verification;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ESMART.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserClaim<string>, ApplicationUserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>(options)
    {
        public DbSet<Guest> Guests { get; set; }
        public DbSet<GuestIdentity> GuestIdentities { get; set; }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionItem> TransactionItems { get; set; }

        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Floor> Floors { get; set; }
        public DbSet<Room> Rooms { get; set; }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<SettingsCategory> SettingsCategories { get; set; }
        public DbSet<HotelSetting> HotelSettings { get; set; }

        public DbSet<VerificationCode> VerificationCodes { get; set; }
        public DbSet<LicenceInformation> LicenceInformation { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            _ = builder.HasDefaultSchema("ESMART");
            _ = builder.Entity<ApplicationUser>(entity =>
            {
                _ = entity.ToTable(name: "User");
            });
            _ = builder.Entity<ApplicationRole>(entity =>
            {
                _ = entity.ToTable(name: "Role");
            });
            _ = builder.Entity<ApplicationUserRole>(entity =>
            {
                _ = entity.ToTable(name: "UserRoles");
            });
            _ = builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                _ = entity.ToTable("UserClaims");
            });
            _ = builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                _ = entity.ToTable("UserLogins");
            });
            _ = builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                _ = entity.ToTable("RoleClaims");
            });
            _ = builder.Entity<IdentityUserToken<string>>(entity =>
            {
                _ = entity.ToTable("UserTokens");
            });
        }
    }
}
