// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalibrationAmbiguity.cs" company="Solidsoft Reply Ltd.">
//   (c) 2023 Solidsoft Reply Ltd. All rights reserved.
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
// Represents a calibration ambiguity for one or more expected character
// sequences and an ambiguous reported character sequence.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

/// <summary>
///   Represents a calibration ambiguity for one or more expected character sequences
///   and an ambiguous reported character sequence.
/// </summary>
/// <param name="Expected">Gets an array of expected character sequences.</param>
/// <param name="Reported">Gets an ambiguous reported character sequence.</param>
/// <param name="InvariantCharactersOnly">
///   Gets a value indicating whether the expected character sequence contains only invariant characters.
/// .</param>
/// <param name="DeadKey">Gets a value indicating whether the reported character is a dead key.</param>
public record CalibrationAmbiguity(
        [property: JsonProperty("expected", Order = 0)]
        IList<string>  Expected,
        [property: JsonProperty("reported", Order = 1)]
        string Reported,
        [property: JsonProperty("invariantCharactersOnly", Order = 2)]
        bool InvariantCharactersOnly,
        [property: JsonProperty("deadKey", Order = 3)]
        bool DeadKey)
    : CalibrationBaseRecord;