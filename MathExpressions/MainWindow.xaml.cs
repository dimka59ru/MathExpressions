﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MathExpressions
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> Formulas { get; } = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();

            //
            //
            //
            string expressions = "5+((1+2)*4)-3";// 5 + ((1 + 2) * 4) - 3

            Formulas.Add(expressions);
            Formulas.Add("a*b");
            Formulas.Add("(b+c)*d");

            //
            // Список доступных операций с весом (приоритетом) операции
            //
            var operations = new List<OperationWeight>
            {
                new OperationWeight { Symbol = '*', Weight = 3 },
                new OperationWeight { Symbol = '/', Weight = 3 },
                new OperationWeight { Symbol = '+', Weight = 2 },
                new OperationWeight { Symbol = '-', Weight = 2 },
                new OperationWeight { Symbol = '(', Weight = 1 },
                new OperationWeight { Symbol = ')', Weight = 1 },
            };            

            string rpn = ConvertInfixToRPN(expressions, operations);

            //ПРоверить выходную строку на наличие скобок;


            DataContext = this;

        }

        private static string ConvertInfixToRPN(string expressions, List<OperationWeight> operations)
        {
            //
            // Объект для хранения результирующей строки
            //
            var RPNBuilder = new StringBuilder();

            //
            // Стек для хранения операций
            //
            var operationStack = new Stack<OperationWeight>();


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
                    RPNBuilder.Append(";");

                    while (operationStack.Count > 0 && operationStack.Peek().Symbol != '(')
                    {
                        RPNBuilder.Append(operationStack.Pop().Symbol);
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

                    RPNBuilder.Append(";");

                    while (operationStack.Count > 0 && operationStack.Peek().Weight >= operation.Weight)
                    {
                        RPNBuilder.Append(operationStack.Pop().Symbol);
                    }

                    operationStack.Push(operation);
                }
                else
                {
                    RPNBuilder.Append(symbol);
                }
            }

            RPNBuilder.Append(";");

            while (operationStack.Count > 0)
            {
                RPNBuilder.Append(operationStack.Pop().Symbol);
                RPNBuilder.Append(";");
            }

            return RPNBuilder.ToString();
        }
    }
}
