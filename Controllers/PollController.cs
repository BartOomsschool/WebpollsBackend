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
    public class PollController : ControllerBase
    {
        private readonly WebpollContext _context;

        public PollController(WebpollContext context)
        {
            _context = context;
        }

        // Deze functie haalt alle polls op van de gebruiker en kijkt of de gebruiker al gestemd heeft op de poll of niet.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Poll>>> GetPoll()
        {
            List<long> pollsID = new List<long>();
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;

             List<PollUser> Ids = await _context.PollUser.Where(a => a.UserID == long.Parse(userID)).ToListAsync();

            foreach (PollUser id in Ids)
            {
                pollsID.Add(id.PollID);
            }
                var list = await _context.Poll.Where(i => pollsID.Contains(i.PollID)).OrderByDescending(o => o.PollID).ToListAsync();

            foreach (var p in list)
            {
                var pollUser = await _context.PollUser.Where(pu => pu.UserID == long.Parse(userID)).Where(pu => pu.PollID == p.PollID).SingleOrDefaultAsync();

                if(pollUser.gestemd)
                {
                    p.Gestemd = true;
                }
            }

            return list;
        }

        // Deze functie haalt de poll op met de ingegeven Id.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPoll([FromRoute] long id)
        {
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var poll = await _context.Poll.FindAsync(id);

            if (poll == null)
            {
                return NotFound();
            }

            return Ok(poll);
        }

        // Deze functie update de poll met ingegeven Id en zet gestemd van de pollUser op true.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPoll([FromRoute] long id, [FromBody] Poll poll)
        {
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != poll.PollID)
            {
                return BadRequest();
            }

            var pollUser = await _context.PollUser.Where(p => p.PollID == id).Where(p => p.UserID == long.Parse(userID)).SingleOrDefaultAsync();
            pollUser.gestemd = true;
            _context.Entry(pollUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PollExists(id))
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

        // Deze functie maakt een nieuwe poll aan en ze admin van de poll op true.
        [HttpPost]
        public async Task<IActionResult> PostPoll([FromBody] Poll poll)
        {
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pollUser = new PollUser();
            pollUser.Poll = poll;
            pollUser.UserID = long.Parse(userID);
            pollUser.admin = true;
            _context.PollUser.Add(pollUser);

            await _context.SaveChangesAsync();

        

            return CreatedAtAction("GetPoll", new { id = poll.PollID }, poll);
        }


        // Deze functie delete de poll met ingegeven Id wanneer de user admin is van de poll. Ook de antwoorden en stemmen van de poll worden verwijderd.
        // Indien de user geen admin is van de poll dan word alleen de user verwijdert van de poll
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePoll([FromRoute] long id)
        {
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var poll = await _context.Poll.FindAsync(id);
            var pollUser = await _context.PollUser.Where(pu => pu.UserID == long.Parse(userID)).Where(pu => pu.PollID == id).SingleOrDefaultAsync();

            if (pollUser.admin)
            {
                List<Stem> stemmen = new List<Stem>();
                var antwoorden = await _context.Antwoord.Where(a => a.PollID == id).ToListAsync();
                var pollUsers = await _context.PollUser.Where(pu => pu.PollID == id).ToListAsync();

                foreach(var a in antwoorden)
                {
                    stemmen.AddRange(await _context.Stem.Where(s => s.AntwoordID == a.AntwoordID).ToListAsync());
                }

                foreach(Stem s in stemmen)
                {
                    _context.Stem.Remove(s);
                }

                foreach(var a in antwoorden)
                {
                    _context.Antwoord.Remove(a);
                }

                foreach(var pu in pollUsers)
                {
                    _context.PollUser.Remove(pu);
                }

                _context.Poll.Remove(poll);
            } 
            else
            {
                _context.PollUser.Remove(pollUser);
            }

        
         
            if (poll == null)
            {
                return NotFound();
            }
            
            await _context.SaveChangesAsync();

            return Ok(poll);
        }

        private bool PollExists(long id)
        {
            return _context.Poll.Any(e => e.PollID == id);
        }
    }
}