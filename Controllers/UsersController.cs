using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using ZENITH_TEST.CORE.Models;
using ZENITH_TEST.INFRASTRUCTURE.Services.User.Interfaces;

namespace ZENITH_TEST.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            try
            {
                var newUser = await _userService.CreateUser(user);
                return CreatedAtAction(nameof(GetUserById), JsonConvert.SerializeObject(new { responseMessage = newUser }), newUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet]
        public async Task<ActionResult<User>> GetUserByUsername(string username)
        {
            var user = await _userService.GetUserByUsername(username);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(User user)
        {
            var result = await _userService.UpdateUser(user);
            if (result)
            {
                return NoContent();
            }
            return NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUser(id);
            if (result)
            {
                return NoContent();
            }
            return NotFound();
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            var result = await _userService.ActivateUser(id);
            return Ok(result);
        }
    }
}
