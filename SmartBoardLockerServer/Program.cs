using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json;
using SmartBoardLockerServer.Types;
using System.Net.WebSockets;
using System.Text;
using MySql.Data.MySqlClient;
using Microsoft.IdentityModel.Tokens;

class Program
{
    
    public static List<DeviceClient> clients = new List<DeviceClient>();
    
    public static MySqlConnection dbConnection;
    
    public static ServerConfiguration configJson;

    #pragma warning disable CS8600 
    public static void Main(string[] args)
    {
        
        string config = File.ReadAllText("config.json");
        
        configJson = JsonConvert.DeserializeObject<ServerConfiguration>(config);
        
        dbConnection = new MySqlConnection($"Server={configJson.DatabaseIP};Database={configJson.DatabaseName};Uid={configJson.DatabaseUsername};Pwd={configJson.DatabasePassword}");
        
        dbConnection.Open();

        
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddSingleton(configJson);
        builder.Services.AddSingleton(clients);
        builder.Services.AddHttpContextAccessor();
        
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configJson.JwtKey)) 
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["jwt"]; 
                        return Task.CompletedTask;
                    }
                };
            });

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.UseWebSockets();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        
        app.Map("/wsServer", async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var client = await context.WebSockets.AcceptWebSocketAsync(); 
                await SendMessage(JsonConvert.SerializeObject(new Beep(configJson.SchoolName, configJson.SchoolIconURL, configJson.LockTimes)), client); 
                var buffer = new byte[1024 * 20480]; 
                while (client.State == WebSocketState.Open) 
                {
                    await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None); 
                    var content = Encoding.UTF8.GetString(buffer); 
                    HandleEvent(content, client); 
                }
            }
            else context.Response.StatusCode = 400; 
        });

        
        app.Run();

        
        async void HandleEvent(string content, WebSocket client)
        {
            BasicWSEvent wsEvent = null;
            try {
                wsEvent = JsonConvert.DeserializeObject<BasicWSEvent>(content); 
            } catch (Exception e) {
                Console.WriteLine(e); 
                return;
            }
            switch (wsEvent.id) 
            {
                case EventTypes.Identify:
                    {
                        IdentifyEvent identifyEvent = JsonConvert.DeserializeObject<IdentifyEvent>(content); 
                        Guid v4 = Guid.NewGuid(); 
                        string uuid = v4.ToString(); 
                        clients.Add(new DeviceClient(identifyEvent.data.className, client, uuid)); 
                        GenerateQREvent generateQREvent = new GenerateQREvent(uuid); 
                        string msg = JsonConvert.SerializeObject(generateQREvent); 
                        await SendMessage(msg, client); 
                        break;
                    }
            }
        }
    }

    
    public async static Task SendMessage(string content, WebSocket client)
    {
        var bytes = Encoding.UTF8.GetBytes(content); 
        await client.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None); 
    }
}