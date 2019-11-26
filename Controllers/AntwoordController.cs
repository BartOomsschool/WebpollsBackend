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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class AntwoordController : ControllerBase
    {
        private readonly WebpollContext _context;

        public AntwoordController(WebpollContext context)
        {
            _context = context;
        }


        // Deze functie vraagt alle antwoorden op. Deze functie wordt niet gebruikt.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Antwoord>>> GetAntwoord()
        {
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;
            return await _context.Antwoord.ToListAsync();
        }

        // Deze functie vraagt alle stemmen op van een poll en geeft het aanal stemmen terug van ieder antwoord.
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<Antwoord>>> GetAntwoordenPoll(long id)
        {
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;            
            var list = await _context.Antwoord.Where(i => i.PollID == id).ToListAsync();

            foreach(var a in list)
            {
           
                var stemLijst = await _context.Stem.Where(s => s.AntwoordID == a.AntwoordID).ToListAsync();
                a.AantalGestemd = stemLijst.Count();
                
            }

            return list;
        }

        // Deze functie doet een update wanneer er een antwoord verandert. Deze functie wordt niet gebruikt
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAntwoord([FromRoute] long id, [FromBody] Antwoord antwoord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != antwoord.AntwoordID)
            {
                return BadRequest();
            }

            _context.Entry(antwoord).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AntwoordExists(id))
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

        // Deze functie voegt een antwoord toe.
        [HttpPost]
        public async Task<IActionResult> PostAntwoord([FromBody] Antwoord antwoord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Antwoord.Add(antwoord);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAntwoord", new { id = antwoord.AntwoordID }, antwoord);
        }

        // Deze functie delete het antwoord met de ingegeven Id.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAntwoord([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var antwoord = await _context.Antwoord.FindAsync(id);
            if (antwoord == null)
            {
                return NotFound();
            }

            _context.Antwoord.Remove(antwoord);
            await _context.SaveChangesAsync();

            return Ok(antwoord);
        }

        private bool AntwoordExists(long id)
        {
            return _context.Antwoord.Any(e => e.AntwoordID == id);
        }
    }
}