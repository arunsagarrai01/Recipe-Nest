using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sql_Backend.DAL;
using Sql_Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Sql_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecipeController> _logger;

        public RecipeController(ApplicationDbContext context, ILogger<RecipeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Recipe
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipes()
        {
            try
            {
                _logger.LogInformation("Fetching all recipes");
                
                var recipes = await _context.Recipe
                    .Include(r => r.Chef)
                    .Include(r => r.Reviews)
                    .Select(r => new
                    {
                        r.recipe_id,
                        r.title,
                        r.description,
                        r.ingredients,
                        r.instructions,
                        image = r.image != null ? $"/uploads/{r.image}" : null,
                        r.rating,
                        r.difficulty_level,
                        r.cuisine_type,
                        r.cooking_time,
                        r.servings,
                        r.created_at,
                        chef = new
                        {
                            r.Chef.chef_id,
                            r.Chef.name,
                            r.Chef.email
                        },
                        reviews = r.Reviews.Select(review => new
                        {
                            review.review_id,
                            review.rating,
                            review.comment,
                            review.created_at
                        }).ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation($"Found {recipes.Count} recipes");
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all recipes");
                return StatusCode(500, "An error occurred while fetching recipes");
            }
        }

        // GET: api/Recipe/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Recipe>> GetRecipe(int id)
        {
            var recipe = await _context.Recipe
                .Include(r => r.Chef)
                .Include(r => r.Reviews)
                .FirstOrDefaultAsync(r => r.recipe_id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            return recipe;
        }

        // GET: api/Recipe/chef/{chefId}
        [HttpGet("chef/{chefId}")]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetChefRecipes(int chefId)
        {
            try
            {
                _logger.LogInformation($"Fetching recipes for chef ID: {chefId}");
                
                // First check if we can connect to the database
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogError("Database connection failed");
                    return StatusCode(500, new { message = "Database connection failed" });
                }

                // Validate chef exists
                var chef = await _context.Chefs.FindAsync(chefId);
                if (chef == null)
                {
                    _logger.LogWarning($"Chef with ID {chefId} not found");
                    return NotFound($"Chef with ID {chefId} not found");
                }

                _logger.LogInformation($"Chef found: {chef.name}");

                // Get recipes with error handling
                var recipes = await _context.Set<Recipe>()
                    .Where(r => r.chef_id == chefId)
                    .Include(r => r.Chef)
                    .Include(r => r.Reviews)
                    .Select(r => new
                    {
                        recipe_id = r.recipe_id,
                        title = r.title ?? "No title",
                        description = r.description ?? "No description",
                        ingredients = r.ingredients ?? "No ingredients",
                        instructions = r.instructions ?? "No instructions",
                        image = r.image != null ? $"/uploads/{r.image}" : null,
                        rating = r.rating,
                        difficulty_level = r.difficulty_level ?? "Not specified",
                        cuisine_type = r.cuisine_type ?? "Not specified",
                        cooking_time = r.cooking_time,
                        servings = r.servings,
                        created_at = r.created_at,
                        chef = new
                        {
                            chef_id = r.Chef.chef_id,
                            name = r.Chef.name ?? "Unknown",
                            email = r.Chef.email ?? "No email"
                        },
                        reviews = r.Reviews.Select(review => new
                        {
                            review_id = review.review_id,
                            rating = review.rating,
                            comment = review.comment ?? "No comment",
                            created_at = review.created_at
                        }).ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation($"Found {recipes.Count} recipes for chef ID: {chefId}");
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching recipes for chef ID: {chefId}. Error details: {ex.Message}\nStack trace: {ex.StackTrace}\nInner exception: {ex.InnerException?.Message}");
                return StatusCode(500, new { 
                    message = "An error occurred while fetching recipes", 
                    error = ex.Message,
                    details = ex.StackTrace,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        // POST: api/Recipe/chef
        [HttpPost("chef")]
        public async Task<ActionResult<Recipe>> CreateChefRecipe()
        {
            try
            {
                if (!Request.HasFormContentType)
                {
                    return BadRequest("Invalid content type. Expected multipart/form-data.");
                }

                var form = await Request.ReadFormAsync();
                
                // Validate required fields
                if (string.IsNullOrEmpty(form["title"]) ||
                    string.IsNullOrEmpty(form["description"]) ||
                    string.IsNullOrEmpty(form["ingredients"]) ||
                    string.IsNullOrEmpty(form["instructions"]) ||
                    string.IsNullOrEmpty(form["cooking_time"]) ||
                    string.IsNullOrEmpty(form["servings"]) ||
                    string.IsNullOrEmpty(form["difficulty_level"]) ||
                    string.IsNullOrEmpty(form["cuisine_type"]) ||
                    string.IsNullOrEmpty(form["chef_id"]))
                {
                    return BadRequest("All required fields must be provided.");
                }

                // Create new recipe
                var recipe = new Recipe
                {
                    title = form["title"].ToString(),
                    description = form["description"].ToString(),
                    ingredients = form["ingredients"].ToString(),
                    instructions = form["instructions"].ToString(),
                    cooking_time = int.Parse(form["cooking_time"].ToString()),
                    servings = int.Parse(form["servings"].ToString()),
                    difficulty_level = form["difficulty_level"].ToString(),
                    cuisine_type = form["cuisine_type"].ToString(),
                    chef_id = int.Parse(form["chef_id"].ToString()),
                    rating = 0.00m,
                    created_at = DateTime.Now
                };

                // Handle file upload
                if (form.Files.Count > 0)
                {
                    var file = form.Files[0];
                    
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest("Invalid file type. Only JPG, JPEG, PNG, and GIF files are allowed.");
                    }

                    // Get the wwwroot path
                    var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    
                    // Create uploads directory if it doesn't exist
                    var uploadsPath = Path.Combine(webRootPath, "uploads");
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    // Generate a unique filename
                    var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadsPath, uniqueFileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    // Store the relative path in the database
                    recipe.image = uniqueFileName;
                }

                // Add to database
                _context.Recipe.Add(recipe);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetRecipe), new { id = recipe.recipe_id }, recipe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recipe");
                return StatusCode(500, new { 
                    message = "An error occurred while creating the recipe", 
                    error = ex.Message 
                });
            }
        }

        // PUT: api/Recipe/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecipe(int id, Recipe recipe)
        {
            if (id != recipe.recipe_id)
            {
                return BadRequest();
            }

            _context.Entry(recipe).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecipeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Recipe/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var recipe = await _context.Recipe.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            _context.Recipe.Remove(recipe);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                // Test database connection
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogError("Database connection failed");
                    return StatusCode(500, new { message = "Database connection failed" });
                }

                // Test Recipe table
                var recipeCount = await _context.Recipe.CountAsync();
                _logger.LogInformation($"Database connection successful. Recipe count: {recipeCount}");

                return Ok(new { 
                    message = "Database connection successful",
                    recipeCount = recipeCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new { 
                    message = "Health check failed",
                    error = ex.Message,
                    details = ex.StackTrace
                });
            }
        }

        private bool RecipeExists(int id)
        {
            return _context.Recipe.Any(e => e.recipe_id == id);
        }
    }
} 