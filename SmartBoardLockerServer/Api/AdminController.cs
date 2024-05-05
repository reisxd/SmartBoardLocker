using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBoardLockerServer.Types;
using Newtonsoft.Json;

namespace SmartBoardLockerServer.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly IWebHostEnvironment environment;
        public AdminController(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        
        [HttpGet("[action]")]
        [Authorize] 
        public IActionResult UnlockBoard([FromQuery] string className)
        {
            
            if (User.IsInRole("User")) return Unauthorized();
            
            if (className == null) return BadRequest(false);
            
            DeviceClient client = Program.clients.Find(x => x.ClassName == className);
            if (client != null)
            {
                
                Program.SendMessage(JsonConvert.SerializeObject(new BasicWSEvent(EventTypes.Unlock)), client.WebSocket);
                
                Program.clients.RemoveAt(Program.clients.FindIndex(x => x.ClassName == className));
                return Ok(true);
            }
            
            else return NotFound(false);
        }

        
        [HttpGet("[action]")]
        [Authorize]
        public IActionResult ShutdownBoard([FromQuery] string className)
        {
            if (User.IsInRole("User")) return Unauthorized();
            if (className == null) return BadRequest(false);
            DeviceClient client = Program.clients.Find(x => x.ClassName == className);
            if (client != null)
            {
                
                Program.SendMessage(JsonConvert.SerializeObject(new RunCommand("shutdown /s /t 0")), client.WebSocket);
                Program.clients.RemoveAt(Program.clients.FindIndex(x => x.ClassName == className));
                return Ok(true);
            }
            else return NotFound(false);
        }

        
        [HttpGet("[action]")]
        [Authorize]
        public IActionResult RestartBoard([FromQuery] string className)
        {
            if (User.IsInRole("User")) return Unauthorized();
            if (className == null) return BadRequest(false);
            DeviceClient client = Program.clients.Find(x => x.ClassName == className);
            if (client != null)
            {
                
                Program.SendMessage(JsonConvert.SerializeObject(new RunCommand("shutdown /r /t 0")), client.WebSocket);
                Program.clients.RemoveAt(Program.clients.FindIndex(x => x.ClassName == className));
                return Ok(true);
            }
            
            else return NotFound(false);
        }

        
        [HttpGet("[action]")]
        [Authorize]
        public IActionResult RunCommand([FromQuery] string className, [FromQuery] string command)
        {
            if (User.IsInRole("User")) return Unauthorized();
            if (className == null || command == null) return BadRequest(false);
            DeviceClient client = Program.clients.Find(x => x.ClassName == className);
            if (client != null)
            {
                
                Program.SendMessage(JsonConvert.SerializeObject(new RunCommand(command)), client.WebSocket);
                return Ok(true);
            }
            else return NotFound(false);
        }
    }
}