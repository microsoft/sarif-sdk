/** @namespace {Microsoft.CodeAnalysis.Sarif} */ grammar Sarif;

/**
    @rootObject
    @className {ResultLog}
    @summary {Static Analysis Results Format (SARIF) Version 1.0 JSON Schema (Draft 0.4). SARIF defines a standard format for the output of static analysis tools.}
*/
resultLog :
    /**
        @name {Version}
        @summary {
        The SARIF tool format version of this log file. This value should be set to 0.4, currently.
        This is the third proposed revision of a file format that is not yet completely finalized.
        }
    */
    sarifVersion

    /**
        @name {RunLogs}
        @summary {The set of runLogs contained in this SARIF log.}
		@minItems{1}
    */
    runLogs;

/** @className {SarifVersion} 
	@serializedValues {ZeroDotFour} 
*/
sarifVersion : '0.4';

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
        @name {RuleInfo}
        @summary 
		{
		An array of rule descriptor objects that describe all rules associated with an 
		analysis tool or a specific run of an analysis tool.
		}
		@minItems{1}
    */
    ruleDescriptors?

    /**
        @name {Results}
        @summary {The set of results contained in an SARIF log.}
		@minItems{0}
		@uniqueItems{true}
    */
    results?;

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
		such as its locale, e.g., 'CodeScanner 2.0, Developer Preview (en-US)'.
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
		@pattern {[0-9]+\\.[0-9]+\\.[0-9]+(-[0-9A-Za-z-]+)?(\\+[0-9A-Za-z-]+)?}
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
		@pattern {[0-9]+(\\.[0-9]+){3}}
    */
    fileVersion?
;

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
    invocationInfo?

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
		@minItems{0}
		@uniqueItems{true}
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
		@minItems{1}
		@uniqueItems{true}
    */
    hashes?
	
    /**
        @name {Properties}
        @summary {
        Key/value pairs that provide additional details about the file reference.
        }
        @remarks {
        Properties may be included in the dictionary that have an empty value.
        }
		@default {{}}
    */
    properties?
	
	/** 
		@name {Tag}
		@summary {
		A unique set of strings that provide additional information for the associated file reference.
		}
		@default {[]}
	*/
		tags?;

/** @className {Hashes} */
hashes : hash*;

/** @className {Tags} */
tags : tag*;

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
		This shall be one of the following: BLAKE-256, BLAKE-512, ECOH, FSB, GOST, Groestl, HAS-160, HAVAL, JH, MD2, 
		MD4, MD5, MD6, RadioGatun, RIPEMD, RIPEMD-128, RIPEMD-160, RIPEMD-320, SHA-1, SHA-224, SHA-256, SHA-384, 
		SHA-512, SHA-3, Skein, Snefru, Spectral Hash, SWIFFT, Tiger, Whirlpool. 
        }
    */
    algorithmKind;

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
		@default {"warning"}
    */
    resultKind?

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
    fullMessage?

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
        @name {FormattedMessage}
        @summary {
		A formattedMessage object that can be used to construct a fully formatted message that describes the result.
		If the formatted message property is present on an result, the full message property shall not be present.
		If the full message property is present on an result, the formatted message property shall not be present
        }
    */
    formattedMessage?

    /**
        @name {Locations}
        @summary {
        Specifies one or more peer locations where an issue is located. Note that this is not used
        to point to multiple instances of the same issue; separate instances should have separate
        issue objects. For example, a misspelled partial class in C# may list all the source
        lines on which the partial class is declared as separate top-level locations. However, two
        independent misspellings of the same word need to be top level issues.
        }
		@minItems{1}
		@uniqueItems{true}
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
		@minItems{1}
		@uniqueItems{true}
    */
    stacks?

    /**
        @name {ExecutionFlows} @serializedName {executionFlows}
        @summary {
        A grouped set of location, if available, that comprise annotated
        execution flows through code which are associated with this result.
        }
		@minItems{1}
		@uniqueItems{true}
    */
    executionFlows?

    /**
        @name {RelatedLocations} @serializedName {relatedLocations}
        @summary {
        A grouped set of locations and messages, if available, that represent code areas that are related
		to this result.
        }
		@minItems{1}
		@uniqueItems{true}
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
		@minItems{1}
		@uniqueItems{true}
    */
    fixes?
	
    /**
        @name {Properties}
        @summary {
        Key/value pairs that provide additional details about the result.
        }
        @remarks {
        Properties may be included in the dictionary that have an empty value.
        }
		@default {{}}
    */
    properties?
	
	/** 
		@name {Tag}
		@summary {
		A unique set of strings that provide additional information for the associated result.
		}
		@default {[]}
	*/
		tags?		
	;

/**
    @className {Location}
    @summary {
    Specifies a location within a file, or within an object nested within a file
    (such as a location within an assembly contained in an appx file).
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
        @name {ResultFile} @serializedName {resultFile}
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
		@default{{}}
    */
    properties?

	/** 
		@name {Tag}
		@summary {
		A unique set of strings that provide additional information for the associated location.
		}
		@default {[]}
	*/
		tags?	;

/** @className {ResultKind} 
	@serializedValues {Error, Warning, Pass, Note, NotApplicable, InternalError, ConfigurationError} 
*/
resultKind : 'error' | 'warning' | 'pass' | 'note' | 'notApplicable' | 'internalError' | 'configurationError';

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
	@minItems{1}
	@uniqueItems{false}
*/
physicalLocation : physicalLocationComponent*;

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
        The MIME content type (RFC 2045) of the item referred to by this location.
        }
        @remarks {
        Examples include application/macbinary, text/html, application/zip. If a viewer does not recognize
        a given MIME type, it should at least try application/zip behavior.
        }
		@pattern {[^/]+/.+}
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
		@minimum {1}
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
		@minimum {1}
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
		@minimum {1}
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
		@minimum {1}
    */
    endColumn?
	
    /**
        @name {CharOffset}
        @summary {
        The zero-based offset (measured in characters) of the first character in the region from the beginning of the file. 
        }
		@minimum {0}
    */
    charOffset?

    /**
        @name {ByteOffset}
        @summary {
        The zero-based offset (measured in bytes) of the first byte in the region from the beginning of the file. 
        }
		@minimum {0}
    */
    byteOffset?

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
		@minimum {0}
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
	stacks, execution flow through a method, or other locations that are related to a result.
    }
*/
annotatedCodeLocation:
	/**
		@name {PhysicalLocation} @serializedName{physicalLocation}
		@summary {
		A code location to which this annotation refers. 
		}
	*/
	physicalLocation

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
	@minItems{1}
	@uniqueItems{false}
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
Stacks are lists of physical locations with the addition that
each physical location in the list can have an associated message.
*/

/**
    @className {Stacks}
    @summary {
    A set of one or more stacks reported for a given result.
    }
	@minItems{1}
	@uniqueItems{false}
*/
stacks: stack*;

/**
    @className {Stacks}
    @summary {
    A set of frames that represent a stack of interest generated during analysis. 
    }
*/
stack: annotatedCodeLocation*;

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
	@minItems{1}
	@uniqueItems{false}
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
		A string that represents the location of the file to change as a valid URI.
		}
	*/
	uri

	/**
		@name {Replacements} @serializedName {replacements}
		@summary {
		An array of replacement objects, each of which represents the replacement of a single range of
		bytes in a single file specified by the uri property.
		}
		@minItems{1}
		@uniqueItems{true}
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
		@minimum {0}
	*/
	offset

	/**
		@name {DeletedLength} @serializedName {deletedLength}
		@summary {
		An optional integer specifying the number of bytes to delete, start at the 
		byte offset specified by the offset property, measured from the beginning of the file.
		}
		@remarks {
		If present, the value of deletedLength shall be 1 or greater.
		}
		@minimum {1}
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

/**
    @className {RuleDescriptors}
    @summary {
    An array of rule descriptors, each of which contains information that describes a rule.
    }
*/
ruleDescriptors : ruleDescriptor*;


/** 
	@className {RuleDescriptor}
	@interface {IRuleDescriptor}
    @summary {
    An object that contains information about an analysis rule.
    }
*/
ruleDescriptor : 
	/**
		@name {Id}
		@summary {
		A string that contains a stable, opaque identifier for a rule.
		}		
	*/
	id

	/**
		@name {Name}
		@summary {
		An optional string that contains a rule identifier that is understandable to an end user. 
		}
		@remarks {
		If the name property refers to implementation details for a rule that change over time, a
		tool author might alter a rule's name (while leaving the stable id property unchanged).
		}
	*/
	name?
	
	/**
		@name {ShortDescription}
		@summary {
		A string that contains a concise description of the rule. The short description property
		should be a single sentence that is understandable when displayed in user interface contexts
		where the available space is limited to a single line of text.
		}
	*/
	shortDescription?

	/**
		@name {FullDescription}
		@summary {
		A string whose value is a string that describes the rule. The fullDescription property should,
		as far as possible, provide details sufficient to enable resolution of any problem indicated
		by the result.
		}
		@remarks {
		The first sentence of the fullDescription property should provide a concise description of
		the rule, suitable for display in cases where available space is limited. Tools that
		construct fullDescription in this way need not provide a value for the shortDescription
		property. Tools that do not construct fullDescription in this way should provide a value
		for the shortDescription property, because otherwise, the initial portion of fullDescription
		that a viewer displays where available space is limited might not be understandable.
		}
	*/
	fullDescription?

	/**
		@name {Options}
		@summary {
		A dictionary consisting of a set of name/value pairs with arbitrary names. The options
		objects shall describe the set of configurable options supported by the rule. The value
		within each name/value pair shall be a string, which may be the empty string. The value
		shall not be a dictionary or sub-object. 
		}
	*/
	options?

	/**
		@name {FormatSpecifiers}
		@summary {
		A dictionary consisting of a set of name/value pairs with arbitrary names. The value
		within each name/value pair shall be a string that can be passed to a string formatting
		function (e.g., the C language printf function) to construct a formatted message in
		combination with an arbitrary number of additional function arguments. 
		}
		@remarks{
		The set of names appearing in the formatSpecifiers property shall contain at least
		the set of strings which occur as values of the result formatted message specifier
		id property in the result log. The formatSpecifiers property may contain additional
		name/value pairs whose names do not appear as a specifier id value for any result
		in the result log.

		Additional name/value pairs are permitted in the formatSpecifiers property for the
		convenience of tool vendors, who might find it easier to emit the entire set of
		messages supported by a rule, rather than restricting it to those messages that
		happen to appear in the result log.
		}
	*/
	formatSpecifiers?

    /**
        @name {HelpUri}
        @summary {
        A URI where the primary documentation for the rule can be found.
        }
    */
    helpUri?

    /**
        @name {Properties}
        @summary {
        A dictionary consisting of a set of name/value pairs with arbitrary names. This
		allows tools to include information about the rule that is not explicitly specified
		in the SARIF format. The value within each name/value pair shall be a string,
		which may be the empty string. The value shall not be a dictionary or sub-object.
        }
		@default {{}}
    */
    properties?

	/** 
		@name {Tag}
		@summary {
		A unique set of strings that provide additional information for the associated rule.
		}
		@default {[]}
	*/
		tags?	
	;

/**
    @className {FormattedMessage}
    @summary {
    A formatted message object encapsulates information that can be used to construct a
	fully formatted message that describes an issue.
    }
*/
formattedMessage:
	/**
		@name {SpecifierId}
		@summary {
		A string that identifies the format string used to format the message that describes
		this result. The value of specifierId must correspond to one of the names in the set
		of name/value pairs contained in the format specifiers property of the rule info
		object whose id property matches the rule id property of this issue.
		}		
	*/
	specifierId

	/**
		@name {Arguments} @serializedName {arguments}
		@summary {
		An array of string values that will be used, in combination with a format specifier,
		to construct a result message.
		}		
	*/
	arguments
;

/** @className {AlgorithmKind} 
	@serializedValues {Blake256, Blake512, Ecoh, Fsb, Gost, Groestl, Has160, Haval, JH, MD2, MD4, MD5, MD6, RadioGatun, RipeMD, RipeMD128, RipeMD160, RipeMD320, Sha1, Sha224, Sha256, Sha384, Sha512, Sha3, Skein, Snefru, SpectralHash, Swifft, Tiger, Whirlpool} 
*/
algorithmKind : 'BLAKE-256' | 'BLAKE-512' | 'ECOH' | 'FSB' | 'GOST' | 'Groestl' | 'HAS-160' | 'HAVAL' | 'JH' | 'MD2' | 'MD4' | 'MD5' | 'MD6' | 'RadioGatun' | 'RIPEMD' | 'RIPEMD-128' | 'RIPEMD-160' | 'RIPEMD-320' | 'SHA-1' | 'SHA-224' | 'SHA-256' | 'SHA-384' | 'SHA-512' | 'SHA-3' | 'Skein' | 'Snefru' | 'Spectral Hash' | 'SWIFFT' | 'Tiger' | 'Whirlpool';

version : STRING;
fullVersion : STRING;
fileVersion : STRING;
fullName : STRING;
uri : URI;
helpUri : URI;
mimeType : STRING;
toolFingerprint : STRING;
ruleId : STRING;
shortMessage : STRING;
fullMessage : STRING;
isSuppressedInSource : BOOLEAN;
/** @className {Properties} */
properties  : DICTIONARY;
/** @className {Options} */
options  : DICTIONARY;
/** @className {FormatSpecifiers} */
formatSpecifiers : DICTIONARY;
fullyQualifiedLogicalName: STRING;
name : STRING;
offset : INTEGER;
byteOffset : INTEGER;
charOffset : INTEGER;
length : INTEGER;
startLine : INTEGER;
startColumn : INTEGER;
endLine : INTEGER;
endColumn : INTEGER;
message : STRING;
invocationInfo : STRING;
value : STRING;
kind : STRING;
description : STRING;
deletedLength : INTEGER;
insertedBytes : INTEGER;
id : STRING;
tag: STRING;
specifierId : STRING;
shortDescription : STRING;
fullDescription : STRING;
arguments : STRING*;