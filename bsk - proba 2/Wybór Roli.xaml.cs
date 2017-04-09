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
            //todo uzupełnić możliwe role
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            //todo zapisz wybraną rolę do RBACConnectora
            Close();
        }
    }
}
