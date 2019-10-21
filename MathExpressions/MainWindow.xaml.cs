using MathExpressions.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace MathExpressions
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Коллекция формул для отображения
        /// </summary>
        public ObservableCollection<FormulaModel> Formulas { get; } = new ObservableCollection<FormulaModel>();

        /// <summary>
        /// Коллекция для отображения параметров формулы
        /// </summary>
        public ObservableCollection<ParamModel> Params { get; } = new ObservableCollection<ParamModel>();

        

        public MainWindow()
        {
            InitializeComponent();

            UpdateFormulasOnDisplay();

            DataContext = this;

        }

        /// <summary>
        /// Обновляет список формул на экране из БД
        /// </summary>
        private void UpdateFormulasOnDisplay()
        {
            var formulas = SqliteDataAccess.LoadFormulas();

            Formulas.Clear();

            foreach (var formula in formulas)
            {
                Formulas.Add(formula);
            }
        }


        /// <summary>
        /// Срабатывает при выборе формулы
        /// </summary>        
        private void ListOfFormulas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListOfFormulas.SelectedItem is FormulaModel selectedItem)
            {
                // Загружаем параметры формулы из БД
                var _params = SqliteDataAccess.LoadParams(selectedItem.Id);

                Params.Clear();

                // Переносим параметры для отображения в окне
                foreach (var param in _params)
                {
                    Params.Add(param);
                }                
            }          
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (ListOfFormulas.SelectedItem is FormulaModel selectedItem)
            {
                // Загружаем операции для формулы
                var operations = SqliteDataAccess.LoadOperations(selectedItem.Id);

                // Длина формулы
                int lenFormula = Params.Count + operations.Count;

                // Обработать ошибку опустошения стека
                // Обработать ошибку деления на 0

                var stack = new Stack<double>();
                // Вычисляем
                for (int i = 0; i < lenFormula; i++)
                {
                    if (Params.SingleOrDefault(x => x.Position == i) != null)
                    {
                        double value = Convert.ToDouble(Params.SingleOrDefault(x => x.Position == i).Value);
                        stack.Push(value);
                    }
                    else if (operations.SingleOrDefault(x => x.Position == i) != null)
                    {
                        var operation = operations.SingleOrDefault(x => x.Position == i).Operation;
                        
                        double value2 = stack.Pop();
                        double value1 = stack.Pop();

                        if (operation == "+")
                        {
                            stack.Push(value1 + value2);
                        }
                        else if (operation == "-")
                        {
                            stack.Push(value1 - value2);
                        }
                        else if (operation == "*")
                        {
                            stack.Push(value1 * value2);
                        }
                        else if (operation == "/")
                        {
                            if (value2 == 0.0)
                            {
                                throw new DivideByZeroException("Нельзя делить на 0!");
                            }

                            stack.Push(value1 / value2);
                        }                        
                    }
                }

                var result = stack.Pop();
                Result_TextBox.Text = "";
                Result_TextBox.Text = result.ToString();
            }

        }

        private void AddFormulaButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputFormulaDialog();
            if (dialog.ShowDialog() == true)
            {
                string expression = dialog.FormulaText;

                // Проверим реуглярным выражением введенные символы
                bool isValid = Regex.IsMatch(expression, @"^[a-zA-Z\d\s\+\-\/\*\(\)]+$");

                if (!isValid)
                {
                    MessageBox.Show("В формуле обнаружены недопустимые символы!", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверим правильность расстановки скобок
                if (!IsBracketsCorrect(expression))
                {
                    MessageBox.Show("В формуле обнаружены ошибки, проверьте скобки!",
                                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                
                try
                {
                    // Переведем формулу в обратную польскую нотацию
                    var rpn = ConvertInfixToRPN(expression);

                    // Сохраним в базу данных результат
                    SqliteDataAccess.AddFormula(expression, rpn);

                    // Обновим данные на экране
                    UpdateFormulasOnDisplay();
                }
                catch (System.InvalidOperationException)
                {
                    MessageBox.Show("В формуле обнаружены ошибки, проверьте скобки!", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool IsBracketsCorrect(string expression)
        {
            bool isCorrect = true;
            Stack<char> stack = new Stack<char>();

            foreach (var item in expression.ToCharArray())
            {
                if (item == '(')
                {
                    stack.Push(item);
                }
                else if (item == ')')
                {
                    if (stack.Count == 0)
                    {
                        isCorrect = false;
                        break;
                    }                                     
                    
                    stack.Pop();                    
                }
            }

            if (stack.Count > 0)
            {
                isCorrect = false;
            }

            return isCorrect;
        }

        private static string[] ConvertInfixToRPN(string expression)
        {
            //
            // Список доступных операций с весом (приоритетом) операции
            //
            List<OperationWeight> operations = new List<OperationWeight>
            {
            new OperationWeight { Symbol = '*', Weight = 3 },
            new OperationWeight { Symbol = '/', Weight = 3 },
            new OperationWeight { Symbol = '+', Weight = 2 },
            new OperationWeight { Symbol = '-', Weight = 2 },
            new OperationWeight { Symbol = '(', Weight = 1 },
            new OperationWeight { Symbol = ')', Weight = 1 },
            };


            //
            // Объект для хранения результирующей строки
            //            
            var rpnBuilder = new StringBuilder();

            //
            // Стек для хранения операций
            //
            var operationStack = new Stack<OperationWeight>();

            // Удалим пробелы из выражения
            expression = Regex.Replace(expression, @"\s+", "");

            for (int i = 0; i < expression.Length; i++)
            {
                var symbol = expression[i];

                if (symbol == '(')
                {
                    var operation = operations.First(x => x.Symbol == symbol);
                    operationStack.Push(operation);
                }
                else if (symbol == ')')
                {
                    rpnBuilder.Append(";");
                    while (operationStack.Count > 0 && operationStack.Peek().Symbol != '(')
                    {
                        rpnBuilder.Append(operationStack.Pop().Symbol.ToString());
                    }

                    // del '('                    
                    operationStack.Pop();

                }
                else if (operations.Where(x => x.Symbol == symbol).Any())
                {
                    var operation = operations.Where(x => x.Symbol == symbol).First();
                    rpnBuilder.Append(";");
                    while (operationStack.Count > 0 && operationStack.Peek().Weight >= operation.Weight)
                    {
                        rpnBuilder.Append(operationStack.Pop().Symbol.ToString());
                        rpnBuilder.Append(";");
                    }

                    operationStack.Push(operation);
                }
                else
                {
                    rpnBuilder.Append(symbol.ToString());
                }
            }

            while (operationStack.Count > 0)
            {
                rpnBuilder.Append(";");
                rpnBuilder.Append(operationStack.Pop().Symbol.ToString());

            }

            var rpnString = rpnBuilder.ToString();

            //if (rpnString.Contains("("))
            //    throw new System.InvalidOperationException();

            var rpnArray = rpnString.Split(';');
            return rpnArray;
        }
    }
}
