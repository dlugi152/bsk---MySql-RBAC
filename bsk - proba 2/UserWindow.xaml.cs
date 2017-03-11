using System.Windows;

namespace bsk___proba_2
{
    /// <summary>
    /// Logika interakcji dla klasy UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        public UserWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            OknoTabeli win2 = new OknoTabeli();
            win2.Show();
        }
    }
}
