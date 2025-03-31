using Magazine.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace Magazine.Windows
{
    public partial class CheckDetails : Window, INotifyPropertyChanged
    {
        private readonly DbHelper _dbHelper = new DbHelper();
        private readonly int _saleId;
        private string _comment;
        private decimal _totalAmount;

        public event PropertyChangedEventHandler PropertyChanged;

        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged(nameof(Comment));
            }
        }

        public CheckDetails(int saleId)
        {
            InitializeComponent();
            _saleId = saleId;
            LoadSaleDetails();
            LoadSaleHeader();
            DataContext = this;
        }

        private void LoadSaleHeader()
        {
            string query = @"
            SELECT 
                s.id, 
                s.data_sale, 
                s.payment_type,
                ps.name as point_name,
                cps.start_balance,
                cps.total_balance,
                pp.first_name || ' ' || pp.last_name as cashier_name
            FROM sales s
            JOIN change_point_sales cps ON s.id_change_point_sale = cps.id
            JOIN point_sales ps ON cps.id_point_sale = ps.id
            JOIN employees e ON cps.id_employee_open = e.id
            JOIN physical_persons pp ON e.id_physical_person = pp.id
            WHERE s.id = @saleId";

            var parameters = new Dictionary<string, object> { { "@saleId", _saleId } };
            var result = _dbHelper.ExecuteQuery(query, parameters);

            if (result.Rows.Count > 0)
            {
                var row = result.Rows[0];
                NumberText.Text = row["id"].ToString();
                DateText.Text = Convert.ToDateTime(row["data_sale"]).ToString("dd.MM.yyyy HH:mm");
                PointNameText.Text = row["point_name"].ToString();
                CashierText.Text = row["cashier_name"].ToString();

                var paymentType = row["payment_type"].ToString();
                CashRadio.IsChecked = paymentType == "Наличные";
                CashlessRadio.IsChecked = paymentType == "Безнал";
                QrRadio.IsChecked = paymentType == "QR-код";
            }
        }

        private void LoadSaleDetails()
        {
            string query = @"
            SELECT 
                ds.count,
                ds.price,
                p.name as product_name,
                p.specifications,
                p.unit,
                (ds.price * ds.count * COALESCE(s.discount, 0) / 100) as auto_discount
            FROM detail_sales ds
            JOIN products p ON ds.id_product = p.id
            JOIN sales s ON ds.id_sale = s.id
            WHERE ds.id_sale = @saleId";

            var parameters = new Dictionary<string, object> { { "@saleId", _saleId } };
            var result = _dbHelper.ExecuteQuery(query, parameters);

            var details = new List<SaleDetailItem>();
            int position = 1;

            foreach (DataRow row in result.Rows)
            {
                details.Add(new SaleDetailItem
                {
                    PositionNumber = position++,
                    ProductName = row["product_name"].ToString(),
                    Specifications = row["specifications"].ToString(),
                    Quantity = Convert.ToDecimal(row["count"]),
                    Unit = row["unit"].ToString(),
                    Price = Convert.ToDecimal(row["price"]),
                    AutoDiscount = Convert.ToDecimal(row["auto_discount"])
                });
            }

            datagrid.ItemsSource = details;
            TotalAmount = details.Sum(d => d.Total);

            string commentQuery = "SELECT comment FROM sales WHERE id = @saleId";
            var commentResult = _dbHelper.ExecuteScalar(commentQuery, new Dictionary<string, object> { { "@saleId", _saleId } });
            Comment = commentResult?.ToString() ?? string.Empty;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string query = "UPDATE sales SET comment = @comment WHERE id = @saleId";
                var parameters = new Dictionary<string, object>
                {
                    { "@comment", Comment },
                    { "@saleId", _saleId }
                };

                _dbHelper.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Комментарий сохранен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}