Feature: emit-finalize enriches CWE rule descriptors and stamps security-severity

  As a downstream code-scanning platform
  I want every AI rule to carry a security-severity prior and CWE metadata
  So that I can bucket the finding (critical/high/medium/low) and link to MITRE

  Background:
    Given a clean working directory
    And an open event log for tool "ai-scanner" with a GitHub source root and VCP

  Scenario: A CWE-79 result yields an enriched descriptor with security-severity
    When emit-results is invoked with a result at "src/app.ts" line 1 with ruleId "CWE-79/dom-xss"
    And emit-finalize is invoked
    Then the finalized SARIF rule "CWE-79" has a non-empty "name"
    And the finalized SARIF rule "CWE-79" carries property "security-severity"
    And the finalized SARIF rule "CWE-79" tags include "security"
    And the finalized SARIF rule "CWE-79" tags include "external/cwe/cwe-79"
    And the finalized SARIF rule "CWE-79" has helpUri starting with "https://cwe.mitre.org/"

  Scenario: A NOVEL- result yields the default medium security-severity
    When emit-results is invoked with a result at "src/app.ts" line 1 with ruleId "NOVEL-llm-leak"
    And emit-finalize is invoked
    Then the finalized SARIF rule "NOVEL-llm-leak" carries property "security-severity" equal to "5.0"
    And the finalized SARIF rule "NOVEL-llm-leak" tags include "security"

  Scenario: GitHub-hosted run collapses result ruleId sub-ids to descriptor id
    When emit-results is invoked with a result at "src/app.ts" line 1 with ruleId "CWE-79/dom-xss"
    And emit-finalize is invoked
    Then the finalized SARIF result 0 ruleId equals "CWE-79"
