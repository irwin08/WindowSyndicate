using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Windows.Forms;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Threading;

namespace WindowSyndicate
{
    public partial class WindowService : ServiceBase
    {
        private Dictionary<string, int> processToDisplay;

        public WindowService()
        {
            InitializeComponent();

            loadAppSettings();
            
        }

        private void loadAppSettings()
        {
            processToDisplay = new Dictionary<string, int>();

            var appSettings = ConfigurationManager.AppSettings;

            foreach (var key in appSettings.AllKeys)
            {
                int displayNum;
                if (int.TryParse(appSettings[key], out displayNum) && displayNum < Screen.AllScreens.Length)
                {
                    processToDisplay.Add(key, displayNum);
                }
                else
                {
                    Console.WriteLine($"Error! Process {key}'s value is either not parsable as an integer or out of the range of available display screens.");
                }
            }
        }

        protected override void OnStart(string[] args)
        {
            ListenForProcesses();
        }

        protected override void OnStop()
        {

        }

        private void ListenForProcesses()
        {
            ManagementEventWatcher watcher = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace")
            );
            watcher.EventArrived += new EventArrivedEventHandler(HandleNewProcess);
            watcher.Start();
        }

        private void HandleNewProcess(object sender, EventArrivedEventArgs e)
        { 
            if (processToDisplay.Keys.Contains(e.NewEvent.Properties["ProcessName"].Value))
            {
                foreach (var prop in e.NewEvent.Properties)
                {
                    Console.WriteLine(prop.Name);
                }

                SendToScreen(int.Parse(e.NewEvent.Properties["ProcessId"].Value.ToString()), processToDisplay[e.NewEvent.Properties["ProcessName"].Value.ToString()]);
            }
        }

        private void KillAndSendToScreen(int processId, int screenId)
        {
            Process process = Process.GetProcessById(processId);
            var screen = Screen.AllScreens[screenId];

            process.Kill();
            //process.StartInfo.WindowStyle = ProcessWindowStyle.;
        }

        private void SendToScreen(int processId, int screenId)
        {
            Console.WriteLine($"Here: {processId}");
            Process process = Process.GetProcessById(processId);
            var screen = Screen.AllScreens[screenId];

            process.Refresh();

            if (process.MainWindowHandle == IntPtr.Zero)
            {
                int timeout = 6;
                for (int i = 0; i < timeout; i++)
                {
                    Thread.Sleep(5000);
                    process.Refresh();
                    if (process.MainWindowHandle != IntPtr.Zero)
                        break;
                }
            }

            Console.WriteLine($"Window Handle: {process.MainWindowHandle}");

            const int SWP_NOOWNERZORDER = 0x0200;
            const int SWP_FRAMECHANGED = 0x0020;


            MoveWindow(process.MainWindowHandle, screen.WorkingArea.Left, screen.WorkingArea.Top, screen.WorkingArea.Width, screen.WorkingArea.Height, true);
            SetWindowPos(process.MainWindowHandle, IntPtr.Zero, screen.Bounds.Left, screen.Bounds.Top, screen.Bounds.Width, screen.Bounds.Height, SWP_FRAMECHANGED | SWP_NOOWNERZORDER);


            Console.WriteLine("Fullscreen");
            
        }

        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadKey(true);
            this.OnStop();
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
    }
}
