// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalibrationLigatureMapping.cs" company="Solidsoft Reply Ltd.">
//   (c) 2023-2024 Solidsoft Reply Ltd. All rights reserved.
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
// Represents a calibration mapping from an expected sequence to a reported dead key character sequence.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

/// <summary>
///   Represents a calibration mapping from a ligature to a reported character.
/// </summary>
/// <param name="Expected">Gets an expected character.</param>
/// <param name="Reported">Gets a reported ligature sequence.</param>
/// <param name="InvariantCharacterOnly">
///   Gets a value indicating whether the expected character is am invariant character.
/// .</param>
public record LigatureMapping(
[property: JsonProperty("expected", Order = 0)]
        char Expected,
        [property: JsonProperty("reported", Order = 1)]
        string Reported,
        [property: JsonProperty("invariantCharacterOnly", Order = 2)]
        bool InvariantCharacterOnly)
    : BaseRecord;