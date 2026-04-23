using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace PublishingHouseApp
{
    public static class DatabaseHelper
    {
        private static readonly string ConnectionString =
            ConfigurationManager.ConnectionStrings["PublishingDB"].ConnectionString;

        // ── Таблицы и их первичные ключи (для ручного управления ID) ─────────
        private static readonly Dictionary<string, string> TablePrimaryKeys =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Author",           "author_id"       },
                { "Contract",         "contract_id"     },
                { "Publication",      "publication_id"  },
                { "PreparationStage", "stage_id"        },
                { "Expertise",        "expertise_id"    },
                { "PrintRun",         "print_run_id"    },
                { "Format",           "format_id"       },
                { "Subject",          "subject_id"      },
                { "Type",             "type_id"         },
                { "Class",            "class_id"        },
            };

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        // ══════════════════════════════════════════════════════════════════════
        // SELECT
        // ══════════════════════════════════════════════════════════════════════
        public static DataTable ExecuteQuery(string sql, SqlParameter[] parameters = null)
        {
            var dt = new DataTable();
            try
            {
                using (var conn = GetConnection())
                using (var cmd  = new SqlCommand(sql, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    conn.Open();
                    using (var adapter = new SqlDataAdapter(cmd))
                        adapter.Fill(dt);
                }
            }
            catch (SqlException ex)
            {
                UIHelper.ShowError($"Ошибка при выполнении запроса:\n{ex.Message}");
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Неожиданная ошибка:\n{ex.Message}");
            }
            return dt;
        }

        // ══════════════════════════════════════════════════════════════════════
        // INSERT / UPDATE / DELETE  — с автоматическим подбором ID
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Выполняет INSERT/UPDATE/DELETE.
        /// При ошибке нарушения IDENTITY / отсутствия автоинкремента
        /// автоматически подбирает следующий свободный ID и повторяет запрос.
        /// Возвращает количество затронутых строк, или -1 при ошибке.
        /// </summary>
        public static int ExecuteNonQuery(string sql, SqlParameter[] parameters = null,
                                           string tableName = null)
        {
            try
            {
                return RunNonQuery(sql, parameters);
            }
            catch (SqlException ex) when (IsIdentityError(ex))
            {
                // Ошибка автоинкремента — пробуем подобрать ID вручную
                if (tableName != null && IsInsertStatement(sql))
                {
                    return RetryInsertWithManualId(sql, parameters, tableName);
                }
                UIHelper.ShowError($"Ошибка вставки записи:\n{ex.Message}");
                return -1;
            }
            catch (SqlException ex) when (ex.Number == 547) // FK violation
            {
                UIHelper.ShowError("Невозможно выполнить операцию: существуют связанные записи.\n" +
                                   "Сначала удалите зависимые данные.");
                return -1;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // unique
            {
                UIHelper.ShowError("Запись с такими данными уже существует.");
                return -1;
            }
            catch (SqlException ex)
            {
                UIHelper.ShowError($"Ошибка базы данных:\n{ex.Message}");
                return -1;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Неожиданная ошибка:\n{ex.Message}");
                return -1;
            }
        }

        public static object ExecuteScalar(string sql, SqlParameter[] parameters = null)
        {
            try
            {
                using (var conn = GetConnection())
                using (var cmd  = new SqlCommand(sql, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
            catch (SqlException ex)
            {
                UIHelper.ShowError($"Ошибка при выполнении запроса:\n{ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Неожиданная ошибка:\n{ex.Message}");
                return null;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // SMART INSERT — подбирает следующий свободный ID
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Универсальный INSERT с автоматическим определением следующего ID.
        /// Используй вместо ExecuteNonQuery для INSERT-запросов.
        /// </summary>
        public static int SmartInsert(string tableName, string sql, SqlParameter[] parameters)
        {
            // Сначала пробуем как есть (если IDENTITY настроен)
            try
            {
                return RunNonQuery(sql, parameters);
            }
            catch (SqlException ex) when (IsIdentityError(ex))
            {
                return RetryInsertWithManualId(sql, parameters, tableName);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                UIHelper.ShowError("Невозможно добавить запись: нарушение связи с другой таблицей.");
                return -1;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                UIHelper.ShowError("Такая запись уже существует.");
                return -1;
            }
            catch (SqlException ex)
            {
                UIHelper.ShowError($"Ошибка базы данных:\n{ex.Message}");
                return -1;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Неожиданная ошибка:\n{ex.Message}");
                return -1;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ══════════════════════════════════════════════════════════════════════

        private static int RunNonQuery(string sql, SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd  = new SqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        private static int RetryInsertWithManualId(string sql, SqlParameter[] parameters,
                                                     string tableName)
        {
            try
            {
                if (!TablePrimaryKeys.TryGetValue(tableName, out string pkCol))
                {
                    UIHelper.ShowError($"Не удалось определить первичный ключ таблицы {tableName}.");
                    return -1;
                }

                // Получаем следующий свободный ID
                int nextId = GetNextId(tableName, pkCol);

                // Добавляем параметр ID в запрос
                string modifiedSql = InjectIdIntoSql(sql, tableName, pkCol, nextId);

                var newParams = new SqlParameter[
                    (parameters?.Length ?? 0) + 1];
                parameters?.CopyTo(newParams, 0);
                newParams[newParams.Length - 1] = new SqlParameter($"@{pkCol}", nextId);

                return RunNonQuery(modifiedSql, newParams);
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Ошибка при повторной вставке с ручным ID:\n{ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Возвращает MAX(pk) + 1 для таблицы, или 1 если таблица пустая
        /// </summary>
        public static int GetNextId(string tableName, string pkColumn)
        {
            try
            {
                var result = ExecuteScalar(
                    $"SELECT ISNULL(MAX({pkColumn}), 0) + 1 FROM {tableName}");
                return result != null ? Convert.ToInt32(result) : 1;
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// Модифицирует INSERT-запрос, добавляя в него колонку ID и параметр
        /// Пример: INSERT INTO Author (surname,...) VALUES (@s,...)
        ///      → INSERT INTO Author (author_id,surname,...) VALUES (@author_id,@s,...)
        /// </summary>
        private static string InjectIdIntoSql(string sql, string tableName,
                                               string pkCol, int id)
        {
            // Ищем паттерн: INSERT INTO TableName (cols) VALUES (vals)
            int colsStart = sql.IndexOf('(');
            int colsEnd   = sql.IndexOf(')');
            int valsStart = sql.LastIndexOf('(');
            int valsEnd   = sql.LastIndexOf(')');

            if (colsStart < 0 || valsStart <= colsEnd)
                return sql; // не можем распарсить — возвращаем как есть

            string colsPart = sql.Substring(colsStart + 1, colsEnd - colsStart - 1).Trim();
            string valsPart = sql.Substring(valsStart + 1, valsEnd - valsStart - 1).Trim();

            string newCols = $"{pkCol},{colsPart}";
            string newVals = $"@{pkCol},{valsPart}";

            return sql.Substring(0, colsStart) +
                   $"({newCols}) VALUES ({newVals})";
        }

        private static bool IsIdentityError(SqlException ex)
        {
            // 544 = Cannot insert explicit value for identity column
            // 8101 = An explicit value for the identity column can only be specified...
            // 515 = Cannot insert NULL into column (когда нет IDENTITY и не передан ID)
            return ex.Number == 544 || ex.Number == 8101 || ex.Number == 515;
        }

        private static bool IsInsertStatement(string sql)
        {
            return sql.TrimStart().StartsWith("INSERT", StringComparison.OrdinalIgnoreCase);
        }

        // ══════════════════════════════════════════════════════════════════════
        // CONNECTION TEST
        // ══════════════════════════════════════════════════════════════════════
        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
