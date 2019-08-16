using System.Collections.Generic;
using ModelMaintainer.Samples.Azure.Model;

namespace ModelMaintainer.Samples.Azure
{
    public class FakeAzureReader : IAzureReader
    {
        public Subscription ReadSubscription()
        {
            Subscription subscription = new Subscription("Test Subscription");
            subscription.ResourceGroups.AddRange(ReadResourceGroups());

            return subscription;
        }

        public List<ResourceGroup> ReadResourceGroups()
        {
            var resourceGroups = new List<ResourceGroup>();

            // Create ResourceGroup
            ResourceGroup resourceGroup = new ResourceGroup("Test ResourceGroup")
            {
                Tags = new List<string> { "Develop" }
            };
            resourceGroups.Add(resourceGroup);

            // Create Sql server
            SqlServer sqlServer = new SqlServer("Test Sql server");
            SqlDatabase sqlDatabase = new SqlDatabase("Test Sql database")
            {
                Tags = new List<string> { "Develop" },
                Uri = "my-sqlserver-dev.database.windows.net"
            };
            sqlServer.Databases.Add(sqlDatabase);
            resourceGroup.SqlServers.Add(sqlServer);

            // Create StorageAccounts
            StorageAccount storageAccount = new StorageAccount("Test storage account")
            {
                Tags = new List<string> { "Develop" }
            };
            resourceGroup.StorageAccounts.Add(storageAccount);

            return resourceGroups;
        }
    }
}
