namespace SmartBoardLockerServer.Types
{
    public class News
    {
        public News(string title, string image)
        {
            Title = title;
            Image = image;
        }

        public string Title { get; set; }
        public string Image { get; set; }
    }
}
