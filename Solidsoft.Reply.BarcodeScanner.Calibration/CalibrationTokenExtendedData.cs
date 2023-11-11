// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalibrationTokenExtendedData.cs" company="Solidsoft Reply Ltd.">
//   (c) 2018-2023 Solidsoft Reply Ltd. All rights reserved.
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
// A set of extended data passed as part of a calibration token. This data is only provided if the token is being
// used in a stateless enumeration of calibration barcodes using the NextCalibrationToken method.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Runtime.Serialization;

using Newtonsoft.Json.Serialization;

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json;

/// <summary>
///   A set of extended data passed as part of a calibration token. This data is only provided if the token is being
///   used in a stateless enumeration of calibration barcodes using the NextCalibrationToken method.
/// </summary>
public sealed class CalibrationTokenExtendedData : IEquatable<CalibrationTokenExtendedData>
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="CalibrationTokenExtendedData" /> class.
    /// </summary>
    internal CalibrationTokenExtendedData()
    {
        DeadKeysMap = new Dictionary<string, string>();
        DeadKeyCharacterMap = new Dictionary<string, char>();
        DeadKeyFixUp = new Dictionary<string, string>();
        ScannerDeadKeysMap = new Dictionary<string, string>();
        ScannerUnassignedKeys = new List<string>();
        CharacterMap = new Dictionary<char, char>();
        LigatureMap = new Dictionary<string, char>();
        UnrecognisedKeys = new List<char>();
        Prefix = string.Empty;
        Code = string.Empty;
        Suffix = string.Empty;
        ReportedPrefix = string.Empty;
        ReportedCode = string.Empty;
        ReportedSuffix = string.Empty;
        KeyboardScript = string.Empty;
        ScannerKeyboardPerformance = ScannerKeyboardPerformance.High ;
        AimFlagCharacterSequence = string.Empty;
        ReportedCharacters = string.Empty;
        PotentialIsoIec15434Unreadable = false;
        PotentialIsoIec15434EdiUnreadable = false;
        AssessFormat06Support = true;
        NonInvariantAmbiguities = new Dictionary<string, IList<string>>();
        InvariantGs1Ambiguities = new Dictionary<string, IList<string>>();
        NonInvariantUnrecognisedCharacters = new List<string>();
        InvariantGs1UnrecognisedCharacters = new List<string>();
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="CalibrationTokenExtendedData" /> class.
    /// </summary>
    /// <param name="deadKeysMap">
    ///   A dictionary of differences in reported and expected characters where the reported data uses dead keys.
    /// </param>
    /// <param name="deadKeyCharacterMap">
    ///   A dictionary of initially detected differences in reported and expected characters where the reported data uses
    ///   dead keys.
    /// </param>
    /// <param name="deadKeyFixUp">
    ///   A dictionary of dead key characters that must be fixed up due to reporting of different character to the dead key character.
    /// </param>
    /// <param name="scannerDeadKeysMap">
    ///   A dictionary of differences in reported and expected characters where the scanner keyboard layout uses dead keys.
    /// </param>
    /// <param name="scannerUnassignedKeys">
    ///   A list of expected characters where the scanner keyboard layout key maps to an unassigned key on the computer
    ///   keyboard layout.
    /// </param>
    /// <param name="characterMap">
    ///   A dictionary of differences in reported and expected characters.
    /// </param>
    /// <param name="ligatureMap">
    ///   A dictionary of candidate ligatures.
    /// </param>
    /// <param name="unrecognisedKeys">
    ///   A list of characters that are not recognised by the scanner keyboard layout.
    /// </param>
    /// <param name="prefix">
    ///   Any prefix observed during calibration.
    /// </param>
    /// <param name="code">
    ///   Any code between the AIM ID (if present) and the data observed during calibration.
    /// </param>
    /// <param name="suffix">
    ///   Any suffix observed during calibration.
    /// </param>
    /// <param name="reportedPrefix">
    ///   Any prefix reported during calibration.
    /// </param>
    /// <param name="reportedCode">
    ///   Any code between the AIM ID (if present) and the data reported during calibration.
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
    /// <param name="aimFlagCharacterSequence">
    ///   The first (flag) character. By default this is "]". If a dead key is used, the sequence will contain two
    ///   characters.
    /// </param>
    /// <param name="reportedCharacters">
    ///   A regular expression for reported characters.
    /// </param>
    /// <param name="potentialIsoIec15434Unreadable">
    ///   A value indicating whether an ISO/IEC 15434 barcode may be unreadable due to non-representation of ASCII 30.
    ///   character.
    /// </param>
    /// <param name="potentialIsoIec15434EdiUnreadable">
    ///   A value indicating whether EDI data in an ISO/IEC 15434 barcode may be unreadable due to non-representation of ASCII 28 and/or ASCII 31.
    ///   character.
    /// </param>
    /// <param name="assessFormat06Support">
    ///   A value indicating whether to assess Format 06 and Format 05 support.
    /// </param>
    /// <param name="nonInvariantAmbiguities">
    ///   A dictionary of ambiguous non-invariant ASCII characters that map to a reported character.
    /// </param>
    /// <param name="invariantGs1Ambiguities">
    ///   A dictionary of ambiguous invariant or other characters that may be used in GS1-compliant barcodes.
    /// </param>
    /// <param name="nonInvariantUnrecognisedCharacters">
    ///   A list of unrecognised non-invariant ASCII character sequences.
    /// </param>
    /// <param name="invariantGs1UnrecognisedCharacters">
    ///   A list of unrecognised invariant or other character sequences that may be used in GS1-compliant barcodes.
    /// </param>
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "Reviewed. Suppression is OK here.")]
    internal CalibrationTokenExtendedData(
        IDictionary<string, string> deadKeysMap,
        IDictionary<string, char> deadKeyCharacterMap,
        IDictionary<string, string> deadKeyFixUp,
        IDictionary<string, string> scannerDeadKeysMap,
        IList<string> scannerUnassignedKeys,
        IDictionary<char, char> characterMap,
        IDictionary<string, char> ligatureMap,
        IList<char> unrecognisedKeys,
        string prefix,
        string code,
        string suffix,
        string reportedPrefix,
        string reportedCode,
        string reportedSuffix,
        string keyboardScript,
        ScannerKeyboardPerformance scannerKeyboardPerformance,
        string aimFlagCharacterSequence,
        string reportedCharacters,
        bool potentialIsoIec15434Unreadable,
        bool potentialIsoIec15434EdiUnreadable,
        bool assessFormat06Support,
        IDictionary<string, IList<string>> nonInvariantAmbiguities,
        IDictionary<string, IList<string>> invariantGs1Ambiguities,
        IList<string> nonInvariantUnrecognisedCharacters,
        IList<string> invariantGs1UnrecognisedCharacters)
    {
        DeadKeysMap = (Dictionary<string, string>)deadKeysMap;
        DeadKeyCharacterMap = (Dictionary<string, char>)deadKeyCharacterMap;
        DeadKeyFixUp = (Dictionary<string, string>)deadKeyFixUp;
        ScannerDeadKeysMap = (Dictionary<string, string>)scannerDeadKeysMap;
        ScannerUnassignedKeys = (List<string>)scannerUnassignedKeys;
        CharacterMap = (Dictionary<char, char>)characterMap;
        LigatureMap = (Dictionary<string, char>)ligatureMap;
        UnrecognisedKeys = (List<char>)unrecognisedKeys;
        Prefix = prefix;
        Code = code;
        Suffix = suffix;
        ReportedPrefix = reportedPrefix;
        ReportedCode = reportedCode;
        ReportedSuffix = reportedSuffix;
        KeyboardScript = keyboardScript;
        ScannerKeyboardPerformance = scannerKeyboardPerformance;
        AimFlagCharacterSequence = aimFlagCharacterSequence;
        ReportedCharacters = reportedCharacters;
        PotentialIsoIec15434Unreadable = potentialIsoIec15434Unreadable;
        PotentialIsoIec15434EdiUnreadable = potentialIsoIec15434EdiUnreadable;
        AssessFormat06Support = assessFormat06Support;
        NonInvariantAmbiguities = nonInvariantAmbiguities;
        InvariantGs1Ambiguities = invariantGs1Ambiguities;
        NonInvariantUnrecognisedCharacters = nonInvariantUnrecognisedCharacters;
        InvariantGs1UnrecognisedCharacters = invariantGs1UnrecognisedCharacters;
    }

    /// <summary>
    ///   Gets the first (flag) character. By default this is "]".
    ///   If a dead key is used, the sequence will contain two characters.
    /// </summary>
    [JsonProperty("aimFlagCharacterSequence", Order = 0)]
    public string AimFlagCharacterSequence { get; private set; }

    /// <summary>
    ///   Gets a dictionary of differences in reported and expected characters.
    /// </summary>
    [JsonProperty("characterMap", Order = 1)]
    public Dictionary<char, char> CharacterMap { get; private set; }

    /// <summary>
    ///   Gets a dictionary of differences in reported and expected characters where the reported data uses dead keys.
    /// </summary>
    [JsonProperty("deadKeysMap", Order = 2)]
    public Dictionary<string, string> DeadKeysMap { get; private set; }

    /// <summary>
    ///   Gets a dictionary of initially detected differences in reported and expected characters where the reported data
    ///   uses dead keys.
    /// </summary>
    [JsonProperty("deadKeyCharacterMap", Order = 3)]
    public Dictionary<string, char> DeadKeyCharacterMap { get; private set; }

    /// <summary>
    ///   Gets a dictionary of the dead key fix up characters detected during baseline calibration.
    /// </summary>
    [JsonProperty("deadKeyFixUp", Order = 4)]
    public Dictionary<string, string> DeadKeyFixUp { get; private set; }

    /// <summary>
    ///   Gets a dictionary of differences in reported and expected characters where the scanner keyboard layout uses dead
    ///   keys.
    /// </summary>
    [JsonProperty("scannerDeadKeysMap", Order = 5)]
    public Dictionary<string, string> ScannerDeadKeysMap { get; private set; }

    /// <summary>
    ///   Gets a list of expected characters where the scanner keyboard layout key maps to an unassigned key on the computer
    ///   keyboard layout.
    /// </summary>
    [JsonProperty("scannerUnassignedKeys", Order = 6)]
    public IList<string> ScannerUnassignedKeys { get; private set; }

    /// <summary>
    ///   Gets a dictionary of candidate ligatures.
    /// </summary>
    [JsonProperty("ligatureMap", Order = 7)]
    public Dictionary<string, char> LigatureMap { get; private set; }

    /// <summary>
    ///   Gets a regular expression for matching reported characters.
    /// </summary>
    [JsonProperty("reportedCharacters", Order = 8)]
    public string ReportedCharacters { get; private set; }

    /// <summary>
    ///   Gets any prefix observed during calibration.
    /// </summary>
    [JsonProperty("prefix", Order = 9)]
    public string Prefix { get; private set; }

    /// <summary>
    ///   Gets any code between the AIM ID (if present) and the data observed during calibration.
    /// </summary>
    [JsonProperty("code", Order = 10)]
    public string Code { get; private set; }

    /// <summary>
    ///   Gets any suffix observed during calibration.
    /// </summary>
    [JsonProperty("suffix", Order = 11)]
    public string Suffix { get; private set; }

    /// <summary>
    ///   Gets any prefix reported during calibration.
    /// </summary>
    [JsonProperty("reportedPrefix", Order = 12)]
    public string ReportedPrefix { get; private set; }

    /// <summary>
    ///   Gets any code between the AIM ID (if present) and the data reported during calibration.
    /// </summary>
    [JsonProperty("reportedCode", Order = 13)]
    public string ReportedCode { get; private set; }

    /// <summary>
    ///   Gets any suffix reported during calibration.
    /// </summary>
    [JsonProperty("reportedSuffix", Order = 14)]
    public string ReportedSuffix { get; private set; }

    /// <summary>
    ///   Gets the Unicode name of the keyboard script.
    /// </summary>
    /// <remarks>
    ///   The library can only detect the keyboard script by heuristic analysis of the
    ///   reported data. This value does not refer t the keyboard layout, but rather
    ///   to the characters that the keyboard predominantly represents.
    /// </remarks>
    [JsonProperty("keyboardScript", Order = 15)]
    public string KeyboardScript { get; private set; }

    /// <summary>
    ///   Gets the 'Traffic Light' assessment of the performance of the barcode scanner keyboard input.
    /// </summary>
    [JsonProperty("scannerKeyboardPerformance", Order = 16)]
    public ScannerKeyboardPerformance ScannerKeyboardPerformance { get; private set; }

    /// <summary>
    ///   Gets a list of characters that are not recognised by the scanner keyboard layout.
    /// </summary>
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "Reviewed. Suppression is OK here.")]
    [JsonProperty("unrecognisedKeys", Order = 17)]
    public IList<char> UnrecognisedKeys { get; private set; }

    /// <summary>
    ///   Gets a value indicating whether an ISO/IEC 15434 barcode may be unreadable due to non-representation of ASCII 30.
    ///   character.
    /// </summary>
    [JsonProperty("potentialFormat06Unreadable30", Order = 18)]
    public bool PotentialIsoIec15434Unreadable { get; private set; }

    /// <summary>
    ///   Gets a value indicating whether EDI data in an ISO/IEC 15434 barcode may be unreadable due to non-representation of ASCII 28 and/or ASCII 31.
    ///   character.
    /// </summary>
    [JsonProperty("potentialIsoIec15434EdiUnreadable", Order = 19)]
    public bool PotentialIsoIec15434EdiUnreadable { get; private set; }

    /// <summary>
    ///   Gets a value indicating whether to assess Format 06 and Format 05 support.
    /// </summary>
    [JsonProperty("testSupportForFormat06", Order = 20)]
    public bool AssessFormat06Support { get; private set; }

    /// <summary>
    ///   Gets a dictionary of ambiguous non-invariant ASCII character sequences that map to a reported character.
    /// </summary>
    [JsonProperty("nonInvariantAmbiguities", Order = 21)]
    public IDictionary<string, IList<string>> NonInvariantAmbiguities { get; private set; }

    /// <summary>
    ///   Gets a dictionary of ambiguous invariant or other character sequences that may be used in GS1-compliant barcodes.
    /// </summary>
    [JsonProperty("invariantGs1Ambiguities", Order = 22)]
    public IDictionary<string, IList<string>> InvariantGs1Ambiguities { get; private set; }

    /// <summary>
    ///   Gets a list of unrecognised non-invariant character sequences.
    /// </summary>
    [JsonProperty("nonInvariantUnrecognisedCharacters", Order = 23)]
    public IList<string> NonInvariantUnrecognisedCharacters { get; private set; }

    /// <summary>
    ///   Gets a list of unrecognised invariant or other character sequences that may be used in GS1-compliant barcodes.
    /// </summary>
    [JsonProperty("invariantGs1UnrecognisedCharacters", Order = 24)]
    public IList<string> InvariantGs1UnrecognisedCharacters { get; private set; }

    /// <summary>
    ///   Gets the latest serialization or deserialization error.
    /// </summary>
    [JsonIgnore]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public string LatestError { get; private set; } = string.Empty;

    /// <summary>
    ///   Initializes the token extended data from a JSON string representing the serialized data.
    /// </summary>
    /// <param name="json">A JSON string representing the serialized data.</param>
    // ReSharper disable once UnusedMember.Global
    public static CalibrationTokenExtendedData FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new CalibrationTokenExtendedData();

        var calibrationTokenExtendedData = JsonConvert.DeserializeObject<CalibrationTokenExtendedData>(json);

        if (calibrationTokenExtendedData is null) return new CalibrationTokenExtendedData();

        return new CalibrationTokenExtendedData(
            calibrationTokenExtendedData.DeadKeysMap,
            calibrationTokenExtendedData.DeadKeyCharacterMap,
            calibrationTokenExtendedData.DeadKeyFixUp,
            calibrationTokenExtendedData.ScannerDeadKeysMap,
            calibrationTokenExtendedData.ScannerUnassignedKeys,
            calibrationTokenExtendedData.CharacterMap,
            calibrationTokenExtendedData.LigatureMap,
            calibrationTokenExtendedData.UnrecognisedKeys,
            calibrationTokenExtendedData.Prefix,
            calibrationTokenExtendedData.Code,
            calibrationTokenExtendedData.Suffix,
            calibrationTokenExtendedData.ReportedPrefix,
            calibrationTokenExtendedData.ReportedCode,
            calibrationTokenExtendedData.ReportedSuffix,
            calibrationTokenExtendedData.KeyboardScript,
            calibrationTokenExtendedData.ScannerKeyboardPerformance,
            calibrationTokenExtendedData.AimFlagCharacterSequence,
            calibrationTokenExtendedData.ReportedCharacters,
            calibrationTokenExtendedData.PotentialIsoIec15434Unreadable,
            calibrationTokenExtendedData.PotentialIsoIec15434EdiUnreadable,
            calibrationTokenExtendedData.AssessFormat06Support,
            calibrationTokenExtendedData.NonInvariantAmbiguities,
            calibrationTokenExtendedData.InvariantGs1Ambiguities,
            calibrationTokenExtendedData.NonInvariantUnrecognisedCharacters,
            calibrationTokenExtendedData.InvariantGs1UnrecognisedCharacters);
    }

    /// <summary>
    ///   Override for the equality operator.
    /// </summary>
    /// <param name="calibrationTokenExtendedData1">The first calibration token extended data.</param>
    /// <param name="calibrationTokenExtendedData2">The second calibration token extended data.</param>
    /// <returns>True, if the calibration token extended data are equal; otherwise false.</returns>
    public static bool operator ==(
        CalibrationTokenExtendedData? calibrationTokenExtendedData1,
        CalibrationTokenExtendedData calibrationTokenExtendedData2) =>
        calibrationTokenExtendedData1?.Equals(calibrationTokenExtendedData2) ?? false;

    /// <summary>
    ///   Override for the inequality operator.
    /// <param name="calibrationTokenExtendedData1">The first calibration token extended data.</param>
    /// <param name="calibrationTokenExtendedData2">The second calibration token extended data.</param>
    /// </summary>
    /// <returns>True, if the calibration token extended data are not equal; otherwise false.</returns>
    public static bool operator !=(
        CalibrationTokenExtendedData? calibrationTokenExtendedData1,
        CalibrationTokenExtendedData calibrationTokenExtendedData2) =>
        !calibrationTokenExtendedData1?.Equals(calibrationTokenExtendedData2) ?? false;

    /// <summary>
    ///   Indicates whether the current calibration token extended data is equal to another calibration token extended data
    ///   object.
    /// </summary>
    /// <param name="other">
    ///   A calibration token extended data object to compare with this current calibration token extended
    ///   data object.
    /// </param>
    /// <returns>true if the current calibration token extended data object is equal to the other parameter; otherwise, false.</returns>
    [SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1126:PrefixCallsCorrectly",
        Justification = "Reviewed. Suppression is OK here.")]
    public bool Equals(CalibrationTokenExtendedData? other) =>
        other is not null && (ReferenceEquals(this, other) ||
                         (Equals(DeadKeysMap, other.DeadKeysMap) &&
                          Equals(DeadKeyCharacterMap, other.DeadKeyCharacterMap) &&
                          Equals(DeadKeyFixUp, other.DeadKeyFixUp) &&
                          Equals(ScannerDeadKeysMap, other.ScannerDeadKeysMap) &&
                          Equals(ScannerUnassignedKeys, other.ScannerUnassignedKeys) &&
                          Equals(CharacterMap, other.CharacterMap) &&
                          Equals(LigatureMap, other.LigatureMap) &&
                          Equals(UnrecognisedKeys, other.UnrecognisedKeys) &&
                          string.Equals(Prefix, other.Prefix, StringComparison.Ordinal) &&
                          string.Equals(Code, other.Code, StringComparison.Ordinal) &&
                          string.Equals(Suffix, other.Suffix, StringComparison.Ordinal) &&
                          string.Equals(ReportedPrefix, other.ReportedPrefix, StringComparison.Ordinal) &&
                          string.Equals(ReportedCode, other.ReportedCode, StringComparison.Ordinal) &&
                          string.Equals(ReportedSuffix, other.ReportedSuffix, StringComparison.Ordinal) &&
                          ScannerKeyboardPerformance.Equals(other.ScannerKeyboardPerformance) &&
                          string.Equals(KeyboardScript, other.KeyboardScript, StringComparison.Ordinal) &&
                          string.Equals(
                              ReportedCharacters,
                              other.ReportedCharacters,
                              StringComparison.Ordinal) &&
                          string.Equals(
                              AimFlagCharacterSequence,
                              other.AimFlagCharacterSequence,
                              StringComparison.Ordinal) &&
                          PotentialIsoIec15434Unreadable.Equals(other.PotentialIsoIec15434Unreadable) &&
                          PotentialIsoIec15434EdiUnreadable.Equals(other.PotentialIsoIec15434EdiUnreadable) &&
                          AssessFormat06Support.Equals(other.AssessFormat06Support)  &&
                          PotentialIsoIec15434Unreadable.Equals(other.PotentialIsoIec15434Unreadable) &&
                          Equals(NonInvariantAmbiguities, other.NonInvariantAmbiguities) &&
                          Equals(InvariantGs1Ambiguities, other.InvariantGs1Ambiguities) &&
                          Equals(NonInvariantUnrecognisedCharacters, other.NonInvariantUnrecognisedCharacters) &&
                          Equals(InvariantGs1UnrecognisedCharacters, other.InvariantGs1UnrecognisedCharacters)));

    /// <summary>
    ///   Indicates whether the current calibration token extended data is equal to another object.
    /// </summary>
    /// <param name="obj">An object to compare with this current calibration token extended data object.</param>
    /// <returns>true if the current calibration token extended data object is equal to the other parameter; otherwise, false.</returns>
    public override bool Equals(object? obj) =>
        obj is not null && (ReferenceEquals(this, obj) ||
                       (obj is CalibrationTokenExtendedData systemCapabilities && Equals(systemCapabilities)));

    /// <summary>
    ///   Returns a hash value for the current token.
    /// </summary>
    /// <returns>The hash value.</returns>
    [SuppressMessage(
        "ReSharper",
        "NonReadonlyMemberInGetHashCode",
        Justification = "Must use private setters for JSON deserialization")]
    public override int GetHashCode() =>
        Fnv.CreateHashFnv1A(
            DeadKeysMap,
            DeadKeyCharacterMap,
            DeadKeyFixUp,
            ScannerDeadKeysMap,
            ScannerUnassignedKeys,
            CharacterMap,
            LigatureMap,
            UnrecognisedKeys,
            Prefix,
            Code,
            Suffix,
            ReportedPrefix,
            ReportedCode,
            ReportedSuffix,
            ScannerKeyboardPerformance,
            KeyboardScript,
            ReportedCharacters,
            AimFlagCharacterSequence,
            PotentialIsoIec15434Unreadable,
            PotentialIsoIec15434EdiUnreadable,
            AssessFormat06Support,
            NonInvariantAmbiguities,
            InvariantGs1Ambiguities,
            NonInvariantUnrecognisedCharacters,
            InvariantGs1UnrecognisedCharacters);

    /// <summary>
    ///   Returns a JSON representation of the calibration token extended data.
    /// </summary>
    /// <returns>A JSON representation of the calibration token extended data.</returns>
    public override string ToString() =>
        ToJson();

    /// <summary>
    ///   Returns a JSON representation of the calibration token extended data.
    /// </summary>
    /// <param name="formatting">Specifies the formatting to be applied to the JSON.</param>
    /// <returns>A JSON representation of the calibration token extended data.</returns>

    // ReSharper disable once MemberCanBePrivate.Global
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