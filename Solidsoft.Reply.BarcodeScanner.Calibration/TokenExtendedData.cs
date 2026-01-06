// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TokenExtendedData.cs" company="Solidsoft Reply Ltd">
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
// A set of extended data passed as part of a calibration token. This data is only provided if the token is being
// used in a stateless enumeration of calibration barcodes using the NextCalibrationToken method.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
///   A set of extended data passed as part of a calibration token. This data is only provided if the token is being
///   used in a stateless enumeration of calibration barcodes using the NextCalibrationToken method.
/// </summary>
public sealed record TokenExtendedData {
    /// <summary>
    ///   Initializes a new instance of the <see cref="TokenExtendedData" /> class.
    /// </summary>
    internal TokenExtendedData() {
#pragma warning disable IDE0028 // Simplify collection initialization
        DeadKeysMap = [];
        DeadKeyCharacterMap = [];
        DeadKeyFixUp = [];
        ScannerDeadKeysMap = [];
        ScannerUnassignedKeys = new List<string>();
        CharacterMap = [];
        LigatureMap = [];
        UnrecognisedKeys = new List<char>();
        Prefix = string.Empty;
        Code = string.Empty;
        Suffix = string.Empty;
        ReportedPrefix = string.Empty;
        ReportedCode = string.Empty;
        ReportedSuffix = string.Empty;
        KeyboardScript = string.Empty;
        ScannerKeyboardPerformance = ScannerKeyboardPerformance.High;
        ScannerCharactersPerSecond = 0;
        AimFlagCharacterSequence = string.Empty;
        ReportedCharacters = string.Empty;
        PotentialIsoIec15434Unreadable30 = false;
        PotentialIsoIec15434EdiUnreadableFs = false;
        PotentialIsoIec15434EdiUnreadableUs = false;
        PotentialIsoIec15434EotUnreadable = false;
        AssessFormat06Support = true;
        NonInvariantAmbiguities = new Dictionary<string, IList<string>>();
        InvariantGs1Ambiguities = new Dictionary<string, IList<string>>();
        NonInvariantUnrecognisedCharacters = new List<string>();
        InvariantGs1UnrecognisedCharacters = new List<string>();
#pragma warning restore IDE0028 // Simplify collection initialization
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="TokenExtendedData" /> class.
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
    /// <param name="scannerCharactersPerSecond">
    ///   Performance of the barcode scanner keyboard input in characters per second.
    /// </param>
    /// <param name="aimFlagCharacterSequence">
    ///   The first (flag) character. By default, this is "]". If a dead key is used, the sequence will contain two
    ///   characters.
    /// </param>
    /// <param name="reportedCharacters">
    ///   A regular expression for reported characters.
    /// </param>
    /// <param name="potentialIsoIec15434Unreadable30">
    ///   A value indicating whether an ISO/IEC 15434 barcode may be unreadable due to non-representation of ASCII 30.
    ///   character.
    /// </param>
    /// <param name="potentialIsoIec15434EdiUnreadableFs">
    ///   A value indicating whether EDI data in an ISO/IEC 15434 barcode may be unreadable due to non-representation of ASCII 28.
    /// </param>
    /// <param name="potentialIsoIec15434EdiUnreadableUs">
    ///   A value indicating whether EDI data in an ISO/IEC 15434 barcode may be unreadable due to non-representation of ASCII 31.
    /// </param>
    /// <param name="potentialIsoIec15434EotUnreadable">
    ///   A value indicating whether data in an ISO/IEC 15434 barcode may be unreadable due to non-representation of ASCII 04.
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
    internal TokenExtendedData(
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
        int scannerCharactersPerSecond,
        string aimFlagCharacterSequence,
        string reportedCharacters,
        bool potentialIsoIec15434Unreadable30,
        bool potentialIsoIec15434EdiUnreadableFs,
        bool potentialIsoIec15434EdiUnreadableUs,
        bool potentialIsoIec15434EotUnreadable,
        bool assessFormat06Support,
        IDictionary<string, IList<string>> nonInvariantAmbiguities,
        IDictionary<string, IList<string>> invariantGs1Ambiguities,
        IList<string> nonInvariantUnrecognisedCharacters,
        IList<string> invariantGs1UnrecognisedCharacters) {
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
        ScannerCharactersPerSecond = scannerCharactersPerSecond;
        AimFlagCharacterSequence = aimFlagCharacterSequence;
        ReportedCharacters = reportedCharacters;
        PotentialIsoIec15434Unreadable30 = potentialIsoIec15434Unreadable30;
        PotentialIsoIec15434EdiUnreadableFs = potentialIsoIec15434EdiUnreadableFs;
        PotentialIsoIec15434EdiUnreadableUs = potentialIsoIec15434EdiUnreadableUs;
        PotentialIsoIec15434EotUnreadable = potentialIsoIec15434EotUnreadable;
        AssessFormat06Support = assessFormat06Support;
        NonInvariantAmbiguities = nonInvariantAmbiguities;
        InvariantGs1Ambiguities = invariantGs1Ambiguities;
        NonInvariantUnrecognisedCharacters = nonInvariantUnrecognisedCharacters;
        InvariantGs1UnrecognisedCharacters = invariantGs1UnrecognisedCharacters;
    }

    /// <summary>
    ///   Gets the first (flag) character. By default, this is "]".
    ///   If a dead key is used, the sequence will contain two characters.
    /// </summary>
    [JsonProperty("aimFlagCharacterSequence", Order = 0)]
    public string AimFlagCharacterSequence { get; init; }

    /// <summary>
    ///   Gets a dictionary of differences in reported and expected characters.
    /// </summary>
    [JsonProperty("characterMap", Order = 1)]
    public Dictionary<char, char> CharacterMap { get; init; }

    /// <summary>
    ///   Gets a dictionary of differences in reported and expected characters where the reported data uses dead keys.
    /// </summary>
    [JsonProperty("deadKeysMap", Order = 2)]
    public Dictionary<string, string> DeadKeysMap { get; init; }

    /// <summary>
    ///   Gets a dictionary of initially detected differences in reported and expected characters where the reported data
    ///   uses dead keys.
    /// </summary>
    [JsonProperty("deadKeyCharacterMap", Order = 3)]
    public Dictionary<string, char> DeadKeyCharacterMap { get; init; }

    /// <summary>
    ///   Gets a dictionary of the dead key fix up characters detected during baseline calibration.
    /// </summary>
    [JsonProperty("deadKeyFixUp", Order = 4)]
    public Dictionary<string, string> DeadKeyFixUp { get; init; }

    /// <summary>
    ///   Gets a dictionary of differences in reported and expected characters where the scanner keyboard layout uses dead
    ///   keys.
    /// </summary>
    [JsonProperty("scannerDeadKeysMap", Order = 5)]
    public Dictionary<string, string> ScannerDeadKeysMap { get; init; }

    /// <summary>
    ///   Gets a list of expected characters where the scanner keyboard layout key maps to an unassigned key on the computer
    ///   keyboard layout.
    /// </summary>
    [JsonProperty("scannerUnassignedKeys", Order = 6)]
    public IList<string> ScannerUnassignedKeys { get; init; }

    /// <summary>
    ///   Gets a dictionary of candidate ligatures.
    /// </summary>
    [JsonProperty("ligatureMap", Order = 7)]
    public Dictionary<string, char> LigatureMap { get; init; }

    /// <summary>
    ///   Gets a regular expression for matching reported characters.
    /// </summary>
    [JsonProperty("reportedCharacters", Order = 8)]
    public string ReportedCharacters { get; init; }

    /// <summary>
    ///   Gets any prefix observed during calibration.
    /// </summary>
    [JsonProperty("prefix", Order = 9)]
    public string Prefix { get; init; }

    /// <summary>
    ///   Gets any code between the AIM ID (if present) and the data observed during calibration.
    /// </summary>
    [JsonProperty("code", Order = 10)]
    public string Code { get; init; }

    /// <summary>
    ///   Gets any suffix observed during calibration.
    /// </summary>
    [JsonProperty("suffix", Order = 11)]
    public string Suffix { get; init; }

    /// <summary>
    ///   Gets any prefix reported during calibration.
    /// </summary>
    [JsonProperty("reportedPrefix", Order = 12)]
    public string ReportedPrefix { get; init; }

    /// <summary>
    ///   Gets any code between the AIM ID (if present) and the data reported during calibration.
    /// </summary>
    [JsonProperty("reportedCode", Order = 13)]
    public string ReportedCode { get; init; }

    /// <summary>
    ///   Gets any suffix reported during calibration.
    /// </summary>
    [JsonProperty("reportedSuffix", Order = 14)]
    public string ReportedSuffix { get; init; }

    /// <summary>
    ///   Gets the Unicode name of the keyboard script.
    /// </summary>
    /// <remarks>
    ///   The library can only detect the keyboard script by heuristic analysis of the
    ///   reported data. This value does not refer t the keyboard layout, but rather
    ///   to the characters that the keyboard predominantly represents.
    /// </remarks>
    [JsonProperty("keyboardScript", Order = 15)]
    public string KeyboardScript { get; init; }

    /// <summary>
    ///   Gets the 'Traffic Light' assessment of the performance of the barcode scanner keyboard input.
    /// </summary>
    [JsonProperty("scannerKeyboardPerformance", Order = 16)]
    public ScannerKeyboardPerformance ScannerKeyboardPerformance { get; init; }

    /// <summary>
    ///   Gets the performance of the barcode scanner in characters per second.
    /// </summary>
    [JsonProperty("scannerCharactersPerSecond", Order = 17)]
    public int ScannerCharactersPerSecond { get; init; }

    /// <summary>
    ///   Gets a list of characters that are not recognised by the scanner keyboard layout.
    /// </summary>
    [JsonProperty("unrecognisedKeys", Order = 18)]
    public IList<char> UnrecognisedKeys { get; init; }

    /// <summary>
    ///   Gets a value indicating whether an ISO/IEC 15434 barcode may be unreadable due to non-representation of ASCII 30.
    ///   character.
    /// </summary>
    [JsonProperty("potentialFormat06Unreadable30", Order = 19)]
    public bool PotentialIsoIec15434Unreadable30 { get; init; }

    /// <summary>
    ///   Gets a value indicating whether EDI data in an ISO/IEC 15434 barcode may be unreadable due to non-representation of ASCII 28.
    ///   character.
    /// </summary>
    [JsonProperty("potentialIsoIec15434EdiUnreadableFs", Order = 20)]
    public bool PotentialIsoIec15434EdiUnreadableFs { get; init; }

    /// <summary>
    ///   Gets a value indicating whether EDI data in an ISO/IEC 15434 barcode may be unreadable due to non-representation of ASCII 31.
    ///   character.
    /// </summary>
    [JsonProperty("potentialIsoIec15434EdiUnreadableUs", Order = 21)]
    public bool PotentialIsoIec15434EdiUnreadableUs { get; init; }

    /// <summary>
    ///   Gets a value indicating whether data in an ISO/IEC 15434 barcode may be unreadable due to non-representation of ASCII 04.
    ///   character.
    /// </summary>
    [JsonProperty("potentialIsoIec15434EotUnreadableUs", Order = 22)]
    public bool PotentialIsoIec15434EotUnreadable { get; init; }

    /// <summary>
    ///   Gets a value indicating whether to assess Format 06 and Format 05 support.
    /// </summary>
    [JsonProperty("testSupportForFormat06", Order = 23)]
    public bool AssessFormat06Support { get; init; }

    /// <summary>
    ///   Gets a dictionary of ambiguous non-invariant ASCII character sequences that map to a reported character.
    /// </summary>
    [JsonProperty("nonInvariantAmbiguities", Order = 24)]
    public IDictionary<string, IList<string>> NonInvariantAmbiguities { get; init; }

    /// <summary>
    ///   Gets a dictionary of ambiguous invariant or other character sequences that may be used in GS1-compliant barcodes.
    /// </summary>
    [JsonProperty("invariantGs1Ambiguities", Order = 25)]
    public IDictionary<string, IList<string>> InvariantGs1Ambiguities { get; init; }

    /// <summary>
    ///   Gets a list of unrecognised non-invariant character sequences.
    /// </summary>
    [JsonProperty("nonInvariantUnrecognisedCharacters", Order = 26)]
    public IList<string> NonInvariantUnrecognisedCharacters { get; init; }

    /// <summary>
    ///   Gets a list of unrecognised invariant or other character sequences that may be used in GS1-compliant barcodes.
    /// </summary>
    [JsonProperty("invariantGs1UnrecognisedCharacters", Order = 27)]
    public IList<string> InvariantGs1UnrecognisedCharacters { get; init; }

    /// <summary>
    ///   Initializes the token extended data from a JSON string representing the serialized data.
    /// </summary>
    /// <param name="json">A JSON string representing the serialized data.</param>
    /// <returns>Extended data for inclusion in the token.</returns>
    // ReSharper disable once UnusedMember.Global
    public static TokenExtendedData FromJson(string json) {
        if (string.IsNullOrWhiteSpace(json)) return new TokenExtendedData();

        var calibrationTokenExtendedData = JsonConvert.DeserializeObject<TokenExtendedData>(json);

        if (calibrationTokenExtendedData is null) return new TokenExtendedData();

        return new TokenExtendedData(
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
            calibrationTokenExtendedData.ScannerCharactersPerSecond,
            calibrationTokenExtendedData.AimFlagCharacterSequence,
            calibrationTokenExtendedData.ReportedCharacters,
            calibrationTokenExtendedData.PotentialIsoIec15434Unreadable30,
            calibrationTokenExtendedData.PotentialIsoIec15434EdiUnreadableFs,
            calibrationTokenExtendedData.PotentialIsoIec15434EdiUnreadableUs,
            calibrationTokenExtendedData.PotentialIsoIec15434EotUnreadable,
            calibrationTokenExtendedData.AssessFormat06Support,
            calibrationTokenExtendedData.NonInvariantAmbiguities,
            calibrationTokenExtendedData.InvariantGs1Ambiguities,
            calibrationTokenExtendedData.NonInvariantUnrecognisedCharacters,
            calibrationTokenExtendedData.InvariantGs1UnrecognisedCharacters);
    }

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
            new JsonSerializerSettings {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = new DataIgnoreEmptyEnumerableResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            });
}