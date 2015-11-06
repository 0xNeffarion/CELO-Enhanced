using System;
using System.Management;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.VisualBasic.Devices;

namespace CELO_Enhanced
{
    public partial class App : Application
    {
        private readonly Utilities.INIFile cfgFile;
        private readonly Utilities.Log logFile = new Utilities.Log(AppDomain.CurrentDomain.BaseDirectory + @"\logs");

        public App()
        {
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            Current.Exit += Current_Exit;
            cfgFile = new Utilities.INIFile(AppDomain.CurrentDomain.BaseDirectory + @"\config.ini");
            logFile.CreateNew();
            

            logFile.WriteLine("CELO - STARTED");
            logFile.WriteLine("CELO VERSION: " + Assembly.GetExecutingAssembly().GetName().Version);
            if (cfgFile.IniReadValue("Main", "HardwareAcceleration").ToLower() == "false")
            {
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
                logFile.WriteLine("CELO - WPF Hardware Acceleration: FALSE");
            }
            else
            {
                logFile.WriteLine("CELO - WPF Hardware Acceleration: TRUE");
            }
        }

        public bool DoHandle { get; set; }

        private void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            if (Utilities.FastCRC32.CRC32String(e.ToString()) != 1532817726)
            {
                logFile.WriteLine("UNHANDLED EXCEPTION (4) (Hash: " + Utilities.FastCRC32.CRC32String(e.ToString()) + ") : " +
                                  e.Exception.ToString());
            }
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                "A Critical error caused the application to crash!\nThe exception has been logged.\nError code: " +
                Utilities.FastCRC32.CRC32String(e.ToString()), "ERROR",
                MessageBoxButton.OK, MessageBoxImage.Error);

            logFile.WriteLine("UNHANDLED EXCEPTION (2) (Hash: " + Utilities.FastCRC32.CRC32String(e.ToString()) + ") : " +
                              e.Exception.ToString());
            e.Handled = true;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            MessageBox.Show(
                "A Critical error caused the application to crash!\nThe exception has been logged.\nError code: " +
                Utilities.FastCRC32.CRC32String(e.ToString()), "ERROR",
                MessageBoxButton.OK, MessageBoxImage.Error);

            logFile.WriteLine("UNHANDLED EXCEPTION (3) (Hash: " + Utilities.FastCRC32.CRC32String(e.ToString()) + ") : " +
                              e.Exception.ToString());
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            
            if (Utilities.FastCRC32.CRC32String(e.ToString()) != 2225983837)
            {
                MessageBox.Show("A Critical error caused the application to crash!\nThe exception has been logged.\nError code: " +
                    Utilities.FastCRC32.CRC32String(e.ToString()), "ERROR",
                    MessageBoxButton.OK, MessageBoxImage.Error);

            }
            logFile.WriteLine("UNHANDLED EXCEPTION (1) (Hash: " + Utilities.FastCRC32.CRC32String(e.ToString()) + ") (IsTerminate? : " + e.IsTerminating.ToString() + ") : " + (e.ExceptionObject as Exception).ToString());
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            logFile.WriteLine("CELO - ENDED - EXIT CODE: " + e.ApplicationExitCode);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            logFile.WriteLine("APP START - STARTED");

            var CPU_NAME = "";
            var GPU_NAME = "";
            try
            {
                var searcher = new ManagementObjectSearcher("select * from Win32_Processor");
                foreach (var share in searcher.Get())
                {
                    foreach (var property in share.Properties)
                    {
                        if (property.Name == "Name")
                        {
                            CPU_NAME = property.Value.ToString();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logFile.WriteLine("EXCEPTION AT CPU RETRIEVAL: " + ex);
            }

            try
            {
                var searcher2 = new ManagementObjectSearcher("select * from Win32_DisplayConfiguration");
                foreach (var share in searcher2.Get())
                {
                    foreach (var property in share.Properties)
                    {
                        if (property.Name == "Description")
                        {
                            GPU_NAME = property.Value.ToString();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logFile.WriteLine("EXCEPTION AT GPU RETRIEVAL: " + ex);
            }

            logFile.WriteLine("========= SYSTEM AND APP INFORMATION ======");
            logFile.WriteLine("Main Directory: " + AppDomain.CurrentDomain.BaseDirectory);
            logFile.WriteLine("Machine Name: " + Environment.UserDomainName);
            logFile.WriteLine("Operating System: " + Environment.OSVersion);
            logFile.WriteLine("x64 OS: " + Environment.Is64BitOperatingSystem);
            logFile.WriteLine("Total RAM: " + (((new ComputerInfo().TotalPhysicalMemory)/1024)/1024) + " MBytes");
            logFile.WriteLine("CPU: " + CPU_NAME);
            logFile.WriteLine("Graphic's Card: " + GPU_NAME);
            logFile.WriteLine("==========================================");

            try
            {
                var uri =
                    new Uri(
                        "PresentationFramework.Aero;V3.0.0.0;31bf3856ad364e35;component\\themes/aero.normalcolor.xaml",
                        UriKind.Relative);
                Resources.MergedDictionaries.Add(LoadComponent(uri) as ResourceDictionary);
            }
            catch (Exception ex)
            {
                logFile.WriteLine("EXCEPTION: " + ex);
            }
            finally
            {
                logFile.WriteLine("APP START - ENDED");
            }
        }
    }
}