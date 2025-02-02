// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemCapabilities.cs" company="Solidsoft Reply Ltd">
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
/// <param name="KeyboardLayoutsCorrespondForInvariants"></param>
/// <remarks>
///   Even if the keyboard layouts do not correspond, it can still be possible to read barcodes
///   reliably using a map. See the MappingPossible property.
/// </remarks>
/// <param name="KeyboardLayoutsCorrespondForNonInvariantCharacters">
///   Gets a value indicating whether the barcode scanner and the computer keyboard correspond
///   for invariant characters.
/// </param>
/// <remarks>
///   Even if the keyboard layouts do not correspond, it can still be possible to read barcodes
///   reliably using a map. See the MappingPossible property.
/// </remarks>
/// <param name="KeyboardLayoutsCanRepresentFileSeparatorsWithoutMapping">
///   Gets a value indicating whether the barcode scanner and the computer keyboard can represent
///   Group Separator control characters without mapping.
/// </param>
/// <param name="KeyboardLayoutsCanRepresentGroupSeparatorsWithoutMapping">
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
/// <param name="KeyboardLayoutsCanRepresentRecordSeparatorsWithoutMapping">
///   Gets a value indicating whether the barcode scanner and the computer keyboard can represent
///   Record Separator control characters without mapping.
/// </param>
/// <param name="KeyboardLayoutsCanRepresentUnitSeparatorsWithoutMapping">
///   Gets a value indicating whether the barcode scanner and the computer keyboard can represent
///   Record Separator control characters without mapping.
/// </param>
/// <param name="KeyboardLayoutsCanRepresentEotCharactersWithoutMapping">
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
/// <param name="KeyboardLayoutsCanRepresentEdiSeparatorsWithoutMapping">
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
///   to the computer keyboard to read invariant characters reliably, using a mapping
///   if necessary.
/// </param>
/// <remarks>
///   If true, this indicates that invariant characters and ASCII 29 characters
///   can be read reliably. This implies that any FNC1 (GS1) barcode can be read. It
///   also implies that invariant-only data can be read in Format 06 and Format 05
///   barcodes as long as ASCII 30 and ASCII 04 characters can be read reliably. See
///   the CanReadFormat05AndFormat06Reliably property.
/// </remarks>
/// <param name="CanReadNonInvariantsReliably">
///   Gets a value indicating whether it is possible to map from the barcode scanner
///   to the computer keyboard to read non-invariant characters reliably, using a mapping
///   if necessary.
/// </param>
/// <remarks>
///   The Calibration library only tests additional non-invariant ASCII characters.
/// </remarks>
/// <param name="CanReadIsoIec15434EnvelopeReliably">
///   Gets a value indicating whether the calibrated system can read ISO/IEC 15434
///   envelopes reliably, using a mapping if necessary.
/// </param>
/// <param name="CanReadEdiReliably">
///   Gets a value indicating whether the calibrated system can read EDI barcode data
///   formatted in accordance with ISO/IEC 15434 reliably, using a mapping if necessary.
/// </param>
/// <param name="CanReadGroupSeparatorsReliably">
///   Gets a value indicating whether the calibrated system can read Group Separator characters reliably, using a mapping if necessary.
/// </param>
/// <param name="CanReadRecordSeparatorsReliably">
///   Gets a value indicating whether the calibrated system can read Record Separator characters reliably, using a mapping if necessary.
/// </param>
/// <param name="CanReadFileSeparatorsReliably">
///   Gets a value indicating whether the calibrated system can read File separator characters reliably, using a mapping if necessary.
/// </param>
/// <param name="CanReadUnitSeparatorsReliably">
///   Gets a value indicating whether the calibrated system can read Unit separator characters reliably, using a mapping if necessary.
/// </param>
/// <param name="CanReadEotCharactersReliably">
///   Gets a value indicating whether the calibrated system can read EOT characters reliably, using a mapping if necessary.
/// </param>
/// <param name="AmbiguousInvariantForEotSeparator">
///   Gets a value indicating whether there is an ambiguous invariant character for the EOT separator.
/// </param>
/// <param name="CanReadAimIdentifiersWithoutMapping">
///   Gets a value indicating whether the calibrated system can read AIM identifiers without using a mapping.
/// </param>
/// <param name="CanReadAimIdentifiersReliably">
///   Gets a value indicating whether the calibrated system can read AIM identifiers reliably, using a mapping if necessary.
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
/// <param name="KeyboardScriptDoesNotSupportCase">
///   Gets a value indicating whether the computer keyboard script does not support upper- and lower-case letters.
/// </param>
/// <param name="CapsLockIndicator">
///   Gets a value indicating whether the calibrator determined heuristically if the Caps Lock
///   key appears to be on or off.
/// </param>
/// <param name="ScannerKeyboardPerformance">
///   Gets the time span specifying how long it took from the start of the scan to submitting the data.
/// </param>
/// <param name="ScannerCharactersPerSecond">
///   Gets the performance of the barcode scanner keyboard input in characters per second.
/// </param>
/// <remarks>
///   This cannot be determined with 100% certainty.
/// </remarks>
/// <param name="FormatSupportAssessed">
///   Gets a value indicating whether calibration included tests for Format nn support.
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
/// <param name="DeadKeys">
///   Gets a value indicating whether dead key barcodes are required for calibration.
/// </param>
/// <param name="CharacterMappings">
///   Gets a list of reported character sequences that map to characters.
/// </param>
/// <param name="DeadKeyMappings">
///   Gets a list of reported character sequences that map to dead key character sequences.
/// </param>
/// <param name="Ambiguities">
///   Gets a list of ambiguous character sequences that map to a reported character.
/// </param>
/// <param name="UnrecognisedCharacters">
///   Gets a list of unrecognised characters.
/// </param>
/// <param name="LigatureMappings">
///   Gets a list of ligature character sequences that map to a reported character.
/// </param>
/// <param name="Assumption">
///   Gets the assumption made concerning the use of calibration in client systems.
/// </param>
[method: JsonConstructor]
public sealed record SystemCapabilities(
    [property: JsonProperty("unexpectedError", Order = 0)] bool UnexpectedError = false,
    [property: JsonProperty("testsSucceeded", Order = 1)] bool TestsSucceeded = true,
    [property: JsonProperty("dataReported", Order = 2)] bool DataReported = true,
    [property: JsonProperty("correctSequenceReported", Order = 3)] bool CorrectSequenceReported = true,
    [property: JsonProperty("completeDataReported", Order = 4)] bool CompleteDataReported = true,
    [property: JsonProperty("keyboardLayoutsCorrespond", Order = 5)] bool? KeyboardLayoutsCorrespond = true,
    [property: JsonProperty("keyboardLayoutsCorrespondForInvariants", Order = 6)] bool? KeyboardLayoutsCorrespondForInvariants = true,
    [property: JsonProperty("keyboardLayoutsCorrespondForNonInvariantCharacters", Order = 7)] bool? KeyboardLayoutsCorrespondForNonInvariantCharacters = true,
    [property: JsonProperty("keyboardLayoutsCanRepresentFileSeparatorsWithoutMapping", Order = 8)] bool? KeyboardLayoutsCanRepresentFileSeparatorsWithoutMapping = true,
    [property: JsonProperty("keyboardLayoutsCanRepresentGroupSeparatorsWithoutMapping", Order = 9)] bool? KeyboardLayoutsCanRepresentGroupSeparatorsWithoutMapping = true,
    [property: JsonProperty("keyboardLayoutsCanRepresentRecordSeparatorsWithoutMapping", Order = 10)] bool? KeyboardLayoutsCanRepresentRecordSeparatorsWithoutMapping = true,
    [property: JsonProperty("keyboardLayoutsCanRepresentUnitSeparatorsWithoutMapping", Order = 11)] bool? KeyboardLayoutsCanRepresentUnitSeparatorsWithoutMapping = true,
    [property: JsonProperty("keyboardLayoutsCanRepresentEotCharactersWithoutMapping", Order = 12)] bool? KeyboardLayoutsCanRepresentEotCharactersWithoutMapping = true,
    [property: JsonProperty("keyboardLayoutsCanRepresentEdiSeparatorWithoutMapping", Order = 13)] bool? KeyboardLayoutsCanRepresentEdiSeparatorsWithoutMapping = true,
    [property: JsonProperty("keyboardLayoutsCorrespondForAimIdentifier", Order = 14)] bool? KeyboardLayoutsCorrespondForAimIdentifier = true,
    [property: JsonProperty("canReadInvariantsReliably", Order = 15)] bool? CanReadInvariantsReliably = true,
    [property: JsonProperty("canReadNonInvariantsReliably", Order = 16)] bool? CanReadNonInvariantsReliably = true,
    [property: JsonProperty("canReadIsoIec15434EnvelopeReliably", Order = 17)] bool? CanReadIsoIec15434EnvelopeReliably = null,
    [property: JsonProperty("canReadEdiReliably", Order = 18)] bool? CanReadEdiReliably = true,
    [property: JsonProperty("canReadGroupSeparatorsReliably", Order = 19)] bool? CanReadGroupSeparatorsReliably = false,
    [property: JsonProperty("canReadRecordSeparatorsReliably", Order = 20)] bool? CanReadRecordSeparatorsReliably = false,
    [property: JsonProperty("canReadFileSeparatorsReliably", Order = 21)] bool? CanReadFileSeparatorsReliably = null,
    [property: JsonProperty("canReadUnitSeparatorsReliably", Order = 22)] bool? CanReadUnitSeparatorsReliably = null,
    [property: JsonProperty("canReadEotCharactersReliably", Order = 23)] bool? CanReadEotCharactersReliably = null,
    [property: JsonProperty("ambiguousInvariantForEotSeparator", Order = 24)] bool AmbiguousInvariantForEotSeparator = false,
    [property: JsonProperty("canReadAimIdentifiersWithoutMapping", Order = 25)] bool? CanReadAimIdentifiersWithoutMapping = true,
    [property: JsonProperty("canReadAimIdentifiersReliably", Order = 25)] bool? CanReadAimIdentifiersReliably = true,
    [property: JsonProperty("scannerTransmitsAimIdentifiers", Order = 26)] bool? ScannerTransmitsAimIdentifiers = true,
    [property: JsonProperty("scannerTransmitsEndOfLineSequence", Order = 27)] bool? ScannerTransmitsEndOfLineSequence = true,
    [property: JsonProperty("scannerTransmitsAdditionalPrefix", Order = 28)] bool? ScannerTransmitsAdditionalPrefix = false,
    [property: JsonProperty("scannerTransmitsAdditionalCode", Order = 29)] bool? ScannerTransmitsAdditionalCode = false,
    [property: JsonProperty("scannerTransmitsAdditionalSuffix", Order = 30)] bool? ScannerTransmitsAdditionalSuffix = false,
    [property: JsonProperty("scannerMayConvertToUpperCase", Order = 31)] bool? ScannerMayConvertToUpperCase = null,
    [property: JsonProperty("scannerMayConvertToLowerCase", Order = 32)] bool? ScannerMayConvertToLowerCase = null,
    [property: JsonProperty("keyboardScriptDoesNotSupportCase", Order = 33)] bool? KeyboardScriptDoesNotSupportCase = null,
    [property: JsonProperty("capsLockIndicator", Order = 34)] bool CapsLockIndicator = false,
    [property: JsonProperty("scannerKeyboardPerformance", Order = 35)] ScannerKeyboardPerformance ScannerKeyboardPerformance = ScannerKeyboardPerformance.High,
    [property: JsonProperty("scannerCharactersPerSecond", Order = 36)] int ScannerCharactersPerSecond = 0,
    [property: JsonProperty("formatSupportAssessed", Order = 37)] bool FormatSupportAssessed = false,
    [property: JsonProperty("aimIdentifier", Order = 38)] string? AimIdentifier = null,
    [property: JsonProperty("aimIdentifierUncertain", Order = 39)] bool? AimIdentifierUncertain = false,
    [property: JsonProperty("endOfLineSequence", Order = 40)] string EndOfLineSequence = "",
    [property: JsonProperty("additionalPrefix", Order = 41)] string AdditionalPrefix = "",
    [property: JsonProperty("additionalCode", Order = 42)] string AdditionalCode = "",
    [property: JsonProperty("additionalSuffix", Order = 43)] string AdditionalSuffix = "",
    [property: JsonProperty("keyboardScript", Order = 44)] string KeyboardScript = "",
    [property: JsonProperty("platform", Order = 45)] SupportedPlatform Platform = SupportedPlatform.Windows,
    [property: JsonProperty("deadKeys", Order = 46)] bool DeadKeys = false,
    [property: JsonProperty("characterMappings", Order = 47)] IList<CharacterMapping>? CharacterMappings = null,
    [property: JsonProperty("deadKeyMappings", Order = 48)] IList<DeadKeyMapping>? DeadKeyMappings = null,
    [property: JsonProperty("ambiguities", Order = 49)] IList<Ambiguity>? Ambiguities = null,
    [property: JsonProperty("unrecognisedCharacters", Order = 50)] IList<UnrecognisedCharacter>? UnrecognisedCharacters = null,
    [property: JsonProperty("ligatureMappings", Order = 51)] IList<LigatureMapping>? LigatureMappings = null,
    [property: JsonProperty("calibrationAssumption", Order = 52)] Assumption Assumption = Assumption.Agnostic)
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
    /// <param name="scannerCharactersPerSecond">Performance of the barcode scanner keyboard input in characters per second.</param>
    /// <param name="formatSupportAssessed">Indicates whether calibration included tests for Format nn support.</param>
    /// <param name="deadKeys">Indicates whether dead key barcodes are required for calibration.</param>
    /// <param name="characterMap">A dictionary of differences in reported and expected characters.</param>
    /// <param name="deadKeyCharacterMap">A dictionary of initially detected differences in reported and expected characters where the reported data uses dead keys.</param>
    /// <param name="deadKeysMap">A dictionary of initially detected differences in reported and expected character sequences where the reported data uses dead keys.</param>
    /// <param name="invariantGs1Ambiguities">A dictionary of ambiguous invariant or other characters that may be used in GS1-compliant barcodes.</param>
    /// <param name="nonInvariantAmbiguities">A dictionary of ambiguous non-invariant characters that map to a reported character.</param>
    /// <param name="invariantGs1UnrecognisedCharacters"> A list of unrecognised invariant or other characters that may be used in GS1-compliant barcodes.</param>
    /// <param name="nonInvariantUnrecognisedCharacters">A list of unrecognised non-invariant characters.</param>
    /// <param name="ligatureMap">A list of ligature character sequences.</param>
    internal SystemCapabilities(
        Token token,
        Assumption assumption,
        bool? capsLock,
        ScannerKeyboardPerformance scannerKeyboardPerformance,
        int scannerCharactersPerSecond,
        bool formatSupportAssessed,
        bool deadKeys,
        IDictionary<char, char>? characterMap,
        IDictionary<string, char>? deadKeyCharacterMap,
        IDictionary<string, string>? deadKeysMap,
        IDictionary<string, IList<string>>? invariantGs1Ambiguities,
        IDictionary<string, IList<string>>? nonInvariantAmbiguities,
        IEnumerable<string>? invariantGs1UnrecognisedCharacters,
        IEnumerable<string>? nonInvariantUnrecognisedCharacters,
        IDictionary<string, char>? ligatureMap)
        : this() {
        if (token.ToJson().Replace(" ", string.Empty) == "{}") {
            return;
        }

        Assumption = assumption;
        FormatSupportAssessed = formatSupportAssessed;
        ScannerKeyboardPerformance = scannerKeyboardPerformance;
        ScannerCharactersPerSecond = scannerCharactersPerSecond;
        DeadKeys = deadKeys;

        // Process information
        foreach (var info in token.Information) {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (info.InformationType) {
                case InformationType.KeyboardScript:
                    KeyboardScript = ParameterValue(info);
                    break;
                case InformationType.GroupSeparatorSupported:
                    CanReadGroupSeparatorsReliably = true;
                    break;
                case InformationType.RecordSeparatorSupported:
                    CanReadRecordSeparatorsReliably = true;
                    break;
                case InformationType.FileSeparatorSupported:
                    CanReadFileSeparatorsReliably = true;
                    break;
                case InformationType.UnitSeparatorSupported:
                    CanReadUnitSeparatorsReliably = true;
                    break;
                case InformationType.InvariantAmbiguityForEotCharacter:
                    AmbiguousInvariantForEotSeparator = true;
                    CanReadEotCharactersReliably = false;
                    break;
                case InformationType.EndOfTransmissionSupported:
                    CanReadEotCharactersReliably = true;
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
                case InformationType.AimNotTransmitted:
                case InformationType.AimNotRecognised:
                case InformationType.AimNotReadReliably:
                case InformationType.PrefixTransmitted:
                case InformationType.CodeTransmitted:
                case InformationType.SuffixTransmitted:
                case InformationType.EndOfLineNotTransmitted:
                case InformationType.MultipleKeysNonInvariantCharacters:
                case InformationType.MultipleKeysMultipleNonInvariantCharacters:
                case InformationType.MultipleKeysAimFlagCharacter:
                case InformationType.DeadKeyMultiMappingNonInvariantCharacters:
                case InformationType.FileSeparatorMappingIsoIec15434EdiNotReliablyReadable:
                case InformationType.ControlCharacterMappingNonInvariants:
                case InformationType.NonInvariantCharacterSequence:
                case InformationType.NonCorrespondingKeyboardLayouts:
                case InformationType.NonCorrespondingKeyboardLayoutsForInvariants:
                case InformationType.NonCorrespondingKeyboardLayoutsForNonInvariantCharacters:
                case InformationType.NonCorrespondingKeyboardLayoutsFileSeparator:
                case InformationType.NonCorrespondingKeyboardLayoutsGroupSeparator:
                case InformationType.NonCorrespondingKeyboardLayoutsUnitSeparator:
                case InformationType.NonCorrespondingKeyboardLayoutsEotCharacter:
                case InformationType.NonCorrespondingKeyboardLayoutsRecordSeparator:
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
                case InformationType.LigatureCharacters:
                case InformationType.NoDelimiters:
                case InformationType.NoTemporaryDelimiterCandidate:
                case InformationType.CalibrationFailed:
                case InformationType.CalibrationFailedUnexpectedly:
                case InformationType.RecordSeparatorNotReliablyReadableInvariant:
                case InformationType.GroupSeparatorNotReliablyReadableInvariant:
                case InformationType.EotCharacterNotReliablyReadableInvariant:
                case InformationType.FileSeparatorNotReliablyReadableInvariant:
                case InformationType.FileSeparatorNotReliablyReadableNonInvariant:
                case InformationType.UnitSeparatorNotReliablyReadableInvariant:
                case InformationType.UnitSeparatorNotReliablyReadableNonInvariant:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(token),
                        info.InformationType,
                        Resources.CalibrationIncorrectInformationalInformationType);
            }
        }

        // Process warnings
        foreach (var info in token.Warnings) {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (info.InformationType) {
                case InformationType.SomeNonInvariantCharactersUnreported:
                case InformationType.SomeNonInvariantCharactersUnrecognised:
                case InformationType.SomeNonInvariantCharacterCombinationsUnrecognised:
                case InformationType.MultipleKeysMultipleNonInvariantCharacters:
                case InformationType.DeadKeyMultiMappingNonInvariantCharacters:
                case InformationType.ControlCharacterMappingNonInvariants:
                case InformationType.NonInvariantCharacterSequence:
                    KeyboardLayoutsCorrespondForNonInvariantCharacters = false;
                    KeyboardLayoutsCorrespond = false;
                    CanReadNonInvariantsReliably = false;
                    break;
                case InformationType.RecordSeparatorNotReadable:
                    CanReadRecordSeparatorsReliably = false;
                    CanReadIsoIec15434EnvelopeReliably = false;
                    break;
                case InformationType.FileSeparatorNotReadable:
                    CanReadFileSeparatorsReliably = false;
                    CanReadEdiReliably = false;
                    break;
                case InformationType.UnitSeparatorNotReadable:
                    CanReadUnitSeparatorsReliably = false;
                    CanReadEdiReliably = false;
                    break;
                case InformationType.EotCharacterNotReadable:
                    CanReadEotCharactersReliably = false;
                    CanReadIsoIec15434EnvelopeReliably = false;
                    break;
                case InformationType.RecordSeparatorNotReliablyReadableInvariant:
                    CanReadInvariantsReliably = false;
                    CanReadRecordSeparatorsReliably = false;
                    CanReadIsoIec15434EnvelopeReliably = false;
                    break;
                case InformationType.FileSeparatorNotReliablyReadableInvariant:
                    CanReadFileSeparatorsReliably = false;
                    CanReadEdiReliably = false;
                    break;
                case InformationType.UnitSeparatorNotReliablyReadableInvariant:
                    CanReadUnitSeparatorsReliably = false;
                    CanReadEdiReliably = false;
                    break;
                case InformationType.EotCharacterNotReliablyReadableInvariant:
                    CanReadInvariantsReliably = false;
                    CanReadEotCharactersReliably = false;
                    CanReadIsoIec15434EnvelopeReliably = false;
                    break;
                case InformationType.FileSeparatorNotReliablyReadableNonInvariant:
                    CanReadNonInvariantsReliably = false;
                    CanReadFileSeparatorsReliably = false;
                    CanReadEdiReliably = false;
                    break;
                case InformationType.UnitSeparatorNotReliablyReadableNonInvariant:
                    CanReadNonInvariantsReliably = false;
                    CanReadUnitSeparatorsReliably = false;
                    CanReadEdiReliably = false;
                    break;
                case InformationType.IsoIec15434SyntaxNotRecognised:
                case InformationType.RecordSeparatorMappingNotReliablyReadable:
                    CanReadIsoIec15434EnvelopeReliably = false;
                    break;
                case InformationType.IsoIec15434EdiNotReliablyReadable:
                    CanReadEdiReliably = false;
                    break;
                case InformationType.FileSeparatorNotReliablyReadable:
                    CanReadEdiReliably = false;
                    CanReadFileSeparatorsReliably = false;
                    break;
                case InformationType.UnitSeparatorNotReliablyReadable:
                    CanReadEdiReliably = false;
                    CanReadUnitSeparatorsReliably = false;
                    break;
                case InformationType.EotNotReliablyReadable:
                    CanReadEotCharactersReliably = false;
                    break;
                case InformationType.FileSeparatorMappingIsoIec15434EdiNotReliablyReadable:
                    CanReadEdiReliably = false;
                    CanReadFileSeparatorsReliably = false;
                    break;
                case InformationType.UnitSeparatorMappingIsoIec15434EdiNotReliablyReadable:
                    CanReadEdiReliably = false;
                    CanReadUnitSeparatorsReliably = false;
                    break;
                case InformationType.FileSeparatorMappingNotReliablyReadable:
                    CanReadFileSeparatorsReliably = false;
                    break;
                case InformationType.UnitSeparatorMappingNotReliablyReadable:
                    CanReadUnitSeparatorsReliably = false;
                    break;
                case InformationType.EotCharacterMappingIsoIec15434EdiNotReliablyReadable:
                    CanReadEotCharactersReliably = false;
                    break;
                case InformationType.AimNotTransmitted:
                    ScannerTransmitsAimIdentifiers = false;
                    break;
                case InformationType.AimNotRecognised:
                case InformationType.MultipleKeysAimFlagCharacter:
                    KeyboardLayoutsCorrespondForAimIdentifier = false;
                    KeyboardLayoutsCorrespond = false;
                    CanReadAimIdentifiersWithoutMapping = false;
                    break;
                case InformationType.AimNotReadReliably:
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
                case InformationType.NonCorrespondingKeyboardLayoutsFileSeparator:
                    KeyboardLayoutsCanRepresentFileSeparatorsWithoutMapping = false;
                    KeyboardLayoutsCanRepresentEdiSeparatorsWithoutMapping = false;
                    KeyboardLayoutsCorrespond = false;
                    CanReadEdiReliably = null;
                    break;
                case InformationType.NonCorrespondingKeyboardLayoutsGroupSeparator:
                    KeyboardLayoutsCanRepresentGroupSeparatorsWithoutMapping = false;
                    KeyboardLayoutsCorrespond = false;
                    break;
                case InformationType.NonCorrespondingKeyboardLayoutsRecordSeparator:
                    KeyboardLayoutsCanRepresentRecordSeparatorsWithoutMapping = false;
                    KeyboardLayoutsCorrespond = false;
                    break;
                case InformationType.NonCorrespondingKeyboardLayoutsUnitSeparator:
                    KeyboardLayoutsCanRepresentUnitSeparatorsWithoutMapping = false;
                    KeyboardLayoutsCanRepresentEdiSeparatorsWithoutMapping = false;
                    KeyboardLayoutsCorrespond = false;
                    CanReadEdiReliably = null;
                    break;
                case InformationType.NonCorrespondingKeyboardLayoutsEotCharacter:
                    KeyboardLayoutsCanRepresentEotCharactersWithoutMapping = false;
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
                case InformationType.EndOfTransmissionSupported:
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
                case InformationType.LigatureCharacters:
                case InformationType.NoDelimiters:
                case InformationType.NoTemporaryDelimiterCandidate:
                case InformationType.CalibrationFailed:
                case InformationType.CalibrationFailedUnexpectedly:
                case InformationType.GroupSeparatorNotReliablyReadableInvariant:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(token),
                        info.InformationType,
                        Resources.CalibrationIncorrectWarningInformationType);
            }
        }

        var capabilitiesUnknown = false;

        // Process errors
        foreach (var informationType in token.Errors.Select(info => info.InformationType)) {
#pragma warning disable S907
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (informationType) {
                // We received partial data.  We cannot determine the system capabilities.
                case InformationType.PartialCalibrationDataReported:
                    CompleteDataReported = false;
                    capabilitiesUnknown = true;
                    break;
                case InformationType.IncorrectCalibrationDataReported:
                    CorrectSequenceReported = false;
                    capabilitiesUnknown = true;
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
                    capabilitiesUnknown = true;
                    goto case InformationType.CalibrationFailed;
                case InformationType.CalibrationFailed:
                    TestsSucceeded = false;
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
                case InformationType.LigatureCharacters:
                    KeyboardLayoutsCorrespond = false;
                    KeyboardLayoutsCorrespondForInvariants = false;
                    CanReadInvariantsReliably = false;
                    break;
                case InformationType.GroupSeparatorNotReliablyReadableInvariant:
                    CanReadInvariantsReliably = false;
                    CanReadGroupSeparatorsReliably = false;
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
                case InformationType.EndOfTransmissionSupported:
                case InformationType.ScannerMayCompensateForCapsLock:
                case InformationType.KeyboardScript:
                case InformationType.Platform:
                case InformationType.SomeNonInvariantCharactersUnreported:
                case InformationType.SomeNonInvariantCharactersUnrecognised:
                case InformationType.SomeNonInvariantCharacterCombinationsUnrecognised:
                case InformationType.IsoIec15434SyntaxNotRecognised:
                case InformationType.IsoIec15434EdiNotReliablyReadable:
                case InformationType.AimNotTransmitted:
                case InformationType.AimNotRecognised:
                case InformationType.AimNotReadReliably:
                case InformationType.PrefixTransmitted:
                case InformationType.CodeTransmitted:
                case InformationType.SuffixTransmitted:
                case InformationType.EndOfLineNotTransmitted:
                case InformationType.MultipleKeysNonInvariantCharacters:
                case InformationType.MultipleKeysMultipleNonInvariantCharacters:
                case InformationType.MultipleKeysAimFlagCharacter:
                case InformationType.DeadKeyMultiMappingNonInvariantCharacters:
                case InformationType.FileSeparatorMappingIsoIec15434EdiNotReliablyReadable:
                case InformationType.FileSeparatorMappingNotReliablyReadable:
                case InformationType.UnitSeparatorMappingNotReliablyReadable:
                case InformationType.EotCharacterNotReliablyReadableInvariant:
                case InformationType.ControlCharacterMappingNonInvariants:
                case InformationType.NonInvariantCharacterSequence:
                case InformationType.NonCorrespondingKeyboardLayouts:
                case InformationType.NonCorrespondingKeyboardLayoutsForInvariants:
                case InformationType.NonCorrespondingKeyboardLayoutsForNonInvariantCharacters:
                case InformationType.NonCorrespondingKeyboardLayoutsFileSeparator:
                case InformationType.NonCorrespondingKeyboardLayoutsGroupSeparator:
                case InformationType.NonCorrespondingKeyboardLayoutsRecordSeparator:
                case InformationType.NonCorrespondingKeyboardLayoutsUnitSeparator:
                case InformationType.NonCorrespondingKeyboardLayoutsEotCharacter:
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

        if (capabilitiesUnknown) {
            KeyboardLayoutsCorrespond = null;
            KeyboardLayoutsCorrespondForInvariants = null;
            KeyboardLayoutsCorrespondForNonInvariantCharacters = null;
            KeyboardLayoutsCanRepresentFileSeparatorsWithoutMapping = null;
            KeyboardLayoutsCanRepresentGroupSeparatorsWithoutMapping = null;
            KeyboardLayoutsCanRepresentRecordSeparatorsWithoutMapping = null;
            KeyboardLayoutsCanRepresentUnitSeparatorsWithoutMapping = null;
            KeyboardLayoutsCanRepresentEotCharactersWithoutMapping = null;
            KeyboardLayoutsCanRepresentEdiSeparatorsWithoutMapping = null;
            KeyboardLayoutsCorrespondForAimIdentifier = null;
            CanReadInvariantsReliably = null;
            CanReadNonInvariantsReliably = null;
            CanReadIsoIec15434EnvelopeReliably = null;
            CanReadEdiReliably = null;
            CanReadGroupSeparatorsReliably = null;
            CanReadRecordSeparatorsReliably = null;
            CanReadFileSeparatorsReliably = null;
            CanReadUnitSeparatorsReliably = null;
            CanReadEotCharactersReliably = null;
            CanReadAimIdentifiersWithoutMapping = null;
            CanReadAimIdentifiersReliably = null;
            if (ScannerTransmitsAimIdentifiers ?? true) ScannerTransmitsAimIdentifiers = null;
            if (ScannerTransmitsEndOfLineSequence ?? true) ScannerTransmitsEndOfLineSequence = null;
            if (!(ScannerTransmitsAdditionalPrefix ?? false)) ScannerTransmitsAdditionalPrefix = null;
            if (!(ScannerTransmitsAdditionalCode ?? false)) ScannerTransmitsAdditionalCode = null;
            if (!(ScannerTransmitsAdditionalSuffix ?? false)) ScannerTransmitsAdditionalSuffix = null;
            if (!AimIdentifierUncertain ?? false) AimIdentifierUncertain = null;
            DeadKeys = false;
            CharacterMappings = null;
            DeadKeyMappings = null;
            Ambiguities = null;
            UnrecognisedCharacters = null;
            LigatureMappings = null;
        }

        if (capsLock.GetValueOrDefault()) {
            // Caps Lock is ON
            _keyboardCapsLock = true;
        }
        else {
            // Caps Lock is OFF or not reported
            if (capsLock is null) {
                // Caps Lock is not reported. We will assume that the caps lock indicator is accurate.
                _keyboardCapsLock = CapsLockIndicator;
                ScannerMayCompensateForCapsLock = null;
                ScannerMayInvertCase = null;
            }
            else {
                // Caps Lock is OFF.
                _keyboardCapsLock = false;
                ScannerMayCompensateForCapsLock = null;
            }
        }

        if (_keyboardCapsLock) {
            if (ScannerMayConvertToUpperCase ?? false) {
                ScannerMayConvertToUpperCase = false;
                ScannerMayConvertToLowerCase = true;
            }
            else if (ScannerMayConvertToLowerCase ?? false) {
                ScannerMayConvertToUpperCase = true;
                ScannerMayConvertToLowerCase = false;
            }
        }

        if ((ScannerMayConvertToUpperCase ?? false) || (ScannerMayConvertToLowerCase ?? false)) {
            KeyboardLayoutsCorrespond = null;
            KeyboardLayoutsCorrespondForInvariants = null;
            KeyboardLayoutsCorrespondForAimIdentifier = null;
        }

        if (!string.IsNullOrWhiteSpace(KeyboardScript)) {
            KeyboardScriptDoesNotSupportCase = KeyboardScript switch {
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
            new CharacterMapping(
                characterMapping.Value.ToControlPicture(),
                characterMapping.Key.ToControlPictureString(),
                CalibrationCharacterCategory(characterMapping.Value),
                false)).ToList();

#pragma warning disable IDE0301 // Simplify collection initialization
        (CharacterMappings as List<CharacterMapping>)?.AddRange(
            deadKeyCharacterMap?.Select(deadKeyCharacterMapping =>
                new CharacterMapping(
                    deadKeyCharacterMapping.Value.ToControlPicture(),
                    deadKeyCharacterMapping.Key[(deadKeyCharacterMapping.Key.LastIndexOf('\0') + 1)..].ToControlPictures(),
                    CalibrationCharacterCategory(deadKeyCharacterMapping.Value),
                    true)) ?? Array.Empty<CharacterMapping>());
#pragma warning restore IDE0301 // Simplify collection initialization

#pragma warning disable IDE0305 // Simplify collection initialization
        CharacterMappings =
            (from cm in CharacterMappings
             orderby cm.Expected
             select cm).ToList();
#pragma warning restore IDE0305 // Simplify collection initialization

        // Set up dead key sequence match data for justification
        DeadKeyMappings = deadKeysMap?.Select(deadKeyMapping =>
            new DeadKeyMapping(
                deadKeyMapping.Value.ToControlPictures(),
                deadKeyMapping.Key.ToControlPictures(),
                IsSequenceInvariantDataOrApplication(deadKeyMapping.Value, formatSupportAssessed))).ToList();

#pragma warning disable IDE0305 // Simplify collection initialization
        DeadKeyMappings =
            (from dkm in DeadKeyMappings
             orderby dkm.Expected
             select dkm).ToList();
#pragma warning restore IDE0305 // Simplify collection initialization

        // Set up ambiguity data for justification.
        Ambiguities = invariantGs1Ambiguities?
            .Select(ambiguity =>
                new Ambiguity(ambiguity.Value, ambiguity.Key, true, deadKeyCharacterMap?.ContainsKey(ambiguity.Key.Replace('\u2400', '\0')) ?? false)).ToList();

        // Create a list of non-invariant ambiguities that includes any additional variant characters
        // for the same reported character.
        var ambiguities = new Dictionary<string, IList<string>>();

        foreach (var (key, value) in nonInvariantAmbiguities ?? new Dictionary<string, IList<string>>()) {
            if (invariantGs1Ambiguities?.TryGetValue(key, out var ambiguity) ?? false) {
                foreach (var ambiguityValue in ambiguity) {
                    if (!value.Contains(ambiguityValue)) value.Add(ambiguityValue);
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

#pragma warning disable IDE0301 // Simplify collection initialization
        (UnrecognisedCharacters as List<UnrecognisedCharacter>)?.AddRange(
            nonInvariantUnrecognisedCharacters?.Select(
                unrecognisedCharacter =>
                    new UnrecognisedCharacter(unrecognisedCharacter, false)) ?? Array.Empty<UnrecognisedCharacter>());
#pragma warning restore IDE0301 // Simplify collection initialization

        // Set up ligature sequence match data for justification
        LigatureMappings = ligatureMap?.Select(ligatureMapping =>
            new LigatureMapping(
                ligatureMapping.Value.ToControlPicture(),
                ligatureMapping.Key.ToControlPictures(),
                IsSequenceInvariantDataOrApplication(ligatureMapping.Value.ToString(), formatSupportAssessed))).ToList();

#pragma warning disable IDE0305 // Simplify collection initialization
        LigatureMappings =
            (from dkm in LigatureMappings
             orderby dkm.Expected
             select dkm).ToList();
#pragma warning restore IDE0305 // Simplify collection initialization

#pragma warning disable S3626
        return;
#pragma warning restore S3626

        static string ParameterValue(Information info) {
            var msgSplit = info.Description?.Split(':');
            return msgSplit?.Length > 1 ? msgSplit[1].Trim() : string.Empty;
        }

        static CharacterCategory CalibrationCharacterCategory(int character) =>
            character switch {
                >= 0 and < 32 => CharacterCategory.Ascii | CharacterCategory.Control,
                32 => CharacterCategory.Ascii,
                < 35 => CharacterCategory.Ascii | CharacterCategory.Invariant,
                >= 35 and < 37 => CharacterCategory.Ascii,
                >= 37 and < 64 => CharacterCategory.Ascii | CharacterCategory.Invariant,
                64 => CharacterCategory.Ascii,
                < 91 => CharacterCategory.Ascii | CharacterCategory.Invariant,
                >= 91 and < 95 => CharacterCategory.Ascii,
                95 => CharacterCategory.Ascii | CharacterCategory.Invariant,
                96 => CharacterCategory.Ascii,
                < 123 => CharacterCategory.Ascii | CharacterCategory.Invariant,
                >= 123 and < 128 => CharacterCategory.Ascii,
                _ => CharacterCategory.None
            };

        static bool IsSequenceInvariantDataOrApplication(string sequence, bool formatSupportAssessed) {
            if (string.IsNullOrWhiteSpace(sequence)) return false;

            var isInvariantDataOrApplication = true;
            foreach (var c in sequence) {
                var category = CalibrationCharacterCategory(c);
                isInvariantDataOrApplication = category switch {
                    _ when (category & CharacterCategory.Invariant) is CharacterCategory.Invariant => true,

                    // ReSharper disable once RedundantCast
                    _ when (category | CharacterCategory.Control) is CharacterCategory.Control => (int)c switch {
                        >= 28 and < 32 => formatSupportAssessed || (c >= 29 && c < 31),
                        _ => false
                    },
                    _ => false
                };

                if (!isInvariantDataOrApplication) break;
            }

            return isInvariantDataOrApplication;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether CAPS LOCK in on or off.
    /// </summary>
    [JsonProperty("capsLock", Order = 47)]
    public bool CapsLock {
        get => _keyboardCapsLock;

        set {
            _keyboardCapsLock = value;
            ScannerMayCompensateForCapsLock = _keyboardCapsLock ? !CapsLockIndicator : null;
            ScannerMayInvertCase = _keyboardCapsLock ? !CapsLockIndicator : CapsLockIndicator;
        }
    }

    /// <summary>
    /// Gets a value that indicates whether the scanner may compensate for CAPS LOCK.
    /// </summary>
    [JsonProperty("scannerMayCompensateForCapsLock", Order = 48)]
    public bool? ScannerMayCompensateForCapsLock { get; private set; }

    /// <summary>
    /// Gets a value that indicates whether the scanner may invert the case of characters.
    /// </summary>
    [JsonProperty("scannerMayInvertCase", Order = 49)]
    public bool? ScannerMayInvertCase { get; private set; }
}