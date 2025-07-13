using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sql_Backend.Models
{
    public class Chef
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int chef_id { get; set; }
        
        [MaxLength(255)]
        public string? image { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string password { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(10)]
        public string gender { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(15)]
        public string contact_number { get; set; } = string.Empty;
        
        [Required]
        public string address { get; set; } = string.Empty;
        
        [Required]
        public int experience { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime created_at { get; set; }
        
        public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    }
} 