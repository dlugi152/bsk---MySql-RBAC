using System.Windows;

namespace bsk___proba_2
{
    /// <summary>
    /// Logika interakcji dla klasy Tworzenie_tunelu_ssh.xaml
    /// </summary>
    public partial class Tworzenie_tunelu_ssh : Window
    {
        public Tworzenie_tunelu_ssh()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow win2 = new MainWindow();
            win2.Show();
            Close();
        }
    }
}
