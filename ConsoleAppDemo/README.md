# Console App Demo
I wanted a sample console app with the basics of logging and configuration added. Here are the steps to set this up.

## 1. Create the Project
use eithe the command prompt or Visual Studio to create a new console application.
```
dotnet new console -o ConsoleAppDemo
```

## 2. Install the Nuget Packages
```Powershell
Install-Package Serilog
Install-Package Serilog.Settings.Configuration
Install-Package Serilog.Sinks.File
Install-Package Serilog.Sinks.Console
Install-Package Microsoft.Extensions.Configuration.Json
```
## 3. Add the configuration class
Add a new class named ```ConsoleAppDemoCOnfig.cs``` to the project. Change the contents to the code below.
```c#
	internal class ConsoleAppDemoConfig
	{
		public string MyStringValue { get; set; } = string.Empty;
		public int MyIntValue { get; set; }
		public bool MyBoolValue { get; set; }
	}
```

## 4. Add the configuration file
Add a new item to the project. Select "JSON File" as the item type and name it ```appsettings.json```.  Change the contents to the code below.
```json
{
	"Serilog": {
		"Using": [ "Serilog.Sinks.File", "Serilog.Sinks.Console" ],
		"MinimumLevel": "Verbose",
		"WriteTo": [
			{
				"Name": "File",
				"Args": { "path": "D:/Temp/Logs/ConsoleAppDemo-.txt" }
			},
			{ "Name": "Console" }
		],
		"Properties": {
			"Application": "ConsoleAppDemo"
		}
	},
	"ConsoleAppDemoConfig": {
		"MyStringValue": "Hello, World!",
		"MyIntValue": 5,
		"MyBoolValue": true
	}
}
```
Be sure to set the "Copy to Output Directory" property to "Copy if newer" for this file so that it ends up in the output folder.

## 5. Put it all together!
Now we will add code to the Program.cs file to accomplish what we want.
Setup the configuration to use our ```appsettings.json``` file.
```c#
// Setup Configuration
var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();
```

Read in our application specific config settings
```c#
// Get our application specific configuration
var section = configuration.GetSection(nameof(ConsoleAppDemoConfig));
var config = section.Get<ConsoleAppDemoConfig>();
```

Setup our logger and use the config settings in ```appsettings.json``` file.
```c#
// Setup our logger
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
```

Use our configuration settings to accomplish a task
```c#
// Apply our config settings and output to verify
for(int i = 0; i < config.MyIntValue; i++)
{
  if(config.MyBoolValue == true)
	{
    logger.Information($"Loop {i}: {config.MyStringValue}");
	}
}
```

And here's the whole Program.cs file...
```c#
using ConsoleAppDemo;
using Microsoft.Extensions.Configuration;
using Serilog;

// Setup Configuration
var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();

// Get our application specific configuration
var section = configuration.GetSection(nameof(ConsoleAppDemoConfig));
var config = section.Get<ConsoleAppDemoConfig>();

// Setup our logger
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

// Notify that we are starting
logger.Information("Console App Demo has begun...");

// Get timing start point
DateTime startDate = DateTime.Now;

// Apply our config settings and output to verify
for(int i = 0; i < config.MyIntValue; i++)
{
  if(config.MyBoolValue == true)
	{
    logger.Information($"Loop {i}: {config.MyStringValue}");
	}
}

// Figure out how long it took
TimeSpan ts = DateTime.Now - startDate;

// Output our results
logger.Information($"Console App Demo completed in {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}");
```
