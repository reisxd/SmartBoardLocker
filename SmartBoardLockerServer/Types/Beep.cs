namespace SmartBoardLockerServer.Types
{
    public class Beep : ServerConfiguration
    {
        public EventTypes id { get; set; } = EventTypes.Beep;

        public Beep(string schoolName, string SchoolIconURL, List<string> lockTimes)
        {
            this.SchoolName = schoolName;
            this.SchoolIconURL = SchoolIconURL;
            this.LockTimes = lockTimes;
        }
    }
}
