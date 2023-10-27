namespace SqlFileGenerator.Classes
{
    public static class FileManager
    {
        // create directory if non exists
        public static string CreateDirectory()
        {
            string folderPath = "sqlDataFiles";
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var directoryPath = Path.Combine(baseDirectory, folderPath);
            try
            {             
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Console.WriteLine("Directory created successfully!");
                }
                else
                {
                    Console.WriteLine("Directory already exists!");
                }
                return directoryPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return directoryPath;
        }

        // add is_ibb_account value and set it to false
        public static void UpdateScriptContent(string directoryPath)
        {
            var fileName = "credit_application.sql";
            string sqlFilePath = Path.Combine(directoryPath, fileName);

            string sqlScript = File.ReadAllText(sqlFilePath);
            string substringToReplace = ");";
            string newValue = ",0);";
            string updatedScript = sqlScript.Replace(substringToReplace, newValue);

            File.WriteAllText(sqlFilePath, updatedScript);

            Console.WriteLine("SQL script updated successfully.");
        }


    }
}
