using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.NetworkInformation;
using System.Net;//do pozyskania adresu IP
using System.Data.SQLite;


namespace BandwidthMonitor
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    /// 
    public class InterfacesClass
    {
        public NetworkInterface[] networkInterfaces { get; set; }
        public List<NetworkInterface> usefulInterfaces3 = new List<NetworkInterface>();
    }

    public partial class MainWindow : Window
    {
        InterfacesClass intClass = new InterfacesClass();
        private System.Windows.Forms.NotifyIcon notifyIcon;

        //Timer sekundowy
        private System.Windows.Threading.DispatcherTimer dispatcherTimer;
        //Timer minutowy
        private System.Windows.Threading.DispatcherTimer dispatcherTimerMinute;
        //To rememer selected item after refreshing interfaces list
        private int lastSelectedItem = 0;

        public MainWindow()
        {
            InitializeComponent();
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = new System.Drawing.Icon(@"D:\Visual Studio\BandwidthMonitor\BandwidthMonitor\Resources\icon.ico");
            notifyIcon.Text = "Bandwidth monitor";
            notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(NofityIcon_MouseDoubleClick);
        }
        void NofityIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.WindowState = WindowState.Normal;  
        }

        void MainWindows_Loaded(object sender, RoutedEventArgs e)
        {
            Sqlite sqlite = new Sqlite();
            InitializeNetworkInterfaces();
            sqlite.InitBinding(intClass.usefulInterfaces3);
            sqlite.CheckIfAnyRowsExists(intClass.usefulInterfaces3);
            sqlite.GetStatsOnStartup(intClass.usefulInterfaces3);
            InitTimer();
            InitTimerMinute();
        }

        private void InitializeNetworkInterfaces()
        {
            //Get all interfaces
            intClass.networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            //Get only useful interfaces
            foreach (NetworkInterface nint in intClass.networkInterfaces) {
                if (nint.SupportsMulticast  && nint.OperationalStatus.ToString() == "Up") {
                            intClass.usefulInterfaces3.Add(nint);

                }
            }
            //If there is at least than 1 useful interface, fill comboBox
            if (intClass.usefulInterfaces3.Count > 0) {
                foreach(NetworkInterface ni in intClass.usefulInterfaces3) {
                    if (!cb_Interfaces.Items.Contains(ni.Name))
                    {
                        cb_Interfaces.Items.Add(ni.Name);
                    }
                }
            }
            //Select first interface on startup
            cb_Interfaces.SelectedIndex = lastSelectedItem;
        }
        //Is executed every 10 second and checks if any new interface is available
        private void InitializeNetworkInterfaces2()
        {
            //Get all interfaces
            intClass.networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            //This table stores previous interfaces. In next step they will be compared to all interfaces
            //If any new will appear, it will be added
            string[] previousInterfaces = new string[intClass.networkInterfaces.Count()];
            for (int i = 0; i < intClass.usefulInterfaces3.Count; i++)
            {
                previousInterfaces[i] = intClass.usefulInterfaces3[i].Name;
            }

            //Get only useful interfaces
            foreach (NetworkInterface nint in intClass.networkInterfaces)
            {
                if (nint.SupportsMulticast && nint.OperationalStatus.ToString() == "Up")
                {
                    if (!(previousInterfaces.Contains(nint.Name)))
                    {
                        intClass.usefulInterfaces3.Add(nint);
                    }
                }
            }
            //If there is at least than 1 useful interface, fill comboBox
            if (intClass.usefulInterfaces3.Count > 0)
            {
                //cb_Interfaces.Items.Clear();
                foreach (NetworkInterface ni in intClass.usefulInterfaces3)
                {
                    if (!cb_Interfaces.Items.Contains(ni.Name))
                    {
                        cb_Interfaces.Items.Add(ni.Name);
                    }
                }
                /*foreach (NetworkInterface ni in intClass.usefulInterfaces3)
                {
                    MessageBox.Show(ni.Name);
                }*/
            }
            //Select first interface on startup
            cb_Interfaces.SelectedIndex = lastSelectedItem;
        }
        private void InitTimer()
        {
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }
        //Currently ticks every 10 sec
        private void InitTimerMinute()
        {
            dispatcherTimerMinute = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimerMinute.Tick += dispatcherTimerMinute_Tick;
            dispatcherTimerMinute.Interval = new TimeSpan(0, 0, 10);
            dispatcherTimerMinute.Start();
        }

        //Updates Main windows labels
        private void UpdateNetworkInterfacesLabels()
        {
            NetworkInterface nic = intClass.usefulInterfaces3[cb_Interfaces.SelectedIndex];
            IPv4InterfaceStatistics interfaceStatistics = nic.GetIPv4Statistics();

            double BytesRecived = Math.Round((interfaceStatistics.BytesReceived)/(Math.Pow(1024,2)),2);
            label_BytesRecivedDay.Content = BytesRecived.ToString() + "MB";

            double BytesSent = Math.Round((interfaceStatistics.BytesSent) / (Math.Pow(1024, 2)), 2);
            label_BytesSentDay.Content = BytesSent.ToString() + "MB";


            foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses) {
                if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    label_IPAddress_Value.Content = ip.Address.ToString();
                }
            }
        }
        private void UpdateDatabase()
        {
            Sqlite sq = new Sqlite();
            sq.Update(intClass.usefulInterfaces3);

        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            UpdateNetworkInterfacesLabels();
        }

        private void dispatcherTimerMinute_Tick(object sender, EventArgs e)
        {
            InitializeNetworkInterfaces2();
            UpdateDatabase();
            Sqlite sq = new Sqlite();
            GetLast7Days();
            GetLast30Days();
            sq.CheckIfCurrentDayExists(intClass.usefulInterfaces3);
            
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            try {
                if (this.WindowState == WindowState.Minimized) {
                    this.ShowInTaskbar = false;
                    notifyIcon.BalloonTipTitle = "Minimize Sucessful";
                    notifyIcon.BalloonTipText = "Minimized the app";
                    notifyIcon.ShowBalloonTip(4000);
                    notifyIcon.Visible = true;
                }
                else if (this.WindowState == WindowState.Normal) {
                    notifyIcon.Visible = false;
                    this.ShowInTaskbar = true;
                }
            }
            catch (Exception) {

                throw;
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            //this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            //this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            this.Topmost = true;
            //this.Top = 0;
            //this.Left = 0;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Topmost = true;
            this.Activate();
        }

        private void cb_Interfaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lastSelectedItem = cb_Interfaces.SelectedIndex;
            UpdateNetworkInterfacesLabels();
            GetLast7Days();
            GetLast30Days();
        }
        private void GetLast7Days()
        {
            Sqlite sq = new Sqlite();

            double[] Bytes = sq.GetLast7Days(intClass.usefulInterfaces3[cb_Interfaces.SelectedIndex]);
            double BytesRecived = Math.Round((Bytes[0]) / (Math.Pow(1024, 2)), 2);
            double BytesSent = Math.Round((Bytes[1]) / (Math.Pow(1024, 2)), 2);
            label_BytesRecivedWeek.Content = BytesRecived.ToString() + "MB";
            label_BytesSentWeek.Content = BytesSent.ToString() + "MB";
        }
        private void GetLast30Days()
        {
            Sqlite sq = new Sqlite();

            double[] Bytes = sq.GetLast30Days(intClass.usefulInterfaces3[cb_Interfaces.SelectedIndex]);
            double BytesRecived = Math.Round((Bytes[0]) / (Math.Pow(1024, 2)), 2);
            double BytesSent = Math.Round((Bytes[1]) / (Math.Pow(1024, 2)), 2);
            label_BytesRecivedMonth.Content = BytesRecived.ToString() + "MB";
            label_BytesSentMonth.Content = BytesSent.ToString() + "MB";
        }
    }
}
