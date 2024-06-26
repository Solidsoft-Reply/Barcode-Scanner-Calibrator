﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeadKeyMapping.cs" company="Solidsoft Reply Ltd">
// Copyright (c) 2018-2024 Solidsoft Reply Ltd. All rights reserved.
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
// </copyright>
// <summary>
// Represents a calibration mapping from an expected sequence to a reported dead key character sequence.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using Newtonsoft.Json;

/// <summary>
///   Represents a calibration mapping from an expected sequence to a reported dead key character sequence.
/// </summary>
/// <param name="Expected">Gets an expected character sequence.</param>
/// <param name="Reported">Gets a reported dead key sequence.</param>
/// <param name="InvariantCharactersOnly">
///   Gets a value indicating whether the expected character sequence contains only invariant characters.
/// .</param>
[method: JsonConstructor]
public record DeadKeyMapping(
        [property: JsonProperty("expected", Order = 0)] string Expected,
        [property: JsonProperty("reported", Order = 1)] string Reported,
        [property: JsonProperty("invariantCharactersOnly", Order = 2)] bool InvariantCharactersOnly)
    : BaseRecord;