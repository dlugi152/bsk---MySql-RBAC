using System.Windows;

namespace bsk___proba_2
{
    /// <summary>
    /// Logika interakcji dla klasy UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        public UserWindow() {
            InitializeComponent();
            Wybór_Roli win2 = new Wybór_Roli();
            win2.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            OknoTabeli win2 = new OknoTabeli();
            win2.Show();
        }
    }
}
