using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathExpressions;
using MathExpressions.Models;
using Xunit;

namespace MathExpressions.Tests
{
    public class FormulaTest
    {
        [Fact]
        public void ConvertInfixToRPN_Test1()
        {
            string[] expected = new string[] { "a", "b", "c", "-", "d", "*", "+"};

            Formula formula = new Formula();

            string[] actual = formula.ConvertInfixToRPN("a + (b - c) * d");


            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ConvertInfixToRPN_Test2()
        {
            string[] expected = new string[] { "b", "c", "/", "d", "x", "y", "-", "*", "+" };

            Formula formula = new Formula();

            string[] actual = formula.ConvertInfixToRPN("b/c + d*(x - y)");


            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Solve_Test()
        {
            double expected = 0.75;

            Formula formula = new Formula();
            formula.Params = new ObservableCollection<ParamModel>
            {
                new ParamModel { Param = "a", Position = 0, Value = 1 },
                new ParamModel { Param = "b", Position = 1, Value = 2 },
                new ParamModel { Param = "c", Position = 2, Value = 3 },
                new ParamModel { Param = "d", Position = 4, Value = 4 }
            };

            formula.Operations = new List<OperationModel>
            {
                 new OperationModel { Operation = "-", Position = 3 },
                 new OperationModel { Operation = "/", Position = 5 },
                 new OperationModel { Operation = "+", Position = 6 },
            };

            double actual = formula.Solve();

            Assert.Equal(expected, actual);
        }


    }
}
