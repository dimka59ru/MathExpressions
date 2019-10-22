using Dapper;
using MathExpressions.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathExpressions
{
    public static class SqliteDataAccess
    {
        /// <summary>
        /// Загрузка всех формул из БД
        /// </summary>        
        public static List<Formula> LoadFormulas()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string sql = "SELECT * FROM formulas";
                var formulas = cnn.Query<Formula>(sql, new DynamicParameters()).ToList();

                foreach (var formula in formulas)
                {
                    formula.Params = new ObservableCollection<ParamModel>(LoadParams(formula.Id));
                    formula.Operations = LoadOperations(formula.Id);
                }
                return formulas;
            }
        }

        /// <summary>
        /// Загрузка параметров формулы из БД по id формулы
        /// </summary>  
        public static List<ParamModel> LoadParams(int idFormula)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string sql = "SELECT * FROM params WHERE id_formula = @idFormula";
                var output = cnn.Query<ParamModel>(sql, new { idFormula });
                return output.ToList();
            }
        }

        /// <summary>
        /// Загрузка операций для формулы из БД по id формулы
        /// </summary>
        public static List<OperationModel> LoadOperations(int idFormula)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string sql = "SELECT * FROM operations WHERE id_formula = @idFormula";
                var output = cnn.Query<OperationModel>(sql, new { idFormula });
                return output.ToList();
            }
        }

        /// <summary>
        /// Добавление формулы в базу данных
        /// </summary>        
        public static void AddFormula(Formula formula)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string sql = "INSERT INTO formulas (infix) VALUES (@Infix)";
                var affectedRows = cnn.Execute(sql, new { formula.Infix });

                sql = "SELECT MAX(id) FROM formulas";
                int lastId = cnn.QuerySingle<int>(sql);

                var rpn = formula.RPN;

                for (int i = 0; i < rpn.Length; i++)
                {
                    // Если символ - это операция (+, -, * и т.д.)
                    if (Formula.AvailableOperations.Where(x => x.Symbol.ToString() == rpn[i]).Any())
                    {
                        sql = @"INSERT INTO operations (id_formula, operation, position) 
                                    VALUES (@id_formula, @value, @position)";                        
                    }
                    else
                    {
                        sql = @"INSERT INTO params (id_formula, param, position) 
                                    VALUES (@id_formula, @value, @position)";                        
                    }

                    cnn.Execute(sql, new { id_formula = lastId, value = rpn[i], position = i });
                }
                
            }
        }

        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}
