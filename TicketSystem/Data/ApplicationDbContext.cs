using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketSystem.TSModel.Permission;
using TicketSystem.Model;
using TicketSystem.TSModel;

namespace TicketSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<OrderDetailOut> OrderDetailOuts { get; set; }
        public virtual DbSet<OrderFixed> OrderFixeds { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<UserSection> UserSections { get; set; }
        public DbSet<OrderRead> OrderReads { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Rate> Rates { get; set; }
        public DbSet<RawsData> RawsDatas { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 
            modelBuilder.Entity<UserSection>()
                .HasKey(us => new { us.UserId, us.SectionId });
            modelBuilder.Entity<UserSection>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserSections)
                .HasForeignKey(us => us.UserId);
            modelBuilder.Entity<UserSection>()
                .HasOne(us => us.Section)
                .WithMany(s => s.UserSections)
                .HasForeignKey(us => us.SectionId);
            modelBuilder.Entity<Section>().ToTable("Sections");
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("Orders$PrimaryKey");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Closed).HasDefaultValue(false);
                entity.Property(e => e.CreatedBy).HasMaxLength(255);
                entity.Property(e => e.DateTime).HasPrecision(0);
                entity.Property(e => e.FixedId)
                    .HasDefaultValue(0)
                    .HasColumnName("FixedID");
                entity.Property(e => e.LastDepartment)
                    .HasMaxLength(255)
                    .HasColumnName("Last_Department");
                entity.Property(e => e.Nid)
                    .HasMaxLength(255)
                    .HasColumnName("NID");
                entity.Property(e => e.PcName)
                    .HasMaxLength(255)
                    .HasColumnName("PC_Name");
                entity.Property(e => e.QueueNumber).HasMaxLength(255);
                entity.Property(e => e.RepeateFactor).HasDefaultValue(1);
                entity.Property(e => e.SsmaTimeStamp)
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("SSMA_TimeStamp");
                entity.HasOne(d => d.Fixed).WithMany(p => p.Orders)
                    .HasForeignKey(d => d.FixedId)
                    .HasConstraintName("FK_Orders_OrderFixed");
            });
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedBy).HasMaxLength(255);
                entity.Property(e => e.CreationDate).HasPrecision(0);
                entity.Property(e => e.FromDep).HasMaxLength(255);
                entity.Property(e => e.OrderId).HasColumnName("OrderID");
                entity.Property(e => e.PcName).HasMaxLength(255);
                entity.Property(e => e.QueueNumber).HasMaxLength(255);
                entity.Property(e => e.RecieptStatus).HasMaxLength(255);
                entity.Property(e => e.ToDepartment).HasMaxLength(255);
                entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderDetails_Orders");
            });
            modelBuilder.Entity<OrderFixed>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("OrderFixed$PrimaryKey");
                entity.ToTable("OrderFixed");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedBy).HasMaxLength(255);
                entity.Property(e => e.Level1).HasMaxLength(255);
                entity.Property(e => e.Level2).HasMaxLength(255);
                entity.Property(e => e.Level3).HasMaxLength(255);
                entity.Property(e => e.Level4).HasMaxLength(255);
                entity.Property(e => e.Level5).HasMaxLength(255);
                entity.Property(e => e.Level6).HasMaxLength(255);
                entity.Property(e => e.Okm).HasColumnName("OKM");
            });
            modelBuilder.Entity<Rate>()
               .HasOne(r => r.OrderFixed)
               .WithMany(of => of.Rates)
               .HasForeignKey(r => r.OrderFixedId)
               .OnDelete(DeleteBehavior.Cascade); 
            modelBuilder.Entity<RawsData>()
                .HasOne(r => r.Order)
                .WithMany(o => o.RawsDataRecords)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RawsData>()
                .HasOne(r => r.OrderFixed)
                .WithMany(of => of.RawsDataRecords)
                .HasForeignKey(r => r.OrderFixedId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RawsData>()
                .HasOne(r => r.Rate)
                .WithMany()
                .HasForeignKey(r => r.RateId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}