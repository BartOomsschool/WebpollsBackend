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
    public class VriendController : ControllerBase
    {
        private readonly WebpollContext _context;

        public VriendController(WebpollContext context)
        {
            _context = context;
        }

        // GET: api/Vriend
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vriend>>> GetVriend()
        {          
            //Nog te doen zorgen dat de persoon zelf niet in de vriendenlijst komt
            List<long> vriendIDs = new List<long>();
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;

            List<VriendUser> allePersonen = await _context.VriendUser.Where(a => a.UserID == long.Parse(userID)).Where(a => a.Status).ToListAsync();

            foreach (VriendUser persoon in allePersonen)
            {
                vriendIDs.Add(persoon.VriendID);
            }
           
            return await _context.Vriend.Where(i => vriendIDs.Contains(i.VriendID)).ToListAsync();
        }


        [HttpGet("getNietVrienden")]
         public async Task<ActionResult<IEnumerable<Vriend>>> GetNietVriend()
         {
            int teller;
            List<long> vriendIDs = new List<long>();
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;
            var userName = User.Claims.FirstOrDefault(c => c.Type == "UserName").Value;
            List<Vriend> allePersonen = await _context.Vriend.ToListAsync();
            List<VriendUser> vrienden = await _context.VriendUser.Where(a => a.UserID == long.Parse(userID)).ToListAsync();

            foreach (Vriend persoon in allePersonen)
            {
                teller = 0;
                if (persoon.UserName.Equals(userName))
                {
                    teller++;
                }
                foreach(VriendUser vriend in vrienden) {
                    if (persoon.VriendID == vriend.VriendID)
                    {
                        teller++;
                    }
                }
                if (teller == 0)
                {
                    vriendIDs.Add(persoon.VriendID);
                }                                    
            }

            return await _context.Vriend.Where(i => vriendIDs.Contains(i.VriendID)).ToListAsync();
         }

        [HttpGet("getVerzoeken")]
        public async Task<ActionResult<IEnumerable<Vriend>>> GetVerzoek()
        {
            List<Vriend> verzoekenLijst = new List<Vriend>();
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;
            var user = await _context.Users.Where(u => u.UserID == long.Parse(userID)).SingleOrDefaultAsync();
            var vriend = await _context.Vriend.Where(v => v.UserName == user.UserName).SingleOrDefaultAsync();
            List<VriendUser> verzoeken = await _context.VriendUser.Where(v => v.VriendID == vriend.VriendID).Where(s => s.Status == false).Where(u => u.UserID != user.UserID).ToListAsync();
            List<User> users = new List<User>();
            foreach (VriendUser persoon in verzoeken)
            {
                users.Add(await _context.Users.Where(u => u.UserID == persoon.UserID).SingleOrDefaultAsync());          
            }

            foreach(User persoon in users)
            {
                verzoekenLijst.Add(await _context.Vriend.Where(v => v.UserName == persoon.UserName).SingleOrDefaultAsync());
            }



            return verzoekenLijst;
        }

     /*   [HttpGet("{getVriendenZonderPoll}")]
        public async Task<ActionResult<IEnumerable<Vriend>>> GetvriendenZonderPoll()
        {
            //Nog te doen zorgen dat de persoon zelf niet in de vriendenlijst komt
            List<long> vriendIDs = new List<long>();
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;
            int teller;
            List<VriendUser> vriendenUsers = await _context.VriendUser.Where(a => a.UserID == long.Parse(userID)).Where(a => a.Status == false).ToListAsync();

            foreach (VriendUser persoon in vriendenUsers)
            {
                vriendIDs.Add(persoon.VriendID);
            }

            List<Vriend> vrienden = await _context.Vriend.Where(i => vriendIDs.Contains(i.VriendID)).ToListAsync();
            List<User> users = new List<User>();
            List<User> usersZonderPoll = new List<User>();
            List<Vriend> vriendenZonderPoll = new List<Vriend>();

            foreach (Vriend vriend in vrienden)
            {
                users.Add(await _context.Users.Where(u => u.UserName == vriend.UserName).SingleOrDefaultAsync());
            }

            List<PollUser> usersMetPoll = await _context.PollUser.ToListAsync();

            foreach (User user in users)
            {
                teller = 0;
                foreach (PollUser pollUser in usersMetPoll)
                {
                    if (pollUser.UserID == user.UserID)
                    {
                        teller++;
                    }
                }
                if (teller == 1)
                {
                    usersZonderPoll.Add(user);
                }
            }

            foreach(User user in usersZonderPoll)
            {
                vriendenZonderPoll.Add(await _context.Vriend.Where(v => v.UserName == user.UserName).SingleOrDefaultAsync());
            }


            return vriendenZonderPoll;
        }
*/


        // GET: api/Vriend/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVriend([FromRoute] long id)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vriend = await _context.Vriend.FindAsync(id);

            if (vriend == null)
            {
                return NotFound();
            }

            return Ok(vriend);
        }

        // PUT: api/Vriend/5
        [HttpPut("accepteerVerzoek/{vriendID}")]
        public async Task<IActionResult> UpdateVriend([FromRoute] long vriendID, [FromBody] Vriend vriend)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (vriendID != vriend.VriendID)
            {
                return BadRequest();
            }
            var user = await _context.Users.Where(u => u.UserName == vriend.UserName).SingleOrDefaultAsync();
            var userName = User.Claims.FirstOrDefault(c => c.Type == "UserName").Value;
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;
            var ikZelf = await _context.Vriend.Where(v => v.UserName == userName).SingleOrDefaultAsync();


            var vriendUser = await _context.VriendUser.Where(v => v.UserID == user.UserID).Where(s => s.VriendID == ikZelf.VriendID).SingleOrDefaultAsync();

            vriendUser.Status = true;

          var vriendUser2 = new VriendUser();
            vriendUser2.VriendID = vriend.VriendID; 
            vriendUser2.UserID = long.Parse(userID);
            vriendUser2.Status = true;
            _context.VriendUser.Add(vriendUser2);
            await _context.SaveChangesAsync();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VriendExists(vriendID))
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
        // PUT: api/Vriend/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVriend([FromRoute] long id, [FromBody] Vriend vriend)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != vriend.VriendID)
            {
                return BadRequest();
            }

            _context.Entry(vriend).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VriendExists(id))
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

        // POST: api/Vriend
        [HttpPost]
        public async Task<IActionResult> PostVriend([FromBody] Vriend vriend)
        {
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vriendUser = new VriendUser();
            vriendUser.Vriend = vriend;
            vriendUser.UserID = long.Parse(userID);
            vriendUser.Status = false;
            _context.VriendUser.Add(vriendUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVriend", new { id = vriend.VriendID }, vriend);
        }

        // DELETE: api/Vriend/5
        [HttpDelete("verwijderVerzoek/{vriendID}")]
        public async Task<IActionResult> DeleteVerzoekVriend([FromRoute] long vriendID)
        {
            var userName = User.Claims.FirstOrDefault(c => c.Type == "UserName").Value;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vriend = await _context.Vriend.Where(v => v.UserName == userName).SingleOrDefaultAsync();
            var vriend2 = await _context.Vriend.Where(v => v.VriendID == vriendID).SingleOrDefaultAsync();
            var vriend2User = await _context.Users.Where(u => u.UserName == vriend2.UserName).SingleOrDefaultAsync();
            var vriendUser = await _context.VriendUser.Where(p => p.VriendID == vriend.VriendID).Where(p => p.UserID == vriend2User.UserID).SingleOrDefaultAsync();

            if (vriendUser == null)
            {
                return NotFound();
            }
            _context.VriendUser.Remove(vriendUser);
            await _context.SaveChangesAsync();

            return Ok(vriendUser);
        }

        // DELETE: api/Vriend/5
        [HttpDelete("{vriendID}")]
        public async Task<IActionResult> DeleteVriend([FromRoute] long vriendID)
        {
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;
            var userName = User.Claims.FirstOrDefault(c => c.Type == "UserName").Value;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vriend = await _context.Vriend.Where(v => v.VriendID == vriendID).SingleOrDefaultAsync();
            var userVriend = await _context.Users.Where(u => u.UserName == vriend.UserName).SingleOrDefaultAsync();
            var ikZelf = await _context.Vriend.Where(v => v.UserName == userName).SingleOrDefaultAsync();

            var vriendUser = await _context.VriendUser.Where(p => p.VriendID == vriendID).Where(p => p.UserID == long.Parse(userID)).SingleOrDefaultAsync();
            var vriendUser2 = await _context.VriendUser.Where(v => v.VriendID == ikZelf.VriendID).Where(u => u.UserID == userVriend.UserID).SingleOrDefaultAsync();
           // var vriend = await _context.Vriend.FindAsync(id);
            if (vriendUser == null)
            {
                return NotFound();
            }
            _context.VriendUser.Remove(vriendUser);

            if (vriendUser2 != null)
            {
                _context.VriendUser.Remove(vriendUser2);
            }
            await _context.SaveChangesAsync();

            return Ok(vriendUser);
        }

        private bool VriendExists(long id)
        {
            return _context.Vriend.Any(e => e.VriendID == id);
        }
    }
}