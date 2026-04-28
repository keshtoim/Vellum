using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace PublishingHouseApp
{
    // Статический класс для работы с базой данных.
    // Все запросы к БД проходят через этот класс — единая точка доступа к данным.
    // Использует параметризованные запросы для защиты от SQL-инъекций.
    public static class DatabaseHelper
    {
        // Строка подключения из App.config
        private static readonly string ConnectionString =
            ConfigurationManager.ConnectionStrings["PublishingDB"].ConnectionString;

        // ── Таблицы и их первичные ключи ──────────────────────────────────────
        // Используется в SmartInsert для автоматического подбора ID
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

        // Создаёт новое соединение с БД (не открывает — только создаёт объект)
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        // ── SELECT ────────────────────────────────────────────────────────────
        // Выполняет SELECT-запрос и возвращает результат в виде DataTable.
        // При ошибке показывает сообщение пользователю и возвращает пустую таблицу.
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

        // ── INSERT / UPDATE / DELETE ───────────────────────────────────────────
        // Выполняет модифицирующий запрос с обработкой типовых ошибок БД.
        // Возвращает количество затронутых строк или -1 при ошибке.
        public static int ExecuteNonQuery(string sql, SqlParameter[] parameters = null,
                                           string tableName = null)
        {
            try
            {
                return RunNonQuery(sql, parameters);
            }
            catch (SqlException ex) when (IsIdentityError(ex))
            {
                // Ошибка автоинкремента — пробуем вставить с ручным ID
                if (tableName != null && IsInsertStatement(sql))
                    return RetryInsertWithManualId(sql, parameters, tableName);

                UIHelper.ShowError($"Ошибка вставки записи:\n{ex.Message}");
                return -1;
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                // Нарушение внешнего ключа — нельзя удалить/изменить запись со связями
                UIHelper.ShowError("Невозможно выполнить операцию: существуют связанные записи.\n" +
                                   "Сначала удалите зависимые данные.");
                return -1;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // Нарушение уникальности — такая запись уже существует
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

        // Выполняет скалярный запрос (например COUNT или MAX) и возвращает одно значение
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

        // ── SMART INSERT ──────────────────────────────────────────────────────
        // Умная вставка: сначала пробует обычный INSERT.
        // Если в таблице не настроен автоинкремент (IDENTITY) — автоматически
        // определяет следующий свободный ID через MAX(pk)+1 и повторяет запрос.
        public static int SmartInsert(string tableName, string sql, SqlParameter[] parameters)
        {
            try
            {
                return RunNonQuery(sql, parameters);
            }
            catch (SqlException ex) when (IsIdentityError(ex))
            {
                // Автоинкремент не настроен — подбираем ID вручную
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

        // ── ПРОВЕРКА СОЕДИНЕНИЯ ───────────────────────────────────────────────
        // Вызывается при старте приложения — предупреждает если БД недоступна
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

        // Возвращает MAX(pkColumn)+1 для таблицы — следующий свободный ID
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

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        // Базовое выполнение команды без дополнительной обработки ошибок
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

        // Повтор INSERT с явным ID: модифицирует SQL добавляя колонку pk и параметр
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

                int nextId = GetNextId(tableName, pkCol);

                // Вставляем имя колонки pk и параметр @pk в SQL-запрос
                string modifiedSql = InjectIdIntoSql(sql, tableName, pkCol, nextId);

                var newParams = new SqlParameter[(parameters?.Length ?? 0) + 1];
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

        // Добавляет колонку pk и параметр @pk в существующий INSERT-запрос
        private static string InjectIdIntoSql(string sql, string tableName,
                                               string pkCol, int id)
        {
            int colsStart = sql.IndexOf('(');
            int colsEnd   = sql.IndexOf(')');
            int valsStart = sql.LastIndexOf('(');
            int valsEnd   = sql.LastIndexOf(')');

            if (colsStart < 0 || valsStart <= colsEnd)
                return sql;

            string colsPart = sql.Substring(colsStart + 1, colsEnd - colsStart - 1).Trim();
            string valsPart = sql.Substring(valsStart + 1, valsEnd - valsStart - 1).Trim();

            return sql.Substring(0, colsStart) +
                   $"({pkCol},{colsPart}) VALUES (@{pkCol},{valsPart})";
        }

        // Коды ошибок SQL Server связанных с автоинкрементом:
        // 544 — нельзя вставить явное значение в IDENTITY-колонку
        // 515 — нельзя вставить NULL (когда нет IDENTITY и ID не передан)
        private static bool IsIdentityError(SqlException ex)
        {
            return ex.Number == 544 || ex.Number == 8101 || ex.Number == 515;
        }

        private static bool IsInsertStatement(string sql)
        {
            return sql.TrimStart().StartsWith("INSERT", StringComparison.OrdinalIgnoreCase);
        }
    }
}
