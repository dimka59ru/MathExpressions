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

            //
            //
            //
            string expressions = "a + ( b - c ) * d";// 5 + ((1 + 2) * 4) - 3            

            var formulas = SqliteDataAccess.LoadFormulas();

            foreach (var formula in formulas)
            {
                Formulas.Add(formula);
            }

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


            string rpn = ConvertInfixToRPN(expressions, operations);

            //Проверить выходную строку на наличие скобок;


            DataContext = this;

        }

        private static string ConvertInfixToRPN(string expressions, List<OperationWeight> operations)
        {

            //
            // Объект для хранения результирующей строки
            //            
            var rpn = new List<string>();            

            //
            // Стек для хранения операций
            //
            var operationStack = new Stack<OperationWeight>();

            // Удалим пробелы из выражения
            expressions = Regex.Replace(expressions, @"\s+", "");

            for (int i = 0; i < expressions.Length; i++)
            {
                var symbol = expressions[i];

                if (symbol == '(')
                {
                    var operation = operations.First(x => x.Symbol == symbol);
                    operationStack.Push(operation);
                }
                else if (symbol == ')')
                {
                    while (operationStack.Count > 0 && operationStack.Peek().Symbol != '(')
                    {                        
                        rpn.Add(operationStack.Pop().Symbol.ToString());
                    }

                    // del '('
                    try
                    {
                        operationStack.Pop();
                    }
                    catch (System.InvalidOperationException)
                    {
                        MessageBox.Show("Обнаружена лишняя закрывающая скобка");
                    }
                }
                else if (operations.Where(x => x.Symbol == symbol).Any())
                {
                    var operation = operations.Where(x => x.Symbol == symbol).First();                    

                    while (operationStack.Count > 0 && operationStack.Peek().Weight >= operation.Weight)
                    {                        
                        rpn.Add(operationStack.Pop().Symbol.ToString());
                    }

                    operationStack.Push(operation);
                }
                else
                {                    
                    rpn.Add(symbol.ToString());
                }
            }
            

            while (operationStack.Count > 0)
            {                
                rpn.Add(operationStack.Pop().Symbol.ToString());                
            }

            

            return "";
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
    }
}
