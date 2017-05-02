using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MySql.Data.MySqlClient;

namespace bsk___proba_2 {
    public class RBACowyConnector {
        private static MySqlConnection polaczenie;
        private static string serwer;
        private static string login;
        private static string haslo;
        private static string port;
        private static List<string> idZalogowanegoPracownika;
        private static List<string> idUzytejRoli;
        private static List<string> KluczGłównyRól;
        private static bool czyAdmin;
        private const string NazwaBazy = "bsk";
        private const string TabelaZPracownikami = "pracownik";
        private const string TabelaZRolami = "rola";
        private const string NazwaKolumnyLoginow = "login_uzytkownika";
        private const string NazwaKolumnyCzyAdmin = "adminska";
        private const string NazwaKolumnyRoli = "nazwa";
        private const string TabelaZPrzypisaniemRól = "przypisanie_roli";

        private static readonly IList<string> TabeleAdmińskie = new ReadOnlyCollection<string>
            (new List<string> {TabelaZRolami, TabelaZPrzypisaniemRól, TabelaZPracownikami});

        public enum KodyBledow {
            BlednyLoginHaslo,
            BladLaczenia,
            InnyBlad,
            NieMoznaZamknac,
            BrakInsert,
            BrakSelect,
            BrakDelete,
            BrakUpdate,
            BledneZapytanie
        }

        public class Bledy : Exception {
            public KodyBledow Kod;
            public string Wiadomosc;

            public Bledy(KodyBledow kod) {
                Kod = kod;
                Wiadomosc = "";
            }

            public Bledy(KodyBledow kod, string wiadomosc) {
                Kod = kod;
                Wiadomosc = wiadomosc;
            }
        }

        static RBACowyConnector() {

        }

        public static bool CzyZalogowanyAdmin() {
            return czyAdmin;
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
            try {
                polaczenie = new MySqlConnection(connectionString);
                TestujPolaczenie();
                UstawIdZalogowanego();
                UstawKluczGłównyRól();
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        private static void UstawKluczGłównyRól() {
            KluczGłównyRól = KluczGlowny(TabelaZRolami);
        }

        private static List<string> IdPracownika(string login) {
            string glowny = KluczGlowny(TabelaZPracownikami).Aggregate("(", (current, s) => current + s + ", ");
            glowny = glowny.Remove(glowny.Length - 2);
            glowny += ")";
            string query = "SELECT " + glowny + " FROM " + TabelaZPracownikami + " WHERE " + NazwaKolumnyLoginow +
                           " = @login";
            List<string> idUżytkownika = new List<string>();
            if (OtworzPolaczenie()) {
                //brak sprawdzania czy można selectować tabelę z pracownikami
                MySqlCommand cmd = new MySqlCommand(query, polaczenie);
                cmd.Parameters.Add(new MySqlParameter("@login", login));
                MySqlDataReader dataReader = cmd.ExecuteReader();

                dataReader.Read(); //zał. loginy są unikalne
                for (int i = 0; i < dataReader.FieldCount; i++)
                    idUżytkownika.Add(dataReader.GetString(i));

                dataReader.Close();
                ZamknijPolaczenie();
            }
            return idUżytkownika;
        }

        private static void UstawIdZalogowanego() {
            idZalogowanegoPracownika = IdPracownika(login);
        }

        private static List<string> IdRoli(string nazwa) {
            string query = " SELECT " + KluczGlowny(TabelaZRolami).Aggregate("", (current, s) => current + s + ", ") +
                           NazwaKolumnyCzyAdmin + " FROM " + TabelaZRolami + " WHERE " + NazwaKolumnyRoli +
                           " = @nazwa";
            List<string> idRoli = new List<string>();
            if (OtworzPolaczenie()) {
                //brak sprawdzania czy można selectować tabelę z rolami
                MySqlCommand cmd = new MySqlCommand(query, polaczenie);
                cmd.Parameters.Add(new MySqlParameter("@nazwa", nazwa));
                MySqlDataReader dataReader = cmd.ExecuteReader();

                dataReader.Read(); //zał. nazwy ról są unikalne
                for (int i = 0; i < dataReader.FieldCount - 1; i++) //-1, bo wyzej czyadmin jest ostatnie
                    idRoli.Add(dataReader.GetString(i));
                czyAdmin = Boolean.Parse(dataReader.GetString(dataReader.FieldCount - 1));
                dataReader.Close();
                ZamknijPolaczenie();
            }
            return idRoli;
        }

        public static void UstawAktualnąRolę(string nazwa) {
            idUzytejRoli = IdRoli(nazwa);
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

        public static void Insert(string tabela, List<KeyValuePair<string, string>> kolAtr, bool nieIstnieją = false) {
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


            if (CanInsert(tabela)) {
                if (!OtworzPolaczenie()) return;
                try {
                    MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                    foreach (var t in kolAtr)
                        if (t.Value != "")
                            cmd.Parameters.Add(new MySqlParameter("@" + t.Key, t.Value));
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex) {
                    ZamknijPolaczenie();
                    throw new Bledy(KodyBledow.BledneZapytanie, ex.Message);
                }
                finally {
                    ZamknijPolaczenie();
                }
            }
            else
                throw new Bledy(KodyBledow.BrakInsert);
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
                (current, pair) => current + "(" + pair.Key + " = @" + pair.Key + ") AND ");
            zapytanie = zapytanie.Remove(zapytanie.Length - 5);


            if (CanUpdate(tabela)) {
                if (!OtworzPolaczenie()) return;
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
                catch (Exception ex) {
                    throw new Bledy(KodyBledow.BledneZapytanie, ex.Message);
                }
                finally {
                    ZamknijPolaczenie();
                }
            }
            else
                throw new Bledy(KodyBledow.BrakUpdate);
        }

        public static void Delete(string tabela, List<KeyValuePair<string, string>> kluczGlowny) {
            string zapytanie = "DELETE FROM " + tabela + " WHERE ";
            zapytanie = kluczGlowny.Aggregate(zapytanie,
                (current, pair) => current + "(" + pair.Key + " = @" + pair.Key + ") AND ");
            zapytanie = zapytanie.Remove(zapytanie.Length - 5);


            if (CanDelete(tabela)) {
                if (!OtworzPolaczenie()) return;
                try {
                    MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                    foreach (var t in kluczGlowny)
                        if (t.Value != "")
                            cmd.Parameters.Add(new MySqlParameter("@" + t.Key, t.Value));
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex) {
                    throw new Bledy(KodyBledow.BledneZapytanie, ex.Message);
                }
                finally {
                    ZamknijPolaczenie();
                }
            }
            else
                throw new Bledy(KodyBledow.BrakDelete);
        }

        public static List<string> ListaTabel(bool? admińskie) {
            string zapytanie = "show tables";
            List<string> list = new List<string>();

            if (OtworzPolaczenie()) {
                MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                List<string> listaWszystkich = new List<string>();
                while (dataReader.Read())
                    listaWszystkich.Add(dataReader.GetString(0));
                dataReader.Close();
                ZamknijPolaczenie();
                if (admińskie == null)
                    list.AddRange(listaWszystkich.Where(
                        nowy => !TabeleAdmińskie.Contains(nowy) &&
                                (CanSelect(nowy) || CanDelete(nowy) || CanInsert(nowy) || CanUpdate(nowy))));
                else
                    list.AddRange(listaWszystkich.Where(s => admińskie.Value && TabeleAdmińskie.Contains(s) ||
                                                             !admińskie.Value && !TabeleAdmińskie.Contains(s)));
            }
            return list;
        }

        private static string DomyślnaWartość(string tabela, string kolumna) {
            string zapytanie = "SELECT Column_Default FROM information_schema.columns " +
                               "WHERE (`TABLE_NAME` = @tabela)  AND (`COLUMN_NAME` = @kolumna);";
            string domyślna = "";

            if (OtworzPolaczenie()) {
                //if (CanSelect(tabela)) {
                MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                cmd.Parameters.Add(new MySqlParameter("@tabela", tabela));
                cmd.Parameters.Add(new MySqlParameter("@kolumna", kolumna));
                MySqlDataReader dataReader = cmd.ExecuteReader();

                dataReader.Read();
                try {
                    domyślna = dataReader.GetString(0);
                }
                catch (Exception) {
                }
                finally {

                    dataReader.Close();
                    //}
                    //else {
                    //    ZamknijPolaczenie();
                    //    throw new Bledy(KodyBledow.BrakSelect);
                    //}
                    ZamknijPolaczenie();
                }
            }
            return domyślna;
        }

        public static List<string> KluczGlowny(string tabela) {
            string zapytanie = "SELECT COLUMN_NAME FROM information_schema.columns " +
                               "WHERE (`TABLE_NAME` = @tabela)  AND (`COLUMN_KEY` = \'PRI\');";

            List<string> glowny = new List<string>();

            if (OtworzPolaczenie()) {
                //if (CanSelect(tabela)) {
                MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                cmd.Parameters.Add(new MySqlParameter("@tabela", tabela));
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                    glowny.Add(dataReader.GetString(0));

                dataReader.Close();
                //}
                //else {
                //    ZamknijPolaczenie();
                //    throw new Bledy(KodyBledow.BrakSelect);
                //}
                ZamknijPolaczenie();
            }
            return glowny;
        }

        public static List<string> ListaKolumn(string tabela) {
            string zapytanie = "SELECT column_name " +
                               "FROM information_schema.columns " +
                               "WHERE table_name=@tabela";
            List<string> list = new List<string>();

            if (CanSelect(tabela)) {
                if (OtworzPolaczenie()) {
                    MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                    cmd.Parameters.Add(new MySqlParameter("@tabela", tabela));
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    while (dataReader.Read())
                        list.Add(dataReader.GetString(0));
                    //przy takim zapytaniu na pewno nie trzeba pętli, wystarczy (0)

                    dataReader.Close();
                    ZamknijPolaczenie();
                }
            }
            else
                throw new Bledy(KodyBledow.BrakSelect);
            return list;
        }

        public static List<List<string>> Select(string tabela) {
            string query = "SELECT * FROM " + tabela;
            List<List<string>> list = new List<List<string>>();

            if (CanSelect(tabela)) {
                if (OtworzPolaczenie()) {
                    MySqlCommand cmd = new MySqlCommand(query, polaczenie);
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    while (dataReader.Read()) {
                        List<string> nowaLista = new List<string>();
                        for (int i = 0; i < dataReader.FieldCount; i++)
                            nowaLista.Add(dataReader.GetString(i));
                        list.Add(nowaLista);
                    }
                    dataReader.Close();
                    ZamknijPolaczenie();
                }
            }
            else
                throw new Bledy(KodyBledow.BrakSelect);
            return list;
        }

        private static string SelectUprawnienia(string table) {
            string zapytanie = "SELECT " + table + " FROM " + TabelaZRolami + " WHERE (";
            zapytanie = KluczGłównyRól.Aggregate(zapytanie, (current, t) => current + t + " = @" + t + " and ");
            zapytanie = zapytanie.Remove(zapytanie.Length - 5);
            zapytanie += ")";
            string uprawnienia = "";
            if (OtworzPolaczenie()) {
                MySqlCommand cmd = new MySqlCommand(zapytanie, polaczenie);
                for (var i = 0; i < idUzytejRoli.Count; i++)
                    cmd.Parameters.Add(new MySqlParameter("@" + KluczGłównyRól[i], idUzytejRoli[i]));
                MySqlDataReader dataReader = cmd.ExecuteReader();
                if (dataReader.Read())
                    uprawnienia = dataReader.GetString(0);
                dataReader.Close();
                ZamknijPolaczenie();
            }
            return uprawnienia;
        }

        public static bool CanSelect(string table) {
            return CzyZalogowanyAdmin() || SelectUprawnienia(table)[0] != '-';
        }

        public static bool CanInsert(string table) {
            return CzyZalogowanyAdmin() || SelectUprawnienia(table)[1] != '-';
        }

        public static bool CanUpdate(string table) {
            return CzyZalogowanyAdmin() || SelectUprawnienia(table)[2] != '-';
        }

        public static bool CanDelete(string table) {
            return CzyZalogowanyAdmin() || SelectUprawnienia(table)[3] != '-';
        }

        public static List<string> MojeRoleNazwy() {
            return RoleUżytkownika(login);
        }

        private static List<string> SelectJednąKolumnę(string kolumna, string tabela) {
            string query = "SELECT " + kolumna + " FROM " + tabela;
            List<string> wynik = new List<string>();

            if (CzyZalogowanyAdmin() || CanSelect(tabela)) {
                if (OtworzPolaczenie()) {
                    MySqlCommand cmd = new MySqlCommand(query, polaczenie);
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    while (dataReader.Read())
                        wynik.Add(dataReader.GetString(0));
                    dataReader.Close();
                    ZamknijPolaczenie();
                }
            }
            else
                throw new Bledy(KodyBledow.BrakSelect);
            return wynik;
        }

        public static List<string> ListaPracowników() {
            return SelectJednąKolumnę(NazwaKolumnyLoginow, TabelaZPracownikami);
        }

        public static List<string> ListaWszystkichRól() {
            return SelectJednąKolumnę(NazwaKolumnyRoli, TabelaZRolami);
        }

        public static List<string> RoleUżytkownika(string użytkownik) {
            List<string> kluczGlownyRól = KluczGlowny(TabelaZRolami);
            List<string> kluczGłównyPracowników = KluczGlowny(TabelaZPracownikami);
            string query =
                kluczGlownyRól.Aggregate(
                    "SELECT " + NazwaKolumnyRoli + " FROM " + TabelaZRolami + " INNER JOIN (SELECT " +
                    TabelaZPracownikami + "." + NazwaKolumnyLoginow + ",",
                    (current, s) => current + TabelaZPrzypisaniemRól + "." + s + ",");
            query = query.Remove(query.Length - 1);
            query += " FROM " + TabelaZPrzypisaniemRól + " INNER JOIN " + TabelaZPracownikami + " ON ";
            query = kluczGłównyPracowników.Aggregate(query,
                (current, s) => current + TabelaZPracownikami + "." + s + " = " + TabelaZPrzypisaniemRól + "." + s +
                                " and ");
            query = query.Remove(query.Length - 5);
            query += ") as tmp ON ";
            query = kluczGlownyRól.Aggregate(query,
                (current, s) => current + TabelaZRolami + "." + s + " = tmp." + s + " and ");
            query = query.Remove(query.Length - 5);
            query += " WHERE tmp." + NazwaKolumnyLoginow + " = @login";

            List<string> listaRól = new List<string>();
            if (OtworzPolaczenie()) {
                //brak sprawdzania czy można selectować tabelę z rolami
                MySqlCommand cmd = new MySqlCommand(query, polaczenie);
                cmd.Parameters.Add(new MySqlParameter("@login", użytkownik));
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                    listaRól.Add(dataReader.GetString(0)); //0 bo zapytanie czyta tylko 1 kolumnę - nazwę

                dataReader.Close();
                ZamknijPolaczenie();
            }
            return listaRól;
        }

        public static void DodajRolę(string rola, string użytkownik) {
            List<string> idPracownika = IdPracownika(użytkownik);
            List<string> idRoli = IdRoli(rola);
            List<string> kluczGłównyPracowników = KluczGlowny(TabelaZPracownikami);

            //fartowne
            var pairs = KluczGłównyRól.Select((t, i) => new KeyValuePair<string, string>(t, idRoli[i])).ToList();
            pairs.AddRange(
                kluczGłównyPracowników.Select((t, i) => new KeyValuePair<string, string>(t, idPracownika[i])));

            Insert(TabelaZPrzypisaniemRól, pairs, true);
        }

        public static bool UsuńRolę(string rola, string użytkownik, bool wymuś = false) {
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

        public static void NowaRola(string nazwa, List<KeyValuePair<string, string>> kolAtr) {
            kolAtr.Add(new KeyValuePair<string, string>(NazwaKolumnyRoli, nazwa));
            Insert(TabelaZRolami,kolAtr);
        }
    }
}