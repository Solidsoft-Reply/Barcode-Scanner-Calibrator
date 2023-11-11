// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalibrationExtensions.cs" company="Solidsoft Reply Ltd.">
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
// Extension methods for calibration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System;
using System.Linq;
using System.Diagnostics.Contracts;
using System.Text;
using System.Collections.Generic;

/// <summary>
///   Extension methods for calibration.
/// </summary>

// ReSharper disable once UnusedMember.Global
internal static class CalibrationExtensions
{
    /// <summary>
    ///   Strips off any trailing carriage return and line feed characters.
    /// </summary>
    /// <param name="input">The input data from which trailing CR and LF characters will be stripped</param>
    /// <param name="trailingCrLfChars">The trailing CR and LF characters, if any; Otherwise, and empty string.</param>
    /// <param name="baseline">Indicates if this is the baseline barcode or the last small barcode in the baseline sequence.</param>
    /// <returns>The input data without any trailing CR or LF characters.</returns>
    public static string StripTrailingCrLfs(this string input, out string trailingCrLfChars, bool baseline = false)
    {
        trailingCrLfChars = string.Empty;

        // Remove any trailing CR or LF. This will almost certainly have been added by the scanner, which will be assumed.
        while (true) {
            if (!input.EndsWith("\r", StringComparison.Ordinal) && !input.EndsWith("\n", StringComparison.Ordinal)) {
                if (input.Length == 0) return input;

                if (!input[..^1].EndsWith("\r", StringComparison.Ordinal)) {
                    // If this is the baseline and the last reported character is not an LF character, but is a control 
                    // character greater than ASCII 0, and the fifth- to second-to-last characters are spaces, then we 
                    // will assume it represents an LF. This is not reliable because the scanner may be outputting some 
                    // other control character as the last character in the sequence, but it is very unlikely that a space 
                    // will be represented by a control character, and if it is, everything else will be broken anyway. We 
                    // are also assuming that barcode scanners use the ENTER key to enter CRs. If these two assumptions 
                    // are correct, then a sequence of four spaces followed by a non-0 control character is almost 
                    // certainly a representation of an LF terminator, and even if not, we can assume that it is safe to 
                    // interpret the last character as if it was an LF. This approach will not work if there are other 
                    // suffix characters before the LF. In this case, the calibrator may mistakenly report the suffix with 
                    // the final character and conclude there is no end-of-line sequence.
                    if (baseline && input.Length > 4 && input[^5..^1] == "    " && (int)input.Last() is > 0 and < 32) {
                        trailingCrLfChars = "LF";
                    }

                    return input;
                }

                // If the second to last reported character is a CR, then we will assume the last character is an LF. Again
                // this is not reliable, but it is a reasonable assumption. At worst, the CR will act as a line terminator
                // anyway, so it won't generally be a problem if the last character is incorrectly interpreted as an LF.
                trailingCrLfChars = "LF";
                input = input[..^1];
                continue;
            }

            trailingCrLfChars = (input.Last() == '\r' ? "CR" : "LF") + trailingCrLfChars;
            if (input.Length >= 1) input = input[..^1];
        }
    }

    /// <summary>
    ///   Strips off any trailing carriage return and line feed characters.
    /// </summary>
    /// <param name="input">The input data from which trailing CR and LF characters will be stripped</param>
    /// <param name="baseline">Indicates if this is the baseline barcode or the last small barcode in the baseline sequence.</param>
    /// <returns>The input data without any trailing CR or LF characters.</returns>
    public static string StripTrailingCrLfs(this string input, bool baseline = false) =>
        StripTrailingCrLfs(input, out _, baseline);

    /// <summary>
    ///   Finds an extended ASCII character that is not being used in the input string.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="unusedChar">The first extended ASCII char that does not appear in the input.</param>
    /// <returns>True, if an unused extended ASCII character was found; otherwise false.</returns>
    public static bool TryUnusedExtendedAsciiCharacter(this string input, out char unusedChar)
    {
        unusedChar = default;

        // Find a character that is not being used in the sequence. This will be used to temporarily indicate spaces returned by scanner dead keys.
        try {
            unusedChar = input.UnusedExtendedAsciiCharacter();
            return true;
        }
        catch (CalibrationException) {
            // All extended ASCII characters are used in the data string.
            return false;
        }
    }

    /// <summary>
    ///   Finds an extended ASCII character that is not being used in the input string.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>An extended ASCII character that is not being used in the input string.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static char UnusedExtendedAsciiCharacter(this string input)
    {
        // Find a character that is not being used in the sequence. This will be used to temporarily indicate spaces returned by scanner dead keys.
        var candidateCharacter = default(char);

        for (var extAsciiChar = 128; extAsciiChar <= 255; extAsciiChar++) {
            candidateCharacter = (char)extAsciiChar;

            if (!input.Contains(candidateCharacter, StringComparison.Ordinal)) {
                break;
            }

            candidateCharacter = default;
        }

        if (candidateCharacter == default) {
            throw new CalibrationException(Properties.Resources.UnusedExtendedAsciiCharacterNotFound);
        }

        return candidateCharacter;
    }

    /// <summary>
    ///   Processes a string, normalising it according the the calibration character map,
    ///   converting into the correct sequence of characters.
    /// </summary>
    /// <param name="characters">
    ///   The string to be processed. This is data transmitted by the barcode scanner, as reported to
    ///   the application.
    /// </param>
    /// <param name="dataCharacterMap">The character map.</param>
    /// <param name="deadKeyCharacterMap">The dead key character map.</param>
    /// <param name="deadKeysMap">The dead keys map.</param>
    /// <param name="aimFlagCharacterSequence">The AIM flag character sequence.</param>
    /// <returns>The normalised string, processed according to the calibration character map.</returns>
    public static string NormaliseCharacters(
        this string characters, 
        IDictionary<char, char> dataCharacterMap,
        IDictionary<string, char> deadKeyCharacterMap, 
        IDictionary<string, string> deadKeysMap,
        string? aimFlagCharacterSequence = null)
    {
        var builder = new StringBuilder();

        for (var idx = 0; idx < characters.Length; idx++) {
            var character = characters[idx];
            var foundDeadKey = false;

            if (character == '\0') {
                foreach (var map in deadKeyCharacterMap) {
                    var endIndex = idx + map.Key.Length;
                    if (endIndex >= characters.Length) endIndex = characters.Length - 1;
                    if (map.Key != characters[idx..endIndex]) continue;

                    foundDeadKey = true;
                    idx += map.Key.Length - 1;
                    character = map.Value;
                }

                if (!foundDeadKey) {
                    foreach (var map in deadKeysMap) {
                        // Look up the value in the dead Key character map
                        var deadkeyMap = (from m in deadKeyCharacterMap
                            where m.Value == map.Value[0]
                            select map).FirstOrDefault();

                        var endIndex = idx + map.Key.Length;
                        if (endIndex >= characters.Length) endIndex = characters.Length - 1;
                        if (deadkeyMap.Key != characters[idx..endIndex]) continue;

                        idx += map.Key.Length - 1;
                        character = deadkeyMap.Value[0];
                    }
                }

                builder.Append(character);
            }
            else {
                // The AIM flag character may have been stripped out of the character map because it is
                // ambiguous. In this case, we will attempt to resolve using the recorded flag character sequence.
                string ResolveFlagCharacter(char mappedValue) {
                    if (!string.IsNullOrEmpty(aimFlagCharacterSequence) && character == aimFlagCharacterSequence[0]) {
                        return "]";
                    }

                    return mappedValue.ToInvariantString();
                }

                builder.Append(
                    dataCharacterMap.TryGetValue(character, out var value)
#pragma warning disable S3358
                        ? idx == 0
                            ? ResolveFlagCharacter(value)
                            : value.ToInvariantString()
#pragma warning restore S3358
                        : ResolveFlagCharacter(character));
            }
        }

        return builder.ToString();
    }

    /// <summary>
    ///   Determines if a character is drawn from the invariant character set.
    /// </summary>
    /// <param name="character">The character to be assessed.</param>
    /// <returns>True, if the character is invariant; otherwise false.</returns>
    [Pure]
    public static bool IsInvariant(this char character) =>
        character switch {
            _ when character == 33 || character == 34  => true,
            _ when character > 36  && character < 64   => true,
            _ when character > 64  && character < 91   => true,
            _ when character == 95                     => true,
            _ when character > 96  && character < 123  => true,
            _                                            => false
        };


    /// <summary>
    ///   Converts a an ASCII control character to a Unicode Control Picture character.
    /// </summary>
    /// <param name="originalChar">The original character.</param>
    /// <returns>
    ///   The Unicode Control Picture character. If the original character is not an ASCII
    ///   control character, it is returned unchanged.
    /// </returns>
    [Pure]
    public static char ToControlPicture(this char originalChar) =>
        originalChar switch
        {
            _ when originalChar < 32 => (char)(originalChar + 9216),
            _ => originalChar
        };

    /// <summary>
    ///   Converts a an ASCII control character to a Unicode Control Picture string.
    /// </summary>
    /// <param name="originalChar">The original character.</param>
    /// <returns>
    ///   The Unicode Control Picture string. If the original character is not an ASCII
    ///   control character, it is returned unchanged.
    /// </returns>
    [Pure]
    public static string ToControlPictureString(this char originalChar) =>
        ToControlPicture(originalChar).ToInvariantString();

    /// <summary>
    ///   Converts any ASCII control characters in a string to Unicode Control Picture characters.
    /// </summary>
    /// <param name="originalString">The original string.</param>
    /// <returns>A string containing Unicode Control Pictures for any ASCII control characters.</returns>
    [Pure]
    public static string ToControlPictures(this string originalString) =>
        new(originalString.ToCharArray().Select(c => c.ToControlPicture()).ToArray());

}