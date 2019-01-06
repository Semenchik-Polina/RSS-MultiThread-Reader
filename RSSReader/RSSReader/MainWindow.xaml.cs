using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
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
using System.Xml;
using System.Xml.Serialization;


namespace RSSReader
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
        private List<SyndicationItem> feedItems = new List<SyndicationItem>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadFeeds()
        {
            feedItems.Clear();
            XmlReader reader;
            SyndicationFeed feed;
            foreach (string url in utls)
            {
                reader = XmlReader.Create(url);
                feed = SyndicationFeed.Load(reader);
                foreach (SyndicationItem feedItem in feed.Items)
                    feedItems.Add(feedItem);
                reader.Close();
            }
        }

        private void PrintFeeds()
        {
            LBItems.Items.Clear();
            foreach(SyndicationItem feedItem in feedItems)
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
         //   Browser.Navigate(feedItems[index].Links[0].Uri.AbsoluteUri);
        }

    }
}
