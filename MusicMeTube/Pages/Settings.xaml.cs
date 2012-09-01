using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;

namespace MusicMeTube.Pages
{
    public partial class Setings : PhoneApplicationPage
    {
        public Setings()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(Setings_Loaded);
        }

        void Setings_Loaded(object sender, RoutedEventArgs e)
        {
            bool usecellular = false;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("use_cellular",out usecellular);
            checkBox1.IsChecked = usecellular;

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
           
            IsolatedStorageSettings.ApplicationSettings["use_cellular"] = checkBox1.IsChecked;
            IsolatedStorageSettings.ApplicationSettings.Save();
            MessageBox.Show("Settings Saved");
            NavigationService.GoBack();
        }
    }
}