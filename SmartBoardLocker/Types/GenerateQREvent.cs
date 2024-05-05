namespace SmartBoardLocker.Types
{
    public class GenerateQREvent
    {
        public GenerateQREvent(string id) {
            data.id = id;
        }
        public EventTypes id { get; } = EventTypes.GenerateQR;
        public GenerateQREventData data { get; set; } = new GenerateQREventData();
    }

    public class GenerateQREventData
    {
        public string id { get; set; }
    }
}
