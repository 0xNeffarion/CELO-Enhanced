using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace CELO_Enhanced
{
    /// <summary>
    ///     Interaction logic for ScriptCompiler.xaml
    /// </summary>
    public partial class ScriptCompiler : Window
    {
        private readonly StringBuilder content;
        private readonly String ScripComp;
        private readonly String ScriptOut;
        private readonly String TempF;

        public ScriptCompiler(String outputFile, String ScriptCompilerFile, String TempFile, StringBuilder ScriptContent)
        {
            InitializeComponent();
            ScripComp = ScriptCompilerFile;
            ScriptOut = outputFile;
            content = ScriptContent;
            TempF = TempFile;
        }

        private void CompMain_Loaded(object sender, RoutedEventArgs e)
        {
            tBox_content.Text = content.ToString();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                File.WriteAllText(TempF, tBox_content.Text);
                var st = new ProcessStartInfo();
                st.Arguments = string.Format("/in \"{0}\" /out \"{1}\"", TempF, ScriptOut);
                st.WindowStyle = ProcessWindowStyle.Hidden;
                st.FileName = ScripComp;
                var proc = new Process();
                proc.StartInfo = st;
                proc.Start();
                proc.WaitForExit(4000);
                var ex = proc.ExitCode;
                if (ex == 0)
                {
                    Utilities.showMessage(this, "Script Compiled!\nLocation: " + ScriptOut, "Script");
                    Close();
                }
                else
                {
                    Utilities.showError(this,
                        "An error occured while the script was being compiled.\nPlease check the code!");
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}