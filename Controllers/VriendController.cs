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

        // Deze functie haalt alle vrienden op van de ingelogde user.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vriend>>> GetVriend()
        {          
            List<long> vriendIDs = new List<long>();
            var userID = User.Claims.FirstOrDefault(c => c.Type == "UserID").Value;

            List<VriendUser> allePersonen = await _context.VriendUser.Where(a => a.UserID == long.Parse(userID)).Where(a => a.Status).ToListAsync();

            foreach (VriendUser persoon in allePersonen)
            {
                vriendIDs.Add(persoon.VriendID);
            }
           
            return await _context.Vriend.Where(i => vriendIDs.Contains(i.VriendID)).ToListAsync();
        }

        // Deze functie haalt alle users op die geen vriend zijn van de ingelogde user.
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

        // Deze functie haalt alle vriend verzoeken op van de ingelogde user.
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

        // Deze functie haalt de vriend op met de ingegeven Id. Deze functie wordt niet gebruikt.
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

        // Deze functie update de vriendUser tabel.
        // Wanneer men het verzoek accepteert zal de ingelogde user worden toegevoegd bij de vriend en de vriend wordt toegevoegd bij de ingelogde user.
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
        // Deze update een vriend met de ingegeven Id. Deze functie wordt niet gebruikt.
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

        // Deze functie post een nieuwe vriend.
        // Deze vriend word ook in de vriendUser tabel geplaatst.
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

        // Deze functie update de vriendUser tabel.
        // Wanneer men het verzoek Ignored zal de vriend zijn Id uit de vriendUser tabel verwijdert worden.
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

        // Deze functie verwijdert de vriend van de user en verwijdert ook de user bij de vriend.
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