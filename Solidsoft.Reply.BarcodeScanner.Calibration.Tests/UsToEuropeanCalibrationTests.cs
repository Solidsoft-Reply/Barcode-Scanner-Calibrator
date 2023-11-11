// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UsToEuropeanCalibrationTests.cs" company="Solidsoft Reply Ltd.">
//   (c) 2021 Solidsoft Reply Ltd.  All rights reserved.
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
// Unit tests for the Keyboard Calibrator
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration.Tests;

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Data;

using Xunit;

using Calibration;
using DataMatrix;


/// <summary>
/// Unit tests for calibration when the scanner is configured as a US keyboard
/// and the computer keyboard is a European keyboard.
/// </summary>
public class UsToEuropeanCalibrationTests
{
    /// <summary>
    /// Test a simple string
    /// </summary>
    [Fact]
    public void UnitedStatesToUnitedStates()
    {
        var calibrator = new Calibrator();
        var loopCount = 0;

        foreach (var token in calibrator.CalibrationTokens())
        {
            calibrator.Calibrate(ConvertToCharacterValues(BaselineCalibrationUsUs()), token);
            loopCount++;
        }

        Assert.Equal(1, loopCount);
    }

    /// <summary>
    /// Test calibration for a Belgian (Comma) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToBelgianComma()
    {
        PerformCalibrationTest("Belgian (Comma)");
    }

    /// <summary>
    /// Test calibration for a Belgian French computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToBelgianFrench()
    {
        PerformCalibrationTest("Belgian French");
    }

    /// <summary>
    /// Test calibration for a Belgian (Period) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToBelgianPeriod()
    {
        PerformCalibrationTest("Belgian (Period)");
    }

    /// <summary>
    /// Test calibration for a Bulgarian computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToBulgarian()
    {
        PerformCalibrationTest("Bulgarian");
    }

    /// <summary>
    /// Test calibration for a Bulgarian (Latin) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToBulgarianLatin()
    {
        PerformCalibrationTest("Bulgarian (Latin)");
    }

    /// <summary>
    /// Test calibration for a Bulgarian (Phonetic) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToBulgarianPhonetic()
    {
        PerformCalibrationTest("Bulgarian (Phonetic)");
    }

    /// <summary>
    /// Test calibration for a Bulgarian (Phonetic Traditional) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToBulgarianPhoneticTraditional()
    {
        PerformCalibrationTest("Bulgarian (Phonetic Traditional)");
    }

    /// <summary>
    /// Test calibration for a Bulgarian (Typewriter) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToBulgarianTypewriter()
    {
        PerformCalibrationTest("Bulgarian (Typewriter)");
    }

    /// <summary>
    /// Test calibration for a Czech computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToCzech()
    {
        PerformCalibrationTest("Czech");
    }

    /// <summary>
    /// Test calibration for a Czech Programmers computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToCzechProgrammers()
    {
        PerformCalibrationTest("Czech Programmers");
    }

    /// <summary>
    /// Test calibration for a Czech (QWERTY) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToCzechQwerty()
    {
        PerformCalibrationTest("Czech (QWERTY)");
    }

    /// <summary>
    /// Test calibration for a Danish computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToDanish()
    {
        PerformCalibrationTest("Danish");
    }

    /// <summary>
    /// Test calibration for a Dutch computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToDutch()
    {
        PerformCalibrationTest("Dutch");
    }

    /// <summary>
    /// Test calibration for a Estonian computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToEstonian()
    {
        PerformCalibrationTest("Estonian");
    }

    /// <summary>
    /// Test calibration for a Finnish computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToFinnish()
    {
        PerformCalibrationTest("Finnish");
    }

    /// <summary>
    /// Test calibration for a Finnish with Sami computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToFinnishWithSami()
    {
        PerformCalibrationTest("Finnish with Sami");
    }

    /// <summary>
    /// Test calibration for a French computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToFrench()
    {
        PerformCalibrationTest("French");
    }

    /// <summary>
    /// Test calibration for a Scottish Gaelic computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToGaelic()
    {
        PerformCalibrationTest("Gaelic");
    }

    /// <summary>
    /// Test calibration for a German computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToGerman()
    {
        PerformCalibrationTest("German");
    }

    /// <summary>
    /// Test calibration for a German (IBM) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToGermanIbm()
    {
        PerformCalibrationTest("German (IBM)");
    }

    /// <summary>
    /// Test calibration for a Greek (220) computer keyboard layout
    /// </summary>
    [Fact]
    public void ToGreek220()
    {
        // Calibration fails
        var token = PerformCalibrationTest("Greek (220)");
        Assert.Null(token.CalibrationData);
    }

    /// <summary>
    /// Test calibration for a Greek (220) Latin computer keyboard layout
    /// </summary>
    [Fact]
    public void ToGreek220Latin()
    {
        // Calibration fails
        var token = PerformCalibrationTest("Greek (220) Latin");
        Assert.Null(token.CalibrationData);
    }

    /// <summary>
    /// Test calibration for a Greek (319) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToGreek319()
    {
        PerformCalibrationTest("Greek (319)");
    }

    /// <summary>
    /// Test calibration for a Greek (319) Latin computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToGreek319Latin()
    {
        PerformCalibrationTest("Greek (319) Latin");
    }

    /// <summary>
    /// Test calibration for a Greek computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToGreek()
    {
        PerformCalibrationTest("Greek");
    }

    /// <summary>
    /// Test calibration for a Greek computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToGreekLatin()
    {
        PerformCalibrationTest("Greek Latin");
    }

    /// <summary>
    /// Test calibration for a Greek Polytonic computer keyboard layout
    /// </summary>
    [Fact]
    public void ToGreekPolytonic()
    {
        // Calibration fails
        var token = PerformCalibrationTest("Greek Polytonic");
        Assert.Null(token.CalibrationData);
    }

    /// <summary>
    /// Test calibration for a Bulgarian (Phonetic) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToHungarian()
    {
        PerformCalibrationTest("Hungarian");
    }

    /// <summary>
    /// Test calibration for a Hungarian 101-key computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToHungarian101Key()
    {
        PerformCalibrationTest("Hungarian 101-key");
    }

    /// <summary>
    /// Test calibration for a Icelandic computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToIcelandic()
    {
        PerformCalibrationTest("Icelandic");
    }

    /// <summary>
    /// Test calibration for a Irish computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToIrish()
    {
        PerformCalibrationTest("Irish");
    }

    /// <summary>
    /// Test calibration for a Italian computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToItalian()
    {
        PerformCalibrationTest("Italian");
    }

    /// <summary>
    /// Test calibration for a Italian (142) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToItalian142()
    {
        PerformCalibrationTest("Italian (142)");
    }

    /// <summary>
    /// Test calibration for a Latvian computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToLatvian()
    {
        PerformCalibrationTest("Latvian");
    }

    /// <summary>
    /// Test calibration for a Latvian (QWERTY) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToLatvianQwerty()
    {
        PerformCalibrationTest("Latvian (QWERTY)");
    }

    /// <summary>
    /// Test calibration for a Latvian (Standard) computer keyboard layout
    /// </summary>
    [Fact]
    public void ToLatvianStandard()
    {
        // Calibration fails
        var token = PerformCalibrationTest("Latvian (Standard)");
        Assert.Null(token.CalibrationData);
    }

    /// <summary>
    /// Test calibration for a Lithuanian computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToLithuanian()
    {
        PerformCalibrationTest("Lithuanian");
    }

    /// <summary>
    /// Test calibration for a Lithuanian (IBM) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToLithuanianIbm()
    {
        PerformCalibrationTest("Lithuanian (IBM)");
    }

    /// <summary>
    /// Test calibration for a Lithuanian (Standard) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToLithuanianStandard()
    {
        PerformCalibrationTest("Lithuanian (Standard)");
    }

    /// <summary>
    /// Test calibration for a Luxembourgish computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToLuxembourgish()
    {
        PerformCalibrationTest("Luxembourgish");
    }

    /// <summary>
    /// Test calibration for a Maltese 47-key computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToMaltese47Key()
    {
        PerformCalibrationTest("Maltese 47-key");
    }

    /// <summary>
    /// Test calibration for a Maltese 48-key computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToMaltese48Key()
    {
        PerformCalibrationTest("Maltese 48-key");
    }

    /// <summary>
    /// Test calibration for a Norwegian computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToNorwegian()
    {
        PerformCalibrationTest("Norwegian");
    }

    /// <summary>
    /// Test calibration for a Norwegian with Sami computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToNorwegianWithSami()
    {
        PerformCalibrationTest("Norwegian with Sami");
    }

    /// <summary>
    /// Test calibration for a Polish (214) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToPolish214()
    {
        PerformCalibrationTest("Polish (214)");
    }

    /// <summary>
    /// Test calibration for a Polish (Programmers) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToPolishProgrammers()
    {
        PerformCalibrationTest("Polish (Programmers)");
    }

    /// <summary>
    /// Test calibration for a Portuguese computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToPortuguese()
    {
        PerformCalibrationTest("Portuguese");
    }

    /// <summary>
    /// Test calibration for a Romanian (Legacy) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToRomanianLegacy()
    {
        PerformCalibrationTest("Romanian (Legacy)");
    }

    /// <summary>
    /// Test calibration for a Romanian (Programmers) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToRomanianProgrammers()
    {
        PerformCalibrationTest("Romanian (Programmers)");
    }

    /// <summary>
    /// Test calibration for a Romanian (Standard) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToRomanianStandard()
    {
        PerformCalibrationTest("Romanian (Standard)");
    }

    /// <summary>
    /// Test calibration for a Sami Extended Finland Sweden computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void SamiExtendedFinlandSwedenCalibration()
    {
        PerformCalibrationTest("Sami Extended Finland-Sweden");
    }

    /// <summary>
    /// Test calibration for a Sami Extended Norway computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void SamiExtendedNorwayCalibration()
    {
        PerformCalibrationTest("Sami Extended Norway");
    }

    /// <summary>
    /// Test calibration for a Slovak computer keyboard layout
    /// </summary>
    [Fact]
    public void ToSlovak()
    {
        // Calibration fails
        var token = PerformCalibrationTest("Slovak");
        Assert.Null(token.CalibrationData);
    }

    /// <summary>
    /// Test calibration for a Slovak (QWERTY) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToSlovakQwerty()
    {
        PerformCalibrationTest("Slovak (QWERTY)");
    }

    /// <summary>
    /// Test calibration for a Slovenian computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToSlovenian()
    {
        PerformCalibrationTest("Slovenian");
    }

    /// <summary>
    /// Test calibration for a Sorbian Extended computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToSorbianExtended()
    {
        PerformCalibrationTest("Sorbian Extended");
    }

    /// <summary>
    /// Test calibration for a Sorbian Standard computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToSorbianStandard()
    {
        PerformCalibrationTest("Sorbian Standard");
    }

    /// <summary>
    /// Test calibration for a Sorbian Standard (Legacy) computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToSorbianStandardLegacy()
    {
        PerformCalibrationTest("Sorbian Standard (Legacy)");
    }

    /// <summary>
    /// Test calibration for a Spanish computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToSpanish()
    {
        PerformCalibrationTest("Spanish");
    }

    /// <summary>
    /// Test calibration for a Spanish Variation computer keyboard layout
    /// </summary>
    [Fact]
    public void ToSpanishVariation()
    {
        // Calibration fails
        var token = PerformCalibrationTest("Spanish Variation");
        Assert.Null(token.CalibrationData);
    }

    /// <summary>
    /// Test calibration for a Croatian Standard computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToStandard()
    {
        PerformCalibrationTest("Standard");
    }

    /// <summary>
    /// Test calibration for a Swedish computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToSwedish()
    {
        PerformCalibrationTest("Swedish");
    }

    /// <summary>
    /// Test calibration for a Swedish with Sami computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToSwedishWithSami()
    {
        PerformCalibrationTest("Swedish with Sami");
    }

    /// <summary>
    /// Test calibration for a Swiss French computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToSwissFrench()
    {
        PerformCalibrationTest("Swiss French");
    }

    /// <summary>
    /// Test calibration for a Swiss German computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToSwissGerman()
    {
        PerformCalibrationTest("Swiss German");
    }

    /// <summary>
    /// Test calibration for a United Kingdom computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToUnitedKingdom()
    {
        PerformCalibrationTest("United Kingdom");
    }

    /// <summary>
    /// Test calibration for a United Kingdom Extended computer keyboard layout
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ToUnitedKingdomExtended()
    {
        PerformCalibrationTest("United Kingdom Extended");
    }

    /// <summary>
    /// Performs a calibration test.
    /// </summary>
    /// <param name="layoutName">The name of the computer keyboard layout</param>
    /// <param name="multiplier">The multiplier for the size of the data matrix image.</param>
    /// <param name="size">The size of the data matrix.</param>
    /// <returns>A calibration token.</returns>
    private static CalibrationToken PerformCalibrationTest(string layoutName, float multiplier = 1F, DataMatrixSize size = DataMatrixSize.Automatic)
    {
        var expectedCalibrations = ExpectedCalibrations();
        var computerKeyboardLayout = CalibrationBarcodes()[layoutName];

        var calibrator = new Calibrator(calibrationAssumption: CalibrationAssumption.Agnostic);
        var loopCountForBaseline = 0;
        var loopCount = -1;
        CalibrationToken currentToken = default;

        var recognisedDataElements = new List<RecognisedDataElement>
        {
            new(Syntax.Gs1ApplicationIdentifiers, "01"),
            new(Syntax.Gs1ApplicationIdentifiers, "10"),
            new(Syntax.Gs1ApplicationIdentifiers, "17"),
            new(Syntax.Gs1ApplicationIdentifiers, "21"),
            new(Syntax.Gs1ApplicationIdentifiers, "710"),
            new(Syntax.Gs1ApplicationIdentifiers, "711"),
            new(Syntax.Gs1ApplicationIdentifiers, "712"),
            new(Syntax.Gs1ApplicationIdentifiers, "714"),
            new(Syntax.AscMhDataIdentifiers, "9N"),
            new(Syntax.AscMhDataIdentifiers, "1T"),
            new(Syntax.AscMhDataIdentifiers, "D"),
            new(Syntax.AscMhDataIdentifiers, "S")
        };

        calibrator.RecognisedDataElements = recognisedDataElements;

        foreach (var token in calibrator.CalibrationTokens(multiplier, size))
        {
            var baseLine = computerKeyboardLayout.Keys.First();
            currentToken = token;

            if (loopCount < 0)
            {
                currentToken = calibrator.Calibrate(ConvertToCharacterValues(baseLine[loopCountForBaseline++]), currentToken);
                loopCount = loopCountForBaseline == baseLine.Length ? ++loopCount : loopCount;
            }
            else
            {
                if (loopCount < computerKeyboardLayout[baseLine].Count)
                {
                    currentToken = calibrator.Calibrate(
                        ConvertToCharacterValues(computerKeyboardLayout[baseLine][loopCount++]),
                        currentToken);
                }
            }

        }

        foreach (var error in currentToken.Errors)
        {
            System.Diagnostics.Trace.WriteLine("Error: " + error.Description);
        }

        System.Diagnostics.Trace.WriteLine(
            $"private const string {layoutName.Replace(" ", "").Replace("(", "").Replace(")", "")}Calibration = " +
            ToLiteral($"\"{calibrator.CalibrationData}\";"));
        System.Diagnostics.Trace.WriteLine("");

        // Assert that the calibrator calculated the expected calibration.
        Assert.Equal(expectedCalibrations[layoutName], currentToken.CalibrationData?.ToJson() ?? string.Empty);

        ////Assert.Equal(computerKeyboardLayout.Value[baseLine], loopCount);

        return currentToken;

        static string ToLiteral(string input)
        {
            using var writer = new StringWriter();
            using var provider = CodeDomProvider.CreateProvider("CSharp");
            provider.GenerateCodeFromExpression(new System.CodeDom.CodePrimitiveExpression(input), writer, null!);
            return writer.ToString();
        }
    }

    /// <summary>
    /// Returns test data for testing calibration for a scanner configured as a United States keyboard and
    /// computer keyboard layouts for each European keyboard defined in Windows.
    /// </summary>
    /// <returns>A dictionary of test data.</returns>
    private static Dictionary<string, Dictionary<string[], IList<string>>> CalibrationBarcodes()
    {
        var unitedStatesTestData = new Dictionary<string, Dictionary<string[], IList<string>>>
                                   {
                                       {
                                           "Belgian (Comma)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.BelgianCommaBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.BelgianCommaDeadkeyBarcode1, UsToEuropeanGs1BarcodesWithAim.BelgianCommaDeadkeyBarcode2
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Belgian French",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.BelgianFrenchBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.BelgianFrenchDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.BelgianFrenchDeadkeyBarcode2
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Belgian (Period)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.BelgianPeriodBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.BelgianPeriodDeadkeyBarcode1, UsToEuropeanGs1BarcodesWithAim.BelgianPeriodDeadkeyBarcode2
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Bulgarian",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.BulgarianBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Bulgarian (Latin)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.BulgarianLatinBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Bulgarian (Phonetic)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.BulgarianPhoneticBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Bulgarian (Phonetic Traditional)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.BulgarianPhoneticTraditionalBaseline },
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "Bulgarian (Typewriter)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.BulgarianTypewriterBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Czech",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.CzechBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.CzechDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.CzechDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.CzechDeadkeyBarcode3,
                                                       UsToEuropeanGs1BarcodesWithAim.CzechDeadkeyBarcode4
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Czech Programmers",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.CzechProgrammersBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Czech (QWERTY)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.CzechQwertyBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.CzechQwertyDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.CzechQwertyDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.CzechQwertyDeadkeyBarcode3,
                                                       UsToEuropeanGs1BarcodesWithAim.CzechQwertyDeadkeyBarcode4
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Danish",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.DanishBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.DanishDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.DanishDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.DanishDeadkeyBarcode3,
                                                       UsToEuropeanGs1BarcodesWithAim.DanishDeadkeyBarcode4
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Dutch",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.DutchBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.DutchDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.DutchDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.DutchDeadkeyBarcode3,
                                                       UsToEuropeanGs1BarcodesWithAim.DutchDeadkeyBarcode4,
                                                       UsToEuropeanGs1BarcodesWithAim.DutchDeadkeyBarcode5
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Estonian",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.EstonianBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.EstonianDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.EstonianDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.EstonianDeadkeyBarcode3,
                                                       UsToEuropeanGs1BarcodesWithAim.EstonianDeadkeyBarcode4
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Finnish",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.FinnishBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.FinnishDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.FinnishDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.FinnishDeadkeyBarcode3,
                                                       UsToEuropeanGs1BarcodesWithAim.FinnishDeadkeyBarcode4
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Finnish with Sami",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.FinnishWithSamiBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.FinnishWithSamiDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.FinnishWithSamiDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.FinnishWithSamiDeadkeyBarcode3,
                                                       UsToEuropeanGs1BarcodesWithAim.FinnishWithSamiDeadkeyBarcode4
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "French",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.FrenchBaseline },
                                                   new List<string> { UsToEuropeanGs1BarcodesWithAim.FrenchDeadkeyBarcode1, UsToEuropeanGs1BarcodesWithAim.FrenchDeadkeyBarcode2 }
                                               }
                                           }
                                       },
                                       {
                                           "Gaelic",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.GaelicBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.GaelicDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.GaelicDeadkeyBarcode2
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "German",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.GermanBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.GermanDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.GermanDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.GermanDeadkeyBarcode3
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "German (IBM)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.GermanIbmBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.GermanIbmDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.GermanIbmDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.GermanIbmDeadkeyBarcode3
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Greek (220)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.Greek220Baseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Greek (220) Latin",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.Greek220LatinBaseline }, new List<string>()}
                                           }
                                       },
                                       {
                                           "Greek (319)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.Greek319Baseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.Greek319DeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.Greek319DeadkeyBarcode2
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Greek (319) Latin",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.Greek319LatinBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.Greek319LatinDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.Greek319LatinDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.Greek319LatinDeadkeyBarcode3,
                                                       UsToEuropeanGs1BarcodesWithAim.Greek319LatinDeadkeyBarcode4,
                                                       UsToEuropeanGs1BarcodesWithAim.Greek319LatinDeadkeyBarcode5
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Greek",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.GreekBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.GreekDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.GreekDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.GreekDeadkeyBarcode3
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Greek Latin",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.GreekLatinBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.GreekLatinDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.GreekLatinDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.GreekLatinDeadkeyBarcode3
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Greek Polytonic",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.GreekPolytonicBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.GreekPolytonicDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.GreekPolytonicDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.GreekPolytonicDeadkeyBarcode3,
                                                       UsToEuropeanGs1BarcodesWithAim.GreekPolytonicDeadkeyBarcode4,
                                                       UsToEuropeanGs1BarcodesWithAim.GreekPolytonicDeadkeyBarcode5,
                                                       UsToEuropeanGs1BarcodesWithAim.GreekPolytonicDeadkeyBarcode6,
                                                       UsToEuropeanGs1BarcodesWithAim.GreekPolytonicDeadkeyBarcode7
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Hungarian 101-key",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.Hungarian101KeyBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Hungarian",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.HungarianBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Icelandic",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.IcelandicBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.IcelandicDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.IcelandicDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.IcelandicDeadkeyBarcode3
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Irish",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.IrishBaseline },
                                                   new List<string> { UsToEuropeanGs1BarcodesWithAim.IrishDeadkeyBarcode1 }
                                               }
                                           }
                                       },
                                       {
                                           "Italian (142)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.Italian142Baseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Italian",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.ItalianBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Latvian",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.LatvianBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.LatvianDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.LatvianDeadkeyBarcode2
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Latvian (QWERTY)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.LatvianQwertyBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.LatvianQwertyDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.LatvianQwertyDeadkeyBarcode2
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Latvian (Standard)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.LatvianStandardBaseline },
                                                   new List<string> { UsToEuropeanGs1BarcodesWithAim.LatvianStandardDeadkeyBarcode1 }
                                               }
                                           }
                                       },
                                       {
                                           "Lithuanian",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.LithuanianBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Lithuanian (IBM)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.LithuanianIbmBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Lithuanian (Standard)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.LithuanianStandardBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Luxembourgish",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.LuxembourgishBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.LuxembourgishDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.LuxembourgishDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.LuxembourgishDeadkeyBarcode3
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Maltese 47-key",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.Maltese47KeyBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Maltese 48-key",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.Maltese48KeyBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Norwegian",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.NorwegianBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.NorwegianDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.NorwegianDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.NorwegianDeadkeyBarcode3
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Norwegian with Sami",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.NorwegianWithSamiBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.NorwegianWithSamiDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.NorwegianWithSamiDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.NorwegianWithSamiDeadkeyBarcode3
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Polish (214)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.Polish214Baseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.Polish214DeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.Polish214DeadkeyBarcode2
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Polish (Programmers)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.PolishProgrammersBaseline },
                                                   new List<string> { UsToEuropeanGs1BarcodesWithAim.PolishProgrammersDeadkeyBarcode1 }
                                               }
                                           }
                                       },
                                       {
                                           "Portuguese",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.PortugueseBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.PortugueseDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.PortugueseDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.PortugueseDeadkeyBarcode3,
                                                       UsToEuropeanGs1BarcodesWithAim.PortugueseDeadkeyBarcode4
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Romanian (Legacy)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.RomanianLegacyBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Romanian (Programmers)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.RomanianProgrammersBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Romanian (Standard)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.RomanianStandardBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Sami Extended Finland-Sweden",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.SamiExtendedFinlandSwedenBaseline }, new List<string>
                                                       {
                                                           UsToEuropeanGs1BarcodesWithAim.SamiExtendedFinlandSwedenDeadkeyBarcode1,
                                                           UsToEuropeanGs1BarcodesWithAim.SamiExtendedFinlandSwedenDeadkeyBarcode2
                                                       }
                                               }
                                           }
                                       },
                                       {
                                           "Sami Extended Norway",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.SamiExtendedNorwayBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.SamiExtendedNorwayDeadkeyBarcode1
                                                   }

                                               }
                                           }
                                       },
                                       {
                                           "Slovak",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.SlovakBaseline },
                                                   new List<string> { UsToEuropeanGs1BarcodesWithAim.SlovakDeadkeyBarcode1 }
                                               }
                                           }
                                       },
                                       {
                                           "Slovak (QWERTY)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.SlovakQwertyBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.SlovakQwertyDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.SlovakQwertyDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.SlovakQwertyDeadkeyBarcode3
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Slovenian",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.SlovenianBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.SlovenianDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.SlovenianDeadkeyBarcode2
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Sorbian Extended",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.SorbianExtendedBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.SorbianExtendedDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.SorbianExtendedDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.SorbianExtendedDeadkeyBarcode3,
                                                       UsToEuropeanGs1BarcodesWithAim.SorbianExtendedDeadkeyBarcode4
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Sorbian Standard",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.SorbianStandardBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.SorbianStandardDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.SorbianStandardDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.SorbianStandardDeadkeyBarcode3
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Sorbian Standard (Legacy)",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.SorbianStandardLegacyBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.SorbianStandardLegacyDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.SorbianStandardLegacyDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.SorbianStandardLegacyDeadkeyBarcode3,
                                                       UsToEuropeanGs1BarcodesWithAim.SorbianStandardLegacyDeadkeyBarcode4
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Spanish",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.SpanishBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.SpanishDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.SpanishDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.SpanishDeadkeyBarcode3,
                                                       UsToEuropeanGs1BarcodesWithAim.SpanishDeadkeyBarcode4
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Spanish Variation",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.SpanishVariationBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "Standard",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.StandardBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.StandardDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.StandardDeadkeyBarcode2
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Swedish",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.SwedishBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.SwedishDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.SwedishDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.SwedishDeadkeyBarcode3,
                                                       UsToEuropeanGs1BarcodesWithAim.SwedishDeadkeyBarcode4
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Swedish with Sami",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.SwedishWithSamiBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.SwedishWithSamiDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.SwedishWithSamiDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.SwedishWithSamiDeadkeyBarcode3,
                                                       UsToEuropeanGs1BarcodesWithAim.SwedishWithSamiDeadkeyBarcode4
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Swiss French",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.SwissFrenchBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.SwissFrenchDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.SwissFrenchDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.SwissFrenchDeadkeyBarcode3
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Swiss German",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.SwissGermanBaseline },
                                                   new List<string>
                                                   {
                                                       UsToEuropeanGs1BarcodesWithAim.SwissGermanDeadkeyBarcode1,
                                                       UsToEuropeanGs1BarcodesWithAim.SwissGermanDeadkeyBarcode2,
                                                       UsToEuropeanGs1BarcodesWithAim.SwissGermanDeadkeyBarcode3
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "United Kingdom",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               { new[] { UsToEuropeanGs1BarcodesWithAim.UnitedKingdomBaseline }, new List<string>() }
                                           }
                                       },
                                       {
                                           "United Kingdom Extended",
                                           new Dictionary<string[], IList<string>>
                                           {
                                               {
                                                   new[] { UsToEuropeanGs1BarcodesWithAim.UnitedKingdomExtendedBaseline },
                                                   new List<string> { UsToEuropeanGs1BarcodesWithAim.UnitedKingdomExtendedDeadkeyBarcode1 }
                                               }
                                           }
                                       }
                                   };

        return unitedStatesTestData;
    }

    /// <summary>
    /// Returns expected calibrations for a scanner configured as a United States keyboard and computer
    /// keyboard layouts for each European keyboard defined in Windows.
    /// </summary>
    /// <returns>A dictionary of expected calibrations.</returns>
    private static Dictionary<string, string> ExpectedCalibrations()
    {
        var testCalibrations = new Dictionary<string, string>
                               {
                                   { "Belgian (Comma)", UsToEuropeanCalibrations.BelgianCommaCalibration },
                                   { "Belgian French", UsToEuropeanCalibrations.BelgianFrenchCalibration },
                                   { "Belgian (Period)", UsToEuropeanCalibrations.BelgianPeriodCalibration },
                                   { "Bulgarian", UsToEuropeanCalibrations.BulgarianCalibration },
                                   { "Bulgarian (Latin)", UsToEuropeanCalibrations.BulgarianLatinCalibration },
                                   { "Bulgarian (Phonetic)", UsToEuropeanCalibrations.BulgarianPhoneticCalibration },
                                   { "Bulgarian (Phonetic Traditional)", UsToEuropeanCalibrations.BulgarianPhoneticTraditionalCalibration },
                                   { "Bulgarian (Typewriter)", UsToEuropeanCalibrations.BulgarianTypewriterCalibration },
                                   { "Czech", UsToEuropeanCalibrations.CzechCalibration },
                                   { "Czech Programmers", UsToEuropeanCalibrations.CzechProgrammersCalibration },
                                   { "Czech (QWERTY)", UsToEuropeanCalibrations.CzechQwertyCalibration },
                                   { "Danish", UsToEuropeanCalibrations.DanishCalibration },
                                   { "Dutch", UsToEuropeanCalibrations.DutchCalibration },
                                   { "Estonian", UsToEuropeanCalibrations.EstonianCalibration },
                                   { "Finnish", UsToEuropeanCalibrations.FinnishCalibration },
                                   { "Finnish with Sami", UsToEuropeanCalibrations.FinnishWithSamiCalibration },
                                   { "French", UsToEuropeanCalibrations.FrenchCalibration },
                                   { "Gaelic", UsToEuropeanCalibrations.GaelicCalibration },
                                   { "German", UsToEuropeanCalibrations.GermanCalibration },
                                   { "German (IBM)", UsToEuropeanCalibrations.GermanIbmCalibration },
                                   { "Greek (220)", UsToEuropeanCalibrations.Greek220Calibration },
                                   { "Greek (220) Latin", UsToEuropeanCalibrations.Greek220LatinCalibration },
                                   { "Greek (319)", UsToEuropeanCalibrations.Greek319Calibration },
                                   { "Greek (319) Latin", UsToEuropeanCalibrations.Greek319LatinCalibration },
                                   { "Greek", UsToEuropeanCalibrations.GreekCalibration },
                                   { "Greek Latin", UsToEuropeanCalibrations.GreekLatinCalibration },
                                   { "Greek Polytonic", UsToEuropeanCalibrations.GreekPolytonicCalibration },
                                   { "Hungarian", UsToEuropeanCalibrations.HungarianCalibration },
                                   { "Hungarian 101-key", UsToEuropeanCalibrations.Hungarian101KeyCalibration },
                                   { "Icelandic", UsToEuropeanCalibrations.IcelandicCalibration },
                                   { "Irish", UsToEuropeanCalibrations.IrishCalibration },
                                   { "Italian (142)", UsToEuropeanCalibrations.Italian142Calibration },
                                   { "Italian", UsToEuropeanCalibrations.ItalianCalibration },
                                   { "Latvian", UsToEuropeanCalibrations.LatvianCalibration },
                                   { "Latvian (QWERTY)", UsToEuropeanCalibrations.LatvianQwertyCalibration },
                                   { "Latvian (Standard)", UsToEuropeanCalibrations.LatvianStandardCalibration },
                                   { "Lithuanian", UsToEuropeanCalibrations.LithuanianCalibration },
                                   { "Lithuanian (IBM)", UsToEuropeanCalibrations.LithuanianIbmCalibration },
                                   { "Lithuanian (Standard)", UsToEuropeanCalibrations.LithuanianStandardCalibration },
                                   { "Luxembourgish", UsToEuropeanCalibrations.LuxembourgishCalibration },
                                   { "Maltese 47-key", UsToEuropeanCalibrations.Maltese47KeyCalibration },
                                   { "Maltese 48-key", UsToEuropeanCalibrations.Maltese48KeyCalibration },
                                   { "Norwegian", UsToEuropeanCalibrations.NorwegianCalibration },
                                   { "Norwegian with Sami", UsToEuropeanCalibrations.NorwegianWithSamiCalibration },
                                   { "Polish (214)", UsToEuropeanCalibrations.Polish214Calibration },
                                   { "Polish (Programmers)", UsToEuropeanCalibrations.PolishProgrammersCalibration },
                                   { "Portuguese", UsToEuropeanCalibrations.PortugueseCalibration },
                                   { "Romanian (Legacy)", UsToEuropeanCalibrations.RomanianLegacyCalibration },
                                   { "Romanian (Programmers)", UsToEuropeanCalibrations.RomanianProgrammersCalibration },
                                   { "Romanian (Standard)", UsToEuropeanCalibrations.RomanianStandardCalibration },
                                   { "Sami Extended Finland-Sweden", UsToEuropeanCalibrations.SamiExtendedFinlandSwedenCalibration },
                                   { "Sami Extended Norway", UsToEuropeanCalibrations.SamiExtendedNorwayCalibration },
                                   { "Slovak", UsToEuropeanCalibrations.SlovakCalibration },
                                   { "Slovak (QWERTY)", UsToEuropeanCalibrations.SlovakQwertyCalibration },
                                   { "Slovenian", UsToEuropeanCalibrations.SlovenianCalibration },
                                   { "Sorbian Extended", UsToEuropeanCalibrations.SorbianExtendedCalibration },
                                   { "Sorbian Standard", UsToEuropeanCalibrations.SorbianStandardCalibration },
                                   { "Sorbian Standard (Legacy)", UsToEuropeanCalibrations.SorbianStandardLegacyCalibration },
                                   { "Spanish", UsToEuropeanCalibrations.SpanishCalibration },
                                   { "Spanish Variation", UsToEuropeanCalibrations.SpanishVariationCalibration },
                                   { "Standard", UsToEuropeanCalibrations.StandardCalibration },
                                   { "Swedish", UsToEuropeanCalibrations.SwedishCalibration },
                                   { "Swedish with Sami", UsToEuropeanCalibrations.SwedishWithSamiCalibration },
                                   { "Swiss French", UsToEuropeanCalibrations.SwissFrenchCalibration },
                                   { "Swiss German", UsToEuropeanCalibrations.SwissGermanCalibration },
                                   { "United Kingdom", UsToEuropeanCalibrations.UnitedKingdomCalibration },
                                   { "United Kingdom Extended", UsToEuropeanCalibrations.UnitedKingdomExtendedCalibration }
                               };
        return testCalibrations;
    }

    /// <summary>
    /// Return the character input for the baseline calibration barcode using a scanner
    /// configured for a US keyboard and computer with US keyboard layout. 
    /// </summary>
    /// <returns>
    /// The <see cref="string"/> for scanning the baseline calibration barcode.
    /// The scanner is configured for a US keyboard.
    /// The computer is configured for a US keyboard.
    /// </returns>
    private static string BaselineCalibrationUsUs()
    {
        var testString =
            "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    "
          + (char)29;
        return testString + "    \x001C    \x001E    \x001F    ";
    }

    /// <summary>
    /// Convert an input string to a list of character values.
    /// </summary>
    /// <param name="input">The string of characters to be converted.</param>
    /// <returns>A comma-separated value list of character values.</returns>
    private static int[] ConvertToCharacterValues(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Array.Empty<int>();
        }

        var outputBuilder = new int[input.Length];

        for (var idx = 0; idx < input.Length; idx++)
        {
            outputBuilder[idx] = input[idx];
        }

        return outputBuilder;
    }
}