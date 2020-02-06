The Sarif.Multitool has a 'query' verb. Query takes a SarifLog and query expression and determines how many Results match the expression. It can write the count to the console, return the count as an exit code, signal if the count exceeds a threshold, or write an output log with the filtered results only.

Query mode is not as sophisticated as the SARIF viewers, but can be used to separate logs which are too large into meaningful subsets or to trigger on simple conditions as part of an automated workflow. For arbitrarily complex scenarios, it's easiest to reference the Sarif SDK and write the logic as code.

### Examples

| Command | Meaning |
| ------- | ------- |
| `Sarif.Multitool query In.sarif -e "RuleId = SM00251"`    | Output Result count with RuleId SM00251 |
| `Sarif.Multitool query In.sarif -e "RuleId = SM00251" -o Out.sarif` | ...and write filtered log to 'Out.sarif' |
| `... -e "Uri : 'Test Framework'"` | Find Results where any URI contains 'Test Framework' |
| `... -e "!(Uri >\| .cs)"`   | Find Results where no URI EndWith '.cs' |
| `... -e "OccurrenceCount > 10 AND RuleId = SM00251"` | Look for Results which match both criteria |


### Comparison Operators

| Operator | Meaning |
| -------- | ------- |
| =, ==    | Equals (OrdinalIgnoreCase for strings) |
| !=, <>   | Not Equals |
| >        | Greater Than |
| >=       | Greater Than or Equals |
| <        | Less Than |
| <=       | Less Than or Equals |
| :        | Contains (string only, OrdinalIgnoreCase) |
| &#124;>  | StartsWith (string only, OrdinalIgnoreCase) |
| >&#124;  | EndsWith (string only, OrdinalIgnoreCase) |


### Boolean Operators

| Operator | Meaning |
| -------- | ------- |
| !, NOT | NOT (of following expression) |
| &&, AND  | AND     |
| &#124;&#124;, OR | OR |

These operators are in precedence order, so these two are equivalent:
```
NOT RuleId = SM00251 OR OccurrenceCount > 10 AND OccurrenceCount < 100
(NOT RuleId = SM00251) OR (OccurrenceCount > 10 AND OccurrenceCount < 100)
```

### Supported Result Columns:
* BaselineState
* CorrelationGuid
* Guid
* HostedViewerUri
* Kind
* Level
* Message.Text
* OccurrenceCount
* Rank
* RuleId
* Uri