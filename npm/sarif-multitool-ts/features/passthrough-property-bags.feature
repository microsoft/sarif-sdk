Feature: caller-supplied properties and unknown keys round-trip verbatim

  As a SARIF producer
  I want my custom properties and any extra keys to survive the emit chain
  So that the sparse TS object model never silently drops my data

  Background:
    Given a clean working directory

  Scenario: A run header property bag and unknown top-level key survive replay
    Given a run header with tool.driver.name "ai-scanner"
    And the run header carries properties bag key "customer/foo" = "bar"
    And the run header carries unknown top-level key "specialField" = "kept"
    When emit-run is invoked
    And the event log is replayed
    Then the replayed run field "properties.customer/foo" equals "bar"
    And the replayed run field "specialField" equals "kept"

  Scenario: A result property bag survives the full emit chain
    Given an open event log for tool "ai-scanner" with a GitHub source root and VCP
    When emit-results is invoked with a result at "src/app.ts" line 2 carrying properties bag key "customer/score" = "0.9"
    And emit-finalize is invoked
    Then the finalized SARIF result 0 field "properties.customer/score" equals "0.9"

  Scenario: An invocation unknown key survives append and replay
    Given an open event log for tool "ai-scanner"
    When emit-invocations is invoked with a single valid invocation carrying unknown key "extraKey" = "extraValue"
    And the event log is replayed
    Then the replayed invocation 0 field "extraKey" equals "extraValue"
