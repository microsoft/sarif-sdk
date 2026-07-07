Feature: Attested self-hosted VCS hosts finalize instead of hard-failing

  As a scanner running against a self-hosted GitHub Enterprise Server
  I want to attest the GHES host as ghe:<host>
  So that an unrecognized host does not discard a completed scan

  Background:
    Given a clean working directory

  Scenario: An attested ghe host rebases to a blob permalink root
    Given an open event log for tool "ai-scanner" with a GHES source root and VCP authorized as "ghe:gecgithub01.walmart.com"
    When emit-results is invoked with a result at "src/app.ts" line 1
    And emit-finalize is invoked with the authorized host
    Then the finalized SARIF originalUriBaseIds "SRCROOT" uri starts with "https://gecgithub01.walmart.com/"
    And the finalized SARIF originalUriBaseIds "SRCROOT" uri contains "/blob/"
    And the finalized SARIF contains no "file://" anywhere

  Scenario: An unattested self-hosted host fails emit-run
    Given a GHES run header for tool "ai-scanner"
    When emit-run is invoked expecting failure
    Then the failure message mentions "is not a supported host"
