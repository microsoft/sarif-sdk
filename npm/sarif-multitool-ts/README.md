# @microsoft/sarif-multitool-ts

Native-TypeScript SARIF Multitool verbs (`emit-*`, `get-*`, `validate`).
In-process library + arg-compatible `sarif` CLI. No CLR dependency.

> **v0.0.x is a placeholder** that reserves the package name and proves the
> publish pipeline. Every verb currently throws / exits with a pointer to
> `@microsoft/sarif-multitool` (the .NET-backed wrapper). The TypeScript
> implementation lands in a subsequent release; track progress at
> https://github.com/microsoft/sarif-sdk.

Depends on [`@microsoft/sarif`](https://www.npmjs.com/package/@microsoft/sarif)
for the open-typed object model and core helpers.

## License

MIT
