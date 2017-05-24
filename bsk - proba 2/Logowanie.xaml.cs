using System;
using System.Collections.Generic;
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
        private string wybranyPlik;

        private void zaloguj()
        {
            try
            {
                RBACowyConnector.Inicjalizuj(AdresTextBox.Text, //ip
                    loginTextBox.Text,
                    hasloTextBox.Password,
                    PortTextBox.Text, wybranyPlik, HasłoCert.Password);
                if (RBACowyConnector.PierwszeLogowanie())
                {
                    while (true)
                    {
                        string stareHaslo, noweHaslo;
                        var w = new ProstyTextBox(
                            "Podaj stare hasło\n\n\n\n\n\n\n\n\nZamknij to okno, żeby \nzrezygnować ze zmiany\n" +
                            "i się wylogować");
                        if (w.ShowDialog() == true)
                            stareHaslo = w.TextDoPrzekazania;
                        else
                            return;
                        if (stareHaslo != hasloTextBox.Password)
                        {
                            MessageBox.Show("Użytkownik podał złe hasło. Spróbuj ponownie");
                            continue;
                        }
                        var w2 = new ProstyTextBox("Podaj nowe hasło");
                        if (w2.ShowDialog() == true)
                            noweHaslo = w2.TextDoPrzekazania;
                        else
                        {
                            MessageBox.Show("Użytkownik nie podał nowego hasła. Spróbuj ponownie");
                            continue;
                        }
                        var w3 = new ProstyTextBox("Powtórz nowe hasło");
                        if (w3.ShowDialog() != true || noweHaslo != w3.TextDoPrzekazania)
                        {
                            MessageBox.Show("Użytkownik nie powtórzył hasła. Spróbuj ponownie");
                            continue;
                        }
                        RBACowyConnector.AktualizujHasło(loginTextBox.Text, noweHaslo);
                        break;
                    }
                }
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();



            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".pem";
            dlg.Filter = "Pliki PEM (*.pem)|*.pem";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                wybranyPlik =  filename;
            }
        }
    }
}