using System.Windows;
using MySql.Data.MySqlClient;

namespace bsk___proba_2
{
    /// <summary>
    /// Logika interakcji dla klasy Wybór_Roli.xaml
    /// </summary>
    public partial class Wybór_Roli : Window {
        public Wybór_Roli(string login)
        {
            InitializeComponent();
            foreach (string s in RBACowyConnector.MojeRoleNazwy(login))
                ComboBox.Items.Add(s);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            RBACowyConnector.UstawAktualnąRolę(ComboBox.SelectionBoxItem.ToString());
            DialogResult = true;
            Close();
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            Button.IsEnabled = true;
        }
    }
}
