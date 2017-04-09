using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace bsk___proba_2
{
    /// <summary>
    /// Logika interakcji dla klasy ProstyTextBox.xaml
    /// </summary>
    public partial class ProstyTextBox : Window
    {
        public ProstyTextBox()
        {
            InitializeComponent();
        }

        public ProstyTextBox(string klucz, string wartosc = "") {
            InitializeComponent();
            NazwaKolumny.Content = klucz;
            WpisanyText.Text = wartosc;
            WpisanyText.Focus();
            WpisanyText.SelectAll();
        }

        public string TextDoPrzekazania {
            get { return WpisanyText.Text; }
        }

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
