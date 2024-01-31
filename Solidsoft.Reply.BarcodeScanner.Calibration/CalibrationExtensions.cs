// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalibrationExtensions.cs" company="Solidsoft Reply Ltd.">
//   (c) 2018-2024 Solidsoft Reply Ltd. All rights reserved.
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
using System.Text.RegularExpressions;

/// <summary>
///   Extension methods for calibration.
/// </summary>
// ReSharper disable once UnusedMember.Global
#if NET7_0_OR_GREATER
internal static partial class CalibrationExtensions
{

    /// <summary>
    ///   Returns a regular expression for matching line terminators and providing a stripped string.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"^(?<strippedData>.*?)(?<lineTerminators>[\r\n]+)$", RegexOptions.None, "en-US")]
    private static partial Regex LineTerminators();

    /// <summary>
    ///   Returns a regular expression for matching Control Character terminators and providing a stripped string.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"^(?<strippedData>.*?) {4}(?<lineTerminators>[\x01..\x1F]+)$", RegexOptions.None, "en-US")]
    private static partial Regex ControlCharTerminators();
#else
internal static class CalibrationExtensions {
    
    /// <summary>
    ///   Returns a regular expression for matching line terminators and providing a stripped string.
    /// </summary>
    private static readonly Regex LineTerminators = new (@"^(?<strippedData>.*?)(?<lineTerminators>[\r\n]+)$", RegexOptions.None);

    /// <summary>
    ///   Returns a regular expression for matching Control Character terminators and providing a stripped string.
    /// </summary>
    private static readonly Regex ControlCharTerminators = new (@"^(?<strippedData>.*?) {4}(?<lineTerminators>[\x01..\x1F]+)$", RegexOptions.None);
#endif
    
    /// <summary>
    ///   Strips off any trailing carriage return and line feed characters.
    /// </summary>
    /// <param name="input">The input data from which trailing CR and LF characters will be stripped</param>
    /// <param name="trailingCrLfChars">The trailing CR and LF characters, if any; Otherwise, and empty string.</param>
    public static string StripTrailingCrLfs(this string input, out string trailingCrLfChars)
    {
        trailingCrLfChars = string.Empty;

        if (input.Length == 0) return input;

        var matchLineTerminators = LineTerminators
#if NET7_0_OR_GREATER
            ()
#endif
            .Match(input);

        if (!matchLineTerminators.Success)
        {
            // We will look specifically for any situation where four spaces (the segment delimiter) are followed by 
            // ASCII control characters at the end of the string.  In this case, we will assume that the control
            // characters represent line terminators.  This is a best-endeavours approach.  It cannot handle 
            // suffixes (unless they end with four spaces, in which case the assumption of line terminators may not be
            // valid) and it assumes that the space character is reported without change.  This approach not 100%
            // reliable, but can only be defeated in highly unusual circumstances.
            var matchControlChars = ControlCharTerminators
#if NET7_0_OR_GREATER
                ()
#endif
                .Match(input);

            if (!matchControlChars.Success) return input;
            var trailingCrLfCharsSpan = matchLineTerminators.Groups["lineTerminators"].ValueSpan;

            // Are there more than two control characters represented in the trailing characters?
            var characters = new List<char>();

            foreach (var c in trailingCrLfCharsSpan)
            {
                if (characters.Contains(c)) continue;
                characters.Add(c);
            }

            if (characters.Count > 2) return input;

            for (var idx = 0; idx < characters.Count; idx++)
            {
                _ = characters[idx];
                trailingCrLfChars += idx == 0 ? "CR" : "LF";
            }

            return matchLineTerminators.Groups["strippedData"].Value;
        }

        trailingCrLfChars = matchLineTerminators.Groups["lineTerminators"].Value.Replace("\r", "CR").Replace("\n", "LF");
        return matchLineTerminators.Groups["strippedData"].Value;
    }

    /// <summary>
    ///   Strips off any trailing carriage return and line feed characters.
    /// </summary>
    /// <param name="input">The input data from which trailing CR and LF characters will be stripped</param>
    /// <param name="baseline">Indicates if this is the baseline barcode or the last small barcode in the baseline sequence.</param>
    /// <returns>The input data without any trailing CR or LF characters.</returns>
    public static string StripTrailingCrLfs(this string input, bool baseline = false) =>
        StripTrailingCrLfs(input, out _);

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
    ///   Processes a string, normalising it according to the calibration character map,
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
    ///   Converts an ASCII control character to a Unicode Control Picture character.
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
    ///   Converts an ASCII control character to a Unicode Control Picture string.
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