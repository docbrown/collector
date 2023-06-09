using Microsoft.Extensions.Configuration;
using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace LabelPrinter;

internal static class Program
{
    public static Icon ExecutableIcon = SystemIcons.Application;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets(typeof(Program).Assembly)
            .Build();

        UspsWebTools.UserId = config["UspsWebToolsUserId"];

        ExecutableIcon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location) ?? SystemIcons.Application;
        ApplicationConfiguration.Initialize();
        Application.Run(new LabelForm());
    }
}