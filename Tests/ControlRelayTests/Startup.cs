﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Diagnostics;
using System.IO;

[TestClass]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "<Pending>")]
public class SetupAssemblyInitializer
{
    public static void AssemblyInit(TestContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        //Temporarily disable logging on UnitTests due to RaspberryPi crash
        //Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        //var config = new LoggingConfiguration();
        //var logfile = new FileTarget("logfile")
        //{
        //    FileName = Path.Combine(localFolder.Path, "log.txt"),
        //    Layout = "${longdate}|${level:uppercase=true}|${callsite:className=True:fileName=False:includeSourcePath=False:methodName=True}|${message} ${exception:format=tostring}"
        //};
        //config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
        //LogManager.Configuration = config;
    }
}