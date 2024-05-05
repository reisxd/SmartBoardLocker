namespace SmartBoardLockerServer.Types
{
    public class ServerConfiguration
    {
        public string SchoolName { get; set; }
        public string SchoolIconURL { get; set; }
        public List<string> LockTimes { get; set; }
        public string SchoolWebsiteURL { get; set; }
        public string DatabaseIP { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseUsername { get; set; }
        public string DatabasePassword { get; set; }
        public string JwtKey { get; set; }
    }
}
