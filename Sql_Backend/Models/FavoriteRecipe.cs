using System;
using System.ComponentModel.DataAnnotations;

namespace Sql_Backend.Models
{
    public class FavoriteRecipe
    {
        [Key]
        public int FavoriteRecipeId { get; set; }
        
        public int FoodLoverId { get; set; }
        public FoodLover? FoodLover { get; set; }
        
        public int RecipeId { get; set; }
        public Recipe? Recipe { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
} 