using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserApi.Models;
using UserApprovalApi.Models;

namespace UserApprovalApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets
        public DbSet<User> Users => Set<User>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Approval> Approvals => Set<Approval>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Report> Reports => Set<Report>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----- User entity configuration -----
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(u => u.Id);

                // Unique constraints
                entity.HasIndex(u => u.UserId).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();

                // Required fields & lengths (you can tweak as needed)
                entity.Property(u => u.UserId)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(u => u.Name)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(u => u.Branch)
                      .HasMaxLength(100);

                // Enum -> tinyint (byte) conversions
                entity.Property(u => u.Status)
                      .HasConversion<byte>()           // stores 0/1/2
                      .IsRequired();

                entity.Property(u => u.Role)
                      .HasConversion<byte>()           // stores 1/2/3 for Admin/Manager/Officer
                      .IsRequired();

                // Timestamps
                entity.Property(u => u.CreatedAtUtc)
                      .HasColumnType("datetime2(3)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(u => u.UpdatedAtUtc)
                      .HasColumnType("datetime2(3)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                // Helpful filtered indexes
                entity.HasIndex(u => u.Status).HasDatabaseName("IX_Users_Status");
                entity.HasIndex(u => u.Role).HasDatabaseName("IX_Users_Role");
            });

            // ----- Account entity configuration -----
            modelBuilder.Entity<Account>(entity =>
            {
                entity.Property(a => a.Balance)
                      .HasPrecision(18, 2);

                entity.Property(a => a.AccountType)
                      .HasConversion<string>();

                entity.Property(a => a.Status)
                      .HasConversion<string>();
            });

            // ----- Transaction entity configuration -----
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(t => t.Amount)
                      .HasPrecision(18, 2);

                entity.Property(t => t.Type)
                      .HasConversion<string>();

                entity.Property(t => t.Status)
                      .HasConversion<string>();
            });

            // ----- Approval entity configuration -----
            modelBuilder.Entity<Approval>(entity =>
            {
                entity.Property(a => a.Type)
                      .HasConversion<string>();

                entity.Property(a => a.Decision)
                      .HasConversion<string>();

                // Configure relationships
                entity.HasOne(a => a.Transaction)
                      .WithMany()
                      .HasForeignKey(a => a.TransactionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Account)
                      .WithMany()
                      .HasForeignKey(a => a.AccountId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ----- Notification entity configuration -----
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(n => n.Type)
                      .HasConversion<string>();

                entity.Property(n => n.Status)
                      .HasConversion<string>();
            });

            // ----- Report entity configuration -----
            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("Reports");

                entity.HasKey(r => r.Id);

                entity.Property(r => r.GeneratedByUserId)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(r => r.ReportType)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(r => r.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(r => r.GeneratedAt)
                      .HasColumnType("datetime2")
                      .IsRequired()
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(r => r.FromDate)
                      .HasColumnType("datetime2");

                entity.Property(r => r.ToDate)
                      .HasColumnType("datetime2");

                entity.Property(r => r.GrowthRate)
                      .HasPrecision(18, 2)
                      .HasDefaultValue(0.00m);

                entity.Property(r => r.TotalAmount)
                      .HasPrecision(18, 2)
                      .HasDefaultValue(0.00m);

                entity.Property(r => r.TotalTransactions)
                      .HasDefaultValue(0);

                entity.Property(r => r.DataJson)
                      .HasColumnType("nvarchar(max)");

                entity.Property(r => r.FilePath)
                      .HasMaxLength(500);

                entity.Property(r => r.TransactionStatus)
                      .HasMaxLength(50);

                entity.Property(r => r.TransactionType)
                      .HasMaxLength(50);

                // Foreign key relationship
                entity.HasOne(r => r.Account)
                      .WithMany()
                      .HasForeignKey(r => r.AccountId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Indexes
                entity.HasIndex(r => r.AccountId)
                      .HasDatabaseName("IX_Reports_AccountId");

                entity.HasIndex(r => r.GeneratedAt)
                      .HasDatabaseName("IX_Reports_GeneratedAt");

                entity.HasIndex(r => r.GeneratedByUserId)
                      .HasDatabaseName("IX_Reports_GeneratedByUserId");

                entity.HasIndex(r => r.ReportType)
                      .HasDatabaseName("IX_Reports_ReportType");

                entity.HasIndex(r => new { r.FromDate, r.ToDate })
                      .HasDatabaseName("IX_Reports_FromDate_ToDate");
            });
        }

        /// <summary>
        /// Automatically sets UpdatedAtUtc on entities that have that property whenever SaveChanges is called.
        /// </summary>
        public override int SaveChanges()
        {
            TouchUpdatedAt();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            TouchUpdatedAt();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            TouchUpdatedAt();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            TouchUpdatedAt();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void TouchUpdatedAt()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);

            foreach (var e in entries)
            {
                var prop = e.Properties.FirstOrDefault(p => p.Metadata.Name == nameof(User.UpdatedAtUtc));
                if (prop != null)
                {
                    prop.CurrentValue = DateTime.UtcNow;
                }
            }
        }

        public void ValidateTransaction(Transaction transaction)
        {
            // Convert transaction.Type (string) to TransactionType enum for comparison
            if (Enum.TryParse<TransactionType>(transaction.Type, out var transactionType))
            {
                if (transactionType == TransactionType.Transfer && transaction.TargetAccountId == null)
                {
                    throw new ArgumentException("TargetAccountId is required for Transfer transactions.");
                }

                if (transactionType != TransactionType.Transfer && transaction.TargetAccountId != null)
                {
                    throw new ArgumentException("TargetAccountId should only be set for Transfer transactions.");
                }
            }
            else
            {
                throw new ArgumentException("Invalid transaction type.");
            }
        }


    }
}
