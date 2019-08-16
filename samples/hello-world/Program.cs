using ArdoqFluentModels;
using System.Collections.Generic;


public class Program
{
    public static void Main(string[] args)
    {
        var ardoqUrl = "https://app.ardoq.com/";
        var ardoqToken = "<secret-ardoq-token>";
        var ardoqOrganization = "<ardoq-org>";
        var workspace = "<ardoq-workspace>";
        var folder = "<ardoq-folder>";

        // Create a builder
        var builder =
            new ArdoqModelMappingBuilder(ardoqUrl, ardoqToken, ardoqOrganization)
                .WithWorkspaceNamed(workspace)
                .WithFolderNamed(folder);

        // Add your structured model. This must match the model in Ardoq. 
        builder.AddComponentMapping<MyComponent>("MyComponent")
            .WithKey(s => s.MyKey);

        // Create the source model provider. This supplies the objects which will be documented in Ardoq. 
        ISourceModelProvider sourceModelProvider = new MySourceModelProvider();

        // Build and run
        var session = builder.Build();
        session.Run(sourceModelProvider).Wait();
    }

    public class MySourceModelProvider : ISourceModelProvider
    {
        public IEnumerable<object> GetSourceModel()
        {
            return new List<object> { new MyComponent() };
        }
    }

    public class MyComponent
    {
        public string MyKey { get; } = "Hello World";
    }
}
