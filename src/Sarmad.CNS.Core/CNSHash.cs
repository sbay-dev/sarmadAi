// Copyright (c) 2026 sbay-dev. Licensed under Apache-2.0.

using System.Security.Cryptography;
using System.Text;

namespace Sarmad.CNS.Core;

/// <summary>
/// Deterministic hashing for CNS concept identification.
/// Generates a 16-character hex hash from SpaceId + Concept.
/// </summary>
public static class CNSHash
{
    /// <summary>
    /// Generate a deterministic 16-char hex hash for a concept.
    /// Hash = SHA256(spaceId + concept)[0..16]
    /// </summary>
    public static string Generate(string spaceId, string concept)
    {
        var input = Encoding.UTF8.GetBytes(spaceId + concept);
        var hash = SHA256.HashData(input);
        return Convert.ToHexString(hash)[..16].ToLowerInvariant();
    }
}
