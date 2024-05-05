namespace SmartBoardLockerServer.Types {
    public class RunCommand {
        public EventTypes id { get; set; } = EventTypes.RunCommand;
        public string command { get; set; }
        public RunCommand(string cmd) {
            command = cmd;
        }
    }
}