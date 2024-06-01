// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Data.cs" company="Solidsoft Reply Ltd">
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
// Calibration maps and data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json;

/// <summary>
///   Calibration maps and data.
/// </summary>
/// <param name="AimFlagCharacterSequence">
///   The first (flag) character of an AIM identifier. By default, this is "]".
///   If a dead key is used, the sequence will contain two characters.
/// </param>
/// <param name="CharacterMap">
///   A dictionary of differences in reported and expected characters.
/// </param>
/// <param name="DeadKeysMap">
///   A dictionary of differences in reported and expected characters where the reported data uses dead keys.
/// </param>
/// <param name="DeadKeyCharacterMap">
///   A dictionary of the dead key characters detected during baseline calibration.
/// </param>
/// <param name="LigatureMap">
///   A dictionary of the ligature sequences detected during baseline calibration.
/// </param>
/// <param name="ScannerDeadKeysMap">
///   A dictionary of differences in reported and expected characters where the scanner keyboard layout uses dead keys.
/// </param>
/// <param name="ScannerUnassignedKeys">
///   A list of expected characters where the scanner keyboard layout key maps to an unassigned key on the computer
///   keyboard layout.
/// </param>
/// <param name="ReportedCharacters">
///   A regular expression for matching reported characters.
/// </param>
/// <param name="Prefix">
///   Any prefix reported during calibration.
/// </param>
/// <param name="Code">
///   Any code between the AIM ID (if present) and the reported data reported during calibration.
/// </param>
/// <param name="Suffix">
///   Any suffix reported during calibration.
/// </param>
/// <param name="KeyboardScript">
///   The Unicode name of the keyboard script.
/// </param>
/// <remarks>
///   The library can only detect the keyboard script by heuristic analysis of the
///   reported data. This value does not refer t the keyboard layout, but rather
///   to the characters that the keyboard predominantly represents.
/// </remarks>
/// <param name="ScannerKeyboardPerformance">
///   'Traffic Light' assessment of the performance of the barcode scanner keyboard input.
/// </param>
/// <param name="ScannerCharactersPerSecond">
///   Performance of the barcode scanner keyboard input in characters per second.
/// </param>
/// <param name="LineFeedCharacter">
///   The control character that naps to the line feed character.
/// </param>
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "<Pending>")]
[method: JsonConstructor]
public sealed record Data(
    [property: JsonProperty("characterMap", Order = 0)] IDictionary<char, char>? CharacterMap,
    [property: JsonProperty("deadKeysMap", Order = 1)] IDictionary<string, string>? DeadKeysMap,
    [property: JsonProperty("deadKeyCharacterMap", Order = 2)] IDictionary<string, char>? DeadKeyCharacterMap,
    [property: JsonProperty("ligatureMap", Order = 3)] IDictionary<string, char>? LigatureMap,
    [property: JsonProperty("scannerDeadKeysMap", Order = 4)] IDictionary<string, string>? ScannerDeadKeysMap,
    [property: JsonProperty("scannerUnassignedKeys", Order = 5)] IList<string>? ScannerUnassignedKeys,
    [property: JsonProperty("reportedCharacters", Order = 6)] string? ReportedCharacters = null,
    [property: JsonProperty("aimFlagCharacterSequence", Order = 7)] string? AimFlagCharacterSequence = null,
    [property: JsonProperty("prefix", Order = 8)] string? Prefix = null,
    [property: JsonProperty("code", Order = 9)] string? Code = null,
    [property: JsonProperty("suffix", Order = 10)] string? Suffix = null,
    [property: JsonProperty("keyboardScript", Order = 11)] string? KeyboardScript = null,
    [property: JsonProperty("scannerKeyboardPerformance", Order = 12)] ScannerKeyboardPerformance ScannerKeyboardPerformance = ScannerKeyboardPerformance.High,
    [property: JsonProperty("scannerCharactersPerSecond", Order = 13)] int ScannerCharactersPerSecond = 0,
    [property: JsonProperty("lineFeedCharacter", Order = 14)] string? LineFeedCharacter = null) {
    /// <summary>
    ///   Initializes a new instance of the <see cref="Data" /> class.
    /// </summary>
    /// <param name="json">
    ///   The calibration JSON.
    /// </param>
    public Data(string json)
        : this(
            new Dictionary<char, char>(),
            new Dictionary<string, string>(),
            new Dictionary<string, char>(),
            new Dictionary<string, char>(),
            new Dictionary<string, string>(),
#pragma warning disable IDE0028 // Simplify collection initialization
            new List<string>()) {
        if (string.IsNullOrWhiteSpace(json) || json.Length <= 1) return;

#pragma warning disable SA1010 // Opening square brackets should be spaced correctly
        var calibration = JsonConvert.DeserializeObject<Data>(json, new JsonSerializerSettings {
            Converters = [new DataConverter()],
        });
#pragma warning restore SA1010 // Opening square brackets should be spaced correctly

        if (calibration is null) {
            return;
        }

        CharacterMap = calibration.CharacterMap;
        DeadKeysMap = calibration.DeadKeysMap;
        DeadKeyCharacterMap = calibration.DeadKeyCharacterMap ?? new Dictionary<string, char>();
        KeyboardScript = calibration.KeyboardScript;
        LigatureMap = calibration.LigatureMap;
        LineFeedCharacter = calibration.LineFeedCharacter;
        ReportedCharacters = calibration.ReportedCharacters;
        AimFlagCharacterSequence = calibration.AimFlagCharacterSequence;
        Prefix = calibration.Prefix;
        Code = calibration.Code;
        Suffix = calibration.Suffix;
        ScannerDeadKeysMap = calibration.ScannerDeadKeysMap ?? new Dictionary<string, string>();
        ScannerUnassignedKeys = calibration.ScannerUnassignedKeys;
        ScannerKeyboardPerformance = calibration.ScannerKeyboardPerformance;
        ScannerCharactersPerSecond = calibration.ScannerCharactersPerSecond;
    }

    /// <summary>
    /// Returns a Data object populated from the provided JSON.
    /// </summary>
    /// <param name="json">A JSON string representing the serialized data.</param>
    /// <returns>The deserialised data.</returns>
    // ReSharper disable once UnusedMember.Global
    public static Data FromJson(string json) {
        if (string.IsNullOrWhiteSpace(json)) return new Data(string.Empty);

#pragma warning disable SA1010 // Opening square brackets should be spaced correctly
        var calibrationData = JsonConvert.DeserializeObject<Data>(json, new JsonSerializerSettings {
            Converters = [new DataConverter()],
        });
#pragma warning restore SA1010 // Opening square brackets should be spaced correctly

        return calibrationData ?? new Data(string.Empty);
    }

    /// <summary>
    ///   Returns a JSON representation of the calibration data.
    /// </summary>
    /// <returns>A JSON representation of the calibration data.</returns>
    public override string ToString() =>
        ToJson();

    /// <summary>
    ///   Returns a JSON representation of the calibration data.
    /// </summary>
    /// <param name="formatting">Specifies the formatting to be applied to the JSON.</param>
    /// <returns>A JSON representation of the calibration data.</returns>
#pragma warning disable SA1010 // Opening square brackets should be spaced correctly
    public string ToJson(Formatting formatting = Formatting.None) =>
        JsonConvert.SerializeObject(
            this,
            formatting,
            new JsonSerializerSettings {
                Converters = [new DataConverter()],
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ConstructorHandling = ConstructorHandling.Default,
                ContractResolver = new DataIgnoreEmptyEnumerableResolver { NamingStrategy = null },
            });
#pragma warning restore SA1010 // Opening square brackets should be spaced correctly
}