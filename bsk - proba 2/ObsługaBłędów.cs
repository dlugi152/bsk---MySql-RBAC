using System;
using System.Windows;

namespace bsk___proba_2
{
    internal static class ObsługaBłędów
    {
        public static void ObsłużBłąd(RBACowyConnector.Bledy ex) {
            switch (ex.Kod) {
                case RBACowyConnector.KodyBledow.BlednyLoginHaslo:
                    MessageBox.Show("Błędny login lub hasła");
                    break;
                case RBACowyConnector.KodyBledow.BladLaczenia:
                    MessageBox.Show("Błąd podczas łaczenia z bazą\nJesteś pewny, że adres serwera i port są poprawne?");
                    break;
                case RBACowyConnector.KodyBledow.NieMoznaZamknac:
                    MessageBox.Show("Nie można zamknąć połączenia");
                    break;
                case RBACowyConnector.KodyBledow.NieprawidłoweDane:
                    if (ex.Typ != null && ex.Wartosc != null) //jest pełne info nt. błędu
                        MessageBox.Show("Wpisałeś wartość " + ex.Wartosc + " dla kolumny " + ex.Kolumna +
                                        ". Ta kolumna wymaga " + ex.Typ);
                    else
                        MessageBox.Show("Nieprawidłowa wartość dla kolumny " + ex.Kolumna);
                    break;
                case RBACowyConnector.KodyBledow.BrakDomyślnej:
                    MessageBox.Show("Pole " + ex.Kolumna + " nie ma wartości domyślnej");
                    break;
                case RBACowyConnector.KodyBledow.BrakDanych:
                    MessageBox.Show(
                                    "Zapytanie miewystarczającą ilość danych. Nie możesz używać samych wartości domyślnych");
                    break;
                case RBACowyConnector.KodyBledow.TriggerZablokowal:
                    MessageBox.Show("Co najmniej jedna wartość była nieprawidłowa\n" +
                                    "Dane były niespójne\n" +
                                    "Upewnij się, że np Punkty są dodatnie, data po>data przed i inne takie");
                    break;
                case RBACowyConnector.KodyBledow.MozeDaty:
                    MessageBox.Show("Twoje dane nie były spójne lub były w nieprawidłowym formacie.\n" +
                                    "Może daty odwrotnie niż trzeba?");
                    break;
                case RBACowyConnector.KodyBledow.NieprawidłowyFormat:
                    MessageBox.Show("Podałeś nieprawidłowy format danych");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}