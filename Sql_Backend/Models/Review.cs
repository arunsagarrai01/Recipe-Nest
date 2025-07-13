using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sql_Backend.Models
{
    [Table("reviews")]
    public class Review
    {
        [Key]
        public int review_id { get; set; }

        [Required]
        public int recipe_id { get; set; }

        [Required]
        public int? foodlover_id { get; set; }

        [Required]
        [Column(TypeName = "decimal(3,2)")]
        public decimal rating { get; set; }

        [Required]
        public string comment { get; set; } = string.Empty;

        public DateTime created_at { get; set; }

        // Navigation properties
        [ForeignKey("recipe_id")]
        public Recipe? Recipe { get; set; }

        [ForeignKey("foodlover_id")]
        public FoodLover? FoodLover { get; set; }
    }
} 