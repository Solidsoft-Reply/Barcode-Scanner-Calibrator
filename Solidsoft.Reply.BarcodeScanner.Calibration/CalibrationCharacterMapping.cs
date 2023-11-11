﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalibrationCharacterMapping.cs" company="Solidsoft Reply Ltd.">
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
// Represents a calibration match between an expected character and a reported sequence of characters.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

/// <summary>
///   Represents a calibration mapping from an expected character to a reported character.
/// </summary>
/// <param name="Expected">Gets the expected character.</param>
/// <param name="Reported">Gets the reported character sequence.</param>
/// <param name="ExpectedCharacterCategory">Gets the category of the expected character in a character mapping.</param>
/// <param name="DeadKey">Gets a value indicating whether the reported character is a dead key.</param>
public record CalibrationCharacterMapping(
        [property: JsonProperty("expected", Order = 0)]
        char Expected,
        [property: JsonProperty("reported", Order = 1)]
        string Reported,
        [property: JsonProperty("expectedCharacterCategory", Order = 2)]
        CalibrationCharacterCategory ExpectedCharacterCategory,
        [property: JsonProperty("deadKey", Order = 3)]
        bool DeadKey)
    : CalibrationBaseRecord;