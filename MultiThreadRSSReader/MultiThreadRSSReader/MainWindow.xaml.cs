using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading;
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
using System.Xml;

namespace MultiThreadRSSReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<String> utls = new List<string>()
            {
                @"https://binkl.by/feed/",
                @"http://feeds.feedburner.com/kaktutzhit"
            };
        private List<Thread> threads = new List<Thread>();
        private List<SyndicationItem> feedItems = new List<SyndicationItem>();
        private object locker = new object();
        private bool working = true;
      
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadFeeds()
        {
            string[] categories = TBCategory.Text.Split(' ');

            feedItems.Clear();
            foreach (string url in utls)
            {
                Thread th = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    LoadFeed(url, categories);
                });
                threads.Add(th);
                th.Start();
            }
            while (working)
            {
                working = false;
                foreach (var th in threads)
                    if (th.IsAlive)
                    {
                        working = true;
                        break;
                    }
            }
            SendEmail();
        }

        private void LoadFeed(string url, string[] categories)
        {
            XmlReader reader;
            SyndicationFeed feed;
            reader = XmlReader.Create(url);
            feed = SyndicationFeed.Load(reader);

            if (categories[0] != "")
            {
                foreach (SyndicationItem feedItem in feed.Items)
                    foreach(string category in categories)
                        if (feedItem.Categories[0].Name == category )
                            feedItems.Add(feedItem);
            }
            else
            {
                foreach (SyndicationItem feedItem in feed.Items)
                    feedItems.Add(feedItem);
            }
            reader.Close();
        }

        private void PrintFeeds()
        {
            LBItems.Items.Clear();
            foreach (SyndicationItem feedItem in feedItems)
            {
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Content = feedItem.Title.Text;
                LBItems.Items.Add(listBoxItem);
            }
        }

        private void BUpdateItem_Click(object sender, RoutedEventArgs e)
        {
            LoadFeeds();
            PrintFeeds();
        }

        private void LBItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LBItems.SelectedIndex > -1)
            {
                SyndicationItem item = feedItems[LBItems.SelectedIndex];
                LDate.Content = item.PublishDate.ToString();
                LTitle.Content = item.Title.Text;
                LURL.Content = item.Links[0].Uri.AbsoluteUri.ToString();
                LCategory.Content = item.Categories[0].Name;
            }
        }

        private void BRead_Click(object sender, RoutedEventArgs e)
        {
            int index = LBItems.SelectedIndex;
            if (index > -1)
                System.Diagnostics.Process.Start(feedItems[index].Links[0].Uri.AbsoluteUri);
        }

        private void SendEmail()
        {
            try
            {
                string messageBody = "";
                foreach (SyndicationItem item in feedItems)
                    messageBody += item.Links[0].Uri.AbsoluteUri.ToString() + "   ";
                MailMessage mail = new MailMessage("semenchik.polina@mail.ru", TBEmail.Text, "RSS Feed", messageBody);
                SmtpClient client = new SmtpClient
                {
                    Port = 25,
                    Host = "smtp.mail.ru",
                    EnableSsl = true,
                    Timeout = 10000,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential("semenchik.polina@mail.ru", "polya_29252211")
                };
                client.Send(mail);
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            
        }
    }
}
