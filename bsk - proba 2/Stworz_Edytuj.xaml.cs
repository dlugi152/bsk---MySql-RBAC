using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media.TextFormatting;

namespace bsk___proba_2
{
    /// <summary>
    /// Interaction logic for Role.xaml
    /// </summary>
    public partial class StwórzEdytuj : Window {
        Dictionary<string, string> AktualneUprawnieniaAdmińskie;
        Dictionary<string, string> AktualneUprawnieniaUsera;

        public StwórzEdytuj() {
            InitializeComponent();
            CheckBoxAdmiński.IsChecked = false;
            ZaladujTabele();
            AktualneUprawnieniaAdmińskie = new Dictionary<string, string>();
            AktualneUprawnieniaUsera = new Dictionary<string, string>();
            foreach (string s in RBACowyConnector.ListaTabel(false))
                AktualneUprawnieniaUsera.Add(s, "----");
            foreach (string s in RBACowyConnector.ListaTabel(true))
                AktualneUprawnieniaAdmińskie.Add(s, "----");
        }

        private void ZaladujTabele() {
            List<string> listaTabel = RBACowyConnector.ListaTabel(CheckBoxAdmiński.IsChecked);
            ComboBoxTabel.Items.Clear();
            foreach (string s in listaTabel)
                ComboBoxTabel.Items.Add(s);
        }

        private void CheckBoxAdmiński_Click(object sender, RoutedEventArgs e) {
            ZaladujTabele();
            CheckBoxOdczytu.IsChecked = CheckBoxZapisu.IsChecked =
                CheckBoxEdycji.IsChecked = CheckBoxUsuwania.IsChecked = false;
        }

        private bool CzyZaznaczyć(string nazwa, char uprawnienie) {
            return CheckBoxAdmiński.IsChecked == true
                ? AktualneUprawnieniaAdmińskie[nazwa].Contains(uprawnienie.ToString())
                : AktualneUprawnieniaUsera[nazwa].Contains(uprawnienie.ToString());
        }

        private void ComboBoxTabel_SelectionChanged(object sender,
            System.Windows.Controls.SelectionChangedEventArgs e) {
            if (ComboBoxTabel.SelectedItem == null) return;
            CheckBoxOdczytu.IsChecked = CzyZaznaczyć(ComboBoxTabel.SelectedItem.ToString(), 's');
            CheckBoxZapisu.IsChecked = CzyZaznaczyć(ComboBoxTabel.SelectedItem.ToString(), 'i');
            CheckBoxEdycji.IsChecked = CzyZaznaczyć(ComboBoxTabel.SelectedItem.ToString(), 'u');
            CheckBoxUsuwania.IsChecked = CzyZaznaczyć(ComboBoxTabel.SelectedItem.ToString(), 'd');
        }

        private void ZaktualizujUprawnienie(object nazwa, char uprawnienie) {
            if (nazwa == null) return;
            string tabela = nazwa.ToString();
            var sb = CheckBoxAdmiński.IsChecked == true
                ? new StringBuilder(AktualneUprawnieniaAdmińskie[tabela])
                : new StringBuilder(AktualneUprawnieniaUsera[tabela]);
            switch (uprawnienie) {
                case 's':
                    sb[0] = sb[0] == '-' ? 's' : '-';
                    break;
                case 'i':
                    sb[1] = sb[1] == '-' ? 'i' : '-';
                    break;
                case 'u':
                    sb[2] = sb[2] == '-' ? 'u' : '-';
                    break;
                case 'd':
                    sb[3] = sb[3] == '-' ? 'd' : '-';
                    break;
            }
            if (CheckBoxAdmiński.IsChecked == true)
                AktualneUprawnieniaAdmińskie[tabela] = sb.ToString();
            else
                AktualneUprawnieniaUsera[tabela] = sb.ToString();
        }

        private void CheckBoxZapisu_Click(object sender, RoutedEventArgs e) {
            ZaktualizujUprawnienie(ComboBoxTabel.SelectedItem, 'i');
        }

        private void CheckBoxOdczytu_Click(object sender, RoutedEventArgs e) {
            ZaktualizujUprawnienie(ComboBoxTabel.SelectedItem, 's');
        }

        private void CheckBoxEdycji_Click(object sender, RoutedEventArgs e) {
            ZaktualizujUprawnienie(ComboBoxTabel.SelectedItem, 'u');
        }

        private void CheckBoxUsuwania_Click(object sender, RoutedEventArgs e) {
            ZaktualizujUprawnienie(ComboBoxTabel.SelectedItem, 'd');
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var kolAtr = CheckBoxAdmiński.IsChecked == true
                ? new List<KeyValuePair<string, string>>(AktualneUprawnieniaAdmińskie)
                : new List<KeyValuePair<string, string>>(AktualneUprawnieniaUsera);
            RBACowyConnector.NowaRola(TextBoxNazwy.Text, kolAtr);
            Close();
        }
    }
}
