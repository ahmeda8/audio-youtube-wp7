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

            string instr = "1. Login using your youtube account." + Environment.NewLine
                +"2. If you have any playlists in youtube they will show up. If u do not have any then add a playlist."+ Environment.NewLine
                +"3. Selecting the playlist, will take you to tracks page , select tracks you want to download by clicking checkbox next to the track"+Environment.NewLine
                +"4. You can search and add new racks from the tracks page." + Environment.NewLine
                +"5. Tracks downloaded will be cached offline and can be played anytime." + Environment.NewLine;

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