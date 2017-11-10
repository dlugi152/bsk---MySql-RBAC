using System.Windows;
using System.Windows.Input;

namespace bsk___proba_2
{
    /// <summary>
    /// Logika interakcji dla klasy ProstyTextBox.xaml
    /// </summary>
    public partial class ProstyTextBox
    {
        public ProstyTextBox() {
            InitializeComponent();
        }

        public ProstyTextBox(string klucz, string wartosc = "") {
            InitializeComponent();
            NazwaKolumny.Content = klucz;
            WpisanyText.Text = wartosc;
            WpisanyText.Focus();
            WpisanyText.SelectAll();
        }

        public string TextDoPrzekazania => WpisanyText.Text;

        private void OKButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter)
                OKButton_Click(null, null);
        }
    }
}