using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MySql.Data.MySqlClient;

namespace bsk___proba_2 {
    public class RBACowyConnector {
        private static MySqlConnection polaczenie;
        private static string serwer;
        private static string login;
        private static string haslo;
        private static string port;
        private static int idRoli = -1;
        private const string NazwaBazy = "bsk";

        public enum KodyBledow {
            BlednyLoginHaslo,
            BladLaczenia,
            InnyBlad,
            NieMoznaZamknac,
            BrakInsert,
            BrakSelect,
            BrakDelete,
            BrakUpdate
        }

        public class Bledy : Exception {
            public KodyBledow Kod;
            public Bledy(KodyBledow kod) {
                Kod = kod;
            }
        }

        static RBACowyConnector() {

        }

        public static void Inicjalizuj(string serwer, string login, string haslo, string port) {
            RBACowyConnector.serwer = serwer;
            RBACowyConnector.login = login;
            RBACowyConnector.haslo = haslo;
            RBACowyConnector.port = port;
            var connectionString = "SERVER=" + RBACowyConnector.serwer + ";" + "DATABASE=" +
                                   NazwaBazy + ";" + "UID=" + RBACowyConnector.login + ";" + "PASSWORD=" +
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
                        throw new Bledy(KodyBledow.BladLaczenia);
                    case 1045:
                        throw new Bledy(KodyBledow.BlednyLoginHaslo);
                    default:
                        throw new Bledy(KodyBledow.InnyBlad);
                }
            }
        }

        private static bool ZamknijPolaczenie() {
            try {
                polaczenie.Close();
                return true;
            }
            catch (MySqlException) {
                throw new Bledy(KodyBledow.NieMoznaZamknac);
            }
        }

        public static void Insert(string tabela, List<KeyValuePair<string, string>> kolAtr) {
            string zapytanie = "INSERT INTO " + tabela + " (";
            zapytanie = kolAtr.Aggregate(zapytanie, (current, pair) => current + pair.Key + ", ");
            zapytanie = zapytanie.Remove(zapytanie.Length - 2);
            //usuwanie niepotrzebnego przecinka i spacji z poprzedniej pętli
            zapytanie += ") VALUES(";
            zapytanie = kolAtr.Aggregate(zapytanie, (current, pair) => current + ("'" + pair.Value + "', "));
            zapytanie = zapytanie.Remove(zapytanie.Length - 2);
            zapytanie += ")";

            if (OtworzPolaczenie()) {
                if (CanInsert(tabela)) {
                    MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                    cmd.ExecuteNonQuery();
                }
                else {
                    ZamknijPolaczenie();
                    throw new Bledy(KodyBledow.BrakInsert);
                }
                ZamknijPolaczenie();
            }
        }

        public static void Update(string tabela, List<KeyValuePair<string, string>> kolAtr,
            List<KeyValuePair<string, string>> kluczGlowny) {
            string zapytanie = "UPDATE " + tabela + " SET ";
            zapytanie = kolAtr.Aggregate(zapytanie, (current, pair) => current + pair.Key + "='" + pair.Value + "', ");
            zapytanie = zapytanie.Remove(zapytanie.Length - 2);
            //usuwanie niepotrzebnego przecinka i spacji z poprzedniej pętli
            zapytanie += " WHERE ";
            zapytanie = kluczGlowny.Aggregate(zapytanie,
                (current, pair) => current + "(" + pair.Key + " = " + pair.Value + ") AND ");
            zapytanie = zapytanie.Remove(zapytanie.Length - 5);

            if (OtworzPolaczenie()) {
                if (CanUpdate(tabela)) {
                    MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                    cmd.ExecuteNonQuery();
                }
                else {
                    ZamknijPolaczenie();
                    throw new Bledy(KodyBledow.BrakUpdate);
                }
                ZamknijPolaczenie();
            }
        }

        public static void Delete(string tabela, List<KeyValuePair<string, string>> kluczGlowny) {
            string zapytanie = "DELETE FROM " + tabela + " WHERE ";
            zapytanie = kluczGlowny.Aggregate(zapytanie,
                (current, pair) => current + "(" + pair.Key + " = " + pair.Value + ") AND ");
            zapytanie = zapytanie.Remove(zapytanie.Length - 5);

            if (OtworzPolaczenie()) {
                if (CanDelete(tabela)) {
                    MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                    cmd.ExecuteNonQuery();
                }
                else {
                    ZamknijPolaczenie();
                    throw new Bledy(KodyBledow.BrakDelete);
                }
                ZamknijPolaczenie();
            }
        }

        public static List<string> ListaTabel() {
            string zapytanie = "use " + NazwaBazy + ";show tables";
            List<string> list = new List<string>();

            if (OtworzPolaczenie()) {
                MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read()) {
                    string nowy = dataReader.GetString(0);
                    if (CanSelect(nowy) || CanDelete(nowy) || CanInsert(nowy) || CanUpdate(nowy))
                        list.Add(nowy);
                }

                dataReader.Close();
                ZamknijPolaczenie();
            }
            return list;
        }

        public static List<string> KluczGlowny(string tabela) {
            string zapytanie = "SELECT COLUMN_NAME FROM information_schema.columns " +
                               "WHERE (`TABLE_NAME` = \'" + tabela + "\')  AND (`COLUMN_KEY` = \'PRI\');";

            List<string> glowny = new List<string>();

            if (OtworzPolaczenie()) {
                if (CanSelect(tabela)) {
                    MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    while (dataReader.Read())
                        glowny.Add(dataReader.GetString(0));

                    dataReader.Close();
                }
                else {
                    ZamknijPolaczenie();
                    throw new Bledy(KodyBledow.BrakSelect);
                }
                ZamknijPolaczenie();
            }
            return glowny;
        }

        public static List<string> ListaKolumn(string tabela) {
            string zapytanie = "SELECT column_name " +
                               "FROM information_schema.columns " +
                               "WHERE table_name='" + tabela + "'";
            List<string> list = new List<string>();

            if (OtworzPolaczenie()) {
                if (CanSelect(tabela)) {
                    MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    while (dataReader.Read())
                        list.Add(dataReader.GetString(0));
                    //przy takim zapytaniu na pewno nie trzeba pętli, wystarczy (0)

                    dataReader.Close();
                }
                else {
                    ZamknijPolaczenie();
                    throw new Bledy(KodyBledow.BrakSelect);
                }
                ZamknijPolaczenie();
            }
            return list;
        }

        public static List<List<string>> Select(string tabela) {
            string query = "SELECT * FROM " + tabela;
            List<List<string>> list = new List<List<string>>();

            if (OtworzPolaczenie()) {
                if (CanSelect(tabela)) {
                    MySqlCommand cmd = new MySqlCommand(query, polaczenie);
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    while (dataReader.Read()) {
                        List<string> nowaLista = new List<string>();
                        for (int i = 0; i < dataReader.FieldCount; i++)
                            nowaLista.Add(dataReader.GetString(i));
                        list.Add(nowaLista);
                    }

                    dataReader.Close();
                }
                else {
                    ZamknijPolaczenie();
                    throw new Bledy(KodyBledow.BrakSelect);
                }
                ZamknijPolaczenie();
            }
            return list;
        }

        private static string SelectUprawnienia(string table) {
            if (idRoli == -1)
                return "----";
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
            return false; //tymczasowe
            return SelectUprawnienia(table)[2] != '-';
        }

        public static bool CanDelete(string table) {
            return true; //tymczasowe
            return SelectUprawnienia(table)[3] != '-';
        }
    }
}