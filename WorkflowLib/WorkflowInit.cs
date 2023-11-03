using System;
using System.Xml.Linq;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.DbPersistence;

public static class WorkflowInit
{
    private static readonly Lazy<WorkflowRuntime> LazyRuntime = new Lazy<WorkflowRuntime>(InitWorkflowRuntime);

    public static WorkflowRuntime Runtime
    {
        get { return LazyRuntime.Value; }
    }

    public static string ConnectionString { get; set; }

    private static WorkflowRuntime InitWorkflowRuntime()
    {
        // TODO Uncomment for .NET Framework if you don't set ConnectionString externally.
        //ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        if (string.IsNullOrEmpty(ConnectionString))
        {
            throw new Exception("Please init ConnectionString before calling the Runtime!");
        }
        // TODO If you have a license key, you have to register it here
        //WorkflowRuntime.RegisterLicense("your license key text");

        // TODO If you are using database different from SQL Server you have to use different persistence provider here.
        var dbProvider = new MSSQLProvider(ConnectionString);

        var builder = new WorkflowBuilder<XElement>(
            dbProvider,
            new OptimaJet.Workflow.Core.Parser.XmlWorkflowParser(),
            dbProvider
        ).WithDefaultCache();

        var runtime = new WorkflowRuntime()
            .WithBuilder(builder)
            .WithPersistenceProvider(dbProvider)
            .EnableCodeActions()
            .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn()
            .AsSingleServer();

        var plugin = new OptimaJet.Workflow.Plugins.BasicPlugin();
        // Settings for SendEmail actions
        // plugin.Setting_Mailserver = "smtp.yourserver.com";
        // plugin.Setting_MailserverPort = 25;
        // plugin.Setting_MailserverFrom = "from@yourserver.com";
        // plugin.Setting_MailserverLogin = "login@yourserver.com";
        // plugin.Setting_MailserverPassword = "pass";
        // plugin.Setting_MailserverSsl = true;
        runtime.WithPlugin(plugin);

        // events subscription
        runtime.ProcessActivityChanged += (sender, args) => { };
        runtime.ProcessStatusChanged += (sender, args) => { };
        // TODO If you have planned to use Code Actions functionality that required references to external assemblies
        // you have to register them here
        //runtime.RegisterAssemblyForCodeActions(Assembly.GetAssembly(typeof(SomeTypeFromMyAssembly)));

        // starts the WorkflowRuntime
        // TODO If you have planned use Timers the best way to start WorkflowRuntime is somewhere outside
        // of this function in Global.asax for example
        runtime.Start();

        return runtime;
    }
}