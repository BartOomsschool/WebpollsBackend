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
    public class VriendUserController : ControllerBase
    {
        private readonly WebpollContext _context;

        public VriendUserController(WebpollContext context)
        {
            _context = context;
        }

        // GET: api/VriendUser
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VriendUser>>> GetVriendUser()
        {
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;
            return await _context.VriendUser.ToListAsync();
        }

        // GET: api/VriendUser/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVriendUser([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vriendUser = await _context.VriendUser.FindAsync(id);

            if (vriendUser == null)
            {
                return NotFound();
            }

            return Ok(vriendUser);
        }

        // PUT: api/VriendUser/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVriendUser([FromRoute] long id, [FromBody] VriendUser vriendUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != vriendUser.VriendUserID)
            {
                return BadRequest();
            }

            _context.Entry(vriendUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VriendUserExists(id))
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

        // POST: api/VriendUser
        [HttpPost]
        public async Task<IActionResult> PostVriendUser([FromBody] long vriend)
        {
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            VriendUser vriendUser = new VriendUser();

            vriendUser.UserID = long.Parse(userID);
            vriendUser.VriendID = vriend;

            _context.VriendUser.Add(vriendUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVriendUser", new { id = vriendUser.VriendUserID }, vriendUser);
        }

        // DELETE: api/VriendUser/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVriendUser([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vriendUser = await _context.VriendUser.FindAsync(id);
            if (vriendUser == null)
            {
                return NotFound();
            }

            _context.VriendUser.Remove(vriendUser);
            await _context.SaveChangesAsync();

            return Ok(vriendUser);
        }

        private bool VriendUserExists(long id)
        {
            return _context.VriendUser.Any(e => e.VriendUserID == id);
        }
    }
}