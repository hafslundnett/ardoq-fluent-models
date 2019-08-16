To use ModelMaintainer do the following steps:
================================================

* 	Sign up a free trial at https://app.ardoq.com/ if you don't have one.
*	Log into Ardoq
* 	Go to: Your account settings -> API AND TOKENS. Get "Current organization key" and a token
*	Create a new folder.
*	Create a new custom workspace template by following these steps:
	*	Create a new workspace. Choose "Blank Workspace" under OTHER.
	*	Create your own structure. For Microsoft Azure will the structure look like something in "AzureArdoqStructure.jpg".
		```	Subscription
			*Resource group
			**SQL Server
			***SQL Database
			**Storage Account
	*	This hierarchy structure is defined in AzureMaintenanceModule.cs file.
	*	In your new Blank workspace. Go to Workspace --> Create template from workspace model. 
*	Add a appsettings.Development.json to the project with this content:
	```
	{
		"Ardoq": {
			"url": "https://app.ardoq.com/",
			"token": "<Your Ardoq token>",
			"organization": "<Your Current organization key",
			"folderName": "<Ardoq Folder>",
			"workspaceName": "<Ardoq workspace>",
			"templateName": "<Ardoq template name>"
		}
	}
	```
