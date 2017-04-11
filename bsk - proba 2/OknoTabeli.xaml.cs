﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace bsk___proba_2 {
    /// <summary>
    /// Logika interakcji dla klasy OknoTabeli.xaml
    /// </summary>
    public partial class OknoTabeli : Window {
        private List<string> kluczGlowny;
        private string tabela;

        public OknoTabeli() {
            InitializeComponent();
        }

        private void dopasujRozmiar() {
            SizeToContent = SizeToContent.WidthAndHeight;
            if (Height > 700)
                Height = 700;
        }

        private void PrzeladujDane() {
            try {
                TabelaDataGrid.Items.Clear();
                TabelaDataGrid.Columns.Clear();
                List<List<string>> listaZDanymi = RBACowyConnector.Select(tabela);
                List<string> listaKolumn = RBACowyConnector.ListaKolumn(tabela);
                foreach (var kolumna in listaKolumn) {
                    DataGridTextColumn textColumn = new DataGridTextColumn {
                        Header = kolumna,
                        Binding = new Binding(kolumna)
                    };
                    TabelaDataGrid.Columns.Add(textColumn);
                }
                foreach (var wiersz in listaZDanymi) {
                    dynamic exo = new System.Dynamic.ExpandoObject();
                    for (var i = 0; i < listaKolumn.Count; i++)
                        //trochę fartowne - kolumny mogą być nie w tej kolejności
                        ((IDictionary<string, object>) exo).Add(listaKolumn[i], wiersz[i]);
                    TabelaDataGrid.Items.Add(exo);
                }
                dopasujRozmiar();
            }
            catch (RBACowyConnector.Bledy blad) {
                ObslugaBledow(blad.Kod,blad.Wiadomosc);
            }
        }

        public void OtworzTabele(string tabela) {
            kluczGlowny = RBACowyConnector.KluczGlowny(tabela);
            this.tabela = tabela;
            if (RBACowyConnector.CanDelete(this.tabela) == false) {
                usunButton.IsEnabled = false;
                if (TabelaDataGrid.ContextMenu != null)
                    TabelaDataGrid.ContextMenu.Items.Remove(usunMenuItem);
            }
            if (RBACowyConnector.CanInsert(this.tabela) == false)
                dodajButton.IsEnabled = false;
            if (RBACowyConnector.CanUpdate(this.tabela) == false) {
                edytujButton.IsEnabled = false;
                TabelaDataGrid.IsReadOnly = true;
                if (TabelaDataGrid.ContextMenu != null)
                    TabelaDataGrid.ContextMenu.Items.Remove(edytujMenuItem);
            }

            PrzeladujDane();
        }

        //dodawanie
        private void Button_Click(object sender, RoutedEventArgs e) {
            List<KeyValuePair<string, string>> kolWart = new List<KeyValuePair<string, string>>();
            foreach (DataGridColumn kolumna in TabelaDataGrid.Columns) {
                string klucz = kolumna.Header.ToString();
                if (!kluczGlowny.Contains(kolumna.Header.ToString())) {
                    var w = new ProstyTextBox(klucz); //a to niewygodne
                    if (w.ShowDialog() == true)
                        kolWart.Add(new KeyValuePair<string, string>(klucz, w.TextDoPrzekazania));
                }
            }
            try {
                RBACowyConnector.Insert(tabela, kolWart);
            }
            catch (RBACowyConnector.Bledy blad) {
                ObslugaBledow(blad.Kod,blad.Wiadomosc);
            }
            PrzeladujDane(); //najprościej i bezbłędnie
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            var zaznaczone = TabelaDataGrid.SelectedItems;
            if (zaznaczone.Count > 0) {
                foreach (var o in zaznaczone)
                {
                    var idKlucza = new List<KeyValuePair<string, string>>();
                    foreach (var s in kluczGlowny)
                        idKlucza.Add(new KeyValuePair<string, string>(s, ((IDictionary<string, object>) o)[s].ToString()));
                    try {
                        RBACowyConnector.Delete(tabela, idKlucza);
                    }
                    catch (RBACowyConnector.Bledy blad) {
                        ObslugaBledow(blad.Kod, blad.Wiadomosc);
                    }
                }
                PrzeladujDane();
            }
            else
                MessageBox.Show("Musisz zaznaczyć jakieś wiersze (o ile jakieś są), jeśli chcesz je usunąć");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            var zaznaczone = TabelaDataGrid.SelectedItems;
            switch (zaznaczone.Count) {
                case 0:
                    MessageBox.Show("Musisz zaznaczyć jakiś wiersz, jeśli chcesz go edytować");
                    break;
                case 1:
                    //nie zmieniać na foreach
                    for (var i = 0; i < zaznaczone.Count; i++) {
                        var t = (IDictionary<string, object>) zaznaczone[i];
                        TabelaDataGrid_BeginningEdit(null,
                            new DataGridBeginningEditEventArgs(TabelaDataGrid.Columns[0], new DataGridRow {Item = t},
                                null)); //ważne, że działa
                    }
                    //nie trzeba przeładowywać
                    break;
                default:
                    MessageBox.Show("Nie możesz zaznaczyć więcej niż jednego wiersza do edycji");
                    break;
            }
        }

        private void TabelaDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e) {
            IDictionary<string, object> wiersz = (IDictionary<string, object>) e.Row.Item;
            var kolWart = new List<KeyValuePair<string, string>>();
            var idKlucza = new List<KeyValuePair<string, string>>();
            foreach (var kolumna in TabelaDataGrid.Columns) {
                string nazwaKolumny = kolumna.Header.ToString();
                if (kluczGlowny.Contains(nazwaKolumny)) {
                    idKlucza.Add(new KeyValuePair<string, string>(nazwaKolumny, wiersz[nazwaKolumny].ToString()));
                    continue;
                }
                var w = new ProstyTextBox(nazwaKolumny, wiersz[nazwaKolumny].ToString());
                if (w.ShowDialog() == true) {
                    wiersz[nazwaKolumny] = w.TextDoPrzekazania;
                    kolWart.Add(new KeyValuePair<string, string>(nazwaKolumny, w.TextDoPrzekazania));
                    e.Row.Item = wiersz;
                }
            }
            try {
                RBACowyConnector.Update(tabela, kolWart, idKlucza);
            }
            catch (RBACowyConnector.Bledy blad) {
                ObslugaBledow(blad.Kod, blad.Wiadomosc);
            }
            finally {
                PrzeladujDane();
                e.Cancel = true; //ta jasne, true...
            }
        }

        private void ObslugaBledow(RBACowyConnector.KodyBledow kod, string wiadomosc) {
            switch (kod) {
                case RBACowyConnector.KodyBledow.BlednyLoginHaslo:
                    MessageBox.Show("Błędny login lub hasło");
                    break;
                case RBACowyConnector.KodyBledow.BladLaczenia:
                    MessageBox.Show("Błąd podczas łaczenia z bazą");
                    break;
                case RBACowyConnector.KodyBledow.InnyBlad:
                    MessageBox.Show("Nieznany błąd");
                    break;
                case RBACowyConnector.KodyBledow.NieMoznaZamknac:
                    MessageBox.Show("Nie masz praw do edytowania");
                    break;
                case RBACowyConnector.KodyBledow.BrakInsert:
                    MessageBox.Show("Nie masz praw do insertowania");
                    break;
                case RBACowyConnector.KodyBledow.BrakSelect:
                    MessageBox.Show("Nie masz praw do selectowania");
                    break;
                case RBACowyConnector.KodyBledow.BrakDelete:
                    MessageBox.Show("Nie masz praw do deletowania");
                    break;
                case RBACowyConnector.KodyBledow.BrakUpdate:
                    MessageBox.Show("Nie masz praw do edytowania");
                    break;
                case RBACowyConnector.KodyBledow.BledneZapytanie:
                    MessageBox.Show(wiadomosc);
                    break;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            Button_Click_2(sender, e);
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e) {
            Button_Click_1(sender, e);
        }
    }
}