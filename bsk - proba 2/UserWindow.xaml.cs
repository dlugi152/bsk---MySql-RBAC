using System.Collections.Generic;
using System.Windows;

namespace bsk___proba_2
{
    /// <summary>
    /// Logika interakcji dla klasy UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        public UserWindow() {
            InitializeComponent();
            Wybór_Roli win2 = new Wybór_Roli();
            win2.ShowDialog();

            List<string> list = RBACowyConnector.ListaTabel();
            Tabele.Items.Clear();
            foreach (string list1 in list)
                Tabele.Items.Add(list1);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (Tabele.SelectedItems.Count == 0)
                MessageBox.Show("Musisz wybrać jakąś tabelę");
            else {
                OknoTabeli win2 = new OknoTabeli();
                win2.OtworzTabele(Tabele.SelectedItems[0].ToString());
                win2.Show();
            }
        }

        private void Tabele_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Tabele.SelectedItems.Count == 1) {
                OknoTabeli win2 = new OknoTabeli();
                win2.OtworzTabele(Tabele.SelectedItems[0].ToString());
                win2.Show();
            }
        }
    }
}
