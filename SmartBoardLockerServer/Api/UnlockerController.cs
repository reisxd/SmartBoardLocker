using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SmartBoardLockerServer.Types;

namespace SmartBoardLockerServer.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnlockerController : ControllerBase
    {
        
        [HttpGet("[action]")]
        [Authorize]
        public IActionResult Unlock([FromQuery(Name = "code")] string code)
        {
            
            DeviceClient client = Program.clients.Find(x => x.ID == code);
            
            if (client != null)
            {
                
                Program.SendMessage(JsonConvert.SerializeObject(new BasicWSEvent(EventTypes.Unlock)), client.WebSocket);
                
                Program.clients.RemoveAt(Program.clients.FindIndex(x => x.ID == code));
                
                return Ok(true);
            } 
            
            else return NotFound(false);
        }
    }
}