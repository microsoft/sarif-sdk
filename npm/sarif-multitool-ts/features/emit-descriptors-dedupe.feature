Feature: add-*-reporting-descriptors enforces id uniqueness

  As an AI orchestrator
  I want each descriptor id to appear at most once
  So that the finalized run has a deterministic descriptor table

  Background:
    Given a clean working directory
    And an open event log for tool "ai-scanner"

  Scenario: A NOVEL- rule descriptor is appended
    When emit-rule-descriptors is invoked with id "NOVEL-prompt-injection"
    Then the outcome reports 1 appended and 0 rejected

  Scenario: A non-NOVEL rule descriptor is rejected with AI1012
    When emit-rule-descriptors is invoked with id "CWE-79"
    Then the outcome reports 0 appended and 1 rejected
    And rejected element 0 carries errorCode "AI1012"

  Scenario: An id duplicated within a batch is rejected
    When emit-notification-descriptors is invoked with ids:
      | scan-started |
      | scan-started |
    Then the outcome reports 0 appended and 1 rejected
    And rejected element 1 message mentions "appears more than once in this batch"

  Scenario: An id already present in the event log is rejected
    Given emit-notification-descriptors has appended id "scan-started"
    When emit-notification-descriptors is invoked with id "scan-started"
    Then the outcome reports 0 appended and 1 rejected
    And rejected element 0 message mentions "already present in the event log"
