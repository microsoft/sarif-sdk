---
name        : Validation rule request
about       : A detailed specification for a new SARIF validation rule to add to the Sarif.Multitool validate command.
title       : "[RULE REQUEST] Concise description of new analysis"
labels      : validation-rule-request
assignees   : ''

---

***********************************************************************************************************

# Rule Proposal: [Friendly Rule Name]
- *Synopsis*                : [Brief summary of the rule, include code snippets if possible.]

- *[Violation Example]*     : [Optional, Include code snippet which should trigger the violation.]

- *[No Violation Example]*  : [Optional, Include code snippet which demostrates ideal condition (no violation).]

***********************************************************************************************************

### Rule metadata
- [*Id*]                : [Should be formatted as `SARIF1nnn`, leave blank if unsure]
- *Name*                : [Provide a friendly symbolic name for the rule in PascalCase.]
- *Level*               : [Possible values are: `error`, `warning`, or `note`.]
- *Short description*   : [Short description of the rule.]
- *Full description*    : [Full description, should describe usage of the rule and any other relevant information.]

- User-facing strings:
  Each rule has one or more result message strings, each with symbolic name in PascalCase.

    - *FirstMessage*    : [Default user facing string.]
    - *[SecondMessage]* : [Optional, Any conditional user facing string(s).]
    - *[ThirdMessage]*    : [Optional, Any conditional user facing string(s).]

***********************************************************************************************************

### Links/Additional Information
*[Optional, any Links/Additional Information.]*

### Implementation Notes
*[Optional, any suggestions regarding implementation.]*

### How to resolve
*[Optional, any tips on how to resolve the violation.]*


***********************************************************************************************************