using ArdoqFluentModels;
using System.Threading.Tasks;

namespace ModelMaintainer.Samples.Azure
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = IoC.GetConfiguration();
            var provider = IoC.Resolve<ISourceModelProvider>();

            var builder =
                new ArdoqModelMappingBuilder(config["Ardoq:url"], config["Ardoq:token"], config["Ardoq:organization"])
                    .WithWorkspaceNamed(config["Ardoq:workspaceName"])
                    .WithFolderNamed(config["Ardoq:folderName"])
                    .WithTemplate(config["Ardoq:templateName"]);

            var module = new AzureMaintenanceModule();
            module.Configure(builder);

            var session = builder.Build();
            session.Run(provider).Wait();

            Task.Delay(1000).Wait();
        }
    }
}
