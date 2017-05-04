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

        private bool wybranoRolę;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try {
                RBACowyConnector.Inicjalizuj(AdresTextBox.Text, //ip
                    loginTextBox.Text,
                    hasloTextBox.Password,
                    PortTextBox.Text);
                Wybór_Roli win2 = new Wybór_Roli();
                win2.ShowDialog();
                wybranoRolę = false;
                if (win2.DialogResult == true) {
                    wybranoRolę = true;
                    Close();
                }
                else//todo poprawić to, bo może się scrashować
                    RBACowyConnector.ZamknijPolaczenie();
            }
            catch (MySqlException ex) {
                MessageBox.Show(ex.Message + "\n" + ex.Number);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            if (wybranoRolę == false) 
                RBACowyConnector.ZamknijPolaczenie();
        }
    }
}
