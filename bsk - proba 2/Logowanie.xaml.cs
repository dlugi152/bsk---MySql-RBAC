using System.Windows;

namespace bsk___proba_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UsrButton.IsChecked = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (AdmButton.IsChecked == true) {
                UserPermission win2 = new UserPermission();
                win2.Show();
                Close();
            }
            else {
                Hide();
                UserWindow win2 = new UserWindow();
                win2.Show();
                Close();
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e) {
            AdmButton.IsChecked = false;
        }

        private void AdmButton_Checked(object sender, RoutedEventArgs e) {
            UsrButton.IsChecked = false;
        }
    }
}
