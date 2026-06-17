Feature: emit-finalize populates region snippets and char spans

  As a downstream consumer (AI evidence pipeline, code-flow viewer)
  I want every result region to carry charOffset/charLength and snippet text
  So that I can address the span without re-opening the source file

  Background:
    Given a clean working directory
    And an open event log for tool "ai-scanner" with a GitHub source root and VCP
    And a fixture source file "src/app.ts" with content:
      """
      const a = 1;
      const b = unsafe(input);
      const c = 3;
      """

  Scenario: A line-only region gains charOffset, charLength, and snippet
    When emit-results is invoked with a result at "src/app.ts" line 2
    And emit-finalize is invoked
    Then the finalized SARIF result 0 region has a non-negative "charOffset"
    And the finalized SARIF result 0 region has a positive "charLength"
    And the finalized SARIF result 0 region snippet text equals "const b = unsafe(input);"

  Scenario: A context region surrounds the primary region
    When emit-results is invoked with a result at "src/app.ts" line 2
    And emit-finalize is invoked
    Then the finalized SARIF result 0 has a contextRegion
    And the contextRegion snippet text contains "const a = 1;"
    And the contextRegion snippet text contains "const c = 3;"

  Scenario: The artifact gains a sha-256 hash
    When emit-results is invoked with a result at "src/app.ts" line 2
    And emit-finalize is invoked
    Then the finalized SARIF artifact for "src/app.ts" carries a "sha-256" hash
