using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}