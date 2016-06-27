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

## Add a property to an object's "property bag".

```C#
// You can do this for any object that has a property bag (that is,
// for any instance of a class derived from PropertyBagHolder), such
// as Run, Result, Location, Rule, etc.
Result result = ... ;

// Add a string-valued property:
result.SetProperty("category", "security");

// Add an integer-valued property:
result.SetProperty("occurrences", 42);

// Add a property of arbitrary type:
MyClass myObject = new MyClass(54, "stuff", "otherStuff");
result.SetProperty("myclass", myObject);

// Add a property with a null value (but then you have to specify
// the type:
result.SetProperty<string>("category", null);
```

## Retrieve a property from an object's "property bag"

```C#
// You can do this for any object that has a property bag (that is,
// for any instance of a class derived from PropertyBagHolder), such
// as Run, Result, Location, Rule, etc.
Result result = ... ;

// Retrieve a string-valued property:
string category = result.GetProperty("category");

// Retrieve an integer-valued property:
int occurrences = result.GetProperty<int>("occurrences");

// Retrieve a property of arbitrary type:
MyClass myObject = result.GetProperty<MyClass>("myclass", myObject);

// WRONG: Don't use the generic version to retrieve a string-valued property:
// string category = result.GetProperty<string>("category");
```
