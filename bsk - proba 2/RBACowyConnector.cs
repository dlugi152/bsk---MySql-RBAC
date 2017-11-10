using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;

namespace bsk___proba_2
{
    public static class RBACowyConnector
    {
        private static MySqlConnection polaczenie;
        private static string serwer;
        private static string port;
        private const string NazwaBazy = "abd";

        public enum KodyBledow
        {
            BlednyLoginHaslo,
            BladLaczenia,
            NieMoznaZamknac,
            NieprawidłoweDane,
            BrakDomyślnej,
            BrakDanych,
            TriggerZablokowal,
            MozeDaty,
            NieprawidłowyFormat
        }

        public class Bledy : Exception
        {
            public KodyBledow Kod;
            public string Kolumna;
            public string Typ;
            public string Wartosc;

            public Bledy(KodyBledow kod) {
                Kod = kod;
            }

            public Bledy() { }
        }

        public static void Inicjalizuj(string serwer, string port) {
            RBACowyConnector.serwer = serwer;
            RBACowyConnector.port = port;
            var connectionString = "SERVER=" + RBACowyConnector.serwer + ";DATABASE=" +
                                   NazwaBazy + ";UID=" + "tomasz152" + ";PASSWORD=" +
                                   "bombelek" + ";PORT=" + RBACowyConnector.port +
                                   ";Pooling=false";
            try {
                polaczenie = new MySqlConnection(connectionString);
                TestujPolaczenie();
                OtworzPolaczenie();

            }
            catch (MySqlException ex) {
                ObsłużPrzewidzianeBłędyMySqLa(ex);
            }
        }

        private static bool TestujPolaczenie() {
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
                ObsłużPrzewidzianeBłędyMySqLa(ex);
                return false;
            }
        }

        public static bool ZamknijPolaczenie() {
            try {
                if (polaczenie == null)
                    return true;
                polaczenie.Close();
                polaczenie.Dispose();
                return true;
            }
            catch (MySqlException ex) {
                ObsłużPrzewidzianeBłędyMySqLa(ex);
                return false;
            }
        }

        public static void Insert(string tabela, List<KeyValuePair<string, string>> kolAtr, bool nieIstnieją = false) {
            //jeśli w kolAtr nic nie ma to ktoś chce dodać coś używając samych domyślnych wartości
            if (kolAtr.FindAll(pair => pair.Value != "").Count == 0)
                throw new Bledy {
                    Kod = KodyBledow.BrakDanych
                };

            string zapytanie = "INSERT INTO " + tabela + " (";
            //gdy ktoś poda pustego stringa to albo będzie wartość domyślna, albo błędne zapytanie (message box)
            zapytanie = kolAtr.Where(pair => pair.Value != "")
                              .Aggregate(zapytanie, (current, pair) => current + pair.Key + ", ");
            zapytanie = zapytanie.Remove(zapytanie.Length - 2);
            //usuwanie niepotrzebnego przecinka i spacji z poprzedniej pętli
            zapytanie += ") ";
            if (nieIstnieją == false) {
                zapytanie += "VALUES (";
                zapytanie = kolAtr.Where(t => t.Value != "")
                                  .Aggregate(zapytanie, (current, t) => current + "@" + t.Key + ", ");
                zapytanie = zapytanie.Remove(zapytanie.Length - 2);
                zapytanie += ")";
            }
            else {
                zapytanie += "SELECT ";
                zapytanie = kolAtr.Where(pair => pair.Value != "")
                                  .Aggregate(zapytanie, (current, pair) => current + "@" + pair.Key + ", ");
                zapytanie = zapytanie.Remove(zapytanie.Length - 2);
                zapytanie += " WHERE NOT EXISTS (SELECT * FROM " + tabela + " WHERE ";
                foreach (KeyValuePair<string, string> pair in kolAtr)
                    if (pair.Value != "")
                        zapytanie += pair.Key + " = @" + pair.Key + " and ";
                    else
                        zapytanie += pair.Key + " = '" + DomyślnaWartość(tabela, pair.Key) + "' and ";
                zapytanie = zapytanie.Remove(zapytanie.Length - 5);
                zapytanie += ") LIMIT 1";
            }
            try {
                MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                foreach (var t in kolAtr)
                    if (t.Value != "")
                        cmd.Parameters.Add(new MySqlParameter("@" + t.Key, t.Value));
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex) {
                ObsłużPrzewidzianeBłędyMySqLa(ex);
            }
        }

        private static void ObsłużPrzewidzianeBłędyMySqLa(MySqlException ex) {
            Scanner scanner = new Scanner();
            //brane stąd:
            //https://dev.mysql.com/doc/refman/5.5/en/error-messages-server.html

            const string parser1366 = "Incorrect %s value: '%s' for column '%s' at row %ld";
            const string parser1364 = "Field '%s' doesn't have a default value";
            const string parser1265 = "Data truncated for column '%s' at row %ld";
            object[] targets;
            switch (ex.Number) {
                case 0:
                    throw new Bledy(KodyBledow.BladLaczenia);
                //niegdy nie powinno się zdarzyć - podajemy login i hasło de facto na sztywno
                //case 1045:
                //    throw new Bledy(KodyBledow.BlednyLoginHaslo);
                case 1366:
                    targets = scanner.Scan(ex.Message, parser1366);
                    throw new Bledy {
                        Kod = KodyBledow.NieprawidłoweDane,
                        Kolumna = targets[2].ToString(),
                        Typ = targets[0].ToString(),
                        Wartosc = targets[1].ToString()
                    };
                case 1364:
                    targets = scanner.Scan(ex.Message, parser1364);
                    throw new Bledy {
                        Kod = KodyBledow.BrakDomyślnej,
                        Kolumna = targets[0].ToString()
                    };
                case 1042:
                    throw new Bledy {
                        Kod = KodyBledow.BladLaczenia
                    };
                case 1136:
                    throw new Bledy {
                        Kod = KodyBledow.TriggerZablokowal
                    };
                case 1265:
                    targets = scanner.Scan(ex.Message, parser1265);
                    throw new Bledy {
                        Kod = KodyBledow.NieprawidłoweDane,
                        Kolumna = targets[0].ToString()
                    };
                case 1054:
                    throw new Bledy {
                        Kod = KodyBledow.MozeDaty
                    };
                case 1292:
                    throw new Bledy {
                        Kod = KodyBledow.NieprawidłowyFormat
                    };
                default:
                    throw new Exception("Nieoczekiwany błąd o kodzie " + ex.Number);
            }
        }

        public static void Update(string tabela, List<KeyValuePair<string, string>> kolAtr,
                                  List<KeyValuePair<string, string>> kluczGlowny) {
            string zapytanie = "UPDATE " + tabela + " SET ";
            foreach (var pair in kolAtr)
                if (pair.Value != "")
                    zapytanie += pair.Key + "=@" + pair.Key + ", ";
                else
                    zapytanie += pair.Key + "=default, ";
            zapytanie = zapytanie.Remove(zapytanie.Length - 2);
            //usuwanie niepotrzebnego przecinka i spacji z poprzedniej pętli
            zapytanie += " WHERE ";
            zapytanie = kluczGlowny.Aggregate(zapytanie,
                                              (current, pair) =>
                                                  current + "(" + pair.Key + " = @" + pair.Key + ") AND ");
            zapytanie = zapytanie.Remove(zapytanie.Length - 5);

            try {
                MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                foreach (var t in kolAtr)
                    if (t.Value != "")
                        cmd.Parameters.Add(new MySqlParameter("@" + t.Key, t.Value));
                foreach (var t in kluczGlowny)
                    if (t.Value != "")
                        cmd.Parameters.Add(new MySqlParameter("@" + t.Key, t.Value));
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex) {
                ObsłużPrzewidzianeBłędyMySqLa(ex);
            }
        }

        public static void Delete(string tabela, List<KeyValuePair<string, string>> kluczGlowny) {
            string zapytanie = "DELETE FROM " + tabela + " WHERE ";
            zapytanie = kluczGlowny.Aggregate(zapytanie,
                                              (current, pair) =>
                                                  current + "(" + pair.Key + " = @" + pair.Key + ") AND ");
            zapytanie = zapytanie.Remove(zapytanie.Length - 5);

            try {
                MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                foreach (var t in kluczGlowny)
                    if (t.Value != "")
                        cmd.Parameters.Add(new MySqlParameter("@" + t.Key, t.Value));
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex) {
                ObsłużPrzewidzianeBłędyMySqLa(ex);
            }
        }

        public static List<string> ListaTabel() {
            string zapytanie = "show tables";
            MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            List<string> listaWszystkich = new List<string>();
            while (dataReader.Read())
                listaWszystkich.Add(dataReader.GetString(0));
            dataReader.Close();
            return listaWszystkich;
        }

        //nieprzetestowane!!!!!!!
        private static string DomyślnaWartość(string tabela, string kolumna) {
            string domyślna = "";
            try {
                domyślna = Select(tabela: "information_schema.columns",
                                  where: new List<KeyValuePair<string, string>> {
                                      new KeyValuePair<string, string>("TABLE_NAME", tabela),
                                      new KeyValuePair<string, string>("COLUMN_NAME", kolumna)
                                  }, kolumny: new List<string> {"Column_Default"})[0][0];
            }
            catch (MySqlException ex) {
                ObsłużPrzewidzianeBłędyMySqLa(ex);
            }
            return domyślna;
        }

        public static List<string> KluczGlowny(string tabela) {
            var list = Select("information_schema.columns",
                              new List<KeyValuePair<string, string>> {
                                  new KeyValuePair<string, string>("TABLE_NAME", tabela),
                                  new KeyValuePair<string, string>("COLUMN_KEY", "PRI")
                              }, new List<string> {"COLUMN_NAME"});
            return list.Select(list1 => list1[0]).ToList();
        }

        public static List<string> ListaKolumn(string tabela) {
            List<List<string>> list1 = Select("information_schema.columns",
                                              new List<KeyValuePair<string, string>> {
                                                  new KeyValuePair<string, string>("TABLE_NAME", tabela)
                                              },
                                              new List<string> {"column_name"});
            return list1.Select(list2 => list2[0]).ToList();
        }

        private static List<List<string>> Select(string tabela, List<KeyValuePair<string, string>> where,
                                                 List<string> kolumny = null) {
            if (kolumny == null)
                kolumny = new List<string> {"*"};
            string query = kolumny.Aggregate("SELECT ", (current, s) => current + s + ",");
            query = query.Remove(query.Length - 1);
            query += " FROM " + tabela;
            if (where != null && where.Count > 0) {
                query += " WHERE ";
                query = where.Aggregate(query, (current, pair) => current + pair.Key + " = @" + pair.Key + " and ");
                query = query.Remove(query.Length - 5);
            }
            List<List<string>> list = new List<List<string>>();
            MySqlCommand cmd = new MySqlCommand(query, polaczenie);
            if (where != null && where.Count > 0)
                foreach (KeyValuePair<string, string> t in where)
                    cmd.Parameters.AddWithValue("@" + t.Key, t.Value);

            MySqlDataReader dataReader = cmd.ExecuteReader();

            while (dataReader.Read()) {
                List<string> nowaLista = new List<string>();
                for (int i = 0; i < dataReader.FieldCount; i++)
                    try {
                        string typ = dataReader.GetDataTypeName(i);
                        switch (typ) {
                            case "DATE":
                                nowaLista.Add(dataReader.GetDateTime(i).Date.ToString("yyyy-MM-dd"));
                                break;
                            case "TIME":
                                nowaLista.Add(dataReader.GetTimeSpan(i).Hours + ":" +
                                              dataReader.GetTimeSpan(i).Minutes);
                                break;
                            default:
                                nowaLista.Add(dataReader.GetString(i));
                                break;
                        }
                    }
                    catch {
                        nowaLista.Add("");
                    }
                list.Add(nowaLista);
            }
            dataReader.Close();
            return list;
        }

        public static List<List<string>> Select(string tabela) {
            return Select(tabela, null);
        }
    }
}