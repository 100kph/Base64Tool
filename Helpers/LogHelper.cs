namespace Base64Tool
{
    using log4net;
    using log4net.Config;
    using System;
    using System.IO;
    using System.Reflection;
    class LogHelper
    {
        public static void Setup()
        {
            string asmPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string logsDirectory = Path.Combine(Path.GetDirectoryName(asmPath), "logs");

            Directory.CreateDirectory(logsDirectory);
            DateTime now = DateTime.Now;
            string logfileName = String.Format($"base64-{now.Year:00}{now.Month:00}{now.Date:00}-{now.Hour:00}{now.Minute:00}{now.Second:00}.log");
            string logFilePath = Path.Combine(logsDirectory, logfileName);

            GlobalContext.Properties["LogFilePath"] = logFilePath;

            
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());

            string configPath = Path.Combine(asmPath, "log4net.config");
            XmlConfigurator.Configure(logRepository, new FileInfo(configPath));
        }
    }
}
