using System;

public class SaleInfo
{
    public int Id { get; set; }
    public DateTime SaleDate { get; set; }
    public string PaymentType { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemsCount { get; set; }
    public string PointName { get; set; }
    public string EmployeeName { get; set; }
    public DateTime ShiftStartDate { get; set; }

    // Форматированная дата смены для отображения
    public string ShiftStartFormatted => ShiftStartDate.ToString("dd.MM.yyyy HH:mm");
}