using Microsoft.EntityFrameworkCore;
using Sql_Backend.Models;

namespace Sql_Backend.DAL
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Chef> Chefs { get; set; }
        public DbSet<FoodLover> FoodLovers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Recipe> Recipe { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<FavoriteRecipe> FavoriteRecipes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure FoodLover entity
            modelBuilder.Entity<FoodLover>(entity =>
            {
                entity.ToTable("FoodLover");
                entity.HasKey(e => e.foodlover_id);
                entity.Property(e => e.name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.gender).IsRequired();
                entity.Property(e => e.contact_number).IsRequired().HasMaxLength(15);
                entity.Property(e => e.address).IsRequired();
                entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configure Chef entity
            modelBuilder.Entity<Chef>(entity =>
            {
                entity.ToTable("Chef");
                entity.HasKey(e => e.chef_id);
                entity.Property(e => e.name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.gender).IsRequired().HasMaxLength(10);
                entity.Property(e => e.contact_number).IsRequired().HasMaxLength(15);
                entity.Property(e => e.address).IsRequired();
                entity.Property(e => e.experience).IsRequired();
                entity.Property(e => e.image).HasMaxLength(255);
                entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configure Admin entity
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("Admin");
                entity.HasKey(e => e.admin_id);
                entity.Property(e => e.name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configure Recipe entity
            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.ToTable("Recipe");
                entity.HasKey(r => r.recipe_id);
                entity.Property(r => r.title).IsRequired().HasMaxLength(100);
                entity.Property(r => r.description).IsRequired();
                entity.Property(r => r.ingredients).IsRequired();
                entity.Property(r => r.instructions).IsRequired();
                entity.Property(r => r.rating).HasDefaultValue(0.00m);
                entity.Property(r => r.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Configure relationships
                entity.HasOne(r => r.Chef)
                    .WithMany(c => c.Recipes)
                    .HasForeignKey(r => r.chef_id)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.FoodLover)
                    .WithMany()
                    .HasForeignKey(r => r.foodlover_id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Review entity
            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("reviews");
                entity.HasKey(r => r.review_id);
                entity.Property(r => r.rating).IsRequired();
                entity.Property(r => r.comment).IsRequired();
                entity.Property(r => r.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Configure relationships
                entity.HasOne(r => r.Recipe)
                    .WithMany(r => r.Reviews)
                    .HasForeignKey(r => r.recipe_id)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.FoodLover)
                    .WithMany()
                    .HasForeignKey(r => r.foodlover_id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure FavoriteRecipe entity
            modelBuilder.Entity<FavoriteRecipe>(entity =>
            {
                entity.HasKey(e => e.FavoriteRecipeId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Configure foreign keys
                entity.HasOne(e => e.FoodLover)
                      .WithMany()
                      .HasForeignKey(e => e.FoodLoverId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Recipe)
                      .WithMany()
                      .HasForeignKey(e => e.RecipeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
} 