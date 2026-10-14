using System.Data.Entity;
using MySql.Data.EntityFramework;
using PhotoCenterWeb.Models;

namespace PhotoCenterWeb.Models
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class PhotoCenterContext : DbContext
    {
        public PhotoCenterContext() : base("name=PhotoCenterEntities")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplyRequest> SupplyRequests { get; set; }
        public DbSet<ServiceMaterial> ServiceMaterials { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<PhotoCenterContext>(null);

            // Связь User → Clients (один-ко-многим)
            modelBuilder.Entity<Client>()
                .HasRequired(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Employee>()
                .HasRequired(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserID)
                .WillCascadeOnDelete(false);

            // Связь SupplyRequest → Material
            modelBuilder.Entity<SupplyRequest>()
                .HasRequired(sr => sr.Material)
                .WithMany(m => m.SupplyRequests)
                .HasForeignKey(sr => sr.MaterialID);

            // Связь SupplyRequest → Supplier
            modelBuilder.Entity<SupplyRequest>()
                .HasRequired(sr => sr.Supplier)
                .WithMany(s => s.SupplyRequests)
                .HasForeignKey(sr => sr.SupplierID);

            // Связь ServiceMaterial → Service
            modelBuilder.Entity<ServiceMaterial>()
                .HasRequired(sm => sm.Service)
                .WithMany(s => s.ServiceMaterials)
                .HasForeignKey(sm => sm.ServiceID);

            // Связь ServiceMaterial → Material
            modelBuilder.Entity<ServiceMaterial>()
                .HasRequired(sm => sm.Material)
                .WithMany(m => m.ServiceMaterials)
                .HasForeignKey(sm => sm.MaterialID);

            base.OnModelCreating(modelBuilder);
        }
    }
}