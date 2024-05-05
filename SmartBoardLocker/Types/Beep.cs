using SmartBoardLocker.Types;
using System.Collections.Generic;

namespace SmartBoardLocker.Types
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

    public class ServerConfiguration
    {
        public string SchoolName { get; set; }
        public string SchoolIconURL { get; set; }
        public List<string> LockTimes { get; set; }

    }
}
