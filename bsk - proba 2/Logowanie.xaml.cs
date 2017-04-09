using System.Windows;
using MySql.Data.MySqlClient;

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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var myConnectionString = "server="+adresTextBox.Text+";uid="+loginTextBox.Text+";" +
                                        "pwd="+hasloTextBox.Password+";database=bsk;port="+portTextBox.Text + ";";

            try {
                RBACowyConnector.Inicjalizuj(adresTextBox.Text,//ip
                    "bsk",//baza
                    loginTextBox.Text,
                    hasloTextBox.Password,
                    portTextBox.Text);
                RBACowyConnector.TestujPolaczenie();//jeśli źle to rzuci wyjątkiem
                UserWindow win2 = new UserWindow();
                win2.Show();
                Close();

            }
            catch (MySqlException ex) {
                MessageBox.Show(ex.Message + "\n" + ex.Number.ToString());
            }
        }
    }
}
