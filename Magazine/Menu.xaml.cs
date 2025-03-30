using Magazine.Pages;
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
                case "CashShifts":
                    {
                        MainFrame.Navigate(new Uri("Pages/CashShiftsPage.xaml", UriKind.Relative));
                        if (MainFrame.Content is CashShiftsPage cashShiftsPage)
                        {
                            // Устанавливаем видимость или другие свойства
                            cashShiftsPage.Visibility = Visibility.Visible; // Например, делаем страницу видимой
                        }                   
                        break;
                    }

                case "Calculator":
                    {
                        var calculatorWindow = new Windows.CalculatorWindow();
                        calculatorWindow.Show();
                        break;
                    }

                case "ChecksPoint":
                    {
                        MainFrame.Navigate(new Uri("Pages/ChecksPoint.xaml", UriKind.Relative));
                        if (MainFrame.Content is ChecksPoint checkspoint)
                        {
                            // Устанавливаем видимость или другие свойства
                            checkspoint.Visibility = Visibility.Visible; // Например, делаем страницу видимой
                            MessageBox.Show("Привет");
                        }
                        break;
                    }




                    }
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

       
    }

}