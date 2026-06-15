# CWE Catalog Brief — shared subset

Filtered from `Microsoft.CodeAnalysis.Sarif.Taxonomies.CweTaxonomy.brief.md`
(MITRE CWE 4.20; 969 entries) to the **55 CWEs that at least 4 of 6
surveyed production SAST scanners independently tag in their published
rule metadata** (CodeQL, Semgrep, Bandit, gosec, Snyk Code, Veracode
SAST).

**Purpose.** This is the small, high-confidence "industry-converged"
CWE field guide for AI SARIF producers — the subset to inject by
default into LLM classification contexts when the working set has to
be tight. Sits between MITRE CWE Top 25 (too narrow, severity-weighted
not detection-weighted) and the full 969-entry catalog (too large to
inject cost-effectively, mostly describes CWEs no scanner tags).

**Why the ≥4-of-6 threshold.** Four independent tool teams converging
on the same CWE identifier is the cleanest empirical signal we have
that the CWE is well-defined enough to operationalize. Below that
threshold, you see meaningful per-tool quirks (one tool's pet
abstraction, terminology drift, naming inconsistencies). See companion
`cwe-tool-coverage-methodology.md` for the full survey methodology and
tier distribution.

**This is a snapshot, not a steady state.** The CWE catalog evolves,
tools add and retire rules, and our surveyed-tool set will grow as we
gain access to more vendor docs. Refresh against the methodology's
recipe periodically; expect the membership of this brief to shift
(see methodology's "Refresh procedure").

**Companion files in this directory.**
- `cwe-catalog-brief.observed.md` — broader 384-CWE empirical set
  (≥1 of 6 tools tag it). Escalation reference when a finding's
  weakness doesn't fit any entry in this shared brief.
- `per-tool-coverage.json` — machine-readable per-CWE tool-set
  provenance for the full 396-CWE union.
- `cwe-tool-coverage-methodology.md` — how this was made, what was
  found, how to use it.

**MITRE status retained as-is.** Several entries in this shared subset
are marked `Incomplete` by MITRE (SSRF / CWE-918, OS Command Injection
/ CWE-78). MITRE maturity status is documentation maturity, not
detection maturity; we do not filter on it.

**Outside the brief.** When a finding's CWE is not in this list,
consult `cwe-catalog-brief.observed.md` first, then the full
`CweTaxonomy.brief.md`. See methodology's "When a finding's CWE is
outside the brief" subsection.

**Schema unchanged.** One row per entry. Columns identical to upstream
`CweTaxonomy.brief.md`:

  ID | Name | Abstraction | Status | View1000 Parent | Description

The 1 missing entry (vs. the 56-CWE tool-count cohort at ≥4 tools)
is **CWE-16 "Configuration"** — a deprecated Pillar Weakness still
tagged by several scanners but absent from MITRE 4.20. See
methodology's "CWE-version skew" caveat.

| ID | Name | Abstraction | Status | Parent | Description |
|----|------|-------------|--------|--------|-------------|
| CWE-20 | ImproperInputValidation | Class | Stable | CWE-707 | The product receives input or data, but it does         not validate or incorrectly validates that the input has the         properties that are required to process the data safely and         correctly. |
| CWE-22 | PathTraversal | Base | Stable | CWE-706 | The product uses external input to construct a pathname that is intended to identify a file or directory that is located underneath a restricted parent directory, but the product does not properly neutralize special elements within the pathname that can cause the pathname to resolve to a location that is outside of the restricted directory. |
| CWE-74 | Injection | Class | Incomplete | CWE-707 | The product constructs all or part of a command, data structure, or record using externally-influenced input from an upstream component, but it does not neutralize or incorrectly neutralizes special elements that could modify how it is parsed or interpreted when it is sent to a downstream component. |
| CWE-78 | OsCommandInjection | Base | Stable | CWE-77 | The product constructs all or part of an OS command using externally-influenced input from an upstream component, but it does not neutralize or incorrectly neutralizes special elements that could modify the intended OS command when it is sent to a downstream component. |
| CWE-79 | CrossSiteScripting | Base | Stable | CWE-74 | The product does not neutralize or incorrectly neutralizes user-controllable input before it is placed in output that is used as a web page that is served to other users. |
| CWE-80 | ImproperNeutralizationOfScriptRelatedHtmlTagsInAWebPageBasicXss | Variant | Incomplete | CWE-79 | The product receives input from an upstream component, but it does not neutralize or incorrectly neutralizes special characters such as "<", ">", and "&" that could be interpreted as web-scripting elements when they are sent to a downstream component that processes web pages. |
| CWE-89 | SqlInjection | Base | Stable | CWE-943 | The product constructs all or part of an SQL command using externally-influenced input from an upstream component, but it does not neutralize or incorrectly neutralizes special elements that could modify the intended SQL command when it is sent to a downstream component. Without sufficient removal or quoting of SQL syntax in user-controllable inputs, the generated SQL query can cause those inputs to be interpreted as SQL instead of ordinary user data. |
| CWE-90 | LdapInjection | Base | Draft | CWE-943 | The product constructs all or part of an LDAP query using externally-influenced input from an upstream component, but it does not neutralize or incorrectly neutralizes special elements that could modify the intended LDAP query when it is sent to a downstream component. |
| CWE-91 | XmlInjectionAkaBlindXpathInjection | Base | Draft | CWE-74 | The product does not properly neutralize special elements that are used in XML, allowing attackers to modify the syntax, content, or commands of the XML before it is processed by an end system. |
| CWE-93 | CrlfInjection | Base | Draft | CWE-74 | The product uses CRLF (carriage return line feeds) as a special element, e.g. to separate lines or records, but it does not neutralize or incorrectly neutralizes CRLF sequences from inputs. |
| CWE-94 | CodeInjection | Base | Draft | CWE-74 | The product constructs all or part of a code segment using externally-influenced input from an upstream component, but it does not neutralize or incorrectly neutralizes special elements that could modify the syntax or behavior of the intended code segment. |
| CWE-113 | HttpRequestResponseSplitting | Variant | Incomplete | CWE-93 | The product receives data from an HTTP agent/component (e.g., web server, proxy, browser, etc.), but it does not neutralize or incorrectly neutralizes CR and LF characters before the data is included in outgoing HTTP headers. |
| CWE-117 | ImproperOutputNeutralizationForLogs | Base | Draft | CWE-116 | The product constructs a log message from external input, but it does not neutralize or incorrectly neutralizes special elements when the message is written to a log file. |
| CWE-125 | OutOfBoundsRead | Base | Draft | CWE-119 | The product reads data past the end, or before the beginning, of the intended buffer. |
| CWE-134 | UseOfExternallyControlledFormatString | Base | Draft | CWE-668 | The product uses a function that accepts a format string as an argument, but the format string originates from an external source. |
| CWE-190 | IntegerOverflowOrWraparound | Base | Stable | CWE-682 | The product performs a calculation that can          produce an integer overflow or wraparound when the logic          assumes that the resulting value will always be larger than          the original value. This occurs when an integer value is          incremented to a value that is too large to store in the          associated representation. When this occurs, the value may          become a very small or negative number. |
| CWE-200 | ExposureOfSensitiveInformationToAnUnauthorizedActor | Class | Draft | CWE-668 | The product exposes sensitive information to an actor that is not explicitly authorized to have access to that information. |
| CWE-209 | GenerationOfErrorMessageContainingSensitiveInformation | Base | Draft | CWE-200 | The product generates an error message that includes sensitive information about its environment, users, or associated data. |
| CWE-259 | UseOfHardCodedPassword | Variant | Draft | CWE-798 | The product contains a hard-coded password, which it uses for its own inbound authentication or for outbound communication to external components. |
| CWE-284 | ImproperAccessControl | Pillar | Incomplete |  | The product does not restrict or incorrectly restricts access to a resource from an unauthorized actor. |
| CWE-285 | ImproperAuthorization | Class | Draft | CWE-284 | The product does not perform or incorrectly performs an authorization check when an actor attempts to access a resource or perform an action. |
| CWE-295 | ImproperCertificateValidation | Base | Draft | CWE-287 | The product does not validate, or incorrectly validates, a certificate. |
| CWE-297 | ImproperValidationOfCertificateWithHostMismatch | Variant | Incomplete | CWE-923 | The product communicates with a host that provides a certificate, but the product does not properly ensure that the certificate is actually associated with that host. |
| CWE-311 | MissingEncryptionOfSensitiveData | Class | Draft | CWE-693 | The product does not encrypt sensitive or critical information before storage or transmission. |
| CWE-319 | CleartextTransmissionOfSensitiveInformation | Base | Draft | CWE-311 | The product transmits sensitive or security-critical data in cleartext in a communication channel that can be sniffed by unauthorized actors. |
| CWE-321 | UseOfHardCodedCryptographicKey | Variant | Draft | CWE-798 | The product uses a hard-coded, unchangeable cryptographic key. |
| CWE-326 | InadequateEncryptionStrength | Class | Draft | CWE-693 | The product stores or transmits sensitive data using an encryption scheme that is theoretically sound, but is not strong enough for the level of protection required. |
| CWE-327 | UseOfABrokenOrRiskyCryptographicAlgorithm | Class | Draft | CWE-693 | The product uses a broken or risky cryptographic algorithm or protocol. |
| CWE-329 | GenerationOfPredictableIvWithCbcMode | Variant | Draft | CWE-1204 | The product generates and uses a predictable initialization Vector (IV) with Cipher Block Chaining (CBC) Mode, which causes algorithms to be susceptible to dictionary attacks when they are encrypted under the same key. |
| CWE-330 | UseOfInsufficientlyRandomValues | Class | Stable | CWE-693 | The product uses insufficiently random numbers or values in a security context that depends on unpredictable numbers. |
| CWE-346 | OriginValidationError | Class | Draft | CWE-345 | The product does not properly verify that the source of data or communication is valid. |
| CWE-347 | ImproperVerificationOfCryptographicSignature | Base | Draft | CWE-345 | The product does not verify, or incorrectly verifies, the cryptographic signature for data. |
| CWE-377 | InsecureTemporaryFile | Class | Incomplete | CWE-668 | Creating and using insecure temporary files can leave application and system data vulnerable to attack. |
| CWE-400 | UncontrolledResourceConsumption | Class | Draft | CWE-664 | The product does not properly control the allocation and maintenance of a limited resource. |
| CWE-415 | DoubleFree | Variant | Draft | CWE-825 | The product calls free() twice on the same memory address. |
| CWE-416 | UseAfterFree | Variant | Stable | CWE-825 | The product reuses or references memory after it has been freed. At some point afterward, the memory may be allocated again and saved in another pointer, while the original pointer references a location somewhere within the new allocation. Any operations using the original pointer are no longer valid because the memory "belongs" to the code that operates on the new pointer. |
| CWE-470 | UnsafeReflection | Base | Draft | CWE-913 | The product uses external input with reflection to select which classes or code to use, but it does not sufficiently prevent the input from selecting improper classes or code. |
| CWE-489 | ActiveDebugCode | Base | Draft | CWE-710 | The product is released with debugging code still enabled or active. |
| CWE-501 | TrustBoundaryViolation | Base | Draft | CWE-664 | The product mixes trusted and untrusted data in the same data structure or structured message. |
| CWE-502 | DeserializationOfUntrustedData | Base | Draft | CWE-913 | The product deserializes untrusted data without sufficiently ensuring that the resulting data will be valid. |
| CWE-532 | InsertionOfSensitiveInformationIntoLogFile | Base | Incomplete | CWE-538 | The product writes sensitive information to a log file. |
| CWE-601 | OpenRedirect | Base | Draft | CWE-610 | The web application accepts a user-controlled input that specifies a link to an external site, and uses that link in a redirect. |
| CWE-611 | ImproperRestrictionOfXmlExternalEntityReference | Base | Draft | CWE-610 | The product processes an XML document that can contain XML entities with URIs that resolve to documents outside of the intended sphere of control, causing the product to embed incorrect documents into its output. |
| CWE-614 | SensitiveCookieInHttpsSessionWithoutSecureAttribute | Variant | Draft | CWE-319 | The Secure attribute for sensitive cookies in HTTPS sessions is not set. |
| CWE-676 | UseOfPotentiallyDangerousFunction | Base | Draft | CWE-1177 | The product invokes a potentially dangerous function that could introduce a vulnerability if it is used incorrectly, but the function can also be used safely. |
| CWE-693 | ProtectionMechanismFailure | Pillar | Draft |  | The product does not use or incorrectly uses a protection mechanism that provides sufficient defense against directed attacks against the product. |
| CWE-732 | IncorrectPermissionAssignmentForCriticalResource | Class | Draft | CWE-285 | The product specifies permissions for a security-critical resource in a way that allows that resource to be read or modified by unintended actors. |
| CWE-787 | OutOfBoundsWrite | Base | Draft | CWE-119 | The product writes data past the end, or before the beginning, of the intended buffer. |
| CWE-798 | UseOfHardCodedCredentials | Base | Draft | CWE-1391 | The product contains hard-coded credentials, such as a password or cryptographic key. |
| CWE-862 | MissingAuthorization | Class | Incomplete | CWE-285 | The product does not perform an authorization check when an actor attempts to access a resource or perform an action. |
| CWE-915 | ImproperlyControlledModificationOfDynamicallyDeterminedObjectAttributes | Base | Incomplete | CWE-913 | The product receives input from an upstream component that specifies multiple attributes, properties, or fields that are to be initialized or updated in an object, but it does not properly control which attributes can be modified. |
| CWE-916 | UseOfPasswordHashWithInsufficientComputationalEffort | Base | Incomplete | CWE-328 | The product generates a hash for a password, but it uses a scheme that does not provide a sufficient level of computational effort that would make password cracking attacks infeasible or expensive. |
| CWE-918 | ServerSideRequestForgerySsrf | Base | Incomplete | CWE-441 | The web server receives a URL or similar request from an upstream component and retrieves the contents of this URL, but it does not sufficiently ensure that the request is being sent to the expected destination. |
| CWE-942 | PermissiveCrossDomainSecurityPolicyWithUntrustedDomains | Variant | Incomplete | CWE-863 | The product uses a web-client protection          mechanism such as a Content Security Policy (CSP) or          cross-domain policy file, but the policy includes untrusted          domains with which the web client is allowed to          communicate. |
| CWE-943 | ImproperNeutralizationOfSpecialElementsInDataQueryLogic | Class | Incomplete | CWE-74 | The product generates a query intended to access or manipulate data in a data store such as a database, but it does not neutralize or incorrectly neutralizes special elements that can modify the intended logic of the query. |
