
using Microsoft.Extensions.Configuration;
using SqlFileGenerator.Classes;


IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
// datasource for generating sql files
string dataPumpConnection = configuration.GetSection("DatabaseSettings:DataPumpConnectionString").Value ?? "";
// database on  data warehouse
string rcConnection = configuration.GetSection("DatabaseSettings:DbConnection").Value ?? "";
// database schema
string schema = configuration.GetSection("OtherSettings:schema").Value ?? "";

Console.WriteLine("Data import for datapump");
try
{
   
    var directoryPath = FileManager.CreateDirectory();

    // comment out the line below if you already have sql files in bin\Debug\net6.0\sqlDataFiles
    FileGeneration.GenerateSqlFilesAndUploadData(dataPumpConnection, directoryPath, schema, rcConnection);
     
    // uncomment to upload data to db you have the files sql files in bin\Debug\net6.0\sqlDataFiles
    //DataUpload.UploadData(rcConnection, directoryPath);

    
}
catch (Exception e)
{
    Console.WriteLine("Something went wrong", e.Message);
}
