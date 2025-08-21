using System.Text;

namespace ZenerpSMS.Utils
{
    public class LogWriter
    {
        private readonly string _logDirectory;
        private readonly bool _writeLogs;
        public static string Service_Name = "";
        public LogWriter(IConfiguration configuration)
        {
            // Read settings from configuration
            _logDirectory = configuration["LoggingConfig:LocalLogsPath"];
            _writeLogs = configuration["WRITE_LOCAL_LOGS:value"]?.ToUpper() == "TRUE";

            if (!string.IsNullOrEmpty(_logDirectory) && _writeLogs)
            {
                // Ensure the base directory exists
                Directory.CreateDirectory(_logDirectory);
            }
        }

        // Accepts a StringBuilder instead of individual parameters
        public void LogRequestResponse(StringBuilder logBuilder)
        {
            if (!_writeLogs || logBuilder == null) return;

            try
            {
                // Get the current date parts (Year, Month, Day)
                var year = DateTime.Now.Year;
                var month = DateTime.Now.ToString("MMMM"); 
                var day = DateTime.Now.Day;

                string logDirectory = Path.Combine(_logDirectory,  year.ToString(), month, day.ToString());

               
                Directory.CreateDirectory(logDirectory);

           
                string fileName = Service_Name+"Log.txt";
                string filePath = Path.Combine(logDirectory, fileName);

               
                File.WriteAllText(filePath, logBuilder.ToString());
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Failed to write log: {ex.Message}");
            }
        }
    }
}
