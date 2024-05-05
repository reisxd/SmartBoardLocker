namespace SmartBoardLocker.Types
{
    public class IdentifyEvent
    {
        public EventTypes id { get; } = EventTypes.Identify;
        public IdentifyEventData data { get; set; } = new IdentifyEventData();

        public IdentifyEvent(string className)
        {
            data.className = className;
        }
    }

    public class IdentifyEventData
    {
        public string className { get; set; }
    }
}
