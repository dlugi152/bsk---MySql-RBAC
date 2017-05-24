using System;
using System.Collections.Generic;
using System.Windows;

namespace bsk___proba_2
{
    /// <summary>
    /// Logika interakcji dla klasy UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        public UserWindow()
        {
            InitializeComponent();

            List<string> list = RBACowyConnector.ListaTabel(null);
            Tabele.Items.Clear();
            foreach (string list1 in list)
                Tabele.Items.Add(list1);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Tabele.SelectedItems.Count == 0)
                MessageBox.Show("Musisz wybrać jakąś tabelę");
            else
                OtwórzTabelę(Tabele.SelectedItems[0].ToString());
        }

        private void Tabele_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Tabele.SelectedItems.Count == 1)
                OtwórzTabelę(Tabele.SelectedItems[0].ToString());
        }

        private void OtwórzTabelę(string tabela)
        {
            OknoTabeli win2 = new OknoTabeli();
            win2.OtworzTabele(Tabele.SelectedItems[0].ToString());
            if (RBACowyConnector.CanSelect(Tabele.SelectedItems[0].ToString()))
                win2.Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (Tabele.SelectedItems.Count == 0)
                MessageBox.Show("Musisz wybrać jakąś tabelę");
            else
            {
                List<string> kluczGlowny = RBACowyConnector.KluczGlowny((string) Tabele.SelectedItem);
                List<KeyValuePair<string, string>> kolWart = new List<KeyValuePair<string, string>>();
                foreach (string kolumna in RBACowyConnector.ListaKolumn((string) Tabele.SelectedItem))
                {
                    if (!kluczGlowny.Contains(kolumna))
                    {
                        var w = new ProstyTextBox(kolumna); //a to niewygodne
                        if (w.ShowDialog() == true)
                            kolWart.Add(new KeyValuePair<string, string>(kolumna, w.TextDoPrzekazania));
                        else return;
                    }
                }
                try
                {
                    RBACowyConnector.Insert((string) Tabele.SelectedItem, kolWart);
                }
                catch (RBACowyConnector.Bledy blad)
                {
                    ObsługaBłędów.ObsłużBłąd(blad);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            RBACowyConnector.ZamknijPolaczenie();
        }

        private void Tabele_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Tabele.SelectedItems.Count == 1)
            {
                ButtonWyświetl.IsEnabled = RBACowyConnector.CanSelect(Tabele.SelectedItems[0].ToString());
                ButtonDodaj.IsEnabled = RBACowyConnector.CanInsert(Tabele.SelectedItems[0].ToString());
            }
            else
            {
                ButtonDodaj.IsEnabled = false;
                ButtonWyświetl.IsEnabled = false;
            }
        }
    }
}