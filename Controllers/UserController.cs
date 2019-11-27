using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpdrachtAPI.Models;
using OpdrachtAPI.Services;

namespace OpdrachtAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private IUserService _userService;
        private readonly WebpollContext _context;
        public UserController(IUserService userService, WebpollContext context)
        {
            _userService = userService;
            _context = context;
        }

        // Deze functie Zorgt voor de authenticatie bij het inloggen van het project
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]User userParam)
        {
            var user = _userService.Authenticate(userParam.UserName, userParam.Password);

            if (user == null)
            {
                return BadRequest(new { message = "Username or password is incorrect" });
            }

            return Ok(user);
       }

        // Deze functie kijkt of de username al bestaat bij het registreren van de user.
        [HttpGet("validateUserNaam/{userName}")]
        public async Task<ActionResult<IEnumerable<User>>> validateUserNaam(string userName)
        {
            var dubelleUser = await _context.Users.Where(a => a.UserName.Equals(userName)).SingleOrDefaultAsync();
            if (dubelleUser != null)
            {
               return Ok("Gebruikersnaam bestaat al");
            }

            return Ok();
        }


        // Dere functie vraagt alle users op. Deze functie wordt niet gebruikt.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {

            return await _context.Users.ToListAsync();
        }

        // Deze functie haalt de user op met de ingegeven Id. Deze functie wordt niet gebruikt.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // Deze functie update een user met de ingegeven Id.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser([FromRoute] int id, [FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.UserID)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // Deze functie post een user en een vriend.
        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

         

            var vriend = new Vriend();
            vriend.Email = user.Email;
            vriend.UserName = user.UserName;


            _context.Vriend.Add(vriend);
            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.UserID }, user);
        }

        // Deze functie delete een user met de ingegeven Id. Deze functie wordt niet gebruikt
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }


        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }
    }
}