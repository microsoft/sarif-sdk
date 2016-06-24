# How to accomplish common tasks with the SARIF SDK

## Read a SARIF log file

```C#
using System.IO;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json

// ...

public SarifLog ReadLogFile(string logFilePath)
{
    if (logFilePath == null)
    {
        throw new ArgumentNullException(nameof(logFilePath));
    }
    
    string logContents = File.ReadAllText(logFilePath);

    var settings = new JsonSerializerSettings()
    {
        ContractResolver = SarifContractResolver.Instance
    };

    SarifLog log = JsonConvert.DeserializeObject<SarifLog>(logContents, settings);
    return log;
}

```

## Format a result message

```C#
Result result = ...
IRule rule = ...

// GetMessageText is an extension method on the Result class
string resultMessage = result.GetMessageText(result, rule);

```
