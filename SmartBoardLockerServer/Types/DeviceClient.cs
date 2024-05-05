using System.Net.WebSockets;

namespace SmartBoardLockerServer.Types
{
    public class DeviceClient
    {
        public string ClassName { get; set; }
        public WebSocket WebSocket { get; set; }
        public string ID { get; set; }

        public DeviceClient(string classname, WebSocket ws, string id) { 
            ClassName = classname;
            WebSocket = ws;
            ID = id;
        }
    }
}
