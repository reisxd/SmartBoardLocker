using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartBoardLockerServer.Types;
using System.Net;

namespace SmartBoardLockerServer.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : Controller
    {
        private readonly IWebHostEnvironment environment;
        public NewsController(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        
        [HttpGet("[action]")]
        public IActionResult GetNews()
        {
            List<News> newsResponse = new List<News>();
            
            string siteIndex = CallUrl(Program.configJson.SchoolWebsiteURL).Result;
            HtmlDocument htmlDoc = new HtmlDocument();
            
            htmlDoc.LoadHtml(siteIndex);
            
            var newsHTML = htmlDoc.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "").Contains("haberler"))
                .ToList()[0].Descendants("ul").First().Descendants("li").ToList();

            
            foreach(var node in newsHTML)
            {
                var currentNode = node.Descendants("a").First();
                
                string title = currentNode.Descendants("p").First().InnerText;
                
                var image = Program.configJson.SchoolWebsiteURL + currentNode.Descendants("img").First().GetAttributes("src").First().Value;
                
                News news = new News(title, image);
                newsResponse.Add(news);
            }

            
            return Ok(newsResponse);
        }

        
        private static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            client.DefaultRequestHeaders.Accept.Clear();
            var response = client.GetStringAsync(fullUrl);
            return await response;
        }
    }
}