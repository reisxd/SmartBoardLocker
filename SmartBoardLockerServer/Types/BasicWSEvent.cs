namespace SmartBoardLockerServer.Types
{
    public class BasicWSEvent
    {
        public BasicWSEvent(EventTypes id) {
            this.id = id;
        }
        public EventTypes id { get; set; }
    }
}
