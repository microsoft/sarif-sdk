Feature: emit-finalize --no-repo finalizes a repo-less scan

  As an AI producer scanning code with no version control
  I want emit-finalize to finalize a run that declares no versionControlProvenance
  So that --validate accepts it instead of faulting it for the provenance it has none of

  Background:
    Given a clean working directory

  Scenario: A repo-less run is stamped unpublishable and elides its local root
    Given an open event log for tool "ai-scanner" with a source root, no VCP, and ai/origin "generated"
    When emit-results is invoked with a conformant AI result at "src/app.ts" line 1 with ruleId "CWE-79/dom-xss"
    And emit-finalize is invoked with no-repo
    Then the finalized SARIF run property "unpublishable" is true
    And the finalized SARIF result 0 artifactLocation uri equals "src/app.ts"
    And the finalized SARIF contains no "file://" anywhere

  Scenario: A repo-less run validates clean under --no-repo --validate
    Given an open event log for tool "ai-scanner" with a source root, no VCP, and ai/origin "generated"
    When emit-results is invoked with a conformant AI result at "src/app.ts" line 1 with ruleId "CWE-79/dom-xss"
    And emit-finalize is invoked with no-repo and validation
    Then the finalized SARIF conforms to the AI whole-log schema

  Scenario: --no-repo with versionControlProvenance present is a contradiction and fails
    Given an open event log for tool "ai-scanner" with a GitHub source root and VCP
    When emit-results is invoked with a result at "src/app.ts" line 1
    And emit-finalize is invoked with no-repo expecting failure
    Then the failure message mentions "--no-repo was specified, but run.versionControlProvenance"
