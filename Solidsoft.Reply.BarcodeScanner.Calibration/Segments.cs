// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Advice.cs" company="Solidsoft Reply Ltd.">
//   (c) 2020 Solidsoft Reply Ltd. All rights reserved.
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
// The segments of data provided in a baseline calibration barcode.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

/// <summary>
/// The segments of data provided in a baseline calibration barcode.
/// </summary>
internal enum Segments {

    /// <summary>
    /// The data segment containing any prefix and/or AIM identifier.
    /// </summary>
    PrefixSegment = 0,

    /// <summary>
    /// The data segment containing Invariant characters.
    /// </summary>
    InvariantSegment = 1,

    /// <summary>
    /// The data segment containing additional ASCII characters.
    /// </summary>
    AdditionalAsciiSegment = 2,

    /// <summary>
    /// The data segment containing the group separator (GS) character.
    /// </summary>
    GroupSeparatorSegment = 3,

    /// <summary>
    /// The data segment containing the file separator (FS) character.
    /// </summary>
    FileSeparatorSegment = 4,

    /// <summary>
    /// The data segment containing the record separator (RS) character.
    /// </summary>
    RecordSeparatorSegment = 5,

    /// <summary>
    /// The data segment containing the unit separator (US) character.
    /// </summary>
    UnitSeparatorSegment = 6,

    /// <summary>
    /// The data segment containing any suffix.
    /// </summary>
    SuffixSegment = 7
}