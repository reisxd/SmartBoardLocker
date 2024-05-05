using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBoardLocker.Types
{
    internal class DesktopData
    {
        public EventTypes id { get; } = EventTypes.DesktopData;
        public DesktopInfoData data { get; set; } = new DesktopInfoData();

        public DesktopData(byte[] videoData)
        {
            data.videoData = Convert.ToHexString(videoData);
        }
    }

    public class DesktopInfoData
    {
        public string videoData { get; set; }
    }
}
