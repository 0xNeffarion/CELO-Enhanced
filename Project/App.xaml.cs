using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace CELO_Enhanced
{
    
    public partial class App : Application
    {
        public bool DoHandle { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                Uri uri = new Uri("PresentationFramework.Aero;V3.0.0.0;31bf3856ad364e35;component\\themes/aero.normalcolor.xaml", UriKind.Relative);
                Resources.MergedDictionaries.Add(Application.LoadComponent(uri) as ResourceDictionary);
            }
            catch{}

            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName.Contains("CELO") && process.Id != Process.GetCurrentProcess().Id)
                {
                    Environment.Exit(0);
                }
            }
            
        }
    }
        
}