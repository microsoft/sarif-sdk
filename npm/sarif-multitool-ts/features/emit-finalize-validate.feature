Feature: emit-finalize --validate checks the finalized log against the AI whole-log schema

  As an AI producer finalizing a SARIF log
  I want emit-finalize to validate its output against ai-sarif-log.schema.json
  So that an AI-profile violation is caught at finalize time, not downstream

  Background:
    Given a clean working directory

  Scenario: A conformant finalized run validates clean
    Given an open event log for tool "ai-scanner" with a GitHub source root, VCP, and ai/origin "generated"
    When emit-results is invoked with a conformant AI result at "src/app.ts" line 1 with ruleId "CWE-79/dom-xss"
    And emit-finalize is invoked with validation
    Then the finalized SARIF conforms to the AI whole-log schema

  Scenario: A run missing its ai/origin is reported as non-conformant
    Given an open event log for tool "ai-scanner" with a GitHub source root and VCP
    When emit-results is invoked with a result at "src/app.ts" line 1 with ruleId "CWE-79/dom-xss"
    And emit-finalize is invoked with validation
    Then the finalized SARIF does not conform to the AI whole-log schema
    And a validation error mentions "properties"
