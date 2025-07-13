using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sql_Backend.DAL;
using Sql_Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sql_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodLoverController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FoodLoverController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/FoodLover
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FoodLover>>> GetFoodLovers()
        {
            return await _context.FoodLovers.ToListAsync();
        }

        // GET: api/FoodLover/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FoodLover>> GetFoodLover(int id)
        {
            var foodLover = await _context.FoodLovers.FindAsync(id);

            if (foodLover == null)
            {
                return NotFound();
            }

            return foodLover;
        }

        // POST: api/FoodLover
        [HttpPost]
        public async Task<ActionResult<FoodLover>> PostFoodLover(FoodLover foodLover)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.FoodLovers.Add(foodLover);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFoodLover), new { id = foodLover.foodlover_id }, foodLover);
        }

        // PUT: api/FoodLover/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFoodLover(int id, FoodLover foodLover)
        {
            if (id != foodLover.foodlover_id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(foodLover).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FoodLoverExists(id))
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

        // DELETE: api/FoodLover/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFoodLover(int id)
        {
            var foodLover = await _context.FoodLovers.FindAsync(id);
            if (foodLover == null)
            {
                return NotFound();
            }

            _context.FoodLovers.Remove(foodLover);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FoodLoverExists(int id)
        {
            return _context.FoodLovers.Any(e => e.foodlover_id == id);
        }
    }
} 