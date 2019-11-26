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

        // Deze functie haalt alle stemment op. Deze functie wordt niet gebruikt.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stem>>> GetStem()
        {
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;
            return await _context.Stem.ToListAsync();
        }

        // Deze functie haalt alle stemmen op met het ingegeven antwoordID.
        [HttpGet("getStemmenPerAntwoord/{antwoordID}")]
        public async Task<ActionResult<IEnumerable<Stem>>> GetStemmenPerAntwoord([FromRoute] long antwoordID)
        {
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;

            List<Stem> stemmen = await _context.Stem.Where(s => s.AntwoordID == antwoordID).ToListAsync();

            return stemmen;
        }

        // Deze functie haalt de stem op met de ingegeven Id.
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

        // Deze functie update de stem met de ingegeven Id. Deze functie wordt niet gebruikt.
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

        // Deze functie post een stem met de ingegeven antwoordID.
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

        // Deze functie delete een stem met de ingegeven id.
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