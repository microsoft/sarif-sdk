using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Security.Cryptography", "CA5354:SHA1CannotBeUsed", Scope = "member", Target = "Microsoft.CodeAnalysis.Sarif.HashUtilities.#ComputeHashes(System.String)")]
[module: SuppressMessage("Microsoft.Security.Cryptography", "CA5350:MD5CannotBeUsed", Scope = "member", Target = "Microsoft.CodeAnalysis.Sarif.HashUtilities.#ComputeHashes(System.String)")]
[module: SuppressMessage("Microsoft.Security.Cryptography", "CA5354:SHA1CannotBeUsed", Scope = "member", Target = "Microsoft.CodeAnalysis.Sarif.HashUtilities.#ComputeSha1Hash(System.String)")]
[module: SuppressMessage("Microsoft.Security.Cryptography", "CA5350:MD5CannotBeUsed", Scope = "member", Target = "Microsoft.CodeAnalysis.Sarif.HashUtilities.#ComputeMD5Hash(System.String)")]