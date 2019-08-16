using ArdoqFluentModels;
using ArdoqFluentModels.Mapping.ComponentHierarchy;
using ModelMaintainer.Samples.Azure.Model;

namespace ModelMaintainer.Samples.Azure
{
    public class AzureMaintenanceModule
    {
        public void Configure(ArdoqModelMappingBuilder builder)
        {
            // Subscription
            builder.AddComponentMapping<Subscription>("Subscription")
                .WithKey(s => s.Name)
                .WithModelledHierarchyReference(s => s.ResourceGroups, ModelledReferenceDirection.Parent);

            // Resource Group
            builder.AddComponentMapping<ResourceGroup>("Resource group")
                .WithKey(rg => rg.Name)
                .WithTags(rg => rg.Tags)
                .WithModelledHierarchyReference(rg => rg.SqlServers, ModelledReferenceDirection.Parent)
                .WithModelledHierarchyReference(rg => rg.StorageAccounts, ModelledReferenceDirection.Parent);

            // SQL
            builder.AddComponentMapping<SqlServer>("SQL server")
                .WithKey(s => s.Name)
                .WithModelledHierarchyReference(s => s.Databases, ModelledReferenceDirection.Parent);

            builder.AddComponentMapping<SqlDatabase>("SQL database")
                .WithKey(s => s.Name)
                .WithTags(s => s.Tags)
                .WithField(s => s.Uri, "Uri");

            // Storage
            builder.AddComponentMapping<StorageAccount>("Storage account")
                .WithTags(s => s.Tags)
                .WithKey(s => s.Name);
        }
    }
}
