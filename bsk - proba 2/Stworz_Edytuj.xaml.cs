using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.TextFormatting;

namespace bsk___proba_2
{
    /// <summary>
    /// Interaction logic for Role.xaml
    /// </summary>
    public partial class StwórzEdytuj : Window
    {
        Dictionary<string, string> MaxUprawnieniaAdmińskie;
        Dictionary<string, string> AktualneUprawnieniaAdmińskie;
        Dictionary<string, string> AktualneUprawnieniaUsera;
        private List<KeyValuePair<string, string>> idEdytowanej;
        private bool blokujEdycję;
        private bool blokujZaznaczanie;
        private bool calkowiteBlokowanie;
        private bool blokujBlokowanie=true;

        public StwórzEdytuj(Dictionary<string, string> maxUprawnieniaAdmińskie)
        {
            InitializeComponent();
            MaxUprawnieniaAdmińskie = maxUprawnieniaAdmińskie;
            CheckBoxAdmiński.IsChecked = false;
            ZaladujTabele();
            AktualneUprawnieniaAdmińskie = new Dictionary<string, string>();
            AktualneUprawnieniaUsera = new Dictionary<string, string>();
            foreach (string s in RBACowyConnector.ListaTabel(false))
                AktualneUprawnieniaUsera.Add(s, "----");
            foreach (string s in RBACowyConnector.ListaTabel(true))
                AktualneUprawnieniaAdmińskie.Add(s, "s---");
            idEdytowanej = null;
        }

        private bool porównywarka(string a, string b)
        {
            return a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
        }

        public StwórzEdytuj(Dictionary<string, string> maxUprawnieniaAdmińskie, List<string> nazwyKolumn,
            List<string> dane, bool blokujEdycjęRoli)
        {
            InitializeComponent();
            MaxUprawnieniaAdmińskie = maxUprawnieniaAdmińskie;
            calkowiteBlokowanie = blokujEdycjęRoli;
            AktualneUprawnieniaAdmińskie = new Dictionary<string, string>();
            AktualneUprawnieniaUsera = new Dictionary<string, string>();

            string nazwaKolmnyRól = RBACowyConnector.GetNazwaKolmnyRól();
            int findIndex = nazwyKolumn.FindIndex(s => porównywarka(s, nazwaKolmnyRól));
            TextBoxNazwy.Text = dane[findIndex];
            List<string> kluczGlownyRól = RBACowyConnector.KluczGlownyRól();
            idEdytowanej = new List<KeyValuePair<string, string>>();
            for (var i = 0; i < kluczGlownyRól.Count; i++)
                idEdytowanej.Add(new KeyValuePair<string, string>(kluczGlownyRól[i], dane[i]));

            string kolumnaCzyAdmin = RBACowyConnector.GetNazwaKolumnyCzyAdmin();

            for (var i = 0; i < nazwyKolumn.Count; i++)
            {
                string s = nazwyKolumn[i].ToLower();
                if (kluczGlownyRól.FindIndex(s2 => porównywarka(s2, s)) > -1
                    || porównywarka(nazwaKolmnyRól, s)
                    || porównywarka(kolumnaCzyAdmin, s)) continue;
                if (RBACowyConnector.CzyTabelaAdmińska(s))
                    AktualneUprawnieniaAdmińskie.Add(s, dane[i]);
                else
                    AktualneUprawnieniaUsera.Add(s, dane[i]);
            }
            CheckBoxAdmiński.IsChecked = AktualneUprawnieniaAdmińskie.Any(pair => pair.Value != "----");
            ZaladujTabele();
            PoblokujPrzyciski("");
        }

        private void PoblokujPrzyciski(string item)
        {
            if (blokujBlokowanie==false)
            CheckBoxAdmiński.IsEnabled = !calkowiteBlokowanie;
            if (blokujBlokowanie == false)
                TextBoxNazwy.IsEnabled = !calkowiteBlokowanie;
            if (item != "")
            {
                if (blokujBlokowanie == false)
                    if (CheckBoxAdmiński.IsChecked == true)
                {
                    CheckBoxEdycji.IsEnabled = !calkowiteBlokowanie && MaxUprawnieniaAdmińskie[item].Contains('u');
                    CheckBoxOdczytu.IsEnabled = !calkowiteBlokowanie && MaxUprawnieniaAdmińskie[item].Contains('s');
                    CheckBoxUsuwania.IsEnabled = !calkowiteBlokowanie && MaxUprawnieniaAdmińskie[item].Contains('d');
                    CheckBoxZapisu.IsEnabled = !calkowiteBlokowanie && MaxUprawnieniaAdmińskie[item].Contains('i');
                }
            }
            else
            if (blokujBlokowanie == false)
            {
                CheckBoxEdycji.IsEnabled = true;
                CheckBoxOdczytu.IsEnabled = true;
                CheckBoxUsuwania.IsEnabled = true;
                CheckBoxZapisu.IsEnabled = true;
            }
        }

        private void ZaladujTabele()
        {
            List<string> listaTabel = RBACowyConnector.ListaTabel(CheckBoxAdmiński.IsChecked);
            ComboBoxTabel.Items.Clear();
            foreach (string s in listaTabel)
                ComboBoxTabel.Items.Add(s);
        }

        private void CheckBoxAdmiński_Check(object sender, RoutedEventArgs e)
        {
            ZaladujTabele();
            CheckBoxOdczytu.IsChecked = CheckBoxZapisu.IsChecked =
                CheckBoxEdycji.IsChecked = CheckBoxUsuwania.IsChecked = false;
            if (CheckBoxAdmiński.IsChecked != true && blokujEdycję == false || blokujBlokowanie)
                CheckBoxOdczytu.IsEnabled = true;
        }

        private bool CzyZaznaczyć(string nazwa, char uprawnienie)
        {
            return CheckBoxAdmiński.IsChecked == true
                ? AktualneUprawnieniaAdmińskie[nazwa].Contains(uprawnienie.ToString())
                : AktualneUprawnieniaUsera[nazwa].Contains(uprawnienie.ToString());
        }

        private void ComboBoxTabel_SelectionChanged(object sender,
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            PoblokujPrzyciski(e.AddedItems.Count == 1 ? e.AddedItems[0] as string : "");
            blokujZaznaczanie = true;
            if (ComboBoxTabel.SelectedItem == null) return;
            if (CheckBoxAdmiński.IsChecked != true)
                CheckBoxOdczytu.IsChecked = CzyZaznaczyć(ComboBoxTabel.SelectedItem.ToString(), 's');
            else
            {
                CheckBoxOdczytu.IsChecked = true;
                if (blokujBlokowanie == false)
                    CheckBoxOdczytu.IsEnabled = false;
            }
            CheckBoxZapisu.IsChecked = CzyZaznaczyć(ComboBoxTabel.SelectedItem.ToString(), 'i');
            CheckBoxEdycji.IsChecked = CzyZaznaczyć(ComboBoxTabel.SelectedItem.ToString(), 'u');
            CheckBoxUsuwania.IsChecked = CzyZaznaczyć(ComboBoxTabel.SelectedItem.ToString(), 'd');
            blokujZaznaczanie = false;
        }

        private void ZaktualizujUprawnienie(object nazwa, char uprawnienie)
        {
            if (nazwa == null || blokujZaznaczanie) return;
            string tabela = nazwa.ToString();
            var sb = CheckBoxAdmiński.IsChecked == true
                ? new StringBuilder(AktualneUprawnieniaAdmińskie[tabela])
                : new StringBuilder(AktualneUprawnieniaUsera[tabela]);
            switch (uprawnienie)
            {
                case 's':
                    if (CheckBoxAdmiński.IsChecked == true)
                        MessageBox.Show("Uprawnienia admina muszą obejmować select do wszystkich tabel admińskich");
                    else
                    {
                        if (sb[0] != '-')
                        {
                            if (CheckBoxEdycji.IsChecked == true)
                                CheckBoxEdycji.IsChecked = false;
                            if (CheckBoxUsuwania.IsChecked == true)
                                CheckBoxUsuwania.IsChecked = false;
                            sb = CheckBoxAdmiński.IsChecked == true
                                ? new StringBuilder(AktualneUprawnieniaAdmińskie[tabela])
                                : new StringBuilder(AktualneUprawnieniaUsera[tabela]);
                        }
                        sb[0] = sb[0] == '-' ? 's' : '-';
                    }
                    break;
                case 'i':
                    sb[1] = sb[1] == '-' ? 'i' : '-';
                    break;
                case 'u':
                    if (sb[2] == '-' && CheckBoxOdczytu.IsChecked == false)
                    {
                        CheckBoxOdczytu.IsChecked = true;
                        sb = CheckBoxAdmiński.IsChecked == true
                            ? new StringBuilder(AktualneUprawnieniaAdmińskie[tabela])
                            : new StringBuilder(AktualneUprawnieniaUsera[tabela]);
                    }
                    sb[2] = sb[2] == '-' ? 'u' : '-';
                    break;
                case 'd':
                    if (sb[3] == '-' && CheckBoxOdczytu.IsChecked == false)
                    {
                        CheckBoxOdczytu.IsChecked = true;
                        sb = CheckBoxAdmiński.IsChecked == true
                            ? new StringBuilder(AktualneUprawnieniaAdmińskie[tabela])
                            : new StringBuilder(AktualneUprawnieniaUsera[tabela]);
                    }
                    sb[3] = sb[3] == '-' ? 'd' : '-';
                    break;
            }
            if (CheckBoxAdmiński.IsChecked == true)
                AktualneUprawnieniaAdmińskie[tabela] = sb.ToString();
            else
                AktualneUprawnieniaUsera[tabela] = sb.ToString();
        }

        private void CheckBoxZapisu_Check(object sender, RoutedEventArgs e)
        {
            ZaktualizujUprawnienie(ComboBoxTabel.SelectedItem, 'i');
        }

        private void CheckBoxOdczytu_Check(object sender, RoutedEventArgs e)
        {
            ZaktualizujUprawnienie(ComboBoxTabel.SelectedItem, 's');
        }

        private void CheckBoxEdycji_Check(object sender, RoutedEventArgs e)
        {
            ZaktualizujUprawnienie(ComboBoxTabel.SelectedItem, 'u');
        }

        private void CheckBoxUsuwania_Check(object sender, RoutedEventArgs e)
        {
            ZaktualizujUprawnienie(ComboBoxTabel.SelectedItem, 'd');
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (idEdytowanej == null || blokujEdycję != true)
            {
                var kolAtr = CheckBoxAdmiński.IsChecked == true
                    ? new List<KeyValuePair<string, string>>(AktualneUprawnieniaAdmińskie)
                    : new List<KeyValuePair<string, string>>(AktualneUprawnieniaUsera);
                try
                {
                    if (idEdytowanej == null)
                        RBACowyConnector.NowaRola(TextBoxNazwy.Text, kolAtr);
                    else
                    {
                        kolAtr.Add(new KeyValuePair<string, string>(RBACowyConnector.GetNazwaKolmnyRól(),
                            TextBoxNazwy.Text));
                        RBACowyConnector.EdycjaRoli(idEdytowanej, kolAtr);
                    }
                }
                catch (RBACowyConnector.Bledy ex)
                {
                    ObsługaBłędów.ObsłużBłąd(ex);
                }
            }
            Close();
        }
    }
}