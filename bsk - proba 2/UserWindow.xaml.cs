using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace bsk___proba_2
{
    /// <summary>
    /// Logika interakcji dla klasy UserWindow.xaml
    /// </summary>
    public partial class UserWindow
    {
        public UserWindow() {
            InitializeComponent();
            RBACowyConnector.Inicjalizuj("localhost", "15000");

            List<string> list = RBACowyConnector.ListaTabel();
            Tabele.Items.Clear();
            foreach (string list1 in list)
                Tabele.Items.Add(list1);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (Tabele.SelectedItems.Count == 0)
                MessageBox.Show("Musisz wybrać jakąś tabelę");
            else
                OtwórzTabelę();
        }

        private void Tabele_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (Tabele.SelectedItems.Count == 1)
                OtwórzTabelę();
        }

        private void OtwórzTabelę() {
            OknoTabeli win2 = new OknoTabeli {Title = Tabele.SelectedItems[0].ToString()};
            win2.OtworzTabele(Tabele.SelectedItems[0].ToString());
            win2.Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            if (Tabele.SelectedItems.Count == 0)
                MessageBox.Show("Musisz wybrać jakąś tabelę");
            else {
                List<string> kluczGlowny = RBACowyConnector.KluczGlowny((string)Tabele.SelectedItem);
                List<KeyValuePair<string, string>> kolWart = new List<KeyValuePair<string, string>>();
                foreach (string kolumna in RBACowyConnector.ListaKolumn((string)Tabele.SelectedItem)) {
                    if (!kluczGlowny.Contains(kolumna)) {
                        var w = new ProstyTextBox(kolumna); //a to niewygodne
                        if (w.ShowDialog() == true)
                            kolWart.Add(new KeyValuePair<string, string>(kolumna, w.TextDoPrzekazania));
                        else
                            return;
                    }
                }
                try {
                    RBACowyConnector.Insert((string)Tabele.SelectedItem, kolWart);
                }
                catch (RBACowyConnector.Bledy blad) {
                    ObsługaBłędów.ObsłużBłąd(blad);
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            RBACowyConnector.ZamknijPolaczenie();
        }

        private void Tabele_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ButtonWyświetl.IsEnabled = true;
            ButtonDodaj.IsEnabled = true;
        }
    }
}