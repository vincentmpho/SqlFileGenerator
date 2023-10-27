using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;

namespace SqlFileGenerator.Classes
{
    public static class FileGeneration
    {
        public static void GenerateSqlFilesAndUploadData (string dataPumpConnection, string directoryPath, string schema, string rcConnectionString)
        {
            int counter = 0;
            using (var connection = new NpgsqlConnection(dataPumpConnection))
            {
                connection.Open();
                // names of tables to generate sql files
                string[] tablesToBackup = { "account_write_off", "person", "address", "applied_voucher_codes", "credit_application", "credit_application_history", "credit_application_meta", "credit_application_terms", "debtor_transaction", "employer", "finance", "person", "marketing", "order_detail", "order_detail_promotions", "order_header", "order_return_skus", "order_returns", "payment_plan", "payment_plan_history", "person", "personal_information", "tv_license_log", "wishlist", "wishlist_skus" };

                foreach (string tableName in tablesToBackup)
                {
                    counter++;
                    var fileName = $"{tableName}.sql";
                    string backupFilePath = Path.Combine(directoryPath, fileName);

                    using (var command = new NpgsqlCommand($"SELECT * FROM {tableName}", connection))
                    {

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                using (StreamWriter writer = new StreamWriter(backupFilePath))
                                {
                                    while (reader.Read())
                                    {
                                        string insertStatement = GenerateInsertStatement(reader, tableName, schema);
                                        writer.WriteLine(insertStatement);
                                    }
                                }

                                Console.WriteLine($"{counter} SQL file for {tableName} created successfully.");
                            }
                            else
                            {
                                Console.WriteLine($"No data found for {tableName}");
                            }
                        }

                    }

                }

                // update creditapplication sql file
                FileManager.UpdateScriptContent(directoryPath);
                Console.WriteLine("Backup created successfully.");

                // upload generated data sql files to  data warehouse database
                if (counter == tablesToBackup.Length)
                {
                    DataUpload.UploadData(rcConnectionString, directoryPath);
                }
            }
        }
        static string GenerateInsertStatement(NpgsqlDataReader reader, string tableName, string schema)
        {
            var tableSchema = reader.GetSchemaTable();
            string columns = string.Join(", ", GetColumnNames(reader));
            string values = string.Join(", ", GetColumnValues(reader));

            return $"INSERT INTO {schema}.{tableName} VALUES ({values});";
        }

        static string[] GetColumnNames(NpgsqlDataReader reader)
        {
            string[] columnNames = new string[reader.FieldCount];

            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames[i] = reader.GetName(i);
            }

            return columnNames;
        }

        static string[] GetColumnValues(NpgsqlDataReader reader)
        {
            string[] columnValues = new string[reader.FieldCount];

            for (int i = 0; i < reader.FieldCount; i++)
            {
                object value = reader.GetValue(i);

                if (value == DBNull.Value)
                {
                    columnValues[i] = "NULL";
                }
                else if (IsNumericType(value))
                {
                    columnValues[i] = value.ToString().Replace(",", ".");
                }
                else
                {
                    columnValues[i] = $"'{EscapeSingleQuotes(value.ToString())}'";
                }
            }

            return columnValues;
        }

        static bool IsNumericType(object value)
        {
            return value is byte || value is short || value is int || value is long ||
                   value is float || value is double || value is decimal;
        }

        static string EscapeSingleQuotes(string input)
        {
            return input.Replace("'", "''");
        }

    }
}
