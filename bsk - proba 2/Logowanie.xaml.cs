using System;
using System.Windows;
using System.Windows.Input;
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

        private void zaloguj()
        {
            try
            {
                RBACowyConnector.Inicjalizuj(AdresTextBox.Text, //ip
                    loginTextBox.Text,
                    hasloTextBox.Password,
                    PortTextBox.Text);
                RBACowyConnector.Inicjalizuj(AdresTextBox.Text, loginTextBox.Text, hasloTextBox.Password,
                    PortTextBox.Text);
                Wybór_Roli win2 = new Wybór_Roli(loginTextBox.Text);
                win2.ShowDialog();
                wybranoRolę = false;
                if (win2.DialogResult == true)
                {
                    wybranoRolę = true;
                    if (RBACowyConnector.CzyZalogowanyAdmin())
                    {
                        UserPermission win3 = new UserPermission();
                        win3.Show();
                    }
                    else
                    {
                        UserWindow win3 = new UserWindow();
                        win3.Show();
                    }
                    Close();
                }
                else
                    RBACowyConnector.ZamknijPolaczenie();
            }
            catch (RBACowyConnector.Bledy ex)
            {
                ObsługaBłędów.ObsłużBłąd(ex);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            zaloguj();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (wybranoRolę == false)
                RBACowyConnector.ZamknijPolaczenie();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                zaloguj();
        }
    }
}