Feature: emit-results validates ruleId and appends atomically

  As an AI orchestrator
  I want a batch of results to append all-or-none
  So that I can correct rejected elements and retry without removing partial state

  Background:
    Given a clean working directory
    And an open event log for tool "ai-scanner"

  Scenario: A single conformant result is appended
    When emit-results is invoked with a single result whose ruleId is "CWE-79/dom-xss"
    Then the outcome reports 1 appended and 0 rejected
    And the log contains exactly 1 "result" event

  Scenario: A single non-conformant ruleId is rejected with AI1012
    When emit-results is invoked with a single result whose ruleId is "CWE-79"
    Then the outcome reports 0 appended and 1 rejected
    And rejected element 0 carries errorCode "AI1012"
    And the log contains exactly 0 "result" events

  Scenario Outline: ruleId grammar accepts CWE sub-id and NOVEL forms only
    When emit-results is invoked with a single result whose ruleId is "<ruleId>"
    Then the outcome reports <appended> appended and <rejected> rejected

    Examples:
      | ruleId                          | appended | rejected |
      | CWE-89/kql-injection-from-config | 1        | 0        |
      | NOVEL-prompt-injection           | 1        | 0        |
      | CWE-89                           | 0        | 1        |
      | NOVEL-Prompt-Injection           | 0        | 1        |
      | cwe-89/sub                       | 0        | 1        |

  Scenario: A batch with one bad element appends nothing (atomic all-or-none)
    When emit-results is invoked with a batch of ruleIds:
      | CWE-79/dom-xss     |
      | not-a-valid-ruleid |
      | CWE-89/sql-inject  |
    Then the outcome reports 0 appended and 1 rejected
    And rejected element 1 carries errorCode "AI1012"
    And the log contains exactly 0 "result" events

  Scenario: A fully valid batch appends every element in order
    When emit-results is invoked with a batch of ruleIds:
      | CWE-79/dom-xss    |
      | CWE-89/sql-inject |
      | NOVEL-llm-leak    |
    Then the outcome reports 3 appended and 0 rejected
    And the log contains exactly 3 "result" events
