using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class RegisterModel
{
    [Required(ErrorMessage = "Please select a role")]
    public string Role { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your name")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a password")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select your gender")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your contact number")]
    [MaxLength(15, ErrorMessage = "Contact number cannot exceed 15 characters")]
    public string ContactNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your address")]
    public string Address { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    public int? Experience { get; set; }
} 