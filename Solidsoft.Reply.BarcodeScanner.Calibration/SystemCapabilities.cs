// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemCapabilities.cs" company="Solidsoft Reply Ltd.">
//   (c) 2020-2024 Solidsoft Reply Ltd. All rights reserved.
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
// The capabilities of the combination of the barcode scanner and the system.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using Properties;

/// <summary>
///   The capabilities of the combination of the barcode scanner and the system.
/// </summary>
/// <param name="UnexpectedError">
///   Gets a value indicating whether an unexpected error occurred.
/// </param>
/// <param name="TestsSucceeded">
///   Gets a value indicating whether the tests succeeded.
/// </param>
/// <param name="DataReported">
///   Gets a value indicating whether data was reported for all scanned calibration barcodes.
/// </param>
/// <param name="CorrectSequenceReported">
///   Gets a value indicating whether correct sequence of calibration barcodes was scanned.
/// </param>
/// <remarks>
///   By 'correct sequence' we mean that a baseline barcode was scanned, followed by zero or more
///   dead key calibration barcodes. If this property is false, it means that the user scanned a
///   calibration barcode, but that they scanned a dead key barcode when they should have scanned
///   a baseline barcodes, or vice versa. Note that the user could have scanned dead key barcodes
///   in the wrong order. However, this will not be detected when setting this property.
/// </remarks>
/// <param name="CompleteDataReported">
///   Gets a value indicating whether complete data was reported for any calibration barcode.
/// </param>
/// <param name="KeyboardLayoutsCorrespond">
///   Gets a value indicating whether the barcode scanner and the computer keyboard correspond
///   for ASCII, Group separator characters and Record Separator characters. This property
///   indicates generally if any keyboard non-correspondence has been detected that could affect
///   reading of barcodes.
/// </param>
/// <remarks>
///   Even if the keyboard layouts do not correspond, it can still be possible to read barcodes
///   reliably using a map. See the MappingPossible property.
/// </remarks>
/// <param name="KeyboardLayoutsCorrespondForInvariants">
///   Gets a value indicating whether the barcode scanner and the computer keyboard correspond
///   for invariant characters.
/// </param>
/// <remarks>
///   Even if the keyboard layouts do not correspond, it can still be possible to read barcodes
///   reliably using a map. See the MappingPossible property.
/// </remarks>
/// <param name="KeyboardLayoutsCorrespondForNonInvariantCharacters">
///   Gets a value indicating whether the barcode scanner and the computer keyboard correspond
///   for non-invariant characters.
/// </param>
/// <remarks>
///   Even if the keyboard layouts do not correspond, it can still be possible to read additional
///   reliably using a map. See the MappingPossible property.
/// </remarks>
/// <param name="KeyboardLayoutsCanRepresentGroupSeparator">
///   Gets a value indicating whether the barcode scanner and the computer keyboard can represent
///   Group Separator control characters without mapping.
/// </param>
/// <remarks>
///   Because control characters are generally entered using Control sequences, and the scanner
///   keyboard layout may not implement corresponding control sequences to the equivalent computer
///   keyboard layout, we handle this independently of correspondence for invariant and
///   non-invariant characters. The only capability we can assert here is that the combination
///   of the barcode scanner and computer are capable of representing ASCII 29 characters directly.
///   This is weaker than layout correspondence, as we can't be sure of the exact sequence of
///   Windows messages that will be available to an application - only that there will be
///   sufficient information in those messages to explicitly record an ASCII
///   29 without the need for character-mapping.
/// </remarks>
/// <param name="KeyboardLayoutsCanRepresentRecordSeparator">
///   Gets a value indicating whether the barcode scanner and the computer keyboard can represent
///   Record Separator control characters without mapping.
/// </param>
/// <remarks>
///   Because control characters are generally entered using Control sequences, and the scanner
///   keyboard layout may not implement corresponding control sequences to the equivalent computer
///   keyboard layout, we handle this independently of correspondence for invariant and
///   non-invariant characters. The only capability we can assert here is that the combination
///   of the barcode scanner and computer are capable of representing ASCII 30 characters directly.
///   This is weaker than layout correspondence, as we can't be sure of the exact sequence of
///   Windows messages that will be available to an application - only that there will be
///   sufficient information in those messages to explicitly record an ASCII 30 without the need
///   for character-mapping.
/// </remarks>
/// <param name="KeyboardLayoutsCanRepresentEdiSeparators">
///   Gets a value indicating whether the barcode scanner and the computer keyboard can represent
///   EDI separator control characters without mapping.
/// </param>
/// <remarks>
///   Because control characters are generally entered using Control sequences, and the scanner
///   keyboard layout may not implement corresponding control sequences to the equivalent computer
///   keyboard layout, we handle this independently of correspondence for invariant and
///   non-invariant characters. The only capability we can assert here is that the combination
///   of the barcode scanner and computer are capable of representing ASCII 28 or ASCII 31 characters
///   directly. This is weaker than layout correspondence, as we can't be sure of the exact sequence 
///   of Windows messages that will be available to an application - only that there will be
///   sufficient information in those messages to explicitly record an ASCII 28 or ASCII 30 without 
///   the need for character-mapping.
/// </remarks>
/// <param name="KeyboardLayoutsCorrespondForAimIdentifier">
///   Gets a value indicating whether the barcode scanner and the computer keyboard correspond
///   for characters used in AIM identifiers.
/// </param>
/// <param name="CanReadInvariantsReliably">
///   Gets a value indicating whether it is possible to map from the barcode scanner
///   to the computer keyboard to read invariant-only barcodes reliably, using a mapping
///   if necessary.
/// </param>
/// <remarks>
///   If true, this indicates that invariant characters and ASCII 29 characters
///   can be read reliably. This implies that any FNC1 (GS1) barcode can be read. It
///   also implies that invariant-only data can be read in Format 06 and Format 05
///   barcodes as long as ASCII 30 and ASCII 04 characters can be read reliably. See
///   the CanReadFormat05AndFormat06Reliably property.
/// </remarks>
/// <param name="CanReadFormat05AndFormat06Reliably">
///   Gets a value indicating whether the calibrated system can read Format 05 and
///   Format 06 barcodes reliably, using a mapping if necessary.
/// </param>
/// <param name="CanReadEdiReliably">
///   Gets a value indicating whether the calibrated system can read EDI barcode data
///   formatted in accordance with ISO/IEC 15434 reliably, using a mapping if necessary.
/// </param>
/// <remarks>
/// <p></p>
/// <p>If the Format 06 and Format 05 tests where not run, the value is null.</p>
/// </remarks>
/// <param name="CanReadGroupSeparatorReliably">
///   Gets a value indicating whether the calibrated system can read Group Separator characters reliably, using a mapping if necessary.
/// </param>
/// <param name="CanReadRecordSeparatorReliably">
///   Gets a value indicating whether the calibrated system can read Record Separator characters reliably, using a mapping if necessary.
/// </param>
/// <param name="CanReadFileSeparatorsReliably">
///   Gets a value indicating whether the calibrated system can read File separator characters reliably, using a mapping if necessary.
/// </param>
/// <param name="CanReadUnitSeparatorsReliably">
///   Gets a value indicating whether the calibrated system can read Unit separator characters reliably, using a mapping if necessary.
/// </param>
/// <param name="CanReadAimIdentifiersReliably">
///   Gets a value indicating whether the calibrated system can read AIM identifiers reliably, using a mapping if necessary.
/// </param>
/// <param name="CanReadAdditionalAsciiCharactersReliably">
///   Gets a value indicating whether the calibrated system can read non-invariant characters reliably, using a mapping if necessary.
/// </param>
/// <param name="ScannerTransmitsAimIdentifiers">
///   Gets a value indicating whether the scanner transmits AIM identifiers.
/// </param>
/// <param name="ScannerTransmitsEndOfLineSequence">
///   Gets a value indicating whether the scanner transmits end-of-line sequences.
/// </param>
/// <param name="ScannerTransmitsAdditionalPrefix">
///   Gets a value indicating whether the scanner transmits additional prefixes.
/// </param>
/// <param name="ScannerTransmitsAdditionalCode">
///   Gets a value indicating whether the scanner transmits an additional code
///   between the AIM ID (if present) and the reported data.
/// </param>
/// <param name="ScannerTransmitsAdditionalSuffix">
///   Gets a value indicating whether the scanner transmits additional suffixes.
/// </param>
/// <param name="ScannerMayConvertToUpperCase">
///   Gets a value indicating whether the scanner may be configured to convert lower-case letters to upper case.
/// </param>
/// <param name="ScannerMayConvertToLowerCase">
///   Gets a value indicating whether the scanner may be configured to convert upper-case letters to lower case.
/// </param>
/// <param name="ScannerKeyboardPerformance">
///   Gets the time span specifying how long it took from the start of the scan to submitting the data.
/// </param>
/// <remarks>
///   This cannot be determined with 100% certainty.
/// </remarks>
/// <param name="KeyboardScriptDoesNotSupportCase">
///   Gets a value indicating whether the computer keyboard script does not support upper- and lower-case letters.
/// </param>
/// <param name="AimIdentifier">
///   Gets the AIM identifier transmitted during calibration.
/// </param>
/// <param name="AimIdentifierUncertain">
///   Gets a value indicating whether there is uncertainty about the detected AIM identifier.
/// </param>
/// <param name="EndOfLineSequence">
///   Gets the end-of-line sequence transmitted during calibration.
/// </param>
/// <param name="AdditionalPrefix">
///   Gets the additional prefix transmitted during calibration.
/// </param>
/// <param name="AdditionalCode">
///   Gets the additional code between the AIM ID (if present) and the data transmitted during calibration.
/// </param>
/// <param name="AdditionalSuffix">
///   Gets the additional suffix transmitted during calibration.
/// </param>
/// <param name="KeyboardScript">
///   Gets the computer keyboard script.
/// </param>
/// <param name="Platform">
///   Gets the computer platform (operating system).
/// </param>
/// <param name="FormatnnSupportAssessed">
///   Gets a value indicating whether calibration included tests for Format nn support.
/// </param>
/// <param name="CapsLockIndicator">
///   Gets a value indicating whether the calibrator determined heuristically if the Caps Lock
///   key appears to be on or off.
/// </param>
/// <param name="DeadKeys">
///   Gets a value indicating whether dead key barcodes are required for calibration. 
/// </param>
/// <param name="Assumption">
///   Gets the assumption made concerning the use of calibration in client systems.
/// </param>
/// <param name="Ambiguities">
///   Gets a list of ambiguous character sequences that map to a reported character.
/// </param>
/// <param name="LigatureMappings">
///   Gets a list of ligature character sequences that map to a reported character.
/// </param>
/// <param name="UnrecognisedCharacters">
///   Gets a list of unrecognised characters.
/// </param>
public sealed record SystemCapabilities (
    [property: JsonProperty("unexpectedError", Order = 0)]
    bool UnexpectedError = false,

    [property: JsonProperty("testsSucceeded", Order = 1)]
    bool TestsSucceeded = true,

    [property: JsonProperty("dataReported", Order = 2)]
    bool DataReported = true,

    [property: JsonProperty("correctSequenceReported", Order = 3)]
    bool CorrectSequenceReported = true,

    [property: JsonProperty("completeDataReported", Order = 4)]
    bool CompleteDataReported = true,

    [property: JsonProperty("keyboardLayoutsCorrespond", Order = 5)]
    bool? KeyboardLayoutsCorrespond = true,

    [property: JsonProperty("keyboardLayoutsCorrespondForInvariants", Order = 6)]
    bool? KeyboardLayoutsCorrespondForInvariants = true,

    [property: JsonProperty("keyboardLayoutsCorrespondForNonInvariantCharacters", Order = 7)]
    bool? KeyboardLayoutsCorrespondForNonInvariantCharacters = true,

    [property: JsonProperty("keyboardLayoutsCanRepresentGroupSeparator", Order = 8)]
    bool? KeyboardLayoutsCanRepresentGroupSeparator = true,

    [property: JsonProperty("keyboardLayoutsCanRepresentRecordSeparator", Order = 9)]
    bool? KeyboardLayoutsCanRepresentRecordSeparator = true,

    [property: JsonProperty("keyboardLayoutsCanRepresentEdiSeparator", Order = 10)]
    bool? KeyboardLayoutsCanRepresentEdiSeparators = true,

    [property: JsonProperty("keyboardLayoutsCorrespondForAimIdentifier", Order = 11)]
    bool? KeyboardLayoutsCorrespondForAimIdentifier = true,

    [property: JsonProperty("canReadInvariantsReliably", Order = 12)]
    bool? CanReadInvariantsReliably = true,

    [property: JsonProperty("canReadFormat05AndFormat06Reliably", Order = 13)]
    bool? CanReadFormat05AndFormat06Reliably = true,

    [property: JsonProperty("canReadEdiReliably", Order = 14)]
    bool? CanReadEdiReliably = true,

    [property: JsonProperty("canReadGroupSeparatorReliably", Order = 15)]
    bool? CanReadGroupSeparatorReliably = true,

    [property: JsonProperty("canReadRecordSeparatorReliably", Order = 16)]
    bool? CanReadRecordSeparatorReliably = true,

    [property: JsonProperty("canReadFileSeparatorsReliably", Order = 17)]
    bool? CanReadFileSeparatorsReliably = true,

    [property: JsonProperty("canReadUnitSeparatorsReliably", Order = 18)]
    bool? CanReadUnitSeparatorsReliably = true,

    [property: JsonProperty("canReadAimIdentifiersReliably", Order = 19)]
    bool? CanReadAimIdentifiersReliably = true,

    [property: JsonProperty("canReadAdditionalAsciiCharactersReliably", Order = 20)]
    bool? CanReadAdditionalAsciiCharactersReliably = true,

    [property: JsonProperty("scannerTransmitsAimIdentifiers", Order = 21)]
    bool? ScannerTransmitsAimIdentifiers = true,

    [property: JsonProperty("scannerTransmitsEndOfLineSequence", Order = 22)]
    bool? ScannerTransmitsEndOfLineSequence = true,

    [property: JsonProperty("scannerTransmitsAdditionalPrefix", Order = 23)]
    bool ScannerTransmitsAdditionalPrefix = false,

    [property: JsonProperty("scannerTransmitsAdditionalCode", Order = 24)]
    bool ScannerTransmitsAdditionalCode = false,

    [property: JsonProperty("scannerTransmitsAdditionalSuffix", Order = 25)]
    bool ScannerTransmitsAdditionalSuffix = false,

    [property: JsonProperty("scannerMayConvertToUpperCase", Order = 26)]
    bool? ScannerMayConvertToUpperCase = null,

    [property: JsonProperty("scannerMayConvertToLowerCase", Order = 27)]
    bool? ScannerMayConvertToLowerCase = null,

    [property: JsonProperty("keyboardScriptDoesNotSupportCase", Order = 28)]
    bool? KeyboardScriptDoesNotSupportCase = null,

    [property: JsonProperty("capsLockIndicator", Order = 29)]
    bool CapsLockIndicator = false,

    [property: JsonProperty("scannerKeyboardPerformance", Order = 30)]
    ScannerKeyboardPerformance ScannerKeyboardPerformance = ScannerKeyboardPerformance.High,

    [property: JsonProperty("formatnnSupportAssessed", Order = 31)]
    bool FormatnnSupportAssessed = false,

    [property: JsonProperty("aimIdentifier", Order = 32)]
    string? AimIdentifier = null,

    [property: JsonProperty("aimIdentifierUncertain", Order = 33)]
    bool AimIdentifierUncertain = false,

    [property: JsonProperty("endOfLineSequence", Order = 34)]
    string? EndOfLineSequence = null,

    [property: JsonProperty("additionalPrefix", Order = 35)]
    string AdditionalPrefix = "",

    [property: JsonProperty("additionalCode", Order = 36)]
    string AdditionalCode = "",

    [property: JsonProperty("additionalSuffix", Order = 37)]
    string AdditionalSuffix = "",

    [property: JsonProperty("keyboardScript", Order = 38)]
    string KeyboardScript = "",

    [property: JsonProperty("platform", Order = 39)]
    SupportedPlatform Platform = SupportedPlatform.Windows,

    [property: JsonProperty("deadKeys", Order = 40)]
    bool DeadKeys = false,

    [property: JsonProperty("characterMappings", Order = 41)]
    IList<CharacterMapping>? CharacterMappings = null,

    [property: JsonProperty("deadKeyMappings", Order = 42)]
    IList<DeadKeyMapping>? DeadKeyMappings = null,

    [property: JsonProperty("ambiguities", Order = 43)]
    IList<Ambiguity>? Ambiguities = null,

    [property: JsonProperty("unrecognisedCharacters", Order = 44)]
    IList<UnrecognisedCharacter>? UnrecognisedCharacters = null,

    [property: JsonProperty("ligatureMappings", Order = 45)]
    IList<LigatureMapping>? LigatureMappings = null,

    [property: JsonProperty("calibrationAssumption", Order = 46)]
    Assumption Assumption = Assumption.Agnostic)
     
: BaseRecord {
    /// <summary>
    ///   Indicates whether the keyboard Caps Lock key is on or off.
    /// </summary>
    private bool _keyboardCapsLock;

    /// <summary>
    ///   Initializes a new instance of the <see cref="SystemCapabilities"/> class.
    /// </summary>
    /// <param name="token">The calibration token.</param>
    /// <param name="assumption">The assumption made concerning the use of calibration in client systems.</param>
    /// <param name="capsLock">Indicates whether Caps Lock is switched on.</param>
    /// <param name="scannerKeyboardPerformance">'Traffic Light' assessment of the performance of the barcode scanner keyboard input.</param>
    /// <param name="formatnnSupportAssessed">Indicates whether calibration included tests for Format nn support.</param>
    /// <param name="deadKeys">Indicates whether dead key barcodes are required for calibration.</param>
    /// <param name="characterMap">A dictionary of differences in reported and expected characters.</param>
    /// <param name="deadKeyCharacterMap">A dictionary of initially detected differences in reported and expected characters where the reported data uses dead keys.</param>
    /// <param name="deadKeysMap">A dictionary of initially detected differences in reported and expected character sequences where the reported data uses dead keys.</param>
    /// <param name="invariantGs1Ambiguities">A dictionary of ambiguous invariant or other characters that may be used in GS1-compliant barcodes.</param>
    /// <param name="nonInvariantAmbiguities">A dictionary of ambiguous non-invariant characters that map to a reported character.</param>
    /// <param name="invariantGs1UnrecognisedCharacters"> A list of unrecognised invariant or other characters that may be used in GS1-compliant barcodes.</param>
    /// <param name="nonInvariantUnrecognisedCharacters">A list of unrecognised non-invariant characters.</param>
    /// <param name="ligatureMap">A list of ligature character sequences.</param>
    public SystemCapabilities(
        Token token,
        Assumption assumption = Assumption.Calibration,
        bool? capsLock = null,
        ScannerKeyboardPerformance scannerKeyboardPerformance = default,
        bool formatnnSupportAssessed = true,
        bool deadKeys = false,
        IDictionary<char, char>? characterMap = null,
        IDictionary<string, char>? deadKeyCharacterMap = null,
        IDictionary<string, string>? deadKeysMap = null,
        IDictionary<string, IList<string>>? invariantGs1Ambiguities = null,
        IDictionary<string, IList<string>>? nonInvariantAmbiguities = null,
        IEnumerable<string>? invariantGs1UnrecognisedCharacters = null,
        IEnumerable<string>? nonInvariantUnrecognisedCharacters = null,
        IDictionary<string, char>? ligatureMap = null) : this()
    {
        if (token == default)
        {
            return;
        }

        Assumption = assumption;
        FormatnnSupportAssessed = formatnnSupportAssessed;
        ScannerKeyboardPerformance = scannerKeyboardPerformance;
        DeadKeys = deadKeys;

        // Process information
        foreach (var info in token.Information)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (info.InformationType)
            {
                case InformationType.KeyboardScript:
                    KeyboardScript = ParameterValue(info);
                    break;
                case InformationType.GroupSeparatorSupported:
                    CanReadGroupSeparatorReliably = true;
                    break;
                case InformationType.RecordSeparatorSupported:
                    CanReadRecordSeparatorReliably = true;
                    break;
                case InformationType.FileSeparatorSupported:
                    CanReadFileSeparatorsReliably = true;
                    break;
                case InformationType.UnitSeparatorSupported:
                    CanReadUnitSeparatorsReliably = true;
                    break;
                case InformationType.ScannerMayCompensateForCapsLock:
                    ScannerMayCompensateForCapsLock = true;
                    break;
                case InformationType.Platform:
                    Platform = Enum.TryParse<SupportedPlatform>(
                                        ParameterValue(info),
                                        out var platform)
                                        ? platform
                                        : SupportedPlatform.Windows;
                    break;
                case InformationType.AimTransmitted:
                    AimIdentifier = ParameterValue(info);
                    break;
                case InformationType.AimMayBeTransmitted:
                    AimIdentifier = ParameterValue(info);
                    AimIdentifierUncertain = true;
                    break;
                case InformationType.EndOfLineTransmitted:
                    EndOfLineSequence = ParameterValue(info);
                    break;
                case InformationType.None:
                case InformationType.AimSupported:
                case InformationType.SomeNonInvariantCharactersUnreported:
                case InformationType.SomeNonInvariantCharactersUnrecognised:
                case InformationType.SomeNonInvariantCharacterCombinationsUnrecognised:
                case InformationType.IsoIec15434SyntaxNotRecognised:
                case InformationType.IsoIec15434EdiNotReliablyReadable:
                case InformationType.IsoIec15434RecordSeparatorMapping:
                case InformationType.AimNotTransmitted:
                case InformationType.AimNotRecognised:
                case InformationType.PrefixTransmitted:
                case InformationType.CodeTransmitted:
                case InformationType.SuffixTransmitted:
                case InformationType.EndOfLineNotTransmitted:
                case InformationType.MultipleKeysNonInvariantCharacters:
                case InformationType.MultipleKeysMultipleNonInvariantCharacters:
                case InformationType.MultipleKeysAimFlagCharacter:
                case InformationType.DeadKeyMultiMappingNonInvariantCharacters:
                case InformationType.ControlCharacterMappingIsoIec15434EdiNotReliablyReadable:
                case InformationType.ControlCharacterMappingAdditionalDataElements:
                case InformationType.NonInvariantCharacterSequence:
                case InformationType.NonCorrespondingKeyboardLayouts:
                case InformationType.NonCorrespondingKeyboardLayoutsForInvariants:
                case InformationType.NonCorrespondingKeyboardLayoutsForNonInvariantCharacters:
                case InformationType.NonCorrespondingKeyboardLayoutsGroupSeparator:
                case InformationType.NonCorrespondingKeyboardLayoutsRecordSeparator:
                case InformationType.NonCorrespondingKeyboardLayoutsEdiSeparators:
                case InformationType.NonCorrespondingKeyboardLayoutsForAimIdentifier:
                case InformationType.NonDeterminableKeyboardLayoutCorrespondence:
                case InformationType.CapsLockOn:
                case InformationType.CapsLockProbablyOn:
                case InformationType.ScannerMayConvertToUpperCase:
                case InformationType.ScannerMayConvertToLowerCase:
                case InformationType.ScannerMayInvertCase:
                case InformationType.SubOptimalScannerKeyboardPerformance:
                case InformationType.NoCalibrationDataReported:
                case InformationType.UnrecognisedData:
                case InformationType.TooManyCharactersDetected:
                case InformationType.PartialCalibrationDataReported:
                case InformationType.IncorrectCalibrationDataReported:
                case InformationType.UndetectedInvariantCharacters:
                case InformationType.SomeInvariantCharactersUnrecognised:
                case InformationType.SomeDeadKeyCombinationsUnrecognisedForInvariants:
                case InformationType.NoGroupSeparatorMapping:
                case InformationType.MultipleKeys:
                case InformationType.DeadKeyMultiMapping:
                case InformationType.DeadKeyMultipleKeys:
                case InformationType.MultipleSequences:
                case InformationType.MultipleSequencesForScannerDeadKey:
                case InformationType.IncompatibleScannerDeadKey:
                case InformationType.GroupSeparatorMapping:
                case InformationType.LigatureCharacters:
                case InformationType.NoDelimiters:
                case InformationType.NoTemporaryDelimiterCandidate:
                case InformationType.CalibrationFailed:
                case InformationType.CalibrationFailedUnexpectedly:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(token),
                        info.InformationType,
                        Resources.CalibrationIncorrectInformationalInformationType);
            }
        }

        foreach (var info in token.Warnings)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (info.InformationType)
            {
                case InformationType.SomeNonInvariantCharactersUnreported:
                case InformationType.SomeNonInvariantCharactersUnrecognised:
                case InformationType.SomeNonInvariantCharacterCombinationsUnrecognised:
                case InformationType.MultipleKeysMultipleNonInvariantCharacters:
                case InformationType.DeadKeyMultiMappingNonInvariantCharacters:
                case InformationType.ControlCharacterMappingAdditionalDataElements:
                case InformationType.NonInvariantCharacterSequence:
                    KeyboardLayoutsCorrespondForNonInvariantCharacters = false;
                    KeyboardLayoutsCorrespond = false;
                    CanReadAdditionalAsciiCharactersReliably = false;
                    break;
                case InformationType.IsoIec15434SyntaxNotRecognised:
                case InformationType.IsoIec15434RecordSeparatorMapping:
                    CanReadFormat05AndFormat06Reliably = false;
                    break;
                case InformationType.IsoIec15434EdiNotReliablyReadable:
                case InformationType.ControlCharacterMappingIsoIec15434EdiNotReliablyReadable:
                    CanReadEdiReliably = false;
                    break;
                case InformationType.AimNotTransmitted:
                    ScannerTransmitsAimIdentifiers = false;
                    break;
                case InformationType.AimNotRecognised:
                case InformationType.MultipleKeysAimFlagCharacter:
                    KeyboardLayoutsCorrespondForAimIdentifier = false;
                    KeyboardLayoutsCorrespond = false;
                    CanReadAimIdentifiersReliably = false;
                    break;
                case InformationType.PrefixTransmitted:
                    ScannerTransmitsAdditionalPrefix = true;
                    AdditionalPrefix = ParameterValue(info);
                    break;
                case InformationType.CodeTransmitted:
                    ScannerTransmitsAdditionalCode = true;
                    AdditionalCode = ParameterValue(info);
                    break;
                case InformationType.SuffixTransmitted:
                    ScannerTransmitsAdditionalSuffix = true;
                    AdditionalSuffix = ParameterValue(info);
                    break;
                case InformationType.EndOfLineNotTransmitted:
                    ScannerTransmitsEndOfLineSequence = false;
                    break;
                case InformationType.NonCorrespondingKeyboardLayouts:
                    KeyboardLayoutsCorrespond = false;
                    break;
                case InformationType.NonCorrespondingKeyboardLayoutsForInvariants:
                    KeyboardLayoutsCorrespondForInvariants = false;
                    KeyboardLayoutsCorrespond = false;
                    break;
                case InformationType
                   .NonCorrespondingKeyboardLayoutsForNonInvariantCharacters:
                    KeyboardLayoutsCorrespondForNonInvariantCharacters = false;
                    KeyboardLayoutsCorrespond = false;
                    break;
                case InformationType.NonCorrespondingKeyboardLayoutsGroupSeparator:
                    KeyboardLayoutsCanRepresentGroupSeparator = false;
                    KeyboardLayoutsCorrespond = false;
                    break;
                case InformationType.NonCorrespondingKeyboardLayoutsRecordSeparator:
                    KeyboardLayoutsCanRepresentRecordSeparator = false;
                    KeyboardLayoutsCorrespond = false;
                    break;
                case InformationType.NonCorrespondingKeyboardLayoutsEdiSeparators:
                    KeyboardLayoutsCanRepresentEdiSeparators = false;
                    KeyboardLayoutsCorrespond = false;
                    break;
                case InformationType.NonCorrespondingKeyboardLayoutsForAimIdentifier:
                    KeyboardLayoutsCorrespondForAimIdentifier = false;
                    KeyboardLayoutsCorrespond = false;
                    break;
                case InformationType.CapsLockProbablyOn:
                    CapsLockIndicator = true;
                    break;
                case InformationType.ScannerMayConvertToUpperCase:
                    ScannerMayConvertToUpperCase = true;
                    break;
                case InformationType.ScannerMayConvertToLowerCase:
                    ScannerMayConvertToLowerCase = true;
                    break;
                case InformationType.ScannerMayInvertCase:
                    ScannerMayInvertCase = true;
                    break;
                case InformationType.None:
                case InformationType.AimSupported:
                case InformationType.AimTransmitted:
                case InformationType.AimMayBeTransmitted:
                case InformationType.EndOfLineTransmitted:
                case InformationType.GroupSeparatorSupported:
                case InformationType.RecordSeparatorSupported:
                case InformationType.FileSeparatorSupported:
                case InformationType.UnitSeparatorSupported:
                case InformationType.ScannerMayCompensateForCapsLock:
                case InformationType.KeyboardScript:
                case InformationType.Platform:
                case InformationType.MultipleKeysNonInvariantCharacters:
                case InformationType.NonDeterminableKeyboardLayoutCorrespondence:
                case InformationType.CapsLockOn:
                case InformationType.SubOptimalScannerKeyboardPerformance:
                case InformationType.NoCalibrationDataReported:
                case InformationType.UnrecognisedData:
                case InformationType.TooManyCharactersDetected:
                case InformationType.PartialCalibrationDataReported:
                case InformationType.IncorrectCalibrationDataReported:
                case InformationType.UndetectedInvariantCharacters:
                case InformationType.SomeInvariantCharactersUnrecognised:
                case InformationType.SomeDeadKeyCombinationsUnrecognisedForInvariants:
                case InformationType.NoGroupSeparatorMapping:
                case InformationType.MultipleKeys:
                case InformationType.DeadKeyMultiMapping:
                case InformationType.DeadKeyMultipleKeys:
                case InformationType.MultipleSequences:
                case InformationType.MultipleSequencesForScannerDeadKey:
                case InformationType.IncompatibleScannerDeadKey:
                case InformationType.GroupSeparatorMapping:
                case InformationType.LigatureCharacters:
                case InformationType.NoDelimiters:
                case InformationType.NoTemporaryDelimiterCandidate:
                case InformationType.CalibrationFailed:
                case InformationType.CalibrationFailedUnexpectedly:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(token),
                        info.InformationType,
                        Resources.CalibrationIncorrectWarningInformationType);
            }
        }

        foreach (var informationType in token.Errors.Select(info => info.InformationType))
        {
#pragma warning disable S907
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (informationType)
            {
                // We got a partial result, so retain all the knowledge we have gained.
                case InformationType.PartialCalibrationDataReported:
                    CompleteDataReported = false;
                    KeyboardLayoutsCorrespond = null;
                    KeyboardLayoutsCorrespondForInvariants = null;
                    KeyboardLayoutsCorrespondForNonInvariantCharacters = null;
                    CanReadInvariantsReliably = null;
                    CanReadFormat05AndFormat06Reliably = null;
                    CanReadEdiReliably = null;
                    CanReadAdditionalAsciiCharactersReliably = null;
                    break;
                case InformationType.IncorrectCalibrationDataReported:
                    CorrectSequenceReported = false;
                    KeyboardLayoutsCorrespond = null;
                    KeyboardLayoutsCorrespondForInvariants = null;
                    KeyboardLayoutsCorrespondForNonInvariantCharacters = null;
                    CanReadInvariantsReliably = null;
                    CanReadFormat05AndFormat06Reliably = null;
                    CanReadEdiReliably = null;
                    CanReadAdditionalAsciiCharactersReliably = null;
                    break;
                case InformationType.NoCalibrationDataReported:
                    DataReported = false;
                    goto case InformationType.UnrecognisedData;
                case InformationType.CalibrationFailedUnexpectedly:
                    UnexpectedError = true;
                    goto case InformationType.UnrecognisedData;
                case InformationType.UnrecognisedData:
                case InformationType.TooManyCharactersDetected:
                case InformationType.NoTemporaryDelimiterCandidate:
                case InformationType.NoDelimiters:
                    CompleteDataReported = false;
                    goto case InformationType.CalibrationFailed;
                case InformationType.CalibrationFailed:
                    TestsSucceeded = false;
                    KeyboardLayoutsCorrespond = null;
                    KeyboardLayoutsCorrespondForInvariants = null;
                    KeyboardLayoutsCorrespondForNonInvariantCharacters = null;
                    CanReadInvariantsReliably = null;
                    CanReadFormat05AndFormat06Reliably = null;
                    CanReadEdiReliably = null;
                    CanReadAdditionalAsciiCharactersReliably = null;
                    ScannerTransmitsAdditionalPrefix = false;
                    ScannerTransmitsAdditionalSuffix = false;
                    AdditionalPrefix = string.Empty;
                    AdditionalCode = string.Empty;
                    AdditionalSuffix = string.Empty;
                    KeyboardScript = string.Empty;
                    CorrectSequenceReported = true;
                    break;
                // The following all indicate that reliable scanning is not possible, even with mapping
                case InformationType.UndetectedInvariantCharacters:
                case InformationType.SomeInvariantCharactersUnrecognised:
                case InformationType.SomeDeadKeyCombinationsUnrecognisedForInvariants:
                case InformationType.NoGroupSeparatorMapping:
                case InformationType.MultipleKeys:
                case InformationType.DeadKeyMultiMapping:
                case InformationType.DeadKeyMultipleKeys:
                case InformationType.MultipleSequences:
                case InformationType.MultipleSequencesForScannerDeadKey:
                case InformationType.IncompatibleScannerDeadKey:
                case InformationType.GroupSeparatorMapping:
                case InformationType.LigatureCharacters:
                    KeyboardLayoutsCorrespond = false;
                    KeyboardLayoutsCorrespondForInvariants = false;
                    CanReadInvariantsReliably = false;
                    CanReadFormat05AndFormat06Reliably = false;
                    CanReadEdiReliably = false;
                    CanReadFileSeparatorsReliably = false;
                    CanReadUnitSeparatorsReliably = false;
                    break;
                case InformationType.None:
                case InformationType.AimSupported:
                case InformationType.AimTransmitted:
                case InformationType.AimMayBeTransmitted:
                case InformationType.EndOfLineTransmitted:
                case InformationType.GroupSeparatorSupported:
                case InformationType.RecordSeparatorSupported:
                case InformationType.FileSeparatorSupported:
                case InformationType.UnitSeparatorSupported:
                case InformationType.ScannerMayCompensateForCapsLock:
                case InformationType.KeyboardScript:
                case InformationType.Platform:
                case InformationType.SomeNonInvariantCharactersUnreported:
                case InformationType.SomeNonInvariantCharactersUnrecognised:
                case InformationType.SomeNonInvariantCharacterCombinationsUnrecognised:
                case InformationType.IsoIec15434SyntaxNotRecognised:
                case InformationType.IsoIec15434EdiNotReliablyReadable:
                case InformationType.IsoIec15434RecordSeparatorMapping:
                case InformationType.AimNotTransmitted:
                case InformationType.AimNotRecognised:
                case InformationType.PrefixTransmitted:
                case InformationType.CodeTransmitted:
                case InformationType.SuffixTransmitted:
                case InformationType.EndOfLineNotTransmitted:
                case InformationType.MultipleKeysNonInvariantCharacters:
                case InformationType.MultipleKeysMultipleNonInvariantCharacters:
                case InformationType.MultipleKeysAimFlagCharacter:
                case InformationType.DeadKeyMultiMappingNonInvariantCharacters:
                case InformationType.ControlCharacterMappingIsoIec15434EdiNotReliablyReadable:
                case InformationType.ControlCharacterMappingAdditionalDataElements:
                case InformationType.NonInvariantCharacterSequence:
                case InformationType.NonCorrespondingKeyboardLayouts:
                case InformationType.NonCorrespondingKeyboardLayoutsForInvariants:
                case InformationType.NonCorrespondingKeyboardLayoutsForNonInvariantCharacters:
                case InformationType.NonCorrespondingKeyboardLayoutsGroupSeparator:
                case InformationType.NonCorrespondingKeyboardLayoutsRecordSeparator:
                case InformationType.NonCorrespondingKeyboardLayoutsEdiSeparators:
                case InformationType.NonCorrespondingKeyboardLayoutsForAimIdentifier:
                case InformationType.NonDeterminableKeyboardLayoutCorrespondence:
                case InformationType.CapsLockOn:
                case InformationType.CapsLockProbablyOn:
                case InformationType.ScannerMayConvertToUpperCase:
                case InformationType.ScannerMayConvertToLowerCase:
                case InformationType.ScannerMayInvertCase:
                case InformationType.SubOptimalScannerKeyboardPerformance:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(token),
                        informationType,
                        Resources.CalibrationIncorrectErrorInformationType);
            }
#pragma warning restore S907
        }

        if (capsLock.GetValueOrDefault())
        {
            // Caps Lock is ON
            _keyboardCapsLock = true;
        }
        else
        {
            // Caps Lock is OFF or not reported
            if (capsLock is null)
            {
                // Caps Lock is not reported. We will assume that the caps lock indicator is accurate.
                _keyboardCapsLock = CapsLockIndicator;
                ScannerMayCompensateForCapsLock = null;
                ScannerMayInvertCase = null;
            }
            else
            {
                // Caps Lock is OFF.
                _keyboardCapsLock = false;
                ScannerMayCompensateForCapsLock = null;
            }
        }

        if (_keyboardCapsLock)
        {
            if (ScannerMayConvertToUpperCase ?? false)
            {
                ScannerMayConvertToUpperCase = false;
                ScannerMayConvertToLowerCase = true;

            }
            else if (ScannerMayConvertToLowerCase ?? false)
            {
                ScannerMayConvertToUpperCase = true;
                ScannerMayConvertToLowerCase = false;
            }
        }

        if ((ScannerMayConvertToUpperCase ?? false) || (ScannerMayConvertToLowerCase ?? false))
        {
            KeyboardLayoutsCorrespond = null;
            KeyboardLayoutsCorrespondForInvariants = null;
            KeyboardLayoutsCorrespondForAimIdentifier = null;
        }

        if (!string.IsNullOrWhiteSpace(KeyboardScript))
        {
            KeyboardScriptDoesNotSupportCase = KeyboardScript switch
            {
                "Latin" => false,
                "Greek" => false,
                "Cyrillic" => false,
                "Coptic" => false,
                "Armenian" => false,
                "Adlam" => false,
                "Warang Citi" => false,
                "Cherokee" => false,
                "Osage" => false,
                "Glagolitic" => false,
                "Deseret" => false,
                _ => true
            };
        }

        // Set up match data for justification
        CharacterMappings = characterMap?.Where(kvp => !(deadKeyCharacterMap?.Values.Contains(kvp.Value) ?? false)).Select(characterMapping =>
            new CharacterMapping(characterMapping.Value.ToControlPicture(), characterMapping.Key.ToControlPictureString(),
                CalibrationCharacterCategory(characterMapping.Value),
                false)).ToList();

        (CharacterMappings as List<CharacterMapping>)?.AddRange(
            deadKeyCharacterMap?.Select(deadKeyCharacterMapping =>
                new CharacterMapping(deadKeyCharacterMapping.Value.ToControlPicture(),
                    deadKeyCharacterMapping.Key[(deadKeyCharacterMapping.Key.LastIndexOf('\0') + 1)..].ToControlPictures(),
                    CalibrationCharacterCategory(deadKeyCharacterMapping.Value),
                    true)) ?? Array.Empty<CharacterMapping>());

        CharacterMappings =
            (from cm in CharacterMappings
             orderby cm.Expected
             select cm).ToList();

        // Set up dead key sequence match data for justification
        DeadKeyMappings = deadKeysMap?.Select(deadKeyMapping =>
            new DeadKeyMapping(
                deadKeyMapping.Value.ToControlPictures(), 
                deadKeyMapping.Key.ToControlPictures(),
                IsSequenceInvariantDataOrApplication(deadKeyMapping.Value, formatnnSupportAssessed))).ToList();

        DeadKeyMappings =
            (from dkm in DeadKeyMappings
             orderby dkm.Expected
             select dkm).ToList();

        // Set up ambiguity data for justification.
        Ambiguities = invariantGs1Ambiguities?
            .Select(ambiguity =>
                new Ambiguity(ambiguity.Value, ambiguity.Key, true, deadKeyCharacterMap?.ContainsKey(ambiguity.Key.Replace('\u2400', '\0')) ?? false)).ToList();

        // Create a list of non-invariant ambiguities that includes any additional variant characters
        // for the same reported character.
        var ambiguities = new Dictionary<string, IList<string>>();

        foreach (var (key, value) in nonInvariantAmbiguities ?? new Dictionary<string, IList<string>>())
        {
            if (invariantGs1Ambiguities?.TryGetValue(key, out var ambiguity) ?? false)
            {
                foreach (var ambiguityValue in ambiguity)
                {
                    if(!value.Contains(ambiguityValue)) value.Add(ambiguityValue);
                }
            }

            ambiguities.Add(key, value);
        }

        (Ambiguities as List<Ambiguity>)?.AddRange(
            ambiguities.Select(
                ambiguity =>
                    new Ambiguity(ambiguity.Value, ambiguity.Key, false, deadKeyCharacterMap?.ContainsKey(ambiguity.Key.Replace('\u2400', '\0')) ?? false)));

        // Set up unrecognised character data for justification.
        UnrecognisedCharacters = invariantGs1UnrecognisedCharacters?
            .Select(unrecognisedCharacter =>
                new UnrecognisedCharacter(unrecognisedCharacter, true))
            .ToList();

        (UnrecognisedCharacters as List<UnrecognisedCharacter>)?.AddRange(
            nonInvariantUnrecognisedCharacters?.Select(
                unrecognisedCharacter =>
                    new UnrecognisedCharacter(unrecognisedCharacter, false)) ?? Array.Empty<UnrecognisedCharacter>());

        // Set up ligature sequence match data for justification
        LigatureMappings = ligatureMap?.Select(ligatureMapping =>
            new LigatureMapping(
                ligatureMapping.Value.ToControlPicture(),
                ligatureMapping.Key.ToControlPictures(),
                IsSequenceInvariantDataOrApplication(ligatureMapping.Value.ToString(), formatnnSupportAssessed))).ToList();

        LigatureMappings =
            (from dkm in LigatureMappings
             orderby dkm.Expected
                select dkm).ToList();

#pragma warning disable S3626
        return;
#pragma warning restore S3626

        static string ParameterValue(Information info)
        {
            var msgSplit = info.Description.Split(':');
            return msgSplit.Length > 1 ? msgSplit[1].Trim() : string.Empty;
        }

        static CharacterCategory CalibrationCharacterCategory(int character) =>
            character switch
            {
                >= 0 and < 32 => Calibration.CharacterCategory.Ascii | Calibration.CharacterCategory.Control,
                32 => Calibration.CharacterCategory.Ascii,
                < 35 => Calibration.CharacterCategory.Ascii | Calibration.CharacterCategory.Invariant,
                >= 35 and < 37 => Calibration.CharacterCategory.Ascii,
                >= 37 and < 64 => Calibration.CharacterCategory.Ascii | Calibration.CharacterCategory.Invariant,
                64 => Calibration.CharacterCategory.Ascii,
                < 91 => Calibration.CharacterCategory.Ascii | Calibration.CharacterCategory.Invariant,
                >= 91 and < 95 => Calibration.CharacterCategory.Ascii,
                95 => Calibration.CharacterCategory.Ascii | Calibration.CharacterCategory.Invariant,
                96 => Calibration.CharacterCategory.Ascii,
                < 123 => Calibration.CharacterCategory.Ascii | Calibration.CharacterCategory.Invariant,
                >= 123 and < 128 => Calibration.CharacterCategory.Ascii,
                _ => Calibration.CharacterCategory.None
            };

        static bool IsSequenceInvariantDataOrApplication(string sequence, bool formatnnSupportAssessed)
        {
            if (string.IsNullOrWhiteSpace(sequence)) return false;

            var isInvariantDataOrApplication = true;
            foreach (var c in sequence) {
                var category = CalibrationCharacterCategory(c);
                isInvariantDataOrApplication = category switch
                {
                    _ when (category & Calibration.CharacterCategory.Invariant) is Calibration.CharacterCategory.Invariant => true,
                    // ReSharper disable once RedundantCast
                    _ when (category | Calibration.CharacterCategory.Control) is Calibration.CharacterCategory.Control => (int)c switch
                    {
                        >= 28 and < 32 => formatnnSupportAssessed || ( c >= 29 && c < 31),
                        _ => false
                    },
                    _ => false
                };

                if (!isInvariantDataOrApplication) break;
            }

            return isInvariantDataOrApplication;
        }
    }

    [JsonProperty("capsLock", Order = 41)]
    public bool CapsLock
    {
        get => _keyboardCapsLock;

        set
        {
            _keyboardCapsLock = value;
            ScannerMayCompensateForCapsLock = _keyboardCapsLock ? !CapsLockIndicator : null;
            ScannerMayInvertCase = _keyboardCapsLock ? !CapsLockIndicator : CapsLockIndicator;
        }
    }

    [JsonProperty("scannerMayCompensateForCapsLock", Order = 42)]
    public bool? ScannerMayCompensateForCapsLock { get; private set; }

    [JsonProperty("scannerMayInvertCase", Order = 43)]
    public bool? ScannerMayInvertCase { get; private set; }
}