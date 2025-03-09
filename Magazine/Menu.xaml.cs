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

namespace Magazine
{
    /// <summary>
    /// Логика взаимодействия для Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        public Menu()
        {
            InitializeComponent();
            
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            string tag = menuItem?.Tag?.ToString();

            switch (tag)
            {
                case "Prices":
                    MainFrame.Navigate(new Uri("PricesPage.xaml", UriKind.Relative));
                    break;
             
            }
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Закрытие окна
        }
    }

}