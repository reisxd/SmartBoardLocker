using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using SmartBoardLocker.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;
using WatsonWebsocket;
using System.Windows.Forms;
using Microsoft.UI.Dispatching;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq.Expressions;
using Microsoft.UI.Xaml.Media.Imaging;


namespace SmartBoardLocker
{
    public sealed partial class MainWindow : Window
    {
        private List<News> news;
        private WatsonWsClient client;
        private AppWindow _apw;
        private OverlappedPresenter _presenter;
        ClientConfiguration configJson;
        List<string> lockTimes = new List<string>();
        string lastLockTime = "";

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
        private IntPtr hwnd;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);


        public MainWindow()
        {
            this.InitializeComponent();
            string config = File.ReadAllText("config.json");
            configJson = JsonConvert.DeserializeObject<ClientConfiguration>(config);
            news = GetNews();
            GetAppWindowAndPresenter();
            _apw.SetPresenter(AppWindowPresenterKind.FullScreen);
            _presenter.Maximize();
            _presenter.SetBorderAndTitleBar(hasBorder: false, hasTitleBar: false);
            _presenter.IsAlwaysOnTop = true;
            int change = 1;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (o, a) =>
            {
                string currentTime = DateTime.Now.ToString("HH:mm");
                if (lockTimes.Contains(currentTime) && !this.AppWindow.IsVisible && currentTime != lastLockTime)
                {
                    StartWSClient();
                    lastLockTime = currentTime;
                    this.AppWindow.Show();
                }
                int newIndex = Images.SelectedIndex + change;
                if (newIndex >= Images.Items.Count || newIndex < 0)
                {
                    change *= -1;
                }

                Images.SelectedIndex += change;
            };

            timer.Start();

            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            MainPanel.Loaded += MainPanel_Loaded;
        }

        private void MainPanel_Loaded(object sender, RoutedEventArgs e)
        {
            StartWSClient();
        }

        private List<News> GetNews()
        {
            var list = new List<News>();
            using (var client = new WebClient())
            {
                string response = "";
                try
                {
                    response = client.DownloadString($"{configJson.ApiURL}/api/News/GetNews");
                } catch (WebException) {
                    ContentDialog errorDialog = new ContentDialog()
                        {
                            XamlRoot = MainPanel.XamlRoot,
                            Title = "Bir hata oluştu",
                            Content = "Haberleri alırken bir hata oluştu. Sunucu kapalı veya internet olmayabilir mi?",
                            CloseButtonText = "Tamam"
                        };
                    try
                    {
                        errorDialog.ShowAsync();
                    } catch { }
                }
                if (!string.IsNullOrEmpty(response))
                {
                    list = JsonConvert.DeserializeObject<List<News>>(response);
                }
            }

            return list;
        }

        private void StartWSClient()
        {
            string apiUrl = configJson.ApiURL.StartsWith("https://") ? configJson.ApiURL.Replace("https://", "wss://") : configJson.ApiURL.Replace("http://", "ws://");
            var url = new Uri($"{apiUrl}/wsServer");
            client = new WatsonWsClient(url);
            client.MessageReceived += Client_MessageReceived;
            bool successfulConn = client.StartWithTimeout(5);
            if (!successfulConn) {
                ContentDialog errorDialog = new ContentDialog()
                {
                    XamlRoot = MainPanel.XamlRoot,
                    Title = "Bir hata oluştu",
                    Content = "Sunucuya bağlanırken bir hata oluştu. Sunucu kapalı veya internet olmayabilir mi?",
                    CloseButtonText = "Tamam"
                };
                errorDialog.ShowAsync();
            }
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            string msg = Encoding.UTF8.GetString(e.Data);
            BasicWSEvent wsEvent = JsonConvert.DeserializeObject<BasicWSEvent>(msg);
            switch (wsEvent.id)
            {
                case EventTypes.Beep:
                    {
                        Beep beep = JsonConvert.DeserializeObject<Beep>(msg);
                        lockTimes = beep.LockTimes;
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            SchoolName.Text = beep.SchoolName;
                            SchoolLogo.Source = new BitmapImage(new Uri(beep.SchoolIconURL, UriKind.Absolute));
                        });
                        Process.Start("taskkill", "/f /im explorer.exe");
                        client.SendAsync(JsonConvert.SerializeObject(new IdentifyEvent(configJson.ClassName)));
                        break;
                    }
                case EventTypes.GenerateQR:
                    {
                        GenerateQREvent qrEvent = JsonConvert.DeserializeObject<GenerateQREvent>(msg);
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            QrCodeWebView.Source = new Uri($"{configJson.ApiURL}/qrcodegen.html?code={qrEvent.data.id}");
                        });
                        break;
                    }
                case EventTypes.Unlock:
                    {
                        Process.Start("c:\\windows\\explorer.exe");
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            this.AppWindow.Hide();
                            client.Stop();
                        });
                        break;
                    }
                case EventTypes.RunCommand:
                    {
                        RunCommand runCommand = JsonConvert.DeserializeObject<RunCommand>(msg);
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c {runCommand.command}"
                        };

                        Process.Start(startInfo);
                        break;
                    }
            }
        }

        private void GetAppWindowAndPresenter()
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            hwnd = hWnd;
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            _apw = AppWindow.GetFromWindowId(myWndId);
            _presenter = AppWindow.Presenter as OverlappedPresenter;
        }

        private void ShutdownButton_Click(object sender, RoutedEventArgs e)
        {
            client.Stop();
            var startInfo = new ProcessStartInfo
            {
                FileName = "shutdown.exe",
                Arguments = "/s /t 0"
            };

            Process.Start(startInfo);
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            client.Stop();
            var startInfo = new ProcessStartInfo
            {
                FileName = "shutdown.exe",
                Arguments = "/r /t 0"
            };

            Process.Start(startInfo);
        }
    }
}
