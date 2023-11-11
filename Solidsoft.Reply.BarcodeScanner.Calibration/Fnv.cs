// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Fnv.cs" company="Solidsoft Reply Ltd.">
//   (c) 2021 Solidsoft Reply Ltd. All rights reserved.
// </copyright>
// <license>
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </license>
// <summary>
// The severity of a condition for which advice is provided.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System;
using System.Linq;

/// <summary>
///   Support for Fowler–Noll–Vo hash function. This helper class is specifically designed to
///   support hash code generation based on hash codes of .NET data objects.
/// </summary>
/// <remarks>
///   This implementation of FNV algorithms assumes that each data object has a well-defined
///   hash code algorithm. Of data objects do not override the base implementation
///   of GetHashCode, the algorithm may be compromised due to hash codes based on
///   the object reference rather than data.
/// </remarks>
public static class Fnv
{
    /// <summary>
    ///   The FNV offset basis.
    /// </summary>
    private const int OffsetBasis32 = unchecked((int)2166136261);

    /// <summary>
    ///   The FNV prime number.
    /// </summary>
    private const int Prime32 = 16777619;

    /// <summary>
    ///   Create an FNV-1a hash value based on the hash codes of each data object.
    /// </summary>
    /// <param name="objects">An array of data objects.</param>
    /// <returns>A hash value for the data objects.</returns>
    // ReSharper disable once UnusedMember.Global
    public static int CreateHashFnv1(params object[] objects)
    {
        return objects.Aggregate(OffsetBasis32, (r, o) => r.Combine(o, HashByte1));

        // Adds the specified byte to the hash.
        static int HashByte1(int hash, byte data)
        {
            unchecked
            {
                hash *= Prime32;
                hash ^= data;
                return hash;
            }
        }
    }

    /// <summary>
    ///   Create an FNV-1a hash value based on the hash codes of each data object.
    /// </summary>
    /// <param name="objects">An array of data objects.</param>
    /// <returns>A hash value for the data objects.</returns>
    public static int CreateHashFnv1A(params object?[] objects)
    {
        return objects.Aggregate(OffsetBasis32, (r, o) => o is null ? r : r.Combine(o, HashByte1A));

        // Adds the specified byte to the hash.
        static int HashByte1A(int hash, byte data)
        {
            unchecked
            {
                hash ^= data;
                hash *= Prime32;
                return hash;
            }
        }
    }

    /// <summary>
    ///   Adds the specified byte to the 32-bit hash.
    /// </summary>
    /// <param name="hash">The current hash.</param>
    /// <param name="data">The byte to hash.</param>
    /// <param name="hashByte">The function for adding the byte to the hash.</param>
    private static int HashByte(this int hash, byte data, Func<int, byte, int> hashByte) =>
        hashByte(hash, data);

    /// <summary>
    ///   Adds the specified integer to a 32-bit hash, in little-endian order.
    /// </summary>
    /// <param name="hash">The current hash.</param>
    /// <param name="obj">The object whose hashcode must be hashed.</param>
    /// <param name="hashByte">The function for adding the byte to the hash.</param>
    private static int Combine(this int hash, object? obj, Func<int, byte, int> hashByte)
    {
        var data = obj is string s
                       ? s.GetHashCode(StringComparison.Ordinal)
                       : obj?.GetHashCode() ?? 0;

        return hash.HashByte(unchecked((byte)data), hashByte)
                   .HashByte(unchecked((byte)(data >> 8)), hashByte)
                   .HashByte(unchecked((byte)(data >> 16)), hashByte)
                   .HashByte(unchecked((byte)(data >> 24)), hashByte);
    }
}