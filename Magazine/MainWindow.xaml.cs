using Magazine.Classes;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static BCrypt.Net.BCrypt;

namespace Magazine
{
    public partial class MainWindow : Window
    {
        private DbHelper dbHelper;

        public MainWindow()
        {
            InitializeComponent();
            dbHelper = new DbHelper();

            // Загрузка имен пользователей в ListBox
            string query = "SELECT username, id_employee FROM Users"; 
            DataTable result = dbHelper.ExecuteQuery(query);

            if (result != null)
            {
                var users = new List<User>();
                foreach (DataRow row in result.Rows)
                {
                    users.Add(new User
                    {
                        IdEmployee = Convert.ToInt32(row["id_employee"]), // Используем id_employee
                        Username = row["username"].ToString()
                    });
                }

                usersenter.ItemsSource = users;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранный ID пользователя
            var selectedUser = usersenter.SelectedItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int employeeId = selectedUser.IdEmployee; 
            string username = selectedUser.Username;

            string enteredPassword = passenter.Password;

            // Проверка на пустой пароль
            if (string.IsNullOrEmpty(enteredPassword))
            {
                MessageBox.Show("Пароль не может быть пустым.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // SQL-запрос для получения хешированного пароля
            string query = "SELECT password_hash FROM Users WHERE id = @id";
            var parameters = new Dictionary<string, object>
    {
        { "@id", employeeId }
    };

            DataTable result = dbHelper.ExecuteQuery(query, parameters);

            if (result != null && result.Rows.Count > 0)
            {
                string storedHashedPassword = result.Rows[0]["password_hash"].ToString();

                // Проверка пароля
                bool isPasswordValid = Verify(enteredPassword, storedHashedPassword);

                if (isPasswordValid)
                {
                    MessageBox.Show("Вход выполнен успешно.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                   

                   
                    Classes.AppContext.CurrentUserId = employeeId;

                    // Открываем главное меню
                    Menu menu = new Menu();
                    menu.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Passenter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click(sender, e); // Вызываем обработчик кнопки
            }
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click(sender, e); 
            }
        }
    }
}