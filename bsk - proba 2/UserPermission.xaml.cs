using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace bsk___proba_2
{
    /// <summary>
    /// Interaction logic for UserPermission.xaml
    /// </summary>
    public partial class UserPermission : Window {
        private string wybranyUżytkownik;
        public UserPermission()
        {
            InitializeComponent();
            foreach (string s in RBACowyConnector.ListaPracowników())
                ComboBoxUŻytkowników.Items.Add(s);
            foreach (string s in RBACowyConnector.ListaWszystkichRól()) {
                ListBoxWszystkichRól.Items.Add(s);
                ComboBoxEdycjiRól.Items.Add(s);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PrzeładujPrzypisaneRole(string użytkownik) {
            List<string> roleUżytkownika = RBACowyConnector.RoleUżytkownika(użytkownik);
            foreach (string s in roleUżytkownika)
                ListBoxPrzypisanychRól.SelectedItems.Add(s);
        }

        private void ComboBoxUŻytkowników_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            wybranyUżytkownik = ComboBoxUŻytkowników.SelectedItem.ToString();
            if (ListBoxPrzypisanychRól.Items.IsEmpty) {//wybieranie użytkownika pierwszy raz
                foreach (object item in ListBoxWszystkichRól.Items)
                    ListBoxPrzypisanychRól.Items.Add(item);
                ButtonZatwierdzanie.IsEnabled = true;
            }
            else
                ListBoxPrzypisanychRól.SelectedItems.Clear();
            PrzeładujPrzypisaneRole(wybranyUżytkownik);
            ListBoxPrzypisanychRól.Focus();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            bool wylogować = false;
            foreach (string item in ListBoxPrzypisanychRól.Items)
                if (ListBoxPrzypisanychRól.SelectedItems.Contains(item))
                    RBACowyConnector.DodajRolę(item, wybranyUżytkownik);
                else if (RBACowyConnector.UsuńRolę(item, wybranyUżytkownik) == false) {
                    MessageBoxResult dr =
                        MessageBox.Show(
                            "Próbujesz usunąć sobie rolę, którą aktualnie używasz.\n" +
                            "Kontynuować?\n" +
                            "Po wykonaniu zapytania zostaniesz wylogowany",
                            "Ostrzeżenie", MessageBoxButton.YesNo);
                    if (dr == MessageBoxResult.Yes || dr == MessageBoxResult.OK) {
                        RBACowyConnector.UsuńRolę(item, wybranyUżytkownik, true);
                        wylogować = true;
                    }
                }
            if (wylogować)
                Close();
            PrzeładujPrzypisaneRole(wybranyUżytkownik);
        }
    }
}
