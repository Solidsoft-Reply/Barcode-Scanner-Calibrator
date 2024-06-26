﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnicodeBlocks.cs" company="Solidsoft Reply Ltd">
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
// Resolves a sequence of characters to a Unicode block that heuristically represents the kind of
// keyboard configured in the OS.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
///   Resolves a sequence of characters to a Unicode block that heuristically represents the kind of
///   keyboard configured in the OS.
/// </summary>
internal static class UnicodeBlocks {
    /// <summary>
    ///   A dictionary of ranges for each Unicode block.
    /// </summary>
    private static readonly IDictionary<(int, int), string> UnicodeBlocksDictionary =
        new Dictionary<(int, int), string>
        {
            { (0, 127), "Basic Latin" },
            { (128, 255), "Latin-1 Supplement" },
            { (256, 383), "Latin Extended-A" },
            { (384, 591), "Latin Extended-B" },
            { (592, 687), "IPA Extensions" },
            { (688, 767), "Spacing Modifier Letters" },
            { (768, 879), "Combining Diacritical Marks" },
            { (880, 993), "Greek" },
            { (994, 1023), "Coptic" },
            { (1024, 1279), "Cyrillic" },
            { (1280, 1327), "Cyrillic Supplement" },
            { (1328, 1423), "Armenian" },
            { (1424, 1535), "Hebrew" },
            { (1536, 1791), "Arabic" },
            { (1792, 1871), "Syriac" },
            { (1872, 1919), "Arabic Supplement" },
            { (1920, 1983), "Thaana" },
            { (1984, 2047), "NKo" },
            { (2048, 2111), "Samaritan" },
            { (2112, 2143), "Mandaic" },
            { (2144, 2159), "Syriac Supplement" },
            { (2208, 2303), "Arabic Extended-A" },
            { (2304, 2431), "Devanagari" },
            { (2432, 2559), "Bengali" },
            { (2560, 2687), "Gurmukhi" },
            { (2688, 2815), "Gujarati" },
            { (2816, 2943), "Oriya" },
            { (2944, 3071), "Tamil" },
            { (3072, 3199), "Telugu" },
            { (3200, 3327), "Kannada" },
            { (3328, 3455), "Malayalam" },
            { (3456, 3583), "Sinhala" },
            { (3584, 3711), "Thai" },
            { (3712, 3839), "Lao" },
            { (3840, 4095), "Tibetan" },
            { (4096, 4255), "Myanmar" },
            { (4256, 4351), "Georgian" },
            { (4352, 4607), "Hangul Jamo" },
            { (4608, 4991), "Ethiopic" },
            { (4992, 5023), "Ethiopic Supplement" },
            { (5024, 5119), "Cherokee" },
            { (5120, 5759), "Unified Canadian Aboriginal Syllabics" },
            { (5760, 5791), "Ogham" },
            { (5792, 5887), "Runic" },
            { (5888, 5919), "Tagalog" },
            { (5920, 5951), "Hanunoo" },
            { (5952, 5983), "Buhid" },
            { (5984, 6015), "Tagbanwa" },
            { (6016, 6143), "Khmer" },
            { (6144, 6319), "Mongolian" },
            { (6320, 6399), "Unified Canadian Aboriginal Syllabics Extended" },
            { (6400, 6479), "Limbu" },
            { (6480, 6527), "Tai Le" },
            { (6528, 6623), "New Tai Lue" },
            { (6624, 6655), "Khmer Symbols" },
            { (6656, 6687), "Buginese" },
            { (6688, 6831), "Tai Tham" },
            { (6832, 6911), "Combining Diacritical Marks Extended" },
            { (6912, 7039), "Balinese" },
            { (7040, 7103), "Sundanese" },
            { (7104, 7167), "Batak" },
            { (7168, 7247), "Lepcha" },
            { (7248, 7295), "Ol Chiki" },
            { (7296, 7311), "Cyrillic Extended-C" },
            { (7312, 7359), "Georgian Extended" },
            { (7360, 7375), "Sundanese Supplement" },
            { (7376, 7423), "Vedic Extensions" },
            { (7424, 7551), "Phonetic Extensions" },
            { (7552, 7615), "Phonetic Extensions Supplement" },
            { (7616, 7679), "Combining Diacritical Marks Supplement" },
            { (7680, 7935), "Latin Extended Additional" },
            { (7936, 8191), "Greek Extended" },
            { (8192, 8303), "General Punctuation" },
            { (8304, 8351), "Superscripts and Subscripts" },
            { (8352, 8399), "Currency Symbols" },
            { (8400, 8447), "Combining Diacritical Marks for Symbols" },
            { (8448, 8527), "Letterlike Symbols" },
            { (8528, 8591), "Number Forms" },
            { (8592, 8703), "Arrows" },
            { (8704, 8959), "Mathematical Operators" },
            { (8960, 9215), "Miscellaneous Technical" },
            { (9216, 9279), "Control Pictures" },
            { (9280, 9311), "Optical Character Recognition" },
            { (9312, 9471), "Enclosed Alphanumerics" },
            { (9472, 9599), "Box Drawing" },
            { (9600, 9631), "Block Elements" },
            { (9632, 9727), "Geometric Shapes" },
            { (9728, 9983), "Miscellaneous Symbols" },
            { (9984, 10175), "Dingbats" },
            { (10176, 10223), "Miscellaneous Mathematical Symbols-A" },
            { (10224, 10239), "Supplemental Arrows-A" },
            { (10240, 10495), "Braille Patterns" },
            { (10496, 10623), "Supplemental Arrows-B" },
            { (10624, 10751), "Miscellaneous Mathematical Symbols-B" },
            { (10752, 11007), "Supplemental Mathematical Operators" },
            { (11008, 11263), "Miscellaneous Symbols and Arrows" },
            { (11264, 11359), "Glagolitic" },
            { (11360, 11391), "Latin Extended-C" },
            { (11392, 11519), "Coptic" },
            { (11520, 11567), "Georgian Supplement" },
            { (11568, 11647), "Tifinagh" },
            { (11648, 11743), "Ethiopic Extended" },
            { (11744, 11775), "Cyrillic Extended-A" },
            { (11776, 11903), "Supplemental Punctuation" },
            { (11904, 12031), "CJK Radicals Supplement" },
            { (12032, 12255), "Kangxi Radicals" },
            { (12272, 12287), "Ideographic Description Characters" },
            { (12288, 12351), "CJK Symbols and Punctuation" },
            { (12352, 12447), "Hiragana" },
            { (12448, 12543), "Katakana" },
            { (12544, 12591), "Bopomofo" },
            { (12592, 12687), "Hangul Compatibility Jamo" },
            { (12688, 12703), "Kanbun" },
            { (12704, 12735), "Bopomofo Extended" },
            { (12736, 12783), "CJK Strokes" },
            { (12784, 12799), "Katakana Phonetic Extensions" },
            { (12800, 13055), "Enclosed CJK Letters and Months" },
            { (13056, 13311), "CJK Compatibility" },
            { (13312, 19903), "CJK Unified Ideographs Extension A" },
            { (19904, 19967), "Yijing Hexagram Symbols" },
            { (19968, 40959), "CJK Unified Ideographs" },
            { (40960, 42127), "Yi Syllables" },
            { (42128, 42191), "Yi Radicals" },
            { (42192, 42239), "Lisu" },
            { (42240, 42559), "Vai" },
            { (42560, 42655), "Cyrillic Extended-B" },
            { (42656, 42751), "Bamum" },
            { (42752, 42783), "Modifier Tone Letters" },
            { (42784, 43007), "Latin Extended-D" },
            { (43008, 43055), "Syloti Nagri" },
            { (43056, 43071), "Common Indic Number Forms" },
            { (43072, 43135), "Phags-pa" },
            { (43136, 43231), "Saurashtra" },
            { (43232, 43263), "Devanagari Extended" },
            { (43264, 43311), "Kayah Li" },
            { (43312, 43359), "Rejang" },
            { (43360, 43391), "Hangul Jamo Extended-A" },
            { (43392, 43487), "Javanese" },
            { (43488, 43519), "Myanmar Extended-B" },
            { (43520, 43615), "Cham" },
            { (43616, 43647), "Myanmar Extended-A" },
            { (43648, 43743), "Tai Viet" },
            { (43744, 43775), "Meetei Mayek Extensions" },
            { (43776, 43823), "Ethiopic Extended-A" },
            { (43824, 43887), "Latin Extended-E" },
            { (43888, 43967), "Cherokee Supplement" },
            { (43968, 44031), "Meetei Mayek" },
            { (44032, 55215), "Hangul Syllables" },
            { (55216, 55295), "Hangul Jamo Extended-B" },
            { (55296, 56191), "High Surrogates" },
            { (56192, 56319), "High Private Use Surrogates" },
            { (56320, 57343), "Low Surrogates" },
            { (57344, 63743), "Private Use Area" },
            { (63744, 64255), "CJK Compatibility Ideographs" },
            { (64256, 64335), "Alphabetic Presentation Forms" },
            { (64336, 65023), "Arabic Presentation Forms-A" },
            { (65024, 65039), "Variation Selectors" },
            { (65040, 65055), "Vertical Forms" },
            { (65056, 65071), "Combining Half Marks" },
            { (65072, 65103), "CJK Compatibility Forms" },
            { (65104, 65135), "Small Form Variants" },
            { (65136, 65279), "Arabic Presentation Forms-B" },
            { (65280, 65519), "Halfwidth and Fullwidth Forms" },
            { (65520, 65535), "Specials" },
            { (65536, 65663), "Linear B Syllabary" },
            { (65664, 65791), "Linear B Ideograms" },
            { (65792, 65855), "Aegean Numbers" },
            { (65856, 65935), "Ancient Greek Numbers" },
            { (65936, 65999), "Ancient Symbols" },
            { (66000, 66047), "Phaistos Disc" },
            { (66176, 66207), "Lycian" },
            { (66208, 66271), "Carian" },
            { (66272, 66303), "Coptic Epact Numbers" },
            { (66304, 66351), "Old Italic" },
            { (66352, 66383), "Gothic" },
            { (66384, 66431), "Old Permic" },
            { (66432, 66463), "Ugaritic" },
            { (66464, 66527), "Old Persian" },
            { (66560, 66639), "Deseret" },
            { (66640, 66687), "Shavian" },
            { (66688, 66735), "Osmanya" },
            { (66736, 66815), "Osage" },
            { (66816, 66863), "Elbasan" },
            { (66864, 66927), "Caucasian Albanian" },
            { (67072, 67455), "Linear A" },
            { (67584, 67647), "Cypriot Syllabary" },
            { (67648, 67679), "Imperial Aramaic" },
            { (67680, 67711), "Palmyrene" },
            { (67712, 67759), "Nabataean" },
            { (67808, 67839), "Hatran" },
            { (67840, 67871), "Phoenician" },
            { (67872, 67903), "Lydian" },
            { (67968, 67999), "Meroitic Hieroglyphs" },
            { (68000, 68095), "Meroitic Cursive" },
            { (68096, 68191), "Kharoshthi" },
            { (68192, 68223), "Old South Arabian" },
            { (68224, 68255), "Old North Arabian" },
            { (68288, 68351), "Manichaean" },
            { (68352, 68415), "Avestan" },
            { (68416, 68447), "Inscriptional Parthian" },
            { (68448, 68479), "Inscriptional Pahlavi" },
            { (68480, 68527), "Psalter Pahlavi" },
            { (68608, 68687), "Old Turkic" },
            { (68736, 68863), "Old Hungarian" },
            { (68864, 68927), "Hanifi Rohingya" },
            { (69216, 69247), "Rumi Numeral Symbols" },
            { (69376, 69423), "Old Sogdian" },
            { (69424, 69487), "Sogdian" },
            { (69600, 69631), "Elymaic" },
            { (69632, 69759), "Brahmi" },
            { (69760, 69839), "Kaithi" },
            { (69840, 69887), "Sora Sompeng" },
            { (69888, 69967), "Chakma" },
            { (69968, 70015), "Mahajani" },
            { (70016, 70111), "Sharada" },
            { (70143, 70143), "Sinhala Archaic Numbers" },
            { (70144, 70223), "Khojki" },
            { (70272, 70319), "Multani" },
            { (70320, 70399), "Khudawadi" },
            { (70400, 70527), "Grantha" },
            { (70656, 70783), "Newa" },
            { (70784, 70879), "Tirhuta" },
            { (71040, 71167), "Siddham" },
            { (71168, 71263), "Modi" },
            { (71264, 71295), "Mongolian Supplement" },
            { (71296, 71375), "Takri" },
            { (71424, 71487), "Ahom" },
            { (71680, 71759), "Dogra" },
            { (71840, 71935), "Warang Citi" },
            { (72096, 72191), "Nandinagari" },
            { (72192, 72271), "Zanabazar Square" },
            { (72272, 72367), "Soyombo" },
            { (72384, 72447), "Pau Cin Hau" },
            { (72704, 72815), "Bhaiksuki" },
            { (72816, 72895), "Marchen" },
            { (72960, 73055), "Masaram Gondi" },
            { (73056, 73135), "Gunjala Gondi" },
            { (73440, 73471), "Makasar" },
            { (73664, 73727), "Tamil Supplement" },
            { (73728, 74751), "Cuneiform" },
            { (74752, 74879), "Cuneiform Numbers and Punctuation" },
            { (74880, 75087), "Early Dynastic Cuneiform" },
            { (77824, 78895), "Egyptian Hieroglyphs" },
            { (78896, 78911), "Egyptian Hieroglyph Format Controls" },
            { (82944, 83583), "Anatolian Hieroglyphs" },
            { (92160, 92735), "Bamum Supplement" },
            { (92736, 92783), "Mro" },
            { (92880, 92927), "Bassa Vah" },
            { (92928, 93071), "Pahawh Hmong" },
            { (93760, 93855), "Medefaidrin" },
            { (93952, 94111), "Miao" },
            { (94176, 94207), "Ideographic Symbols and Punctuation" },
            { (94208, 100351), "Tangut" },
            { (100352, 101119), "Tangut Components" },
            { (110592, 110847), "Kana Supplement" },
            { (110848, 110895), "Kana Extended-A" },
            { (110896, 110959), "Small Kana Extension" },
            { (110960, 111359), "Nushu" },
            { (113664, 113823), "Duployan" },
            { (113824, 113839), "Shorthand Format Controls" },
            { (118784, 119039), "Byzantine Musical Symbols" },
            { (119040, 119295), "Musical Symbols" },
            { (119296, 119375), "Ancient Greek Musical Notation" },
            { (119520, 119551), "Mayan Numerals" },
            { (119552, 119647), "Tai Xuan Jing Symbols" },
            { (119648, 119679), "Counting Rod Numerals" },
            { (119808, 120831), "Mathematical Alphanumeric Symbols" },
            { (120832, 121519), "Sutton SignWriting" },
            { (122880, 122927), "Glagolitic Supplement" },
            { (123136, 123215), "Nyiakeng Puachue Hmong" },
            { (123584, 123647), "Wancho" },
            { (124928, 125151), "Mende Kikakui" },
            { (125184, 125279), "Adlam" },
            { (126064, 126143), "Indic Siyaq Numbers" },
            { (126208, 126287), "Ottoman Siyaq Numbers" },
            { (126464, 126719), "Arabic Mathematical Alphabetic Symbols" },
            { (126976, 127023), "Mahjong Tiles" },
            { (127024, 127135), "Domino Tiles" },
            { (127136, 127231), "Playing Cards" },
            { (127232, 127487), "Enclosed Alphanumeric Supplement" },
            { (127488, 127743), "Enclosed Ideographic Supplement" },
            { (127744, 128511), "Miscellaneous Symbols and Pictographs" },
            { (128512, 128591), "Emoticons" },
            { (128592, 128639), "Ornamental Dingbats" },
            { (128640, 128767), "Transport and Map Symbols" },
            { (128768, 128895), "Alchemical Symbols" },
            { (128896, 129023), "Geometric Shapes Extended" },
            { (129024, 129279), "Supplemental Arrows-C" },
            { (129280, 129535), "Supplemental Symbols and Pictographs" },
            { (129536, 129647), "Chess Symbols" },
            { (129648, 129791), "Symbols and Pictographs Extended-A" },
            { (131072, 173791), "CJK Unified Ideographs Extension B" },
            { (173824, 177983), "CJK Unified Ideographs Extension C" },
            { (177984, 178207), "CJK Unified Ideographs Extension D" },
            { (178208, 183983), "CJK Unified Ideographs Extension E" },
            { (183984, 191471), "CJK Unified Ideographs Extension F" },
            { (194560, 195103), "CJK Compatibility Ideographs Supplement" },
            { (917504, 917631), "Tags" },
            { (917760, 917999), "Variation Selectors Supplement" },
            { (983040, 1048575), "Supplementary Private Use Area-A" },
            { (1048576, 1114111), "Supplementary Private Use Area-B" },
        };

    /// <summary>
    ///   Resolve the keyboard script for sequences representing upper and lower case characters.
    /// </summary>
    /// <param name="upperCaseSequences">A sequence representing upper case characters.</param>
    /// <param name="lowerCaseSequences">A sequence representing lower case characters.</param>
    /// <returns>The name of the Unicode block representing the keyboard script for the sequences.</returns>
    public static string ResolveScript(
        IEnumerable<string>? upperCaseSequences,
        IEnumerable<string>? lowerCaseSequences) {
        var scriptUpper = "<unknown>";
        var scriptLower = "<unknown>";

        if (upperCaseSequences is null || lowerCaseSequences is null) {
            return scriptUpper;
        }

        var upperSequences = upperCaseSequences.ToArray();
        var lowerSequences = lowerCaseSequences.ToArray();

        var upperReachedThreshold = Resolve();
        var lowerReachedThreshold = Resolve(false);

        // Flatten the Unicode block name.
        static string Flatten(string scriptName) {
#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
            var firstWord = scriptName.Split(' ', '-')[0];
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"

            return firstWord switch {
                "Latin" => "Latin",
                "IPA" => "Latin",
                "Phonetic" => "Latin",
                "Cyrillic" => "Cyrillic",
                "Greek" => "Greek",
                "Armenian" => "Armenian",
                "Georgian" => "Georgian",
                "Glagolitic" => "Glagolitic",
                "LinearB" => "LinearB",
                "Bamun" => "Bamun",
                "Coptic" => "Coptic",
                "Ethiopic" => "Ethiopic",
                "Metroitic" => "Metroitic",
                "Arabic" => "Arabic",
                "Hebrew" => "Hebrew",
                "Syriac" => "Syriac",
                "Mongolian" => "Mongolian",
                "Devanagari" => "Devanagari",
                "Sinhala" => "Sinhala",
                "Tamil" => "Tamil",
                "Khmer" => "Khmer",
                "Myanmar" => "Myanmar",
                "Sundanese" => "Sundanese",
                "Bopomofo" => "Bopomofo",
                "Katakana" => "Katakana",
                "Tangut" => "Tangut",
                "Yi" => "Yi",
                "Cherokee" => "Cherokee",
                _ => scriptName switch {
                    "Ancient Greek Numbers" => "Greek",
                    "Latin-1 Supplement" => "Latin",
                    "Aegean Numbers" => "LinearB",
                    "Combining Diacritical Marks Extended" => "Combining Diacritical Marks",
                    "Combining Diacritical Marks Supplement" => "Combining Diacritical Marks",
                    "Egyptian Hieroglyph Format Controls" => "Egyptian Hieroglyphs",
                    "Cuneiform Numbers and Punctuation" => "Cuniform",
                    "Early Dynastic Cuneiform" => "Cuniform",
                    "Old Persian" => "Cuniform",
                    "Ugaritic" => "Cuniform",
                    "Meetei Mayek Extensions" => "Meetei Mayek",
                    "CJK Unified Ideographs Extension A" => "CJK Unified Ideographs (Han)",
                    "CJK Unified Ideographs Extension B" => "CJK Unified Ideographs (Han)",
                    "CJK Unified Ideographs Extension C" => "CJK Unified Ideographs (Han)",
                    "CJK Unified Ideographs Extension D" => "CJK Unified Ideographs (Han)",
                    "CJK Unified Ideographs Extension E" => "CJK Unified Ideographs (Han)",
                    "CJK Unified Ideographs Extension F" => "CJK Unified Ideographs (Han)",
                    "CJK Compatibility Ideographs Supplement" => "CJK Compatibility Ideographs",
                    "CJK Radicals Supplement" => "CJK Radicals",
                    "CJK Strokes" => "CJK Radicals",
                    "Ideographic Description Characters" => "CJK Radicals",
                    "Hangul Jamo Extended - A" => "Hangul Jamo",
                    "Hangul Jamo Extended - B" => "Hangul Jamo",
                    "Hangul Compatibility Jamo" => "Hangul Jamo",
                    "Unified Canadian Aboriginal Syllabics Extended" => "Unified Canadian Aboriginal Syllabics",
                    "Ancient Greek Musical Notation" => "Musical Symbols",
                    "Byzantine Musical Symbols" => "Musical Symbols",
                    "Shorthand Format Controls" => "Duployan",
                    "Supplemental Punctuation" => "General Punctuation",
                    "Ideographic Symbols and Punctuation" => "CJK Symbols and Punctuation",
                    "Halfwidth and Fullwidth Forms" => "CJK Compatibility Forms",
                    "Small Form Variants" => "CJK Compatibility Forms",
                    "Vertical Forms" => "CJK Compatibility Forms",
                    "Enclosed Alphanumeric Supplement" => "Enclosed Alphanumerics",
                    "Enclosed Ideographic Supplement" => "Enclosed CJK Letters and Months",
                    "Supplemental Arrows-A" => "Arrows",
                    "Supplemental Arrows-B" => "Arrows",
                    "Supplemental Arrows-C" => "Arrows",
                    "Miscellaneous Symbols and Arrows" => "Arrows",
                    "Arabic Mathematical Alphabetic Symbols" => "Mathematical Alphanumeric Symbols",
                    "Letterlike Symbols" => "Mathematical Alphanumeric Symbols",
                    "Supplemental Mathematical Operators" => "Mathematical Operators",
                    "Miscellaneous Mathematical Symbols - A" => "Mathematical Operators",
                    "Miscellaneous Mathematical Symbols - B" => "Mathematical Operators",
                    "Box Drawing" => "Geometric Shapes",
                    "Block Elements" => "Geometric Shapes",
                    "Geometric Shapes Extended" => "Geometric Shapes",
                    "Ornamental Dingbats" => "Dingbats",
                    _ => scriptName
                }
            };
        }

        FlattenExtendedScripts();

        return upperReachedThreshold && lowerReachedThreshold
                   ? TestScriptsEqual()
                   : TestUnknownScriptUpper();

        bool Resolve(bool upper = true) {
            var resolverDictionary = new Dictionary<string, int>();
            var characterCount = 0D;

            foreach (var sequence in upper ? upperSequences : lowerSequences) {
                foreach (var character in sequence) {
                    characterCount++;

                    foreach (var ((item1, item2), value) in UnicodeBlocksDictionary) {
                        var characterValue = (int)character;

                        if (characterValue < item1 || characterValue > item2) {
                            continue;
                        }

                        resolverDictionary.TryAdd(value, 0);

                        resolverDictionary[value] += 1;

                        break;
                    }
                }
            }

            var reachedThreshold = Convert.ToDouble(resolverDictionary.Values.Max()) / characterCount > 0.65;

            if (reachedThreshold) {
                if (upper) {
                    scriptUpper = (from candidates in resolverDictionary
                                   where candidates.Value == resolverDictionary.Values.Max()
                                   select candidates.Key).First();
                }
                else {
                    scriptLower = (from candidates in resolverDictionary
                                   where candidates.Value == resolverDictionary.Values.Max()
                                   select candidates.Key).First();
                }
            }
            else {
                if (upper) {
                    scriptUpper = "<unknown>";
                }
                else {
                    scriptLower = "<unknown>";
                }
            }

            return reachedThreshold;
        }

        // On a Hebrew keyboard, SHIFT (and CAPS LOCK) provide access to Latin characters. This is very unusual. The
        // Sinhala Wij 9 keyboard, for example, provides access to Latin characters on CAPS LOCK, but not on SHIFT.
        bool CheckForHebrew() {
            return (scriptUpper == "Basic Latin" && scriptLower == "Hebrew")
                   || (scriptUpper == "Hebrew" && scriptLower == "Basic Latin");
        }

        void FlattenExtendedScripts() {
            scriptUpper = scriptUpper == "Basic Latin" ? "Latin" : scriptUpper;
            scriptLower = scriptLower == "Basic Latin" ? "Latin" : scriptLower;

            if (scriptUpper == scriptLower) {
                return;
            }

            // We will treat Georgian (Old Alphabets) as a special case, because the corresponding
            // keyboard supports CAPS LOCK to switch between Georgian and Latin scripts.
            if (scriptUpper == "Georgian Supplement" && scriptLower == "Georgian") {
                scriptUpper = scriptLower = "Georgian (Old Alphabets)";
                return;
            }

            scriptUpper = Flatten(scriptUpper);
            scriptLower = Flatten(scriptLower);
        }

        string TestUnknownScripts() =>
            scriptLower == "<unknown>"
                ? scriptUpper
                : scriptLower;

        string TestUnknownScriptUpper() =>
            scriptUpper == "<unknown>"
                ? scriptLower
                : TestUnknownScripts();

        string TestForHebrew() =>
            CheckForHebrew() ? "Hebrew" : "<unknown>";

        string TestScriptsEqual() =>
            scriptUpper == scriptLower ? scriptUpper : TestForHebrew();
    }
}