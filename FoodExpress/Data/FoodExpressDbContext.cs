using FoodExpress.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Data
{
    public class FoodExpressDbContext : IdentityDbContext<ApplicationUser>
    {
        public FoodExpressDbContext(DbContextOptions<FoodExpressDbContext> options)
            : base(options)
        {
        }

        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<FoodCategory> Categories { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Decimal Precisions
            builder.Entity<FoodItem>()
                .Property(f => f.Price)
                .HasPrecision(18, 2);

            builder.Entity<CartItem>()
                .Property(c => c.Price)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.SubTotal)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.DeliveryFee)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasPrecision(18, 2);

            builder.Entity<OrderItem>()
                .Property(oi => oi.SubTotal)
                .HasPrecision(18, 2);

            builder.Entity<Coupon>()
                .Property(c => c.DiscountValue)
                .HasPrecision(18, 2);

            builder.Entity<Coupon>()
                .Property(c => c.MinimumOrderAmount)
                .HasPrecision(18, 2);

            // Indexes
            builder.Entity<Coupon>()
                .HasIndex(c => c.Code)
                .IsUnique();

            // Foreign Keys and Delete behaviors to prevent cascade cycles in SQL Server
            builder.Entity<Restaurant>()
                .HasOne(r => r.Owner)
                .WithMany(u => u.OwnedRestaurants)
                .HasForeignKey(r => r.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FoodItem>()
                .HasOne(f => f.Restaurant)
                .WithMany(r => r.FoodItems)
                .HasForeignKey(f => f.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FoodItem>()
                .HasOne(f => f.Category)
                .WithMany(c => c.FoodItems)
                .HasForeignKey(f => f.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.Restaurant)
                .WithMany(r => r.Orders)
                .HasForeignKey(o => o.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.Restaurant)
                .WithMany(rest => rest.Reviews)
                .HasForeignKey(r => r.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.FoodItem)
                .WithMany(f => f.Reviews)
                .HasForeignKey(r => r.FoodItemId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Address>()
                .HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.FoodItem)
                .WithMany(f => f.CartItems)
                .HasForeignKey(ci => ci.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.FoodItem)
                .WithMany(f => f.OrderItems)
                .HasForeignKey(oi => oi.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
