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
    private readonly List<AdviceItem> _adviceItems = new();

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
                ScannerKeyboardPerformance.Low    => new AdviceItem(AdviceType.VerySlowScannerPerformance),
                ScannerKeyboardPerformance.Medium => new AdviceItem(AdviceType.SlowScannerPerformance),
                _                                 => null
            });

        // Get boolean values
#pragma warning disable CA1062 // Validate arguments of public methods
        var testsSucceeded = systemCapabilities.TestsSucceeded;
        var dataReported = systemCapabilities.DataReported;
        var correctSequenceReported = systemCapabilities.CorrectSequenceReported;
        var completeDataReported = systemCapabilities.CompleteDataReported;
        var keyboardLayoutsCorrespondForInvariants = systemCapabilities.KeyboardLayoutsCorrespondForInvariants;
        var keyboardLayoutsCorrespondForAdditionalAsciiCharacters = systemCapabilities.KeyboardLayoutsCorrespondForNonInvariantCharacters;
        var keyboardLayoutsCanRepresentGroupSeparator = systemCapabilities.KeyboardLayoutsCanRepresentGroupSeparator;
        var keyboardLayoutsCanRepresentRecordSeparator = systemCapabilities.KeyboardLayoutsCanRepresentRecordSeparator;
        var keyboardLayoutsCanRepresentEdiSeparator = systemCapabilities.KeyboardLayoutsCanRepresentEdiSeparator;
        var keyboardLayoutsCorrespondForAimIdentifier = systemCapabilities.KeyboardLayoutsCorrespondForAimIdentifier;
        var canReadInvariantsReliably = systemCapabilities.CanReadInvariantsReliably;
        var canReadFormat05AndFormat06Reliably = systemCapabilities.CanReadFormat05AndFormat06Reliably;
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

        // AdviceType: 300
        AddAdviceItemToList(!testsSucceeded && dataReported ? new AdviceItem(AdviceType.TestsFailed) : null);

        // AdviceType: 301, 304
        AddAdviceItemToList(!dataReported ? deadKeys ? new AdviceItem(AdviceType.NoDataReportedDeadKeys) : new AdviceItem(AdviceType.NoDataReported) : null);

        // AdviceType: 305
        AddAdviceItemToList(!correctSequenceReported ? new AdviceItem(AdviceType.IncorrectSequenceDeadKeys) : null);

        // AdviceType: 303, 306
        AddAdviceItemToList(!completeDataReported ? deadKeys ? new AdviceItem(AdviceType.PartialDataReportedDeadKeys) : new AdviceItem(AdviceType.PartialDataReported) : null);

        // AdviceTypes: 307, 308
        AddAdviceItemToList(
            keyboardLayoutsCorrespondForInvariants.Map(inv => !inv) ?? false
                ? TestLayoutsDoNotMatch()
                : null);

        // AdviceTypes: 309, 310
        AddAdviceItemToList(
            keyboardLayoutsCorrespondForInvariants.Map(inv => inv) ?? false
                ? keyboardLayoutsCanRepresentGroupSeparator.Map(inv => !inv) ?? false
                    ? TestHiddenCharactersNotRepresentedCorrectly()
                    : null
                : null);

        // AdviceType: 315, 316
        AddAdviceItemToList(
            calibrationAssumption == CalibrationAssumption.Agnostic
                ? canReadInvariantsReliably.Map(readRead => readRead) ?? false
                    ? canReadFormat05AndFormat06Reliably.Map(readRead => !readRead) ?? false
                        ? TestLayoutsDoNotMatchForGs1Only()
                        : null
                    : null
                : null);

        // Advice Type: 320
        AddAdviceItemToList(
            !(canReadInvariantsReliably.Map(read => read) ?? false)
                ? new AdviceItem(AdviceType.CannotReadBarcodesReliably)
                : null);

        // Advice Type 335
        AddAdviceItemToList(
            canReadInvariantsReliably.Map(readRead => !readRead) ?? false
                ? keyboardScriptDoesNotSupportCase.Map(@case => @case) ?? false
                    ? new AdviceItem(AdviceType.NoSupportForCase, systemCapabilities.KeyboardScript)
                    : null
                : null);

        if (systemCapabilities.CapsLock)
        {
            AdviceItem? TestCapsLockCompensation() =>
                scannerMayCompensateForCapsLock
                    ? new AdviceItem(AdviceType.CapsLockCompensation)
                    : null;

            AdviceItem? TestCapsLockOnPreservationMacintosh() =>
                platform == SupportedPlatform.Macintosh
                    ? new AdviceItem(AdviceType.CapsLockOnPreservationMacintosh)
                    : TestCapsLockCompensation();

            // AdviceType: 205, 206
            AddAdviceItemToList(
                !(keyboardScriptDoesNotSupportCase.Map(@case => @case) ?? false)
                    ? TestCapsLockOnPreservationMacintosh()
                    : null);

            // AdviceType: 210
            AddAdviceItemToList(
                keyboardScriptDoesNotSupportCase.Map(@case => @case) ?? false
                    ? new AdviceItem(AdviceType.CapsLockOnNoCase)
                    : null);

            AdviceItem TestCapsLockOnMacintosh() =>
                platform == SupportedPlatform.Macintosh
                    ? new AdviceItem(AdviceType.CapsLockOnMacintosh)
                    : new AdviceItem(AdviceType.CapsLockOn);

            AdviceItem TestCapsLockOnConvertsToLowerCase() =>
                scannerMayConvertToLowerCase
                    ? new AdviceItem(AdviceType.CapsLockOnConvertsToLowerCase)
                    : TestCapsLockOnMacintosh();

            AdviceItem TestCapsLockOnConvertsToUpperCase() =>
                scannerMayConvertToUpperCase
                    ? new AdviceItem(AdviceType.CapsLockOnConvertsToUpperCase)
                    : TestCapsLockOnConvertsToLowerCase();

            // AdviceType: 325, 326, 327, 328
            AddAdviceItemToList(
                !(keyboardScriptDoesNotSupportCase.Map(@case => @case) ?? false)
                    ? TestCapsLockOnConvertsToUpperCase()
                    : null);
        }
        else
        {
            AdviceItem? TestConvertsToLowerCase() =>
                scannerMayConvertToLowerCase
                    ? new AdviceItem(AdviceType.ConvertsToLowerCase)
                    : null;

            AdviceItem? TestConvertsToUpperCase() =>
                scannerMayConvertToUpperCase
                    ? new AdviceItem(AdviceType.ConvertsToUpperCase)
                    : TestConvertsToLowerCase();

            AdviceItem? TestCaseIsSwitched() =>
                scannerMayInvertCase
                    ? new AdviceItem(AdviceType.CaseIsSwitched)
                    : TestConvertsToUpperCase();

            // AdviceType: 330, 331, 332
            AddAdviceItemToList(
                !(keyboardScriptDoesNotSupportCase.Map(@case => @case) ?? false)
                    ? TestCaseIsSwitched()
                    : null);
        }

        // AdviceType: 200
        AddAdviceItemToList(
            !(scannerTransmitsAimIdentifiers ?? false)
                ? new AdviceItem(AdviceType.NotTransmittingAim)
                : null);

        // AdviceType: 215
        AddAdviceItemToList(
            !(scannerTransmitsEndOfLineSequence ?? false)
                ? new AdviceItem(AdviceType.NotTransmittingEndOfLine)
                : null);

        // AdviceType: 220
        AddAdviceItemToList(
            scannerTransmitsAdditionalPrefix
                ? new AdviceItem(AdviceType.TransmittingPrefix)
                : null);

        // AdviceType: 225
        AddAdviceItemToList(
            scannerTransmitsAdditionalSuffix
                ? new AdviceItem(AdviceType.TransmittingSuffix)
                : null);

        // AdviceType: 260, 261, 265
        AddAdviceItemToList(
            !(keyboardLayoutsCorrespondForAdditionalAsciiCharacters.Map(ascii => ascii) ?? false)
                ? canReadNonInvariantCharactersReliably.Map(readRead => !readRead) ?? false
                    ? new AdviceItem(AdviceType.CannotReadNonInvariantCharacters)
                    : TestMayNotReadNonInvariantCharacters()
                : null);

        // AdviceTypes: 230, 231, 235
        AddAdviceItemToList(
            !(keyboardLayoutsCorrespondForAimIdentifier.Map(aim => aim) ?? false)
                ? canReadAimIdentifiersReliably.Map(readAim => !readAim) ?? false
                    ? new AdviceItem(AdviceType.CannotReadAim)
                    : calibrationAssumption switch
                    {
                        CalibrationAssumption.Agnostic => new AdviceItem(AdviceType.MayNotReadAim),
                        CalibrationAssumption.NoCalibration => new AdviceItem(AdviceType.MayNotReadAimNoCalibration),
                        _ => null
                    }
                : null);

        // AdviceTypes: 232
        AddAdviceItemToList(
            !(keyboardLayoutsCorrespondForAimIdentifier.Map(aim => aim) ?? false)
                ? !aimIdentifierUncertain
                    ? null
                    : new AdviceItem(AdviceType.MayNotTransmitAim)
                : null);

        if (testGs1Only)
        {
            // AdviceType: 250
            AddAdviceItemToList(new AdviceItem(AdviceType.Gs1OnlyTest));
        }

        // AdviceTypes: 240, 241, 245
        AddAdviceItemToList(TestCannotReadNonInvariantCharacterReliably());

        // AdviceTypes: 100, 105, 110, 115 (Not Calibration)
        AddAdviceItemToList(
            canReadInvariantsReliably.Map(read => read) ?? false
                ? keyboardLayoutsCorrespondForInvariants.Map(inv => inv) ?? false
                    ? (keyboardLayoutsCanRepresentGroupSeparator.Map(repGs => repGs) ?? false) &&
                      testsSucceeded &&
                      calibrationAssumption != CalibrationAssumption.Calibration
                        ? TestReadsInvariantCharactersReliablyForGs1OnlyTest()
                        : null
                    : null
                : null);

        // AdviceTypes: 100, 105, 155 (Calibration)
        AddAdviceItemToList(
            canReadInvariantsReliably is null
                ? null
                : testsSucceeded &&
                  canReadInvariantsReliably is false &&
                  calibrationAssumption == CalibrationAssumption.Calibration
                    ? TestSupportForFormat06()
                    : null);

        // General fix-up for other issues
        // Even if ANSI MH10.8.2 barcode tests are not selected, it is possible to detect incompatibility
        // with ANSI MH10.8.2 barcodes - e.g., if [ is detected as an ambiguous character. If we report an
        // ANSI MH10.8.2 issue, it feels redundant and confusing to an end user to warn them that they
        // didn't run the ANSI MH10.8.2 tests. So, we will remove the warning if this occurs.
        if ((from ansiMh1082TestWarning in mediumSeverity
             let isAnsiMh1082Error= (from ansiMh1082Error in highSeverity
                              where ansiMh1082Error.AdviceType is
                                  AdviceType.LayoutsDoNotMatchForGs1Only 
                                                        or AdviceType.HiddenCharactersNotRepresentedCorrectlyForGs1Only
                              select ansiMh1082Error).Any()
             let isAnsiMh1082Warning = (from ansiMh1082Warning in mediumSeverity
                                 where ansiMh1082Warning.AdviceType is AdviceType.MayNotReadAnsiMh1082 
                                                             or AdviceType.MayNotReadAnsiMh1082NoCalibration 
                                                             or AdviceType.CannotReadAnsiMh1082Reliably
                                 select ansiMh1082Warning).Any()
             let isAnsiMh1082Info = (from ansiMh1082Info in lowSeverity
                              where ansiMh1082Info.AdviceType is AdviceType.ReadsInvariantCharactersReliablyForGs1OnlyTest 
                                                       or AdviceType.ReadsInvariantCharactersReliablyMayNotReadAnsiMh1082 
                                                       or AdviceType.ReadsInvariantCharactersReliablyExceptAnsiMh1082
                              select ansiMh1082Info).Any()

             where (isAnsiMh1082Error || isAnsiMh1082Warning || isAnsiMh1082Info) && ansiMh1082TestWarning.AdviceType == AdviceType.Gs1OnlyTest
             select ansiMh1082TestWarning).Any())
        {
            mediumSeverity.RemoveAll(item => item.AdviceType == AdviceType.Gs1OnlyTest);
        }

        // Some non-invariant character-related warnings duplicate others. This redundancy should be removed.
        var layoutsDoNotMatchForGs1Only = highSeverity.Find(a => a.AdviceType == AdviceType.LayoutsDoNotMatchForGs1Only);
        var hiddenCharactersNotRepresentedCorrectlyForGs1Only = highSeverity.Find(a => a.AdviceType == AdviceType.HiddenCharactersNotRepresentedCorrectlyForGs1Only);

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

            var mayNotReadAimNoCalibration = mediumSeverity.Find(a => a.AdviceType == AdviceType.MayNotReadAimNoCalibration);
            if (mayNotReadAimNoCalibration is not null)
            {
                mediumSeverity.Remove(mayNotReadAimNoCalibration);
            }

            var cannotReadAim = mediumSeverity.Find(a => a.AdviceType == AdviceType.CannotReadAim);
            if (cannotReadAim is not null)
            {
                mediumSeverity.Remove(cannotReadAim);
            }

            var mayNotReadAnsiMh1082 = mediumSeverity.Find(a => a.AdviceType == AdviceType.MayNotReadAnsiMh1082);
            if (mayNotReadAnsiMh1082 is not null)
            {
                mediumSeverity.Remove(mayNotReadAnsiMh1082);
            }

            var mayNotReadAnsiMh1082NoCalibration = mediumSeverity.Find(a => a.AdviceType == AdviceType.MayNotReadAnsiMh1082NoCalibration);
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
            var testsFailed = highSeverity.Find(a => a.AdviceType == AdviceType.TestsFailed);

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

        AdviceItem? TestHiddenCharactersNotRepresentedCorrectlyNoCalibration() =>
            calibrationAssumption == CalibrationAssumption.NoCalibration
                ? new AdviceItem(AdviceType.HiddenCharactersNotRepresentedCorrectlyNoCalibration)
                : null;

        AdviceItem? TestHiddenCharactersNotRepresentedCorrectly() =>
            calibrationAssumption == CalibrationAssumption.Agnostic
                ? canReadInvariantsReliably is null && canReadFormat05AndFormat06Reliably is null
                    ? null
                    : (canReadInvariantsReliably ?? false) &&
                      (canReadFormat05AndFormat06Reliably ?? false)
                        ? new AdviceItem(AdviceType.HiddenCharactersNotRepresentedCorrectly)
                        : TestHiddenCharactersNotRepresentedCorrectlyNoCalibration()
                : TestHiddenCharactersNotRepresentedCorrectlyNoCalibration();

        void AddAdviceItemToList(AdviceItem? adviceItem)
        {
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

        AdviceItem? TestSupportForFormat06() =>
            testGs1Only
                ? new AdviceItem(AdviceType.ReadsInvariantCharactersReliablyForGs1OnlyTest)
                : TestFormat06CanReadFormat05AndFormat06Reliably();

        AdviceItem? TestFormat06CanReadFormat05AndFormat06Reliably() =>
            canReadFormat05AndFormat06Reliably is null
                ? null
                : (bool)canReadFormat05AndFormat06Reliably
                    ? new AdviceItem(AdviceType.ReadsInvariantCharactersReliably)
                    : new AdviceItem(AdviceType.ReadsInvariantCharactersReliablyExceptAnsiMh1082);

        AdviceItem? TestReadsInvariantCharactersReliablyForGs1OnlyTest() =>
            testGs1Only
                ? new AdviceItem(AdviceType.ReadsInvariantCharactersReliablyForGs1OnlyTest)
                : TestCanReadFormat05AndFormat06Reliably();


        AdviceItem? TestCanReadFormat05AndFormat06Reliably() =>
            canReadFormat05AndFormat06Reliably is null
                ? null
                : (bool)canReadFormat05AndFormat06Reliably
                    ? TestNotCalibrationIsAgnostic()
                    : Test006UnreliableReadsInvariantCharactersReliablyExceptAnsiMh1082();

        AdviceItem? TestNotCalibrationIsAgnostic() =>
            calibrationAssumption == CalibrationAssumption.Agnostic
                ? TestReadsInvariantCharactersReliably()
                : TestReadsInvariantCharactersReliablyExceptAnsiMh1082();

        AdviceItem? Test006UnreliableReadsInvariantCharactersReliablyExceptAnsiMh1082() =>
            !testGs1Only
                ? new AdviceItem(AdviceType.ReadsInvariantCharactersReliablyExceptAnsiMh1082)
                : null;


        AdviceItem? TestReadsInvariantCharactersReliably() =>
            keyboardLayoutsCanRepresentRecordSeparator ?? false
                ? new AdviceItem(AdviceType.ReadsInvariantCharactersReliably)
                : TestReadsInvariantCharactersReliablyMayNotReadAnsiMh1082();

        AdviceItem? TestReadsInvariantCharactersReliablyExceptAnsiMh1082() =>
            calibrationAssumption == CalibrationAssumption.NoCalibration &&
            (keyboardLayoutsCanRepresentRecordSeparator.Map(repRs => !repRs) ?? false) && 
            !testGs1Only
                ? new AdviceItem(AdviceType.ReadsInvariantCharactersReliablyExceptAnsiMh1082)
                : null;

        AdviceItem? TestReadsInvariantCharactersReliablyMayNotReadAnsiMh1082() =>
            testGs1Only
                ? new AdviceItem(AdviceType.ReadsInvariantCharactersReliablyMayNotReadAnsiMh1082)
                : null;

        AdviceItem? TestCannotReadNonInvariantCharacterReliably() =>
            canReadFormat05AndFormat06Reliably is null
                ? null
                : (bool)canReadFormat05AndFormat06Reliably
                    ? TestMayNotReadAnsiMh1082()
                    : new AdviceItem(AdviceType.CannotReadAnsiMh1082Reliably);

        AdviceItem? TestMayNotReadAnsiMh1082() =>
            keyboardLayoutsCanRepresentRecordSeparator is null && canReadInvariantsReliably is null
                ? null
                : (keyboardLayoutsCanRepresentRecordSeparator.Map(repRs => !repRs) ?? false) &&
                  (canReadInvariantsReliably ?? false)
                    ? calibrationAssumption switch {
                        CalibrationAssumption.Agnostic => new AdviceItem(AdviceType.MayNotReadAnsiMh1082),
                        CalibrationAssumption.NoCalibration => new AdviceItem(AdviceType.MayNotReadAnsiMh1082NoCalibration),
                        _ => null
                    }
                    : null;

        AdviceItem? TestMayNotReadNonInvariantCharacters() =>
            calibrationAssumption switch
            {
                CalibrationAssumption.Agnostic => new AdviceItem(AdviceType.MayNotReadNonInvariantCharactersReliably),
                CalibrationAssumption.NoCalibration => new AdviceItem(AdviceType.MayNotReadNonInvariantCharactersNoCalibration),
                _ => null
            };

        AdviceItem? TestLayoutsDoNotMatchForGs1Only() =>
            keyboardLayoutsCorrespondForInvariants is null
                ? TestHiddenCharactersNotRepresentedCorrectlyForGs1Only()
                : !(bool)keyboardLayoutsCorrespondForInvariants
                    ? new AdviceItem(AdviceType.LayoutsDoNotMatchForGs1Only)
                    : TestHiddenCharactersNotRepresentedCorrectlyForGs1Only();

        AdviceItem? TestHiddenCharactersNotRepresentedCorrectlyForGs1Only() =>
            keyboardLayoutsCanRepresentGroupSeparator is null
                ? null
                : !(bool)keyboardLayoutsCanRepresentGroupSeparator
                    ? new AdviceItem(AdviceType.HiddenCharactersNotRepresentedCorrectlyForGs1Only)
                    : null;

        AdviceItem? TestLayoutsDoNotMatch() =>
            calibrationAssumption == CalibrationAssumption.Agnostic
                ? canReadInvariantsReliably is null && canReadFormat05AndFormat06Reliably is null
                    ? null
                    : (canReadInvariantsReliably ?? false) && (canReadFormat05AndFormat06Reliably ?? false)
                        ? new AdviceItem(AdviceType.LayoutsDoNotMatch)
                        : TestLayoutsDoNotMatchNoCalibration()
                : TestLayoutsDoNotMatchNoCalibration();

        AdviceItem? TestLayoutsDoNotMatchNoCalibration()
            => calibrationAssumption == CalibrationAssumption.NoCalibration
                ? new AdviceItem(AdviceType.LayoutsDoNotMatchNoCalibration)
                : null;
    }

    /// <summary>
    ///   Factory method for creating new Advice.
    /// </summary>
    /// <param name="systemCapabilities"></param>
    /// <returns>An ordered sequence of advice items.</returns>
    public static Advice CreateAdvice(SystemCapabilities systemCapabilities) {
        return new Advice(systemCapabilities);
    }

    /// <summary>
    ///   Gets an ordered collection of advice items.
    /// </summary>
    /// <returns>An ordered collection of advice items.</returns>
    public IEnumerable<AdviceItem> Items => _adviceItems;
}