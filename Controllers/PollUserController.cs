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
    public class PollUserController : ControllerBase
    {
        private readonly WebpollContext _context;

        public PollUserController(WebpollContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<PollUser>>> GetPollUser()
        {
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;
            return await _context.PollUser.ToListAsync();
        }

       [HttpGet("getPollVrienden")]
        public async Task<ActionResult<IEnumerable<Vriend>>> GetPollVrienden(long pollID)
        {
            List<PollUser> pollVrienden = await _context.PollUser.Where(p => p.PollID == pollID).ToListAsync();
            List<VriendUser> vrienden = new List<VriendUser>();
            List<Vriend> vriendenLijst = new List<Vriend>();

            foreach (PollUser pollVriend in pollVrienden)
            {
                vrienden.Add(await _context.VriendUser.Where(u => u.UserID == pollVriend.UserID).SingleOrDefaultAsync());
            }

            foreach(VriendUser vriendUser in vrienden)
            {
                vriendenLijst.Add(await _context.Vriend.Where(u => u.VriendID == vriendUser.VriendID).SingleOrDefaultAsync());
            }

            return vriendenLijst;
        }



        // GET: api/PollGebruiker/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPollUser([FromRoute] long id)
        {
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pollGebruiker = await _context.PollUser.FindAsync(id);

            if (pollGebruiker == null)
            {
                return NotFound();
            }

            return Ok(pollGebruiker);
        }

        // PUT: api/PollGebruiker/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPollUser([FromRoute] long id, [FromBody] PollUser pollUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != pollUser.PollUserID)
            {
                return BadRequest();
            }

            _context.Entry(pollUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PollUserExists(id))
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

        // POST: api/PollGebruiker
        [HttpPost]
        public async Task<IActionResult> PostPollUser([FromBody] PollUser pollUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Vriend vriend = await _context.Vriend.Where(v => v.VriendID == pollUser.UserID).SingleOrDefaultAsync();
            User user = await _context.Users.Where(u => u.UserName == vriend.UserName).SingleOrDefaultAsync();

            PollUser juistPollUser = new PollUser();
            juistPollUser.PollID = pollUser.PollID;
            juistPollUser.UserID = user.UserID;

            _context.PollUser.Add(juistPollUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPollUser", new { id = juistPollUser.PollUserID }, juistPollUser);
        }

        // DELETE: api/PollGebruiker/5
        [HttpDelete("verwijderUserVanPoll/{vriendID}/{pollID}")]
        public async Task<IActionResult> DeletePollUserVanPoll([FromRoute] long vriendID, long pollID)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var vriend = await _context.Vriend.Where(v => v.VriendID == vriendID).SingleOrDefaultAsync();
            var user = await _context.Users.Where(u => u.UserName == vriend.UserName).SingleOrDefaultAsync();
            var pollUser = await _context.PollUser.Where(p => p.UserID == user.UserID).Where(p => p.PollID == pollID).SingleOrDefaultAsync();

            if (pollUser == null)
            {
                return NotFound();
            }

            _context.PollUser.Remove(pollUser);
            await _context.SaveChangesAsync();

            return Ok(pollUser);
        }

        // DELETE: api/PollGebruiker/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePollUser([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pollUser = await _context.PollUser.FindAsync(id);
            if (pollUser == null)
            {
                return NotFound();
            }

            _context.PollUser.Remove(pollUser);
            await _context.SaveChangesAsync();

            return Ok(pollUser);
        }

        private bool PollUserExists(long id)
        {
            return _context.PollUser.Any(e => e.PollUserID == id);
        }
    }
}