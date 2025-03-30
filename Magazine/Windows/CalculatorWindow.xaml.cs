using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Magazine.Windows
{
    public partial class CalculatorWindow : Window
    {
        private string currentInput = "0";
        private string leftOperand = string.Empty;
        private string rightOperand = string.Empty;
        private string currentOperator = string.Empty;
        private readonly List<string> history = new List<string>();

        public CalculatorWindow()
        {
            InitializeComponent();
        }

        private void UpdateDisplay()
        {
            display.Text = currentInput;
        }

        private void UpdateHistory()
        {
            historyList.ItemsSource = null;
            historyList.ItemsSource = history;
        }

        private void Number_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            string number = button.Content.ToString();

            if (currentOperator == "=")
            {
                ClearAll();
            }

            if (string.IsNullOrEmpty(currentOperator))
            {
                // Ввод левого операнда
                if (currentInput == "0")
                {
                    leftOperand = number;
                }
                else
                {
                    leftOperand += number;
                }
                currentInput = leftOperand;
            }
            else
            {
                // Ввод правого операнда
                if (currentInput == "0")
                {
                    rightOperand = number;
                }
                else
                {
                    rightOperand += number;
                }
                currentInput = rightOperand;
            }

            UpdateDisplay();
        }

        private void Decimal_Click(object sender, RoutedEventArgs e)
        {
            if (!currentInput.Contains(".") && currentOperator != "=")
            {
                currentInput += ".";
                UpdateDisplay();
            }
        }

        private void Operator_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            string newOperator = button.Content.ToString();

            if (!string.IsNullOrEmpty(currentInput))
            {
                if (string.IsNullOrEmpty(leftOperand))
                {
                    leftOperand = currentInput;
                }
                else if (!string.IsNullOrEmpty(rightOperand))
                {
                    Equals_Click(sender, e); // Автоматически завершаем предыдущую операцию
                }

                currentOperator = newOperator;
                currentInput = "0"; // Сбрасываем ввод для правого операнда
            }
        }

        private void Sign_Click(object sender, RoutedEventArgs e)
        {
            if (currentInput.StartsWith("-"))
            {
                currentInput = currentInput.Substring(1);
            }
            else
            {
                currentInput = "-" + currentInput;
            }
            UpdateDisplay();
        }

        private void Scientific_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            string operation = button.Content.ToString();

            if (double.TryParse(currentInput, out double number))
            {
                double result = 0;
                switch (operation)
                {
                    case "√":
                        result = Math.Sqrt(number);
                        break;
                    case "x²":
                        result = Math.Pow(number, 2);
                        break;
                    case "sin":
                        result = Math.Sin(number);
                        break;
                    case "cos":
                        result = Math.Cos(number);
                        break;
                    case "tan":
                        result = Math.Tan(number);
                        break;
                    case "log":
                        result = Math.Log10(number);
                        break;
                    case "π":
                        result = Math.PI;
                        break;
                    case "e":
                        result = Math.E;
                        break;
                    default:
                        return;
                }

                string expression = $"{operation}({number})";
                currentInput = result.ToString();
                history.Add($"{expression} = {currentInput}");
                UpdateHistory();
            }
        }

        private void Equals_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentOperator))
            {
                MessageBox.Show("Выберите операцию");
                return;
            }

            if (string.IsNullOrEmpty(rightOperand))
            {
                rightOperand = currentInput;
            }

            try
            {
                double left = Convert.ToDouble(leftOperand);
                double right = Convert.ToDouble(rightOperand);

                double result = 0;
                switch (currentOperator)
                {
                    case "+":
                        result = left + right;
                        break;
                    case "-":
                        result = left - right;
                        break;
                    case "×":
                        result = left * right;
                        break;
                    case "÷":
                        if (right == 0)
                        {
                            throw new DivideByZeroException();
                        }
                        result = left / right;
                        break;
                    case "%":
                        result = left % right;
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                string expression = $"{leftOperand} {currentOperator} {rightOperand}";
                currentInput = result.ToString();
                history.Add($"{expression} = {currentInput}");
                UpdateHistory();

                leftOperand = currentInput;
                rightOperand = string.Empty;
                currentOperator = string.Empty;
            }
            catch (DivideByZeroException)
            {
                currentInput = "Ошибка: деление на ноль";
            }
            catch
            {
                currentInput = "Ошибка";
            }
            finally
            {
                UpdateDisplay();
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (currentInput.Length > 1)
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
            }
            else
            {
                currentInput = "0";
            }

            UpdateDisplay();
        }

        private void ClearAll()
        {
            currentInput = "0";
            leftOperand = string.Empty;
            rightOperand = string.Empty;
            currentOperator = string.Empty;
            UpdateDisplay();
        }
    }
}