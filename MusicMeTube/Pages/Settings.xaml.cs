using System;
using System.Windows;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;

namespace MusicMeTube.Pages
{
    public partial class Setings : PhoneApplicationPage
    {
        ProgressIndicator progindicator;

        public Setings()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(Setings_Loaded);
        }

        void Setings_Loaded(object sender, RoutedEventArgs e)
        {
            progindicator = new ProgressIndicator();
            SystemTray.SetProgressIndicator(this, progindicator);
            bool usecellular = false;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("use_cellular",out usecellular);
            checkBox1.IsChecked = usecellular;
            App.GlobalMessaging.Changed += GlobalMessaging_Changed;
            UpdateUsageData();

        }

        private void UpdateUsageData()
        {
            long avstor = IsolatedStorageFile.GetUserStoreForApplication().AvailableFreeSpace / 1024 / 1024;
            long totald = 0;
            long sessiond = 0;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("total_data", out totald);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("session_data", out sessiond);
            totald = totald + (long)0.3 * totald;
            sessiond = sessiond + (long)0.3 * sessiond;
            avstorage.Text = avstor.ToString();
            totaldata.Text = totald.ToString();
            sessiondata.Text = sessiond.ToString();
        }

        void GlobalMessaging_Changed(object sender, Resources.APICompletedEventArgs e)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => {
                progindicator.Text = e.Response;
            });
            
        }

        private void Save_Click(object sender, EventArgs e)
        {
           
            IsolatedStorageSettings.ApplicationSettings["use_cellular"] = checkBox1.IsChecked;
            IsolatedStorageSettings.ApplicationSettings.Save();
            App.GlobalMessaging.SetMessage("Settings saved.");
            NavigationService.GoBack();
        }
                
    }
}