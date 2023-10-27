using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SqlFileGenerator.Classes
{
    public static class DataUpload
    {
        // insert data to datapump warehouse db
        public static void UploadData(string rcConnection, string scriptDirectory)
        {
            int batchSize = 1000;

            // Reset variables
            Console.Clear();
            Console.WriteLine("Uploading data to warehouse");
            try
            {

                using (SqlConnection connection = new SqlConnection(rcConnection))
                {
                    connection.Open();

                    string[] scriptFiles = Directory.GetFiles(scriptDirectory, "*.sql");

                    foreach (string scriptFile in scriptFiles)
                    {

                        string sqlScript = File.ReadAllText(scriptFile);
                        string pattern = @"INSERT\s+INTO\s+\S+\s+VALUES\s*\([\s\S]*?\);";

                        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        MatchCollection matches = regex.Matches(sqlScript);

                        SqlTransaction transaction = connection.BeginTransaction();
                        int count = 0;
                        int index = 0;

                        while (count < matches.Count)
                        {
                            string insertStatement = matches[index].Value;

                            using (SqlCommand command = new SqlCommand(insertStatement, connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }

                            index++;
                            count++;

                            if (count % batchSize == 0)
                            {
                                transaction.Commit();
                                transaction = connection.BeginTransaction();
                            }
                        }
                        Console.WriteLine($"Uploading data for {scriptFile}");
                        transaction.Commit();

                    }
                }

                Console.WriteLine("Data import was completed successfully.");

                Console.ReadLine();
                RunMigration(rcConnection);

            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                Console.ReadLine();

            }
        }

        public static void RunMigration(string connectionString)
        {
            var serviceProvider = CreateServices(connectionString);

            using (var scope = serviceProvider.CreateScope())
            {
                UpdateDatabase(scope.ServiceProvider);
            }

            Console.WriteLine("Migration completed successfully!");

            Console.WriteLine("Migration completed successfully!");
        }
        private static IServiceProvider CreateServices(string connectionString)
        {
            return new ServiceCollection()
               .AddFluentMigratorCore()
               .ConfigureRunner(rb => rb
                   .AddSqlServer()
                   .WithGlobalConnectionString(connectionString)
                   .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
               .BuildServiceProvider(false);
        }

        private static void UpdateDatabase(IServiceProvider serviceProvider)
        {
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }
    }
}
