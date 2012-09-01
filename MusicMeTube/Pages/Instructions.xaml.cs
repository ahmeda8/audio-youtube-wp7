using System;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace MusicMeTube.Pages
{
    public partial class Instructions : PhoneApplicationPage
    {
        public Instructions()
        {
            InitializeComponent();

        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            string about = "This application will access your youtube playlists and convert them to audio for you to listen offline. This App is a great tool "+
                "when you can't access zune to sync your mp3 files. This app will cache all your youtube songs for offline play."+Environment.NewLine;

            string instr = "1. login using your youtube account." + Environment.NewLine +
                "2. you will now be taken to playlists page , if you have any playlist they will show up. if u do not have any playlist select"
                + " manage playlists from menu, this will take you to youtube website, create a playlist and add songs to it." + Environment.NewLine +
                "3. Select the playlist, App will take you to tracks page , select sync n play , and the tracks will be cached offline, and then played" + Environment.NewLine;

            instuctiontxt.Text = instr;
            abttxt.Text = about;
            base.OnNavigatedTo(e);
        }

        private void contactme_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EmailComposeTask email = new EmailComposeTask();
            email.To = "ahmed_abrar@live.com";
            email.Subject = "musify Mytube user inquiry";
            email.Show();
        }
    }
}