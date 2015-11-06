using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace CELO_Enhanced
{
    /// <summary>
    /// Interaction logic for Feedback.xaml
    /// </summary>
    public partial class Feedback : Window
    {
        System.Windows.Forms.WebBrowser wb = new System.Windows.Forms.WebBrowser();

        public Feedback()
        {
            InitializeComponent();
        }

        private int num1, num2;

        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            if (num1 + num2 == Int32.Parse(txtCode.Text))
            {
                if (txtText.Text.Length > 3)
                {
                    if (Utilities.CheckInternet())
                    {
                        if (Utilities.CheckWebSiteLoad("http://neffware.com", 10000))
                        {
                            SendFeedback();
                        }
                        else
                        {
                            Utilities.showError(this, "Error while sending feedback. Please try again later");
                        }
                    }
                    else
                    {
                        Utilities.showError(this, "Error while sending feedback. Please try again later");
                    }

                }
            }

        }

        private void SendFeedback()
        {
           
            String SendText = txtText.Text.Replace(Environment.NewLine,"_555_");
            String Name = txtName.Text;
            String CeloVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            String url = String.Format("http://www.neffware.com/downloads/celo/fback/feedback.php?user={0}&text={1}&version={2}",Name,SendText,CeloVersion);

            wb.DocumentCompleted += Wb_DocumentCompleted;
            wb.Navigate(new Uri(url));
        }

        private void Wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            string content = wb.DocumentText;
            if (content.Contains("data inserted"))
            {
                Utilities.showMessage(this,
                    "Thank you for sending feedback.\nIf you would like to support further please check my website:\nwww.neffware.com/index.php/support",
                    "Feedback sent");
                this.Close();
            }
            else
            {
                Utilities.showError(this, "Error while sending feedback. Please try again later");
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void txtText_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCondValid();
        }

        

        private bool isCondValid()
        {
            try
            {
                if (txtCode.Text.Any(Char.IsDigit))
                {
                    if (txtText.Text.Length > 3)
                    {
                        if (num1 + num2 == Int32.Parse(txtCode.Text))
                        {
                            buttonSend.IsEnabled = true;
                            return true;
                        }
                        else
                        {
                            buttonSend.IsEnabled = false;
                        }
                    }
                    else
                    {
                        buttonSend.IsEnabled = false;
                    }
                }
                else
                {
                    buttonSend.IsEnabled = false;
                }
            }
            catch (Exception)
            {
                buttonSend.IsEnabled = false;
            }


            return false;
        }

        private void txtCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCondValid();
        }

        private void labelBug_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://www.coh2.org/pm/create/access_users/7871");
        }

        private void FeedbackWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Random rng = new Random();
            Random rng2 = new Random();
            num1 = rng.Next(1, 8);
            num2 = rng2.Next(2, 9);

            txtCodeQuestion.Text = String.Format("Question: {0} + {1} = ?", num1, num2);
        }
    }
}
