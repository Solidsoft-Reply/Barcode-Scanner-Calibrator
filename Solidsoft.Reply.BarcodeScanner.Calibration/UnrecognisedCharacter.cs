﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnrecognisedCharacter.cs" company="Solidsoft Reply Ltd">
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
// The status of a calibration mapping for an expected character.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using Newtonsoft.Json;

/// <summary>
///   Represents a calibration ambiguity for an expected character sequence and two or more reported sequences of characters.
/// </summary>
/// <param name="Expected">Gets the expected character.</param>
/// <param name="InvariantCharactersOnly">
///   Gets a value indicating whether the expected character sequence contains only invariant characters.
/// .</param>
[method: JsonConstructor]
public record UnrecognisedCharacter(
        [property: JsonProperty("expected", Order = 0)] string Expected,
        [property: JsonProperty("invariantCharactersOnly", Order = 1)] bool InvariantCharactersOnly)
    : BaseRecord;