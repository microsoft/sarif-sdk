// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace Test.UnitTests.Sarif.Mcp.Server.Fixtures
{
    /// <summary>
    /// Per-test scratch directory under <see cref="Path.GetTempPath"/> with
    /// guaranteed best-effort cleanup. Lift from the inline pattern in the
    /// pre-fixture acceptance tests so every emit-side test that touches the
    /// file system shares one disposal path.
    /// </summary>
    public abstract class McpScratchTestBase : IDisposable
    {
        protected string ScratchDir { get; }

        protected McpScratchTestBase()
        {
            this.ScratchDir = Path.Combine(
                Path.GetTempPath(),
                "sarif-mcp-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.ScratchDir);
        }

        protected string ScratchPath(string fileName) => Path.Combine(this.ScratchDir, fileName);

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(this.ScratchDir))
                {
                    Directory.Delete(this.ScratchDir, recursive: true);
                }
            }
            catch
            {
                // Best-effort cleanup; tests should not fail on tear-down errors.
            }

            GC.SuppressFinalize(this);
        }
    }
}
