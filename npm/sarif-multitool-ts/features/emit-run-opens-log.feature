Feature: emit-run opens an event log seeded with a run header

  As an AI security scanner
  I want to open a SARIF authoring session with my tool identity
  So that subsequent add-* events accumulate against a valid run skeleton

  Background:
    Given a clean working directory

  Scenario: A minimal run header opens a new .wip.jsonl
    Given a run header with tool.driver.name "ai-scanner"
    When emit-run is invoked
    Then a .wip.jsonl event log exists
    And the log contains exactly 1 "run-header" event
    And the run-header payload field "tool.driver.name" equals "ai-scanner"

  Scenario: A run header without tool.driver.name is rejected
    Given a run header without tool.driver.name
    When emit-run is invoked expecting failure
    Then the failure message mentions "tool.driver.name is required"
    And no .wip.jsonl event log exists

  Scenario: A SARIF log accidentally supplied as the run header is rejected
    Given a run header that is actually a full SARIF log
    When emit-run is invoked expecting failure
    Then the failure message mentions "expects a SARIF Run object, not a SARIF log"

  Scenario: An existing .wip.jsonl is refused without --force-overwrite
    Given a run header with tool.driver.name "ai-scanner"
    And emit-run has already been invoked
    When emit-run is invoked expecting failure
    Then the failure message mentions "already exists"

  Scenario: --force-overwrite replaces an existing .wip.jsonl
    Given a run header with tool.driver.name "ai-scanner"
    And emit-run has already been invoked
    When emit-run is invoked with force-overwrite
    Then a .wip.jsonl event log exists
    And the log contains exactly 1 "run-header" event
