using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MathExpressions.Models
{
    public class Formula
    {
        public int Id { get; set; }

        /// <summary>
        /// Обычное представление формулы
        /// </summary>
        public string Infix { get; }
        /// <summary>
        /// Пердставление формулы в обратной польской нотации
        /// </summary>
        public string[] RPN { get; }

        public ObservableCollection<ParamModel> Params { get; set; }

        public List<OperationModel> Operations { get; set; }

        public Formula(string expression)
        {
            Infix = expression;
            RPN = ConvertInfixToRPN(expression);
        }

        public Formula()
        {

        }

        /// <summary>
        /// Вычисляем формулу
        /// </summary>        
        public double Solve()
        {
            // Длина формулы
            int lenFormula = Params.Count + Operations.Count;

            var stack = new Stack<double>();
            // Вычисляем
            for (int i = 0; i < lenFormula; i++)
            {
                if (Params.SingleOrDefault(x => x.Position == i) != null)
                {
                    double value = Convert.ToDouble(Params.SingleOrDefault(x => x.Position == i).Value);
                    stack.Push(value);
                }
                else if (Operations.SingleOrDefault(x => x.Position == i) != null)
                {
                    var operation = Operations.SingleOrDefault(x => x.Position == i).Operation;

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
            
            return stack.Pop();
        }

        //
        // Список доступных операций с весом (приоритетом) операции
        //
        public static readonly List<OperationWeight> AvailableOperations = new List<OperationWeight>
        {
            new OperationWeight { Symbol = '*', Weight = 3 },
            new OperationWeight { Symbol = '/', Weight = 3 },
            new OperationWeight { Symbol = '+', Weight = 2 },
            new OperationWeight { Symbol = '-', Weight = 2 },
            new OperationWeight { Symbol = '(', Weight = 1 },
            new OperationWeight { Symbol = ')', Weight = 1 },
        };

        /// <summary>
        /// Перевод обычного вида формулы в обратную польскую нотацию
        /// </summary>        
        private string[] ConvertInfixToRPN(string expression)
        {
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
                    var operation = AvailableOperations.First(x => x.Symbol == symbol);
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
                else if (AvailableOperations.Where(x => x.Symbol == symbol).Any())
                {
                    var operation = AvailableOperations.Where(x => x.Symbol == symbol).First();
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

            var rpnArray = rpnString.Split(';');
            return rpnArray;
        }
    }
}
