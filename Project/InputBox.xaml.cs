using System.Windows;
using System.Windows.Controls;

namespace CELO_Enhanced
{
    /// <summary>
    ///     Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        public string val = "";

        public InputBox(string title, string caption, string text)
        {
            InitializeComponent();
            Title = title;
            lbl_Caption.Content = caption;
            tBox_Text.Text = text;
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            val = tBox_Text.Text;
            DialogResult = true;
        }

     }
}