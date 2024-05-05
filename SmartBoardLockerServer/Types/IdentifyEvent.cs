namespace SmartBoardLockerServer.Types
{
    public class IdentifyEvent
    {
        public EventTypes id { get; set; }
        public required IdentifyEventData data { get; set; }
    }

    public class IdentifyEventData
    {
        public required string className { get; set; }
    }
}
