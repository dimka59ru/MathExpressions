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


        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}
