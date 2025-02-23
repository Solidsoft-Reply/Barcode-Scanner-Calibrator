﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InformationType.cs" company="Solidsoft Reply Ltd">
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
// The type of calibration information.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

/// <summary>
///   The type of calibration information.
/// </summary>
public enum InformationType {
    /// <summary>
    /// <p>No calibration information available.</p>
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    None = 0,

    /// <summary>
    ///   INFORMATION: AIM identifiers are supported.
    /// </summary>
    AimSupported = 130,

    /// <summary>
    ///   INFORMATION: The barcode scanner is transmitting an AIM identifier: {0}
    /// </summary>
    AimTransmitted = 131,

    /// <summary>
    ///   INFORMATION: The barcode scanner may be transmitting an AIM identifier: {0}
    /// </summary>
    AimMayBeTransmitted = 132,

    /// <summary>
    ///   INFORMATION: The barcode scanner is transmitting an end-of-line character sequence: {0}
    /// </summary>
    EndOfLineTransmitted = 150,

    /// <summary>
    ///   INFORMATION: Group Separator characters are supported.
    /// </summary>
    GroupSeparatorSupported = 160,

    /// <summary>
    ///   INFORMATION: Record Separator characters are supported.
    /// </summary>
    RecordSeparatorSupported = 161,

    /// <summary>
    ///   INFORMATION: File separator characters are supported.
    /// </summary>
    FileSeparatorSupported = 162,

    /// <summary>
    ///   INFORMATION: Unit separator characters are supported.
    /// </summary>
    UnitSeparatorSupported = 163,

    /// <summary>
    ///   INFORMATION: End-of-Transmission characters are supported.
    /// </summary>
    EndOfTransmissionSupported = 164,

    /// <summary>
    ///   INFORMATION: The reported character for an invariant character is the same as the character reported for the End-of-Transmission character.
    /// </summary>
    InvariantAmbiguityForEotCharacter = 165,

    /// <summary>
    ///   INFORMATION: The barcode scanner may be configured to compensate for Caps Lock.
    /// </summary>
    ScannerMayCompensateForCapsLock = 170,

    /// <summary>
    ///   INFORMATION: The computer keyboard supports the following script: {0}.
    /// </summary>
    KeyboardScript = 180,

    /// <summary>
    ///   INFORMATION: The computer keyboard is configured for the following platform: {0}.
    /// </summary>
    Platform = 190,

    /// <summary>
    ///   WARNING: Some non-invariant ASCII characters cannot be detected.
    /// </summary>
    SomeNonInvariantCharactersUnreported = 200,

    /// <summary>
    ///   WARNING: Some non-invariant ASCII characters are not recognised: {0}
    /// </summary>
    SomeNonInvariantCharactersUnrecognised = 201,

    /// <summary>
    ///   WARNING: Some combinations of non-invariant ASCII characters are not recognised: {0}
    /// </summary>
    SomeNonInvariantCharacterCombinationsUnrecognised = 202,

    /// <summary>
    ///   WARNING: Barcodes that use ISO/IEC 15434 syntax cannot be recognised.
    /// </summary>
    IsoIec15434SyntaxNotRecognised = 210,

    /// <summary>
    ///   WARNING: Barcodes that use ISO/IEC 15434 syntax to represent EDI data cannot be reliably read.
    /// </summary>
    IsoIec15434EdiNotReliablyReadable = 211,

    /// <summary>
    ///   WARNING: The record separator character cannot be read.
    /// </summary>
    RecordSeparatorNotReadable = 213,

    /// <summary>
    ///   WARNING: The reported character sequence {0} is ambiguous.The record separator character cannot be reliably read.
    /// </summary>
    RecordSeparatorNotReliablyReadableInvariant = 214,

    /// <summary>
    ///   WARNING: The file separator character cannot be read.
    /// </summary>
    FileSeparatorNotReadable = 216,

    /// <summary>
    ///   WARNING: The file separator character cannot be reliably read.
    /// </summary>
    FileSeparatorNotReliablyReadable = 217,

    /// <summary>
    ///   WARNING: The reported character sequence {0} is ambiguous.The file separator character cannot be reliably read.
    /// </summary>
    FileSeparatorNotReliablyReadableInvariant = 218,

    /// <summary>
    ///   WARNING: The reported character sequence {0} is ambiguous.The file separator character cannot be reliably read.
    /// </summary>
    FileSeparatorNotReliablyReadableNonInvariant = 219,

    /// <summary>
    ///   WARNING: The unit separator character cannot be read.
    /// </summary>
    UnitSeparatorNotReadable = 220,

    /// <summary>
    ///   WARNING: The unit separator character cannot be reliably read.
    /// </summary>
    UnitSeparatorNotReliablyReadable = 221,

    /// <summary>
    ///   WARNING: The reported character sequence {0} is ambiguous.The unit separator character cannot be reliably read.
    /// </summary>
    UnitSeparatorNotReliablyReadableInvariant = 222,

    /// <summary>
    ///   WARNING: The reported character sequence {0} is ambiguous.The unit separator character cannot be reliably read.
    /// </summary>
    UnitSeparatorNotReliablyReadableNonInvariant = 223,

    /// <summary>
    ///   WARNING: The end-of-transmission character cannot be read.
    /// </summary>
    EotCharacterNotReadable = 224,

    /// <summary>
    ///   WARNING: The end-of-transmission character cannot be reliably read.
    /// </summary>
    EotNotReliablyReadable = 225,

    /// <summary>
    ///   WARNING: The reported character sequence {0} is ambiguous.The end-of-transmission character cannot be reliably read.
    /// </summary>
    EotCharacterNotReliablyReadableInvariant = 226,

    /// <summary>
    ///   WARNING: The barcode scanner is not transmitting an AIM Identifier.
    /// </summary>
    AimNotTransmitted = 230,

    /// <summary>
    ///   WARNING: The AIM Identifier cannot be recognised.
    /// </summary>
    AimNotRecognised = 231,

    /// <summary>
    ///   WARNING: The AIM Identifier cannot be read reliably.
    /// </summary>
    AimNotReadReliably = 232,

    /// <summary>
    ///   WARNING: The barcode scanner is transmitting a prefix: {0}
    /// </summary>
    PrefixTransmitted = 240,

    /// <summary>
    ///   WARNING: The barcode scanner is transmitting a code: {0}
    /// </summary>
    CodeTransmitted = 241,

    /// <summary>
    ///   WARNING: The barcode scanner is transmitting a suffix: {0}
    /// </summary>
    SuffixTransmitted = 242,

    /// <summary>
    ///   WARNING: The barcode scanner is not transmitting an end-of-line character sequence (e.g. a carriage return).
    /// </summary>
    EndOfLineNotTransmitted = 245,

    /// <summary>
    ///   WARNING: The reported character {0} is ambiguous. There are multiple keys for the same character, each
    ///   representing a different expected character. However, at most, only one of the expected characters is invariant.
    /// </summary>
    MultipleKeysNonInvariantCharacters = 250,

    /// <summary>
    ///   WARNING: Some reported characters are ambiguous. There are multiple keys for the same character, each representing
    ///   a different non-invariant expected ASCII character: {0}
    /// </summary>
    MultipleKeysMultipleNonInvariantCharacters = 251,

    /// <summary>
    ///   Warning: AIM Identifiers cannot be recognised. The reported character {0} is ambiguous. There are multiple keys for
    ///   this character which represents the AIM flag character as well as other expected character(s).
    /// </summary>
    MultipleKeysAimFlagCharacter = 252,

    /// <summary>
    ///   WARNING: Some reported non-invariant ASCII characters are ambiguous: {0}
    /// </summary>
    DeadKeyMultiMappingNonInvariantCharacters = 253,

    /// <summary>
    ///   WARNING: The reported character {0} for a File Separator is ambiguous. Barcodes that use ISO/IEC 15434 syntax to represent EDI data cannot be reliably read.
    /// </summary>
    FileSeparatorMappingIsoIec15434EdiNotReliablyReadable = 254,

    /// <summary>
    ///   WARNING: The reported character {0} for a Unit Separator is ambiguous. Barcodes that use ISO/IEC 15434 syntax to represent EDI data cannot be reliably read.
    /// </summary>
    UnitSeparatorMappingIsoIec15434EdiNotReliablyReadable = 255,

    /// <summary>
    ///   WARNING: The reported character {0} for an End-of-Transmission character is ambiguous. Barcodes that use ISO/IEC 15434 syntax cannot be reliably read.
    /// </summary>
    EotCharacterMappingIsoIec15434EdiNotReliablyReadable = 256,

    /// <summary>
    ///   WARNING: Some reported character sequences are ambiguous. These characters do not represent invariant characters: {0}
    /// </summary>
    NonInvariantCharacterSequence = 257,

    /// <summary>
    ///   WARNING: The reported character sequence {0} is ambiguous. This may prevent reading of any additional data elements included in a barcode.
    /// </summary>
    ControlCharacterMappingNonInvariants = 258,

    /// <summary>
    ///   WARNING: The reported character {0} for a File Separator is ambiguous. Barcodes that use ASCII 28 control characters may not be reliably read.
    /// </summary>
    FileSeparatorMappingNotReliablyReadable = 259,

    /// <summary>
    ///   WARNING: The reported character {0} for a Unit Separator is ambiguous. Barcodes that use ASCII 31 control characters may not be reliably read.
    /// </summary>
    UnitSeparatorMappingNotReliablyReadable = 260,

    /// <summary>
    ///   WARNING: The reported character {0} for an End-of-Transmission character is ambiguous. Barcodes that use ASCII04 control characters may not be reliably read.
    /// </summary>
    EotCharacterMappingNotReliablyReadable = 261,

    /// <summary>
    ///   WARNING: The reported character sequence {0} is ambiguous. This represents the record separator character.
    /// </summary>
    RecordSeparatorMappingNotReliablyReadable = 262,

    /// <summary>
    ///   WARNING: The barcode scanner and computer keyboard layouts do not correspond.
    /// </summary>
    NonCorrespondingKeyboardLayouts = 270,

    /// <summary>
    ///   WARNING: The barcode scanner and computer keyboard layouts do not correspond for invariant characters.
    /// </summary>
    NonCorrespondingKeyboardLayoutsForInvariants = 271,

    /// <summary>
    ///   WARNING: The barcode scanner and computer keyboard layouts do not correspond for non-invariant ASCII characters.
    /// </summary>
    NonCorrespondingKeyboardLayoutsForNonInvariantCharacters = 272,

    /// <summary>
    ///   WARNING: The barcode scanner and computer keyboard layouts do not correspond when representing File Separators.
    /// </summary>
    NonCorrespondingKeyboardLayoutsFileSeparator = 273,

    /// <summary>
    ///   WARNING: The barcode scanner and computer keyboard layouts do not correspond when representing Group Separators.
    /// </summary>
    NonCorrespondingKeyboardLayoutsGroupSeparator = 274,

    /// <summary>
    ///   WARNING: The barcode scanner and computer keyboard layouts do not correspond when representing Record Separators.
    /// </summary>
    NonCorrespondingKeyboardLayoutsRecordSeparator = 275,

    /// <summary>
    ///   WARNING: The barcode scanner and computer keyboard layouts do not correspond when representing Record Separators.
    /// </summary>
    NonCorrespondingKeyboardLayoutsUnitSeparator = 276,

    /// <summary>
    ///   WARNING: The barcode scanner and computer keyboard layouts do not correspond when representing Record Separators.
    /// </summary>
    NonCorrespondingKeyboardLayoutsEotCharacter = 277,

    /// <summary>
    ///   WARNING: The barcode scanner and computer keyboard layouts do not correspond when representing AIM identifiers.
    /// </summary>
    NonCorrespondingKeyboardLayoutsForAimIdentifier = 278,

    /// <summary>
    ///   Warning - The correspondence of the barcode scanner and computer keyboard layouts cannot be determined.
    /// </summary>
    NonDeterminableKeyboardLayoutCorrespondence = 279,

    /// <summary>
    ///   WARNING: Caps Lock is switched on.
    /// </summary>
    CapsLockOn = 280,

    /// <summary>
    ///   WARNING: Caps Lock may be switched on.
    /// </summary>
    CapsLockProbablyOn = 281,

    /// <summary>
    ///   WARNING: Scanner may be configured to convert characters to upper case.
    /// </summary>
    ScannerMayConvertToUpperCase = 282,

    /// <summary>
    ///   WARNING: Scanner may be configured to convert characters to lower case.
    /// </summary>
    ScannerMayConvertToLowerCase = 283,

    /// <summary>
    ///   WARNING: Scanner may be configured to invert character case.
    /// </summary>
    ScannerMayInvertCase = 284,

    /// <summary>
    ///   WARNING: The best reported keyboard data entry time during calibration indicates that the barcode scanner does not perform optimally.
    /// </summary>
    SubOptimalScannerKeyboardPerformance = 290,

    /// <summary>
    /// WARNING: A pre-processing reported a warning during calibration.
    /// </summary>
    PreProcessorWarning = 299,

    /// <summary>
    ///   ERROR: No calibration data was reported.
    /// </summary>
    NoCalibrationDataReported = 300,

    /// <summary>
    ///   ERROR: The reported data is unrecognised. The wrong barcode may have been scanned.
    /// </summary>
    UnrecognisedData = 301,

    /// <summary>
    ///   ERROR: Too many characters detected. The wrong barcode may have been scanned.
    /// </summary>
    TooManyCharactersDetected = 302,

    /// <summary>
    ///   WARNING: Partial data reported for calibration barcode.
    /// </summary>
    PartialCalibrationDataReported = 303,

    /// <summary>
    ///   WARNING: The reported data is for the wrong calibration barcode.
    /// </summary>
    IncorrectCalibrationDataReported = 304,

    /// <summary>
    ///   WARNING: An attempt was made to calibrate a barcode, but no calibration token was provided.
    /// </summary>
    NoCalibrationTokenProvided = 305,

    /// <summary>
    ///   ERROR: Some invariant characters cannot be detected.
    /// </summary>
    UndetectedInvariantCharacters = 310,

    /// <summary>
    ///   ERROR: Some invariant characters are not recognised by the barcode scanner in its current configuration: {0}
    /// </summary>
    SomeInvariantCharactersUnrecognised = 311,

    /// <summary>
    ///   ERROR: Some key combinations that include invariant characters are not recognised: {0}
    /// </summary>
    SomeDeadKeyCombinationsUnrecognisedForInvariants = 312,

    /// <summary>
    ///   ERROR: No group separator is reported.
    /// </summary>
    NoGroupSeparatorMapping = 320,

    /// <summary>
    ///   ERROR: The reported character {0} is ambiguous. There are multiple keys for the same character, each representing a
    ///   different expected character.
    /// </summary>
    MultipleKeys = 360,

    /// <summary>
    ///   ERROR: The reported character {0} is ambiguous. The same character is reported for multiple dead key sequences
    ///   representing different expected characters.
    /// </summary>
    DeadKeyMultiMapping = 361,

    /// <summary>
    ///   ERROR: The reported dead key character {0} is ambiguous. There are multiple dead keys for the same character, each
    ///   representing a different expected character.
    /// </summary>
    DeadKeyMultipleKeys = 362,

    /// <summary>
    ///   ERROR: The reported character sequence {0} is ambiguous. The same sequence is reported for multiple expected
    ///   character sequences.
    /// </summary>
    MultipleSequences = 363,

    /// <summary>
    ///   ERROR: The reported character sequence {0} is ambiguous. The same sequence is reported for multiple dead keys in
    ///   the barcode scanner's keyboard layout.
    /// </summary>
    MultipleSequencesForScannerDeadKey = 364,

    /// <summary>
    ///   ERROR: The character {0} cannot be represented reliably because of incompatibility with the keyboard layout.
    /// </summary>
    IncompatibleScannerDeadKey = 365,

    /// <summary>
    ///   ERROR: The group separator character cannot be read.
    /// </summary>
    GroupSeparatorNotReadable = 366,

    /// <summary>
    ///   ERROR: The reported character sequence {0} is ambiguous.The group separator character cannot be reliably read.
    /// </summary>
    GroupSeparatorNotReliablyReadableInvariant = 367,

    /// <summary>
    ///   ERROR: Some reported characters are ambiguous. They can be reported individually but are also used to compose ligatures: {0}
    /// </summary>
    LigatureCharacters = 368,

    /// <summary>
    ///   ERROR: The reported calibration data cannot be processed. It does not include expected delimiters.
    /// </summary>
    NoDelimiters = 370,

    /// <summary>
    ///   ERROR: The reported calibration data cannot be processed. No character can be determined to act as a temporary
    ///   delimiter.
    /// </summary>
    NoTemporaryDelimiterCandidate = 371,

    /// <summary>
    ///   ERROR: Calibration failed. {0}
    /// </summary>
    CalibrationFailed = 390,

    /// <summary>
    ///   ERROR: Calibration failed unexpectedly. {0}
    /// </summary>
    CalibrationFailedUnexpectedly = 391,

    /// <summary>
    /// ERROR: Calibration failed due to a pre-processing error.
    /// </summary>
    PreProcessorError = 399,
}