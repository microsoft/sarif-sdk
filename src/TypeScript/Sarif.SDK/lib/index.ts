// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import * as Sarif from 'sarif'

export function CreateEmptySarifLog() : Sarif.Log
{
    return { version: "2.1.0", runs: [] };
}
