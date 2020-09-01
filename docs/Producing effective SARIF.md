# Rules and guidelines for producing effective SARIF

## Introduction

This document is for creators of static analysis tools who want to produce SARIF output that's useful to SARIF consumers, both human and automated.

Teams can use SARIF log files in many ways. They can view the results in an IDE extension such as the [SARIF extension for VS Code](https://marketplace.visualstudio.com/items?itemName=MS-SarifVSCode.sarif-viewer) or the [SARIF Viewer VSIX for Visual Studio](https://marketplace.visualstudio.com/items?itemName=WDGIS.MicrosoftSarifViewer), or in a [web-based viewer](https://microsoft.github.io/sarif-web-component/). They can import it into a static analysis results database, or use it to drive automatic bug fiing. Most important, developers use the information in a SARIF log file to understand and fix the problems it reports.

Because of this variety of usage scenarios, a SARIF log file that is useful in one scenario might not be useful in another. Ideally, static analysis tools will provide options to let their users specify the output that meets their needs.

The [SARIF specification](https://docs.oasis-open.org/sarif/sarif/v2.1.0/sarif-v2.1.0.html) defines dozens of objects with hundreds of properties. It can be hard to decide which ones are important (aside from the few that the spec says are mandatory). What information is most helpful to developers? What information should you include if you want to file useful bugs from the SARIF log?

On top of all that, the spec is written in format language that's hard to read. If you want to learn SARIF, take a look at the [SARIF Tutorials](https://github.com/microsoft/sarif-tutorials).

The purpose of this document is to cut through the confusion and provide clear guidance on what information your tool should include in a SARIF file, and how to make that information as helpful and usable as possible.

## Principles for producing effective SARIF

This document contains dozens of individual rules and guidelines for producing effective SARIF, but they all derive from a handful of bedrock principles.

### Readability/Understandability/Actionability

The most important elements of a SARIF log file are the "result messages". A result messages must be readable, understandable, and actionable. It must describe exactly what went wrong, why it's a problem, and how to fix it -- and it must do all of that in one compact, well-written plain-text paragraph. (You can supply Markdown messages as well, but the plain-text message is required because not every SARIF consumer can interpret Markdown.)

### Fitness for purpose

The SARIF format has many optional properties. Some of them depend on what kind of analysis tool you are writing. For example, a Web analyzer will probably emit `run.webRequests` and `.webResponses`; a crash dump analyzer might emit `run.addresses`.

Other optional properties are more or less useful depending how end users or downstream systems plan to use the logs. For example:
- If you plan to use SARIF log files as the input to an automated bug filing system, you'll want to populate `result.partialFingerprints` to make it easier to determine which results are new in each run.
- If you plan to view the results in an environment where you don't have access to the source code (for example, in a Web-based SARIF viewer), you'll want to populate `physicalLocation.contextRegion` so that the viewer can display a few lines of code around the actual location of the result. You might even want to populate `run.artifacts[].contents`, which contains the entire contents of the artifact that was analyzed.
- If you plan to use SARIF files as an input to a compliance system, you might want to populate `run.tool.driver.rules` with the complete set of rules that were run, even if most of them didn't produce any results. Similarly, you might want to populate `run.artifacts` with the complete list of files that were analyzed, even if most of them didn't contain any results.

### Compactness

SARIF files from large projects can be huge: multiple gigabytes in size, with over a million results. Even though a great deal of work has been and is being done to compress SARIF files and make them faster to access, it's still important not to unnecessarily increase log file size.

Some optional SARIF properties can take up alot of space, most notably `artifact.contents`.

In some cases, SARIF can represent the same information in multiple places in the log file. For example, a `result` object can (and usually does) specify the result's location with a URI, but that same URI appears in the `run.artifacts` array. Deciding which duplicative information to include is a trade-off between file size on the one hand and what we might call "local readability" on the other.

In short, both "fitness for purpose" and "compactness" are important values, they are in tension with each other, and so it's important for analysis tools to provide flexibility in which properties they emit.

Having said that, the SARIF MultiTool can "enrich" SARIF files with additional properties after the fact (especially if the MultiTool has access to the source code). So one possible strategy is for a tool to emit a minimal SARIF file, and rely on consumers to enrich it as necessary to address specific usage scenarios.

### Serviceability

SARIF files are often used in scenarios where it's important to know which tool, and which _version_ of the tool, produced the results. Therefore it's important for your tool to populate `run.tool.driver` with enough information to identify your tool and its version.

### What's next

The remainder of this document will present a set of specific rules and guidelines, all of them aimed at producing SARIF that conforms to these principles.

## Structural requirements

Many of SARIF's structural requirements are expressed in a [JSON schema](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/schemas/sarif-schema-2.1.0.json), but the schema can't express all the structural requirements. In addition to providing helpful, useful information, it's important for tools to produce SARIF that meet all the structural requirements, even the ones that the schema can't express.

## The SARIF validation tool

The SARIF validation tool (the "validator") helps ensure that a SARIF file conforms to SARIF's structural requirements as well as to the guidelines for producing effective SARIF output.

### What the validator does

Here's how the validator process a SARIF log file:

1. If the file is not valid JSON, report the syntax error and stop.
2. If the file does not conform to the SARIF schema, report the schema violations and stop.
3. Run a set of analysis rules. Report any rule violations.

The analysis rules in Step 3 fall into two categories:

1. Rules that detect structural problems that the schema can't express.
2. Rules that enforce guidelines for producing effective SARIF.

The validator is incomplete: it does not enforce every structural condition in the spec, nor every guideline for producing effective SARIF. We hope to continue to add analysis rules in both those areas.

### Installing and using the validator

To install the validator, run the command
```
dotnet tool install Sarif.Multitool --global
```
Now you can validate a SARIF file by running the command
```
sarif validate MyFile.sarif --output MyFile-validation.sarif
```
The SARIF Multitool can do many things besides validate a SARIF file (that's why it's called the "MultiTool"). To see what it can do, just run the command
```
sarif
```

## How this document is organized

This document expresses each structural requirement and guideline as a validator analysis rule. At the time of this writing, not all of those rules actually exist. Those that do not are labled "(NYI)".

First come the rules that detect serious violations of the SARIF spec (rules which the validator would report as `"error"`). They have numbers in the range 1000-1999, for example, `SARIF1001.RuleIdentifiersMustBeValid`.

Then come the rules that detect either less serious violations of the SARIF spec (rules which the validator would report as `"warning"` or `"note"`), or guidance based on integrating SARIF into a wide variety of static analysis tools. They have numbers in the range 2000-2999, for example, `SARIF2005.ProvideToolProperties`.

Each rule has a description that describes its purpose, followed by one or more messages that can appear in a SARIF result object that reports a violation of this rule. Each message includes one or more replacement sequences (`{0}`, `{1}`, _etc._). The first one (`{0}`) is always a JSON path expression that describes the location of the result. For example, `/runs/0/results/0/locations/0/physicalLocation` specifies the `physicalLocation` property of the first location of the first result in the first run in the log file.

## Rules that describe serious violations

Rules that describe violations of **SHALL**/**SHALL NOT** requirements of the [SARIF specification](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html) have numbers between 1000 and 1999, and always have level `"error"`.

---
