Feature: get-schema, get-skill, get-cwe serve bundled assets

  As an AI orchestrator
  I want to fetch the input schemas, the operational skills, and the CWE taxonomy
  So that I can validate payloads and follow the authoring contract without a network round-trip

  Scenario: get-schema returns the emit-results input schema
    When get-schema is invoked for "emit-results"
    Then the schema parses as JSON
    And the schema "$id" contains "ai-result.schema.json"

  Scenario: get-schema accepts the deprecated add-* verb name
    When get-schema is invoked for "add-results"
    Then the schema parses as JSON
    And the schema "$id" contains "ai-result.schema.json"

  Scenario: get-schema rejects an unknown verb
    When get-schema is invoked for "no-such-verb" expecting failure
    Then the failure message mentions "is not a verb with a schema"

  Scenario: get-schema lists every emit verb with a schema
    When get-schema --list is invoked
    Then the schema list includes "emit-run"
    And the schema list includes "emit-results"
    And the schema list includes "emit-invocations"
    And the schema list includes "emit-rule-descriptors"
    And the schema list includes "emit-notification-descriptors"

  Scenario: get-skill rewrites repository-relative links to pinned permalinks
    When get-skill is invoked for "emit-sarif" with pinRef "v5.2.0"
    Then the skill markdown contains "raw.githubusercontent.com/microsoft/sarif-sdk/v5.2.0/"
    And the skill markdown contains no "](.."

  Scenario: get-skill --list surfaces frontmatter descriptions
    When get-skill --list is invoked
    Then the skill list includes "emit-sarif" with a non-empty description

  Scenario: get-cwe returns a status-filtered SARIF taxonomy
    When get-cwe is invoked with default statuses
    Then the taxonomy is a SARIF log with at least one taxonomy
    And every taxon's "cwe/status" property is one of "Stable", "Draft", "Incomplete"
