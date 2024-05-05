namespace SmartBoardLocker.Types
{
    public class RunCommand
    {
        public EventTypes id { get; set; }
        public string command { get; set; }
        public RunCommand(string cmd)
        {
            command = cmd;
        }
    }
}