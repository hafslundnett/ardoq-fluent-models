ardoq-modelmaintainer
=====================

Library for declarative mapping of C# object models to Ardoq workspaces.

## Overview
In order to use this library you already have two things: an interconnected C# object model of something you want to document in Ardoq, and a template in Ardoq containing the component types, fields and references that you need in order to define the model in Ardoq.

The point of this library then is to let you state the relationship between the C# model and corresponding Ardoq model in a declarative fashion. The functionality in the library then handles the following mapping tasks automatically:

* Create the folder in Ardoq if it doesn't already exist
* Create the workspace in Ardoq if it doesn't already exist
* Create all components in Ardoq where there exists a C# object but no corresponding component
* Update all components that exist in both the C# model and Ardoq, but where the fields values differ
* Delete all components in Ardoq where there does not exist a corresponding C# object
* Place all new components correctly in the component hierarchy upon creation 
* Maintain references between all components within the workspace
* Maintain references between components and components in other workspaces.

## ISourceModelProvider
This is an interface that you have to implement. It is the provider that a provider that returns a flat list of objects that should be mapped to the Ardoq workspace. Normally, the list represents an interconnected model in that the different objects in the list refer to eachother.

## Defining the mapping rules 
The rules for 
ArdoqModelMappingBuilder

```csharp
var ardoqUrl = "https://app.ardoq.com/";
var ardoqToken = "<secret-ardoq-token>";
var ardoqOrganization = "<ardoq-org>";
var workspace = "<ardoq-workspace>";
var folder = "<ardoq-folder>";

// Create a builder
var builder =
    new ArdoqModelMappingBuilder(config["Ardoq:url"], config["Ardoq:token"], config["Ardoq:organization"])
        .WithWorkspaceNamed(config["Ardoq:workspaceName"])
        .WithFolderNamed(config["Ardoq:folderName"])
        .WithTemplate(config["Ardoq:templateName"]);

// Add your structured model. This must match the model in Ardoq. 
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

// Create the source model provider. This supplies the objects which will be documented in Ardoq. 
ISourceModelProvider sourceModelProvider = new AzureSourceModelProvider();

// Build and run
var session = builder.Build();
session.Run(sourceModelProvider).Wait();
```

This code and more samples can be found under `samples/`. 