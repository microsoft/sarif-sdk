/** @namespace {Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.DataContracts} */ grammar ToolIssuesLog;

/**
    @className {IssueLog}
    @summary {An SARIF format log.}
*/
issueLog :
    /**
        @name {Version}
        @summary {
        The SARIF tool format version of this log file. This value should be set to 0.3, currently.
        This is the second proposed revision of a file format that is not yet completely finalized.
        }
    */
    version

    /**
        @name {RunLogs}
        @summary {The set of runLogs contained in this SARIF log.}
    */
    runLogs;

runLog:

    /**
        @name {ToolInfo}
        @summary {
        Information about the tool or tool pipeline that generated the issues in this log. An issue
        log can only contain issues produced by a single tool or tool pipeline. An issue log can
        aggregate results from multiple tool log files, as long as context around the tool run
        (tool command-line arguments and the like) is identical for all aggregated files.
        }
    */
    toolInfo

	/**
        @name {RunInfo}
        @summary {
		A runInfo object describes the invocation of the static analysis tool that produced the issues
		specified in the containing runLog object (§6.4). 
        }
    */
	runInfo

    /**
        @name {Issues}
        @summary {The set of issues contained in an SARIF log.}
    */
    issues;

/**
    @className {ToolInfo}
    @summary {
    Information about the tool or tool pipeline that generated the issues in this log.
    }
*/
toolInfo :
    /**
        @name {ToolName}
        @summary {
        The name of the tool or tool pipeline that generated the issues in this log, e.g., FxCop.
        }
    */
    toolName

    /**
        @name {FullVersion}
        @summary {
        An unformatted version string that can include additional, arbitrary details
        identifying the tool (such as build branch information, company details, etc.).
        }
    */
    fullVersion?

    /**
        @name {ProductVersion}
        @summary {
        A version that refers to the tool as a whole (as opposed to, for example,
        the build version of an individual binary in the tool).
        }
        @remarks {
        RECOMMENDED. The version must conform to the Semantic Versioning spec (semver.org). If
        the code analysis product is a native Windows program, this value SHOULD be the first
        three values for product version in the VS_VERSION_INFO structure. If the tool is a .NET
        application, this value SHOULD be the first three dotted version values of AssemblyVersion.
        }
    */
    productVersion?

    /**
        @name {FileVersion}
        @summary {For Windows tools only, the binary version of the primary tool exe. }
        @remarks {
        If the code analysis product is a native Windows program, this value MUST be the FileVersion
        field of the VS_VERSION_INFO structure. If the code analysis product is a .NET application,
        this value MUST match the first three dotted version values of AssemblyFileVersion.
        }
    */
    fileVersion?;

/**
    @className {RunInfo}
    @summary {
	A runInfo object describes the invocation of the static analysis tool that produced the issues
	specified in the containing runLog object (§6.4). 

	NOTE: The information in the runInfo object makes it possible to precisely repeat a run of a
	static analysis tool, and to verify that the issues reported in the log file were generated 
	by an appropriate invocation of the tool.
	}
*/
runInfo :
    /**
		@name {CommandLineArguments}
		@summary {
		A string containing the command line arguments with which the tool was invoked. This string 
		shall not include the file name or path to the executable itself.
		}
    */
    commandLineArguments 


    /**
        @name {AnalysisTargets}
        @summary {
		An array, each of whose elements is a fileReference object (§6.7) representing the location of 
		a single analysis target scanned during the run. This array shall contain one entry for each 
		analysis target that was scanned, even if the analysis targets were not individually specified 
		on the command line. 

		NOTE 1: The command line with which the tool was invoked might specify its input files by means 
		of a wild card such as *.cc, or it might specify them implicitly, for example, by scanning the 
		files in the current directory.

		The analysisTargets array shall be empty if no analysis targets were scanned in the course of the run. 

		NOTE 2: This could happen if the command line specified a wildcard such as *.cc for the input files, 
		and no files matched the wildcard.
		}
    */
    analysisTargets;

/** @className {RuleLogs} */
runLogs: runLog*;

/** @className {Issues} */
issues : issue*;

/** @className {AnalysisTargets} */
analysisTargets: fileReference*;

/**
    @className {FileReference}
    @summary {
    A fileReference object represents a single file
    }
*/
fileReference :
    /**
        @name {Uri}
        @summary {
        The location of the file as a valid URI
        }
    */
    uri

    /**
        @name {Hashes}
        @summary {
		An array of hash objects (§6.8), each of which specifies a hashed value for the file specified 
		by the uri property (§6.7.2), along with the name of the algorithm used to compute the hash.
		}
    */
    hashes;

/** @className {Hashes} */
hashes : hash*;

/**
    @className {Hash}
    @summary {
    A hash value of some file or collection of files, together with the algorithm used to compute the hash.
    }
*/
hash :
    /**
        @name {Value}
        @summary {
        The hash value of some file or collection of files, computed by the algorithm named in the algorithm property.
        }
    */
    value 
    /**
        @name {Algorithm}
        @summary {
        A string specifying the name of the algorithm used to compute the hash value specified in the value property. 
		This shall be one of the following: BLAKE-256, BLAKE-512, ECOH, FSB, GOST, Grøstl, HAS-160, HAVAL, JH, MD2, 
		MD4, MD5, MD6, RadioGatún, RIPEMD, RIPEMD-128, RIPEMD-160, RIPEMD-320, SHA-1, SHA-224, SHA-256, SHA-384, 
		SHA-512, SHA-3, Skein, Snefru, Spectral Hash, SWIFFT, Tiger, Whirlpool. 
        }
    */
    algorithm;

/**
    @className {Issue}
    @summary {
    Represents one or more observations about an analysis target produced by a static analysis tool.
    }
    @remarks {
    An issue frequently, but not always, represents a code defect.
    }
*/
issue :
    /**
        @name {ToolFingerprint}
        @summary {
        A string that baselining mechanisms can merge with other data to help uniquely identify this issue, run-over-run.
        }
        @remarks {
        The tool fingerprint is an optional item that is intended to be combined with other data in
        order to identify unique issues that occur run-over-run. This property should not recapitulate
        other information expressed in the issue type (for example, it should not encode the RuleId
        or the file name of the analysis target). Instead, the fingerprint element should include one
        or both of: 1) the essential data that define the issue, 2) a representation of the code that
        defines the issue. When spellchecking the type name 'MyMsspelledType' for example, 'Msspelled'
        is the essential data that identifies the issue. Other possible fingerprint elements include
        providing an appropriate region of the source code or a normalized code representation (that
        elides non-essential aspects of code, such as variable names).
        }
    */
    toolFingerprint?

    /**
        @name {RuleId}
        @summary {
        An opaque, stable identifier that uniquely identifies the specific rule associated with
        the issue(e.g., CA2001).
        }
        @remarks {
        RuleIds need only be unique in the context of a specific tool, that is, a rule CA2001
        defined by ToolA is distinct from a rule with the same ID defined by ToolB.
        }
    */
    ruleId?

    /**
        @name {Locations}
        @summary {
        Specifies one or more peer locations where an issue is located. Note that this is not used
        to point to multiple instances of the same issue; separate instances should have separate
        issue objects. For example, a misspelled partial class in C# may list all the source
        lines on which the partial class is declared as separate top-level locations. However, two
        independent misspellings of the same word need to be top level issues.
        }
    */
    locations

    /**
        @name {ShortMessage}
        @summary {
        A short description that summarizes an issue to the user in one or two lines.
        }
        @remarks {
        If this member is not present, then the beginning of the FullMessage should be
        displayed in elements of the UI where real estate is limited.
        }
    */
    shortMessage?

    /**
        @name {FullMessage}
        @summary {
        A string that comprehensively describes the issue to the users.
        }
        @remarks {
        The full message should be a single paragraph of text. It should provide sufficient
        details to completely drive resolution of an issue. This means it should always include:
        1) information sufficiently to identify the item under analysis, 2) the state that was
        observed that led to the issue firing, 3) the risks or problems potentially associated
        with not fixing the problem, 4) the full range of responses that could be taken (including
        conditions when a problem might not be fixed or where a reported issue might be a false positive).
       
        If the optional short message is not present, UI elements with limited display real estate
        for showing issues (e.g., a list view, pop-ups in the code editor, etc.) should display the
        beginning of the full message.
        }
    */
    fullMessage

    /**
        @name {Stacks} @serializedName {stacks}
        @summary {
        A grouped set of locations, if available, that represent stacks associated with this issue.
        }
    */
    executionFlows?

    /**
        @name {ExecutionFlows} @serializedName {executionFlows}
        @summary {
        A grouped set of location, if available, that comprise annotated
        execution flows through code which are associated with this issue.
        }
    */
    executionFlows?

    /**
        @name {IsSuppressedInSource}
        @summary {
        A flag that indicates whether or not this issue was suppressed in source code.
        }
    */
    isSuppressedInSource?

    /**
        @name {Properties}
        @summary {
        Key/value pairs that additional details about the issue.
        }
        @remarks {
        Properties may be included in the dictionary that have an empty value.
        }
    */
    properties?;

/**
    @className {Location}
    @summary {
    Specifies a location within a file, or within an object nested within a file
    (such as a location within an assembly contained in an appx file).

    At least one of [AnalysisTarget, IssueFile] MUST be filled in. If they are the same,
    fill out only AnalysisTarget.
    }
*/
location :
    /**
        @name {AnalysisTarget} @serializedName {analysisTarget}
        @summary {
        A source file that is associated with the item that the static analysis tool scanned. This
        may be a .dll in the case of a binary analysis tool like FxCop, or a C++ file in the case
        of a source analysis tool like PREfast. Note that the defect may not actually occur in this file.
        }
    */
    physicalLocation?

    /**
        @name {IssueFile} @serializedName {issueFile}
        @summary {
        A source file that is associated with the current issue if and only if that is not the
        same as the analysis target. This member will populated or not, in many cases, depending
        on whether PDBs associated with the analysis target are available. Examples include a C# file
        in the FxCop-like binary analysis case, or possibly a .H C++ file in the case of a source
        analysis tool like PREfast.
        }
    */
    physicalLocation?

    /**
        @name {FullyQualifiedLogicalName} @serializedName {fullyQualifiedLogicalName}
        @summary {
        A string containing the language-specific logical name of the location where the issue occurs; e.g.
            C: Foo
            C++: Namespace::Class::MemberFunction(int, double) const&&
            C++: Namespace::NonmemberFunction(int, double)
            C#: SecurityCryptographyRuleTests.DESCannotBeUsed.EncryptData(System.String,System.String,System.Byte[],System.Byte[])
        }
    */
    fullyQualifiedLogicalName?

    /**
        @name {LogicalLocation} @serializedName {logicalLocation}
        @summary {
        An object that specifies the logical namespace / function / etc location.
        }
    */
    logicalLocation?

    /**
        @name {Properties}
        @summary {
        Key/value pairs that provide additional information about this location.
        This might be used to annotate specific stack frames or points in code
        with additional information, such as assumed values of variables at that
        point of execution, etc.
        }
    */
    properties?;

/** @className {Locations} */
locations : location*;

/*********************************************************
Physical location infrastructure; physical locations represent a file or similar located on disk or HTTP or similar.
*/

/**
    @className {PhysicalLocation}
    @summary {
    A location that refers to a file. Each location component is relative to the one previous;
    this allows representation of a dll in an appx or header file in an apt package or similar.
    }
*/
physicalLocation : physicalLocationComponent*;

/** @className {PhysicalLocations} */
physicalLocations : physicalLocation*;

/**
    @className {PhysicalLocationComponent}
    @summary {A part of a location that refers to a file.}
*/
physicalLocationComponent:
    /**
        @name {Uri}
        @summary {
        Uri to the file specified by this location.
        }
        @remarks {
        This uri should be absolute for the first physicalLocationComponent in a physicalLocation;
        but should be relative for subsequent physicalLocationComponents.
        }
    */
    uri

    /**
        @name {MimeType}
        @summary {
        The MIME content type (RFC 2045) of the item refered to by this location.
        }
        @remarks {
        Examples include application/macbinary, text/html, application/zip. If a viewer does not recognize
        a given MIME type, it should at least try application/zip behavior.
        }
    */
    mimeType?

    /**
        @name {Region}
        @summary {
        The specific region within the analysis where the issue was detected. This SHOULD only be
        set on the last physicalLocationComponent in a physicalLocation most of the time.

        (There are some exceptions e.g. an embedded .SWF in an Office 2003 format ppt)
        }
    */
    region?;

/*********************************************************
Execution flow infrastructure; execution flows are the same lists of physical locations except that
each physical location in the list can have a message stapled on.
*/

/**
    @className {ExecutionFlows}
    @summary {
    A set of one or more execution flows reported for a given issue. Usually this will have a single
    execution flow, but in some cases (such as when showing multiple threads accessing the same object at once)
    more than one execution flow may be present.
    }
*/
executionFlows: executionFlow*;

/**
    @className {ExecutionFlow}
    @summary {
    A list of places code has visited in encountering a given issue. For example, this list may contain
    a function's entry point, an if branch with a message "take true branch", and some bad call that
    happens inside that if branch.
    }
*/
executionFlow: executionFlowEntry*;

/**
    @className {ExecutionFlowEntry}
    @summary {
    A single physical location visited in an execution flow.
    }
*/
executionFlowEntry:
    /**
        @name {PhysicalLocations} @serializedName{physicalLocations}
        @summary {
        A set of places to which this excution flow entry refers. These locations are of equal
        priority; for example, one entry may be the source code location while another is the
        assembly or IL location.
        }
    */
    physicalLocations

    /**
        @name {Message} @serializedName {message}
        @summary {
        A message associated with this execution flow entry, if applicable.
        }
    */
    message?;

/*********************************************************
Logical location infrastructure; logical locations refer to specific semantic code elements such as classes,
fields, methods, functions, namespaces, and/or packages.
*/

/**
    @className {LogicalLocation}
    @summary {
    A "code pointer" or similar that refers to a logical place where an issue occurs. For example, the
    function where the defect occurs. Components are in order from most general to most specific.
    }
*/
logicalLocation : logicalLocationComponent*;

/**
    @className {LogicalLocationComponent}
    @summary {One level (e.g. namespace, function, etc.) of a logical location tree.}
*/
logicalLocationComponent:
    /**
        @name {Name}
        @summary {
        Name of the item specified by this location component.
        }
    */
    name

    /**
        @name {LocationKind}
        @summary {
        The type of item this location refers to.
        }
        @remarks {
        Examples include namespace, class, or function.
        }
    */
    locationKind;

/**
    @className {Region}
    @summary {
    Specifies a region within a file where an issue was detected.
    }
    @remarks {
    Minimally, the Region should be populated with the StartLine or Offset members.
    There is some duplication of information in Region members. Log file producers
    are responsible for ensuring that any members populated in this type are
    consistent with each other (e.g., the file offset, if provided, should match the
    StartLine + EndColumn, if also provided). In the event that the type members are
    fully populated, viewers are free to choose whatever data is easiest to consume.
    }
*/
region :
    /**
        @name {StartLine}
        @summary {
        The starting line location associated with a region in the text file.
        }
        @remarks {
        This value is 1-based; that is, the first valid location within a file is line 1.
        }
    */
    startLine?

    /**
        @name {StartColumn}
        @summary {
        The starting column location associated with a region in the text file.
        }
        @remarks {
        This value is 1-based; that is, the first valid column location on a line is column 1.
        }
    */
    startColumn?

    /**
        @name {EndLine}
        @summary {
        The ending line location associated with a region in the text file.
        }
        @remarks {
        This value is 1-based; that is, the first valid location within a file is line 1.
        }
    */
    endLine?

    /**
        @name {EndColumn}
        @summary {
        The ending column location associated with a region in the text file.
        }
        @remarks {
        This value is 1-based; that is, the first valid column location on a line is column 1.
        }
    */
    endColumn?

    /**
        @name {Offset}
        @summary {
        The zero-based offset into the file.
        }
        @remarks {
        The element size associated with the length value depends on the MIME type of the
        container that it refers to (e.g, for binaries, this would be a count of bytes;
        for text files, a count of characters).
        }
    */
    offset?

    /**
        @name {Length}
        @summary {
        The length of the region.
        }
        @remarks {
        The element size associated with the length value depends on the MIME type of the
        container that it refers to (e.g, for binaries, this would be a count of bytes;
        for text files, a count of characters).
        }
    */
    length?;

version : /* @summary {A string, of the form Major.Minor, where Major and Minor are integers in the range [0, 2^16)} */ VERSION;
toolName : STRING;
fullVersion : STRING;
fileVersion : STRING;
productVersion : STRING;
uri : URI;
mimeType : STRING;
toolFingerprint : STRING;
ruleId : STRING;
shortMessage : STRING;
fullMessage : STRING;
isSuppressedInSource : BOOLEAN;
/** @className {Properties} */
properties  : DICTIONARY;
fullyQualifiedLogicalName: STRING;
name : STRING;
locationKind : STRING;
offset : INTEGER;
length : INTEGER;
startLine : INTEGER;
startColumn : INTEGER;
endLine : INTEGER;
endColumn : INTEGER;
message: STRING;
commandLineArguments: STRING;
value: STRING;
algorithm: STRING;

