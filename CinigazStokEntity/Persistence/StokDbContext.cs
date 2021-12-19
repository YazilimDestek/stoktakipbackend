using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinigazStokEntity
{
    public class StokDbContext : DbContext
    {
        public StokDbContext(DbContextOptions<StokDbContext> options) : base(options)
        {
            Database.SetCommandTimeout(300);
        }

        public StokDbContext() : base()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            //optionsBuilder.UseNpgsql("Database=postgres;Port=5432;Host=localhost;User ID=cinigaz;Password=cinigaz2019;Pooling=true;Integrated Security=true;");
        }

        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CategoryField> CategoryFields { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemVariant> ItemVariants { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<LocationType> LocationTypes { get; set; }
        public DbSet<StockHistory> StockHistories { get; set; }
        public DbSet<StockLevel> StockLevels { get; set; }
        public DbSet<Timeout> Timeouts { get; set; }
        public DbSet<TransType> TransTypes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<QRTransfer> QRTransfers { get; set; }
        public DbSet<ItemType> ItemTypes { get; set; }
        public DbSet<ItemKind> ItemKinds { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>()
             .HasIndex(b => b.Barcode);

            modelBuilder.Entity<Item>()
             .HasIndex(b => b.SerialNumber);

            modelBuilder.Entity<StockLevel>()
             .HasKey(c => new { c.SerialNumber, c.BrandId,  c.LocationId });

            modelBuilder.Entity<Item>()
            .HasKey(c => new { c.SerialNumber, c.BrandId});
        }
    }
}
