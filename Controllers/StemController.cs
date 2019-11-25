using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpdrachtAPI.Models;

namespace OpdrachtAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StemController : ControllerBase
    {
        private readonly WebpollContext _context;

        public StemController(WebpollContext context)
        {
            _context = context;
        }

        // GET: api/Stem
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stem>>> GetStem()
        {
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;
            return await _context.Stem.ToListAsync();
        }

        [HttpGet("getStemmenPerAntwoord/{antwoordID}")]
        public async Task<ActionResult<IEnumerable<Stem>>> GetStemmenPerAntwoord([FromRoute] long antwoordID)
        {
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;

            List<Stem> stemmen = await _context.Stem.Where(s => s.AntwoordID == antwoordID).ToListAsync();


            return stemmen;
        }

        // GET: api/Stem/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStem([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var stem = await _context.Stem.FindAsync(id);

            if (stem == null)
            {
                return NotFound();
            }

            return Ok(stem);
        }

        // PUT: api/Stem/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStem([FromRoute] long id, [FromBody] Stem stem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != stem.StemID)
            {
                return BadRequest();
            }

            _context.Entry(stem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StemExists(id))
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

        // POST: api/Stem
        [HttpPost]
        public async Task<IActionResult> PostStem([FromBody] long antwoordID)
        {
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Stem stem = new Stem();
            stem.AntwoordID = antwoordID;
            stem.UserID = long.Parse(userID);



            _context.Stem.Add(stem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStem", new { id = stem.StemID }, stem);
        }

        // DELETE: api/Stem/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStem([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var stem = await _context.Stem.FindAsync(id);
            if (stem == null)
            {
                return NotFound();
            }

            _context.Stem.Remove(stem);
            await _context.SaveChangesAsync();

            return Ok(stem);
        }

        private bool StemExists(long id)
        {
            return _context.Stem.Any(e => e.StemID == id);
        }
    }
}