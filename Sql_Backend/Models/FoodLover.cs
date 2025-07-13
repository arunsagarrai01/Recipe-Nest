using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sql_Backend.Models
{
    public class FoodLover
    {
        [Key]
        public int foodlover_id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string password { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(10)]
        public string gender { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(15)]
        public string contact_number { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string address { get; set; } = string.Empty;
        
        public DateTime created_at { get; set; } = DateTime.Now;
        
        public ICollection<Review>? Reviews { get; set; }
    }
} 