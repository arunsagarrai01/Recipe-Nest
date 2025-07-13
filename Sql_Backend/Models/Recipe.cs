using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sql_Backend.Models
{
    [Table("Recipe")]  // Specify exact case-sensitive table name
    public class Recipe
    {
        [Key]
        public int recipe_id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string title { get; set; } = string.Empty;
        
        [Required]
        public string description { get; set; } = string.Empty;
        
        [Required]
        public string ingredients { get; set; } = string.Empty;
        
        [Required]
        public string instructions { get; set; } = string.Empty;
        
        public string? image { get; set; }
        
        [Column(TypeName = "decimal(3,2)")]
        public decimal rating { get; set; }
        
        public int chef_id { get; set; }
        
        public int? foodlover_id { get; set; }
        
        [Required]
        [StringLength(20)]
        public string difficulty_level { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string cuisine_type { get; set; } = string.Empty;
        
        [Required]
        public int cooking_time { get; set; }
        
        [Required]
        public int servings { get; set; }
        
        public DateTime created_at { get; set; }
        
        // Navigation properties
        [ForeignKey("chef_id")]
        public Chef? Chef { get; set; }
        
        [ForeignKey("foodlover_id")]
        public FoodLover? FoodLover { get; set; }
        
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
} 