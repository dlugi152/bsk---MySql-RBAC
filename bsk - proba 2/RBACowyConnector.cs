using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MySql.Data.MySqlClient;

namespace bsk___proba_2 {
    public class RBACowyConnector {
        private static MySqlConnection polaczenie;
        private static string serwer;
        private static string bazaDanych;
        private static string login;
        private static string haslo;
        private static string port;
        private static int idRoli = -1;

        static RBACowyConnector() {

        }

        /*static RBACowyConnector(string serwer, string bazaDanych, string login, string haslo, string port) {
            Inicjalizuj(serwer, bazaDanych, login, haslo, port);
        }*/

        public static void Inicjalizuj(string serwer, string bazaDanych, string login, string haslo, string port) {
            RBACowyConnector.serwer = serwer;
            RBACowyConnector.bazaDanych = bazaDanych;
            RBACowyConnector.login = login;
            RBACowyConnector.haslo = haslo;
            RBACowyConnector.port = port;
            var connectionString = "SERVER=" + RBACowyConnector.serwer + ";" + "DATABASE=" +
                                   RBACowyConnector.bazaDanych + ";" + "UID=" + RBACowyConnector.login + ";" + "PASSWORD=" +
                                   RBACowyConnector.haslo + ";" +
                                   "PORT=" + RBACowyConnector.port;

            polaczenie = new MySqlConnection(connectionString);
        }

        public static void UstawRole(int id) {
            idRoli = id;
        }

        public static bool TestujPolaczenie() {
            polaczenie.Open();
            polaczenie.Close();
            return true;
        }

        private static bool OtworzPolaczenie() {
            try {
                polaczenie.Open();
                return true;
            }
            catch (MySqlException ex) {
                switch (ex.Number) {
                    case 0:
                        MessageBox.Show(
                            "Nie można połączyć się z bazą. Upewnij się, że wszystkie pola w formularzu są poprawne");
                        break;

                    case 1045:
                        MessageBox.Show("Nieprawidłowy login lub hasło");
                        break;
                    default:
                        MessageBox.Show("Wystąpił nieznany błąd. Spróbuj ponownie lub uruchom aplikację jeszcze raz");
                        break;
                }
                return false;
            }
        }

        private static bool ZamknijPolaczenie() {
            try {
                polaczenie.Close();
                return true;
            }
            catch (MySqlException ex) {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        //todo możemy pomyśleć o przeciążaniu tych funkcji, jeśli to będzie potrzebne
        public static void Insert(string tabela, List<KeyValuePair<string, string>> kolAtr) {
            string zapytanie = "INSERT INTO " + tabela + " (";
            zapytanie = kolAtr.Aggregate(zapytanie, (current, pair) => current + pair.Key + ", ");
            zapytanie=zapytanie.Remove(zapytanie.Length - 2); //usuwanie niepotrzebnego przecinka i spacji z poprzedniej pętli
            zapytanie += ") VALUES(";
            zapytanie = kolAtr.Aggregate(zapytanie, (current, pair) => current + ("'" + pair.Value + "', "));
            zapytanie = zapytanie.Remove(zapytanie.Length - 2);
            zapytanie += ")";

            if (OtworzPolaczenie() && CanInsert(tabela)) {
                MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                cmd.ExecuteNonQuery();
            }
            else
                MessageBox.Show("Nie masz uprawnień do insertowania");
            ZamknijPolaczenie();
        }

        public static void Update(string tabela, List<KeyValuePair<string, string>> kolAtr, string kluczGlowny, string idUpdate) {
            string zapytanie = "UPDATE " + tabela + " SET ";
            zapytanie = kolAtr.Aggregate(zapytanie, (current, pair) => current + pair.Key + "='" + pair.Value + "', ");
            zapytanie = zapytanie.Remove(zapytanie.Length - 2); //usuwanie niepotrzebnego przecinka i spacji z poprzedniej pętli
            zapytanie += " WHERE " + kluczGlowny + "='" + idUpdate + "'";

            if (OtworzPolaczenie() && CanUpdate(tabela)) {//todo poprawić takie warunki chyba
                MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                cmd.ExecuteNonQuery();
            }
            else
                MessageBox.Show("Nie masz uprawnień do updateowania");
            ZamknijPolaczenie();
        }

        public static void Delete(string tabela,string nazwa_id, string idDelete) {
            string zapytanie = "DELETE FROM " + tabela + " WHERE " +
                               nazwa_id + "='" + idDelete + "'";

            if (OtworzPolaczenie() && CanDelete(tabela)) {
                MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                cmd.ExecuteNonQuery();
            }
            else
                MessageBox.Show("Nie masz uprawnień do deletowania");
            ZamknijPolaczenie();
        }

        public static List<string> ListaTabel() {
            string zapytanie = "use bsk;show tables";
            List<string> list = new List<string>();

            if (OtworzPolaczenie())
            {
                MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read()) {
                    string nowy = dataReader.GetString(0);
                    if (CanSelect(nowy) || CanDelete(nowy) || CanInsert(nowy) || CanUpdate(nowy))
                        list.Add(nowy);
                }

                dataReader.Close();
            }
            ZamknijPolaczenie();
            return list;
        }

        //działa tylko dla kluczy prostych
        public static string KluczGlowny(string tabela) {
            string zapytanie = "SELECT COLUMN_NAME FROM information_schema.columns " +
                "WHERE (`TABLE_NAME` = \'"+tabela+"\')  AND (`COLUMN_KEY` = \'PRI\');";

            string glowny = "";

            if (OtworzPolaczenie() && CanSelect(tabela))//chyba select?
            {
                MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                dataReader.Read();
                glowny = dataReader.GetString(0);

                dataReader.Close();
            }
            ZamknijPolaczenie();
            return glowny;
        }

        public static List<string> ListaKolumn(string tabela) {
            string zapytanie = "SELECT column_name " +
                               "FROM information_schema.columns " +
                               "WHERE table_name='" + tabela+"'";
            List<string> list = new List<string>();

            if (OtworzPolaczenie() && CanSelect(tabela))
            {
                MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                    list.Add(dataReader.GetString(0));//przy takim zapytaniu na pewno nie trzeba pętli, wystarczy (0)

                dataReader.Close();
            }
            ZamknijPolaczenie();
            return list;
        }

        public static List<List<string>> Select(string tabela) {
            string query = "SELECT * FROM " + tabela;
            List<List<string>> list = new List<List<string>>();

            if (OtworzPolaczenie() && CanSelect(tabela)) {
                MySqlCommand cmd = new MySqlCommand(query, polaczenie);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read()) {
                    List<string> nowaLista = new List<string>();
                    for (int i=0;i< dataReader.FieldCount;i++)
                        nowaLista.Add(dataReader.GetString(i));
                    list.Add(nowaLista);
                }

                dataReader.Close();
            }
            ZamknijPolaczenie();
            return list;
        }

        private static string SelectUprawnienia(string table) {
            if (idRoli == -1)
                return "----";
            //TODO try-catch, bo można złą tabelę podać
            string zapytanie = "SELECT " + table + " FROM rola WHERE ID_Roli=" + idRoli;

            MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            dataReader.Read();
            return dataReader[table] + "";
        }

        public static bool CanSelect(string table) {
            return true; //tymczasowe
            return SelectUprawnienia(table)[0] != '-';
        }

        public static bool CanInsert(string table) {
            return true; //tymczasowe
            return SelectUprawnienia(table)[1] != '-';
        }

        public static bool CanUpdate(string table) {
            return true; //tymczasowe
            return SelectUprawnienia(table)[2] != '-';
        }

        public static bool CanDelete(string table) {
            return true; //tymczasowe
            return SelectUprawnienia(table)[3] != '-';
        }
    }
}