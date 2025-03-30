using System;
using System.Windows;
using System.Windows.Controls;

namespace Magazine.Windows
{
    public partial class DateRangeWindow : Window
    {
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public string FilterType { get; private set; }

        public DateRangeWindow()
        {
            InitializeComponent();
        }

        // Применение фильтра
        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (startDatePicker.SelectedDate == null || endDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите обе даты.");
                return;
            }

            StartDate = startDatePicker.SelectedDate.Value;
            EndDate = endDatePicker.SelectedDate.Value;
            FilterType = (filterType.SelectedItem as ComboBoxItem)?.Content.ToString();

            DialogResult = true;
            Close();
        }

        // Отмена
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}