using System.Windows;
using System.Windows.Controls;

namespace bsk___proba_2
{
    /// <summary>
    /// Interaction logic for UserPermission.xaml
    /// </summary>
    public partial class UserPermission : Window
    {
        public UserPermission()
        {
            InitializeComponent();
        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            DescribeRoleLabel.Content = "Opis Roli \""+((ListBoxItem)sender).Content+"\"";
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        
        }

        private void ComboBox_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem.IsSelected = true;
            ListBoxItem2.IsSelected = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
