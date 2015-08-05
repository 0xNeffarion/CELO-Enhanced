using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Management;
using System.Windows.Input;

namespace CELO_Enhanced
{
    
    public partial class App : Application
    {
        public bool DoHandle { get; set; }
        private Utilities.Log logFile = new Utilities.Log(AppDomain.CurrentDomain.BaseDirectory + @"\logs");

        public App()
        {
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            Application.Current.Exit += Current_Exit;
            logFile.CreateNew();
            
            logFile.WriteLine("CELO - STARTED");
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            MessageBox.Show("A Critical error caused the application to crash!\nThe exception has been logged.", "ERROR",
                MessageBoxButton.OK, MessageBoxImage.Error);

            logFile.WriteLine("UNHANDLED EXCEPTION (3): " + (e.Exception).ToString());
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("A Critical error caused the application to crash!\nThe exception has been logged.", "ERROR",
                MessageBoxButton.OK, MessageBoxImage.Error);

            logFile.WriteLine("UNHANDLED EXCEPTION (1): " + (e.ExceptionObject as Exception).ToString());
            
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            logFile.WriteLine("CELO - ENDED - EXIT CODE: " + e.ApplicationExitCode.ToString());
            
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("A Critical error caused the application to crash!\nThe exception has been logged.", "ERROR",
                MessageBoxButton.OK, MessageBoxImage.Error);

            logFile.WriteLine("UNHANDLED EXCEPTION (2): " + e.Exception.ToString());
            e.Handled = true;

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            logFile.WriteLine("APP START - STARTED");

            string CPU_NAME = "";
            string GPU_NAME = "";
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_Processor");
                foreach (var share in searcher.Get())
                {
                    foreach (PropertyData property in share.Properties)
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
                logFile.WriteLine("EXCEPTION AT CPU RETRIEVAL: " + ex.ToString());
            }

            try
            {
                ManagementObjectSearcher searcher2 = new ManagementObjectSearcher("select * from Win32_DisplayConfiguration");
                foreach (var share in searcher2.Get())
                {
                    foreach (PropertyData property in share.Properties)
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
                logFile.WriteLine("EXCEPTION AT GPU RETRIEVAL: " + ex.ToString());
            }

            logFile.WriteLine("========= SYSTEM AND APP INFORMATION ======");
            logFile.WriteLine("Main Directory: " + AppDomain.CurrentDomain.BaseDirectory);
            logFile.WriteLine("Machine Name: " + Environment.UserDomainName);
            logFile.WriteLine("Operating System: " + Environment.OSVersion.ToString());
            logFile.WriteLine("x64 OS: " + Environment.Is64BitOperatingSystem.ToString());
            logFile.WriteLine("Total RAM: " + (((new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory) / 1024) / 1024).ToString() + " MBytes");
            logFile.WriteLine("CPU: " + CPU_NAME);
            logFile.WriteLine("Graphic's Card: " + GPU_NAME);
            logFile.WriteLine("==========================================");
            
            try
            {
                Uri uri =
                    new Uri(
                        "PresentationFramework.Aero;V3.0.0.0;31bf3856ad364e35;component\\themes/aero.normalcolor.xaml",
                        UriKind.Relative);
                Resources.MergedDictionaries.Add(Application.LoadComponent(uri) as ResourceDictionary);
            }
            catch (Exception ex)
            {
                logFile.WriteLine("EXCEPTION: " + ex.ToString());
            }
            finally
            {
                logFile.WriteLine("APP START - ENDED");
            }
            
            
            
        }

     }
        
}