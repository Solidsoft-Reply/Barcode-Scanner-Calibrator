// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalibrationToken.cs" company="Solidsoft Reply Ltd.">
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
// A token passed within the keyboard calibration code to represent the current calibration session.  The token
// provides calibration state and results.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

using ProcessFlow;
using Properties;
using DataMatrix;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

/// <summary>
///   A token passed within the keyboard calibration code to represent the current calibration session. The token
///   provides calibration state and results.
/// </summary>
public struct CalibrationToken : IEquatable<CalibrationToken>, IEnvironment<CalibrationToken>
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="CalibrationToken" /> struct.
    /// </summary>
    public CalibrationToken()
    {
        Errors = new List<CalibrationInformation>();
        Warnings = new List<CalibrationInformation>();
        Information = new List<CalibrationInformation>();
        LatestError = string.Empty;
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="CalibrationToken" /> struct.
    /// </summary>
    /// <param name="barcodeData">
    ///   The unsegmented barcode data for the current calibration barcode.
    /// </param>
    /// <param name="key">
    ///   The dead key currently being calibrated. Null indicates baseline calibration.
    /// </param>
    /// <param name="value">
    ///   The expected character for the current dead key being calibrated.
    /// </param>
    /// <param name="calibrationRemaining">
    ///   A count of the estimated number of calibrations that still need to be performed during this session.
    /// </param>
    /// <param name="smallBarcodeSequenceIndex">
    ///   The index of the current small barcode in a sequence.
    /// </param>
    /// <param name="smallBarcodeSequenceCount">
    ///   The number of small barcodes that encode the current calibration data.
    /// </param>
    /// <param name="smallBarcodeSequencePrefix">
    ///   The prefix for each small barcode in a sequence.
    /// </param>
    /// <param name="reportedCharacters">
    ///   The reported characters for the current calibration barcode.
    /// </param>
    /// <param name="bitmapStream">
    ///   The stream containing the bitmap image of the current calibration barcode.
    /// </param>
    /// <param name="remaining">
    ///   A count of the estimated number of barcodes that will be generated during this session.
    /// </param>
    /// <param name="size">
    ///   The size of data matrix required.
    /// </param>
    /// <param name="keyboardMatch">
    ///   Indicates whether the scanner and computer keyboard layouts correspond.
    /// </param>
    /// <param name="calibrationData">
    ///   The Calibration configuration.
    /// </param>
    /// <param name="systemCapabilities">
    ///   The system capabilities and advice items.
    /// </param>
    /// <param name="calibrationSessionAbandoned">
    ///   Indicates whether the calibration session was abandoned.
    /// </param>
    /// <param name="reportedPrefixSegment">The reported prefix segment.</param>
    /// <param name="reportedSuffix">The reported suffix segment.</param>
    public CalibrationToken(
        string barcodeData,
        string? key = null,
        char value = default,
        int calibrationRemaining = -1,
        int smallBarcodeSequenceIndex = -1,
        int smallBarcodeSequenceCount = -1,
        string? smallBarcodeSequencePrefix = "",
        string? reportedCharacters = "",
        Stream? bitmapStream = null,
        int remaining = -1,
        DataMatrixSize size = DataMatrixSize.Automatic,
        bool? keyboardMatch = null,
        CalibrationData? calibrationData = null,
        SystemCapabilities? systemCapabilities = null,
        bool? calibrationSessionAbandoned = false,
        List<string>? reportedPrefixSegment = null,
        string? reportedSuffix = "")
    {
        Data = new CalibrationTokenData(
            barcodeData,
            key,
            value,
            calibrationRemaining,
            smallBarcodeSequenceIndex,
            smallBarcodeSequenceCount,
            smallBarcodeSequencePrefix ?? string.Empty,
            reportedCharacters ?? string.Empty);
        BitmapStream = bitmapStream;
        Remaining = remaining;
        Size = size;
        KeyboardMatch = keyboardMatch;
        CalibrationData = calibrationData;
        SystemCapabilities = systemCapabilities;
        Errors = new List<CalibrationInformation>();
        Warnings = new List<CalibrationInformation>();
        Information = new List<CalibrationInformation>();
        ExtendedData = null;
        LatestError = string.Empty;
        CalibrationSessionAbandoned = calibrationSessionAbandoned;
        ReportedPrefixSegment = reportedPrefixSegment;
        ReportedSuffix = reportedSuffix;
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="CalibrationToken" /> struct by cloning an existing token.
    /// </summary>
    /// <param name="oldToken">
    ///   The unsegmented barcode data for the current calibration barcode.
    /// </param>
    /// <param name="extendedData">Optional extended data to add to the token.</param>
    public CalibrationToken(CalibrationToken oldToken, CalibrationTokenExtendedData? extendedData = null)
    {
        Data = oldToken.Data is null
                        ? null
                        : new CalibrationTokenData(
                            oldToken.Data?.BarcodeData ?? string.Empty,
                            oldToken.Data?.Key,
                            oldToken.Data?.Value ?? char.MinValue,
                            oldToken.Data?.CalibrationsRemaining ?? 0,
                            oldToken.Data?.SmallBarcodeSequenceIndex ?? 0,
                            oldToken.Data?.SmallBarcodeSequenceCount ?? 0,
                            oldToken.Data?.SmallBarcodeSequencePrefix ?? string.Empty,
                            oldToken.Data?.ReportedCharacters ?? string.Empty);

        BitmapStream = oldToken.BitmapStream is null
                                ? null
                                : new MemoryStream(((MemoryStream)oldToken.BitmapStream).ToArray()) { Position = 0 };

        Remaining = oldToken.Remaining;
        Size = oldToken.Size;
        KeyboardMatch = oldToken.KeyboardMatch;

        CalibrationData = oldToken.CalibrationData is null
                                   ? null
                                   : new CalibrationData(
                                       oldToken.CalibrationData.AimFlagCharacterSequence,
                                       new Dictionary<char, char>(),
                                       new Dictionary<string, string>(),
                                       new Dictionary<string, char>(),
                                       new Dictionary<string, char>(),
                                       new Dictionary<string, string>(),
                                       new List<string>(),
                                       oldToken.CalibrationData.ReportedCharacters,
                                       oldToken.CalibrationData.ReportedPrefix,
                                       oldToken.CalibrationData.ReportedCode,
                                       oldToken.CalibrationData.ReportedSuffix,
                                       oldToken.CalibrationData.KeyboardScript,
                                       oldToken.CalibrationData.ScannerKeyboardPerformance,
                                       oldToken.CalibrationData.LineFeedCharacter);

        if (CalibrationData is not null)
        {
            foreach (var (key, value) in oldToken.CalibrationData?.CharacterMap ?? new Dictionary<char, char>())
            {
                CalibrationData.CharacterMap?.Add(key, value);
            }

            foreach (var (key, value) in oldToken.CalibrationData?.DeadKeysMap ?? new Dictionary<string, string>())
            {
                CalibrationData.DeadKeysMap?.Add(key, value);
            }

            foreach (var (key, value) in oldToken.CalibrationData?.DeadKeyCharacterMap ?? new Dictionary<string, char>())
            {
                CalibrationData.DeadKeyCharacterMap?.Add(key, value);
            }

            foreach (var (key, value) in oldToken.CalibrationData?.LigatureMap ?? new Dictionary<string, char>())
            {
                CalibrationData.LigatureMap?.Add(key, value);
            }

            foreach (var (key, value) in oldToken.CalibrationData?.ScannerDeadKeysMap ?? new Dictionary<string, string>())
            {
                CalibrationData.ScannerDeadKeysMap?.Add(key, value);
            }

            foreach (var value in oldToken.CalibrationData?.ScannerUnassignedKeys  ?? new List<string>())
            {
                CalibrationData.ScannerUnassignedKeys?.Add(value);
            }
        }

        SystemCapabilities = oldToken.SystemCapabilities is null
                                      ? null
                                      : new SystemCapabilities(
                                          oldToken.SystemCapabilities.TestsSucceeded,
                                          oldToken.SystemCapabilities.DataReported,
                                          oldToken.SystemCapabilities.CorrectSequenceReported,
                                          oldToken.SystemCapabilities.CompleteDataReported,
                                          oldToken.SystemCapabilities.KeyboardLayoutsCorrespond,
                                          oldToken.SystemCapabilities.KeyboardLayoutsCorrespondForInvariants,
                                          oldToken.SystemCapabilities.KeyboardLayoutsCorrespondForNonInvariantCharacters,
                                          oldToken.SystemCapabilities.KeyboardLayoutsCanRepresentGroupSeparator,
                                          oldToken.SystemCapabilities.KeyboardLayoutsCanRepresentRecordSeparator,
                                          oldToken.SystemCapabilities.KeyboardLayoutsCanRepresentEdiSeparators,
                                          oldToken.SystemCapabilities.KeyboardLayoutsCorrespondForAimIdentifier,
                                          oldToken.SystemCapabilities.CanReadInvariantsReliably,
                                          oldToken.SystemCapabilities.CanReadFormat05AndFormat06Reliably,
                                          oldToken.SystemCapabilities.CanReadEdiReliably,
                                          oldToken.SystemCapabilities.CanReadGroupSeparatorReliably,
                                          oldToken.SystemCapabilities.CanReadRecordSeparatorReliably,
                                          oldToken.SystemCapabilities.CanReadFileSeparatorsReliably,
                                          oldToken.SystemCapabilities.CanReadUnitSeparatorsReliably,
                                          oldToken.SystemCapabilities.CanReadAimIdentifiersReliably,
                                          oldToken.SystemCapabilities.CanReadAdditionalAsciiCharactersReliably,
                                          oldToken.SystemCapabilities.ScannerTransmitsAimIdentifiers,
                                          oldToken.SystemCapabilities.ScannerTransmitsEndOfLineSequence,
                                          oldToken.SystemCapabilities.ScannerTransmitsAdditionalPrefix,
                                          oldToken.SystemCapabilities.ScannerTransmitsAdditionalCode,
                                          oldToken.SystemCapabilities.ScannerTransmitsAdditionalSuffix,
                                          oldToken.SystemCapabilities.ScannerMayConvertToUpperCase,
                                          oldToken.SystemCapabilities.ScannerMayConvertToLowerCase,
                                          oldToken.SystemCapabilities.KeyboardScriptDoesNotSupportCase,
                                          oldToken.SystemCapabilities.CapsLockIndicator,
                                          oldToken.SystemCapabilities.ScannerKeyboardPerformance,
                                          oldToken.SystemCapabilities.FormatnnSupportAssessed,
                                          oldToken.SystemCapabilities.AimIdentifier,
                                          oldToken.SystemCapabilities.AimIdentifierUncertain,
                                          oldToken.SystemCapabilities.EndOfLineSequence,
                                          oldToken.SystemCapabilities.AdditionalPrefix,
                                          oldToken.SystemCapabilities.AdditionalCode,
                                          oldToken.SystemCapabilities.AdditionalSuffix,
                                          oldToken.SystemCapabilities.KeyboardScript,
                                          oldToken.SystemCapabilities.Platform,
                                          oldToken.SystemCapabilities.DeadKeys,
                                          oldToken.SystemCapabilities.CharacterMappings,
                                          oldToken.SystemCapabilities.DeadKeyMappings,
                                          oldToken.SystemCapabilities.Ambiguities,
                                          oldToken.SystemCapabilities.UnrecognisedCharacters,
                                          oldToken.SystemCapabilities.LigatureMappings,
                                          oldToken.SystemCapabilities.CalibrationAssumption);

        if (SystemCapabilities is not null && oldToken.SystemCapabilities is not null)
            SystemCapabilities.CapsLock = oldToken.SystemCapabilities.CapsLock;

        Errors = new List<CalibrationInformation>();
        Warnings = new List<CalibrationInformation>();
        Information = new List<CalibrationInformation>();
        ExtendedData = oldToken.ExtendedData is null && 
                            extendedData is null
                                ? null
                                : extendedData ?? new CalibrationTokenExtendedData(
                                      new Dictionary<string, string>(),
                                      new Dictionary<string, char>(),
                                      new Dictionary<string, string>(),
                                      new Dictionary<string, string>(),
                                      new List<string>(),
                                      new Dictionary<char, char>(),
                                      new Dictionary<string, char>(),
                                      new List<char>(),
                                      oldToken.ExtendedData?.Prefix ?? string.Empty,
                                      oldToken.ExtendedData?.Code ?? string.Empty,
                                      oldToken.ExtendedData?.Suffix ?? string.Empty,
                                      oldToken.ExtendedData?.ReportedPrefix ?? string.Empty,
                                      oldToken.ExtendedData?.ReportedCode ?? string.Empty,
                                      oldToken.ExtendedData?.ReportedSuffix ?? string.Empty,
                                      oldToken.ExtendedData?.KeyboardScript ?? string.Empty,
                                      oldToken.ExtendedData?.ScannerKeyboardPerformance ?? ScannerKeyboardPerformance.High,
                                      oldToken.ExtendedData?.AimFlagCharacterSequence ?? string.Empty,
                                      oldToken.ExtendedData?.ReportedCharacters ?? string.Empty,
                                      oldToken.ExtendedData?.PotentialIsoIec15434Unreadable30 ?? false,
                                      oldToken.ExtendedData?.PotentialIsoIec15434EdiUnreadableFs ?? false,
                                      oldToken.ExtendedData?.PotentialIsoIec15434EdiUnreadableUs ?? false,
                                      oldToken.ExtendedData?.AssessFormat06Support ?? false,
                                      oldToken.ExtendedData?.NonInvariantAmbiguities ?? new Dictionary<string, IList<string>>(),
                                      oldToken.ExtendedData?.InvariantGs1Ambiguities ?? new Dictionary<string, IList<string>>(),
                                      oldToken.ExtendedData?.NonInvariantUnrecognisedCharacters ?? new List<string>(),
                                      oldToken.ExtendedData?.InvariantGs1UnrecognisedCharacters ?? new List<string>());

        if (extendedData is null && oldToken.ExtendedData is not null && ExtendedData is not null)
        {
            foreach (var (key, value) in oldToken.ExtendedData.DeadKeysMap)
            {
                ExtendedData.DeadKeysMap.Add(key, value);
            }

            foreach (var (key, value) in oldToken.ExtendedData.DeadKeyCharacterMap)
            {
                ExtendedData.DeadKeyCharacterMap.Add(key, value);
            }

            foreach (var (key, value) in oldToken.ExtendedData.DeadKeyFixUp)
            {
                ExtendedData.DeadKeyFixUp.Add(key, value);
            }

            foreach (var (key, value) in oldToken.ExtendedData.ScannerDeadKeysMap)
            {
                ExtendedData.ScannerDeadKeysMap.Add(key, value);
            }

            foreach (var value in oldToken.ExtendedData.ScannerUnassignedKeys)
            {
                ExtendedData.ScannerUnassignedKeys.Add(value);
            }

            foreach (var (key, value) in oldToken.ExtendedData.CharacterMap)
            {
                ExtendedData.CharacterMap.Add(key, value);
            }

            foreach (var (key, value) in oldToken.ExtendedData.LigatureMap)
            {
                ExtendedData.LigatureMap.Add(key, value);
            }

            foreach (var value in oldToken.ExtendedData.UnrecognisedKeys)
            {
                ExtendedData.UnrecognisedKeys.Add(value);
            }
        }

        ReportedPrefixSegment = oldToken.ReportedPrefixSegment;
        ReportedSuffix = oldToken.ReportedSuffix;
        LatestError = string.Empty;

        foreach (var error in oldToken.Errors)
        {
            AddInformation(new CalibrationInformation(error.Level, error.InformationType, error.Description));
        }

        foreach (var warning in oldToken.Warnings)
        {
            AddInformation(new CalibrationInformation(warning.Level, warning.InformationType, warning.Description));
        }

        foreach (var info in oldToken.Information)
        {
            AddInformation(new CalibrationInformation(info.Level, info.InformationType, info.Description));
        }

        CalibrationSessionAbandoned = oldToken.CalibrationSessionAbandoned;
    }

    /// <summary>
    ///   Gets a count of the number of barcodes that remain to be calibrated during a calibration session.
    /// </summary>
    [JsonProperty("remaining", Order = 0)]
    public int Remaining { get; private set; }

    /// <summary>
    ///   Gets the maximum characters allowed in a barcode image.
    /// </summary>
    [JsonProperty("size", Order = 1)]
    public DataMatrixSize Size { get; private set; }

    /// <summary>
    ///   Gets a value indicating whether the scanner emulates a keyboard that corresponds with the current computer
    ///   keyboard layout.
    /// </summary>
    /// <remarks>
    ///   When true, this property cannot be assumed to indicate that the scanner and computer keyboard layouts are
    ///   identical, but simply that the keys representing ASCII characters are the same in both layouts. Another
    ///   possibility is that the scanner is emulating a numeric keyboard.
    /// </remarks>
    [JsonProperty("keyboardMatch", Order = 2)]
    public bool? KeyboardMatch { get; internal set; }

    /// <summary>
    ///   Gets the stream containing the bitmap image of the current calibration barcode.
    /// </summary>
    [JsonIgnore]
    public Stream? BitmapStream { get; }

    /// <summary>
    ///   Gets the system capabilities and advice items.
    /// </summary>
    [JsonProperty("systemCapabilities", Order = 3)]
    public SystemCapabilities? SystemCapabilities { get; private set; }

    /// <summary>
    ///   Gets the collection or calibration errors.
    /// </summary>
    [JsonProperty("errors", Order = 4)]
    public IEnumerable<CalibrationInformation> Errors { get; private set; }

    /// <summary>
    ///   Gets the collection of calibration warnings.
    /// </summary>
    [JsonProperty("warnings", Order = 5)]
    public IEnumerable<CalibrationInformation> Warnings { get; private set; }

    /// <summary>
    ///   Gets the collection of calibration information.
    /// </summary>
    [JsonProperty("information", Order = 6)]
    public IEnumerable<CalibrationInformation> Information { get; private set; }

    /// <summary>
    ///   Gets data for tokens that is primarily intended for internal calibration use only.
    /// </summary>c
    [JsonProperty("data", Order = 7)]
    public CalibrationTokenData? Data { get; private set; }

    /// <summary>
    ///   Gets the Calibration configuration.
    /// </summary>
    [JsonProperty("calibrationData", Order = 8)]
    public CalibrationData? CalibrationData { get; private set; }

    /// <summary>
    ///   Gets extended token data that must be used in stateless interactions.
    /// </summary>
    [JsonProperty("extendedData", Order = 9)]
    public CalibrationTokenExtendedData? ExtendedData { get; private set; }

    /// <summary>
    ///   Gets a value indicating whether the calibration session has been abandoned.
    /// </summary>
    [JsonProperty("calibrationSessionAbandoned", Order = 10)]
    public bool? CalibrationSessionAbandoned { get; private set; }

    /// <summary>
    ///   Gets a list of reported prefix segments.
    /// </summary>
    [JsonProperty("reportedPrefixSegment", Order = 11)]
    public List<string>? ReportedPrefixSegment { get; private set; }

    /// <summary>
    ///   Gets a reported suffix.
    /// </summary>
    [JsonProperty("reportedSuffix", Order = 12)]
    public string? ReportedSuffix { get; private set; }

    /// <summary>
    ///   Gets the latest serialization or deserialization error.
    /// </summary>
    [JsonIgnore]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public string LatestError { get; private set; }

    /// <summary>
    ///   Initializes the token data from a JSON string representing the serialized data.
    /// </summary>
    /// <param name="json">A JSON string representing the serialized data.</param>
    // ReSharper disable once UnusedMember.Global
    public static CalibrationToken FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;

        var calibrationToken = JsonConvert.DeserializeObject<CalibrationToken>(json);

        return new CalibrationToken(
            calibrationToken.Data?.BarcodeData ?? string.Empty,
            calibrationToken.Data?.Key ?? string.Empty,
            calibrationToken.Data?.Value ?? char.MinValue,
            calibrationToken.Data?.CalibrationsRemaining ?? 0,
            calibrationToken.Data?.SmallBarcodeSequenceIndex ?? 0,
            calibrationToken.Data?.SmallBarcodeSequenceCount ?? 0,
            calibrationToken.Data?.SmallBarcodeSequencePrefix ?? string.Empty,
            calibrationToken.Data?.ReportedCharacters ?? string.Empty,
            calibrationToken.BitmapStream,
            calibrationToken.Remaining,
            calibrationToken.Size,
            calibrationToken.KeyboardMatch,
            calibrationToken.CalibrationData,
            calibrationToken.SystemCapabilities,
            calibrationToken.CalibrationSessionAbandoned,
            calibrationToken.ReportedPrefixSegment,
            calibrationToken.ReportedSuffix);
    }

    /// <summary>
    ///   Returns an extended clone of the original token.
    /// </summary>
    /// <param name="token">The original token.</param>
    /// <param name="deadKeysMap">
    ///   A dictionary of differences in reported and expected characters where the reported data uses
    ///   dead keys.
    /// </param>
    /// <param name="deadKeyCharacterMap">
    ///   A dictionary of initially detected differences in reported and expected characters
    ///   where the reported data uses dead keys.
    /// </param>
    /// <param name="deadKeyFixUp">
    ///   A dictionary of dead key characters that must be fixed up due to reporting of different character
    ///   to the dead key character.
    /// </param>
    /// <param name="scannerDeadKeysMap">
    ///   A dictionary of differences in reported and expected characters where the scanner
    ///   keyboard layout uses dead keys.
    /// </param>
    /// <param name="scannerUnassignedKeys">
    ///   A list of expected characters where the scanner keyboard layout key maps to an unassigned key on the computer
    ///   keyboard layout.
    /// </param>
    /// <param name="characterMap">A dictionary of differences in reported and expected characters.</param>
    /// <param name="ligatureMap">A dictionary of candidate ligatures.</param>
    /// <param name="unrecognisedKeys">A list of characters that are not recognised by the scanner keyboard layout.</param>
    /// <param name="prefix">Any prefix observed during calibration.</param>
    /// <param name="code">Any code between the AIM ID (if present) and the data observed during calibration.</param>
    /// <param name="suffix">Any suffix observed during calibration.</param>
    /// <param name="reportedPrefix">Any prefix reported during calibration.</param>
    /// <param name="reportedCode">Any code between the AIM ID (if present) and the data reported during calibration.</param>
    /// <param name="reportedSuffix">Any suffix reported during calibration.</param>
    /// <param name="keyboardScript">The Unicode name of the keyboard script.</param>
    /// <param name="scannerKeyboardPerformance">The 'Traffic Light' assessment of the performance of the barcode scanner keyboard input.</param>
    /// <param name="aimFlagCharacterSequence">
    ///   The first (flag) character. By default this is "]". If a dead key is used, the
    ///   sequence will contain two characters.
    /// </param>
    /// <param name="reportedCharacters">A regular expression for matching reported characters.</param>
    /// <param name="potentialIsoIec15434Unreadable30">
    ///   A value indicating whether a Format 05 or 06 barcode may be unreadable due
    ///   to non-representation of ASCII 30 character.
    /// </param>
    /// <param name="potentialIsoIec15434EdiUnreadableFs">
    ///   A value indicating whether an EDI barcode may be unreadable due
    ///   to non-representation of ASCII 28 characters.
    /// </param>
    /// <param name="potentialIsoIec15434EdiUnreadableUs">
    ///   A value indicating whether an EDI barcode may be unreadable due
    ///   to non-representation of ASCII 31 characters.
    /// </param>
    /// <param name="testSupportForFormat06">
    ///   A value indicating whether to test for Format 06 and Format 05 support.
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

    // ReSharper disable once StyleCop.SA1650
    public static CalibrationToken SetExtendedData(
        CalibrationToken token,
        IDictionary<string, string>? deadKeysMap,
        IDictionary<string, char>? deadKeyCharacterMap,
        IDictionary<string, string>? deadKeyFixUp,
        IDictionary<string, string>? scannerDeadKeysMap,
        IEnumerable<string>? scannerUnassignedKeys,
        IDictionary<char, char>? characterMap,
        IDictionary<string, char>? ligatureMap,
        IList<char>? unrecognisedKeys,
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
        bool potentialIsoIec15434Unreadable30,
        bool potentialIsoIec15434EdiUnreadableFs,
        bool potentialIsoIec15434EdiUnreadableUs,
        bool testSupportForFormat06,
        IDictionary<string, IList<string>> nonInvariantAmbiguities,
        IDictionary<string, IList<string>> invariantGs1Ambiguities,
        IList<string> nonInvariantUnrecognisedCharacters,
        IList<string> invariantGs1UnrecognisedCharacters)
    {
        var extendedData = new CalibrationTokenExtendedData(
            new Dictionary<string, string>(),
            new Dictionary<string, char>(),
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            new List<string>(),
            new Dictionary<char, char>(),
            new Dictionary<string, char>(),
            new List<char>(),
            prefix,
            code,
            suffix,
            reportedPrefix,
            reportedCode,
            reportedSuffix,
            keyboardScript,
            scannerKeyboardPerformance,
            aimFlagCharacterSequence,
            reportedCharacters,
            potentialIsoIec15434Unreadable30,
            potentialIsoIec15434EdiUnreadableFs,
            potentialIsoIec15434EdiUnreadableUs,
            testSupportForFormat06,
            nonInvariantAmbiguities,
            invariantGs1Ambiguities,
            nonInvariantUnrecognisedCharacters,
            invariantGs1UnrecognisedCharacters);

        if (deadKeysMap is not null)
        {
            foreach (var (key, value) in deadKeysMap)
            {
                extendedData.DeadKeysMap.Add(key, value);
            }
        }

        if (deadKeyCharacterMap is not null)
        {
            foreach (var (key, value) in deadKeyCharacterMap)
            {
                extendedData.DeadKeyCharacterMap.Add(key, value);
            }
        }

        if (deadKeyFixUp is not null)
        {
            foreach (var (key, value) in deadKeyFixUp)
            {
                extendedData.DeadKeyFixUp.Add(key, value);
            }
        }

        if (scannerDeadKeysMap is not null)
        {
            foreach (var (key, value) in scannerDeadKeysMap)
            {
                extendedData.ScannerDeadKeysMap.Add(key, value);
            }
        }

        if (scannerUnassignedKeys is not null)
        {
            foreach (var value in scannerUnassignedKeys)
            {
                extendedData.ScannerUnassignedKeys.Add(value);
            }
        }

        if (characterMap is not null)
        {
            foreach (var (key, value) in characterMap)
            {
                extendedData.CharacterMap.Add(key, value);
            }
        }

        if (ligatureMap is not null)
        {
            foreach (var (key, value) in ligatureMap)
            {
                extendedData.LigatureMap.Add(key, value);
            }
        }

        if (unrecognisedKeys is null)
        {
            return new CalibrationToken(token, extendedData);
        }

        foreach (var value in unrecognisedKeys)
        {
            extendedData.UnrecognisedKeys.Add(value);
        }

        return new CalibrationToken(token, extendedData);
    }

    /// <summary>
    ///   Override for the equality operator.
    /// </summary>
    /// <param name="token1">The first calibration token.</param>
    /// <param name="token2">The second calibration token.</param>
    /// <returns>True, if the calibration tokens are equal; otherwise false.</returns>
    public static bool operator ==(CalibrationToken token1, CalibrationToken token2) =>
        token1.Equals(token2);

    /// <summary>
    ///   Override for the inequality operator.
    /// </summary>
    /// <param name="token1">The first calibration token.</param>
    /// <param name="token2">The second calibration token.</param>
    /// <returns>True, if the calibration tokens are not equal; otherwise false.</returns>
    public static bool operator !=(CalibrationToken token1, CalibrationToken token2) =>
        !token1.Equals(token2);

    /// <summary>
    ///   Tests the equality of this token with another, based on the key values, only.
    /// </summary>
    /// <param name="other">The token to be tested.</param>
    /// <returns>True, if the calibration tokens are not equal; otherwise false.</returns>
    public readonly bool Equals(CalibrationToken other) =>
        Remaining.Equals(other.Remaining) &&
        Size.Equals(other.Size) &&
        Equals(BitmapStream, other.BitmapStream) &&
        Equals(SystemCapabilities, other.SystemCapabilities) &&
        Equals(Data, other.Data) &&
        Equals(CalibrationData, other.CalibrationData) &&
        Equals(ExtendedData, other.ExtendedData);

    /// <summary>
    ///   Tests the equality of this token with another.
    /// </summary>
    /// <param name="obj">The token to be tested.</param>
    /// <returns>True, if the calibration tokens are not equal; otherwise false.</returns>
    public readonly override bool Equals(object? obj) =>
        obj is CalibrationToken token && Equals(token);

    /// <summary>
    ///   Returns a hash value for the current token.
    /// </summary>
    /// <returns>The hash value.</returns>
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public readonly override int GetHashCode() =>
        Fnv.CreateHashFnv1A(
            Remaining,
            Size,
            BitmapStream,
            SystemCapabilities,
            Data,
            CalibrationData,
            ExtendedData);

    /// <summary>
    ///   Returns a JSON representation of the calibration token.
    /// </summary>
    /// <returns>A JSON representation of the calibration token.</returns>
    public readonly override string ToString() => ToJson();

    /// <summary>
    ///   Returns a JSON representation of the calibration token.
    /// </summary>
    /// <param name="formatting">Specifies the formatting to be applied to the JSON.</param>
    /// <returns>A JSON representation of the calibration token.</returns>

    // ReSharper disable once MemberCanBePrivate.Global
    public readonly string ToJson(Formatting formatting = Formatting.None) =>
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
    ///   Marks the abandonment of the current calibration session.
    /// </summary>
    internal CalibrationToken AbandonCalibrationSession()
    {
        CalibrationSessionAbandoned = true;
        return this;
    }

    /// <summary>
    ///   Adds an information record to the correct collection.
    /// </summary>
    /// <param name="level">The information level. This is error, warning or information.</param>
    /// <param name="type">The information type.</param>
    /// <param name="description">The description of the information.</param>
    internal void AddInformation(
        CalibrationInformationLevel level,
        CalibrationInformationType type,
        string? description) =>
        AddInformation(new CalibrationInformation(level, type, description));

    /// <summary>
    ///   Adds an information record to the correct collection.
    /// </summary>
    /// <param name="information">The calibration information.</param>
    internal void AddInformation(CalibrationInformation information)
    {
        switch (information.Level)
        {
            case CalibrationInformationLevel.Information:
                Information ??= new List<CalibrationInformation>();

                ((IList)Information).Add(information);
                break;
            case CalibrationInformationLevel.Warning:
                Warnings ??= new List<CalibrationInformation>();

                ((IList)Warnings).Add(information);
                break;
            case CalibrationInformationLevel.Error:
                Errors ??= new List<CalibrationInformation>();

                ((IList)Errors).Add(information);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(information), 
                    information, 
                    Resources.CalibrationInvalidInformationLevel);
        }
    }

    /// <summary>
    ///   Removes an information record from the correct collection.
    /// </summary>
    /// <param name="information"></param>
    internal void RemoveInformation(CalibrationInformation information)
    {
        switch (information.Level)
        {
            case CalibrationInformationLevel.Information:
                Information ??= new List<CalibrationInformation>();

                ((IList)Information).Remove(information);
                break;
            case CalibrationInformationLevel.Warning:
                Warnings ??= new List<CalibrationInformation>();

                ((IList)Warnings).Remove(information);
                break;
            case CalibrationInformationLevel.Error:
                Errors ??= new List<CalibrationInformation>();

                ((IList)Errors).Remove(information);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(information), 
                    information.Level, 
                    Resources.CalibrationInvalidInformationLevel);
        }
    }

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