using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Diagnostics;
using System.IO;

[TestClass]
public class SetupAssemblyInitializer
{
    [AssemblyInitialize]
    public static void AssemblyInit(TestContext context)
    {
        Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        var config = new LoggingConfiguration();
        var logfile = new FileTarget("logfile")
        {
            FileName = Path.Combine(localFolder.Path, "log.txt"),
            Layout = "${longdate}|${level:uppercase=true}|${callsite:className=True:fileName=False:includeSourcePath=False:methodName=True}|${message} ${exception:format=tostring}"
        };
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
        LogManager.Configuration = config;
    }
}