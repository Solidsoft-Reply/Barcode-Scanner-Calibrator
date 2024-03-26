// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalibrationCharacterCategory.cs" company="Solidsoft Reply Ltd.">
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
// The category of the character.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

/// <summary>
/// The category of the character.
/// </summary>
[Flags]
public enum CharacterCategory
{
    /// <summary>
    ///   The character is not categorised.
    /// </summary>
    None = 0,

    /// <summary>
    /// The character is an ASCII character
    /// </summary>
    Ascii = 1 << 0,

    /// <summary>
    ///   The character is an invariant character (ISO 646).
    /// </summary>
    Invariant = 1 << 2,

    /// <summary>
    ///   The expected character is a control character.
    /// </summary>
    Control = 1 << 3
}