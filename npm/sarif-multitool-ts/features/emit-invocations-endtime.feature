Feature: emit-invocations stamps endTimeUtc only for a lone object

  As an AI orchestrator
  I want a single invocation's end time stamped from receipt
  So that I do not have to compute it; but a batch must carry per-element times

  Background:
    Given a clean working directory
    And an open event log for tool "ai-scanner"

  Scenario: A lone invocation without endTimeUtc is stamped from receipt time
    When emit-invocations is invoked with a single valid invocation lacking endTimeUtc
    Then the outcome reports 1 appended and 0 rejected
    And the appended invocation carries a non-empty "endTimeUtc"

  Scenario: A batched invocation without endTimeUtc is rejected
    When emit-invocations is invoked with a batch of 2 valid invocations lacking endTimeUtc
    Then the outcome reports 0 appended and 2 rejected
    And rejected element 0 message mentions "'endTimeUtc' is required when submitting invocations as a batch"

  Scenario: An invocation without executionSuccessful is rejected
    When emit-invocations is invoked with a single invocation missing "executionSuccessful"
    Then the outcome reports 0 appended and 1 rejected
    And rejected element 0 message mentions "'executionSuccessful' is required"

  Scenario: An invocation with an unanchored workingDirectory is rejected
    When emit-invocations is invoked with a single invocation whose workingDirectory has neither uri nor uriBaseId
    Then the outcome reports 0 appended and 1 rejected
    And rejected element 0 message mentions "'workingDirectory' must be an artifactLocation with a non-whitespace 'uri' or 'uriBaseId'"
