Feature: emit-finalize rebases local paths to portable per-repository roots

  As a SARIF consumer on a different machine
  I want artifact locations expressed relative to a portable repository root
  So that the log carries no machine-specific path

  Background:
    Given a clean working directory
    And an open event log for tool "ai-scanner" with a GitHub source root and VCP

  Scenario: A SRCROOT-relative path rebases to a GitHub blob permalink root
    When emit-results is invoked with a result at "src/app.ts" line 1
    And emit-finalize is invoked
    Then the finalized SARIF originalUriBaseIds "SRCROOT" uri starts with "https://github.com/"
    And the finalized SARIF originalUriBaseIds "SRCROOT" uri contains "/blob/"
    And the finalized SARIF result 0 artifactLocation uriBaseId equals "SRCROOT"
    And the finalized SARIF result 0 artifactLocation uri equals "src/app.ts"
    And the finalized SARIF contains no "file://" anywhere

  Scenario: A run without versionControlProvenance fails finalize
    Given an open event log for tool "ai-scanner" with a source root but no VCP
    When emit-finalize is invoked expecting failure
    Then the failure message mentions "requires run.versionControlProvenance"
