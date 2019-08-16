using System.IO;
using Autofac;
using Microsoft.Extensions.Configuration;

namespace ModelMaintainer.Samples.Azure
{
    public static class IoC
    {
        private static IContainer _container;
        private static IConfigurationRoot _configuration;

        public static T Resolve<T>()
        {
            InitIfNecessary();

            return _container.Resolve<T>();
        }

        public static IConfigurationRoot GetConfiguration()
        {
            if (_configuration != null)
            {
                return _configuration;
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();

            return _configuration;
        }

        private static void InitIfNecessary()
        {
            if (_container != null)
            {
                return;
            }

            _container = BuildContainer();
        }

        private static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();
            GetConfiguration();
            
            var azureReader = new FakeAzureReader();

            builder.RegisterInstance<IAzureReader>(azureReader);

            builder.Register(c => new AzureSourceModelProvider(c.Resolve<IAzureReader>()))
                .As<ISourceModelProvider>();

            return builder.Build();
        }
    }
}
