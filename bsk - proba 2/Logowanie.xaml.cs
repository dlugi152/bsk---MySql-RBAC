using System;
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
            try {
                RBACowyConnector.Inicjalizuj(AdresTextBox.Text, //ip
                    loginTextBox.Text,
                    hasloTextBox.Password,
                    PortTextBox.Text);
                Wybór_Roli win2 = new Wybór_Roli();
                win2.ShowDialog();
                if (win2.DialogResult==true)
                Close();

            }
            catch (MySqlException ex) {
                MessageBox.Show(ex.Message + "\n" + ex.Number);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
