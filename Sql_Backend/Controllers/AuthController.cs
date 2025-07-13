using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sql_Backend.DAL;
using Sql_Backend.Models;
using System.Threading.Tasks;
using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace Sql_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AuthController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterModel model)
        {
            try
            {
                Console.WriteLine($"Registration attempt for email: {model.Email}, role: {model.Role}");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    Console.WriteLine($"Validation errors: {string.Join(", ", errors)}");
                    return BadRequest(new { message = "Validation failed", errors = errors });
                }

                // Check if email already exists
                if (await _context.FoodLovers.AnyAsync(u => u.email == model.Email) ||
                    await _context.Chefs.AnyAsync(u => u.email == model.Email))
                {
                    Console.WriteLine($"Email already exists: {model.Email}");
                    return BadRequest(new { message = "Email already exists" });
                }

                // Hash password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                if (model.Role.ToLower() == "foodlover")
                {
                    var foodLover = new FoodLover
                    {
                        name = model.Name,
                        email = model.Email,
                        password = hashedPassword,
                        gender = model.Gender,
                        contact_number = model.ContactNumber,
                        address = model.Address,
                        created_at = DateTime.Now
                    };

                    _context.FoodLovers.Add(foodLover);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"FoodLover registered successfully: {model.Email}");
                    return Ok(new { message = "FoodLover registered successfully" });
                }
                else if (model.Role.ToLower() == "chef")
                {
                    // Validate chef-specific fields
                    if (!model.Experience.HasValue)
                    {
                        return BadRequest(new { message = "Experience is required for chefs" });
                    }

                    string? imagePath = null;
                    if (model.Image != null)
                    {
                        // Save the image and get the path
                        imagePath = await SaveImage(model.Image);
                        if (string.IsNullOrEmpty(imagePath))
                        {
                            Console.WriteLine("Failed to save profile image, continuing without image");
                        }
                    }

                    // Create chef with image path
                    var chef = new Chef
                    {
                        name = model.Name,
                        email = model.Email,
                        password = hashedPassword,
                        gender = model.Gender,
                        contact_number = model.ContactNumber,
                        address = model.Address,
                        image = imagePath,
                        experience = model.Experience.Value,
                        created_at = DateTime.Now
                    };

                    try
                    {
                        _context.Chefs.Add(chef);
                        await _context.SaveChangesAsync();
                        return Ok(new { message = "Chef registered successfully" });
                    }
                    catch (DbUpdateException ex)
                    {
                        Console.WriteLine($"Database error while saving chef: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                        }
                        return BadRequest(new { message = "Failed to save chef data to database" });
                    }
                }
                else
                {
                    Console.WriteLine($"Invalid role: {model.Role}");
                    return BadRequest(new { message = "Invalid role" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return BadRequest(new { message = "Registration failed", error = ex.Message });
            }
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    Console.WriteLine("No image file provided");
                    return null;
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    Console.WriteLine($"Invalid file type: {fileExtension}");
                    return null;
                }

                // Get the wwwroot path
                var webRootPath = _environment.WebRootPath;
                if (string.IsNullOrEmpty(webRootPath))
                {
                    webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                // Ensure wwwroot directory exists
                if (!Directory.Exists(webRootPath))
                {
                    Directory.CreateDirectory(webRootPath);
                    Console.WriteLine($"Created wwwroot directory at: {webRootPath}");
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(webRootPath, "uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                    Console.WriteLine($"Created uploads directory at: {uploadsPath}");
                }

                // Generate a unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                Console.WriteLine($"Saving image to: {filePath}");

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                // Return the relative path
                var relativePath = $"/uploads/{uniqueFileName}";
                Console.WriteLine($"Image saved successfully at: {relativePath}");
                return relativePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving image: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return null;
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                Console.WriteLine($"Login attempt for email: {model.Email}");

                if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest(new { message = "Email and password are required" });
                }

                // Check FoodLovers table
                var foodLover = await _context.FoodLovers
                    .FirstOrDefaultAsync(f => f.email == model.Email);

                if (foodLover != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(model.Password, foodLover.password))
                    {
                        Console.WriteLine($"FoodLover login successful: {model.Email}");
                        return Ok(new
                        {
                            token = "foodlover-token",
                            role = "foodlover",
                            userId = foodLover.foodlover_id,
                            name = foodLover.name,
                            email = foodLover.email
                        });
                    }
                    else
                    {
                        Console.WriteLine($"Invalid password for FoodLover: {model.Email}");
                        return BadRequest(new { message = "Invalid password" });
                    }
                }

                // Check Chefs table
                var chef = await _context.Chefs
                    .FirstOrDefaultAsync(c => c.email == model.Email);

                if (chef != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(model.Password, chef.password))
                    {
                        Console.WriteLine($"Chef login successful: {model.Email}");
                        return Ok(new
                        {
                            token = "chef-token",
                            role = "chef",
                            userId = chef.chef_id,
                            name = chef.name,
                            email = chef.email,
                            image = chef.image
                        });
                    }
                    else
                    {
                        Console.WriteLine($"Invalid password for Chef: {model.Email}");
                        return BadRequest(new { message = "Invalid password" });
                    }
                }

                Console.WriteLine($"No user found with email: {model.Email}");
                return BadRequest(new { message = "Invalid email or password" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
            }
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            return Ok(new { message = "API is working" });
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // Get user ID from headers
                var userId = Request.Headers["UserId"].ToString();
                var userRole = Request.Headers["UserRole"].ToString();

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                Console.WriteLine($"Fetching profile for user: {userId}, role: {userRole}");

                if (userRole.ToLower() == "foodlover")
                {
                    var foodLover = await _context.FoodLovers
                        .FirstOrDefaultAsync(u => u.foodlover_id == int.Parse(userId));

                    if (foodLover == null)
                    {
                        return NotFound(new { message = "User not found" });
                    }

                    return Ok(new
                    {
                        name = foodLover.name,
                        email = foodLover.email,
                        gender = foodLover.gender,
                        contactNumber = foodLover.contact_number,
                        address = foodLover.address,
                        role = "foodlover",
                        userId = foodLover.foodlover_id
                    });
                }
                else if (userRole.ToLower() == "chef")
                {
                    var chef = await _context.Chefs
                        .FirstOrDefaultAsync(u => u.chef_id == int.Parse(userId));

                    if (chef == null)
                    {
                        return NotFound(new { message = "User not found" });
                    }

                    return Ok(new
                    {
                        name = chef.name,
                        email = chef.email,
                        gender = chef.gender,
                        contactNumber = chef.contact_number,
                        address = chef.address,
                        image = chef.image,
                        experience = chef.experience,
                        role = "chef",
                        userId = chef.chef_id
                    });
                }

                return BadRequest(new { message = "Invalid user role" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching profile: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching profile", error = ex.Message });
            }
        }
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required")]
        [MaxLength(15, ErrorMessage = "Contact number cannot exceed 15 characters")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        public int? Experience { get; set; }

        public IFormFile? Image { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
} 