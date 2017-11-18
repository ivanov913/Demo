using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Net;
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
using System.Windows.Forms;

namespace DemoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly string cUploadUrl = "https://jsonplaceholder.typicode.com/posts";
        private readonly int cFetchInterval = 5000;
        private readonly int cUploadInterval = 600000;

        private WebClient mWebClient;
        private Timer mFetchTimer;
        private Timer mUploadTimer;
        private string mCpuUsage;
        private string mRamUsage;
        private string mHardDriveUsage;

        public string cpuUsage
        {
            get { return mCpuUsage; }
            set
            {
                mCpuUsage = value;
                OnPropertyChanged("cpuUsage");
            }
        }

        public string ramUsage
        {
            get { return mRamUsage; }
            set
            {
                mRamUsage = value;
                OnPropertyChanged("ramUsage");
            }
        }

        public string hardDriveUsage
        {
            get { return mHardDriveUsage; }
            set
            {
                mHardDriveUsage = value;
                OnPropertyChanged("hardDriveUsage");
            }
        }

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            initWebClient();
            // TODO: Should create a thread to fetch system information. Looks like it blocked UI thread.
            startTimer();
        }

        private void initWebClient()
        {
            if (mWebClient == null)
            {
                mWebClient = new WebClient();
            }
        }
        
        private void startTimer()
        {
            if (this.mFetchTimer == null)
            {
                this.mFetchTimer = new Timer();
                this.mFetchTimer.Interval = cFetchInterval;
                this.mFetchTimer.Tick += OnFetchTimerTick;
            }

            if (this.mUploadTimer == null)
            {
                this.mUploadTimer = new Timer();
                this.mUploadTimer.Interval = cUploadInterval;
                this.mUploadTimer.Tick += OnUploadTimerTick;
            }
            mFetchTimer.Start();
            mUploadTimer.Start();
            getAllInfo();
        }

        private void getAllInfo()
        {
            getCpuUsage();
            getRamUsage();
            getHDUsage();
        }

        private void getCpuUsage()
        {
            ManagementObjectSearcher objSearcher = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_PerfOS_Processor");
            foreach (ManagementObject obj in objSearcher.Get())
            {
                cpuUsage = obj["PercentProcessorTime"].ToString() + "%";
            }
        }

        private void getRamUsage()
        {
            ManagementObjectSearcher objSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject obj in objSearcher.Get())
            {
                double free = Double.Parse(obj["FreePhysicalMemory"].ToString());
                double total = Double.Parse(obj["TotalVisibleMemorySize"].ToString());
                ramUsage = Math.Round(((total - free) / total * 100), 2).ToString() + "%";
            }
        }

        private void getHDUsage()
        {
            ManagementObjectSearcher objSearcher = new ManagementObjectSearcher("Select * from Win32_LogicalDisk");
            double free = 0;
            double total = 0;
            foreach (ManagementObject obj in objSearcher.Get())
            {
                string deviceId = obj["DeviceID"].ToString();
                ManagementObjectSearcher objSearcher1 = new ManagementObjectSearcher("Select * from Win32_LogicalDisk Where DeviceId='" + deviceId + "'");
                foreach (ManagementObject obj1 in objSearcher1.Get())
                {
                    free += Convert.ToInt64(obj1["FreeSpace"]);
                    total += Convert.ToInt64(obj1["Size"]);
                }
            }

            hardDriveUsage = Math.Round(((total - free) / total * 100), 2).ToString() + "%";
        }

        private void uploadData(string url, string cpuUsage, string ramUsage, string hddUsage)
        {
            this.mWebClient.UploadStringAsync(new Uri(url), cpuUsage + ramUsage + hddUsage);
        }

        private void OnFetchTimerTick(object sender, EventArgs e)
        {
            getAllInfo();
        }

        private void OnUploadTimerTick(object sender, EventArgs e)
        {
            uploadData(cUploadUrl, cpuUsage, ramUsage, hardDriveUsage);
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
