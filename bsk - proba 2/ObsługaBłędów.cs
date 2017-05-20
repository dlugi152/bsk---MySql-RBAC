using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace bsk___proba_2
{
    static class ObsługaBłędów
    {
        public static void ObsłużBłąd(RBACowyConnector.Bledy ex)
        {
            switch (ex.Kod)
            {
                case RBACowyConnector.KodyBledow.BlednyLoginHaslo:
                    MessageBox.Show("Błędny login lub hasło");
                    break;
                case RBACowyConnector.KodyBledow.BladLaczenia:
                    MessageBox.Show("Błąd podczas łaczenia z bazą");
                    break;
                case RBACowyConnector.KodyBledow.NieMoznaZamknac:
                    MessageBox.Show("Nie można zamknąć połączenia");
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
                case RBACowyConnector.KodyBledow.NieprawidłoweDane:
                    MessageBox.Show("Wpisałeś wartość " + ex.Wartosc + " dla kolumny " + ex.Kolumna + ". Ta kolumna wymaga " + ex.Typ);
                    break;
                case RBACowyConnector.KodyBledow.BrakDomyślnej:
                    MessageBox.Show("Pole " + ex.Kolumna + " nie ma wartości domyślnej");
                    break;
                case RBACowyConnector.KodyBledow.BrakDanych:
                    MessageBox.Show(
                        "Zapytanie miewystarczającą ilość danych. Nie możesz używać samych wartości domyślnych");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
