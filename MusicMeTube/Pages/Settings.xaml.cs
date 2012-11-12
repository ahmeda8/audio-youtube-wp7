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