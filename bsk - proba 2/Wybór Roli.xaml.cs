using System.Windows;
using MySql.Data.MySqlClient;

namespace bsk___proba_2
{
    /// <summary>
    /// Logika interakcji dla klasy Wybór_Roli.xaml
    /// </summary>
    public partial class Wybór_Roli : Window {
        public Wybór_Roli()
        {
            InitializeComponent();
            foreach (string s in RBACowyConnector.MojeRoleNazwy())
                ComboBox.Items.Add(s);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            RBACowyConnector.UstawRole(ComboBox.SelectionBoxItem.ToString());
            if (RBACowyConnector.CzyZalogowanyAdmin()) {
                UserPermission win2 = new UserPermission();
                win2.Show();
            }
            else {
                UserWindow win2 = new UserWindow();
                win2.Show();
            }
            DialogResult = true;
            Close();
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            Button.IsEnabled = true;
        }
    }
}
