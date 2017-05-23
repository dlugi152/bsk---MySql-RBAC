using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace bsk___proba_2
{
    public static class RBACowyConnector
    {
        private static MySqlConnection polaczenie;
        private static string serwer;
        private static string login;
        private static string haslo;
        private static string port;
        private static List<string> idZalogowanegoPracownika;
        private static List<string> idUzytejRoli;
        private static List<string> KluczGłównyRól;
        private static List<string> KluczGłównyPracowników;
        private static Dictionary<string, string> UprawnieniaZalogowanego = new Dictionary<string, string>();
        private static bool czyAdmin;
        private static List<string> tabele = new List<string>();
        private const string NazwaBazy = "bsk";
        private const string TabelaZPracownikami = "pracownik";
        private const string TabelaZRolami = "rola";
        private const string NazwaKolumnyLoginow = "login_uzytkownika";
        private const string NazwaKolumnyCzyAdmin = "adminska";
        private const string NazwaKolumnyRoli = "nazwa";
        private const string NazwaLicznikaPolaczen = "licznik_polaczen";
        private const string NazwaKolumnyWymuszaniaHasla = "Zmiana_Hasla";
        private const string NazwaKolumnyHasel = "hash_hasla";
        private const string TabelaZPrzypisaniemRól = "przypisanie_roli";
        private static readonly List<string> ListaObowiązkowychPólUsera = new List<string>
        {
            NazwaKolumnyLoginow,NazwaKolumnyHasel
        };

        private static List<string> ListaPólUseraNiePytać;

        private static readonly List<string> TabeleAdmińskie =
            new List<string> {TabelaZRolami, TabelaZPrzypisaniemRól, TabelaZPracownikami};

        private static readonly List<string> TabeleSpecjalne =
            new List<string> {"information_schema.columns", "information_schema"};

        public enum KodyBledow
        {
            BlednyLoginHaslo,
            BladLaczenia,
            NieMoznaZamknac,
            BrakInsert,
            BrakSelect,
            BrakDelete,
            BrakUpdate,
            NieprawidłoweDane,
            BrakDomyślnej,
            BrakDanych,
            TriggerZablokowal,
            InnaRolaPelniona,
            MozeDaty
        }

        public class Bledy : Exception
        {
            public KodyBledow Kod;
            public string Wiadomosc;
            public string Kolumna;
            public string Typ;
            public string Wartosc;
            public string Tabela;

            public Bledy(KodyBledow kod)
            {
                Kod = kod;
                Wiadomosc = "";
            }

            public Bledy()
            {
            }
        }

        static RBACowyConnector()
        {
        }

        public static void Inicjalizuj(string serwer, string login, string haslo, string port)
        {
            RBACowyConnector.serwer = serwer;
            RBACowyConnector.login = login;
            RBACowyConnector.haslo = haslo;
            RBACowyConnector.port = port;
            var connectionString = "SERVER=" + RBACowyConnector.serwer + ";DATABASE=" +
                                   NazwaBazy + ";UID=" + "tomasz152" + ";PASSWORD=" +
                                   "bombelek" + ";PORT=" + RBACowyConnector.port + ";Pooling=false";
            try
            {
                polaczenie = new MySqlConnection(connectionString);
                TestujPolaczenie();
                OtworzPolaczenie();
                SprawdźUżytkownika(login, haslo);
                PierwszePołączenie(login);

            }
            catch (MySqlException ex)
            {
                ObsłużPrzewidzianeBłędyMySQLa(ex);
            }
        }

        private static String sha256_hash(String value)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));
                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }

        private static void SprawdźUżytkownika(string login, string haslo)
        {
            string hashHasla = sha256_hash(haslo);
            List<List<string>> list = Select(TabelaZPracownikami, new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(NazwaKolumnyLoginow, login),
                new KeyValuePair<string, string>(NazwaKolumnyHasel, hashHasla)
            });
            if (list.Count!=1)
                throw new Bledy
                {
                    Kod = KodyBledow.BlednyLoginHaslo
                };
        }

        private static void PierwszePołączenie(string login)
        {
            KluczGłównyPracowników = KluczGlowny(TabelaZPracownikami);
            KluczGłównyRól = KluczGlowny(TabelaZRolami);
            UstawKluczGłównyRól();

            ListaPólUseraNiePytać = new List<string>().Concat(KluczGłównyRól)
                .Concat(new List<string> { NazwaLicznikaPolaczen, NazwaKolumnyWymuszaniaHasla }).ToList();
        }

        private static void UstawKluczGłównyRól()
        {
            KluczGłównyRól = KluczGlowny(TabelaZRolami);
        }

        private static List<string> IdPracownika(string login)
        {
            if (KluczGłównyPracowników == null)
                KluczGłównyPracowników = KluczGlowny(TabelaZPracownikami);
            List<string> list = Select(TabelaZPracownikami,
                new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>(NazwaKolumnyLoginow, login)},
                KluczGłównyPracowników)[0];
            return list;
        }

        private static List<string> IdRoli(string nazwa)
        {
            List<string> selectowane = KluczGlowny(TabelaZRolami);
            selectowane.Add(NazwaKolumnyCzyAdmin);
            List<string> list = Select(TabelaZRolami,
                new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>(NazwaKolumnyRoli, nazwa)},
                selectowane)[0];
            czyAdmin = Boolean.Parse(list[list.Count - 1]);
            list.RemoveAt(list.Count - 1);
            return list;
        }

        public static void UstawAktualnąRolę(string login, string nazwa)
        {
            List<string> tmpIdPracownika = IdPracownika(login);
            List<string> tmpId = IdRoli(nazwa);
            var paryUzytejRoli = KluczGłównyRól.Select((t, i) =>
                    new KeyValuePair<string, string>(t, tmpId[i]))
                .ToList();
            var paryKluczaPracownika = tmpIdPracownika.Select((t, i) =>
                    new KeyValuePair<string, string>(KluczGłównyPracowników[i], t))
                .ToList();

            List<string> list = Select(TabelaZPracownikami, paryKluczaPracownika,
                KluczGłównyRól)[0];

            if (list.Any(s => s != "" && !paryUzytejRoli.Exists(pair => pair.Value == s)))
                throw new Bledy
                {
                    Kod = KodyBledow.InnaRolaPelniona,
                };
            string iloscAktPolaczen = Select(TabelaZPracownikami, paryKluczaPracownika, new List<string> {NazwaLicznikaPolaczen})[0][0];
            paryUzytejRoli.Add(new KeyValuePair<string, string>(NazwaLicznikaPolaczen,(int.Parse(iloscAktPolaczen)+1).ToString()));
            Update(TabelaZPracownikami, paryUzytejRoli, paryKluczaPracownika);
            //dopiero w tym momencie mogę ustawić te dwie zmienne, bo dopiero od teraz zacznę ich używać
            //tu się kończy rola admina i po ustawieniu tych zmiennych wszystkie działania będą
            //kontrolowane, będą sprawdzane uprawnienia roli
            idUzytejRoli = tmpId;
            idZalogowanegoPracownika = tmpIdPracownika;
        }

        public static bool TestujPolaczenie()
        {
            polaczenie.Open();
            polaczenie.Close();
            return true;
        }

        public static bool OtworzPolaczenie()
        {
            try
            {
                polaczenie.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                ObsłużPrzewidzianeBłędyMySQLa(ex);
                return false;
            }
        }

        public static bool ZamknijPolaczenie()
        {
            try
            {
                if (idZalogowanegoPracownika != null)
                {
                    var paryKluczaPracownika = idZalogowanegoPracownika.Select((t, i) =>
                            new KeyValuePair<string, string>(KluczGłównyPracowników[i], t))
                        .ToList();
                    idZalogowanegoPracownika = null;
                    idUzytejRoli = null;

                    List<List<string>> listtmp = Select(TabelaZPracownikami, paryKluczaPracownika,
                        KluczGłównyRól);

                    if (listtmp.Capacity==1 && listtmp[0].Any(s => s != "")) //ktoś zdążył wybrać rolę
                    {
                        string iloscAktPolaczen = Select(TabelaZPracownikami, paryKluczaPracownika,
                            new List<string> {NazwaLicznikaPolaczen})[0][0];
                        List<KeyValuePair<string, string>> paryUzytejRoli = new List<KeyValuePair<string, string>>();
                        if (iloscAktPolaczen == "1")
                            paryUzytejRoli.AddRange(KluczGłównyRól.Select(s => new KeyValuePair<string, string>(s, "")));
                        paryUzytejRoli.Add(new KeyValuePair<string, string>(NazwaLicznikaPolaczen,
                            (int.Parse(iloscAktPolaczen) - 1).ToString()));
                        Update(TabelaZPracownikami, paryUzytejRoli, paryKluczaPracownika);
                    }
                }
                polaczenie.Close();
                polaczenie.Dispose();
                return true;
            }
            catch (MySqlException ex)
            {
                ObsłużPrzewidzianeBłędyMySQLa(ex);
                return false;
            }
        }

        public static void Insert(string tabela, List<KeyValuePair<string, string>> kolAtr, bool nieIstnieją = false)
        {
            //jeśli w kolAtr nic nie ma to ktoś chce dodać coś używając samych domyślnych wartości
            if (kolAtr.FindAll(pair => pair.Value != "").Count == 0)
                throw new Bledy
                {
                    Kod = KodyBledow.BrakDanych
                };

            string zapytanie = "INSERT INTO " + tabela + " (";
            //gdy ktoś poda pustego stringa to albo będzie wartość domyślna, albo błędne zapytanie (message box)
            zapytanie = kolAtr.Where(pair => pair.Value != "")
                .Aggregate(zapytanie, (current, pair) => current + pair.Key + ", ");
            zapytanie = zapytanie.Remove(zapytanie.Length - 2);
            //usuwanie niepotrzebnego przecinka i spacji z poprzedniej pętli
            zapytanie += ") ";
            if (nieIstnieją == false)
            {
                zapytanie += "VALUES (";
                zapytanie = kolAtr.Where(t => t.Value != "")
                    .Aggregate(zapytanie, (current, t) => current + "@" + t.Key + ", ");
                zapytanie = zapytanie.Remove(zapytanie.Length - 2);
                zapytanie += ")";
            }
            else
            {
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


            if (CanInsert(tabela))
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                    foreach (var t in kolAtr)
                        if (t.Value != "")
                            cmd.Parameters.Add(new MySqlParameter("@" + t.Key, t.Value));
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    ObsłużPrzewidzianeBłędyMySQLa(ex);
                }
            }
            else
                throw new Bledy
                {
                    Kod = KodyBledow.BrakInsert,
                    Tabela = tabela
                };
        }

        private static void ObsłużPrzewidzianeBłędyMySQLa(MySqlException ex)
        {
            Scanner scanner = new Scanner();
            //brane stąd:
            //https://dev.mysql.com/doc/refman/5.5/en/error-messages-server.html

            string parser1366 = "Incorrect %s value: '%s' for column '%s' at row %ld";
            string parser1364 = "Field '%s' doesn't have a default value";
            string parser1265 = "Data truncated for column '%s' at row %ld";
            string parser1054 = "Unknown column '%s' in '%s'";
            object[] targets;
            switch (ex.Number)
            {
                case 0:
                    throw new Bledy(KodyBledow.BladLaczenia);
                //niegdy nie powinno się zdarzyć - podajemy login i hasło de facto na sztywno
                //case 1045:
                //    throw new Bledy(KodyBledow.BlednyLoginHaslo);
                case 1366:
                    targets = scanner.Scan(ex.Message, parser1366);
                    throw new Bledy
                    {
                        Kod = KodyBledow.NieprawidłoweDane,
                        Kolumna = targets[2].ToString(),
                        Typ = targets[0].ToString(),
                        Wartosc = targets[1].ToString()
                    };
                case 1364:
                    targets = scanner.Scan(ex.Message, parser1364);
                    throw new Bledy
                    {
                        Kod = KodyBledow.BrakDomyślnej,
                        Kolumna = targets[0].ToString()
                    };
                case 1042:
                    throw new Bledy
                    {
                        Kod = KodyBledow.BladLaczenia
                    };
                case 1136:
                    throw new Bledy
                    {
                        Kod = KodyBledow.TriggerZablokowal
                    };
                case 1265:
                    targets = scanner.Scan(ex.Message, parser1265);
                    throw new Bledy
                    {
                        Kod = KodyBledow.NieprawidłoweDane,
                        Kolumna = targets[0].ToString()
                    };
                case 1054:
                    targets = scanner.Scan(ex.Message, parser1054);
                    throw new Bledy
                    {
                        Kod = KodyBledow.MozeDaty
                    };
                default:
                    throw new Exception("Nieoczekiwany błąd o kodzie " + ex.Number);
            }
        }

        public static void Update(string tabela, List<KeyValuePair<string, string>> kolAtr,
            List<KeyValuePair<string, string>> kluczGlowny)
        {
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
                (current, pair) => current + "(" + pair.Key + " = @" + pair.Key + ") AND ");
            zapytanie = zapytanie.Remove(zapytanie.Length - 5);


            if (CanUpdate(tabela))
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                    foreach (var t in kolAtr)
                        if (t.Value != "")
                            cmd.Parameters.Add(new MySqlParameter("@" + t.Key, t.Value));
                    foreach (var t in kluczGlowny)
                        if (t.Value != "")
                            cmd.Parameters.Add(new MySqlParameter("@" + t.Key, t.Value));
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    ObsłużPrzewidzianeBłędyMySQLa(ex);
                }
            }
            else
                throw new Bledy
                {
                    Kod = KodyBledow.BrakUpdate,
                    Tabela = tabela
                };
        }

        public static void Delete(string tabela, List<KeyValuePair<string, string>> kluczGlowny)
        {
            string zapytanie = "DELETE FROM " + tabela + " WHERE ";
            zapytanie = kluczGlowny.Aggregate(zapytanie,
                (current, pair) => current + "(" + pair.Key + " = @" + pair.Key + ") AND ");
            zapytanie = zapytanie.Remove(zapytanie.Length - 5);

            if (CanDelete(tabela))
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                    foreach (var t in kluczGlowny)
                        if (t.Value != "")
                            cmd.Parameters.Add(new MySqlParameter("@" + t.Key, t.Value));
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    ObsłużPrzewidzianeBłędyMySQLa(ex);
                }
            }
            else
                throw new Bledy
                {
                    Kod = KodyBledow.BrakDelete,
                    Tabela = tabela
                };
        }

        public static List<string> ListaTabel(bool? admińskie)
        {
            string zapytanie = "show tables";
            List<string> list = new List<string>();

            MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            List<string> listaWszystkich = new List<string>();
            while (dataReader.Read())
                listaWszystkich.Add(dataReader.GetString(0));
            dataReader.Close();
            if (admińskie == null) //jeśli to nie admin to daj mu wszystko do czego ma dostęp
                return listaWszystkich.FindAll(s => CanSelect(s) || CanInsert(s));
            //jeśli to admin to może chcieć zwykłych tabel, albo admińskich
            list.AddRange(listaWszystkich.Where
            (s => admińskie.Value &&
                  TabeleAdmińskie.FindIndex(s2 => s2.Equals(s, StringComparison.InvariantCultureIgnoreCase)) > -1
                  ||
                  !admińskie.Value &&
                  TabeleAdmińskie.FindIndex(s2 => s2.Equals(s, StringComparison.InvariantCultureIgnoreCase)) == -1));
            return list;
        }

        //nieprzetestowane!!!!!!!
        private static string DomyślnaWartość(string tabela, string kolumna)
        {
            string domyślna = "";
            try
            {
                domyślna = Select(tabela: "information_schema.columns",
                    @where: new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("TABLE_NAME", tabela),
                        new KeyValuePair<string, string>("COLUMN_NAME", kolumna)
                    }, kolumny: new List<string> {"Column_Default"})[0][0];
            }
            catch (MySqlException ex)
            {
                ObsłużPrzewidzianeBłędyMySQLa(ex);
            }
            return domyślna;
        }

        public static List<string> KluczGlowny(string tabela)
        {
            var list = Select("information_schema.columns",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("TABLE_NAME", tabela),
                    new KeyValuePair<string, string>("COLUMN_KEY", "PRI")
                }, new List<string> {"COLUMN_NAME"});
            return list.Select(list1 => list1[0]).ToList();
        }

        public static List<string> ListaKolumn(string tabela)
        {
            List<List<string>> list1 = Select("information_schema.columns",
                new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("TABLE_NAME", tabela)},
                new List<string> {"column_name"});
            return list1.Select(list2 => list2[0]).ToList();
        }

        private static List<List<string>> Select(string tabela, List<KeyValuePair<string, string>> where,
            List<string> kolumny = null)
        {
            if (kolumny == null)
                kolumny = new List<string> {"*"};
            string query = kolumny.Aggregate("SELECT ", (current, s) => current + s + ",");
            query = query.Remove(query.Length - 1);
            query += " FROM " + tabela;
            if (where != null && where.Count > 0)
            {
                query += " WHERE ";
                query = where.Aggregate(query, (current, pair) => current + pair.Key + " = @" + pair.Key + " and ");
                query = query.Remove(query.Length - 5);
            }
            List<List<string>> list = new List<List<string>>();

            if (CanSelect(tabela))
            {
                MySqlCommand cmd = new MySqlCommand(query, polaczenie);
                if (@where != null && @where.Count > 0)
                    foreach (KeyValuePair<string, string> t in @where)
                        cmd.Parameters.AddWithValue("@" + t.Key, t.Value);

                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    List<string> nowaLista = new List<string>();
                    for (int i = 0; i < dataReader.FieldCount; i++)
                        try
                        {
                            nowaLista.Add(dataReader.GetString(i));
                        }
                        catch
                        {
                            nowaLista.Add("");
                        }
                    list.Add(nowaLista);
                }
                dataReader.Close();
            }
            else
                throw new Bledy
                {
                    Kod = KodyBledow.BrakSelect,
                    Tabela = tabela
                };
            return list;
        }

        public static List<List<string>> Select(string tabela)
        {
            return Select(tabela, null);
        }

        private static string SelectUprawnienia(string table)
        {
            //selecta w tej funkcji nie wrzucać do selecta ogólnego!!
            //jeśli idUżytejRoli==null to znaczy, że to admin na początku zagląda do tabel itp
            if (TabeleSpecjalne.FindIndex(s => s.Equals(table, StringComparison.InvariantCultureIgnoreCase)) > -1 ||
                idUzytejRoli == null &&
                TabeleAdmińskie.FindIndex(s => s.Equals(table, StringComparison.InvariantCultureIgnoreCase)) > -1)
                return "s-u-";
            //select potrzebny do wyświetlenia ról użytkownika
            //update do ustawienia aktualnej pełnionej roli (jedna rola na raz)
            //czy na pewno na to zezwalać?
            string zapytanie = "SELECT " + table + " FROM " + TabelaZRolami + " WHERE (";
            zapytanie = KluczGłównyRól.Aggregate(zapytanie, (current, t) => current + t + " = @" + t + " and ");
            zapytanie = zapytanie.Remove(zapytanie.Length - 5);
            zapytanie += ")";
            string uprawnienia = "";
            MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
            for (var i = 0; i < idUzytejRoli.Count; i++)
                cmd.Parameters.Add(new MySqlParameter("@" + KluczGłównyRól[i], idUzytejRoli[i]));
            MySqlDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
                uprawnienia = dataReader.GetString(0);
            dataReader.Close();
            return uprawnienia;
        }

        public static bool CanSelect(string table)
        {
            return SelectUprawnienia(table)[0] != '-';
        }

        public static bool CanInsert(string table)
        {
            return SelectUprawnienia(table)[1] != '-';
        }

        public static bool CanUpdate(string table)
        {
            return SelectUprawnienia(table)[2] != '-';
        }

        public static bool CanDelete(string table)
        {
            return SelectUprawnienia(table)[3] != '-';
        }

        public static List<string> MojeRoleNazwy(string login)
        {
            List<string> list = RoleUżytkownika(login);
            return list;
        }

        private static List<string> SelectJednąKolumnę(string kolumna, string tabela)
        {
            List<List<string>> list = Select(tabela, null, new List<string> {kolumna});
            return list.Select(list1 => list1[0]).ToList();
        }

        public static List<string> ListaPracowników()
        {
            return SelectJednąKolumnę(NazwaKolumnyLoginow, TabelaZPracownikami);
        }

        public static List<string> ListaWszystkichRól()
        {
            return SelectJednąKolumnę(NazwaKolumnyRoli, TabelaZRolami);
        }

        public static List<string> RoleUżytkownika(string użytkownik)
        {
            //zajebiste zapytanie
            string query =
                KluczGłównyRól.Aggregate(
                    "SELECT " + NazwaKolumnyRoli + " FROM " + TabelaZRolami + " INNER JOIN (SELECT " +
                    TabelaZPracownikami + "." + NazwaKolumnyLoginow + ",",
                    (current, s) => current + TabelaZPrzypisaniemRól + "." + s + ",");
            query = query.Remove(query.Length - 1);
            query += " FROM " + TabelaZPrzypisaniemRól + " INNER JOIN " + TabelaZPracownikami + " ON ";
            query = KluczGłównyPracowników.Aggregate(query,
                (current, s) => current + TabelaZPracownikami + "." + s + " = " + TabelaZPrzypisaniemRól + "." + s +
                                " and ");
            query = query.Remove(query.Length - 5);
            query += ") as tmp ON ";
            query = KluczGłównyRól.Aggregate(query,
                (current, s) => current + TabelaZRolami + "." + s + " = tmp." + s + " and ");
            query = query.Remove(query.Length - 5);
            query += " WHERE tmp." + NazwaKolumnyLoginow + " = @login";

            List<string> listaRól = new List<string>();
            //if (CanSelect(TabelaZRolami) && CanSelect(TabelaZPrzypisaniemRól) && CanSelect(TabelaZPracownikami)) {
            //brak sprawdzania czy można selectować tabelę z rolami, każdy powinien mieć prawo do
            //wglądu jakie role posiada i na co one pozwalają - bez tego nie da się pracować
            MySqlCommand cmd = new MySqlCommand(query, polaczenie);
            cmd.Parameters.Add(new MySqlParameter("@login", użytkownik));
            MySqlDataReader dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
                listaRól.Add(dataReader.GetString(0)); //0 bo zapytanie czyta tylko 1 kolumnę - nazwę

            dataReader.Close();
            //}
            //else {
            //    throw new Bledy(KodyBledow.BrakSelect);
            //}
            return listaRól;
        }

        public static void DodajPrzypisanieRoli(string rola, string użytkownik)
        {
            List<string> idPracownika = IdPracownika(użytkownik);
            List<string> idRoli = IdRoli(rola);
            List<string> kluczGłównyPracowników = KluczGlowny(TabelaZPracownikami);

            //fartowne
            var pairs = KluczGłównyRól.Select((t, i) => new KeyValuePair<string, string>(t, idRoli[i])).ToList();
            pairs.AddRange(
                kluczGłównyPracowników.Select((t, i) => new KeyValuePair<string, string>(t, idPracownika[i])));

            Insert(TabelaZPrzypisaniemRól, pairs, true);
        }

        public static bool UsuńPrzypisanieRoli(string rola, string użytkownik, bool wymuś = false)
        {
            List<string> idPracownika = IdPracownika(użytkownik);
            List<string> idRoli = IdRoli(rola);
            List<string> kluczGłównyPracowników = KluczGlowny(TabelaZPracownikami);

            //fartowne
            var pairs = KluczGłównyRól.Select((t, i) => new KeyValuePair<string, string>(t, idRoli[i])).ToList();
            pairs.AddRange(
                kluczGłównyPracowników.Select((t, i) => new KeyValuePair<string, string>(t, idPracownika[i])));

            if (!wymuś && idUzytejRoli.SequenceEqual(idRoli) &&
                idZalogowanegoPracownika.SequenceEqual(idPracownika)) return false;
            Delete(TabelaZPrzypisaniemRól, pairs);
            return true;
        }

        public static void NowaRola(string nazwa, List<KeyValuePair<string, string>> kolAtr)
        {
            kolAtr.Add(new KeyValuePair<string, string>(NazwaKolumnyRoli, nazwa));
            Insert(TabelaZRolami, kolAtr);
        }

        public static List<string> ListaKolumnRól()
        {
            return ListaKolumn(TabelaZRolami);
        }

        public static List<string> WierszRól(string nazwaRoli)
        {
            List<string> idRoli = IdRoli(nazwaRoli);
            List<string> kluczGlowny = KluczGlowny(TabelaZRolami);
            List<KeyValuePair<string, string>> pairs = kluczGlowny
                .Select((s, i) => new KeyValuePair<string, string>(s, idRoli[i])).ToList();
            List<List<string>> list = Select(TabelaZRolami, pairs);
            return list[0].ToList();
        }

        public static string GetNazwaKolmnyRól()
        {
            return NazwaKolumnyRoli;
        }

        public static List<string> KluczGlownyRól()
        {
            return KluczGlowny(TabelaZRolami);
        }

        public static bool CzyTabelaAdmińska(string s)
        {
            return TabeleAdmińskie.FindIndex(s2 => s2.Equals(s, StringComparison.InvariantCultureIgnoreCase)) > -1;
        }

        public static string GetNazwaKolumnyCzyAdmin()
        {
            return NazwaKolumnyCzyAdmin;
        }

        public static void EdycjaRoli(List<KeyValuePair<string, string>> idEdytowanej,
            List<KeyValuePair<string, string>> kolAtr)
        {
            Update(TabelaZRolami, kolAtr, idEdytowanej);
        }

        public static void UsuńRolę(string nazwaRoli)
        {
            List<string> idRoli = IdRoli(nazwaRoli);
            List<string> kluczGlowny = KluczGlowny(TabelaZRolami);
            List<KeyValuePair<string, string>> pairs = kluczGlowny
                .Select((t, i) => new KeyValuePair<string, string>(t, idRoli[i])).ToList();
            Delete(TabelaZRolami, pairs);
        }

        public static bool MożnaUsuwaćRole()
        {
            return CanDelete(TabelaZRolami);
        }

        public static bool MożnaUsuwaćPrzypisania()
        {
            return CanDelete(TabelaZPrzypisaniemRól);
        }

        public static bool MożnaDodawaćPrzypisania()
        {
            return CanInsert(TabelaZPrzypisaniemRól);
        }

        public static bool MożnaDodawaćRole()
        {
            return CanInsert(TabelaZRolami);
        }

        public static bool MożnaEdytowaćRole()
        {
            return CanUpdate(TabelaZRolami);
        }

        public static bool CzyZalogowanyAdmin()
        {
            return czyAdmin;
        }

        public static Dictionary<string, string> AktualneUprawnieniaAdminskich()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (string s in TabeleAdmińskie)
            {
                string uprawnienia = SelectUprawnienia(s);
                result.Add(s,uprawnienia);
            }
            return result;
        }

        public static bool MożnaUsuwaćUserów()
        {
            return CanDelete(TabelaZPracownikami);
        }

        public static string MojaNazwa()
        {
            return login;
        }

        public static void UsuńUżytkownika(string user)
        {
            Delete(TabelaZPracownikami,new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(NazwaKolumnyLoginow,user)
            });
        }

        public static bool MożnaDodawaćUżytkownika()
        {
            return CanInsert(TabelaZPracownikami);
        }

        public static bool MożnaEdytowaćUserów()
        {
            return CanUpdate(TabelaZPracownikami);
        }

        public static List<string> FormularzPracownika()
        {
            List<string> listaKolumn = ListaKolumn(TabelaZPracownikami);
            foreach (string s in ListaPólUseraNiePytać)
                listaKolumn.RemoveAll(s1 => s1.Equals(s, StringComparison.CurrentCultureIgnoreCase));
            return listaKolumn;
        }

        public static List<string> GetKluczGłównyPracowników()
        {
            return KluczGłównyPracowników;
        }

        public static void DodajUżytkownika(List<KeyValuePair<string, string>> kolWart)
        {
            int index = kolWart.FindIndex(
                pair => pair.Key.Equals(NazwaKolumnyHasel, StringComparison.CurrentCultureIgnoreCase));
            string haslo = kolWart[index].Value;
            kolWart.RemoveAt(index);
            kolWart.Add(new KeyValuePair<string, string>(NazwaKolumnyHasel,
                sha256_hash(haslo)));
            Insert(TabelaZPracownikami,kolWart);
        }

        public static List<KeyValuePair<string, string>> EdytowanyUżytkownik(string login)
        {
            List<string> GłownyPracowników = KluczGlowny(TabelaZPracownikami);
            List<string> idPracownikaEdytowanego = Select(TabelaZPracownikami, new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(NazwaKolumnyLoginow, login)
            }, GłownyPracowników)[0];

            List<KeyValuePair<string, string>> where = GłownyPracowników.Select((s, i) => new KeyValuePair<string, string>(s, idPracownikaEdytowanego[i])).ToList();

            List<string> listaKolumn = ListaKolumn(TabelaZPracownikami);
            List<KeyValuePair<string,string>> wynik = new List<KeyValuePair<string, string>>();
            List<List<string>> edytowanyPracownik = Select(TabelaZPracownikami, @where);
            for (var i = 0; i < listaKolumn.Count; i++)
                if (ListaPólUseraNiePytać.FindIndex(s => s.Equals(listaKolumn[i],StringComparison.CurrentCultureIgnoreCase))==-1)
                    wynik.Add(new KeyValuePair<string, string>(listaKolumn[i], edytowanyPracownik[0][i]));
            wynik.RemoveAll(pair => pair.Key.Equals(NazwaKolumnyHasel, StringComparison.CurrentCultureIgnoreCase));
            wynik.RemoveAll(pair => pair.Key.Equals(NazwaKolumnyLoginow, StringComparison.CurrentCultureIgnoreCase));
            return wynik;
        }

        public static void EdytujUżytkownika(List<KeyValuePair<string, string>> kolWart, List<KeyValuePair<string, string>> idKlucza)
        {
            Update(TabelaZPracownikami,kolWart,idKlucza);
        }

        public static bool CzyKolumnaZHasłem(string kolumna)
        {
            return NazwaKolumnyHasel.Equals(kolumna, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}