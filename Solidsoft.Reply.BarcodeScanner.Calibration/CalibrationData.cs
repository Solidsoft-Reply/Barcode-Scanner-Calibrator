// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalibrationData.cs" company="Solidsoft Reply Ltd.">
//   (c) 2018-2024 Solidsoft Reply Ltd. All rights reserved.
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
// Calibration maps and data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

/// <summary>
///   Calibration maps and data.
/// </summary>
public sealed class CalibrationData : IEquatable<CalibrationData>
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="CalibrationData" /> class.
    /// </summary>
    /// <param name="json">
    ///   The calibration JSON.
    /// </param>
    public CalibrationData(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json.Length <= 1)
        {
            AimFlagCharacterSequence = null;
            CharacterMap = new Dictionary<char, char>();
            DeadKeysMap = new Dictionary<string, string>();
            DeadKeyCharacterMap = new Dictionary<string, char>();
            KeyboardScript = null;
            LigatureMap = new Dictionary<string, char>();
            LineFeedCharacter = null;
            ReportedCharacters = null;
            ReportedCode = null;
            ReportedPrefix = null;
            ReportedSuffix = null;
            ScannerDeadKeysMap = new Dictionary<string, string>();
            ScannerUnassignedKeys = new List<string>();
            ScannerKeyboardPerformance = ScannerKeyboardPerformance.High;

            return;
        }

        var calibration = JsonConvert.DeserializeObject<CalibrationData>(json);

        if (calibration is null)
        {
            return;
        }

        AimFlagCharacterSequence = calibration.AimFlagCharacterSequence;
        CharacterMap = calibration.CharacterMap ?? new Dictionary<char, char>();
        DeadKeysMap = calibration.DeadKeysMap ?? new Dictionary<string, string>();
        DeadKeyCharacterMap = calibration.DeadKeyCharacterMap ?? new Dictionary<string, char>();
        KeyboardScript = calibration.KeyboardScript;
        LigatureMap = calibration.LigatureMap ?? new Dictionary<string, char>();
        LineFeedCharacter = calibration.LineFeedCharacter;
        ReportedCharacters = calibration.ReportedCharacters;
        ReportedCode = calibration.ReportedCode;
        ReportedPrefix = calibration.ReportedPrefix;
        ReportedSuffix = calibration.ReportedSuffix;
        ScannerDeadKeysMap = calibration.ScannerDeadKeysMap ?? new Dictionary<string, string>();
        ScannerUnassignedKeys = calibration.ScannerUnassignedKeys ?? new List<string>();
        ScannerKeyboardPerformance = calibration.ScannerKeyboardPerformance;
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="CalibrationData" /> class.
    /// </summary>
    /// <param name="aimFlagCharacterSequence">
    ///   The first (flag) character of an AIM identifier. By default, this is "]".
    ///   If a dead key is used, the sequence will contain two characters.
    /// </param>
    /// <param name="characterMap">
    ///   A dictionary of differences in reported and expected characters.
    /// </param>
    /// <param name="deadKeysMap">
    ///   A dictionary of differences in reported and expected characters where the reported data uses dead keys.
    /// </param>
    /// <param name="deadKeyCharacterMap">
    ///   A dictionary of the dead key characters detected during baseline calibration.
    /// </param>
    /// <param name="ligatureMap">
    ///   A dictionary of the ligature sequences detected during baseline calibration.
    /// </param>
    /// <param name="scannerDeadKeysMap">
    ///   A dictionary of differences in reported and expected characters where the scanner keyboard layout uses dead keys.
    /// </param>
    /// <param name="scannerUnassignedKeys">
    ///   A list of expected characters where the scanner keyboard layout key maps to an unassigned key on the computer
    ///   keyboard layout.
    /// </param>
    /// <param name="reportedCharacters">
    ///   A regular expression for matching reported characters.
    /// </param>
    /// <param name="reportedPrefix">
    ///   Any prefix reported during calibration.
    /// </param>
    /// <param name="reportedCode">
    ///   Any code between the AIM ID (if present) and the reported data reported during calibration.
    /// </param>
    /// <param name="reportedSuffix">
    ///   Any suffix reported during calibration.
    /// </param>
    /// <param name="keyboardScript">
    ///   The Unicode name of the keyboard script.
    /// </param>
    /// <param name="scannerKeyboardPerformance">
    ///   'Traffic Light' assessment of the performance of the barcode scanner keyboard input.
    /// </param>
    /// <param name="lineFeedCharacter">
    ///   The control character that naps to the line feed character.
    /// </param>
    [JsonConstructor]
    public CalibrationData(
        string? aimFlagCharacterSequence,
        IDictionary<char, char>? characterMap,
        IDictionary<string, string>? deadKeysMap,
        IDictionary<string, char>? deadKeyCharacterMap,
        IDictionary<string, char>? ligatureMap,
        IDictionary<string, string>? scannerDeadKeysMap,
        IList<string>? scannerUnassignedKeys,
        string? reportedCharacters,
        string? reportedPrefix,
        string? reportedCode,
        string? reportedSuffix,
        string? keyboardScript,
        ScannerKeyboardPerformance scannerKeyboardPerformance,
        string? lineFeedCharacter)
    {
        AimFlagCharacterSequence = aimFlagCharacterSequence;
        CharacterMap = characterMap ?? new Dictionary<char, char>();
        DeadKeysMap = deadKeysMap ?? new Dictionary<string, string>();
        DeadKeyCharacterMap = deadKeyCharacterMap ?? new Dictionary<string, char>();
        LigatureMap = ligatureMap ?? new Dictionary<string, char>();
        ScannerDeadKeysMap = scannerDeadKeysMap ?? new Dictionary<string, string>();
        ScannerUnassignedKeys = scannerUnassignedKeys ?? new List<string>();
        ReportedCharacters = reportedCharacters;
        ReportedPrefix = string.IsNullOrEmpty(reportedPrefix) ? null : reportedPrefix;
        ReportedCode = string.IsNullOrEmpty(reportedCode) ? null : reportedCode;
        ReportedSuffix = string.IsNullOrEmpty(reportedSuffix) ? null : reportedSuffix;
        KeyboardScript = keyboardScript;
        ScannerKeyboardPerformance = scannerKeyboardPerformance;
        LineFeedCharacter = lineFeedCharacter;
    }

    /// <summary>
    ///   Gets the first (flag) character of an AIM identifier. By default, this is "]".
    ///   If a dead key is used, the sequence will contain two characters.
    /// </summary>
    [JsonProperty("aimFlagCharacterSequence", Order = 0)]
    public string? AimFlagCharacterSequence { get; private set; }

    /// <summary>
    ///   Gets a dictionary of differences in reported and expected characters.
    /// </summary>
    [JsonProperty("characterMap", Order = 1)]
    public IDictionary<char, char>? CharacterMap { get; private set; }

    /// <summary>
    ///   Gets a dictionary of differences in reported and expected characters where the reported data uses dead keys.
    /// </summary>
    [JsonProperty("deadKeysMap", Order = 2)]
    public IDictionary<string, string>? DeadKeysMap { get; private set; }

    /// <summary>
    ///   Gets a dictionary of the dead key characters detected during baseline calibration.
    /// </summary>
    [JsonProperty("deadKeyCharacterMap", Order = 3)]
    public IDictionary<string, char>? DeadKeyCharacterMap { get; private set; }

    /// <summary>
    ///   Gets a dictionary of differences in reported and expected characters where the scanner keyboard layout uses dead
    ///   keys.
    /// </summary>
    [JsonProperty("scannerDeadKeysMap", Order = 4)]
    public IDictionary<string, string>? ScannerDeadKeysMap { get; private set; }

    /// <summary>
    ///   Gets a list of expected characters where the scanner keyboard layout key maps to an unassigned key on the computer
    ///   keyboard layout.
    /// </summary>
    [JsonProperty("scannerUnassignedKeys", Order = 5)]
    public IList<string>? ScannerUnassignedKeys { get; private set; }

    /// <summary>
    ///   Gets a dictionary of reported ligature sequences and corresponding expected characters.
    /// </summary>
    [JsonProperty("ligatureMap", Order = 6)]
    public IDictionary<string, char>? LigatureMap { get; private set; }
        
    /// <summary>
    ///   Gets a regular expression for matching reported characters.
    /// </summary>
    [JsonProperty("reportedCharacters", Order = 7)]
    public string? ReportedCharacters { get; private set; }

    /// <summary>
    ///   Gets any prefix reported during calibration.
    /// </summary>
    [JsonProperty("prefix", Order = 8)]
    public string? ReportedPrefix { get; private set; }

    /// <summary>
    ///   Gets any code between the AIM ID (id present) and the data reported during calibration.
    /// </summary>
    [JsonProperty("code", Order = 9)]
    public string? ReportedCode { get; private set; }

    /// <summary>
    ///   Gets any suffix reported during calibration.
    /// </summary>
    [JsonProperty("suffix", Order = 10)]
    public string? ReportedSuffix { get; private set; }

    /// <summary>
    ///   Gets the Unicode name of the keyboard script.
    /// </summary>
    /// <remarks>
    ///   The library can only detect the keyboard script by heuristic analysis of the
    ///   reported data. This value does not refer t the keyboard layout, but rather
    ///   to the characters that the keyboard predominantly represents.
    /// </remarks>
    [JsonProperty("keyboardScript", Order = 11)]
    public string? KeyboardScript { get; private set; }

    /// <summary>
    ///   Gets the current 'Traffic Light' assessment of the performance of the barcode scanner keyboard input. 
    /// </summary>
    [JsonProperty("scannerKeyboardPerformance", Order = 12)]
    public ScannerKeyboardPerformance ScannerKeyboardPerformance { get; private set; }

    /// <summary>
    ///   Gets the control character that maps to the line feed character. If \0,
    ///   no LF mapping was detected.
    /// </summary>
    [JsonProperty("lineFeedCharacter", Order = 13)]
    public string? LineFeedCharacter { get; private set; }

    /// <summary>
    ///   Gets the latest serialization or deserialization error.
    /// </summary>
    [JsonIgnore]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public string LatestError { get; private set; } = string.Empty;

    /// <summary>
    ///   Initializes the data from a JSON string representing the serialized data.
    /// </summary>
    /// <param name="json">A JSON string representing the serialized data.</param>
    // ReSharper disable once UnusedMember.Global
    public static CalibrationData FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new CalibrationData(string.Empty);

        var calibrationData = JsonConvert.DeserializeObject<CalibrationData>(json);

        if (calibrationData is null) return new CalibrationData(string.Empty);

        return new CalibrationData(
            calibrationData.AimFlagCharacterSequence,
            calibrationData.CharacterMap,
            calibrationData.ScannerDeadKeysMap,
            calibrationData.DeadKeyCharacterMap,
            calibrationData.LigatureMap,
            calibrationData.ScannerDeadKeysMap,
            calibrationData.ScannerUnassignedKeys,
            calibrationData.ReportedCharacters,
            calibrationData.ReportedPrefix,
            calibrationData.ReportedCode,
            calibrationData.ReportedSuffix,
            calibrationData.KeyboardScript,
            calibrationData.ScannerKeyboardPerformance,
            calibrationData.LineFeedCharacter);
    }

    /// <summary>
    ///   Override for the equality operator.
    /// </summary>
    /// <param name="calibrationData1">The first calibration data.</param>
    /// <param name="calibrationData2">The second calibration data.</param>
    /// <returns>True, if the calibration data are equal; otherwise false.</returns>
    public static bool operator ==(CalibrationData? calibrationData1, CalibrationData calibrationData2) =>
        calibrationData1?.Equals(calibrationData2) ?? false;

    /// <summary>
    ///   Override for the inequality operator.
    /// </summary>
    /// <param name="calibrationData1">The first calibration data.</param>
    /// <param name="calibrationData2">The second calibration data.</param>
    /// <returns>True, if the calibration data are not equal; otherwise false.</returns>
    public static bool operator !=(CalibrationData? calibrationData1, CalibrationData calibrationData2) =>
        !calibrationData1?.Equals(calibrationData2) ?? false;

    /// <summary>
    ///   Indicates whether the current calibration data is equal to another calibration data object.
    /// </summary>
    /// <param name="other">A calibration data object to compare with this current calibration data object.</param>
    /// <returns>true if the current calibration data object is equal to the other parameter; otherwise, false.</returns>
    public bool Equals(CalibrationData? other) =>
        other is not null && (ReferenceEquals(this, other) || string.Equals(
                             AimFlagCharacterSequence,
                             other.AimFlagCharacterSequence,
                             StringComparison.Ordinal) &&
                         Equals(CharacterMap, other.CharacterMap) &&
                         Equals(DeadKeysMap, other.DeadKeysMap) &&
                         Equals(DeadKeyCharacterMap, other.DeadKeyCharacterMap) &&
                         Equals(ScannerDeadKeysMap, other.ScannerDeadKeysMap) &&
                         Equals(ScannerUnassignedKeys, other.ScannerUnassignedKeys) &&
                         Equals(LigatureMap, other.LigatureMap) &&
                         string.Equals(ReportedCharacters, other.ReportedCharacters, StringComparison.Ordinal) &&
                         string.Equals(ReportedPrefix, other.ReportedPrefix, StringComparison.Ordinal) &&
                         string.Equals(ReportedCode, other.ReportedCode, StringComparison.Ordinal) &&
                         string.Equals(ReportedSuffix, other.ReportedSuffix, StringComparison.Ordinal) &&
                         string.Equals(KeyboardScript, other.KeyboardScript, StringComparison.Ordinal) &&
                         ScannerKeyboardPerformance.Equals(other.ScannerKeyboardPerformance) &&
                         (LineFeedCharacter?.Equals(other.LineFeedCharacter) ?? false));

    /// <summary>
    ///   Indicates whether the current calibration data is equal to another object.
    /// </summary>
    /// <param name="obj">An object to compare with this current calibration data object.</param>
    /// <returns>true if the current calibration data object is equal to the other parameter; otherwise, false.</returns>
    public override bool Equals(object? obj) =>
        obj is not null && 
        (ReferenceEquals(this, obj) || obj is CalibrationData data && Equals(data));

    /// <summary>
    ///   Returns a hash value for the current token.
    /// </summary>
    /// <returns>The hash value.</returns>
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode() =>
        Fnv.CreateHashFnv1A(
            AimFlagCharacterSequence,
            CharacterMap,
            DeadKeysMap,
            DeadKeyCharacterMap,
            ScannerDeadKeysMap,
            ScannerUnassignedKeys,
            LigatureMap,
            ReportedCharacters,
            ReportedPrefix,
            ReportedCode,
            ReportedSuffix,
            KeyboardScript,
            ScannerKeyboardPerformance,
            LineFeedCharacter);

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
    public string ToJson(Formatting formatting = Formatting.None) =>
        JsonConvert.SerializeObject(
            this,
            formatting,
            new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = new CalibrationDataIgnoreEmptyEnumerableResolver()
            });

    /// <summary>
    ///   Handles errors in serialization and deserialization
    /// </summary>
    /// <param name="context">The streaming context.</param>
    /// <param name="errorContext">The error context</param>
    [OnError]
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once UnusedParameter.Global
#pragma warning disable CA1801 // Review unused parameters
    internal void OnError(StreamingContext context, ErrorContext errorContext)
#pragma warning restore CA1801 // Review unused parameters
    {
        var settings = new JsonSerializerSettings
                       {
                           StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                           DefaultValueHandling = DefaultValueHandling.Ignore,
                           ContractResolver = new CalibrationDataIgnoreEmptyEnumerableResolver()
                       };

        LatestError = JsonConvert.SerializeObject(errorContext, settings);
        errorContext.Handled = true;
    }
}