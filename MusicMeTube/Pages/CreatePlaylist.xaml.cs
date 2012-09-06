using System;
using Microsoft.Phone.Controls;
using System.ComponentModel;
using Resources;

namespace MusicMeTube.Pages
{
    public partial class CreatePlaylist : PhoneApplicationPage
    {
        BackgroundWorker addplay_worker;

        public CreatePlaylist()
        {
            InitializeComponent();
            addplay_worker = new BackgroundWorker();
            addplay_worker.DoWork += new DoWorkEventHandler(addplay_worker_DoWork);
            addplay_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(addplay_worker_RunWorkerCompleted);
        }

        void addplay_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressbar.IsEnabled = false;
            progressbar.IsIndeterminate = false;
            if (Add.ErrorOccured)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    System.Windows.MessageBox.Show("Could not add playlist");
                });
            }
            else
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    System.Windows.MessageBox.Show("playlist added, please refresh playlist page to see new playlist.");
                    NavigationService.GoBack();
                });
            }
        }

        void addplay_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string args = (string)e.Argument;
            string[] param = args.Split('|');
            Add.Playlist(param[0], param[1]);
            while (!Add.Completed)
            {
                System.Threading.Thread.Sleep(3000);
            }
        }

        private void save_click(object sender, EventArgs e)
        {
            progressbar.IsEnabled = true;
            progressbar.IsIndeterminate = true;
            string param = txtName.Text + "|" + txtDesc.Text;
            addplay_worker.RunWorkerAsync(param);
        }
    }
}