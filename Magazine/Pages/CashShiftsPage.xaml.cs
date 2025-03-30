using Magazine.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Magazine.Pages
{
    public partial class CashShiftsPage : Page
    {
        private DbHelper dbHelper;

        public CashShiftsPage()
        {
            InitializeComponent();
            dbHelper = new DbHelper();
            LoadPointSales(); // Загружаем точки продажи
            LoadPointSalesForFilter(); // Загружаем точки продажи для фильтрации
            LoadData(); // Загружаем данные кассовых смен
        }

        // Загрузка точек продажи в ComboBox
        private void LoadPointSales()
        {
            try
            {
                string query = "SELECT id, name FROM point_sales";
                DataTable result = dbHelper.ExecuteQuery(query);

                if (result != null)
                {
                    var pointSalesList = new List<PointSale>();
                    foreach (DataRow row in result.Rows)
                    {
                        pointSalesList.Add(new PointSale
                        {
                            Id = Convert.ToInt32(row["id"]),
                            Name = row["name"].ToString()
                        });
                    }

                    point_saless.ItemsSource = pointSalesList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки точек продажи: {ex.Message}");
            }
        }

        // Загрузка данных кассовых смен с JOIN на имена сотрудников
        private void LoadData()
        {
            try
            {
                string query = @"
            SELECT 
                cps.conducted,
                cps.id,
                cps.shift_number, -- Добавляем shift_number
                ps.name AS point_sale_name,
                CONCAT(pp_open.first_name, ' ', pp_open.last_name) AS open_employee_name,
                CONCAT(pp_close.first_name, ' ', pp_close.last_name) AS close_employee_name,
                cps.date_start,
                cps.date_end,
                cps.start_balance,
                cps.total_balance,
                cps.description
            FROM public.change_point_sales cps
            JOIN point_sales ps ON cps.id_point_sale = ps.id
            LEFT JOIN employees e_open ON cps.id_employee_open = e_open.id
            LEFT JOIN physical_persons pp_open ON e_open.id_physical_person = pp_open.id
            LEFT JOIN employees e_close ON cps.id_employee_close = e_close.id
            LEFT JOIN physical_persons pp_close ON e_close.id_physical_person = pp_close.id
            ORDER BY cps.id"; // Сортировка по номеру смены

                DataTable result = dbHelper.ExecuteQuery(query);

                if (result != null && result.Rows.Count > 0)
                {
                    datagrid.ItemsSource = result.DefaultView;
                }
                else
                {
                    MessageBox.Show("Данные не найдены.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        // Метод для получения последнего баланса
        private decimal GetLastTotalBalance(int idPointSale)
        {
            string query = @"
                SELECT total_balance 
                FROM change_point_sales 
                WHERE id_point_sale = @idPointSale AND date_end IS NOT NULL 
                ORDER BY date_end DESC 
                LIMIT 1";

            var parameters = new Dictionary<string, object>
            {
                { "@idPointSale", idPointSale }
            };

            object result = dbHelper.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToDecimal(result) : 0;
        }

        // Метод для расчета итогового баланса
        private decimal CalculateTotalBalance(int shiftId)
        {
            string query = @"
                SELECT COALESCE(SUM(amount), 0) 
                FROM transactions 
                WHERE shift_id = @shift_id";

            var parameters = new Dictionary<string, object>
            {
                { "@shift_id", shiftId }
            };

            object totalAmount = dbHelper.ExecuteScalar(query, parameters) ?? 0;
            decimal startBalance = GetStartBalance(shiftId);

            return startBalance + Convert.ToDecimal(totalAmount);
        }

        // Получение начального баланса для смены
        private decimal GetStartBalance(int shiftId)
        {
            string query = @"
                SELECT start_balance 
                FROM change_point_sales 
                WHERE id = @shift_id";

            var parameters = new Dictionary<string, object>
            {
                { "@shift_id", shiftId }
            };

            object result = dbHelper.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToDecimal(result) : 0;
        }

        // Кнопка "Открыть смену"
        private void OpenShift_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedPointSale = point_saless.SelectedItem as PointSale;
                if (selectedPointSale == null)
                {
                    MessageBox.Show("Выберите точку продажи.");
                    return;
                }

                int idPointSale = selectedPointSale.Id;

                // Проверка, есть ли уже открытая смена для этой точки
                string checkQuery = @"
            SELECT COUNT(*) 
            FROM change_point_sales 
            WHERE id_point_sale = @idPointSale AND conducted = false";

                var checkParams = new Dictionary<string, object>
        {
            { "@idPointSale", idPointSale }
        };

                int openShiftsCount = Convert.ToInt32(dbHelper.ExecuteScalar(checkQuery, checkParams));

                if (openShiftsCount > 0)
                {
                    MessageBox.Show("Для этой точки продажи уже есть открытая смена.");
                    return;
                }

                // Если проверка пройдена, открываем смену
                decimal startBalance = GetLastTotalBalance(idPointSale);
                int idEmployeeOpen = Classes.AppContext.CurrentUserId;
                DateTime dateStart = DateTime.Now;
                string description = "Новая кассовая смена";

                // Вставка новой записи (triigger автоматически добавит shift_number)
                string query = @"
            INSERT INTO change_point_sales 
            (id_point_sale, id_employee_open, date_start, start_balance, description, conducted) 
            VALUES 
            (@id_point_sale, @id_employee_open, @date_start, @start_balance, @description, false)";

                var parameters = new Dictionary<string, object>
        {
            { "@id_point_sale", idPointSale },
            { "@id_employee_open", idEmployeeOpen },
            { "@date_start", dateStart },
            { "@start_balance", startBalance },
            { "@description", description }
        };

                int rowsAffected = dbHelper.ExecuteNonQuery(query, parameters);

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Смена успешно открыта!");
                    LoadData(); // Обновляем DataGrid
                }
                else
                {
                    MessageBox.Show("Ошибка при открытии смены.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // Кнопка "Закрыть смену"
        private void CloseShift_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedRow = datagrid.SelectedItem as DataRowView;
                if (selectedRow == null)
                {
                    MessageBox.Show("Выберите смену для закрытия.");
                    return;
                }

                int shiftId = Convert.ToInt32(selectedRow["id"]);
                bool conducted = Convert.ToBoolean(selectedRow["conducted"]);

                // Проверка, что смена не закрыта
                if (conducted)
                {
                    MessageBox.Show("Эта смена уже закрыта.");
                    return;
                }

                // Расчет итогового баланса
                decimal totalBalance = CalculateTotalBalance(shiftId);
                int idEmployeeClose = Classes.AppContext.CurrentUserId;
                DateTime dateEnd = DateTime.Now;

                // Обновление смены
                string query = @"
            UPDATE change_point_sales 
            SET 
                id_employee_close = @id_employee_close,
                date_end = @date_end,
                total_balance = @total_balance,
                conducted = true
            WHERE id = @shift_id AND conducted = false"; // Дополнительная проверка в запросе

                var parameters = new Dictionary<string, object>
        {
            { "@id_employee_close", idEmployeeClose },
            { "@date_end", dateEnd },
            { "@total_balance", totalBalance },
            { "@shift_id", shiftId }
        };

                int rowsAffected = dbHelper.ExecuteNonQuery(query, parameters);

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Смена успешно закрыта!");
                    LoadData(); // Обновляем DataGrid
                }
                else
                {
                    MessageBox.Show("Ошибка при закрытии смены. Возможно, смена уже закрыта.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // Кнопка сворачивания
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        // Кнопка закрытия страницы
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = NavigationService;
            navigationService?.Navigate(null);
        }

        private void LoadPointSalesForFilter()
        {
            try
            {
                string query = "SELECT id, name FROM point_sales";
                DataTable result = dbHelper.ExecuteQuery(query);

                if (result != null)
                {
                    var pointSalesList = new List<PointSale>
            {
                // Добавляем "Все" как отдельный элемент с фиктивным Id = -1
                new PointSale { Id = -1, Name = "Все" }
            };

                    foreach (DataRow row in result.Rows)
                    {
                        pointSalesList.Add(new PointSale
                        {
                            Id = Convert.ToInt32(row["id"]),
                            Name = row["name"].ToString()
                        });
                    }

                    // Привязываем данные к ComboBox
                    filterPointSale.ItemsSource = pointSalesList;
                    filterPointSale.DisplayMemberPath = "Name"; // Отображаемое поле
                    filterPointSale.SelectedValuePath = "Id";   // Значение для выбора
                    filterPointSale.SelectedIndex = 0;         // По умолчанию выбираем "Все"
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки точек продажи: {ex.Message}");
            }
        }

        private void FilterPointSale_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Получаем выбранную точку продажи из ComboBox
                var selectedPointSale = filterPointSale.SelectedItem as PointSale;

                string query;
                var parameters = new Dictionary<string, object>();

                if (selectedPointSale == null || selectedPointSale.Id == -1)
                {
                    // Если выбрано "Все", загружаем все смены
                    query = @"
                SELECT 
                    cps.conducted,
                    cps.id,
                    cps.shift_number,
                    ps.name AS point_sale_name,
                    CONCAT(pp_open.first_name, ' ', pp_open.last_name) AS open_employee_name,
                    CONCAT(pp_close.first_name, ' ', pp_close.last_name) AS close_employee_name,
                    cps.date_start,
                    cps.date_end,
                    cps.start_balance,
                    cps.total_balance,
                    cps.description
                FROM public.change_point_sales cps
                JOIN point_sales ps ON cps.id_point_sale = ps.id
                LEFT JOIN employees e_open ON cps.id_employee_open = e_open.id
                LEFT JOIN physical_persons pp_open ON e_open.id_physical_person = pp_open.id
                LEFT JOIN employees e_close ON cps.id_employee_close = e_close.id
                LEFT JOIN physical_persons pp_close ON e_close.id_physical_person = pp_close.id
                ORDER BY cps.date_start DESC";
                }
                else
                {
                    // Если выбрана конкретная точка продажи
                    query = @"
                SELECT 
                    cps.conducted,
                    cps.id,
                    cps.shift_number,
                    ps.name AS point_sale_name,
                    CONCAT(pp_open.first_name, ' ', pp_open.last_name) AS open_employee_name,
                    CONCAT(pp_close.first_name, ' ', pp_close.last_name) AS close_employee_name,
                    cps.date_start,
                    cps.date_end,
                    cps.start_balance,
                    cps.total_balance,
                    cps.description
                FROM public.change_point_sales cps
                JOIN point_sales ps ON cps.id_point_sale = ps.id
                LEFT JOIN employees e_open ON cps.id_employee_open = e_open.id
                LEFT JOIN physical_persons pp_open ON e_open.id_physical_person = pp_open.id
                LEFT JOIN employees e_close ON cps.id_employee_close = e_close.id
                LEFT JOIN physical_persons pp_close ON e_close.id_physical_person = pp_close.id
                WHERE cps.id_point_sale = @idPointSale
                ORDER BY cps.date_start DESC";

                    parameters.Add("@idPointSale", selectedPointSale.Id);
                }

                // Выполняем запрос и обновляем DataGrid
                DataTable result = dbHelper.ExecuteQuery(query, parameters);

                if (result != null && result.Rows.Count > 0)
                {
                    datagrid.ItemsSource = result.DefaultView;
                }
                else
                {
                    MessageBox.Show("Данные не найдены.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }


        private void Diapazone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Открываем окно выбора диапазона дат
                var dateRangeWindow = new Windows.DateRangeWindow();
                bool? result = dateRangeWindow.ShowDialog();

                if (result == true)
                {
                    // Получаем выбранные значения
                    DateTime startDate = dateRangeWindow.StartDate.Value;
                    DateTime endDate = dateRangeWindow.EndDate.Value;
                    string filterType = dateRangeWindow.FilterType;

                    // Формируем запрос в зависимости от выбранного типа фильтра
                    string query;
                    var parameters = new Dictionary<string, object>();

                    if (filterType == "Дата открытия")
                    {
                        query = @"
                    SELECT 
                        cps.conducted,
                        cps.id,
                        cps.shift_number,
                        ps.name AS point_sale_name,
                        CONCAT(pp_open.first_name, ' ', pp_open.last_name) AS open_employee_name,
                        CONCAT(pp_close.first_name, ' ', pp_close.last_name) AS close_employee_name,
                        cps.date_start,
                        cps.date_end,
                        cps.start_balance,
                        cps.total_balance,
                        cps.description
                    FROM public.change_point_sales cps
                    JOIN point_sales ps ON cps.id_point_sale = ps.id
                    LEFT JOIN employees e_open ON cps.id_employee_open = e_open.id
                    LEFT JOIN physical_persons pp_open ON e_open.id_physical_person = pp_open.id
                    LEFT JOIN employees e_close ON cps.id_employee_close = e_close.id
                    LEFT JOIN physical_persons pp_close ON e_close.id_physical_person = pp_close.id
                    WHERE cps.date_start >= @startDate AND cps.date_start <= @endDate
                    ORDER BY cps.date_start DESC";
                    }
                    else if (filterType == "Дата закрытия")
                    {
                        query = @"
                    SELECT 
                        cps.conducted,
                        cps.id,
                        cps.shift_number,
                        ps.name AS point_sale_name,
                        CONCAT(pp_open.first_name, ' ', pp_open.last_name) AS open_employee_name,
                        CONCAT(pp_close.first_name, ' ', pp_close.last_name) AS close_employee_name,
                        cps.date_start,
                        cps.date_end,
                        cps.start_balance,
                        cps.total_balance,
                        cps.description
                    FROM public.change_point_sales cps
                    JOIN point_sales ps ON cps.id_point_sale = ps.id
                    LEFT JOIN employees e_open ON cps.id_employee_open = e_open.id
                    LEFT JOIN physical_persons pp_open ON e_open.id_physical_person = pp_open.id
                    LEFT JOIN employees e_close ON cps.id_employee_close = e_close.id
                    LEFT JOIN physical_persons pp_close ON e_close.id_physical_person = pp_close.id
                    WHERE cps.date_end >= @startDate AND cps.date_end <= @endDate
                    ORDER BY cps.date_start DESC";
                    }
                    else
                    {
                        MessageBox.Show("Неверный тип фильтра.");
                        return;
                    }

                    // Добавляем параметры
                    parameters.Add("@startDate", startDate);
                    parameters.Add("@endDate", endDate);

                    // Выполняем запрос и обновляем DataGrid
                    DataTable result1 = dbHelper.ExecuteQuery(query, parameters);

                    if (result1 != null && result1.Rows.Count > 0)
                    {
                        datagrid.ItemsSource = result1.DefaultView;
                    }
                    else
                    {
                        MessageBox.Show("Данные не найдены.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}