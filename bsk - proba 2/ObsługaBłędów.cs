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
        public static void ObsłużBłąd(RBACowyConnector.KodyBledow kod, string wiadomosc)
        {
            switch (kod)
            {
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
    }
}
