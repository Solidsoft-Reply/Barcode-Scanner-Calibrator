// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Advice.cs" company="Solidsoft Reply Ltd.">
//   (c) 2020 Solidsoft Reply Ltd. All rights reserved.
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
// Provides an ordered sequence of advice items.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#pragma warning disable S3358
namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System;
using Properties;
using System.Collections.Generic;
using System.Linq;

/// <summary>
///   Provides an ordered sequence of advice items.
/// </summary>
public class Advice : IAdvice<AdviceItem, AdviceType>
{
    /// <summary>
    ///   An ordered list of advice items.
    /// </summary>
    private readonly List<AdviceItem> _adviceItems = [];

    /// <summary>
    ///   Initializes a new instance of the <see cref="Advice"/> class.
    /// </summary>
    /// <param name="systemCapabilities">The capabilities of the barcode scanner/computer combination.</param>
    private Advice(
        SystemCapabilities systemCapabilities)
    {
        var lowSeverity = new List<AdviceItem>();
        var mediumSeverity = new List<AdviceItem>();
        var highSeverity = new List<AdviceItem>();

        if (systemCapabilities is null)
        {
            throw new ArgumentNullException(nameof(systemCapabilities));
        }

        var testGs1Only = !systemCapabilities.FormatnnSupportAssessed;

        AddAdviceItemToList(
            systemCapabilities.ScannerKeyboardPerformance switch
            {
                ScannerKeyboardPerformance.Low    => ReportThatTheDataInputPerformanceIsVeryPoor(),
                ScannerKeyboardPerformance.Medium => ReportThatTheDataInputPerformanceIsSlowerThanExpected(),
                _                                 => null
            });

        // Get boolean values
#pragma warning disable CA1062 // Validate arguments of public methods
        var unexpectedError = systemCapabilities.UnexpectedError;
        var testsSucceeded = systemCapabilities.TestsSucceeded;
        var dataReported = systemCapabilities.DataReported;
        var correctSequenceReported = systemCapabilities.CorrectSequenceReported;
        var completeDataReported = systemCapabilities.CompleteDataReported;
        var keyboardLayoutsCorrespondForInvariantCharacters = systemCapabilities.KeyboardLayoutsCorrespondForInvariants;
        var keyboardLayoutsCorrespondForNonInvariantCharacters = systemCapabilities.KeyboardLayoutsCorrespondForNonInvariantCharacters;
        var keyboardLayoutsCanRepresentGroupSeparator = systemCapabilities.KeyboardLayoutsCanRepresentGroupSeparator;
        var keyboardLayoutsCanRepresentRecordSeparator = systemCapabilities.KeyboardLayoutsCanRepresentRecordSeparator;
        var keyboardLayoutsCanRepresentEdiSeparators = systemCapabilities.KeyboardLayoutsCanRepresentEdiSeparators;
        var keyboardLayoutsCorrespondForAimIdentifier = systemCapabilities.KeyboardLayoutsCorrespondForAimIdentifier;
        var canReadInvariantCharactersReliably = systemCapabilities.CanReadInvariantsReliably;
        var canReadFormat05AndFormat06Reliably = systemCapabilities.CanReadFormat05AndFormat06Reliably;
        var canReadEdiReliably = systemCapabilities.CanReadEdiReliably;
        var canReadAimIdentifiersReliably = systemCapabilities.CanReadAimIdentifiersReliably;
        var canReadNonInvariantCharactersReliably = systemCapabilities.CanReadAdditionalAsciiCharactersReliably;
        var scannerTransmitsAimIdentifiers = systemCapabilities.ScannerTransmitsAimIdentifiers;
        var scannerTransmitsEndOfLineSequence = systemCapabilities.ScannerTransmitsEndOfLineSequence;
        var scannerTransmitsAdditionalPrefix = systemCapabilities.ScannerTransmitsAdditionalPrefix;
        var scannerTransmitsAdditionalSuffix = systemCapabilities.ScannerTransmitsAdditionalSuffix;
        var scannerMayConvertToUpperCase = systemCapabilities.ScannerMayConvertToUpperCase.GetValueOrDefault();
        var scannerMayConvertToLowerCase = systemCapabilities.ScannerMayConvertToLowerCase.GetValueOrDefault();
        var scannerMayInvertCase = systemCapabilities.ScannerMayInvertCase.GetValueOrDefault();
        var scannerMayCompensateForCapsLock = systemCapabilities.ScannerMayCompensateForCapsLock.GetValueOrDefault();
        var keyboardScriptDoesNotSupportCase = systemCapabilities.KeyboardScriptDoesNotSupportCase;
        var aimIdentifierUncertain = systemCapabilities.AimIdentifierUncertain;
        var calibrationAssumption = systemCapabilities.CalibrationAssumption;
        var deadKeys = systemCapabilities.DeadKeys;
        var platform = systemCapabilities.Platform;
#pragma warning restore CA1062 // Validate arguments of public methods

        /* To facilitate reasoning over this code, I've used verbosely named methods to represent boolean expressions and
           I have nested ternary operators quite deeply in some places. "The code is the documentation".
        */

        // AdviceTypes: 100, 105, 115 (Calibration)
        AddAdviceItemToList(
            IfTheTestSucceeded()
            && IfWeCanReadInvariantCharactersReliably()
            && IfWeAssumeCalibration()
                ? IfWeOmittedTheFormatnnTest()
                    ? ReportThatInvariantCharactersAreReadReliablyButTheFormatnnTestWasOmitted()                            // 105
                    : IfWeKnowIfWeCanReadFormat05AndFormat06Reliably()
                        ? IfWeCanReadFormat05AndFormat06Reliably()
                            ? ReportThatInvariantCharactersAreReadReliably()                                                // 100
                            : ReportThatInvariantCharactersButNotFormat05OrFormat06AreReadReliably()                        // 115
                        : null
                : null);

        // AdviceTypes: 100, 105, 110, 115 (No Calibration)
        AddAdviceItemToList(
            IfTheTestSucceeded()
            && IfWeCanReadInvariantCharactersReliably()
            && IfTheKeyboardLayoutsCorrespondForInvariantCharacters()
            && IfTheKeyboardLayoutsCanRepresentGroupSeparators()
            && IfWeDoNotAssumeCalibration()
            && IfWeOmittedTheFormatnnTest()
                ? ReportThatInvariantCharactersAreReadReliablyButTheFormatnnTestWasOmitted()                                // 105
                : IfWeKnowIfWeCanReadFormat05AndFormat06Reliably()
                    ? IfWeCanReadFormat05AndFormat06Reliably()
                        ? IfWeAssumeAgnosticism()
                            ? IfTheKeyboardLayoutsCanRepresentRecordSeparators()
                                ? ReportThatInvariantCharactersAreReadReliably()                                            // 100
                            : ReportThatInvariantCharactersAreReadReliablyButFormat05OrFormat06MayNotBeReadReliably()       // 110
                            : IfWeAssumeNoCalibration()
                              && IfTheKeyboardLayoutsCannotRepresentRecordSeparators()
                              && IfWeIncludedTheFormatnnTest()
                                ? ReportThatInvariantCharactersAreReadReliablyButTheFormatnnTestWasOmitted()                // 105
                                : null
                        : IfWeIncludedTheFormatnnTest()
                          ? ReportThatInvariantCharactersButNotFormat05OrFormat06AreReadReliably()                          // 115
                          : null
                    : null);

        // AdviceType: 200
        AddAdviceItemToList(
            IfWeDoNotAscertainThatTheScannerTransmitsAimIdentifiers()
                ? ReportThatTheBarcodeScannerDoesNotTransmitAimIdentifiers()                                                // 200
                : null);

        // AdviceType: 205, 206
        AddAdviceItemToList(
            IfCapsLockIsOn() 
            && IfWeDoNotAscertainThatTheKeyboardScriptDoesNotSupportCase()
                ? IfPlatformIsMacOs()
                    ? ReportThatCapsLockIsSwitchedOnOnMacOsButCaseIsPreserved()                                             // 206
                    : IfScannerMayCompensateForCapsLock()
                        ? ReportThatCapsLockIsSwitchedOnButCaseIsReportedCorrectly()                                        // 205
                        : null
                : null);

        // AdviceType: 210
        AddAdviceItemToList(
            IfCapsLockIsOn() 
            && IfTheKeyboardScriptDoesNotSupportCase()
                ? ReportThatCapsLockIsSwitchedOnButScriptDoesNotSupportCase()                                               // 210
                : null);

        // AdviceType: 215
        AddAdviceItemToList(
            IfWeDoNotAscertainThatTheScannerTransmitsAnEndOfLineSequence()
                ? ReportThatTheScannerDoesNotTransmitAnEndOfLineSequence()                                                  // 215
                : null);

        // AdviceType: 220
        AddAdviceItemToList(
            IfScannerTransmitsAnAdditionalPrefix()
                ? ReportThatTheScannerTransmitsAPrefix()                                                                    // 220
                : null);

        // AdviceType: 225
        AddAdviceItemToList(
            IfScannerTransmitsAnAdditionalSuffix()
                ? ReportThatTheScannerTransmitsASuffix()                                                                    // 225
                : null);

        // AdviceTypes: 230, 231, 235
        AddAdviceItemToList(
            IfWeDoNotAscertainThatTheKeyboardLayoutsCorrespondForAimIdentifierFlagCharacter()
                ? IfWeCannotReadAimIdentifiersReliably()
                    ? ReportThatWeCannotReadAimIdentifiers()                                                                // 235
                    : IfWeCannotReadAimIdentifiersReliably()
                        ? IfWeAssumeAgnosticism()                                      
                            ? ReportThatWeMayNotReadAimIdentifiersAssumingAgnosticism()                                     // 230
                            : IfWeAssumeNoCalibration()
                                ? ReportThatWeMayNotReadAimIdentifiersAssumingNoCalibration()                               // 231
                                : null
                        : null
                : null);

        // AdviceTypes: 232
        AddAdviceItemToList(
            IfWeDoNotAscertainThatTheKeyboardLayoutsCorrespondForAimIdentifierFlagCharacter()
            && IfThereIsUncertaintyAboutTheDetectedAimIdentifier()
                ? ReportThatTheBarcodeScannerMayNotTransmitAimIdentifiers()                                                 // 232
                : null);

        // AdviceTypes: 240, 241, 245
        AddAdviceItemToList(
            IfDataWasFullyReported()
            && IfWeKnowIfWeCanReadFormat05AndFormat06Reliably()
                ? IfWeCanReadFormat05AndFormat06Reliably()
                    ? (IfWeKnowIfTheKeyboardLayoutsCanRepresentRecordSeparators() || IfWeKnowIfWeCanReadInvariantCharactersReliably())
                      && IfTheKeyboardLayoutsCannotRepresentRecordSeparators() 
                      && IfWeCanReadInvariantCharactersReliably()
                        ? IfWeAssumeAgnosticism()
                            ? ReportThatFormat05OrFormat06MayNotBeReadReliablyAssumingAgnosticism()                         // 240
                            : IfWeAssumeNoCalibration()
                                ? ReportThatFormat05OrFormat06MayNotBeReadReliablyAssumingNoCalibration()                   // 241
                                : null
                        : null
                    : ReportThatFormat05OrFormat06AreNotReadReliably()                                                      // 245
                : null);

        // AdviceType: 250
        AddAdviceItemToList(
            IfWeOmittedTheFormatnnTest()
                ? ReportThatWeDidNotTestForIsoIec15434()                                                                    // 250
                : null
            );

        // AdviceType: 260, 261, 265
        AddAdviceItemToList(
            IfDataWasFullyReported()
            && IfWeDoNotAscertainThatTheKeyboardLayoutsCorrespondsForNonInvariantCharacters()
                ? IfWeCannotReadNonInvariantCharactersReliably()
                    ? ReportThatTheSystemCannotReadNonInvariantCharactersReliably()                                         // 265
                    : IfWeAssumeAgnosticism()
                        ? ReportThatNonInvariantCharactersMayNotBeReadReliablyAssumingAgnosticism()                         // 260
                        : IfWeAssumeNoCalibration()
                            ? ReportThatNonInvariantCharactersMayNotBeReadReliablyAssumingNoCalibration()                   // 261
                            : null
                : null);

        // AdviceType: 270, 271, 275
        AddAdviceItemToList(
            IfDataWasFullyReported()
            && IfWeDoNotAscertainThatTheKeyboardLayoutsCanRepresentEdiSeparators()                                             // 275
                ? IfWeCannotReadEdiCharactersReliably()
                    ? ReportThatTheSystemCannotReadEdiCharactersReliably()
                    : IfWeAssumeAgnosticism()
                        ? ReportThatEdiCharactersMayNotBeReadReliablyAssumingAgnosticism()                                  // 270
                        : IfWeAssumeNoCalibration()
                            ? ReportThatEdiCharactersMayNotBeReadReliablyAssumingNoCalibration()                            // 271
                            : null
                : null);

        // AdviceType: 300
        AddAdviceItemToList(
            IfNoUnexpectedErrorOccurred()
            && IfTestDidNotSucceed() 
            && IfDataWasReported() 
                ? ReportThatTheTestFailed()                                                                                 // 300
                : null);

        // AdviceType: 301, 304
        AddAdviceItemToList(
            IfNoDataWasReported()
                ? IfDeadKeyBarcodesWereGeneratedDuringCalibration()
                    ? ReportThatNoScannedDataWasReportedForDeadKeyBarcodes()                                                // 304
                    : ReportThatNoScannedDataWasReportedForBaseLineBarcode()                                                // 301
                : null);

        // AdviceType: 303, 306
        AddAdviceItemToList(
            IfNoUnexpectedErrorOccurred()
            && IfDataWasOnlyPartiallyReported()
                ? IfDeadKeyBarcodesWereGeneratedDuringCalibration()
                    ? ReportThatScannedDataWasPartiallyReportedForDeadKeyBarcodes()                                         // 306
                    : ReportThatScannedDataWasPartiallyReportedForBaselineBarcode()                                         // 303
                : null);

        // AdviceType: 305
        AddAdviceItemToList(
            IfBarcodesWereScannedInAnIncorrectSequence()
                ? ReportThatUserScannedADeadKeyBarcodeOutOfSequence()                                                       // 305
                : null);

        // AdviceTypes: 307, 308
        AddAdviceItemToList(
            IfDataWasFullyReported()
            && IfTheKeyboardLayoutsDoNotCorrespondForInvariantCharacters()
                ? IfWeAssumeAgnosticism()
                    ? IfWeKnowIfWeCanReadInvariantCharactersReliably() 
                      || IfWeKnowIfWeCanReadFormat05AndFormat06Reliably()
                        ? IfWeCanReadInvariantCharactersReliably() 
                          && IfWeCanReadFormat05AndFormat06Reliably()
                            ? ReportThatLayoutsDoNotMatch()                                                                 // 307
                            : IfWeAssumeNoCalibration()
                                ?ReportThatLayoutsDoNotMatchForNoCalibrationAssumption()                                    // 308
                                : null
                        : null
                    : IfWeAssumeNoCalibration()
                        ? ReportThatLayoutsDoNotMatchForNoCalibrationAssumption()                                           // 308
                        : null
                : null);

        // AdviceTypes: 309, 310
        AddAdviceItemToList(
            IfDataWasFullyReported()
            && IfTheKeyboardLayoutsCorrespondForInvariantCharacters()
            && IfTheKeyboardLayoutsCannotRepresentGroupSeparators()
                ? IfWeAssumeAgnosticism()
                    ? IfWeKnowIfWeCanReadInvariantCharactersReliably() 
                      || IfWeKnowIfWeCanReadFormat05AndFormat06Reliably()
                        ? IfWeCanReadInvariantCharactersReliably() 
                          && IfWeCanReadFormat05AndFormat06Reliably()
                            ? ReportThatHiddenCharactersAreNotRepresentedCorrectly()                                        // 309
                            : IfWeAssumeNoCalibration()
                                ? ReportThatHiddenCharactersAreNotRepresentedCorrectlyAssumingNoCalibration()               // 310
                                : null
                        : null
                    : IfWeAssumeNoCalibration()
                        ? ReportThatHiddenCharactersAreNotRepresentedCorrectlyAssumingNoCalibration()                       // 310
                        : null
                : null);

        // AdviceType: 315, 316
        AddAdviceItemToList(
            IfDataWasFullyReported()
            && IfWeAssumeAgnosticism()
            && IfWeCanReadInvariantCharactersReliably()
            && IfWeCannotReadFormat05AndFormat06Reliably()
                ? IfWeKnowIfKeyboardLayoutsCorrespondForInvariants()
                    ? IfTheKeyboardLayoutsCorrespondForInvariantCharacters()
                        ? IfTheKeyboardLayoutsCannotRepresentGroupSeparators()
                            ? ReportThatHiddenCharactersAreNotReportedCorrectly()                                           // 316
                            : null
                        : ReportThatLayoutsDoNotMatchAndFormat05AndFormat06CannotBeReadReliably()                           // 315
                    : IfTheKeyboardLayoutsCannotRepresentGroupSeparators()
                        ? ReportThatHiddenCharactersAreNotReportedCorrectly()                                               // 316
                        : null
                : null);

        // Advice Type: 320
        AddAdviceItemToList(
            IfDataWasFullyReported()
            && IfWeDoNotAscertainThatWeCanReadVariantCharactersReliably()
                ? ReportThatSystemCannotReadBarcodesReliably()                                                              // 320
                : null);

        // AdviceType: 325, 326, 327, 328
        AddAdviceItemToList(
            IfCapsLockIsOn() 
            && IfWeDoNotAscertainThatTheKeyboardScriptDoesNotSupportCase()
                ? IfScannerMayConvertToUpperCase()
                    ? ReportThatCapsLockIsOnAndSystemConvertsToUpperCases()                                                 // 327
                    : IfScannerMayConvertToLowerCase()
                        ? ReportThatCapsLockIsOnAndSystemConvertsToLowerCases()                                             // 328
                        : IfTheCurrentPlatformIsMacintosh()
                            ? ReportThatCapsLockIsSwitchedOnForMacintosh()                                                  // 326
                            : ReportThatCapsLockIsSwitchedOn()                                                              // 325
                : null);

        // AdviceType: 330, 331, 332
        AddAdviceItemToList(
            IfCapsLockIsOff() 
            && IfTheKeyboardScriptDoesNotSupportCase()
                ? IfScannerMayInvertCase()
                    ? ReportThatSystemConvertsUpperAndLowerCases()                                                          // 330
                    : IfScannerMayConvertToUpperCase()
                        ? ReportThatSystemConvertsToUpperCase()                                                             // 331
                        : IfScannerMayConvertToLowerCase()
                            ? ReportThatSystemConvertsToLowerCase()                                                         // 332
                            : null
                : null);

        // Advice Type 335
        AddAdviceItemToList(
            IfDataWasFullyReported()
            && IfWeCannotReadVariantCharactersReliably()
            && IfTheKeyboardScriptDoesNotSupportCase()
                ? ReportBarcodesCannotBeReadReliablyForKeyboardScriptThatDoesNotSupportCase()                               // 335
                : null);

        // AdviceType: 390
        AddAdviceItemToList(
            IfUnexpectedErrorOccurred()
                ? ReportThatAnUnexpectedErrorWasReported()
                : null);

        // General fix-up for other issues
        // Even if ANSI MH10.8.2 barcode tests are not selected, it is possible to detect incompatibility
        // with ANSI MH10.8.2 barcodes - e.g., if [ is detected as an ambiguous character. If we report an
        // ANSI MH10.8.2 issue, it feels redundant and confusing to an end user to warn them that they
        // didn't run the ANSI MH10.8.2 tests. So, we will remove the warning if this occurs.
        if ((from ansiMh1082TestWarning in mediumSeverity
             let isAnsiMh1082Error= (from ansiMh1082Error in highSeverity
                              where ansiMh1082Error.AdviceType is
                                  AdviceType.LayoutsDoNotMatchNoFormat0506 
                                                        or AdviceType.HiddenCharactersNotReportedCorrectlyNoFormat0506
                              select ansiMh1082Error).Any()
             let isAnsiMh1082Warning = (from ansiMh1082Warning in mediumSeverity
                                 where ansiMh1082Warning.AdviceType is AdviceType.MayNotReadFormat0506 
                                                             or AdviceType.MayNotReadFormat0506NoCalibration 
                                                             or AdviceType.CannotReadAnsiMh1082Reliably
                                 select ansiMh1082Warning).Any()
             let isAnsiMh1082Info = (from ansiMh1082Info in lowSeverity
                              where ansiMh1082Info.AdviceType is AdviceType.ReadsInvariantCharactersReliablyNoFormatnnTest 
                                                       or AdviceType.ReadsInvariantCharactersReliablyMayNotReadFormat0506Reliably 
                                                       or AdviceType.ReadsInvariantCharactersReliablyExceptFormat0506
                              select ansiMh1082Info).Any()

             where (isAnsiMh1082Error || isAnsiMh1082Warning || isAnsiMh1082Info) && ansiMh1082TestWarning.AdviceType == AdviceType.Gs1OnlyTest
             select ansiMh1082TestWarning).Any())
        {
            mediumSeverity.RemoveAll(item => item.AdviceType == AdviceType.Gs1OnlyTest);
        }

        // Some non-invariant character-related warnings duplicate others. This redundancy should be removed.
        var layoutsDoNotMatchForGs1Only = highSeverity.Find(a => a.AdviceType == AdviceType.LayoutsDoNotMatchNoFormat0506);
        var hiddenCharactersNotRepresentedCorrectlyForGs1Only = highSeverity.Find(a => a.AdviceType == AdviceType.HiddenCharactersNotReportedCorrectlyNoFormat0506);

        if (layoutsDoNotMatchForGs1Only is not null || hiddenCharactersNotRepresentedCorrectlyForGs1Only is not null ) {
            var cannotReadAnsiMh1082Reliably = mediumSeverity.Find(a => a.AdviceType == AdviceType.CannotReadAnsiMh1082Reliably);

            if (cannotReadAnsiMh1082Reliably is not null) {
                mediumSeverity.Remove(cannotReadAnsiMh1082Reliably);
            }
        }

        // Fix up the situation where the system reports that the system is changing case (upper to lower, lower to upper)
        // and also that the system is compensating. Also, remove any other advice about unreliable reads.
        var convertsToUpperCase = highSeverity.Find(a => a.AdviceType == AdviceType.ConvertsToUpperCase);
        var convertsToLowerCase = highSeverity.Find(a => a.AdviceType == AdviceType.ConvertsToLowerCase);
        var capsLockOnConvertsToUpperCase = highSeverity.Find(a => a.AdviceType == AdviceType.CapsLockOnConvertsToUpperCase);
        var capsLockOnConvertsToLowerCase = highSeverity.Find(a => a.AdviceType == AdviceType.CapsLockOnConvertsToLowerCase);

        if (convertsToUpperCase is not null || convertsToLowerCase is not null || capsLockOnConvertsToUpperCase is not null || capsLockOnConvertsToLowerCase is not null) {
            var capsLockCompensation = mediumSeverity.Find(a => a.AdviceType == AdviceType.CapsLockCompensation);

            if (capsLockCompensation is not null) {
                mediumSeverity.Remove(capsLockCompensation);
            }

            var cannotReadInvariantCharactersReliably = highSeverity.Find(a => a.AdviceType == AdviceType.CannotReadBarcodesReliably);
            if (cannotReadInvariantCharactersReliably is not null)
            {
                highSeverity.Remove(cannotReadInvariantCharactersReliably);
            }

            var mayNotReadAim = mediumSeverity.Find(a => a.AdviceType == AdviceType.MayNotReadAim);
            if (mayNotReadAim is not null)
            {
                mediumSeverity.Remove(mayNotReadAim);
            }

            var mayNotReadAimNoCalibration = mediumSeverity.Find(a => a.AdviceType == AdviceType.CannotReadAimNoCalibration);
            if (mayNotReadAimNoCalibration is not null)
            {
                mediumSeverity.Remove(mayNotReadAimNoCalibration);
            }

            var cannotReadAim = mediumSeverity.Find(a => a.AdviceType == AdviceType.CannotReadAim);
            if (cannotReadAim is not null)
            {
                mediumSeverity.Remove(cannotReadAim);
            }

            var mayNotReadAnsiMh1082 = mediumSeverity.Find(a => a.AdviceType == AdviceType.MayNotReadFormat0506);
            if (mayNotReadAnsiMh1082 is not null)
            {
                mediumSeverity.Remove(mayNotReadAnsiMh1082);
            }

            var mayNotReadAnsiMh1082NoCalibration = mediumSeverity.Find(a => a.AdviceType == AdviceType.MayNotReadFormat0506NoCalibration);
            if (mayNotReadAnsiMh1082NoCalibration is not null)
            {
                mediumSeverity.Remove(mayNotReadAnsiMh1082NoCalibration);
            }

            var cannotReadAnsiMh1082Reliably = mediumSeverity.Find(a => a.AdviceType == AdviceType.CannotReadAnsiMh1082Reliably);
            if (cannotReadAnsiMh1082Reliably is not null)
            {
                mediumSeverity.Remove(cannotReadAnsiMh1082Reliably);
            }

            var mayNotReadAdditionalDataReliably = mediumSeverity.Find(a => a.AdviceType == AdviceType.MayNotReadNonInvariantCharactersReliably);
            if (mayNotReadAdditionalDataReliably is not null)
            {
                mediumSeverity.Remove(mayNotReadAdditionalDataReliably);
            }

            var mayNotReadAdditionalDataNoCalibration = mediumSeverity.Find(a => a.AdviceType == AdviceType.MayNotReadNonInvariantCharactersNoCalibration);
            if (mayNotReadAdditionalDataNoCalibration is not null)
            {
                mediumSeverity.Remove(mayNotReadAdditionalDataNoCalibration);
            }

            var cannotReadAdditionalData = mediumSeverity.Find(a => a.AdviceType == AdviceType.CannotReadNonInvariantCharacters);
            if (cannotReadAdditionalData is not null)
            {
                mediumSeverity.Remove(cannotReadAdditionalData);
            }
        }

        // Fix up the issue with CAPS LOCK where, if CAPS LOCK is reported as being on, but the system also
        // determines that te scanner appears to be automatically compensating for this, we don't need to
        // report the CAPS LOCK being on, as this is subsumed into the information about compensation.
        var capsLockOn = highSeverity.Find(a => a.AdviceType == AdviceType.CapsLockOn);
        if (capsLockOn is not null)
        {
            var capsLockCompensation = mediumSeverity.Find(a => a.AdviceType == AdviceType.CapsLockCompensation);
            if (capsLockCompensation is not null)
            {
                highSeverity.Remove(capsLockOn);
            }
        }

        // Fix up test failures. If a 301, 303, 304, 305 0r 306 error occurs, remove the 300 error, as this is
        // redundant.
        var noDataReported = highSeverity.Find(a => a.AdviceType == AdviceType.NoDataReported);
        var partialDataReported = highSeverity.Find(a => a.AdviceType == AdviceType.PartialDataReported);

        if (noDataReported is not null ||
            partialDataReported is not null ||
            highSeverity.Find(a => a.AdviceType == AdviceType.NoDataReportedDeadKeys) is not null ||
            highSeverity.Find(a => a.AdviceType == AdviceType.IncorrectSequenceDeadKeys) is not null ||
            highSeverity.Find(a => a.AdviceType == AdviceType.PartialDataReportedDeadKeys) is not null)
        {
            var testsFailed = highSeverity.Find(a => a.AdviceType == AdviceType.TestFailed);

            if (testsFailed is not null)
            {
                highSeverity.Remove(testsFailed);
            }

            if (noDataReported is not null && partialDataReported is not null)
            {
                highSeverity.Remove(partialDataReported);
            }

            if (noDataReported is not null || partialDataReported is not null)
            {
                // Remove any report about AIM identifiers, additional characters or control characters.
            }
        }

        // If the calibrator determines that your system cannot read AIM identifier characters, then it
        // should not report that your barcode scanner does not transmit AIM identifiers, as it cannot
        // determine this for certain.
        var cannotReadAimNoCalibration = highSeverity.Find(a => a.AdviceType == AdviceType.CannotReadAimNoCalibration);
        var notTransmittingAim = highSeverity.Find(a => a.AdviceType == AdviceType.NotTransmittingAim);

        if (cannotReadAimNoCalibration is not null && notTransmittingAim is not null) {
            _ = highSeverity.Remove(notTransmittingAim);
        }

        if (highSeverity.Any())
        {
            // Do not report low-severity, as these are used to represent 'green' conditions. 
            // Given that there are high-severity problems, it can be very confusing if the 
            // list also contains low-priority entries ("there is a significant problem...but all is well").
            _adviceItems.AddRange(highSeverity.OrderBy(ai => (int)ai.AdviceType));
            _adviceItems.AddRange(mediumSeverity.OrderBy(ai => (int)ai.AdviceType));
        }
        else
        {
            // Fix-up for 'green' messages.
            if (mediumSeverity.Any())
            {
                // Add a hint that there are other issues to 'green' messages. If a UI shows one
                // advice message at a time, this will improve the UX.
                for (var idx = 0; idx < lowSeverity.Count; idx++)
                {
                    lowSeverity[idx] = new AdviceItem(
                        lowSeverity[idx].AdviceType,
                        lowSeverity[idx].Condition,
                        lowSeverity[idx].Description +
                        lowSeverity[idx].Advice +
                        (mediumSeverity.Count > 1
                            ? "There are also some additional issues:"
                            : "There is also an additional issue."),
                        lowSeverity[idx].Severity);
                }
            }

            _adviceItems.AddRange(lowSeverity.OrderBy(ai => (int)ai.AdviceType));
            _adviceItems.AddRange(mediumSeverity.OrderBy(ai => (int)ai.AdviceType));
        }

        return;

        void AddAdviceItemToList(AdviceItem? adviceItem) {
            if (adviceItem?.AdviceType == AdviceType.None) return;
            switch (adviceItem?.Severity)
            {
                case ConditionSeverity.Low:
                    lowSeverity.Add(adviceItem);
                    break;
                case ConditionSeverity.Medium:
                    mediumSeverity.Add(adviceItem);
                    break;
                case ConditionSeverity.High:
                    highSeverity.Add(adviceItem);
                    break;
                case ConditionSeverity.None:
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(adviceItem),
                        adviceItem.Severity,
                        Resources.CalibrationInvalidAdviceItem);
            }
        }

        bool IfTheTestSucceeded() => testsSucceeded;
        bool IfTestDidNotSucceed() => !testsSucceeded;
        bool IfWeOmittedTheFormatnnTest() => testGs1Only;
        bool IfWeIncludedTheFormatnnTest() => !testGs1Only;
        bool IfWeKnowIfWeCanReadFormat05AndFormat06Reliably() => canReadFormat05AndFormat06Reliably is not null;
        bool IfWeCanReadFormat05AndFormat06Reliably() => canReadFormat05AndFormat06Reliably ?? false;
        bool IfWeCannotReadFormat05AndFormat06Reliably() => !canReadFormat05AndFormat06Reliably ?? false;
        bool IfWeKnowIfWeCanReadInvariantCharactersReliably() => canReadInvariantCharactersReliably is not null;
        bool IfWeCanReadInvariantCharactersReliably() => canReadInvariantCharactersReliably ?? false;
        bool IfWeCannotReadVariantCharactersReliably() => !canReadInvariantCharactersReliably ?? false;
        bool IfWeDoNotAscertainThatWeCanReadVariantCharactersReliably() => !(canReadInvariantCharactersReliably ?? false);
        bool IfTheKeyboardLayoutsCorrespondForInvariantCharacters() => keyboardLayoutsCorrespondForInvariantCharacters ?? false;
        bool IfTheKeyboardLayoutsDoNotCorrespondForInvariantCharacters() => !keyboardLayoutsCorrespondForInvariantCharacters ?? false;
        bool IfWeKnowIfKeyboardLayoutsCorrespondForInvariants() => keyboardLayoutsCorrespondForInvariantCharacters is not null;
        bool IfWeAssumeAgnosticism() => calibrationAssumption == CalibrationAssumption.Agnostic;
        bool IfWeAssumeCalibration() => calibrationAssumption == CalibrationAssumption.Calibration;
        bool IfWeDoNotAssumeCalibration() => calibrationAssumption != CalibrationAssumption.Calibration;
        bool IfWeAssumeNoCalibration() => calibrationAssumption == CalibrationAssumption.NoCalibration;
        bool IfTheCurrentPlatformIsMacintosh() => platform == SupportedPlatform.Macintosh;
        bool IfTheKeyboardLayoutsCanRepresentRecordSeparators() => keyboardLayoutsCanRepresentRecordSeparator ?? false;
        bool IfTheKeyboardLayoutsCannotRepresentRecordSeparators() => !keyboardLayoutsCanRepresentRecordSeparator ?? false;
        bool IfWeKnowIfTheKeyboardLayoutsCanRepresentRecordSeparators() => keyboardLayoutsCanRepresentRecordSeparator is not null;
        bool IfTheKeyboardLayoutsCanRepresentGroupSeparators() => keyboardLayoutsCanRepresentGroupSeparator ?? false;
        bool IfWeDoNotAscertainThatTheKeyboardLayoutsCanRepresentEdiSeparators() => !(keyboardLayoutsCanRepresentEdiSeparators ?? false);
        bool IfCapsLockIsOn() => systemCapabilities.CapsLock;
        bool IfCapsLockIsOff() => !systemCapabilities.CapsLock;
        bool IfWeDoNotAscertainThatTheKeyboardScriptDoesNotSupportCase() => !(keyboardScriptDoesNotSupportCase ?? false);
        bool IfTheKeyboardScriptDoesNotSupportCase() => keyboardScriptDoesNotSupportCase ?? false;
        bool IfPlatformIsMacOs() => platform == SupportedPlatform.Macintosh;
        bool IfScannerMayCompensateForCapsLock() => scannerMayCompensateForCapsLock;
        bool IfWeDoNotAscertainThatTheScannerTransmitsAimIdentifiers() => !(scannerTransmitsAimIdentifiers ?? false);
        bool IfWeDoNotAscertainThatTheScannerTransmitsAnEndOfLineSequence() => !(scannerTransmitsEndOfLineSequence ?? false);
        bool IfScannerTransmitsAnAdditionalPrefix() => scannerTransmitsAdditionalPrefix;
        bool IfScannerTransmitsAnAdditionalSuffix() => scannerTransmitsAdditionalSuffix;
        bool IfWeDoNotAscertainThatTheKeyboardLayoutsCorrespondForAimIdentifierFlagCharacter() => !(keyboardLayoutsCorrespondForAimIdentifier ?? false);
        bool IfWeCannotReadAimIdentifiersReliably() => !canReadAimIdentifiersReliably ?? false;
        bool IfThereIsUncertaintyAboutTheDetectedAimIdentifier() => aimIdentifierUncertain;
        bool IfWeDoNotAscertainThatTheKeyboardLayoutsCorrespondsForNonInvariantCharacters() => !(keyboardLayoutsCorrespondForNonInvariantCharacters ?? false);
        bool IfWeCannotReadNonInvariantCharactersReliably() => !canReadNonInvariantCharactersReliably ?? false;
        bool IfWeCannotReadEdiCharactersReliably() => !canReadEdiReliably ?? false;
        bool IfUnexpectedErrorOccurred() => unexpectedError;
        bool IfNoUnexpectedErrorOccurred() => !unexpectedError;
        bool IfDataWasReported() => dataReported;
        bool IfNoDataWasReported() => !dataReported;
        bool IfDataWasFullyReported() => completeDataReported;
        bool IfDataWasOnlyPartiallyReported() => !completeDataReported;
        bool IfDeadKeyBarcodesWereGeneratedDuringCalibration() => deadKeys;
        bool IfBarcodesWereScannedInAnIncorrectSequence() => !correctSequenceReported;
        bool IfTheKeyboardLayoutsCannotRepresentGroupSeparators() => !keyboardLayoutsCanRepresentGroupSeparator ?? false;
        bool IfScannerMayConvertToUpperCase() => scannerMayConvertToUpperCase;
        bool IfScannerMayConvertToLowerCase() => scannerMayConvertToLowerCase;
        bool IfScannerMayInvertCase() => scannerMayInvertCase;

        // 100
        AdviceItem ReportThatInvariantCharactersAreReadReliably () => new(AdviceType.ReadsInvariantCharactersReliably);

        // 105
        AdviceItem ReportThatInvariantCharactersAreReadReliablyButTheFormatnnTestWasOmitted() =>
            new(AdviceType.ReadsInvariantCharactersReliablyNoFormatnnTest);

        // 110
        AdviceItem ReportThatInvariantCharactersAreReadReliablyButFormat05OrFormat06MayNotBeReadReliably() =>
            new(AdviceType.ReadsInvariantCharactersReliablyMayNotReadFormat0506Reliably);

        // 115
        AdviceItem ReportThatInvariantCharactersButNotFormat05OrFormat06AreReadReliably() =>
            new(AdviceType.ReadsInvariantCharactersReliablyExceptFormat0506);

        // 200
        AdviceItem ReportThatTheBarcodeScannerDoesNotTransmitAimIdentifiers() =>
            new(AdviceType.NotTransmittingAim);

        // 205
        AdviceItem ReportThatCapsLockIsSwitchedOnButCaseIsReportedCorrectly() =>
            new(AdviceType.CapsLockCompensation);

        // 206
        AdviceItem ReportThatCapsLockIsSwitchedOnOnMacOsButCaseIsPreserved() =>
            new(AdviceType.CapsLockOnPreservationMacintosh);

        // 210
        AdviceItem ReportThatCapsLockIsSwitchedOnButScriptDoesNotSupportCase() =>
            new(AdviceType.CapsLockOnNoCase);

        // 215
        AdviceItem ReportThatTheScannerDoesNotTransmitAnEndOfLineSequence() =>
            new(AdviceType.NotTransmittingEndOfLine);

        // 220
        AdviceItem ReportThatTheScannerTransmitsAPrefix() =>
            new(AdviceType.TransmittingPrefix);

        // 225
        AdviceItem ReportThatTheScannerTransmitsASuffix() =>
            new(AdviceType.TransmittingSuffix);

        // 230
        AdviceItem ReportThatWeMayNotReadAimIdentifiersAssumingAgnosticism() =>
            new(AdviceType.MayNotReadAim);

        // 231
        AdviceItem ReportThatWeMayNotReadAimIdentifiersAssumingNoCalibration() =>
            new(AdviceType.CannotReadAimNoCalibration);
        
        // 232
        AdviceItem ReportThatTheBarcodeScannerMayNotTransmitAimIdentifiers() => 
            new(AdviceType.MayNotTransmitAim);

        // 235
        AdviceItem ReportThatWeCannotReadAimIdentifiers() => 
            new(AdviceType.CannotReadAim);

        // 240
        AdviceItem ReportThatFormat05OrFormat06MayNotBeReadReliablyAssumingAgnosticism() => 
            new(AdviceType.MayNotReadFormat0506);

        // 241
        AdviceItem ReportThatFormat05OrFormat06MayNotBeReadReliablyAssumingNoCalibration() => 
            new(AdviceType.MayNotReadFormat0506NoCalibration);

        // 245
        AdviceItem ReportThatFormat05OrFormat06AreNotReadReliably() => 
            new(AdviceType.CannotReadAnsiMh1082Reliably);

        // 250
        AdviceItem ReportThatWeDidNotTestForIsoIec15434() => 
            new(AdviceType.Gs1OnlyTest);

        // 255
        AdviceItem ReportThatTheDataInputPerformanceIsSlowerThanExpected() =>
            new(AdviceType.SlowScannerPerformance);

        // 256
        AdviceItem ReportThatTheDataInputPerformanceIsVeryPoor() =>
            new(AdviceType.VerySlowScannerPerformance);

        // 260
        AdviceItem ReportThatNonInvariantCharactersMayNotBeReadReliablyAssumingAgnosticism() =>
            new (AdviceType.MayNotReadNonInvariantCharactersReliably);

        // 261
        AdviceItem ReportThatNonInvariantCharactersMayNotBeReadReliablyAssumingNoCalibration() =>
            new (AdviceType.MayNotReadNonInvariantCharactersNoCalibration);

        // 265
        AdviceItem ReportThatTheSystemCannotReadNonInvariantCharactersReliably() =>
            new(AdviceType.CannotReadNonInvariantCharacters);

        // 270
        AdviceItem ReportThatEdiCharactersMayNotBeReadReliablyAssumingAgnosticism() =>
            new(AdviceType.MayNotReadEdiCharactersReliably);

        // 271
        AdviceItem ReportThatEdiCharactersMayNotBeReadReliablyAssumingNoCalibration() =>
            new(AdviceType.MayNotReadEdiCharactersNoCalibration);

        // 275
        AdviceItem ReportThatTheSystemCannotReadEdiCharactersReliably() =>
            new(AdviceType.CannotReadEdiCharacters);

        // 300
        AdviceItem ReportThatTheTestFailed() =>
            new(AdviceType.TestFailed);

        // 301
        AdviceItem ReportThatNoScannedDataWasReportedForBaseLineBarcode() =>
            new(AdviceType.NoDataReported);

        // 303
        AdviceItem ReportThatScannedDataWasPartiallyReportedForBaselineBarcode() =>
            new(AdviceType.PartialDataReported);

        // 304
        AdviceItem ReportThatNoScannedDataWasReportedForDeadKeyBarcodes() =>
            new(AdviceType.NoDataReportedDeadKeys);

        // 305
        AdviceItem ReportThatUserScannedADeadKeyBarcodeOutOfSequence() =>
            new(AdviceType.IncorrectSequenceDeadKeys);

        // 306
        AdviceItem ReportThatScannedDataWasPartiallyReportedForDeadKeyBarcodes() =>
            new(AdviceType.PartialDataReportedDeadKeys);

        // 307
        AdviceItem ReportThatLayoutsDoNotMatch() => 
            new(AdviceType.LayoutsDoNotMatch);

        // 308
        AdviceItem ReportThatLayoutsDoNotMatchForNoCalibrationAssumption() => 
            new (AdviceType.LayoutsDoNotMatchNoCalibration);

        // 309
        AdviceItem ReportThatHiddenCharactersAreNotRepresentedCorrectly() =>
            new (AdviceType.HiddenCharactersNotRepresentedCorrectly);

        // 310
        AdviceItem ReportThatHiddenCharactersAreNotRepresentedCorrectlyAssumingNoCalibration() =>
            new (AdviceType.HiddenCharactersNotRepresentedCorrectlyNoCalibration);

        // 315
        AdviceItem ReportThatLayoutsDoNotMatchAndFormat05AndFormat06CannotBeReadReliably() =>
            new(AdviceType.LayoutsDoNotMatchNoFormat0506);

        // 316
        AdviceItem ReportThatHiddenCharactersAreNotReportedCorrectly() =>
            new(AdviceType.HiddenCharactersNotReportedCorrectlyNoFormat0506);

        // 320
        AdviceItem ReportThatSystemCannotReadBarcodesReliably() =>
            new(AdviceType.CannotReadBarcodesReliably);

        // 325
        AdviceItem ReportThatCapsLockIsSwitchedOn() =>
            new(AdviceType.CapsLockOn);

        // 326
        AdviceItem ReportThatCapsLockIsSwitchedOnForMacintosh() =>
            new(AdviceType.CapsLockOnMacintosh);

        // 327
        AdviceItem ReportThatCapsLockIsOnAndSystemConvertsToUpperCases() =>
            new(AdviceType.CapsLockOnConvertsToUpperCase);

        // 328
        AdviceItem ReportThatCapsLockIsOnAndSystemConvertsToLowerCases() =>
            new(AdviceType.CapsLockOnConvertsToLowerCase);

        // 330
        AdviceItem ReportThatSystemConvertsUpperAndLowerCases() =>
            new(AdviceType.CaseIsSwitched);

        // 331
        AdviceItem ReportThatSystemConvertsToUpperCase() =>
            new(AdviceType.ConvertsToUpperCase);

        // 332
        AdviceItem ReportThatSystemConvertsToLowerCase() =>
            new(AdviceType.ConvertsToLowerCase);

        // 335
        AdviceItem ReportBarcodesCannotBeReadReliablyForKeyboardScriptThatDoesNotSupportCase() =>
            new(AdviceType.NoSupportForCase, systemCapabilities.KeyboardScript);

        // 390
        AdviceItem ReportThatAnUnexpectedErrorWasReported() =>
            new(AdviceType.UnexpectedErrorReported);
    }

    /// <summary>
    ///   Factory method for creating new Advice.
    /// </summary>
    /// <param name="systemCapabilities"></param>
    /// <returns>An ordered sequence of advice items.</returns>
    // ReSharper disable once UnusedMember.Global
    public static Advice CreateAdvice(SystemCapabilities systemCapabilities) {
        return new Advice(systemCapabilities);
    }

    /// <summary>
    ///   Gets an ordered collection of advice items.
    /// </summary>
    /// <returns>An ordered collection of advice items.</returns>
    public IEnumerable<AdviceItem> Items => _adviceItems;
}