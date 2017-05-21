using System;
using System.Windows;
using MySql.Data.MySqlClient;

namespace bsk___proba_2
{
    /// <summary>
    /// Logika interakcji dla klasy Wybór_Roli.xaml
    /// </summary>
    public partial class Wybór_Roli : Window
    {
        private string login;
        public Wybór_Roli(string login)
        {
            this.login = login;
            InitializeComponent();
            foreach (string s in RBACowyConnector.MojeRoleNazwy(login))
                ComboBox.Items.Add(s);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RBACowyConnector.UstawAktualnąRolę(login, ComboBox.SelectionBoxItem.ToString());
                DialogResult = true;
                Close();
            }
            catch (RBACowyConnector.Bledy bledy)
            {
                ObsługaBłędów.ObsłużBłąd(bledy);
            }
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Button.IsEnabled = true;
        }
    }
}
