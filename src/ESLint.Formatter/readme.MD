# SARIF formatter for ESLint

`eslint-formatter-sarif` is a formatter for [ESLint](https://www.npmjs.com/package/eslint) that produces output in the SARIF (Static Analysis Results Interchange Format) v2.1.0 format.

To embed the contents of the analyzed source files in the resulting SARIF file:

```bat
set SARIF_ESLINT_EMBED=true
```

To disable content embedding:

```bat
set SARIF_ESLINT_EMBED=
```

To run unit tests:

```bat
RunTests.cmd
```
