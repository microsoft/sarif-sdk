<!--
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
-->

# sarif-multitool

Use the SARIF Multitool to transform, enrich, filter, result match, and do other common operations against SARIF files.
See [Multitool Usage](https://github.com/microsoft/sarif-sdk/blob/master/docs/multitool-usage.md) for mode documentation and specific examples.

## Usage (Console) 
```
npm i -g @microsoft/sarif-multitool
npx @microsoft/sarif-multitool <args>
```

## Usage (TypeScript)
```ts
 import {spawnSync} from 'child_process'
 import multitoolPath from '@microsoft/sarif-multitool'
 
 spawnSync(multitoolPath, ['<args>'], { stdio: 'inherit' })
 ```

## Version numbers

This package will have version 0.x.y as a pre-release, and then will follow the SARIF SDK versioning scheme.
See the [Release History](https://github.com/microsoft/sarif-sdk/blob/master/src/ReleaseHistory.md) for changes in each version.

## Contributing

Contribute to the SARIF Multitool at [Sarif SDK](https://github.com/microsoft/sarif-sdk#sarif-sdk).