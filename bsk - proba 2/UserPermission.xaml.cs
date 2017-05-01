using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace bsk___proba_2
{
    /// <summary>
    /// Interaction logic for UserPermission.xaml
    /// </summary>
    public partial class UserPermission : Window
    {
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

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ComboBoxUŻytkowników_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            string wybranyUżytkownik = ComboBoxUŻytkowników.SelectedItem.ToString();
            if (ListBoxPrzypisanychRól.Items.IsEmpty) {
                foreach (object item in ListBoxWszystkichRól.Items)
                    ListBoxPrzypisanychRól.Items.Add(item);
            }
            List<string> roleUżytkownika = RBACowyConnector.RoleUżytkownika(wybranyUżytkownik);
            foreach (string s in roleUżytkownika)
                ListBoxPrzypisanychRól.SelectedItems.Add(s);
            ListBoxPrzypisanychRól.Focus();
        }
    }
}
