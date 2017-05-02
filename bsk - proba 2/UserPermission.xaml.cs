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
        private bool blokujPrzyciskPrzypisywania;
        private bool blokujEdycjęRoli;

        public UserPermission() {
            InitializeComponent();
            foreach (string s in RBACowyConnector.ListaPracowników())
                ComboBoxUŻytkowników.Items.Add(s);
            PrzeładujWszystkieRole();
            BlokujPrzyciski();
        }

        private void BlokujPrzyciski() {
            if (!RBACowyConnector.MożnaUsuwaćRole() || !RBACowyConnector.MożnaUsuwaćPrzypisania())
                ButtonUsuwaniaRól.IsEnabled = false;
            if (!RBACowyConnector.MożnaDodawaćPrzypisania() || !RBACowyConnector.MożnaUsuwaćPrzypisania())
                blokujPrzyciskPrzypisywania = true;
            else
                blokujPrzyciskPrzypisywania = false;
            if (!RBACowyConnector.MożnaDodawaćRole())
                ButtonTworzeniaRoli.IsEnabled = false;
            if (RBACowyConnector.MożnaEdytowaćRole())
                blokujEdycjęRoli = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StwórzEdytuj win2 = new StwórzEdytuj();
            win2.ShowDialog();
            PrzeładujWszystkieRole();
            PrzeładujZaznaczoneRole(wybranyUżytkownik);
        }

        private void PrzeładujZaznaczoneRole(string użytkownik) {
            List<string> roleUżytkownika = RBACowyConnector.RoleUżytkownika(użytkownik);
            ListBoxPrzypisanychRól.SelectedItems.Clear();
            foreach (string s in roleUżytkownika)
                ListBoxPrzypisanychRól.SelectedItems.Add(s);
        }

        private void PrzeładujWszystkieRole() {
            ComboBoxEdycjiRól.Items.Clear();
            ListBoxPrzypisanychRól.Items.Clear();
            ListBoxWszystkichRól.Items.Clear();
            foreach (string s in RBACowyConnector.ListaWszystkichRól()) {
                ListBoxWszystkichRól.Items.Add(s);
                ComboBoxEdycjiRól.Items.Add(s);
                ListBoxPrzypisanychRól.Items.Add(s);
            }
        }

        private void ComboBoxUŻytkowników_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            wybranyUżytkownik = ComboBoxUŻytkowników.SelectedItem.ToString();
            if (blokujPrzyciskPrzypisywania==false)
                ButtonZatwierdzanie.IsEnabled = true;
            PrzeładujZaznaczoneRole(wybranyUżytkownik);
            ListBoxPrzypisanychRól.Focus();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            bool wylogować = false;
            foreach (string item in ListBoxPrzypisanychRól.Items)
                if (ListBoxPrzypisanychRól.SelectedItems.Contains(item))
                    RBACowyConnector.DodajPrzypisanieRoli(item, wybranyUżytkownik);
                else if (RBACowyConnector.UsuńPrzypisanieRoli(item, wybranyUżytkownik) == false) {
                    MessageBoxResult dr =
                        MessageBox.Show(
                            "Próbujesz usunąć sobie rolę, którą aktualnie używasz.\n" +
                            "Kontynuować?\n" +
                            "Po wykonaniu zapytania zostaniesz wylogowany",
                            "Ostrzeżenie", MessageBoxButton.YesNo);
                    if (dr == MessageBoxResult.Yes || dr == MessageBoxResult.OK) {
                        RBACowyConnector.UsuńPrzypisanieRoli(item, wybranyUżytkownik, true);
                        wylogować = true;
                    }
                }
            if (wylogować)
                Close();
            PrzeładujZaznaczoneRole(wybranyUżytkownik);
        }

        private void ComboBoxEdycjiRól_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ButtonEdycjiRól.IsEnabled = true;
        }

        private void ButtonEdycjiRól_Click(object sender, RoutedEventArgs e) {
            if (ComboBoxEdycjiRól.SelectedItem == null) return;
            List<string> nazwyKolumn = RBACowyConnector.ListaKolumnRól();
            List<string> dane = RBACowyConnector.WierszRól(ComboBoxEdycjiRól.SelectedItem.ToString());
            StwórzEdytuj win2 = new StwórzEdytuj(nazwyKolumn,dane,blokujEdycjęRoli);
            win2.ShowDialog();
            PrzeładujWszystkieRole();
            PrzeładujZaznaczoneRole(wybranyUżytkownik);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            if (ListBoxWszystkichRól.SelectedItems == null) return;
            foreach (string item in ListBoxWszystkichRól.SelectedItems)
                RBACowyConnector.UsuńRolę(item);
            PrzeładujWszystkieRole();
            PrzeładujZaznaczoneRole(wybranyUżytkownik);
        }
    }
}
