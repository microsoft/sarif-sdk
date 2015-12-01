/** @namespace {Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.DataContracts} */ grammar SarifGrammar;

/**
    @className {ResultLog}
    @summary {An SARIF format log.}
*/
resultLog :
    /**
        @name {Version}
        @summary {
        The SARIF tool format version of this log file. This value should be set to 0.4, currently.
        This is the third proposed revision of a file format that is not yet completely finalized.
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
        Information about the tool or tool pipeline that generated the results in this log. A results
        log can only contain results produced by a single tool or tool pipeline. A results log can
        aggregate results from multiple tool log files, as long as context around the tool run
        (tool command-line arguments and the like) is identical for all aggregated files.
        }
    */
    toolInfo

	/**
        @name {RunInfo}
        @summary {
		A runInfo object describes the invocation of the static analysis tool that produced the results
		specified in the containing runLog object. 
        }
    */
	runInfo?

    /**
        @name {Results}
        @summary {The set of results contained in an SARIF log.}
    */
    results;

/**
    @className {ToolInfo}
    @summary {
    Information about the tool or tool pipeline that generated the results in this log.
    }
*/
toolInfo :
    /**
        @name {Name}
        @summary {
        The name of the tool or tool pipeline that generated the results in this log, e.g., FxCop.
        }
    */
    name

    /**
        @name {FullName}
        @summary {
		The name of the tool along with its version and any other useful identifying information, 
		such as its locale, e.g., "CodeScanner 2.0, Developer Preview (en-US)".
        }
    */
    fullName?

    /**
        @name {Version}
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
    version?

    /**
        @name {FileVersion}
        @summary {
		For operating systems (such as Windows) that provide the data, the binary version 
		of the primary tool exe.
		}
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
	A runInfo object describes the invocation of the static analysis tool that produced the results
	specified in the containing runLog object. 

	NOTE: The information in the runInfo object makes it possible to precisely repeat a run of a
	static analysis tool, and to verify that the results reported in the log file were generated 
	by an appropriate invocation of the tool.
	}
*/
runInfo :
    /**
		@name {InvocationInfo}
		@summary {
		A string that describes any parameterization for the tool invocation. For command line tools 
		this string may consist of the completely specified command line used to invoke the tool.
		}
    */
    invocationInfo 

    /**
        @name {AnalysisTargets}
        @summary {
		An array, each of whose elements is a fileReference object representing the location of 
		a single analysis target scanned during the run. When present, this array shall contain one entry 
		fo reach analysis target that was scanned, even if the analysis targets were not individually specified 
		on the command line. 

		NOTE 1: The command line with which the tool was invoked might specify its input files by means 
		of a wild card such as *.cc, or it might specify them implicitly, for example, by scanning the 
		files in the current directory.

		The analysisTargets array shall be empty if no analysis targets were scanned in the course of the run. 

		NOTE 2: This could happen if the command line specified a wildcard such as *.cc for the input files, 
		and no files matched the wildcard.
		}
    */
    analysisTargets?;

/** @className {RunLogs} */
runLogs: runLog*;

/** @className {Results} */
results : result*;

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
		An optional array of hash objects, each of which specifies a hashed value for the file specified 
		by the uri property, along with the name of the algorithm used to compute the hash.
		}
    */
    hashes?;

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
    @className {Result}
    @summary {
    Represents one or more observations about an analysis target produced by a static analysis tool.
    }
    @remarks {
    A result frequently, but not always, represents a code defect.
    }
*/
result :
    /**
        @name {RuleId}
        @summary {
        An opaque, stable identifier that uniquely identifies the specific rule associated with
        the result (e.g., CA2001).
        }
        @remarks {
        RuleIds need only be unique in the context of a specific tool, that is, a rule CA2001
        defined by ToolA is distinct from a rule with the same ID defined by ToolB.
        }
    */
    ruleId

    /**
        @name {Kind}
        @summary {
        A string specifying the kind of observation this result represents. 
		This shall be one of the following: warning, error, pass, pending, note, notApplicable, internalError.
		If this member is not present, its implied value is 'warning'.
        }
        @remarks {
        The following table provides more information on each kind value:
		
		* warning : A code defect or other quality issue.
		* error : A serious code defect or quality issue.
		* pass : Analysis target was determined not to be subject to a code defect or quality issue detected by a specific rule.
		* pending : An observation (persisted with relevant corresponding state) that must be evaluated further to determine pass/fail state.
		* note : An informational message.
		* notApplicable : The analysis target is not a valid subject of analysis.
		* internalError : A significant configuration or tool execution error occurred, with the result that analysis may be incomplete or compromised.
        }
    */
    kind?

    /**
        @name {FullMessage}
        @summary {
        A string that comprehensively describes the result to the users.
        }
        @remarks {
        The full message should be a single paragraph of text. If the result comprises a quality issue 
		it should provide sufficient details to completely drive resolution of an issue. This means
		it should always include:
        1) information sufficient to identify the item under analysis, 2) the state that was
        observed that led to the issue firing, 3) the risks or problems potentially associated
        with not fixing the problem, 4) the full range of responses that could be taken (including
        conditions when a problem might not be fixed or where a reported issue might be a false positive).
       
        If the optional short message is not present, UI elements with limited display real estate
        for showing results (e.g., a list view, pop-ups in the code editor, etc.) should display the
        beginning of the full message.
        }
    */
    fullMessage

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
        @name {Stacks} @serializedName {stacks}
        @summary {
        A grouped set of locations, if available, that represent stacks associated with this result.
        }
    */
    annotatedCodeLocations?

    /**
        @name {ExecutionFlows} @serializedName {executionFlows}
        @summary {
        A grouped set of location, if available, that comprise annotated
        execution flows through code which are associated with this result.
        }
    */
    executionFlows?

    /**
        @name {RelatedLocations} @serializedName {relatedLocations}
        @summary {
        A grouped set of locations and messages, if available, that represent code areas that are related
		to this result.
        }
    */
    annotatedCodeLocations?

    /**
        @name {IsSuppressedInSource}
        @summary {
        A flag that indicates whether or not this result was suppressed in source code.
        }
    */
    isSuppressedInSource?

	/**
        @name {Fixes}
        @summary {
        An array of fix objects, if available, that can be applied in order to correct this result.
        }
    */
    fixes?

    /**
        @name {Properties}
        @summary {
        Key/value pairs that additional details about the result.
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
        A source file that is associated with the current result if and only if that is not the
        same as the analysis target. This member will populated or not, in many cases, depending
        on whether PDBs associated with the analysis target are available. Examples include a C# file
        in the FxCop-like binary analysis case, or possibly a .H C++ file in the case of a source
        analysis tool like PREfast.
        }
    */
    physicalLocation?

	/**
        @name {LogicalLocation} @serializedName {logicalLocation}
        @summary {
        An object that specifies the logical location for which a result is produced.
        }
    */
    logicalLocation?

    /**
        @name {FullyQualifiedLogicalName} @serializedName {fullyQualifiedLogicalName}
        @summary {
        A string containing the language-specific logical name of the location where the result occurs; e.g.
            C: Foo
            C++: Namespace::Class::MemberFunction(int, double) const&&
            C++: Namespace::NonmemberFunction(int, double)
            C#: SecurityCryptographyRuleTests.DESCannotBeUsed.EncryptData(System.String,System.String,System.Byte[],System.Byte[])
        }
    */
    fullyQualifiedLogicalName?

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
        The specific region within the analysis where the result was detected. This SHOULD only be
        set on the last physicalLocationComponent in a physicalLocation most of the time.

        (There are some exceptions e.g. an embedded .SWF in an Office 2003 format ppt)
        }
    */
    region?;

/**
    @className {Region}
    @summary {
    Specifies a region within a file where a result was detected.
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
        @name {Kind}
        @summary {
        The type of item this location refers to.
        }
        @remarks {
        Examples include namespace, class, or function.
        }
    */
    kind?;

/**
    @className {AnnotatedCodeLocation}
    @summary {
    A code annotation that consists of single physical location and associated message, used to express
	stacks, document execution flow through a method, etc.
    }
*/
annotatedCodeLocation:
	/**
		@name {PhysicalLocations} @serializedName{physicalLocations}
		@summary {
		A set of places to which this annotation refers. These locations are of equal
		priority; for example, one entry may be the source code location while another is the
		assembly or IL location.
		}
	*/
	physicalLocations

	/**
		@name {Message} @serializedName {message}
		@summary {
		A message associated with this annotation, if applicable.
		}
	*/
	message?;

/** @className {AnnotatedCodeLocations}
    @summary {
    A set of one or more annotated code locations associated with a given result.
    }
*/
annotatedCodeLocations: annotatedCodeLocation*;

/*********************************************************
Execution flow infrastructure; execution flows are the same lists of physical locations except that
each physical location in the list can have an associated message.
*/

/**
    @className {ExecutionFlows}
    @summary {
    A set of one or more execution flows reported for a given result. Usually this will have a single
    annotation, but in some cases (such as when showing multiple threads accessing the same object at once)
    more than one execution flow may be present.
    }
*/
executionFlows: executionFlow*;

/**
    @className {ExecutionFlow}
    @summary {
    A list of places code has visited in encountering a given result. For example, this list may contain
    a function's entry point, an if branch with a message "take true branch", and some bad call that
    happens inside that if branch.
    }
*/
executionFlow: annotatedCodeLocation*;

/*********************************************************
Logical location infrastructure; logical locations refer to specific semantic code elements such as classes,
fields, methods, functions, namespaces, and/or packages.
*/

/**
    @className {LogicalLocation}
    @summary {
    A "code pointer" or similar that refers to a logical place where a result occurs. For example, the
    function where the defect occurs. Components are in order from most general to most specific.
    }
*/
logicalLocation : logicalLocationComponent*;

/**
    @className {Fixes}
    @summary {
    One or more proposed fixes for a code defect.
    }
*/
fixes : fix*;

/**
    @className {Fix}
    @summary {
    A proposed fix an a code defect represented by a result object. A fix specifies a set of file to modify.
	For each file, the fix specifies a set of bytes to remove and provides a set of new bytes to replace them.
    }
*/
fix:
	/**
		@name {Description} @serializedName{description}
		@summary {
		A string that describes the proposed fix, enabling viewers to present a proposed change to an end user.
		}
	*/
	description

	/**
		@name {FileChanges} @serializedName {fileChanges}
		@summary {
		A message associated with this annotation, if applicable.
		}
	*/
	fileChanges;

/**
    @className {FileChanges}
    @summary {
    One or more changes to a single file.
    }
*/
fileChanges : fileChange*;

/**
    @className {FileChange}
    @summary {
    A change to a single file.
    }
*/
fileChange:
	/**
		@name {Uri} @serializedName{uri}
		@summary {
		A string that represents the location of the file to change as a valid URI. If not present,
		the uri associated with this fix is assumed to be the uri of the analysisTarget member
		on the first location in the associated result.
		}
	*/
	uri?

	/**
		@name {Replacements} @serializedName {replacements}
		@summary {
		An array of replacement objects, each of which represents the replacement of a single range of
		bytes in a single file specified by the uri property, if present, or, if not, the uri
		property of the analysisTarget property of the first location of the current result.
		}
	*/
	replacements;

/**
    @className {Replacements}
    @summary {
    An array of replacement objects, each of which represent a replacement of a single range of bytes
	in a file.
    }
*/
replacements : replacement*;

/**
    @className {Replacement}
    @summary {
    The replacement of a single range of bytes in a file. Each instance specifies the location within
	the file where the replacement is to be made, the number of bytes to remove at that location, and
	a sequence of bytes to insert at that location.
    }
    @remarks {
    If a replacement object specifies both the removal of a byte range by means of the deletedLength
	property and the insertion of a sequence of bytes by means of the insertedBytes property, then 
	the effect of the replacment shall be as if the removal were performed before the insertion.

	If a single fileChange object specifies more than one replacement, then the effect of the 
	replacements shall be as if they were performed in the order in which they appear in the 
	replacements array. The offset property of each replacement shall specify an offset in the 
	unmodified file (i.e., the offsets are not recomputed based on any prior changes to the file).
    }
*/
replacement:
	/**
		@name {Offset} @serializedName{offset}
		@summary {
		A non-negative integer specifying the offset in bytes from the beginning of the file at
		which bytes are to be removed, inserted or both. An offset of 0 shall denote the first 
		byte in the file.
		}
	*/
	offset

	/**
		@name {DeletedLength} @serializedName {deletedLength}
		@summary {
		An optional non-negative integer specifying the number of bytes to delete, start at the 
		byte offset specified by the offset property, measured from the beginning of the file.
		}
		@remarks {
		If deletedLength is not present, or if its value is 0, no bytes shall be deleted.
		}
	*/
	deletedLength

/**
		@name {InsertedBytes} @serializedName {insertedBytes}
		@summary {
		An optional string that specifies the byte sequence to be inserted at the byte offset 
		specified by the offset property, measured from the beginning of the file.
		}
		@remarks {
		If insertedBytes is not preset, or if its value is 0, no bytes shall be deleted.

		If the file into which the bytes are to be inserted is a binary file, the value of the 
		insertedBytes string shall be the MIME Base64 encoding of the byte sequence to be
		inserted.

		If the file into which the bytes are to be inserted is a text file, the characters to
		be inserted shall first be encoded in UTF-8. The value of the insertedBytes string shall
		be the MIME Base64 encoding of the resulting UTF-8 byte sequence.
		}
	*/
	insertedBytes;

version : STRING;
fullVersion : STRING;
fileVersion : VERSION;
fullName : STRING;
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
offset : INTEGER;
length : INTEGER;
startLine : INTEGER;
startColumn : INTEGER;
endLine : INTEGER;
endColumn : INTEGER;
message: STRING;
invocationInfo: STRING;
value: STRING;
algorithm: STRING;
kind: STRING;
description: STRING;
deletedLength: INTEGER;
insertedBytes: INTEGER;
