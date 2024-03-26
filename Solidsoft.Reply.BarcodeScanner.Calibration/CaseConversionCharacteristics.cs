// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CaseConversionCharacteristics.cs" company="Solidsoft Reply Ltd.">
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
// Platforms supported for calibration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
///   Computes the case conversion characteristics of the system based on observations.
/// </summary>
internal class CaseConversionCharacteristics
{
    /// <summary>
    ///   A list of segment characters.
    /// </summary>
    private readonly IReadOnlyList<string> _segmentCharacters;

    private bool _upperCaseConversionDetected;
    private bool _lowerCaseConversionDetected;

    /// <summary>
    ///   Initializes a new instance of the <see cref="CaseConversionCharacteristics"/> class.
    /// </summary>
    /// <param name="segmentCharacters">
    ///   The characters in the segment of reported characters.
    /// </param>
    /// <param name="keyboardScript">
    ///   The Unicode block name for the script represented by the OS-configured keyboard layout.
    /// </param>
    /// <param name="capsLockState">The reported Caps Lock state.</param>
    public CaseConversionCharacteristics(
        IReadOnlyList<string> segmentCharacters,
        string keyboardScript = "Latin",
        bool? capsLockState = null)
    {
        KeyboardScript = keyboardScript;
        _segmentCharacters = segmentCharacters;
        CapsLockState = capsLockState;
    }

    /// <summary>
    ///   Gets a value indicating whether conversion of lower-case characters to upper case has been detected.
    /// </summary>
    public bool UpperCaseConversionDetected
    {
        get
        {
            IsCapsLockProbablyOn();
            return _upperCaseConversionDetected;
        }
    }

    /// <summary>
    ///   Gets a value indicating whether conversion of upper-case characters to lower case has been detected.
    /// </summary>
    public bool LowerCaseConversionDetected
    {
        get
        {
            IsCapsLockProbablyOn();
            return _lowerCaseConversionDetected;
        }
    }

    /// <summary>
    ///   Gets the Unicode block name for the script represented by the OS-configured keyboard layout.
    /// </summary>
    private string KeyboardScript { get; }

    /// <summary>
    ///   Gets a value indicating whether Caps Lock is on or off, as reported by the client.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public bool? CapsLockState { get; }

    /// <summary>
    ///   Gets a value indicating whether Caps Lock appears to be on according to heuristic analysis.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public bool CapsLockIndicator => IsCapsLockProbablyOn();

    /// <summary>
    ///   Tests to see if CAPS LOCK is probably on. CAPS LOCK generally only affects the case of
    ///   characters generated for scripts derived from the Latin script. However, on some
    ///   keyboards, CAPS LOCK may be used to switch script - e.g., a Hebrew keyboard will use
    ///   CAPS LOCK to output Latin capitals.
    /// </summary>
    /// <returns>True, if CAPS LOCK is probably on; otherwise false.</returns>
    private bool IsCapsLockProbablyOn()
    {
        // Detect the probable use of CAPS LOCK. This is an heuristic check.
        double upperToLowerLetterCount;
        double lowerToUpperLetterCount;

        return KeyboardScript switch
               {
                   "Latin"                => AnalyseScript(new Range(65, 90), new Range(97, 122)),
                   "Cyrillic"             => AnalyseScript(new Range(1040, 1071), new Range(1072, 1103)),
                   "Greek"                => AnalyseScript(new Range(904, 939), new Range(940, 974)),
                   "Hebrew"               => AnalyseScript(new Range(65, 90), new Range(1488, 1514)),
                   "Armenian"             => AnalyseScript(new Range(1329, 1375), new Range(1369, 1418)),
                   "GeorgianOldAlphabets" => AnalyseScript(new Range(4256, 4301), new Range(11520, 11567)),
                   "Coptic"               => AnalyseCopticScript(),
                   "Adlam"                => AnalyseScript(new Range(125184, 125217), new Range(125218, 125251)),
                   "WarangCiti"           => AnalyseScript(new Range(71840, 71871), new Range(71872, 71903)),
                   "Cherokee"             => AnalyseCherokeeScript(),
                   "Osage"                => AnalyseScript(new Range(66736, 66771), new Range(66776, 66811)),
                   "Glagolitic"           => AnalyseScript(new Range(11264, 11310), new Range(11312, 11358)),
                   "Deseret"              => AnalyseScript(new Range(65560, 66599), new Range(66600, 66639)),
                   _                      => false
               };

        // Test script with simple upper/lower case ranges
        bool AnalyseScript(Range upper, Range lower)
        {
            Reset();
            AnalyseCharacters(lower.Start.Value, lower.End.Value, true);
            AnalyseCharacters(upper.Start.Value, upper.End.Value);
            return TestThreshold();
        }

        // Heuristically, we will assume that if 65% of the characters are of the opposite case, CAPS LOCK
        // is probably on.
        bool TestThreshold()
        {
            _upperCaseConversionDetected = lowerToUpperLetterCount / 26 > 0.65;
            _lowerCaseConversionDetected = upperToLowerLetterCount / 26 > 0.65;

            if (CapsLockState is null)
            {
                return _upperCaseConversionDetected && _lowerCaseConversionDetected;
            }

            return (bool)CapsLockState;
        }

        // Test Cherokee
        bool AnalyseCherokeeScript()
        {
            Reset();
            AnalyseCharacters(5112, 5118, true); 
            AnalyseCharacters(43888, 43967, true);
            AnalyseCharacters(5024, 5109);
            return TestThreshold();
        }

        // Test Coptic
        bool AnalyseCopticScript()
        {
            Reset();
            AnalyseCharactersAlt(11392, 11519, true, true);
            AnalyseCharactersAlt(11392, 11519);
            return TestThreshold();
        }

        void AnalyseCharactersAlt(int from, int to, bool upper = false, bool isEven = false)
        {
            var adjustment = 82 - _segmentCharacters.Count;
            var offset = upper ? 28 - adjustment : 55 - adjustment;

            foreach (var letter in Enumerable.Range(1, 26).Select(idx => _segmentCharacters[idx + offset]))
            {
                var increment = letter[0] >= from && letter[0] <= to &&
                                TestIfEven()
                    ? 1
                    : 0;

                upperToLowerLetterCount += upper ? increment : 0;
                lowerToUpperLetterCount += upper ? 0 : increment;
#pragma warning disable S3626
#pragma warning disable S1751
                continue;
#pragma warning restore S1751
#pragma warning restore S3626

                bool TestIfEven() =>
                    isEven
                        ? letter[0] % 2 == 0
                        : letter[0] % 2 == 1;
            }
        }

        void AnalyseCharacters(int from, int to, bool upper = false)
        {
            var adjustment = 82 - _segmentCharacters.Count;
            var offset = upper ? 28 - adjustment : 55 - adjustment;

            foreach (var letter in Enumerable.Range(1, 26)
                         .Select(idx => _segmentCharacters[idx + offset]))
            {
                var increment = letter[0] >= from && letter[0] <= to ? 1 : 0;
                upperToLowerLetterCount += upper ? increment : 0;
                lowerToUpperLetterCount += upper ? 0 : increment;
            }
        }

        /*  Candidates for further detection. These are keyboards
         *  that support CAPS LOCK, though not necessarily upper/lower case
         *
         *  ?Futhark
            Inuktitut - Latin
            Inuktitut - Naqittaut
            ?Mongolian
            Sinhala wij 9
            Thai Kedmanee
            Thai Pattachote
            Tibetan (PRC)
            Tibetan (PRC) - Updated
         */

        void Reset()
        {
            upperToLowerLetterCount = 0D;
            lowerToUpperLetterCount = 0D;
        }
    }
            }