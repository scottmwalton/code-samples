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
