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

            List<string> list = RBACowyConnector.ListaTabel();
            Tabele.Items.Clear();
            foreach (string list1 in list)
                Tabele.Items.Add(list1);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Tabele.SelectedItems.Count == 0)
                MessageBox.Show("Musisz wybrać jakąś tabelę");
            else
            {
                OknoTabeli win2 = new OknoTabeli();
                win2.OtworzTabele(Tabele.SelectedItems[0].ToString());
                win2.Show();
            }
        }

        private void Tabele_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Tabele.SelectedItems.Count == 1)
            {
                OknoTabeli win2 = new OknoTabeli();
                win2.OtworzTabele(Tabele.SelectedItems[0].ToString());
                win2.Show();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (Tabele.SelectedItems.Count == 0)
                MessageBox.Show("Musisz wybrać jakąś tabelę");
            else
            {
                List<string> kluczGlowny = RBACowyConnector.KluczGlowny((string)Tabele.SelectedItem);
                List<KeyValuePair<string, string>> kolWart = new List<KeyValuePair<string, string>>();
                foreach (string kolumna in RBACowyConnector.ListaKolumn((string)Tabele.SelectedItem))
                {
                    if (!kluczGlowny.Contains(kolumna))
                    {
                        var w = new ProstyTextBox(kolumna); //a to niewygodne
                        if (w.ShowDialog() == true)
                            kolWart.Add(new KeyValuePair<string, string>(kolumna, w.TextDoPrzekazania));
                    }
                }
                try
                {
                    RBACowyConnector.Insert((string)Tabele.SelectedItem, kolWart);
                }
                catch (RBACowyConnector.Bledy blad)
                {
                    MessageBox.Show(blad.Message);
                }
            }
        }
    }
}