using Dapper;
using MathExpressions.Models;
using System;
using System.Collections.Generic;
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
        public static List<FormulaModel> LoadFormulas()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string sql = "SELECT * FROM formulas";
                var output = cnn.Query<FormulaModel>(sql, new DynamicParameters());
                return output.ToList();
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
        public static void AddFormula(string expression, string[] rpn)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string sql = "INSERT INTO formulas (infix) VALUES (@expression)";
                var affectedRows = cnn.Execute(sql, new { expression });

                sql = "SELECT MAX(id) FROM formulas";
                int lastId = cnn.QuerySingle<int>(sql);

                for (int i = 0; i < rpn.Length; i++)
                {
                    if (rpn[i] == "+" || rpn[i] == "-" || rpn[i] == "*" || rpn[i] == "/")
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
