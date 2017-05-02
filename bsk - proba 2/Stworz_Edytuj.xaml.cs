using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<KeyValuePair<string, string>> idEdytowanej;

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
            idEdytowanej = null;
        }

        private bool porównywarka(string a, string b) {
            return a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
        }

        public StwórzEdytuj(List<string> nazwyKolumn, List<string> dane) {
            InitializeComponent();
            AktualneUprawnieniaAdmińskie = new Dictionary<string, string>();
            AktualneUprawnieniaUsera = new Dictionary<string, string>();

            string nazwaKolmnyRól = RBACowyConnector.GetNazwaKolmnyRól();
            int findIndex = nazwyKolumn.FindIndex(s => porównywarka(s,nazwaKolmnyRól));
            TextBoxNazwy.Text = dane[findIndex];
            List<string> kluczGlownyRól = RBACowyConnector.KluczGlownyRól();
            idEdytowanej = new List<KeyValuePair<string, string>>();
            for (var i = 0; i < kluczGlownyRól.Count; i++)
                idEdytowanej.Add(new KeyValuePair<string, string>(kluczGlownyRól[i],dane[i]));

            string kolumnaCzyAdmin = RBACowyConnector.GetNazwaKolumnyCzyAdmin();

            for (var i = 0; i < nazwyKolumn.Count; i++) {
                string s = nazwyKolumn[i].ToLower();
                if (kluczGlownyRól.FindIndex(s2 => porównywarka(s2,s))>-1
                                                   || porównywarka(nazwaKolmnyRól,s)
                                                   || porównywarka(kolumnaCzyAdmin,s)) continue;
                if (RBACowyConnector.CzyTabelaAdmińska(s))
                    AktualneUprawnieniaAdmińskie.Add(s, dane[i]);
                else
                    AktualneUprawnieniaUsera.Add(s, dane[i]);
            }
            CheckBoxAdmiński.IsChecked = AktualneUprawnieniaAdmińskie.Any(pair => pair.Value != "----");
            ZaladujTabele();
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
            try
            {
                if (idEdytowanej == null)
                    RBACowyConnector.NowaRola(TextBoxNazwy.Text, kolAtr);
                else
                {
                    kolAtr.Add(new KeyValuePair<string, string>(RBACowyConnector.GetNazwaKolmnyRól(), TextBoxNazwy.Text));
                    RBACowyConnector.EdycjaRoli(idEdytowanej, kolAtr);
                }
            }
            catch (RBACowyConnector.Bledy ex)
            {
                ObsługaBłędów.ObsłużBłąd(ex.Kod,ex.Wiadomosc);
            }
            Close();
        }
    }
}
