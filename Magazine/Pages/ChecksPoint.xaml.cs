using Magazine.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Magazine.Pages
{
    public partial class ChecksPoint : Page
    {
        private DbHelper dbHelper;
        private ObservableCollection<SaleInfo> sales;
        private int currentShiftId;
        private int currentPointId;

        public ChecksPoint()
        {
            InitializeComponent();
            dbHelper = new DbHelper();
            LoadCurrentShift();
            LoadSales();
        }

        private void LoadCurrentShift()
        {
            string query = @"SELECT cps.id, cps.id_point_sale, 
                            ps.name as point_name,
                            e.id as employee_id,
                            pp.first_name || ' ' || pp.last_name as employee_name
                            FROM change_point_sales cps
                            JOIN point_sales ps ON cps.id_point_sale = ps.id
                            JOIN employees e ON cps.id_employee_open = e.id
                            JOIN physical_persons pp ON e.id_physical_person = pp.id
                            WHERE cps.id_employee_open = @employeeId 
                            AND cps.conducted = false";

            var parameters = new Dictionary<string, object>
            {
                { "@employeeId", Classes.AppContext.CurrentUserId }
            };

            DataTable result = dbHelper.ExecuteQuery(query, parameters);

            if (result.Rows.Count > 0)
            {
                currentShiftId = Convert.ToInt32(result.Rows[0]["id"]);
                currentPointId = Convert.ToInt32(result.Rows[0]["id_point_sale"]);
            }
            else
            {
                MessageBox.Show("Нет активной смены!");
                NavigationService?.GoBack();
            }
        }

        private void LoadSales()
        {
            string query = @"SELECT s.id, s.data_sale, s.payment_type, 
                           SUM(ds.count * ds.price) as total_amount,
                           COUNT(ds.id) as items_count,
                           ps.name as point_name,
                           pp.first_name || ' ' || pp.last_name as employee_name
                           FROM sales s
                           JOIN detail_sales ds ON s.id = ds.id_sale
                           JOIN change_point_sales cps ON s.id_change_point_sale = cps.shift_number
                           JOIN point_sales ps ON cps.id_point_sale = ps.id
                           JOIN employees e ON cps.id_employee_open = e.id
                           JOIN physical_persons pp ON e.id_physical_person = pp.id
                           GROUP BY s.id, s.data_sale, s.payment_type, ps.name, pp.first_name, pp.last_name
                           ORDER BY s.data_sale DESC";

            var parameters = new Dictionary<string, object>
            {
                { "@shiftId", currentShiftId }
            };

            DataTable result = dbHelper.ExecuteQuery(query, parameters);

            sales = new ObservableCollection<SaleInfo>();
            foreach (DataRow row in result.Rows)
            {
                sales.Add(new SaleInfo
                {
                    Id = Convert.ToInt32(row["id"]),
                    SaleDate = Convert.ToDateTime(row["data_sale"]),
                    PaymentType = row["payment_type"].ToString(),
                    TotalAmount = Convert.ToDecimal(row["total_amount"]),
                    ItemsCount = Convert.ToInt32(row["items_count"]),
                    PointName = row["point_name"].ToString(),
                    EmployeeName = row["employee_name"].ToString()
                });
            }

            datagrid.ItemsSource = sales;
        }

        private void OpenCheck_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void AddNewCheck_Click(object sender, RoutedEventArgs e)
        {
          
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSales();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }

    public class SaleInfo
    {
        public int Id { get; set; }
        public DateTime SaleDate { get; set; }
        public string PaymentType { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemsCount { get; set; }
        public string PointName { get; set; }
        public string EmployeeName { get; set; }
    }
}