﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Calibrator.cs" company="Solidsoft Reply Ltd">
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
// Calibrates for a given combination of barcode scanner and OS keyboard layouts.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

////#define Diagnostics

using System.Diagnostics;
#pragma warning disable RS0016

#pragma warning disable S1751
#pragma warning disable S3626

[assembly: CLSCompliant(true)]

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Parsers.Common;

using DataMatrix;

using ProcessFlow;

using Properties;

using EnvToken = ProcessFlow.Environment<Token>;
using SixLabors.ImageSharp;
using System.Threading;

/// <summary>
///   Delegate for pre-processor functions.
/// </summary>
/// <param name="input">The data input to be pre-processed.</param>
/// <param name="exceptions">Collection of exceptions.</param>
/// <returns>The pre-processed data.</returns>
public delegate string? Preprocessor(string? input, out IList<PreprocessorException>? exceptions);

/* Dead Key Handling
 * *****************
 *
 * A dead key is a key that, when pressed, does not appear to output any character. It is only when the next key is pressed that the effect
 * of the dead key is seen. Dead keys are often used to output accented characters. E.g., if ' is a dead key character, then 'e might
 * generate é. The use of dead keys can generally be detected by the presence of an ASCII 0 (NULL) just before the output character. So,
 * in the given example, the actual output sequence is \u0000\u0233 (\0é).
 *
 * If a dead key does not modify the next typed character, the result is the literal dead key character, preceded by an ASCII 0, followed
 * by the next character. So, if ' is a dead key character, but does not modify e, the resulting sequence will be \u0000\u0039\u0101 (\0'e).
 * Typing the same dead key twice will output thw dead key characters e.g., \u0000\u0039\u0039 (\0'')
 *
 * To type the literal dead key character, keyboards generally support the convention of pressing the dead key character followed by the space
 * So, if ' is a dead key, to type a literal ', press ' followed by the space bar. The resulting sequence will be \u0000\u0039 (\0').
 *
 * When a barcode scanner sees a character in a barcode that is a dead key on its current keyboard configuration, it generally outputs the scan codes
 * for the dead key, followed by a space. If the dead key scan code maps to a character on the keyboard configuration set up under the
 * operating system, the application will see the mapped character followed by a space, unless the mapped character is, itself a dead key under
 * the OS keyboard configuration. In that case, the <char><space> sequence sent by the barcode scanner will be processed as a dead key followed by a
 * space, which as we have seen, is generally supported as a way to type in the literal dead key character.  So, if the first character in a
 * <char><space> sequence maps to ' which is a dead key under the OS keyboard configuration, the output will be \u0000\u0039 (\0').
 *
 * Of course, this only works if keyboards follow the space bar convention. This cannot be guaranteed. For example, Swiss French
 * keyboards use ¨ as a dead key. When followed by a space, the dead key modifies the space to produce a quote (\0").
 *
 * */

/// <summary>
///   Manages the calibration for a given combination of barcode scanner and OS keyboard layouts.
/// </summary>
#if NET7_0_OR_GREATER
public partial class Calibrator {
#else
public class Calibrator {
#endif

    /// <summary>
    ///   ASCII Character set.
    /// </summary>
    private const string AsciiChars =
        @"!""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

    /// <summary>
    ///   The section delimiter.
    /// </summary>
    private const string SegmentDelimiter = "    ";

    /// <summary>
    /// Value for ASCII Null character.
    /// </summary>
    private const int AsciiNullChar = 0;

    /// <summary>
    /// Value for ASCII Line Feed character.
    /// </summary>
    private const int AsciiLineFeedChar = 10;

    /// <summary>
    /// Array on non-invariant, non-control ASCII characters.
    /// </summary>
    private static readonly int[] NonInvariantCharacters = [32, 35, 36, 64, 91, 92, 93, 94, 96, 123, 124, 125, 126, 127];

#if !NET7_0_OR_GREATER
    /// <summary>
    ///   Returns a regular expression for matching AIM identifiers.
    /// </summary>
    private static readonly Regex AimIdentifierMatchRegex = new (@"^(?<prefix>.*)(?<characters>][A-Za-z][1-9A-Za-z])(?<code>.*)$", RegexOptions.None);

    /// <summary>
    ///   Returns a regular expression for matching AIM identifiers with unrecognised flag characters.
    /// </summary>
    private static readonly Regex AimIdentifierUnrecognisedFlagMatchRegex = new (@"^(?<prefix>.*)(?<characters>\u0000[A-Za-z][1-9A-Za-z])(?<code>.*)$", RegexOptions.None);

    /// <summary>
    ///   Returns a regular expression for invariant character strings of variable length.
    /// </summary>
    private static readonly Regex InvariantsMatchRegex = new (@"^[-!""%&'()*+,./0-9:;<=>?A-Z_a-z]+$", RegexOptions.None);

    /// <summary>
    ///   Returns a regular expression for invariant character strings of variable length.
    /// </summary>
    private static readonly Regex DeadKeyAsciiControlCharacterSequenceRegex = new (@"\u0000(\u0000*\u0004)([^\u0000\u0020\u0040])$", RegexOptions.None);

    /// <summary>
    ///   Returns a regular expression for dead key candidates.
    /// </summary>
    private static readonly Regex DeadKeysRegex = new (@"\0.", RegexOptions.None);

    /// <summary>
    ///   Returns a regular expression for temporary space holder insertion for case 1.
    /// </summary>
    private static readonly Regex Case1TempSpaceHolderRegex = new (@"(?<a>\u0000[^\u0020])\u0020(?=[^\u0020])", RegexOptions.None);

    /// <summary>
    ///   Returns a regular expression for temporary space holder insertion for case 2.
    /// </summary>
    private static readonly Regex Case2TempSpaceHolderRegex = new (@"(?<a>\u0000[^\u0020]{0,1})\u0020{4}", RegexOptions.None);

    /// <summary>
    ///   Returns a regular expression for reported dead key sequences where the key is not a dead key on the
    ///   scanner keyboard.
    /// </summary>
    /// <returns>A regular expression.</returns>
    private static readonly Regex NonMatchingDeadKeyComputerKeyboardCandidatesRegex = new (@"(?<a>\u0000[^\u0020])((?=[^\u0020])|(?=(\u0020){2}))", RegexOptions.None);

    /// <summary>
    ///   Returns a regular expression for reported dead key sequences where the key is not a dead key on the
    ///   scanner keyboard.
    /// </summary>
    private static readonly Regex MatchingDeadKeyComputerKeyboardRegex = new (@"(?<a>\u0000[^\u0020])\u0020((?=[^\u0020])|(?=(\u0020){2}))", RegexOptions.None);

    /// <summary>
    ///   Returns a regular expression to detect sequences of six or more spaces in multiples
    ///   of three and mark each capture except the last.
    /// </summary>
    private static readonly Regex ThreeSpaceTempSpaceHolderRegex = new (@"(?<c>^|[^\u0020])(?<s>\u0020{3}){2,}?(?!\u0020)", RegexOptions.None);

    /// <summary>
    ///   Returns a regular expression to detect each occurrence of exactly two spaces.
    /// </summary>
    private static readonly Regex TwoSpaceTempSpaceHolderRegex = new (@"(?<c>^|[^\u0020])\u0020{2}(?!\u0020)", RegexOptions.None);

    /// <summary>
    ///   Returns a regular expression to detect unassigned keys.
    /// </summary>
    private static readonly Regex UnassignedKeysRegex = new (@"^\u0000\u0020?$", RegexOptions.None);

    /// <summary>
    ///   Returns a regular expression to filter dead key sequences (case 1).
    /// </summary>
    private static readonly Regex BarcodeScannerDeadKeysFilter1Regex = new (@"(^|\0)[^\u0020]\u0020$", RegexOptions.None);

    /// <summary>
    ///   Returns a regular expression to filter dead key sequences (case 2).
    /// </summary>
    private static readonly Regex BarcodeScannerDeadKeysFilter2Regex = new (@"^\0[^\u0020]\u0020$", RegexOptions.None);

    /// <summary>
    ///   Returns a regular expression to detect chained dead key sequences.
    /// </summary>
    private static readonly Regex ChainedDeadKeysFilter2Regex = new (@"\u0000{2,}(\w|[!""#$%&'()*+,./:;<=>?@\^_`{|}~-])", RegexOptions.None);

    /// <summary>
    ///   Returns a regular expression to detect suffixes. This is relevant during small barcode processing where
    ///   it is used to remove repeated suffixes from the reported data. It is a best-endeavours approach that
    ///   assumes that the suffix never contains a sequence of four or more spaces. It also takes into account
    ///   that in some cases, an ASCII 0 may not be reported for a Control Character that the barcodes scanner
    ///   does not support. It assumes that Control Characters never result in Dead Key activations.
    /// </summary>
    private static readonly Regex SuffixRegex = new (@"(([^\s]\s{4})|([^\s]\s{8})|([^\s]\s{12})|([^\s]\s{16})|([^\s]\s{19})|([^\s]\s{20}))(?!.*\s{4})(?<s>\s{0,3}.*)$", RegexOptions.None);

    /// <summary>
    /// Return a regular expression to detect any character except a space.
    /// </summary>
    private static readonly Regex AllSpaces = new (@"[^ ]", RegexOptions.None);

    /// <summary>
    ///   Returns a regular expression to detect prefixes.  It is a best-endeavours approach that
    ///   assumes that the prefix never contains a sequence of two or more spaces, unless they appear at the end of prefix.
    /// </summary>
    private static readonly Regex PrefixRegex = new (@"^(?<prefix>.*?)(?=\u0020\u0020[^\u0020]).*$", RegexOptions.None);
#endif

    /// <summary>
    ///   The baseline barcode data.
    /// </summary>
    private readonly string _baselineBarcodeData =
        $"  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` {{ | }} ~{SegmentDelimiter}{(char)29}";

    /// <summary>
    ///   The baseline barcode data.
    /// </summary>
    private readonly string _baselineBarcodeDataFormat06 = $"{(char)28}{SegmentDelimiter}{(char)30}{SegmentDelimiter}{(char)31}{SegmentDelimiter}{(char)04}";

    /// <summary>
    ///   Split characters that require escaping.
    /// </summary>
    private readonly char[] _unescapedSplitChars = ['[', '\\', '^', '$', '.', '|', '?', '*', '+', '(', ')'];

    /// <summary>
    ///   The assumption made concerning the use of calibration in client systems.
    /// </summary>
    private readonly Assumption _assumption;

    /// <summary>
    ///   A dictionary of ambiguous non-invariant character sequences that map to a reported character sequence.
    /// </summary>
    private readonly Dictionary<string, IList<string>> _tokenExtendedDataNonInvariantAmbiguities = [];

    /// <summary>
    ///   A dictionary of ambiguous invariant or other character sequences that may be used in GS1-compliant barcodes.
    /// </summary>
    private readonly Dictionary<string, IList<string>> _tokenExtendedDataInvariantGs1Ambiguities = [];

    /// <summary>
    ///   A list of unrecognised non-invariant characters.
    /// </summary>
    private readonly List<string> _tokenExtendedDataNonInvariantUnrecognisedCharacters = [];

    /// <summary>
    ///   A list of unrecognised invariant or other characters that may be used in GS1-compliant barcodes.
    /// </summary>
    private readonly List<string> _tokenExtendedDataInvariantGs1UnrecognisedCharacters = [];

    /// <summary>
    ///   A list of the reported characters mapped to invariant characters.
    /// </summary>
    private StringBuilder? _invariantMappedCharacters;

    /// <summary>
    ///   A list of the reported characters mapped to non-invariant characters.
    /// </summary>
    private StringBuilder? _nonInvariantMappedCharacters;

    /// <summary>
    ///   The current barcode data.
    /// </summary>
    private BarcodeData? _barcodeDataSegments;

    /// <summary>
    ///   The expected value of the prefix reported by a barcode scanner during calibration.
    /// </summary>
    private string? _expectedReportedPrefix;

    /// <summary>
    ///   Dictionary of invariant characters used to ensure that there
    ///   are no unreported characters.
    /// </summary>
    private IDictionary<char, char>? _processedInvariantCharacters;

    /// <summary>
    ///   Dictionary of non-invariant characters used to ensure that there
    ///   are no unreported characters.
    /// </summary>
    private IDictionary<char, char>? _processedNonInvariantCharacters = new Dictionary<char, char>();

    /// <summary>
    ///   The stream containing the bitmap image of the current calibration barcode.
    /// </summary>
    private Stream? _tokenBitmapStream;

    /// <summary>
    ///   The SVG content of the image of the current calibration barcode.
    /// </summary>
    private string? _tokenSvg;

    /// <summary>
    ///   A count of the estimated number of barcodes that still need to be scanned during this session.
    /// </summary>
    private int _tokenRemaining;

    /// <summary>
    ///   The size of data matrix required.
    /// </summary>
    private DataMatrix.Size _tokenSize;

    /// <summary>
    ///   Indicates whether the barcode scanner and computer keyboard layouts correspond.
    /// </summary>
    private bool? _tokenKeyboardMatch;

    /// <summary>
    ///   Indicates whether the current calibration session has been abandoned due to a misreported barcode.
    /// </summary>
    private bool? _tokenCalibrationSessionAbandoned;

    /// <summary>
    ///   The calibration data.
    /// </summary>
    private Data? _tokenCalibrationData;

    /// <summary>
    ///   The system capabilities.
    /// </summary>
    private SystemCapabilities? _tokenSystemCapabilities;

    /// <summary>
    ///   The collection of calibration information.
    /// </summary>
    private List<Information>? _tokenInformation;

    /// <summary>
    ///   The collection of calibration warnings.
    /// </summary>
    private List<Information>? _tokenWarnings;

    /// <summary>
    ///   The collection or calibration errors.
    /// </summary>
    private List<Information>? _tokenErrors;

    /// <summary>
    ///   The current barcode data.
    /// </summary>
    private string _tokenDataBarcodeData = string.Empty;

    /// <summary>
    ///   The dead key currently being calibrated. Null indicates baseline calibration.
    /// </summary>
    private string? _tokenDataKey;

    /// <summary>
    ///   The expected character for the current dead key being calibrated.
    /// </summary>
    private char _tokenDataValue;

    /// <summary>
    ///   A count of the estimated number of calibrations that still need to be performed during this session.
    /// </summary>
    private int _tokenDataCalibrationsRemaining;

    /// <summary>
    ///   The index of the current small barcode in a sequence.
    /// </summary>
    private int _tokenDataSmallBarcodeSequenceIndex;

    /// <summary>
    ///   The number of small barcodes for the current calibration data.
    /// </summary>
    private int _tokenDataSmallBarcodeSequenceCount;

    /// <summary>
    ///   The prefix for each small barcode in a sequence.
    /// </summary>
    private string? _tokenDataPrefix;

    /// <summary>
    ///   The detected suffix.
    /// </summary>
    private string? _tokenDataSuffix;

    /// <summary>
    ///   The reported characters for the current calibration barcode.
    /// </summary>
    private string? _tokenDataReportedCharacters;

    /// <summary>
    ///   A dictionary of differences in expected qnd reported characters where the barcode scanner keyboard layout uses dead keys.
    /// </summary>
    private IDictionary<string, string> _tokenExtendedDataScannerDeadKeysMap = new Dictionary<string, string>();

    /// <summary>
    ///   A list of expected characters where the barcode scanner keyboard layout key maps to an unassigned key on the computer keyboard layout.
    /// </summary>
    private IList<string> _tokenExtendedDataScannerUnassignedKeys = [];

    /// <summary>
    ///   A dictionary of candidate ligatures.
    /// </summary>
    private IDictionary<string, char> _tokenExtendedDataLigatureMap = new Dictionary<string, char>();

    /// <summary>
    ///   A dictionary of differences in reported and expected characters where the reported data uses dead keys.
    /// </summary>
    private IDictionary<string, string> _tokenExtendedDataDeadKeysMap = new Dictionary<string, string>();

    /// <summary>
    ///   The reported prefix segment for the current calibration barcode.
    /// </summary>
    private List<string>? _tokenReportedPrefixSegment = [];

    /// <summary>
    ///   The reported suffix characters for the current calibration barcode.
    /// </summary>
    private string _tokenReportedSuffix = string.Empty;

    /// <summary>
    ///   The first (flag) character. By default, this is "]".
    ///   If a dead key is used, the sequence will contain two characters.
    /// </summary>
    private string? _tokenExtendedDataAimFlagCharacterSequence = "]";

    /// <summary>
    ///   A regular expression for matching reported characters.
    /// </summary>
    private string? _tokenExtendedDataReportedCharacters;

    /// <summary>
    ///   Any prefix observed during calibration.
    /// </summary>
    private string? _tokenExtendedDataPrefix;

    /// <summary>
    ///   Any code observed during calibration between the AIM ID (if present) and the data.
    /// </summary>
    private string? _tokenExtendedDataCode;

    /// <summary>
    ///   Any suffix observed during calibration.
    /// </summary>
    private string? _tokenExtendedDataSuffix;

    /// <summary>
    ///   Any prefix observed during calibration.
    /// </summary>
    private string? _tokenExtendedDataReportedPrefix;

    /// <summary>
    ///   Any code observed during calibration between the AIM ID (if present) and the data.
    /// </summary>
    private string? _tokenExtendedDataReportedCode;

    /// <summary>
    ///   Any suffix observed during calibration.
    /// </summary>
    private string? _tokenExtendedDataReportedSuffix;

    /// <summary>
    ///   The Unicode name of the keyboard script.
    /// </summary>
    private string? _tokenExtendedDataKeyboardScript;

    /// <summary>
    ///   The control character that maps tp the Line Feed character.
    /// </summary>
    private string? _tokenExtendedDataLineFeedCharacter;

    /// <summary>
    ///   'Traffic Light' assessment of the performance of the barcode scanner keyboard input.
    /// </summary>
    private ScannerKeyboardPerformance _tokenExtendedDataScannerKeyboardPerformance;

    /// <summary>
    ///   Barcode scanner calculated in characters per second.
    /// </summary>
    private int _tokenExtendedDataScannerCharactersPerSecond;

    /// <summary>
    ///   A dictionary of differences in reported and expected characters.
    /// </summary>
    private IDictionary<char, char> _tokenExtendedDataCharacterMap = new Dictionary<char, char>();

    /// <summary>
    ///   A dictionary of initially detected differences in reported and expected characters where the reported data uses
    ///   dead keys.
    /// </summary>
    private IDictionary<string, char> _tokenExtendedDataDeadKeyCharacterMap = new Dictionary<string, char>();

    /// <summary>
    ///   A dictionary of fix ups required where the reported data uses dead keys, but where, when a dead key is entered followed
    ///   by a space, the reported character is not the dead key character.
    /// </summary>
    private Dictionary<string, string> _tokenExtendedDataDeadKeyFixUp = [];

    /// <summary>
    ///   Indicates whether an ISO/IEC 15434 barcode may be unreadable due to non-representation of ASCII 30 character.
    /// </summary>
    private bool _tokenExtendedDataPotentialIsoIec15434Unreadable30;

    /// <summary>
    ///   Indicates whether EDI data in an ISO/IEC 15434 barcode may be unreadable due to non-representation of ASCII 28 character.
    /// </summary>
    private bool _tokenExtendedDataUnreadableFs;

    /// <summary>
    ///   Indicates whether EDI data in an ISO/IEC 15434 barcode may be unreadable due to non-representation of ASCII 31 character.
    /// </summary>
    private bool _tokenExtendedDataUnreadableUs;

    /// <summary>
    ///   Indicates whether EDI data in an ISO/IEC 15434 barcode may be unreadable due to non-representation of ASCII 04 character.
    /// </summary>
    private bool _tokenExtendedDataEotUnreadable;

    /// <summary>
    /// Any suffix and end-of-line sequence recorded while processing small barcodes.
    /// </summary>
    private (string suffix, string endOfLine) _tokenSmallBarcodeSuffixData = (string.Empty, string.Empty);

    /// <summary>
    /// The last token. This value is recorded to aid recovery from failures.
    /// </summary>
    private Token _lastToken;

    /// <summary>
    ///   A list of characters that are not recognised by the barcode scanner keyboard layout.
    /// </summary>
    private IList<char> _tokenExtendedDataUnrecognisedKeys = [];

    /// <summary>
    ///   Initializes a new instance of the <see cref="Calibrator" /> class.
    /// </summary>
    /// <param name="calibrationData">The calibration data.</param>
    /// <param name="assumption">The assumption made concerning the use of calibration in client systems.</param>
    // ReSharper disable once UnusedMember.Global
    public Calibrator(
        Data? calibrationData = null,
        Assumption assumption = Assumption.Calibration) {
        if (calibrationData is not null) {
            CalibrationData = calibrationData;
        }

        _assumption = assumption;
    }

    /// <summary>
    ///   Gets the type of calibration barcode currently being processed.
    /// </summary>
    public BarcodeType CurrentBarcodeType { get; private set; }

    /// <summary>
    ///   Gets or sets the Calibration configuration data.
    /// </summary>
    public Data? CalibrationData {
        get => _tokenCalibrationData ?? new Data(string.Empty);

        set {
            if (value is null) {
                return;
            }

            _tokenExtendedDataAimFlagCharacterSequence = value.AimFlagCharacterSequence ?? string.Empty;
            _tokenExtendedDataCharacterMap = value.CharacterMap ?? new Dictionary<char, char>();
            _tokenExtendedDataDeadKeysMap = value.DeadKeysMap ?? new Dictionary<string, string>();
            _tokenExtendedDataDeadKeyCharacterMap = value.DeadKeyCharacterMap ?? new Dictionary<string, char>();
            _tokenExtendedDataScannerDeadKeysMap = value.ScannerDeadKeysMap ?? new Dictionary<string, string>();
            _tokenExtendedDataScannerUnassignedKeys = value.ScannerUnassignedKeys ?? [];
            _tokenExtendedDataLigatureMap = value.LigatureMap ?? new Dictionary<string, char>();
            _tokenExtendedDataReportedCharacters = value.ReportedCharacters;
            _tokenExtendedDataReportedPrefix = value.Prefix;
            _tokenExtendedDataReportedCode = value.Code;
            _tokenExtendedDataReportedSuffix = value.Suffix;
            _tokenExtendedDataKeyboardScript = value.KeyboardScript;
            _tokenExtendedDataScannerKeyboardPerformance = value.ScannerKeyboardPerformance;
            _tokenExtendedDataScannerCharactersPerSecond = value.ScannerCharactersPerSecond;
            _tokenExtendedDataLineFeedCharacter = value.LineFeedCharacter;
            _tokenCalibrationData = value;
        }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether to assess Format support.
    /// </summary>
    public bool AssessFormatSupport { get; set; } = true;

    /// <summary>
    ///   Gets a value indicating whether pre-processing of barcode scanner input is required.
    /// </summary>
    public bool IsProcessingRequired => _tokenExtendedDataCharacterMap.Keys.Count > 0
                                        || _tokenExtendedDataDeadKeysMap.Keys.Count > 0
                                        || _tokenExtendedDataLigatureMap.Keys.Count > 0
                                        || !string.IsNullOrEmpty(_tokenExtendedDataReportedPrefix)
                                        || !string.IsNullOrEmpty(_tokenExtendedDataReportedCode)
                                        || !string.IsNullOrEmpty(_tokenExtendedDataReportedSuffix);

    /// <summary>
    /// Gets or sets a collection of recognised data elements.
    /// </summary>
    /// <remarks>
    /// Optionally pass a list of recognised data elements to the Calibrator to constrain the GS1 application identifiers
    /// and/or the ASC MH 10.8.2 data identifiers that the client software needs to recognise when parsing data. This may
    /// extend the range of transformation strategies that the calibrator can identify.
    /// </remarks>
    // ReSharper disable once UnusedMember.Global
    public IList<RecognisedDataElement> RecognisedDataElements { get; set; } = [];

    /// <summary>
    ///   Sets the reported prefix that the barcode scanner is expected to include when barcodes are
    ///   scanned.
    /// </summary>
    /// <remarks>
    /// <p>This method is only intended for use when a barcode scanner used for calibration is
    ///   configured to transmit a prefix and the prefix characters contain two or more
    ///   consecutive spaces. Because space characters are used as delimiters in the calibration
    ///   barcodes, a prefix that contains spaces may result in incorrect data recognition during
    ///   calibration, unless you set the prefix value explicitly using this method. You must
    ///   state the prefix, as reported to the library. This may depend on your current computer
    ///   keyboard layout, and may not be identical to the prefix characters programmed into the
    ///   barcode scanner.</p>
    /// <p>There is no equivalent requirement to state values of suffixes that are reported with
    ///   spaces. The calibrator will correctly handle such suffixes.</p>
    /// </remarks>
    /// <param name="reportedPrefix">The expected value of the reported prefix.</param>
    public void SetReportedPrefix(string reportedPrefix) =>
        _expectedReportedPrefix = reportedPrefix;

    /// <summary>
    ///   Returns the system capabilities for the current calibration.
    /// </summary>
    /// <param name="capsLock">
    ///   Optional. Indicates if the keyboard Caps Lock was on or off when calibration was carried out.
    /// </param>
    /// <returns>The system capabilities for the current calibration.</returns>
    public SystemCapabilities? SystemCapabilities(bool? capsLock = null) {
        if (_tokenSystemCapabilities is not null) {
            _tokenSystemCapabilities.CapsLock = capsLock ?? _tokenSystemCapabilities.CapsLock;
        }

        return _tokenSystemCapabilities;
    }

    /// <summary>
    ///   Get the baseline calibration barcode(s) for the current calibration.
    /// </summary>
    /// <param name="multiplier">The size multiplier.</param>
    /// <param name="size">The size of data matrix required.</param>
    /// <returns>
    ///   A list of baseline calibration barcodes for the current calibration.
    /// </returns>
    /// <remarks>
    ///   If multiple streams are returned, each stream is a barcode containing a segment of the
    ///   calibration data. Multiple streams are returned when smaller barcode sizes are required.
    /// </remarks>
    public IList<Stream> BaselineBarcodes(float multiplier = 1F, DataMatrix.Size size = DataMatrix.Size.Automatic) {
        // Return the initial baseline barcode containing segments of character sequences for invariant characters,
        // non-invariant printable characters and additional ASCII control characters. It is very important that
        // the final four characters are a segment delimiter because the scanner may add characters beyond this
        // such as CR and LF characters and/or a suffix.
        var barcodeDataEx = _baselineBarcodeData
                          + (AssessFormatSupport
                                 ? SegmentDelimiter + _baselineBarcodeDataFormat06 + SegmentDelimiter
                                 : SegmentDelimiter);

        return SegmentStreams(new BarcodeData(barcodeDataEx, size.MaxCapacity()));

        IList<Stream> SegmentStreams(BarcodeData barcodeSegments) {
            var segments = barcodeSegments.Segments.ToList();
            var barcodeSegmentStreams = new List<Stream>(segments.Count);

            barcodeSegmentStreams.AddRange(
                from t in segments
                let barcode = new Barcode {
                    ForegroundColor = Color.Black,
                    BackgroundColor = Color.White,
                    Multiplier = multiplier,
                    ImageFormat = SixLabors.ImageSharp.Formats.Png.PngFormat.Instance,
                }
                select barcode.CreateBarcode(t));

            return barcodeSegmentStreams;
        }
    }

    /// <summary>
    ///   Get the baseline calibration barcode(s) in SVG format for the current calibration.
    /// </summary>
    /// <param name="multiplier">The size multiplier.</param>
    /// <param name="size">The size of data matrix required.</param>
    /// <returns>
    ///   A list of baseline calibration barcodes for the current calibration.
    /// </returns>
    /// <remarks>
    ///   If multiple strings are returned, each string is a barcode containing a segment of the
    ///   calibration data. Multiple strings are returned when smaller barcode sizes are required.
    /// </remarks>
    public IList<string> BaselineBarcodesSvg(float multiplier = 1F, DataMatrix.Size size = DataMatrix.Size.Automatic) {
        // Return the initial baseline barcode containing segments of character sequences for invariant characters,
        // non-invariant printable characters and additional ASCII control characters. It is very important that
        // the final four characters are a segment delimiter because the scanner may add characters beyond this
        // such as CR and LF characters and/or a suffix.
        var barcodeDataEx = _baselineBarcodeData
                          + (AssessFormatSupport
                                 ? SegmentDelimiter + _baselineBarcodeDataFormat06 + SegmentDelimiter
                                 : SegmentDelimiter);

        return SegmentStrings(new BarcodeData(barcodeDataEx, size.MaxCapacity()));

        IList<string> SegmentStrings(BarcodeData barcodeSegments) {
            var segments = barcodeSegments.Segments.ToList();
            var barcodeSegmentStrings = new List<string>(segments.Count);

            barcodeSegmentStrings.AddRange(
                from t in segments
                let barcode = new Barcode {
                    ForegroundColor = Color.Black,
                    BackgroundColor = Color.White,
                    Multiplier = multiplier,
                }
                select barcode.CreateBarcodeSvg(t));

            return barcodeSegmentStrings;
        }
    }

    /// <summary>
    ///   Get the data for each baseline calibration barcode for the current calibration.
    /// </summary>
    /// <param name="size">The size of data matrix required.</param>
    /// <returns>
    ///   A list of data entries for baseline calibration barcodes for the current calibration.
    /// </returns>
    /// <remarks>
    ///   <p>If multiple strings are returned, each string is the data for a barcode containing a segment of
    ///   the calibration data. Multiple strings are returned when smaller barcode sizes are required.</p>
    ///   <p>This method is useful when the client code creates barcodes directly itself, rather than
    ///   depending on the Calibration library.</p>
    /// </remarks>
    // ReSharper disable once UnusedMember.Global
    public IList<string> BaselineBarcodeData(DataMatrix.Size size = DataMatrix.Size.Automatic) {
        // Return the initial baseline barcode containing segments of character sequences for invariant characters,
        // non-invariant printable characters and additional ASCII control characters.
        var barcodeDataEx = _baselineBarcodeData
                          + (AssessFormatSupport
                                 ? SegmentDelimiter + _baselineBarcodeDataFormat06 + SegmentDelimiter
                                 : SegmentDelimiter);

        return SegmentStreams(new BarcodeData(barcodeDataEx, size.MaxCapacity()));

        static IList<string> SegmentStreams(BarcodeData barcodeSegments) {
            return barcodeSegments.Segments.ToList();
        }
    }

    /// <summary>
    ///   Calibrates for a given combination of barcode scanner and OS keyboard layouts.
    /// </summary>
    /// <param name="data">The reported input after scanning the calibration barcode.</param>
    /// <param name="token">The current calibration token. NB. use CalibrationTokens() to get the token collection enumerator.</param>
    /// <param name="capsLock">Indicates if Caps Lock is switched on.</param>
    /// <param name="platform">The platform on which the system resides.</param>
    /// <param name="dataEntryTimeSpan">The time span specifying how long it took from the start of the scan to submitting the data.</param>
    /// <param name="preProcessors">The pre-processor functions, provided as a delegate.</param>
    /// <param name="assessScript">Indicates whether the calibrator should assess the script for the configured OS keyboard.</param>
    /// <param name="trace">Indicates whether the calibrator should trace the data it receives. This supports debugging.</param>
    /// <returns>The updated calibration token.</returns>
    // ReSharper disable once UnusedMember.Global
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
    public Token Calibrate(
        int[] data,
        Token token,
        bool? capsLock = null,
        SupportedPlatform platform = SupportedPlatform.Windows,
        TimeSpan dataEntryTimeSpan = default,
        Preprocessor? preProcessors = null,
        bool assessScript = true,
        bool trace = false) {
        ArgumentNullException.ThrowIfNull(data);

        Span<char> charOut = stackalloc char[data.Length];

        for (var idx = 0; idx < data.Length; idx++) {
            charOut[idx] = (char)data[idx];
        }

        return Calibrate(new string(charOut), token, capsLock, platform, dataEntryTimeSpan, preProcessors, assessScript, trace);
    }

    /// <summary>
    ///   Calibrates for a given combination of barcode scanner and OS keyboard layouts.
    /// </summary>
    /// <param name="data">The reported input after scanning the calibration barcode.</param>
    /// <param name="token">The current calibration token. NB. use CalibrationTokens() to get the token collection enumerator.</param>
    /// <param name="capsLock">Indicates if Caps Lock is switched on.</param>
    /// <param name="platform">The platform on which the system resides.</param>
    /// <param name="dataEntryTimeSpan">The time span specifying how long it took from the start of the scan to submitting the data.</param>
    /// <param name="preProcessors">The pre-processor functions, provided as a delegate.</param>
    /// <param name="assessScript">Indicates whether the calibrator should assess the script for the configured OS keyboard.</param>
    /// <param name="trace">Indicates whether the calibrator should trace the data it receives. This supports debugging.</param>
    /// <returns>The updated calibration token.</returns>
    public Token Calibrate(
        string? data,
        Token token,
        bool? capsLock = null,
        SupportedPlatform platform = SupportedPlatform.Windows,
        TimeSpan dataEntryTimeSpan = default,
        Preprocessor? preProcessors = null,
        bool assessScript = true,
        bool trace = false)
    {
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
        var tempDataForTrace = string.Empty;

        if (trace) {
            var basicCapsLockValue = capsLock ?? false ? "true" : "false";
            var capsLockValue = capsLock == null ? "unknown" : basicCapsLockValue;
            var timeSpanMs = dataEntryTimeSpan == default ? "not specified" : dataEntryTimeSpan.TotalMilliseconds.ToString("0.00");
            var additionalData = (string.IsNullOrEmpty(data) ? string.Empty : "\r\n") + $"Caps Lock: {capsLockValue}\r\nSupported Platform: {platform}\r\nData Entry Interval: {timeSpanMs}ms";
            tempDataForTrace = data;

            try {
                Console.WriteLine(data?.ToControlPictures() + additionalData);
            }
            catch {
                // Do nothing here
            }

            try {
                Trace.TraceInformation(data?.ToControlPictures() + additionalData);
            }
            catch {
                // Do nothing here
            }
        }

        if (token.ToJson().Replace(" ", string.Empty) == "{}") {
            _lastToken = token;
            return LogCalibrationInformation(token, InformationType.NoCalibrationTokenProvided);
        }

        DoInitializeFromTokenData(token);

        var preprocessorExceptions = new List<PreprocessorException>();

        // Add the calibration processor to the list of pre-processors
        data = preProcessors is not null

            // Aggregate the results of each pre-processor.
            ? preProcessors.GetInvocationList().Aggregate(
                data,
                (current, preProcessor) => {
                    var processedData = ((Preprocessor)preProcessor).Invoke(current, out var exceptions)?.ToString() ??
                                        string.Empty;
                    preprocessorExceptions.AddRange(exceptions ?? []);
                    return processedData;
                })
            : data;

        if (preprocessorExceptions is { Count: > 0 }) {
            // Add list of preprocessor exceptions to token.
            foreach (var barcodeException in preprocessorExceptions) {
#pragma warning disable SA1118 // Parameter should not span multiple lines
                token.AddInformation(
                    barcodeException.IsFatal
                        ? InformationLevel.Error
                        : InformationLevel.Warning,
                    barcodeException.IsFatal
                        ? InformationType.PreProcessorError
                        : InformationType.PreProcessorWarning,
                    barcodeException.Message);
#pragma warning restore SA1118 // Parameter should not span multiple lines
            }

            _lastToken = token;
            return token;
        }

        if (data is null) {
            _lastToken = token;
            return token;
        }

        if (trace
            && !string.IsNullOrEmpty(tempDataForTrace)
            && !string.IsNullOrEmpty(data)
            && tempDataForTrace != data) {
            try {
                Console.WriteLine(data.ToControlPictures());
            }
            catch {
                // Do nothing here
            }

            try {
                Trace.TraceInformation(data.ToControlPictures());
            }
            catch {
                // Do nothing here
            }

            // ReSharper disable once RedundantAssignment
#pragma warning disable S1854 // Unused assignments should be removed
            tempDataForTrace = string.Empty;
#pragma warning restore S1854 // Unused assignments should be removed
        }

        // Set the current calibration barcode type.
        CurrentBarcodeType = token.Data?.Key?.Length == 0
            ? BarcodeType.Baseline
            : BarcodeType.DeadKey;

        (Token token, string suffix, string endOfLine) extendedToken = (token, string.Empty, string.Empty);

        try {
            // Perform calibration
            extendedToken = token.Data?.Key?.Length == 0
                            ? CalibrateBaseLine(data, token, capsLock, platform, dataEntryTimeSpan, assessScript)
                            : CalibrateDeadKey(data, token, dataEntryTimeSpan, _tokenSmallBarcodeSuffixData.suffix, _tokenSmallBarcodeSuffixData.endOfLine);
        }
        catch (Exception ex) {
            LogCalibrationInformation(token, InformationType.CalibrationFailedUnexpectedly);
            Console.WriteLine(ex.Message);
        }

        _tokenSmallBarcodeSuffixData = (extendedToken.suffix, extendedToken.endOfLine);
        var @out = extendedToken.token;

        _tokenDataKey = @out.Data?.Key;
        _tokenDataValue = @out.Data?.Value ?? char.MinValue;
        _tokenBitmapStream = @out.BitmapStream;
        _tokenSvg = @out.Svg;
        _tokenRemaining = @out.Remaining < 0 ? 0 : @out.Remaining;
        _tokenSize = @out.Size;
        _tokenDataSmallBarcodeSequenceIndex = @out.Data?.SmallBarcodeSequenceIndex ?? 0;
        _tokenDataSmallBarcodeSequenceCount = @out.Data?.SmallBarcodeSequenceCount ?? 0;
        _tokenDataPrefix = @out.Data?.Prefix is not null ? _tokenDataPrefix : string.Empty;
        _tokenDataSuffix = @out.Data?.Suffix is not null ? _tokenDataSuffix : string.Empty;

        _tokenInformation?.Clear();
        _tokenWarnings?.Clear();
        _tokenErrors?.Clear();

        @out.Information.ToList().ForEach(information => _tokenInformation?.Add(information));
        @out.Warnings.ToList().ForEach(warning => _tokenWarnings?.Add(warning));
        @out.Errors.ToList().ForEach(error => _tokenErrors?.Add(error));

        // Order the collections
        _tokenInformation = _tokenInformation?.OrderBy(ci => (int)ci.InformationType).ToList();
        _tokenWarnings = _tokenWarnings?.OrderBy(ci => (int)ci.InformationType).ToList();
        _tokenErrors = _tokenErrors?.OrderByDescending(ci => (int)ci.InformationType).ToList();

        // Process AIM candidates, prefixes and suffixes against a final version of the character maps.
        if (_tokenRemaining == 0) {
            foreach (var kvp in _tokenExtendedDataDeadKeyFixUp) {
                if (!_tokenExtendedDataDeadKeyCharacterMap.TryGetValue(kvp.Key, out var deadKeyValue)) continue;
                _tokenExtendedDataDeadKeyCharacterMap.Add(kvp.Value, deadKeyValue);
                _tokenExtendedDataDeadKeyCharacterMap.Remove(kvp.Key);
            }

            @out = DoProcessSuffix(DoProcessAimCandidate(@out), @out.ReportedSuffix ?? string.Empty);
        }

        // Resolve cases where the AIM flag character is reported as an ASCII 0 (mapping to an unassigned key). In this case, the
        // AIM flag character is represented as an empty string. If there are no dead keys and if ASCII 0 is not required for
        // representation of ASCII 29 or ASCII 30, and if the only ambiguities (if any) concern non-invariant characters, then
        // we will map ASCII 0 to ]. In addition, we will remove the AimNotRecognised warning.
        if (_tokenExtendedDataAimFlagCharacterSequence is { Length: 0 } or "\0" &&
            @out.Warnings.Any(w => w.InformationType == InformationType.AimNotRecognised) &&
            !_tokenExtendedDataDeadKeysMap.Any() &&
            !_tokenExtendedDataCharacterMap.Any(kvp => kvp.Key == '\u0000' && kvp.Value.IsInvariant()) &&
            !_tokenExtendedDataCharacterMap.Any(kvp => kvp is { Key: '\u0000', Value: '\u001d' }) &&
            !_tokenExtendedDataCharacterMap.Any(kvp => kvp is { Key: '\u0000', Value: '\u001e' })) {
            foreach (var aimNotRecognised in @out.Warnings.Where(
                         w => w.InformationType == InformationType.AimNotRecognised).ToList()) {
                @out.RemoveInformation(aimNotRecognised);
                _tokenWarnings?.RemoveAll(ci => ci == aimNotRecognised);
            }

            // If the reported AIM identifier starts with a '\0', and there is no prefix, it will have been
            // reported incorrectly as a prefix, so remove this entry.
            _tokenExtendedDataReportedPrefix = _tokenExtendedDataReportedPrefix is ['\0', _, _]
                ? string.Empty
                : _tokenExtendedDataReportedPrefix;

            if (_tokenExtendedDataAimFlagCharacterSequence is "\0" &&
                _tokenExtendedDataCharacterMap.TryGetValue('\u0000', out var existingMap)) {
                // Null may already been mapped to another character.  We favour AIM flag sequences over
                // control characters used in EDI data or any additional character mapped to null
                var addMap = existingMap switch {
                    '\u001C' or '\u001F' or '\u0004' when AssessFormatSupport => ReplaceExistingMapForEdiControlCharacter(),
                    '#' or '$' or '@' or '[' or '\\' or '^' or '`' or '{' or '|' or '}' or '~' => ReplaceExistingMapForAdditionalCharacter(),
                    ']' => char.MinValue,
                    _ => DoNotReplaceExistingMap()
                };

                if (addMap is not char.MinValue) {
                    _tokenExtendedDataCharacterMap.Add('\u0000', addMap);
                }

                char ReplaceExistingMapForEdiControlCharacter() {
                    _tokenExtendedDataCharacterMap.Remove('\u0000');
                    _tokenWarnings?.RemoveAll(
                        ci => ci.InformationType == (existingMap == '\u001C'
                            ? InformationType.FileSeparatorSupported
                            : InformationType.UnitSeparatorSupported));

                    LogCalibrationInformation(
                        @out,
                        InformationType.IsoIec15434EdiNotReliablyReadable,
                        char.MinValue.ToControlPictureString(),
                        existingMap.ToControlPictureString());

                    LogCalibrationInformation(
                        @out,
                        InformationType.FileSeparatorMappingIsoIec15434EdiNotReliablyReadable,
                        char.MinValue.ToControlPictureString(),
                        $"] {existingMap.ToControlPictureString()}");

                    LogCalibrationInformation(
                        @out,
                        existingMap switch {
                            '\u001C' => InformationType.NonCorrespondingKeyboardLayoutsFileSeparator,
                            '\u001F' => InformationType.NonCorrespondingKeyboardLayoutsUnitSeparator,
                            '\u0004' => InformationType.NonCorrespondingKeyboardLayoutsEotCharacter,
                            _ => throw new ArgumentOutOfRangeException(nameof(existingMap))
                        });

                    return ']';
                }

                char ReplaceExistingMapForAdditionalCharacter() {
                    _tokenExtendedDataCharacterMap.Remove('\u0000');

                    LogCalibrationInformation(
                        @out,
                        InformationType.SomeNonInvariantCharactersUnrecognised,
                        char.MinValue.ToControlPictureString(),
                        existingMap.ToControlPictureString());

                    LogCalibrationInformation(
                        @out,
                        InformationType.NonCorrespondingKeyboardLayoutsForNonInvariantCharacters);

                    return ']';
                }

                char DoNotReplaceExistingMap() {
                    return char.MinValue;
                }
            }
        }

        // Only set the calibration data if there are no errors to report.
        if (_tokenRemaining == 0 && !@out.Errors.Any()) {
            _tokenCalibrationData = new Data(
                _tokenExtendedDataCharacterMap,
                _tokenExtendedDataDeadKeysMap,
                _tokenExtendedDataDeadKeyCharacterMap,
                _tokenExtendedDataLigatureMap,
                _tokenExtendedDataScannerDeadKeysMap,
                _tokenExtendedDataScannerUnassignedKeys,
                _tokenExtendedDataReportedCharacters,
                _tokenExtendedDataAimFlagCharacterSequence,
                _tokenExtendedDataReportedPrefix,
                _tokenExtendedDataReportedCode,
                _tokenExtendedDataReportedSuffix,
                _tokenExtendedDataKeyboardScript,
                _tokenExtendedDataScannerKeyboardPerformance,
                _tokenExtendedDataScannerCharactersPerSecond,
                _tokenExtendedDataLineFeedCharacter);
        }

        // Process the calibration results to determine the system capabilities.
        _tokenSystemCapabilities = new SystemCapabilities(
            @out,
            _assumption,
            capsLock,
            _tokenExtendedDataScannerKeyboardPerformance,
            _tokenExtendedDataScannerCharactersPerSecond,
            AssessFormatSupport,
            _tokenExtendedDataDeadKeyCharacterMap.Count > 0,
            _tokenExtendedDataCharacterMap,
            _tokenExtendedDataDeadKeyCharacterMap,
            _tokenExtendedDataDeadKeysMap,
            _tokenExtendedDataInvariantGs1Ambiguities,
            _tokenExtendedDataNonInvariantAmbiguities,
            _tokenExtendedDataInvariantGs1UnrecognisedCharacters,
            _tokenExtendedDataNonInvariantUnrecognisedCharacters,
            _tokenExtendedDataLigatureMap);

        @out = InitializeTokenData();

        // Fix up the Caps Lock warnings by removing CapsLockProbablyOn if CapsLockOn is set.
        // This has to be done after creating the system capabilities.
        if (@out.Warnings.Any(i => i.InformationType == InformationType.CapsLockOn) &&
            @out.Warnings.Any(i => i.InformationType == InformationType.CapsLockProbablyOn)) {
            @out.RemoveInformation(@out.Warnings.FirstOrDefault(w => w.InformationType == InformationType.CapsLockProbablyOn));
        }

        // If the barcode scanner keyboard performance is less than optimal, post additional warnings or error to the log.
        switch (_tokenExtendedDataScannerKeyboardPerformance) {
            case ScannerKeyboardPerformance.Medium:
            case ScannerKeyboardPerformance.Low:
                // Warning - The best reported keyboard data entry time during calibration indicates that the barcode scanner does not perform optimally.
                @out = LogCalibrationInformation(@out, InformationType.SubOptimalScannerKeyboardPerformance);
                break;
            case ScannerKeyboardPerformance.High:
            default:
                break;
        }

        _lastToken = token;

        if (_tokenRemaining != 0 && !@out.Errors.Any()) {
            @out = InitializeTokenData();
            _lastToken = token;
            return @out;
        }

        var failedCalibrationErrors = new List<InformationType>
        {
            InformationType.PartialCalibrationDataReported,
            InformationType.IncorrectCalibrationDataReported,
            InformationType.UnrecognisedData,
            InformationType.NoCalibrationDataReported,
        };

        var abandonCalibrationSession =
            _tokenErrors?.Exists(e => failedCalibrationErrors.Contains(e.InformationType)) ?? false;

        return abandonCalibrationSession ? @out.AbandonCalibrationSession() : @out;
    }

    /// <summary>
    ///   A collection of calibration tokens.
    /// </summary>
    /// <param name="multiplier">The size multiplier.</param>
    /// <param name="size">The size of any data matrix generated by other means.</param>
    /// <param name="backgroundColour">Background colour (hex - use #nnnnnn format).</param>
    /// <param name="foregroundColour">Foreground colour (hex - use #nnnnnn format).</param>
    /// <param name="generateImages">Indicates whether the library should generate barcode images.</param>
    /// <param name="generateSvg">Indicates whether the library should generate SVG content.</param>
    /// <returns>The collection of calibration tokens.</returns>
    // ReSharper disable once UnusedMember.Global
    public IEnumerable<Token> CalibrationTokens(
        float multiplier = 1F,
        DataMatrix.Size size = DataMatrix.Size.Automatic,
        string backgroundColour = "#FFFFFF",
        string foregroundColour = "#000000",
        bool generateImages = true,
        bool generateSvg = true) {
        return GetCalibrationTokens(
            generateImages,
            generateSvg,
            multiplier,
            size,
            backgroundColour,
            foregroundColour);
    }

    /// <summary>
    ///   Get a dictionary of supplementary calibration barcodes for the current calibration.
    /// </summary>
    /// <param name="multiplier">The size multiplier.</param>
    /// <param name="size">The size of data matrix required.</param>
    /// <returns>
    ///   A dictionary of calibration barcodes for the current calibration.
    /// </returns>
    /// <remarks>
    ///   The values represent additional barcodes used to calibrate for dead keys on the computer
    ///   keyboard. The dictionary key for these additional barcodes is the dead key character.
    ///   Each value in the dictionary is a list of one or more streams. If multiple streams are
    ///   returned, each stream is a barcode containing a segment of the calibration data.
    ///   Multiple streams are returned when smaller barcode sizes are required. The full
    ///   list of barcode streams is only available once the baseline barcode has been scanned and
    ///   the data has been processed.
    /// </remarks>
    public IDictionary<char, IList<Stream>> SupplementalBarcodes(
        float multiplier = 1F,
        DataMatrix.Size size = DataMatrix.Size.Automatic) {
        // Return the supplemental calibration barcodes containing character combinations for relevant dead keys.
        return (
           from k in _tokenExtendedDataDeadKeyCharacterMap.Keys
           select k[1])
           .ToDictionary(
                value => value,
                value => SegmentStreams(
                    new BarcodeData(CreateDeadKeyCalibration(value), size.MaxCapacity())));

        IList<Stream> SegmentStreams(BarcodeData barcodeSegments) {
            var segments = barcodeSegments.Segments.ToList();
            var barcodeSegmentStreams = new List<Stream>(segments.Count);

            barcodeSegmentStreams.AddRange(
                from s in segments
                let barcode = new Barcode {
                    ForegroundColor = Color.Black,
                    BackgroundColor = Color.White,
                    Multiplier = multiplier,
                    ImageFormat = SixLabors.ImageSharp.Formats.Png.PngFormat.Instance,
                }
                select barcode.CreateBarcode(s));

            return barcodeSegmentStreams;
        }
    }

    /// <summary>
    ///   Get a dictionary of supplementary calibration barcodes for the current calibration.
    /// </summary>
    /// <param name="multiplier">The size multiplier.</param>
    /// <param name="size">The size of data matrix required.</param>
    /// <returns>
    ///   A dictionary of calibration barcodes for the current calibration.
    /// </returns>
    /// <remarks>
    ///   The values represent additional barcodes used to calibrate for dead keys on the computer
    ///   keyboard. The dictionary key for these additional barcodes is the dead key character.
    ///   Each value in the dictionary is a list of one or more streams. If multiple streams are
    ///   returned, each stream is a barcode containing a segment of the calibration data.
    ///   Multiple streams are returned when smaller barcode sizes are required. The full
    ///   list of barcode streams is only available once the baseline barcode has been scanned and
    ///   the data has been processed.
    /// </remarks>
    public IDictionary<char, IList<string>> SupplementalBarcodesSvg(
        float multiplier = 1F,
        DataMatrix.Size size = DataMatrix.Size.Automatic) {
        // Return the supplemental calibration barcodes containing character combinations for relevant dead keys.
        return (
           from k in _tokenExtendedDataDeadKeyCharacterMap.Keys
           select k[1])
           .ToDictionary(
                value => value,
                value => SegmentStreamsSvg(
                    new BarcodeData(CreateDeadKeyCalibration(value), size.MaxCapacity())));

        IList<string> SegmentStreamsSvg(BarcodeData barcodeSegments) {
            var segments = barcodeSegments.Segments.ToList();
            var barcodeSegmentStreams = new List<string>(segments.Count);

            barcodeSegmentStreams.AddRange(
                from s in segments
                let barcode = new Barcode {
                    ForegroundColor = Color.Black,
                    BackgroundColor = Color.White,
                    Multiplier = multiplier,
                }
                select barcode.CreateBarcodeSvg(s));

            return barcodeSegmentStreams;
        }
    }

    /// <summary>
    ///   Get a dictionary of data for supplementary calibration barcodes for the current calibration.
    /// </summary>
    /// <param name="size">The size of data matrix required.</param>
    /// <returns>
    ///   A dictionary of calibration barcodes for the current calibration.
    /// </returns>
    /// <remarks>
    ///   <p>The data values represent additional barcodes used to calibrate for dead keys on the computer
    ///   keyboard. The dictionary key for these additional barcodes is the dead key character. Each
    ///   value in the dictionary is a list of one or more strings. If multiple strings are returned,
    ///   each string is the data for a barcode containing a segment of the calibration data. Multiple
    ///   strings are returned when smaller barcode sizes are required. The full list of barcode
    ///   strings is only available once the baseline barcodes have been scanned and the data has been
    ///   processed.</p>
    ///   <p>This method is useful when the client code creates barcodes directly itself, rather than
    ///   depending on the Calibration library.</p>
    /// </remarks>
    // ReSharper disable once UnusedMember.Global
    public IDictionary<char, IList<string>> SupplementalBarcodeData(
        DataMatrix.Size size = DataMatrix.Size.Automatic) {
        // Return the supplemental calibration barcodes containing character combinations for relevant dead keys.
        return (
           from k in _tokenExtendedDataDeadKeyCharacterMap.Keys
           select k[1])
           .ToDictionary(
                value => value,
                value => SegmentStreams(
                    new BarcodeData(CreateDeadKeyCalibration(value), size.MaxCapacity())));

        static IList<string> SegmentStreams(BarcodeData barcodeSegments) {
            return barcodeSegments.Segments.ToList();
        }
    }

    /// <summary>
    ///   Processes input, normalizing it according the calibration character map, converting into
    ///   the correct sequence of characters.
    /// </summary>
    /// <param name="input">
    ///   The input to be processed. This is data transmitted by the barcode scanner, as reported to
    ///   the application.
    /// </param>
    /// <param name="exceptions">Collection of exceptions.</param>
    /// <returns>The normalized input, processed according to the calibration character map.</returns>
    public string ProcessInput(string? input, out IList<PreprocessorException>? exceptions) {
        // We need to convert the AIM identifier if it exists and there is a conversion.
        var aimId = PreProcessAimIdentifier(input);
        var preprocessorExceptions = new List<PreprocessorException>();

        var builder = new StringBuilder();

        /* Format 05 and 06 barcodes end with an ASCII 04 EOT character. This can cause issues.
         * If the last character in a barcode data element is reported as a dead key, and the barcode scanner does
         * not output the ASCII 29 character to delimit the data element, the dead key event may precede
         * ASCII 0s and an ASCII 04 without modifying any key. Only when the transmitted data is submitted,
         * typically by a carriage return, does the literal dead key character get added to the input,
         * appearing after the ASCII 04 End-of-Transmission! There is no very good solution to this problem,
         * but as a best-endeavours approach we will look for a string of ASCII 0s, an ASCII 04 and a final
         * character at the end of the input, and adjust it to what it should probably be.
         * */

#if NET7_0_OR_GREATER
        input = DeadKeyAsciiControlCharacterSequenceRegex().Replace(input ?? string.Empty, "\u0000$2$1");
#else
        input = DeadKeyAsciiControlCharacterSequenceRegex.Replace(input ?? string.Empty, "\u0000$2$1");
#endif

        input = FixDeadKeyAsciiControlCharacterSequence(input);

        /* If we observed a prefix during calibration, this should be removed if it is found at or near the beginning
         * of the input. Because we remove prefixes and suffixes in this preprocessor method, we do not have any
         * mechanism to report the values back to the parser. This complies with a deliberate policy not to report
         * these values. We cannot reliably parse the values of prefixes or suffixes generated by a barcode scanner,
         * as the barcode scanner is not necessarily bound to use invariant or other ASCII characters. Even if it
         * uses ASCII, any non-invariant character would not necessarily be reliably reportable. So, it is better to
         * avoid the risk of reporting incorrect prefix or suffix values and to push the responsibility of recognising
         * and processing prefix and suffix values back onto the calling application.
         * */
        if (!string.IsNullOrEmpty(_tokenExtendedDataReportedPrefix)) {
            // We observed a prefix during calibration.
            var prefixStartIdx = input.IndexOf(_tokenExtendedDataReportedPrefix, StringComparison.Ordinal);

            input = prefixStartIdx >= 0 && prefixStartIdx - 2 <= _tokenExtendedDataAimFlagCharacterSequence?.Length

                // Remove the prefix
                ? input[..prefixStartIdx] + input[(prefixStartIdx + _tokenExtendedDataReportedPrefix.Length)..]
                : input;
        }

        /* If we observed a suffix during calibration, this should be removed if it is found at the end of the
         * input. Note that suffixes could conflict with Format 05 and Format 06. In these cases, the expanded
         * barcode data is terminated with an ASCII 04. If the barcode scanner adds the suffix after the ASCII 04, it
         * is placed beyond the end of transmission marker. Scanners may exhibit different behaviour, so we will
         * cater for different possibilities. NB. This is not 100% reliable.  For example, it is possible that the EOT
         * character is reported using a different character to ASCII 04. If the suffix is placed after the EOT,
         * it will be removed, but if before, it won't be and calibration will probably fail. We are forced to
         * adopt a best-endeavours approach.
         * */
        if (!string.IsNullOrEmpty(_tokenExtendedDataReportedSuffix)) {
            // This handles the very unlikely case of a scanner placing the suffix inside the ISO 15434
            // envelope before the final EOT character (ASCII 04). Such behaviour would be quite
            // incorrect, and leads to unreliable scanning. This is reflected here by the fact that the
            // code can only check the final characters to see if they match the suffix. The characters
            // could be part of the last data element, or the suffix emitted by the scanner could include
            // additional leading characters. Neither of these scenarios would be correctly handled.
            string TestSuffixBeforeEoT() =>
                input.EndsWith('\u001e' + _tokenExtendedDataReportedSuffix + '\u0004', StringComparison.Ordinal)
                    ? input[..^(_tokenExtendedDataReportedSuffix.Length + 1)] + '\u0004'
                    : TestForInvalidSuffix();

            var indexOfEoT = input.LastIndexOf('\u0004');

            // This gets any invalid suffix appended after the EOT (ASCII 04) character for data reported
            // by scanning a Format 05 or Format 06 barcode.
            string InvalidSuffixForF0506() =>
                indexOfEoT >= 0
                    ? input[(indexOfEoT + 1)..]
                    : string.Empty;

            var indexOfRs = input.LastIndexOf('\u001e');

            // This gets any invalid suffix appended before the EOT (ASCII 04) character for data reported
            // by scanning a Format 05 or Format 06 barcode. This is a highly unlikely scenario. Such
            // behaviour by the barcode scanner would be quite incorrect.
            string InvalidSuffixForF050BeforeEoT() =>
                indexOfRs >= 0 && indexOfEoT == input.Length - 1
                    ? input[(indexOfRs + 1)..indexOfEoT]
                    : string.Empty;

            var invalidSuffix = InvalidSuffixForF0506();
            if (string.IsNullOrEmpty(invalidSuffix)) invalidSuffix = InvalidSuffixForF050BeforeEoT();

            // Append a barcode exception for any unambiguously determined invalid suffix.
            string AppendInvalidSuffixException() {
                preprocessorExceptions.Add(new PreprocessorException(
                    1100,
                    string.Format(
                        Thread.CurrentThread.CurrentUICulture,
                        Resources.Barcodes_Error_100,
                        invalidSuffix,
                        _tokenExtendedDataReportedSuffix), false));

                return input[..^invalidSuffix.Length];
            }

            string TestForInvalidSuffix() =>
                string.IsNullOrEmpty(invalidSuffix)
                    ? TestSuffixForFnc()
                    : AppendInvalidSuffixException();

            // This handles the most common scenario of the suffix being appended to data reported by
            // scanning a GS1 FNC barcode. This is not reliable. The characters could be part of the last
            // data element (application), or the suffix emitted by the scanner could include additional
            // leading characters.  Neither of these scenarios would be correctly handled.
            string TestSuffixForFnc() =>
                input.EndsWith(_tokenExtendedDataReportedSuffix, StringComparison.Ordinal)
                    ? input[..^_tokenExtendedDataReportedSuffix.Length]
                    : input;

            // Test to see if the suffix is appended to data reported by scanning a Format 05 or Format 06
            // barcode. These are the only scenarios where suffixes can be reliably detected.
            input = input.EndsWith('\u0004' + _tokenExtendedDataReportedSuffix, StringComparison.Ordinal)
                        ? input[..^_tokenExtendedDataReportedSuffix.Length]
                        : TestSuffixBeforeEoT();
        }

#pragma warning disable S127 // "for" loop stop conditions should be invariant
        for (var idx = 0; idx < input.Length; idx++) {
            if (idx < _tokenExtendedDataAimFlagCharacterSequence?.Length + 2 && !string.IsNullOrEmpty(aimId)) {
                if (idx < 3) {
                    builder.Append(aimId[idx]);
                    continue;
                }

                idx++;
            }

            var reportedChar = input[idx];
            var key = input.Length > idx + 1 ? reportedChar.ToInvariantString() + input[idx + 1] : string.Empty;

            if (reportedChar == '\0' &&
                _tokenExtendedDataDeadKeysMap.Keys.Count > 0 &&
                input.Length > idx + 1 &&
                _tokenExtendedDataDeadKeysMap.TryGetValue(key, out var extendedDataDeadKeyValue)) {
                builder.Append(extendedDataDeadKeyValue);
                idx++;
#pragma warning restore S127 // "for" loop stop conditions should be invariant
                continue;
            }

            /* Some barcode scanner configurations represent dead keys as a character followed by a space.
             * This is detected during calibration, and additional entries in this format are
             * added to the dead key map. This code assumes that spaces never occur in a barcode. Of course,
             * they could if the barcode data is incorrect. Take, for example, serial numbers created
             * randomly where there is, at the very worst, a 1 in 10,000 chance of a falsified serial number
             * being the same. In fact, the chance will almost certainly be far lower than this. Consider,
             * for example, that the manufacturer will probably generate serial numbers of a fixed width.
             * The likelihood that a falsified serial number, which contains spaces and that is then
             * resolved by this code, will be the same length is low.
             * */
#pragma warning disable S127 // "for" loop stop conditions should be invariant
            if (_tokenExtendedDataScannerDeadKeysMap.TryGetValue($"{reportedChar}", out var extendedDataScannerDeadKeyValue) &&
                input.Length > idx + 1 &&
                input[idx + 1] == '\u0020') {
                builder.Append(extendedDataScannerDeadKeyValue);
                idx++;
                continue;
            }

            // If the ligature map contains a key that starts with the reported character, then
            // test to see if the next character(s) indicate a ligature.
            var lookAheadIndex = idx;
            var reportedCharStringBuilder = new StringBuilder(reportedChar.ToString(CultureInfo.InvariantCulture));

            // Perform a look ahead against the ligature map keys.
            while (_tokenExtendedDataLigatureMap.Any(
                       kvp => kvp.Key.StartsWith(reportedCharStringBuilder.ToString(), StringComparison.Ordinal))) {
                reportedCharStringBuilder.Append(input[++lookAheadIndex]);
            }

            var reportedCharString = reportedCharStringBuilder.ToString();

            // If an entry was found in the ligature map, then append the mapped value and continue
            if (_tokenExtendedDataLigatureMap.TryGetValue(reportedCharString[..^1], out var c)) {
                idx = lookAheadIndex - 1;
                builder.Append(c);
                continue;
            }
#pragma warning restore S127 // "for" loop stop conditions should be invariant

            /* If the barcode scanner and OS layouts do not match, it is not possible to determine what
             * character actually exists in the barcode unless the character is in the calibration
             * barcode. It would not be possible to construct a barcode to hold all possible
             * characters. This is a fundamental technical limitation when using barcode scanners that
             * emulate keyboards. In this case, we pass the Unicode white square character for any
             * character that is not recognised.
             * */

            builder.Append(
                _tokenExtendedDataCharacterMap.TryGetValue(reportedChar, out var value)
                    ? value.ToInvariantString()
                    : TestForReportedCharacters());
            continue;

            string TestForCrLfCharacters() =>
                reportedChar == '\r' || reportedChar == '\n'
                    ? reportedChar.ToInvariantString()
                    : "\u25a1";

            string TestForSpaceCharacter() =>
                reportedChar == ' '
                    ? " "
                    : TestForCrLfCharacters();

            string TestIfNoMappingRequired() =>
                _tokenExtendedDataCharacterMap.Count == 0
                    ? reportedChar.ToInvariantString()
                    : TestForSpaceCharacter();

            string TestIfReportedCharacterIsRecognised() =>
                _tokenExtendedDataReportedCharacters!.Contains(reportedChar.ToInvariantString(), StringComparison.Ordinal)
                    ? reportedChar.ToInvariantString()
                    : TestIfNoMappingRequired();

            string TestForReportedCharacters() =>
                string.IsNullOrEmpty(_tokenExtendedDataReportedCharacters)
                    ? reportedChar.ToInvariantString()
                    : TestIfReportedCharacterIsRecognised();
        }

        exceptions = preprocessorExceptions;

        return builder.ToString();
    }

    /// <summary>
    ///   Return the next calibration token.
    /// </summary>
    /// <param name="token">The current calibration token.</param>
    /// <param name="multiplier">The size multiplier.</param>
    /// <param name="size">The size of data matrix required.</param>
    /// <returns>The next calibration token.</returns>
    // ReSharper disable once UnusedMember.Global
    internal Token NextCalibrationToken(
        Token token = default,
        float multiplier = 1F,
        DataMatrix.Size size = DataMatrix.Size.Automatic) {
        return NextCalibrationToken(true, token, multiplier, size);
    }

    /// <summary>
    ///   Return the next calibration token.
    /// </summary>
    /// <param name="generateImages">Indicates whether the library should generate barcode images.</param>
    /// <param name="token">The current calibration token.</param>
    /// <returns>The next calibration token.</returns>
    // ReSharper disable once UnusedMember.Global
    internal Token NextCalibrationToken(
        bool generateImages,
        Token token = default) {
        return NextCalibrationToken(generateImages, token, 1F, DataMatrix.Size.Automatic);
    }

#if NET7_0_OR_GREATER
    /// <summary>
    ///   Returns a regular expression for matching AIM identifiers.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"^(?<prefix>.*)(?<characters>][A-Za-z][1-9A-Za-z])(?<code>.*)$", RegexOptions.None, "en-US")]
    private static partial Regex AimIdentifierMatchRegex();

    /// <summary>
    ///   Returns a regular expression for matching AIM identifiers with unrecognised flag characters.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"^(?<prefix>.*)(?<characters>\u0000[A-Za-z][1-9A-Za-z])(?<code>.*)$", RegexOptions.None, "en-US")]
    private static partial Regex AimIdentifierUnrecognisedFlagMatchRegex();

    /// <summary>
    ///   Returns a regular expression for invariant character strings of variable length.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"^[-!""%&'()*+,./0-9:;<=>?A-Z_a-z]+$", RegexOptions.None, "en-US")]
    private static partial Regex InvariantsMatchRegex();

    /// <summary>
    ///   Returns a regular expression for invariant character strings of variable length.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"\u0000(\u0000*\u0004)([^\u0000\u0020\u0040])$", RegexOptions.None, "en-US")]
    private static partial Regex DeadKeyAsciiControlCharacterSequenceRegex();

    /// <summary>
    ///   Returns a regular expression for dead key candidates.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"\0.", RegexOptions.None, "en-US")]
    private static partial Regex DeadKeysRegex();

    /// <summary>
    ///   Returns a regular expression for temporary space holder insertion for case 1.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"(?<a>\u0000[^\u0020])\u0020(?=[^\u0020])", RegexOptions.None, "en-US")]
    private static partial Regex Case1TempSpaceHolderRegex();

    /// <summary>
    /// Returns a regular expression for temporary space holder insertion for case 2.
    /// This also handles the case where there is no assigned computer keyboard key for the
    /// key pressed on the scanner keyboard.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"(?<a>\u0000[^\u0020]{0,1})\u0020{4}", RegexOptions.None, "en-US")]
    private static partial Regex Case2TempSpaceHolderRegex();

    /// <summary>
    ///   Returns a regular expression for reported dead key sequences where the key is not a dead key on the
    ///   scanner keyboard.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"(?<a>\u0000[^\u0020])((?=[^\u0020])|(?=(\u0020){2}))", RegexOptions.None, "en-US")]
    private static partial Regex NonMatchingDeadKeyComputerKeyboardCandidatesRegex();

    /// <summary>
    ///   Returns a regular expression for reported dead key sequences where the key is not a dead key on the
    ///   scanner keyboard.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"(?<a>\u0000[^\u0020])\u0020((?=[^\u0020])|(?=(\u0020){2}))", RegexOptions.None, "en-US")]
    private static partial Regex MatchingDeadKeyComputerKeyboardRegex();

    /// <summary>
    ///   Returns a regular expression to detect sequences of six or more spaces in multiples
    ///   of three and mark each capture except the last.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"(?<c>^|[^\u0020])(?<s>\u0020{3}){2,}?(?!\u0020)", RegexOptions.None, "en-US")]
    private static partial Regex ThreeSpaceTempSpaceHolderRegex();

    /// <summary>
    ///   Returns a regular expression to detect each occurrence of exactly two spaces.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"(?<c>^|[^\u0020])\u0020{2}(?!\u0020)", RegexOptions.None, "en-US")]
    private static partial Regex TwoSpaceTempSpaceHolderRegex();

    /// <summary>
    ///   Returns a regular expression to detect unassigned keys.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"^\u0000\u0020?$", RegexOptions.None, "en-US")]
    private static partial Regex UnassignedKeysRegex();

    /// <summary>
    ///   Returns a regular expression to filter dead key sequences (case 1).
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"(^|\0)[^\u0020]\u0020$", RegexOptions.None, "en-US")]
    private static partial Regex BarcodeScannerDeadKeysFilter1Regex();

    /// <summary>
    ///   Returns a regular expression to filter dead key sequences (case 2).
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"^\0[^\u0020]\u0020$", RegexOptions.None, "en-US")]
    private static partial Regex BarcodeScannerDeadKeysFilter2Regex();

    /// <summary>
    ///   Returns a regular expression to detect chained dead key sequences.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"\u0000{2,}(\w|[!""#$%&'()*+,./:;<=>?@\^_`{|}~-])", RegexOptions.None, "en-US")]
    private static partial Regex ChainedDeadKeysFilter2Regex();

    /// <summary>
    ///   Returns a regular expression to detect suffixes. This is relevant during small barcode processing where
    ///   it is used to remove repeated suffixes from the reported data. It is a best-endeavours approach that
    ///   assumes that the suffix never contains a sequence of four or more spaces. It also takes into account
    ///   that in some cases, an ASCII 0 may not be reported for a Control Character that the barcodes scanner
    ///   does not support. It assumes that Control Characters never result in Dead Key activations.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"(([^\s]\s{4})|([^\s]\s{8})|([^\s]\s{12})|([^\s]\s{16})|([^\s]\s{19})|([^\s]\s{20}))(?!.*\s{4})(?<s>\s{0,3}.*)$", RegexOptions.None, "en-US")]
    private static partial Regex SuffixRegex();

    /// <summary>
    /// Return a regular expression to detect any character except a space.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"[^ ]", RegexOptions.None, "en-US")]

    private static partial Regex AllSpaces();

    /// <summary>
    ///   Returns a regular expression to detect prefixes.  It is a best-endeavours approach that
    ///   assumes that the prefix never contains a sequence of two or more spaces, unless they appear at the end of prefix.
    /// </summary>
    /// <returns>A regular expression.</returns>
    [GeneratedRegex(@"^(?<prefix>.*?)(?=\u0020\u0020[^\u0020]).*$", RegexOptions.None, "en-US")]
    private static partial Regex PrefixRegex();
#endif

    /// <summary>
    ///   Calculate the length of any prefix.
    /// </summary>
    /// <param name="data">The reported barcode data.</param>
    /// <returns>The length of the prefix.</returns>
#if NET7_0_OR_GREATER
    private static string BarcodePrefix(string data) => PrefixRegex().Match(data).Groups["prefix"].Value;
#else
    private static string BarcodePrefix(string data) => PrefixRegex.Match(data).Groups["prefix"].Value;
#endif

    /// <summary>
    /// Removes repeated suffixes from reported data.  This is relevant when small barcode processing
    /// is used. This is a best-endeavours approach that assumes that the suffix does not contain
    /// any sequence of four or more spaces.
    /// </summary>
    /// <param name="data">The reported data.</param>
    /// <param name="knownSuffix">A suffix, if already known.</param>
    /// <param name="knownEndOfLine">An end-of-line sequence, if known.</param>
    /// <returns>The reported data with any internal suffixes stripped out, together with the suffix and end-of-line terminator.</returns>
    private static (string data, string? suffix, string? endOfLine) DetectSuffixAndStripRepeats(string? data, string? knownSuffix = null, string? knownEndOfLine = null) {
        if (string.IsNullOrEmpty(data)) return (string.Empty, knownSuffix, knownEndOfLine);

        var reportedData = data;
        var suffixValue = knownSuffix;
        var endOfLine = knownEndOfLine;
        var idx = 0;

        if (suffixValue is null) {
            // Find any suffix
#if NET7_0_OR_GREATER
            var suffix = SuffixRegex().Match(reportedData).Groups["s"];
#else
            var suffix = SuffixRegex.Match(reportedData).Groups["s"];
#endif

            if (suffix is not { Success: true, Length: > 0 }) return (reportedData, knownSuffix, knownEndOfLine);

            suffixValue = suffix.Value.TrimEnd('\n', '\r');
            endOfLine = suffix.Value[suffixValue.Length..];

            // As a safety measure, if all characters are spaces, then return.  This caters for situations where
            // ASCII control characters are not reported as nulls if not supported by scanner keyboard layout.
            // NB. this situation should be detected already using SuffixRegex(), but if for any reason it is
            // not, this acts as a backstop.
#if NET7_0_OR_GREATER
            var suffixFirstNonSpaceCharacter = AllSpaces().Match(suffixValue);
#else
            var suffixFirstNonSpaceCharacter = AllSpaces.Match(suffixValue);
#endif

            if (suffixFirstNonSpaceCharacter is not { Success: true }) return (reportedData, knownSuffix, knownEndOfLine);
        }

        if (suffixValue == string.Empty) return (reportedData, knownSuffix, knownEndOfLine);

        while (idx >= 0 && idx < reportedData.Length - suffixValue.Length) {
            idx = reportedData.IndexOf(suffixValue, idx, StringComparison.InvariantCulture);

            if (idx < 0) continue;

            if (suffixValue.Length == 1 && idx > 0 && reportedData[idx - 1] == ' '
                && ((idx < reportedData.Length - 1 && reportedData[idx + 1] == ' ')
                    || (idx < reportedData.Length - 2 && reportedData[idx + 1] != ' ' && reportedData[idx + 2] == ' '))) {
                idx++;
                continue;
            }

            reportedData = reportedData[..idx] + reportedData[(idx + suffixValue.Length)..];
            idx += suffixValue.Length;
        }

        return (reportedData.TrimEnd('\n', '\r') + suffixValue + endOfLine, suffixValue, endOfLine);
    }

    /// <summary>
    ///   Calculates the standard deviation of the intervals between repeating characters
    ///   (e.g., space, ASCII 0) in a calibration barcode using a hypothetical mean.
    /// </summary>
    /// <param name="data">The data contained in a barcode.</param>
    /// <param name="character">The repeating character.</param>
    /// <param name="hypotheticalMean">The theoretical mean for the barcode type.</param>
    /// <param name="count">The count f the number of repeating characters detected.</param>
    /// <param name="countCharacter">Indicates if the code should count occurrences of the character parameter, or count other characters.</param>
    /// <returns>The standard deviation of the intervals between repeating characters.</returns>
    private static double StdDevForRepeatingCharacters(string data, char character, double hypotheticalMean, out int count, bool countCharacter = false) {
        var cumulativeDiffSqd = 0D;
        var lastIdx = -1;
        count = 0;

        // Analyse the data to look for predominance of alternating spaces
        for (var idx = 0; idx < data.Length; idx++) {
            if (data[idx] != character) {
                count += countCharacter ? 0 : TestForAscii0();
                continue;
            }

            cumulativeDiffSqd += Math.Pow(idx - lastIdx - hypotheticalMean, 2);
            lastIdx = idx;
            count += countCharacter ? 1 : 0;
            continue;

            int TestForAscii0() => data[idx] == (char)0 ? 0 : 1;
        }

        // ReSharper disable once CommentTypo
        // Because we are using a hypothetical mean based on observation of a sample of barcodes
        // we will apply Bessel's correction (n-1).
        return Math.Sqrt(cumulativeDiffSqd / (count - 1D));
    }

    /// <summary>
    ///   Detect sequence types.
    /// </summary>
    /// <param name="characters">A list of character sequences.</param>
    /// <param name="reportedCharacters">A list to which the reported characters will be added.</param>
    /// <param name="tempSpaceCharacter">The temporary space character.</param>
    /// <returns>True, if the correct number of character sequences are selected; otherwise false.</returns>
    // ReSharper disable once SuggestBaseTypeForParameter
    private static bool DetectSequenceTypes(
        IList<string> characters,
        List<string> reportedCharacters,
        char tempSpaceCharacter) {
#if Diagnostics
            foreach (var s in characters)
            {
                if (s.Length == 0)
                {
                    System.Diagnostics.Debug.Write($"EMPTY");
                }
                foreach (var c in s)
                {
                    if (c < 32)
                    {
                        System.Diagnostics.Debug.Write($"\\{(int)c}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.Write(c);
                    }
                }
                System.Diagnostics.Debug.WriteLine("");
            }
#endif
        for (var sequenceIdx = 1; sequenceIdx < characters.Count; sequenceIdx++) {
            var sequence = characters[sequenceIdx];

            switch (sequence) {
                case { Length: >= 2 }:

                    switch (sequence) {
                        case var _ when sequence[1] == '\u0000':

                            // This is a dead key sequence. Split the sequence by ASCII zero and add to split sequences dictionary
                            var splitSequence = sequence.Split('\u0000').ToList();

                            for (var splitSequenceIdx = 0;
                                 splitSequenceIdx < splitSequence.Count;
                                 splitSequenceIdx++) {
                                var sequenceCharacter = splitSequence[splitSequenceIdx];

                                if (splitSequenceIdx == 0) {
                                    if (sequenceCharacter == tempSpaceCharacter.ToInvariantString()) {
                                        sequenceCharacter = string.Empty;
                                    }

                                    reportedCharacters.Add(sequenceCharacter);
                                }
                                else {
                                    reportedCharacters.Add("\u0000" + sequenceCharacter);
                                }
                            }

                            break;
                        default:
                            reportedCharacters.Add(sequence);

                            break;
                    }

                    break;
                case { Length: 1 }:
                    reportedCharacters.Add(sequence);

                    break;
                default:

                    // An empty sequence represents an unrecognised character.
                    reportedCharacters.Add(sequence);

                    break;
            }
        }

        return reportedCharacters.Count == AsciiChars.Length;
    }

    /// <summary>
    ///   Resolves the reported segment characters to a Unicode block representing the script of the
    ///   OS-configured keyboard layout.
    /// </summary>
    /// <param name="segmentCharacters">The characters in the segment of reported characters.</param>
    /// <param name="capsLockIndicator">Indicates if the caps lock may be switched on.</param>
    /// <returns>
    ///   The name of the Unicode block representing the script of the OS-configured keyboard layout.
    /// </returns>
    private static string ResolveKeyboardScript(IReadOnlyList<string> segmentCharacters, bool capsLockIndicator = false) {
        var adjustment = 82 - segmentCharacters.Count;
        try {
            var upperCaseSequences = Enumerable.Range(1, 26).Select(idx => segmentCharacters[idx + 28 - adjustment]);
            var lowerCaseSequences = Enumerable.Range(1, 26).Select(idx => segmentCharacters[idx + 55 - adjustment]);

            return capsLockIndicator
                ? UnicodeBlocks.ResolveScript(upperCaseSequences, lowerCaseSequences)
#pragma warning disable S2234
                : UnicodeBlocks.ResolveScript(lowerCaseSequences, upperCaseSequences);
#pragma warning restore S2234
        }
        catch (ArgumentOutOfRangeException argEx) {
            Console.WriteLine(Properties.Advice.ErrorWhileResolvingKeyboardScripts, argEx.Message);
            return "<unknown>";
        }
    }

    /// <summary>
    ///   Splits a sequence of reported characters where the sequence contains multiple entries.
    /// </summary>
    /// <param name="sequence">The sequence to be split.</param>
    /// <returns>A list of sequences.</returns>
    private static List<string> SplitSequence(string sequence) {
        // If a dead key sequence is followed by an unrecognised character, it will be just two characters, and there is no need to
        // process it. We only need to process longer sequences.
        if (sequence.Length < 2 || sequence is ['\u0000', _]) {
            return [sequence];
        }

        // If the sequence starts with an ASCII 0 then it needs to be split into inner sequences.
        if (sequence[0] != '\u0000') {
            return [sequence];
        }

        // Split the sequence by ASCII 0 and add to split sequences list.
        var splitSequence = sequence.Split('\u0000').ToList();
        var splitSequenceOut = new List<string>();

        // The first inner sequence will be empty and can be ignored.
        for (var splitSequenceIdx = 1; splitSequenceIdx < splitSequence.Count; splitSequenceIdx++) {
            var sequenceCharacters = splitSequence[splitSequenceIdx];

            switch (sequenceCharacters.Length) {
                // Ignore any sequences that are empty. These represent chained dead keys which we will normalize later.
                case > 1: {
                        /* This must be a dead key sequence for a literal dead key character (or multiple
                         * adjacent literals), either:
                         *
                         *   a)  followed by a non-dead key character where the two sequences have effectively
                         *       been concatenated by the consumption of the space delimiter in the barcode.
                         *   c)  a space, representing a corresponding literal dead key used on the scanner keyboard.
                         *       At the end of sequence, there is a non-dead key character, as for a).
                         *
                         *   Reproduce two or more sequences.
                         * */
                        splitSequenceOut.Add("\u0000" + sequenceCharacters[0] + (sequenceCharacters[1] == ' ' ? " " : string.Empty));
                        var nextSequence = sequenceCharacters[1] == ' ' && sequenceCharacters.Length > 2
                            ? sequenceCharacters[2..]
                            : sequenceCharacters[1..].TrimEnd();
                        if (nextSequence.Length > 0) {
                            splitSequenceOut.Add(nextSequence);
                        }

                        break;
                    }

                case 1:
                case 0:
                    // This is a simple dead key sequence.
                    splitSequenceOut.Add("\u0000" + sequenceCharacters);
                    break;
            }
        }

        return splitSequenceOut;
    }

    /// <summary>
    ///   A collection of calibration tokens.
    /// </summary>
    /// <param name="generateImages">Indicates whether the library should generate barcode images.</param>
    /// <param name="generateSvg">Indicates whether the library should generate SVG content.</param>
    /// <param name="multiplier">The size multiplier.</param>
    /// <param name="size">The size of data matrix required.</param>
    /// <param name="backgroundColour">Background colour (hex - use #nnnnnn format).</param>
    /// <param name="foregroundColour">Foreground colour (hex - use #nnnnnn format).</param>
    /// <returns>The collection of calibration tokens.</returns>
    private IEnumerable<Token> GetCalibrationTokens(
        bool generateImages,
        bool generateSvg,
        float multiplier,
        DataMatrix.Size size,
        string backgroundColour,
        string foregroundColour) {
        using var barcode = generateImages || generateSvg ? new Barcode {
            ForegroundColor = GetColour(foregroundColour),
            BackgroundColor = GetColour(backgroundColour),
            Multiplier = multiplier,
            ImageFormat = SixLabors.ImageSharp.Formats.Png.PngFormat.Instance,
        }
        : null;

        var maximumCharacters = size.MaxCapacity();
        Token token;

        if (_tokenDataBarcodeData.Length == 0) {
            // Yield the initial baseline barcode containing segments of character sequences for invariant characters,
            // non-invariant printable characters and additional ASCII control characters.
            _tokenDataBarcodeData = _baselineBarcodeData
                                        + (AssessFormatSupport
                                            ? SegmentDelimiter + _baselineBarcodeDataFormat06 + SegmentDelimiter
                                            : SegmentDelimiter);
            _tokenDataCalibrationsRemaining = 1;

            InitialiseForYield();

            foreach (var barcodeDataSegment in _barcodeDataSegments?.Segments ?? []) {
                token = new Token(
                    barcodeDataSegment,
                    string.Empty,
                    default,
                    _tokenDataCalibrationsRemaining,
                    ++_tokenDataSmallBarcodeSequenceIndex,
                    _tokenDataSmallBarcodeSequenceCount,
                    _tokenDataPrefix,
                    _tokenDataSuffix,
                    _tokenDataReportedCharacters,
                    generateImages ? barcode?.CreateBarcode(barcodeDataSegment) : null,
                    generateSvg ? barcode?.CreateBarcodeSvg(barcodeDataSegment) : null,
                    --_tokenRemaining,
                    _tokenSize,
                    _tokenKeyboardMatch,
                    _tokenCalibrationData,
                    _tokenSystemCapabilities,
                    _tokenCalibrationSessionAbandoned,
                    _tokenReportedPrefixSegment,
                    _tokenReportedSuffix);
                SetInformation();
                yield return token;
            }
        }

        _tokenDataReportedCharacters = string.Empty;
        _tokenRemaining = _tokenDataCalibrationsRemaining;

        if (_tokenRemaining <= 0) {
            yield break;
        }

        foreach (var (key, value) in _tokenExtendedDataDeadKeyCharacterMap.ToArray()) {
            if (key[0] != '\0') {
                continue;
            }

            // Yield the expected dead key calibration barcode.
            _tokenDataBarcodeData = CreateDeadKeyCalibration(value);
            InitialiseForYield();

            if (_barcodeDataSegments?.Count == 0) {
                token = new Token(
                    _tokenDataBarcodeData,
                    key,
                    value,
                    --_tokenRemaining,
                    _tokenDataCalibrationsRemaining,
                    bitmapStream: generateImages ? barcode?.CreateBarcode(_tokenDataBarcodeData) : null,
                    svg: generateSvg ? barcode?.CreateBarcodeSvg(_tokenDataBarcodeData) : null,
                    prefix: _tokenDataPrefix,
                    suffix: _tokenDataSuffix,
                    size: _tokenSize);
                SetInformation();
                yield return token;
            }
            else {
                var failedCalibrationErrors = new List<InformationType>
                {
                    InformationType.PartialCalibrationDataReported,
                    InformationType.IncorrectCalibrationDataReported,
                    InformationType.UnrecognisedData,
                    InformationType.NoCalibrationDataReported,
                };

                foreach (var barcodeDataSegment in _barcodeDataSegments?.Segments ?? []) {
                    var calibrationSessionAbandoned =
                        _tokenErrors?.Exists(e => failedCalibrationErrors.Contains(e.InformationType)) ?? false;

                    // If the calibration failed because of an issue related to (the) scanned barcode, then return the last token
                    // that was submitted for calibration. Otherwise, return the next token.
                    token = calibrationSessionAbandoned ? _lastToken.AbandonCalibrationSession() : new Token(
                        barcodeDataSegment,
                        key,
                        value,
                        _tokenDataCalibrationsRemaining,
                        ++_tokenDataSmallBarcodeSequenceIndex,
                        _tokenDataSmallBarcodeSequenceCount,
                        _tokenDataPrefix,
                        _tokenDataSuffix,
                        _tokenDataReportedCharacters,
                        generateImages ? barcode?.CreateBarcode(barcodeDataSegment) : null,
                        generateSvg ? barcode?.CreateBarcodeSvg(barcodeDataSegment) : null,
                        --_tokenRemaining,
                        _tokenSize,
                        _tokenKeyboardMatch,
                        _tokenCalibrationData,
                        _tokenSystemCapabilities,
                        _tokenCalibrationSessionAbandoned,
                        _tokenReportedPrefixSegment,
                        _tokenReportedSuffix);
                    if (!calibrationSessionAbandoned) SetInformation();
                    yield return token;
                }
            }
        }

        yield break;

        Color GetColour(string hexColor) {
            if (hexColor.Length is < 6 or > 7) {
                return Color.Black;
            }

            // Remove #
            if (hexColor.First() == '#') hexColor = hexColor[1..];
            return Color.FromRgb(
                Convert.ToByte(hexColor[..2], 16),
                Convert.ToByte(hexColor[2..4], 16),
                Convert.ToByte(hexColor[4..], 16));
        }

        void SetInformation() {
            _tokenInformation?.ForEach(information => token.AddInformation(information));
            _tokenWarnings?.ForEach(warning => token.AddInformation(warning));
            _tokenErrors?.ForEach(error => token.AddInformation(error));
        }

        void InitialiseForYield() {
            _barcodeDataSegments = new BarcodeData(_tokenDataBarcodeData, maximumCharacters);

            _tokenRemaining = _barcodeDataSegments.Count > 0
                ? _barcodeDataSegments.Count * _tokenDataCalibrationsRemaining
                : TestBarcodeDataSegmentsLt0();

            _tokenSize = size;
            _tokenDataSmallBarcodeSequenceIndex = _barcodeDataSegments.Count > 0 ? 0 : -1;
            _tokenDataSmallBarcodeSequenceCount =
                _barcodeDataSegments.Count > 0 ? _barcodeDataSegments.Count : -1;
            _tokenDataReportedCharacters = string.Empty;
            return;

            int TestBarcodeDataSegmentsLt0() =>
                _barcodeDataSegments.Count < 0
                    ? _tokenRemaining
                    : 0;
        }
    }

    /// <summary>
    ///   Return the next calibration token.
    /// </summary>
    /// <param name="generateImages">Indicates whether the library should generate barcode images.</param>
    /// <param name="token">The current calibration token.</param>
    /// <param name="multiplier">The size multiplier.</param>
    /// <param name="size">The size of data matrix required.</param>
    /// <returns>The next calibration token.</returns>
    // ReSharper disable once UnusedMember.Global
    private Token NextCalibrationToken(
        bool generateImages,
        Token token,
        float multiplier,
        DataMatrix.Size size) {
        _tokenSize = size == DataMatrix.Size.Automatic ? token.Size : size;
        var baselineKey = string.Empty;

        using var barcode = generateImages ? new Barcode {
            ForegroundColor = Color.Black,
            BackgroundColor = Color.White,
            Multiplier = multiplier,
            ImageFormat = SixLabors.ImageSharp.Formats.Png.PngFormat.Instance,
        }
        : null;

        if (token.ToJson().Replace(" ", string.Empty) == "{}") {
            // Return the initial baseline barcode containing segments of character sequences for invariant characters,
            // non-invariant printable characters and additional ASCII control characters.
            _tokenDataBarcodeData = _baselineBarcodeData
                                      + (AssessFormatSupport
                                             ? SegmentDelimiter + _baselineBarcodeDataFormat06 + SegmentDelimiter
                                             : SegmentDelimiter);
            _tokenDataCalibrationsRemaining = 1;

            Initialise();

            return new Token(
                _tokenDataBarcodeData,
                baselineKey,
                default,
                _tokenDataCalibrationsRemaining,
                ++_tokenDataSmallBarcodeSequenceIndex,
                _tokenDataSmallBarcodeSequenceCount,
                _tokenDataPrefix,
                _tokenDataSuffix,
                _tokenDataReportedCharacters,
                barcode?.CreateBarcode(_barcodeDataSegments?.Segments.FirstOrDefault() ?? string.Empty),
                barcode?.CreateBarcodeSvg(_barcodeDataSegments?.Segments.FirstOrDefault() ?? string.Empty),
                --_tokenRemaining,
                _tokenSize,
                _tokenKeyboardMatch,
                _tokenCalibrationData,
                _tokenSystemCapabilities,
                _tokenCalibrationSessionAbandoned,
                _tokenReportedPrefixSegment,
                _tokenReportedSuffix);
        }

        DoInitializeFromTokenData(token);

        var failedCalibrationErrors = new List<InformationType>
        {
            InformationType.PartialCalibrationDataReported,
            InformationType.IncorrectCalibrationDataReported,
            InformationType.UnrecognisedData,
            InformationType.NoCalibrationDataReported,
        };

        var hasCalibrationFailed =
            _tokenErrors?.Exists(e => failedCalibrationErrors.Contains(e.InformationType)) ?? false;

        // If the calibration failed because of an issue related to the scanned barcode, then return the last token
        // that was submitted for calibration.
        if (hasCalibrationFailed) return token.AbandonCalibrationSession();

        if (_tokenRemaining <= 0) {
            return default;
        }

        _barcodeDataSegments = new BarcodeData(_tokenDataBarcodeData, _tokenSize.MaxCapacity());

        if (_tokenExtendedDataDeadKeyCharacterMap is null) throw new InvalidOperationException();

        if (_tokenExtendedDataDeadKeyCharacterMap.Count == 0) {
            return new Token(
                _tokenDataBarcodeData,
                baselineKey,
                default,
                _tokenDataCalibrationsRemaining,
                _tokenDataSmallBarcodeSequenceIndex + 1,
                _tokenDataSmallBarcodeSequenceCount,
                _tokenDataPrefix,
                _tokenDataSuffix,
                _tokenDataReportedCharacters,
                barcode?.CreateBarcode(_barcodeDataSegments.Segments.ElementAt(_tokenDataSmallBarcodeSequenceIndex)),
                barcode?.CreateBarcodeSvg(_barcodeDataSegments.Segments.ElementAt(_tokenDataSmallBarcodeSequenceIndex++)),
                --_tokenRemaining,
                _tokenSize,
                _tokenKeyboardMatch,
                _tokenCalibrationData,
                _tokenSystemCapabilities,
                _tokenCalibrationSessionAbandoned,
                _tokenReportedPrefixSegment,
                _tokenReportedSuffix);
        }

        var (s, c) = _tokenExtendedDataDeadKeyCharacterMap
                    .Where(kvp => kvp.Key[0] == '\0')
                    .ElementAt(_tokenExtendedDataDeadKeyCharacterMap.Count - _tokenDataCalibrationsRemaining);

        _tokenDataBarcodeData = CreateDeadKeyCalibration(c);

        if (_tokenDataSmallBarcodeSequenceIndex == -1) {
            Initialise();
        }

        StoreTokenData();

        return InitializeTokenData();

        void Initialise() {
            _barcodeDataSegments = new BarcodeData(_tokenDataBarcodeData, _tokenSize.MaxCapacity());

            _tokenRemaining = _barcodeDataSegments.Count > 0
                ? _barcodeDataSegments.Count * _tokenDataCalibrationsRemaining
                : TestBarcodeDataSegmentsLt0();

            _tokenDataSmallBarcodeSequenceIndex = _barcodeDataSegments.Count > 0 ? 0 : -1;
            _tokenDataSmallBarcodeSequenceCount = _barcodeDataSegments.Count > 0 ? _barcodeDataSegments.Count : -1;
            _tokenDataReportedCharacters = string.Empty;
            _tokenKeyboardMatch = null;
            CalibrationData = null;
            _tokenCalibrationSessionAbandoned = false;
            return;

            int TestBarcodeDataSegmentsLt0() =>
                _barcodeDataSegments.Count < 0
                    ? _tokenRemaining
                    : 0;
        }

        void StoreTokenData() {
            _tokenDataKey = s;
            _tokenDataValue = c;

            if (_barcodeDataSegments.Count == -1 || _tokenDataSmallBarcodeSequenceIndex == _barcodeDataSegments.Count) {
                _barcodeDataSegments = new BarcodeData(
                    _tokenDataBarcodeData,
                    _tokenSize.MaxCapacity());
                _tokenDataSmallBarcodeSequenceIndex = 0;
                _tokenDataReportedCharacters = string.Empty;
            }

            _tokenBitmapStream = barcode?.CreateBarcode(
                _barcodeDataSegments.Segments.ElementAt(_tokenDataSmallBarcodeSequenceIndex));
            _tokenSvg = barcode?.CreateBarcodeSvg(
                _barcodeDataSegments.Segments.ElementAt(_tokenDataSmallBarcodeSequenceIndex++));
            --_tokenRemaining;

            _tokenInformation?.Clear();
            _tokenWarnings?.Clear();
            _tokenErrors?.Clear();

            foreach (var information in token.Information) {
                _tokenInformation?.Add(information);
            }

            foreach (var warning in token.Warnings) {
                _tokenWarnings?.Add(warning);
            }

            foreach (var error in token.Errors) {
                _tokenErrors?.Add(error);
            }

            // Order the collections
            _tokenInformation = _tokenInformation?.OrderBy(ci => (int)ci.InformationType).ToList();
            _tokenWarnings = _tokenWarnings?.OrderBy(ci => (int)ci.InformationType).ToList();
            _tokenErrors = _tokenErrors?.OrderByDescending(ci => (int)ci.InformationType).ToList();
        }
    }

    /// <summary>
    ///   Calibrates for a given combination of barcode scanner and OS keyboard layouts.
    /// </summary>
    /// <param name="data">
    ///   The reported input after scanning the calibration barcode.
    /// </param>
    /// <param name="token">The current calibration token.</param>
    /// <param name="capsLock">Indicates if Caps Lock is switched on.</param>
    /// <param name="platform">The platform on which the system resides.</param>
    /// <param name="dataEntryTimeSpan">
    ///   The time span specifying how long it took from the start of the scan to
    ///   submitting the data.
    /// </param>
    /// <param name="assessScript">Indicates whether the calibrator should assess the script for the configured OS keyboard.</param>
    /// <returns>The calibration token together with any suffix and end-of-line data.</returns>
    private (Token token, string suffix, string endOfLine) CalibrateBaseLine(
        string data,
        Token token,
        bool? capsLock = null,
        SupportedPlatform platform = SupportedPlatform.Windows,
        TimeSpan dataEntryTimeSpan = default,
        bool assessScript = true)
    {
        // Resolve the data entry time span to determine a barcode scanner keyboard performance assessment value.
        _tokenExtendedDataScannerKeyboardPerformance = dataEntryTimeSpan.TotalMilliseconds switch {
            < (double)ScannerKeyboardPerformance.Medium => ScannerKeyboardPerformance.High,
            < (double)ScannerKeyboardPerformance.Low => ScannerKeyboardPerformance.Medium,
            _ => ScannerKeyboardPerformance.Low
        };

        // Calculate the number of scanned characters per second.
        _tokenExtendedDataScannerCharactersPerSecond = data.Length > 0
            ? (int)Math.Floor(data.Length / dataEntryTimeSpan.TotalSeconds)
            : 0;

        // If this is a small barcode within a sequence, but not the last barcode, return the token.
        if (TryInSmallBarcodeSequence(ref data, ref token)) return (token, string.Empty, string.Empty);

        // Strip out any suffix. For small barcode processing, any suffix will be repeated several times.
        var detectedSuffix = DetectSuffixAndStripRepeats(data);
        data = detectedSuffix.data;
        _tokenDataSuffix = detectedSuffix.suffix;
        var strippedData = data[..^((detectedSuffix.suffix?.Length ?? 0) + (detectedSuffix.endOfLine?.Length ?? 0))];

        // Strip off any prefix. For small barcode processing, any prefix will be repeated several times.
        strippedData = _tokenDataPrefix?.Length > 0 &&
                       strippedData.StartsWith(_tokenDataPrefix, StringComparison.Ordinal)
            ? strippedData[_tokenDataPrefix.Length..]
            : strippedData[BarcodePrefix(strippedData).Length..];

        token = new Token(token, prefix: _tokenDataPrefix ?? string.Empty, suffix: _tokenDataSuffix ?? string.Empty);

        // Determine if the user has scanned the correct baseline barcode and if it is fully or partially reported.
        token = DetermineBarcodeProvenance(token, strippedData, out var immediateReturn);

        // If there is an issue with the barcode provenance or reported data, then return immediately.
        if (immediateReturn) {
            return (token, detectedSuffix.suffix ?? string.Empty, detectedSuffix.endOfLine ?? string.Empty);
        }

        ResetStateForBaselineCalibration();

        // Get the list of segments for expected characters (i.e., the characters expected as a result of successful mapping).
        var expectedSegments = ExpectedSegments();

        // Convert the data string to a list of segments of character sequences representing the
        // reported characters in the baseline barcode.
        token = ConvertToSegments(token, data, expectedSegments, out var reportedSegments, out var lineFeedChar);

        // If there are not at least three reported segments, calibration has failed, so exit.
        if (reportedSegments.Count <= (int)Segments.AdditionalAsciiSegment) return (token, detectedSuffix.suffix ?? string.Empty, detectedSuffix.endOfLine ?? string.Empty);

        // Record the prefix segment, if a prefix is reported.
        _tokenReportedPrefixSegment = reportedSegments[(int)Segments.PrefixSegment];

        // Record the suffix segment, if a suffix is reported.
        _tokenReportedSuffix = reportedSegments.Count > (int)Segments.SuffixSegment &&
                                  reportedSegments[(int)Segments.SuffixSegment].Count > 0 &&
                                  reportedSegments[(int)Segments.SuffixSegment][0].Length != 0
            ? reportedSegments[(int)Segments.SuffixSegment][0].TrimEnd('\r', '\n')
            : string.Empty;

        /* We have two clean sets of sequences of characters - one for the data reported from scanning
         * the baseline barcode and one for the characters we expect to be reported after mapping (the
         * characters actually in the baseline barcode). We can now process the reported data and any
         * reported dead keys.
         * */

        ProcessReportedSegments(
            token,
            reportedSegments,
            expectedSegments,
            capsLock,
            platform,
            assessScript);

        ProcessDeadKeys();

        // If line feeds are deemed to map to a different control character, map LF to the control character.
        if ((int)lineFeedChar is not (AsciiLineFeedChar or AsciiNullChar)) {
            _tokenExtendedDataCharacterMap.Add(new KeyValuePair<char, char>(lineFeedChar, '\n'));
        }

        // Record the LF character separately.
        _tokenExtendedDataLineFeedCharacter = lineFeedChar == 0 ? null : lineFeedChar.ToString();

        // Set the current barcode type to Dead Key here. This will be validated on each further calibration call.
        CurrentBarcodeType = BarcodeType.DeadKey;

        return (InitializeTokenData(), detectedSuffix.suffix ?? string.Empty, detectedSuffix.endOfLine ?? string.Empty);
    }

    /// <summary>
    ///   Calibrates for a given combination of barcode scanner and OS keyboard layouts.
    /// </summary>
    /// <param name="data">The reported input after scanning the calibration barcode.</param>
    /// <param name="token">The current calibration token.</param>
    /// <param name="dataEntryTimeSpan">The time span specifying how long it took from the start of the scan to submitting the data.</param>
    /// <param name="suffix">A known suffix, discovered during. </param>
    /// <param name="endOfLine">The end-of-line character sequence.</param>
    /// <returns>The calibration token.</returns>
    private (Token token, string suffix, string endOfLine) CalibrateDeadKey(
        string data,
        Token token,
        TimeSpan dataEntryTimeSpan = default,
        string? suffix = null,
        string? endOfLine = null) {
        // Resolve the data entry time span to a barcode scanner keyboard performance assessment value.
        var candidateAssessmentValue =
            dataEntryTimeSpan.TotalMilliseconds < (double)ScannerKeyboardPerformance.Medium
                ? ScannerKeyboardPerformance.High
                : TestForMediumOrLowPerformance();

        // If the resolved value is less than the current value, choose it.
        _tokenExtendedDataScannerKeyboardPerformance = _tokenExtendedDataScannerKeyboardPerformance > candidateAssessmentValue
                                                               ? candidateAssessmentValue
                                                               : _tokenExtendedDataScannerKeyboardPerformance;

        // Calculate the number of scanned characters per second.
        var candidateCharactersPerSecond = data.Length > 0
            ? (int)Math.Floor(data.Length / dataEntryTimeSpan.TotalSeconds)
            : 0;

        // If the calculated cps value is less than the current value, choose it.
        _tokenExtendedDataScannerCharactersPerSecond = _tokenExtendedDataScannerCharactersPerSecond > candidateCharactersPerSecond
            ? candidateCharactersPerSecond
            : _tokenExtendedDataScannerCharactersPerSecond;

        // If this is a small barcode, but not the last barcode in the sequence, return the token
        if (token.Data?.SmallBarcodeSequenceCount > 0) {
            data = token.Data.SmallBarcodeSequenceIndex < token.Data.SmallBarcodeSequenceCount

                       // Strip off any trailing Carriage Return or Line Feed characters. These are assumed to have
                       // been added by the barcode scanner.
                       ? data.StripTrailingCrLfs()
                       : data;

            // Strip off any prefixes on second and subsequent barcodes. This assumes that the prefix is the
            // same as any detected when processing the baseline calibration barcode.
            data = token.Data.SmallBarcodeSequenceIndex > 1 &&
                   _tokenDataPrefix?.Length > 0 &&
                   data.StartsWith(_tokenDataPrefix, StringComparison.Ordinal)
                       ? data[_tokenDataPrefix.Length..]
                       : data;

            _tokenDataReportedCharacters += data;

            if (token.Data.SmallBarcodeSequenceIndex > 0 && token.Data.SmallBarcodeSequenceIndex < token.Data.SmallBarcodeSequenceCount) {
                return (token, suffix ?? string.Empty, endOfLine ?? string.Empty);
            }

            data = _tokenDataReportedCharacters;
        }

        // Detect the suffix and strip out any repeated suffix.  For small barcode processing, any suffix will be repeated several times.
        var detectedSuffix = DetectSuffixAndStripRepeats(data, _tokenDataSuffix);
        data = detectedSuffix.data[..^((detectedSuffix.suffix?.Length ?? 0) + (detectedSuffix.endOfLine?.Length ?? 0))];

        // Strip off any prefix. For small barcode processing, any prefix will be repeated several times.
        data = _tokenDataPrefix?.Length > 0 &&
               data.StartsWith(_tokenDataPrefix, StringComparison.Ordinal)
            ? data[_tokenDataPrefix.Length..]
            : data[BarcodePrefix(data).Length..];

        var provenance = BarcodeProvenance(data, true);

        switch (provenance) {
            case Calibration.BarcodeProvenance.PartialDeadkey:
                // Error - Partial data reported for calibration barcode.
                LogCalibrationInformation(token, InformationType.PartialCalibrationDataReported);

                // Error - Calibration failed.
                return (LogCalibrationInformation(token, InformationType.CalibrationFailedUnexpectedly), suffix ?? string.Empty, endOfLine ?? string.Empty);
            case Calibration.BarcodeProvenance.Unknown:
                provenance = BarcodeProvenance(data);
                switch (provenance) {
                    case Calibration.BarcodeProvenance.Baseline:
                    case Calibration.BarcodeProvenance.PartialBaseline:

                        // Error - The reported data is for the wrong calibration barcode.
                        LogCalibrationInformation(token, InformationType.IncorrectCalibrationDataReported);

                        // Error - Calibration failed.
                        return (LogCalibrationInformation(token, InformationType.CalibrationFailed), suffix ?? string.Empty, endOfLine ?? string.Empty);
                    case Calibration.BarcodeProvenance.Unknown:

                        // Error - The reported data is unrecognised. The wrong barcode may have been scanned.
                        return (LogCalibrationInformation(token, InformationType.UnrecognisedData), suffix ?? string.Empty, endOfLine ?? string.Empty);
                    case Calibration.BarcodeProvenance.DeadKey:
                        break;
                    case Calibration.BarcodeProvenance.PartialDeadkey:
                    case Calibration.BarcodeProvenance.NoData:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(data),
                            provenance,
                            Resources.CalibrateDeadKeyIncorrectBarcodeProvenance);
                }

                break;
            case Calibration.BarcodeProvenance.NoData:

                // Error - No calibration data was reported.
                return (LogCalibrationInformation(token, InformationType.NoCalibrationDataReported), suffix ?? string.Empty, endOfLine ?? string.Empty);
            case Calibration.BarcodeProvenance.Baseline:
            case Calibration.BarcodeProvenance.DeadKey:
            case Calibration.BarcodeProvenance.PartialBaseline:
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(data),
                    provenance,
                    Resources.CalibrateDeadKeyIncorrectBarcodeProvenance);
        }

        /* A few keyboards do not follow the normal convention of using dead keys to modify the space bar to
         * return the literal dead key character. The character returned when the space bar is pressed is often
         * reminiscent of the literal dead key - e.g., returning a double quote instead of a diaeresis. The
         * code depends on the use of the space as a delimiter in the baseline barcode, and the result of this
         * is that we detect the incorrect dead key. Hence, the reported data for the current barcode may be
         * useless. There is no direct way to know if this is the case. Instead, this code takes a heuristic
         * approach. We look for all the occurrence of ASCII 0 followed by a character, and then find which
         * character occurs the most times. We can be very confident that this is the correct dead key
         * character.
         * */

#if NET7_0_OR_GREATER
        var candidateDeadKeys = DeadKeysRegex().Matches(data);
#else
        var candidateDeadKeys = DeadKeysRegex.Matches(data);
#endif

        var candidateDeadKeyRankings = new Dictionary<char, int>();

        foreach (var key in candidateDeadKeys) {
            if (key is not Match candidate) {
                continue;
            }

            candidateDeadKeyRankings.TryAdd(candidate.Value[1], 0);
            candidateDeadKeyRankings[candidate.Value[1]] += 1;
        }

        var correctDeadKey = candidateDeadKeyRankings.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
        var deadKey = token.Data?.Key;

        if (correctDeadKey != token.Data?.Key?[1]) {
            // The correct dead key is different to the one reported.
            if (!_tokenExtendedDataDeadKeysMap.ContainsKey($"\0{correctDeadKey}") && token.Data?.Key is not null) {
                var deadKeyValue = _tokenExtendedDataDeadKeysMap[token.Data.Key];
                deadKey = $"\0{correctDeadKey.ToInvariantString()}";
                _tokenExtendedDataDeadKeysMap.Add(deadKey, deadKeyValue);
                _tokenExtendedDataDeadKeysMap.Remove(token.Data.Key);
                _tokenExtendedDataDeadKeyFixUp.Add(token.Data.Key, deadKey);

                /* We need to add entries to the character map to handle situations where a deadKeyValue
                 * combination with another character may map to two dead key characters on the computer
                 * keyboard. This requires an entry in the character map to handle the second character
                 * map. On some keyboards, a dead key followed by a space results in the reporting of a
                 * different character to the actual dead key literal character. E.g., ¨ followed by a
                 * space may be reported as ". We account for this here removing the entry for the
                 * incorrect dead key character and adding a new entry for the correct character.
                 * */
                _tokenExtendedDataCharacterMap.Remove(token.Data.Key[1]);

                if (!_tokenExtendedDataCharacterMap.ContainsKey(correctDeadKey)) {
                    _tokenExtendedDataCharacterMap.Add(correctDeadKey, deadKeyValue[0]);
                }
            }

            _tokenDataKey = $"\0{correctDeadKey}";

            // Check AIM identifier
            _tokenExtendedDataAimFlagCharacterSequence =
                token.Data?.Key != null &&
                _tokenExtendedDataAimFlagCharacterSequence == token.Data.Key
                    ? ProcessPrefix()
                    : _tokenExtendedDataAimFlagCharacterSequence;

            // In some cases, the issue with dead keys may have affected the AIM Identifier. We need to check if the AIM
            // flag sequence is correct. If not, we change it and process or remove any prefix that has been recorded.
            string? ProcessPrefix() {
                if (string.IsNullOrEmpty(_tokenExtendedDataPrefix)) return deadKey;

                var aimIdentifierRegEx =
                    new Regex(
                        $@"^(?<prefix>.*)(?<characters>{deadKey}[A-Za-z][1-9A-Za-z])(?<code>.*)$",
                        RegexOptions.None);
                Match match;

                if (!Match1().Success) return deadKey;
                var observedPrefix = match.Groups["prefix"].Value;
                var observedCode = match.Groups["code"].Value;
                _tokenExtendedDataPrefix = string.IsNullOrEmpty(observedPrefix)
                    ? string.IsNullOrEmpty(observedCode)
                        ? string.Empty
                        : observedCode
                    : TestObservedPrefix();
                return deadKey;

                string TestObservedPrefix() {
                    _tokenExtendedDataCode = observedCode;
                    return observedPrefix;
                }

                Match Match1() =>
                    match = aimIdentifierRegEx.Match(_tokenExtendedDataPrefix);
            }
        }

        // Get the character used to split the data
        var splitKeyCharacter = data.Contains(deadKey ?? string.Empty, StringComparison.Ordinal)
                                    ? deadKey
                                    : TestForDeadKeyWithSpace();

        _tokenExtendedDataLigatureMap.Clear();

        var reportedCharacterList = new List<string>();

        /* Find a character that is not being used in the data. This will be used to temporarily create
         * characters to preserve sequences where an empty sequence is immediately followed by a dead-key
         * sequence. For example, consider the case where ` is a dead key and ` followed by a produces
         * the à character. If an empty sequence is followed by a sequence representing à, the following
         * is produced:
         *
         *      \0`\0à
         *
         * When split, this is parsed into a single sequence, rather than two, but the single sequence is
         * identical to what we need as the second sequence. There is no way to infer it is meant to be
         * two. To handle this, we will pre-process the data, inserting temporary characters to ensure
         * the splitting is correct.
         * */
        if (!data.TryUnusedExtendedAsciiCharacter(out var tempSpaceCharacter)) {
            // Error - The reported calibration data cannot be processed. No character can be determined
            // to act as a temporary delimiter.
            return (LogCalibrationInformation(
                InitializeTokenData(),
                InformationType.NoTemporaryDelimiterCandidate), suffix ?? string.Empty, endOfLine ?? string.Empty);
        }

        try {
            var splitChar = (splitKeyCharacter ?? throw new InvalidOperationException()).Last();
            var splitCharOut = _unescapedSplitChars.Contains(splitChar)
                                   ? $@"\{splitChar}"
                                   : splitChar.ToInvariantString();

            data = Regex.Replace(
                data,
                $"\0{splitCharOut}\0(?!{splitCharOut})",
                $"\0{splitChar}{tempSpaceCharacter}\0");
            data = Regex.Replace(
                data,
                $"(?<character>[^\u0000])\0(?<splitSequence>\0{splitCharOut}(?!(\0)))",
                "${character}${splitSequence}\0");
        }
        catch (InvalidOperationException) {
            // Error - The reported data is unrecognised. The wrong barcode may have been scanned.
            return (LogCalibrationInformation(
                InitializeTokenData(),
                InformationType.UnrecognisedData), suffix ?? string.Empty, endOfLine ?? string.Empty);
        }
        catch (ArgumentException argumentException) {
            throw new CalibrationException(
                $"{Resources.Calibration_Error_001} {Resources.Calibration_Error_002}",
                argumentException);
        }
        catch (RegexMatchTimeoutException regexMatchTimeoutException) {
            throw new CalibrationException(
                $"{Resources.Calibration_Error_001} {Resources.Calibration_Error_003}",
                regexMatchTimeoutException);
        }

        // Decrease the count of remaining calibration barcodes.
        --_tokenDataCalibrationsRemaining;

        // If no sequence types are detected, then return.
        return (!DetectSequenceTypes(
                   [.. data.Split([splitKeyCharacter], StringSplitOptions.None)],
                   reportedCharacterList,
                   tempSpaceCharacter)
                   ? InitializeTokenData()

                   // Process the reported character sequences.
                   : ProcessDeadKeyReportedCharacterList(token, reportedCharacterList), suffix ?? string.Empty, endOfLine ?? string.Empty);

        // Test for medium or low scanner performance.
        ScannerKeyboardPerformance TestForMediumOrLowPerformance() =>
            dataEntryTimeSpan.TotalMilliseconds < (double)ScannerKeyboardPerformance.Low
                ? ScannerKeyboardPerformance.Medium
                : ScannerKeyboardPerformance.Low;

        // Get the character used to split the data when deadKey character followed by space.
        string? TestForDeadKeyWithSpace() =>
            deadKey is { Length: >= 2 } &&
            data.Contains(deadKey[1] + "\u0020", StringComparison.Ordinal)
                ? deadKey[1] + "\u0020"
                : null;
    }

    /// <summary>
    ///   Converts a data string containing the reported contents of the baseline barcode to a list of segments, each
    ///   containing a list of character sequences.
    /// </summary>
    /// <param name="token">The calibration token.</param>
    /// <param name="data">The reported contents of the baseline barcode.</param>
    /// <param name="expectedSegments">The expected segments of character sequences.</param>
    /// <param name="reportedSegments">The reported segments of character sequences.</param>
    /// <param name="lineFeedChar">The reported character deemed to map to an LF character.</param>
    /// <returns>The amended calibration token.</returns>
    private Token ConvertToSegments(
        Token token,
        string data,
        IReadOnlyList<List<string>> expectedSegments,
        out List<List<string>> reportedSegments,
        out char lineFeedChar) {
        reportedSegments = [];

        // Strip off any trailing Carriage Return or Line Feed characters. We assume these have been added by the barcode scanner. Typically, this
        // is done as a 'suffix' setting in the barcode scanner configuration, but we won't treat these as normal reportable suffixes, although we
        // will report these separately.
        var strippedData = data.StripTrailingCrLfs(out var trailingCrLfChars);

        // Record if any trailing CR or LF characters were found. Configuring scanners to add these characters is generally beneficial as they allow
        // client software to detect the end of transmission without the need for timers.
        token = trailingCrLfChars.Length > 0
                    ? LogCalibrationInformation(
                        token,
                        InformationType.EndOfLineTransmitted,
                        trailingCrLfChars)
                    : LogCalibrationInformation(token, InformationType.EndOfLineNotTransmitted);

        // Set the reported character deemed to represent a line feed (LF) character. Return an ASCII 0 if no
        // such character is reported.
        lineFeedChar = trailingCrLfChars.Length > 0
            ? TestLineFeedChar()
            : '\0';

        data = strippedData;

        // Find a character that is not being used in the sequence. This will be used to temporarily indicate spaces returned by
        // barcode scanner dead keys.
        if (!data.TryUnusedExtendedAsciiCharacter(out var tempSpaceHolder)) {
            // Error - The reported calibration data cannot be processed. No character can be determined to act as a temporary delimiter.
            return LogCalibrationInformation(token, InformationType.NoTemporaryDelimiterCandidate);
        }

        /* If a dead key on the barcode scanner keyboard layout matches a dead key on the computer keyboard layout, and assuming that
         * the dead key character is not a ligature and that the next key 'pressed' on the scanner keyboard matches an assigned key on
         * the computer keyboard layout, the result will be an ASCII 0 followed by a character followed by a space, followed by either
         * a non-space character (first case), or a delimiter sequence of at least two spaces (second case). No space is consumed
         * because the barcode scanner types in an additional space to enter the literal dead key character.
         *
         * First, we need to detect all candidate reported dead keys for which the expected character is not entered using a dead key.
         * This is necessary because, if the key on the barcode scanner keyboard layout is not a dead key, this indicates a keyboard
         * mismatch which we need to report here.  Consider the case where the ' key on the scanner keyboard layout matches the ' dead
         * key on the computer keyboard layout.  If the ' key on the scanner keyboard layout is, itself, a dead key, the keyboard
         * layouts may well be identical.  If it is not a dead key, the keyboard layouts clearly do not match.
         * */
#if NET7_0_OR_GREATER
        var candidateNonMatches = NonMatchingDeadKeyComputerKeyboardCandidatesRegex().Matches(data);
        var matches = MatchingDeadKeyComputerKeyboardRegex().Matches(data);
#else
        var candidateNonMatches = NonMatchingDeadKeyComputerKeyboardCandidatesRegex.Matches(data);
        var matches = MatchingDeadKeyComputerKeyboardRegex.Matches(data);
#endif
        var candidateCount = candidateNonMatches.Count;

#pragma warning disable SA1312
        foreach (var _ in from Match candidate in candidateNonMatches
                 where matches.Any(m => m.Groups["a"].Value == candidate.Groups["a"].Value)
                          select new { }) {
            candidateCount--;
        }
#pragma warning restore SA1312

        if (candidateCount > 0) {
            // There are reported dead keys does that do not match the dead key on the computer keyboard layout,
            // and therefore the keyboard layouts do not match.
            token = LogCalibrationInformation(token, InformationType.NonCorrespondingKeyboardLayouts);
        }

        /* Next, for matching dead keys, we will replace the single space in the first case with a space holder. In the case
         * that the second key 'pressed' on the scanner maps to an unassigned key
         * */
#if NET7_0_OR_GREATER
        data = Case1TempSpaceHolderRegex().Replace(data, $"${{a}}{tempSpaceHolder}");
#else
        data = Case1TempSpaceHolderRegex.Replace(data, $"${{a}}{tempSpaceHolder}");
#endif

        /* Next, we will replace the first space in the second case. We need to preserve a single space in the last position,
         * but we are about to normalise all sequences of fours spaces to three. So we detect matched dead keys immediately
         * before a sequence of four spaces and replace the first space with a space holder.
         * */
#if NET7_0_OR_GREATER
        data = Case2TempSpaceHolderRegex().Replace(data, $"${{a}}{tempSpaceHolder}{new string('\u0020', 3)}");
#else
        data = Case2TempSpaceHolderRegex.Replace(data, $"${{a}}{tempSpaceHolder}{new string('\u0020', 3)}");
#endif

        // If a four-space segment delimiter is preceded by a dead key, a space may be lost. Normalize all the segment delimiters
        // by reducing all segment delimiters to three spaces. NB. Replace is lazy, not greedy.
        data = data.Replace(new string('\u0020', 4), new string('\u0020', 3), StringComparison.Ordinal);

        // Look for sequences of six or more spaces in multiples of three and mark each capture except the
        // last with the temporary space holder.
#if NET7_0_OR_GREATER
        data = ThreeSpaceTempSpaceHolderRegex().Replace(data, m => $"{m.Groups["c"]}{ReplacementWithAscii0(m.Groups["s"])}");
#else
        data = ThreeSpaceTempSpaceHolderRegex.Replace(data, m => $"{m.Groups["c"]}{ReplacementWithAscii0(m.Groups["s"])}");
#endif

        // If there are still any sequences of four spaces, convert the first space to a space holder.
        data = data.Replace(
            new string('\u0020', 4),
            $"{tempSpaceHolder}{new string('\u0020', 3)}",
            StringComparison.Ordinal);

        // Look for each occurrence of exactly two spaces and replace with a space holder and space.
#if NET7_0_OR_GREATER
        data = TwoSpaceTempSpaceHolderRegex().Replace(data, $"${{c}}{tempSpaceHolder}\u0020");
#else
        data = TwoSpaceTempSpaceHolderRegex.Replace(data, $"${{c}}{tempSpaceHolder}\u0020");
#endif

        // Split into segments around delimiter sequences of three spaces.
        var segments = data.Split([new string('\u0020', 3)], StringSplitOptions.None).ToList();

        // We should have at least two segments. If not, then the wrong barcode was scanned or the reported data is incorrect.
        // This test is not conclusive, but is likely to report any problems.
        if (segments.Count < 2) {
            // Error - The reported data is unrecognised. The wrong barcode may have been scanned.
            return LogCalibrationInformation(token, InformationType.UnrecognisedData);
        }

        // The use of prefixes can lead to edge conditions where the prefix is segmented prematurely. We should only do this check when the
        // expected reported prefix has been set. Fix up the segments if needed.
        if (!string.IsNullOrEmpty(_expectedReportedPrefix)) {
            FixUpExpectedReportedPrefix(segments, tempSpaceHolder);
        }

        // For ASCII control characters the temporary space holder may need to be stripped off.
        if (segments.Count > 2) {
            for (var idx = 2; idx < segments.Count; idx++) {
                var segment = segments[idx];

                if (string.IsNullOrEmpty(segment) || segment.Last() != tempSpaceHolder) {
                    continue;
                }

                if (idx >= 8) {
                    var addSpace = new string(' ', segment == tempSpaceHolder.ToString() ? 1 : 2);
                    segments[idx] = segment.Replace(tempSpaceHolder.ToString(), addSpace, StringComparison.Ordinal);
                    continue;
                }

                segments[idx] = segment[..^1];
            }
        }

        /* The calibration code assumes that all keyboards support the space character and do not map other characters to the
         * space bar or any other keys to a space character. Discounting the use of the space bar with dead keys, this
         * assumption might not be true, but we know of no examples. Any AIM identifier should not contain spaces (if the stated
         * assumptions hold), but there may be additional prefix characters provided by the barcode scanner which may contain spaces.
         * We are forced to assume (hope) this is not the case. We find the first occurrence of the space holder and a space.
         * This should mark the boundary between any prefix, including an AIM identifier, and the first data segment in the
         * barcode data.
         * */
        var prefixOffsetIndex = 0;

        if (!string.IsNullOrEmpty(_expectedReportedPrefix)) {
#if NET7_0_OR_GREATER
            var processedExpectedPrefix = TwoSpaceTempSpaceHolderRegex().Replace(
                _expectedReportedPrefix,
#else
            var processedExpectedPrefix = TwoSpaceTempSpaceHolderRegex.Replace(
                _expectedReportedPrefix,
#endif
                $"${{c}}{tempSpaceHolder}\u0020").TrimEnd();

            prefixOffsetIndex = segments[0].Contains(
                                    $"{processedExpectedPrefix}{tempSpaceHolder}",
                                    StringComparison.Ordinal)
                                    /* The expected reported prefix only needs to be set if the barcode scanner is configured to transmit a prefix that
                                     * includes one or more spaces. We have probably found the prefix, although this is not 100% reliable. We are
                                     * forced to take a best-endeavours approach here
                                     * */
                                    ? segments[0].IndexOf(
                                          $"{processedExpectedPrefix}{tempSpaceHolder}",
                                          StringComparison.Ordinal) + processedExpectedPrefix.Length
                                    : prefixOffsetIndex;
        }

        var headerDelimiterIdx = segments[0].IndexOf($"{tempSpaceHolder}\u0020", prefixOffsetIndex, StringComparison.Ordinal);

        if (headerDelimiterIdx < 0 && prefixOffsetIndex == 0) {
            // Error - The reported calibration data cannot be processed. It does not include expected delimiters.
            return LogCalibrationInformation(token, InformationType.NoDelimiters);
        }

        // If the first occurrence of the delimiter is the start of a run of spaces, Find the end of the run.
        var nextCharIdx = headerDelimiterIdx + 2;

        while (segments[0][nextCharIdx++] == '\u0020') {
            headerDelimiterIdx++;
        }

        /* We have probably found the initial delimiter between any prefix, including any AIM identifier, and the reported
         * barcode data. However, this is not absolutely certain. We have to assume this is the case. Any issues should be
         * detected later. We will start to build the list of reported segments.
         * */
        reportedSegments =
        [
            headerDelimiterIdx > 0
                ? [segments[0][..headerDelimiterIdx]]
                : [],

            // Add the first data segment (for invariant characters), splitting by space delimiters.
            [.. segments[0][(headerDelimiterIdx + 2) ..].Split('\u0020')],

            // Add the second data segment (for non-invariant characters), splitting by space delimiters.
            [.. segments[1].Split('\u0020')]
        ];

        // If we did not include tests for Format 06, then we need to insert some empty entries between the suffix and the rest of the data.
        if (!AssessFormatSupport) {
            var adjustedSegments = new string[segments.Count + 4];
            var toAscii28 = segments.Count - 1;
            segments.ToArray()[..toAscii28].CopyTo(adjustedSegments, 0);

            new[] { string.Empty, string.Empty, string.Empty, string.Empty }.CopyTo(adjustedSegments, toAscii28);
            segments.ToArray()[toAscii28..].CopyTo(adjustedSegments, toAscii28 + 4);

            segments = [.. adjustedSegments];
        }

        // Add any additional segments to sections, normalizing trailing sections into a single section. These segments are
        // for non-printing characters (ASCII 29, and the ASCII 28, 30, 31 amd 04) and any suffix provided by the barcode scanner.
        for (var segmentIdx = 2; segmentIdx < (segments.Count > 8 ? segments.Count : 8); segmentIdx++) {
            if (segmentIdx >= segments.Count) {
                // Pad the segment list with empty segments if necessary.
                reportedSegments.Add([]);
            }
            else if (segmentIdx > (int)Segments.EndOfTransmissionSegment) {
                // If the barcode scanner included a suffix that has split into segments, concatenate the into a single suffix segment.
                reportedSegments[(int)Segments.SuffixSegment][0] = reportedSegments[(int)Segments.SuffixSegment][0] += $"{new string(' ', 3)}{segments[segmentIdx]}";
            }
            else {
                // Add the segment.
                reportedSegments.Add([segments[segmentIdx]]);
            }
        }

        // Fix up the space holders in data segments.
        for (var segmentIdx = (int)Segments.PrefixSegment;
             segmentIdx <= (int)Segments.AdditionalAsciiSegment;
             segmentIdx++) {
            var sequences = reportedSegments[segmentIdx];
            var expectedSequences = expectedSegments[segmentIdx];

            var fixedUpReportedSegment = new List<string>();

            /* If the reported sequence contains too many items, we won't detect here, as this will be reported
             * after this method returns. In this case, there is a risk that the barcode scanner dead keys map may
             * contain incorrect key values, but this is not a problem, as calibration will fail more generally.
             * */

            // Iterate every sequence of characters in the segment.
            for (var sequenceIdx = 0; sequenceIdx < sequences.Count; sequenceIdx++) {
                // Replace the temporary space holder with a space.
                sequences[sequenceIdx] = sequences[sequenceIdx].Replace(tempSpaceHolder, '\u0020');

                /* Collect the barcode scanner dead keys, indicated by sequences that contain either a character followed by a
                 * space or an ASCII 0 followed by a character (where the dead key on the barcode scanner keyboard layout matches
                 * a dead key on the computer keyboard layout) or a space (where the dead key on the barcode scanner layout
                 * matches an unassigned key on the computer layout). These are reported character sequences that indicate
                 * that a character in the barcode is a dead key in the barcode scanner keyboard layout.
                 * */
#if NET7_0_OR_GREATER
                if (!BarcodeScannerDeadKeysFilter1Regex().Match(sequences[sequenceIdx]).Success)
#else
                if (!BarcodeScannerDeadKeysFilter1Regex.Match(sequences[sequenceIdx]).Success)
#endif
                {
                    sequences[sequenceIdx] = segmentIdx > 0
                        ? sequences[sequenceIdx].TrimEnd()
                        : sequences[sequenceIdx];

                    // If a dead key sequence is followed by an unrecognised character, it will be just two characters, and there is no need to
                    // process it. We only need to process longer sequences.
                    var sequence = sequences[sequenceIdx];
                    var splitSequence = SplitSequence(sequence);
                    fixedUpReportedSegment.AddRange(splitSequence);

                    for (var fixedUpSequenceIdx = sequenceIdx;
                         fixedUpSequenceIdx < sequenceIdx + splitSequence.Count;
                         fixedUpSequenceIdx++) {
#if NET7_0_OR_GREATER
                        if (!BarcodeScannerDeadKeysFilter2Regex().Match(fixedUpReportedSegment[fixedUpSequenceIdx]).Success)
#else
                        if (!BarcodeScannerDeadKeysFilter2Regex.Match(fixedUpReportedSegment[fixedUpSequenceIdx]).Success)
#endif
                        {
                            continue;
                        }

                        fixedUpReportedSegment[fixedUpSequenceIdx] =
                            fixedUpReportedSegment[fixedUpSequenceIdx].TrimEnd();
                        SetScannerDeadKey(fixedUpSequenceIdx);
                    }

                    continue;
                }

                fixedUpReportedSegment.Add(sequences[sequenceIdx]);
                var fixedUpIndex = fixedUpReportedSegment.Count - 1;

                if (expectedSequences.Count <= fixedUpIndex) {
                    continue;
                }

                if (sequenceIdx == sequences.Count - 1) {
                    fixedUpReportedSegment[fixedUpIndex] =
                        fixedUpReportedSegment[fixedUpIndex].TrimEnd();
                }

                SetScannerDeadKey(fixedUpIndex);
                continue;

                void SetScannerDeadKey(int idx) {
#if NET7_0_OR_GREATER
                    if (UnassignedKeysRegex().Match(fixedUpReportedSegment[idx]).Success)
#else
                    if (UnassignedKeysRegex.Match(fixedUpReportedSegment[idx]).Success)
#endif
                    {
                        if (!_tokenExtendedDataScannerUnassignedKeys.Contains($"{expectedSequences[idx][0]}")) {
                            _tokenExtendedDataScannerUnassignedKeys.Add($"{expectedSequences[idx][0]}");
                        }
                    }
                    else if (!_tokenExtendedDataScannerDeadKeysMap.ContainsKey($"{expectedSequences[idx][0]}")) {
                        _tokenExtendedDataScannerDeadKeysMap.Add(
                            $"{expectedSequences[idx][0]}",
                            fixedUpReportedSegment[idx]);
                    }
                }
            }

            reportedSegments[segmentIdx] = fixedUpReportedSegment;
        }

        // Replace temporary space holders in suffix and fix up.
        if (reportedSegments[(int)Segments.SuffixSegment].Count > 0
            && !string.IsNullOrEmpty(reportedSegments[(int)Segments.SuffixSegment][0])) {
            reportedSegments[(int)Segments.SuffixSegment][0] =
                reportedSegments[(int)Segments.SuffixSegment][0].Replace(tempSpaceHolder, '\u0020');
        }

        return token;

        /* We have to account for attempts to calibrate in environments that do not yield an ASCII 0
         * for key-down events that have no associated translated character. This could happen in certain
         * terminal applications, for example, and will often lead to failed calibration. If the
         * calibrator encounters dead keys on the computer keyboard layout, all bets are off. Calibration
         * is bound to fail, which is good. However, if there are no dead keys and the keyboard does not
         * support entry of all ASCII control characters (28 to 31 and 04), then we need to fix this up so that
         * calibration can succeed or fail correctly. Specifically, if no Group Separator is found, we
         * need to detect this correctly. Getting this right helps maximise the number of scenarios
         * calibration can be used in poorly behaved environments while avoiding incorrect calibration.
         * */
        string ReplacementWithAscii0(Group whitespace) {
            var output = new StringBuilder();

            foreach (var c in whitespace.Captures) {
                output.Append(c);
                output.Append(tempSpaceHolder);
            }

            return output.ToString()[..^1];
        }

        char TestLineFeedChar() {
            var lfIndex = trailingCrLfChars.LastIndexOf("LF", StringComparison.InvariantCulture);

            if (lfIndex >= 0 && lfIndex % 2 == 0) {
                return data[strippedData.Length + lfIndex / 2];
            }

            return '\0';
        }
    }

    /// <summary>
    ///   Process the segments of reported data.
    /// </summary>
    /// <param name="token">The calibration token.</param>
    /// <param name="reportedSegments">The list of segments of reported sequences.</param>
    /// <param name="expectedSegments">The list of segments fo expected sequences.</param>
    /// <param name="capsLock">Optional reported Caps Lock state.</param>
    /// <param name="platform">Operating system platform.</param>
    /// <param name="assessScript">Indicates whether the calibrator should assess the script for the configured OS keyboard.</param>
    private void ProcessReportedSegments(
        Token token,
        IList<List<string>> reportedSegments,
        IReadOnlyList<List<string>> expectedSegments,
        bool? capsLock,
        SupportedPlatform platform,
        bool assessScript) {
        var selectedCharacterKeyValuePairs = new List<KeyValuePair<char, char>>();
        var ambiguousLigatureStrings = new List<string>();
        CaseConversionCharacteristics? caseConversionCharacteristics = null;

        // We use the ProcessFlow monad here to represent the entire processing flow and logic at a
        // high level. The ProcessFlow expression depends on local functions within this method.
        _ = token.StartProcess()

            .Do(DetermineTheUnicodeBlockNameForTheKeyboardScript)
            .Do(ObtainTheSystemCaseConversionCharacteristics)

            .If(TheKeyboardScriptWasDetermined)
            .Then(RecordTheKeyboardScript)
            .EndIf

            .Do(RecordTheOperatingSystemPlatform)

            .If(CapsLockWasReportedToBeOn)
            .Then(DetermineTheBarcodeScannerCaseConversionBehaviourForCapsLockOn)
            .Else(DetermineTheBarcodeScannerCaseConversionBehaviourForCapsLockOffOrUnknown)
            .EndIf

            .Do(DetectAnyInvariantCharacterIssues)
            .Do(DetectAnyAdditionalAsciiCharacterIssues)
            .Do(DetectAnyCharacterCountMismatches)

            .If(TheScannerDidNotTransmitAnAimIdentifier)
            .Then(LogAWarningThatTheScannerDoesNotTransmitAimIdentifiers)
            .EndIf

            .If(AnyInvariantCharactersWereNotDetected)
            .Then(LogAnErrorThatSomeInvariantCharactersWereNotDetected)
            .EndIf

            .Do(DetectAnyNonInvariantCharacterCountMismatches)

            /* The data sequences are resolved. For each expected character,
             * there is a corresponding sequence in the correct segment.
             * */

            .Do(DetectUnrecognisedCharacters)

            .If(ThereIsASingleUnrecognisedCharacter)
            .Then(EncodeTheSingleUnrecognisedCharacter)
            .EndIf

            .Do(AddAsciiControlCharactersToTheReportedCharacterSet)

            // Process the resolved sequences of reported data.
            .Do(CreateEntriesForAllProcessedCharacters)
            .Do(CheckForAmbiguitiesBetweenCharacterAndLigatureMaps)

            .Do(DetermineIfKeyboardsMatch)

            .If(TheMatchBetweenTheScannerAndComputerKeyboardLayoutsWasDetermined)
            .Then(t1 => t1.Continue()
                .Do(ResolveAnyKeyboardMismatches)
                .If(TheScannerAndComputerKeyboardLayoutsDoNotMatch)
                .Then(t2 => t2.Continue()
                    .Do(LogNonCorrespondingKeyboardLayouts)

                    .If(MismatchesWereDetectedForInvariantCharacters)
                    .Then(LogNonCorrespondingKeyboardLayoutsForInvariantCharacters)
                    .EndIf

                    .If(MismatchesWereDetectedForAdditionalAsciiCharacters)
                    .Then(LogNonCorrespondingKeyboardLayoutsForAdditionalAsciiCharacters)
                    .EndIf)
                .EndIf)
            .Else(LogNonDeterminationOfKeyboardLayoutCorrespondence)
            .EndIf

            .Do(RetrieveTheDeduplicatedCharacterMap)
            .Do(DetermineAnyIssuesForAsciiControlCharacters)
            .Do(CheckForAnyMissedAmbiguities)
            .Do(DetermineCompatibilityWithFormat05AndFormat06Barcodes)
            .Do(DetermineCompatibilityWithEdiBarcodes)
            .Do(DetermineReadibilityOfFs)
            .Do(DetermineReadibilityOfUs)
            .Do(DetermineReadibilityOfEot)

            .If(AmbiguousLigatureStringsWereDetected)
            .Then(LogTheAmbiguousLigatureCharacters)
            .EndIf

            .Do(InitializeFromTokenData)

            .End();

        // Local functions

        // Gets a string containing each ambiguous ligature character.
        string AmbiguousLigatureText() =>
            new string(
                    (from c in ambiguousLigatureStrings.Aggregate((s1, s2) => $"{s1}{s2}")
                     select c)
                    .Distinct()
                    .ToArray())
                .Aggregate(
                    string.Empty,
                    (current, ambiguousCharacter) =>
                        current + $"{ambiguousCharacter}, ");

        // Log information.
        EnvToken LogInformation(
            Token localToken,
            InformationType informationType,
            string data = "") =>
            () =>
                new Lazy<Token>(
                    LogCalibrationInformation(
                        localToken,
                        informationType,
                        data));

        // Determine the Unicode block name for the script represented by the OS-configured keyboard layout.
        EnvToken DetermineTheUnicodeBlockNameForTheKeyboardScript(Token localToken) =>
            () => {
                _tokenExtendedDataKeyboardScript = assessScript
                    ? ResolveKeyboardScript(reportedSegments[(int)Segments.InvariantSegment], capsLock.GetValueOrDefault())
                    : "<unknown>";

                return new Lazy<Token>(localToken);
            };

        // Obtain the computed case conversion characteristics of the system based on observations.
        EnvToken ObtainTheSystemCaseConversionCharacteristics(Token localToken) =>
            () => {
                caseConversionCharacteristics = new CaseConversionCharacteristics(
                    reportedSegments[(int)Segments.InvariantSegment],
                    _tokenExtendedDataKeyboardScript ?? "Latin",
                    capsLock);

                return new Lazy<Token>(localToken);
            };

        // Log information - The computer keyboard supports the following script: {0}
        EnvToken RecordTheKeyboardScript(Token localToken) =>
            LogInformation(
                localToken,
                InformationType.KeyboardScript,
                _tokenExtendedDataKeyboardScript ?? "<unknown>");

        // Log Information - The computer keyboard is configured for the following platform: {0}
        EnvToken RecordTheOperatingSystemPlatform(Token localToken) =>
            LogInformation(
                localToken,
                InformationType.Platform,
                platform.ToString());

        // Log warning - Caps Lock may be switched on.
        EnvToken LogCapsLockProbablyOn(Token localToken) =>
            LogInformation(
                localToken,
                InformationType.CapsLockProbablyOn);

        // Infer the case conversion characteristics of the scanner and keyboard.
        EnvToken DetermineTheBarcodeScannerCaseConversionBehaviourForCapsLockOn(Token localToken) =>
            () => new Lazy<Token>(
                InferCaseConversionCapsLockOn(
                    localToken,
                    caseConversionCharacteristics!,
                    platform,
                    LogCapsLockProbablyOn));

        // Infer the case conversion characteristics of the scanner and keyboard.
        EnvToken DetermineTheBarcodeScannerCaseConversionBehaviourForCapsLockOffOrUnknown(Token localToken) =>
            () => new Lazy<Token>(
                InferCaseConversionCapsLockOffOrUnknown(
                    localToken,
                    caseConversionCharacteristics!,
                    platform,
                    capsLock,
                    LogCapsLockProbablyOn));

        // Test invariant characters for issues.
        EnvToken DetectAnyInvariantCharacterIssues(Token localToken) =>
            () => new Lazy<Token>(
                DetectCharacterIssues(localToken, reportedSegments, expectedSegments, 1, true));

        // Test additional characters for issues.
        EnvToken DetectAnyAdditionalAsciiCharacterIssues(Token localToken) =>
            () => new Lazy<Token>(
                DetectCharacterIssues(localToken, reportedSegments, expectedSegments, 2));

        // Compare the counts of reported and expected characters.
        EnvToken DetectAnyCharacterCountMismatches(Token localToken) =>
            () => new Lazy<Token>(
                reportedSegments[(int)Segments.InvariantSegment].Count switch {
                    var count when expectedSegments[(int)Segments.InvariantSegment].Count == count =>

                        // Detect differences between reported and expected characters
                        DetectDifferences(
                            localToken,
                            reportedSegments[(int)Segments.InvariantSegment],
                            expectedSegments[(int)Segments.InvariantSegment],
                            true,
                            out _processedInvariantCharacters),
                    var count when expectedSegments[(int)Segments.InvariantSegment].Count > count =>

                        // Error - Some invariant characters cannot be detected.
                        LogCalibrationInformation(
                            localToken,
                            InformationType.UndetectedInvariantCharacters),
                    _ =>

                        // Error - Too many characters detected. The wrong barcode may have been scanned.
                        LogCalibrationInformation(
                            localToken,
                            InformationType.TooManyCharactersDetected)
                });

        // Log warning - The barcode scanner is not transmitting AIM Identifiers.
        EnvToken LogAWarningThatTheScannerDoesNotTransmitAimIdentifiers(Token localToken) =>
            LogInformation(localToken, InformationType.AimNotTransmitted);

        // Initialise the processed invariant characters and return the token.
        EnvToken InitialiseProcessedInvariantCharacters(Token localToken) =>
            () => {
                _processedInvariantCharacters = new Dictionary<char, char>();
                return new Lazy<Token>(localToken);
            };

        // Log error - Some invariant characters cannot be detected.
        EnvToken LogAnErrorThatSomeInvariantCharactersWereNotDetected(Token localToken) =>
            LogInformation(
                InitialiseProcessedInvariantCharacters(localToken)().Value,
                InformationType.UndetectedInvariantCharacters);

        // Compare the counts of reported and expected non-invariant characters.
        EnvToken DetectAnyNonInvariantCharacterCountMismatches(Token localToken) =>
            () => new Lazy<Token>(
                reportedSegments[(int)Segments.AdditionalAsciiSegment].Count switch {
                    var count when expectedSegments[(int)Segments.AdditionalAsciiSegment].Count == count =>
                        DetectDifferences(
                            new Token(localToken),
                            reportedSegments[(int)Segments.AdditionalAsciiSegment],
                            expectedSegments[(int)Segments.AdditionalAsciiSegment],
                            false,
                            out _processedNonInvariantCharacters),
                    var count when expectedSegments[(int)Segments.AdditionalAsciiSegment].Count > count =>

                        // Warning - Cannot detect all non-invariant characters.
                        LogCalibrationInformation(
                            localToken,
                            InformationType.SomeNonInvariantCharactersUnreported),
                    _ =>

                        // Error - Too many characters detected. The wrong barcode may have been scanned.
                        LogCalibrationInformation(
                            localToken,
                            InformationType.TooManyCharactersDetected)
                });

        // Detect unrecognised characters.
        EnvToken DetectUnrecognisedCharacters(Token localToken) =>
            () => {
                _tokenExtendedDataUnrecognisedKeys =
                    _processedInvariantCharacters?
                        .Where(m => m.Key == '\0')
                        .Select(m => m.Value)
                        .Concat(
                            _processedNonInvariantCharacters?
                                .Where(m => m.Key == '\0')
                                .Select(m => m.Value) ?? [])
                        .ToList() ?? [];

                return new Lazy<Token>(localToken);
            };

        // A single unrecognised character can be encoded successfully.
        EnvToken EncodeTheSingleUnrecognisedCharacter(Token localToken) =>
            () => {
                _tokenExtendedDataCharacterMap.Add(new KeyValuePair<char, char>(
                    '\0',
                    _tokenExtendedDataUnrecognisedKeys[0]));

                return new Lazy<Token>(localToken);
            };

        // Add ASCII control characters to the reported character set.
        EnvToken AddAsciiControlCharactersToTheReportedCharacterSet(Token localToken) =>
            () => {
                _tokenExtendedDataReportedCharacters += "\0\u001C\u001D\u001E\u001F\u0004";
                return new Lazy<Token>(localToken);
            };

        // Create a collection of dictionary entries for all processed characters.
        EnvToken CreateEntriesForAllProcessedCharacters(Token localToken) =>
            () => {
                // Create a collection of dictionary entries for all processed characters.
                selectedCharacterKeyValuePairs =
                    (_processedInvariantCharacters ?? new Dictionary<char, char>())
                    .Where(m => m.Key != '\0')
                    .Select(m => m)
                    .Concat(
                        (_processedNonInvariantCharacters ?? new Dictionary<char, char>())
                        .Where(m => m.Key != '\0')
                        .Select(m => m)).ToList();

                return new Lazy<Token>(localToken);
            };

        // Check for ambiguities between the character map and the ligature map.
        EnvToken CheckForAmbiguitiesBetweenCharacterAndLigatureMaps(Token localToken) =>
            () => {
                ambiguousLigatureStrings =
                    (from k in
                         from key in _tokenExtendedDataLigatureMap.Keys
                         select key
                     where k.All(c => _tokenExtendedDataCharacterMap.ContainsKey(c))
                     select k).ToList<string>();

                return new Lazy<Token>(localToken);
            };

        // Set an indicator in the token representing if the barcode scanner and
        // computer keyboards are in correspondence.
        EnvToken DetermineIfKeyboardsMatch(Token localToken) =>
            () =>
                new Lazy<Token>(
                    DoDetermineIfKeyboardsMatch(
                        localToken,
                        caseConversionCharacteristics!));

        // Log information that the scanner and operating system keyboard layouts do not match.
        EnvToken LogNonCorrespondingKeyboardLayouts(Token localToken) =>

            // Warning - The barcode scanner and computer keyboard layouts do not correspond.
            LogInformation(
                localToken,
                InformationType.NonCorrespondingKeyboardLayouts);

        // Log a warning that the barcode scanner and computer keyboard layouts do not correspond
        // for invariant characters.
        EnvToken LogNonCorrespondingKeyboardLayoutsForInvariantCharacters(Token localToken) =>
            LogInformation(
                localToken,
                InformationType.NonCorrespondingKeyboardLayoutsForInvariants);

        // Log a warning that the barcode scanner and computer keyboard layouts do not correspond
        // for non-invariant characters.
        EnvToken LogNonCorrespondingKeyboardLayoutsForAdditionalAsciiCharacters(Token localToken) =>
            LogInformation(
                localToken,
                InformationType.NonCorrespondingKeyboardLayoutsForNonInvariantCharacters);

        // Log information that the scanner and operating system keyboard layouts do not match.
        EnvToken LogNonDeterminationOfKeyboardLayoutCorrespondence(Token localToken) =>

            // Warning - The correspondence of the barcode scanner and computer keyboard layouts
            // cannot be determined.
            LogInformation(
                localToken,
                InformationType.NonDeterminableKeyboardLayoutCorrespondence);

        // Resolve keyboard mismatches for dead keys, determining if they affect invariant or
        // non-invariant characters.
        EnvToken ResolveAnyKeyboardMismatches(Token localToken) =>
            () =>
                new Lazy<Token>(ResolveUnspecifiedKeyboardMismatches(localToken));

        // Gets a de-duplicated character mapping between
        // reported and expected characters.
        EnvToken RetrieveTheDeduplicatedCharacterMap(Token localToken) =>
            () =>
                new Lazy<Token>(
                    CreateDeduplicatedCharacterMap(
                        localToken,
                        selectedCharacterKeyValuePairs));

        // Process the reported data to determine any issues with ASCII control characters (ASCII 04, 29, 28, 30 and 31).
        EnvToken DetermineAnyIssuesForAsciiControlCharacters(Token localToken) =>
            () =>
                new Lazy<Token>(
                    DoProcessAsciiControlCharacters(localToken, reportedSegments, expectedSegments));

        // Check for missed ambiguities
        EnvToken CheckForAnyMissedAmbiguities(Token localToken) =>
            () =>
                new Lazy<Token>(DoProcessMissedAmbiguities(localToken));

        // Check for Format 05 and 06 incompatibility
        EnvToken DetermineCompatibilityWithFormat05AndFormat06Barcodes(Token localToken) =>
            () =>
                new Lazy<Token>(DoProcessForIsoIec15434MessageHeaderIncompatibility(localToken));

        // Check for EDI incompatibility
        EnvToken DetermineCompatibilityWithEdiBarcodes(Token localToken) =>
            () =>
                new Lazy<Token>(DoProcessForIsoIec15434EdiIncompatibility(localToken));

        // Check for FS characters
        EnvToken DetermineReadibilityOfFs(Token localToken) =>
            () =>
                new Lazy<Token>(DoProcessForIsoIec15434FsIncompatibility(localToken));

        // Check for US characters
        EnvToken DetermineReadibilityOfUs(Token localToken) =>
            () =>
                new Lazy<Token>(DoProcessForIsoIec15434UsIncompatibility(localToken));

        // Check for EOT characters
        EnvToken DetermineReadibilityOfEot(Token localToken) =>
            () =>
                new Lazy<Token>(DoProcessForIsoIec15434EotIncompatibility(localToken));

        EnvToken LogTheAmbiguousLigatureCharacters(Token localToken) =>

            // Error - Some reported characters are ambiguous. They can be reported individually
            // but are also used to compose ligatures: {0}
            LogInformation(
                localToken,
                InformationType.LigatureCharacters,
                AmbiguousLigatureText()[..^2].ToControlPictures());

        // Initialise the calibrator from the processed calibration token.
        EnvToken InitializeFromTokenData(Token localToken) =>
            () =>
                new Lazy<Token>(DoInitializeFromTokenData(localToken));

        // Test to see if a keyboard script was inferred.
        bool TheKeyboardScriptWasDetermined(Token localToken)
            => !string.IsNullOrWhiteSpace(_tokenExtendedDataKeyboardScript);

        // Test to see if Caps Lock is reported to be on.
        bool CapsLockWasReportedToBeOn(Token localToken)
            => capsLock.GetValueOrDefault();

        // Test to see if the scanner is not transmitting an AIM identifier
        bool TheScannerDidNotTransmitAnAimIdentifier(Token localToken)
            => reportedSegments[(int)Segments.PrefixSegment].Count == 0;

        // Test to see if there are any undetected invariant characters.
        bool AnyInvariantCharactersWereNotDetected(Token localToken)
            => _processedInvariantCharacters is null;

        bool ThereIsASingleUnrecognisedCharacter(Token localToken)
            => _tokenExtendedDataUnrecognisedKeys.Count == 1;

        // Test to see if the match or mismatch of the scanner and keyboard layouts was determined.
        bool TheMatchBetweenTheScannerAndComputerKeyboardLayoutsWasDetermined(Token localToken) =>
            localToken.KeyboardMatch.HasValue;

        // Test to see if the match or mismatch of the scanner and keyboard layouts do not match.
        bool TheScannerAndComputerKeyboardLayoutsDoNotMatch(Token localToken) =>
            localToken.KeyboardMatch == false;

        // Test to see if any character mismatches were detected for invariant characters.
        bool MismatchesWereDetectedForInvariantCharacters(Token localToken) =>
            _processedInvariantCharacters?.Count > 0;

        // Test to see if any character mismatches were detected for non-invariant characters.
        bool MismatchesWereDetectedForAdditionalAsciiCharacters(Token localToken) =>
            _processedNonInvariantCharacters?.Count > 0;

        // ReSharper restore BadParensLineBreaks

        // Test to see if any ambiguous ligature strings were detected.
        bool AmbiguousLigatureStringsWereDetected(Token localToken) =>
            ambiguousLigatureStrings.Count != 0;
    }

    /// <summary>
    ///   Process the reported character sequences for a DeadKey barcode.
    /// </summary>
    /// <param name="token">The current calibration token.</param>
    /// <param name="reportedCharacterList">The reported character sequences.</param>
    /// <returns>The calibration token.</returns>
    private Token ProcessDeadKeyReportedCharacterList(Token token, IReadOnlyList<string> reportedCharacterList) {
        // Flag to record a first error, where we only fail on a second.
        var firstError = false;

        // The sequence of characters that caused the first error.
        var firstErrorSequence = string.Empty;

        // We found the same number of reported characters as ASCII characters, corresponding to the barcode that
        // contains a dead key combination with each ASCII character.
        for (var reportedCharacterIdx = 0; reportedCharacterIdx < reportedCharacterList.Count; reportedCharacterIdx++) {
            var reportedCharacterSequence = reportedCharacterList[reportedCharacterIdx];

            switch (reportedCharacterSequence) {
                case { Length: 2 }:
                    var expectedCharacter = AsciiChars[reportedCharacterIdx].ToInvariantString();

                    switch (reportedCharacterSequence) {
                        case not null when reportedCharacterSequence[1] == '\u0020'
                                           && !_tokenExtendedDataScannerDeadKeysMap.ContainsKey(expectedCharacter):

                            // We have detected a barcode scanner dead key that maps to a keyboard dead key or an unassigned key.
                            // These should have already been recorded, but we will collect if they don't exist.
                            _tokenExtendedDataScannerDeadKeysMap.Add(
                                expectedCharacter,
                                reportedCharacterSequence);
                            break;
                        case not null when reportedCharacterSequence[0] == '\u0000':

                            // Have we already detected the dead key sequence?
                            if (_tokenExtendedDataDeadKeysMap.ContainsKey(reportedCharacterSequence)) {
                                var expectedSequence = _tokenExtendedDataDeadKeysMap.First(kvp => kvp.Key == reportedCharacterSequence).Value;

                                // Determine if the first dead key maps to a combination of invariant characters.
#if NET7_0_OR_GREATER
                                if (InvariantsMatchRegex().IsMatch(expectedSequence)) {
#else
                                if (InvariantsMatchRegex.IsMatch(expectedSequence)) {
#endif
                                    // Determine if the current dead key maps to a combination of invariant characters.
#if NET7_0_OR_GREATER
                                    if (InvariantsMatchRegex().IsMatch(_tokenDataValue + expectedCharacter))
#else
                                    if (InvariantsMatchRegex.IsMatch(_tokenDataValue + expectedCharacter))
#endif
                                    {
                                        // Error - The reported character {0} is ambiguous. The same character is reported for
                                        // multiple dead key sequences representing different expected characters.
                                        return LogCalibrationInformation(
                                            InitializeTokenData(),
                                            InformationType.DeadKeyMultiMapping,
                                            reportedCharacterSequence.ToControlPictures(),
                                            $"{expectedSequence} {_tokenDataValue.ToControlPictureString()}{expectedCharacter[0].ToControlPicture()}");
                                    }

                                    // Warning: Some reported characters are ambiguous. However, these characters do not
                                    // represent invariant characters: {0}
                                    token = LogCalibrationInformation(
                                        InitializeTokenData(),
                                        InformationType.DeadKeyMultiMappingNonInvariantCharacters,
                                        reportedCharacterSequence.ToControlPictures(),
                                        $"{expectedSequence} {_tokenDataValue.ToControlPictureString()}{expectedCharacter[0].ToControlPicture()}");
                                }
                                else {
                                    // Determine if the current dead key maps to a combination of invariant characters.
#if NET7_0_OR_GREATER
                                    if (InvariantsMatchRegex().IsMatch(_tokenDataValue + expectedCharacter))
#else
                                    if (InvariantsMatchRegex.IsMatch(_tokenDataValue + expectedCharacter))
#endif
                                    {
                                        // We will remove the first dead key mapping and replace with the second
                                        _tokenExtendedDataDeadKeysMap.Remove(reportedCharacterSequence);
                                        _tokenExtendedDataDeadKeysMap.Add(
                                            reportedCharacterSequence,
                                            token.Data?.Value + expectedCharacter);
                                    }

                                    // Warning: Some reported characters are ambiguous. However, these characters do not
                                    // represent invariant characters: {0}
                                    token = LogCalibrationInformation(
                                        InitializeTokenData(),
                                        InformationType.DeadKeyMultiMappingNonInvariantCharacters,
                                        reportedCharacterSequence.ToControlPictures(),
                                        $"{expectedSequence} {_tokenDataValue.ToControlPictureString()}{expectedCharacter[0].ToControlPicture()}");
                                }
                            }
                            else {
                                /* Some barcode scanner keyboard configurations do not support ASCII 29 (Group Separator) control
                                 * characters. These are used as field delimiters in both GS1 and ANSI MH 10.8.2 barcodes.
                                 * If the client knows a key has been pressed (which it normally would), this can be
                                 * represented as an ASCII 0. In this case, it may still be possible to read barcodes by
                                 * inferring that the reported ASCII 0 character represents an ASCII 29, but only if this can
                                 * be done without ambiguity.
                                 * */
                                if (_tokenExtendedDataCharacterMap.TryGetValue('\0', out var value) && value == '\u001D') {
                                    // Is there a potential conflict with the character map?
                                    if (_tokenExtendedDataCharacterMap.TryGetValue(reportedCharacterSequence[1], out var conflictingExpectedCharacter)) {
                                        // Return the reported dead key character for an expected character.
                                        char ReportedDeadKeyCharacter(char deadKeyExpectedCharacter) {
                                            var dkExpectedCharacterString = deadKeyExpectedCharacter.ToInvariantString();

                                            if (_tokenExtendedDataDeadKeysMap.Values.Contains(dkExpectedCharacterString)) {
                                                return (from kvp in _tokenExtendedDataDeadKeysMap
                                                        where kvp.Value == dkExpectedCharacterString
                                                        select kvp.Key).First()[1];
                                            }

                                            return char.MinValue;
                                        }

                                        var reportedSequence =
                                            $"{ReportedDeadKeyCharacter(_tokenDataValue)}{reportedCharacterSequence[1].ToControlPictureString()}";
                                        var expectedCharacterReportedAsDeadKey = _tokenDataValue.ToControlPictureString();
                                        var expectedSequence =
                                            $"{expectedCharacterReportedAsDeadKey}{'\u001D'.ToControlPictureString()}{conflictingExpectedCharacter} {expectedCharacterReportedAsDeadKey}{conflictingExpectedCharacter}";
                                        var ambiguousInvariant =
#if NET7_0_OR_GREATER
                                            InvariantsMatchRegex().IsMatch(conflictingExpectedCharacter.ToInvariantString());
#else
                                            InvariantsMatchRegex.IsMatch(conflictingExpectedCharacter.ToInvariantString());
#endif

                                        /* ASCII 29 characters are not being emitted. This is only treated as an error if the conflict
                                         * maps to the first character of a data element identifier for a given syntax (GS1 AIs or ASC MH
                                         * 10.8.2 DIs) provided via the RecognisedDataElements property, or if no data elements are specified
                                         * via the RecognisedDataElements property. Otherwise, if a list of reported data elements is provided,
                                         * but the conflict does mot map to the first character of any stated data element identifiers,
                                         * a warning is provided stating that it may not be possible to reliably read barcodes containing
                                         * other data elements.
                                         *
                                         * Note that when a dead key is pressed and the following character (in this case, a <GS>)
                                         * is not supported by the OS keyboard layout, no input occurs until the next character is
                                         * entered. However, the client can still detect the key-press events generated by the
                                         * scanner and report a character. In this case, we are handling the generation of an
                                         * ASCII 0 to represent the ASCII 29. The ASCII 0 (for the <GS>) will be followed by a
                                         * dead key sequence. So, if %0 in the barcode maps to \0^) in the reported data, %␝0 will
                                         * map to \0\0^), causing the ambiguity. The first ASCII 0 in the reported sequence
                                         * actually represents the <GS>, so in effect, %␝0 is reported as if it was ␝%0. While
                                         * this makes no difference in this scenario, it is an example of the way that characters
                                         * can logically be reported out of sequence in certain dead key scenarios.
                                         * */
                                        if (RecognisedDataElements is { Count: > 0 })
                                        {
                                            if (RecognisedDataElements.Any(e =>
                                                    conflictingExpectedCharacter ==
                                                    e.Identifier[0]))
                                            {
                                                // Error: The reported character sequence {0} is ambiguous. This represents the group separator character.
                                                return LogCalibrationInformation(
                                                InitializeTokenData(),
                                                InformationType.GroupSeparatorNotReliablyReadableInvariant,
                                                reportedSequence,
                                                expectedSequence);
                                            }

                                            if (!ambiguousInvariant) {
                                                // Warning: The reported character sequence {0} is ambiguous. This may prevent reading of any additional data
                                                // elements included in a barcode.
                                                token = LogCalibrationInformation(
                                                    InitializeTokenData(),
                                                    InformationType.ControlCharacterMappingNonInvariants,
                                                    reportedSequence,
                                                    expectedSequence);
                                            }
                                        }
                                        else {
                                            // Error: The reported character sequence {0} is ambiguous. This represents the group separator character.
                                            return LogCalibrationInformation(
                                                InitializeTokenData(),
                                                ambiguousInvariant
                                                    ? InformationType.GroupSeparatorNotReliablyReadableInvariant
                                                    : InformationType.ControlCharacterMappingNonInvariants,
                                                reportedSequence,
                                                expectedSequence);
                                        }
                                    }

                                    /* There is no conflict with the character map. However, there is a conflict if the reported
                                     * character sequence is composed of an ASCII 0, representing the dead key, and the first
                                     * character of a data element identifier for a given syntax (GS1 AIs or ASC MH 10.8.2 DIs)
                                     * provided via the ReportedDataElements property. Because ASCII 29 characters are reported
                                     * as ASCII 0, the data for these elements cannot be read reliably.
                                     * */
                                    else if (RecognisedDataElements.Any(e =>
                                                 reportedCharacterSequence[1] ==
                                                 e.Identifier[0])) {
                                        var ambiguousInvariant =
#if NET7_0_OR_GREATER
                                            InvariantsMatchRegex().IsMatch(expectedCharacter[0].ToInvariantString());
#else
                                            InvariantsMatchRegex.IsMatch(expectedCharacter[0].ToInvariantString());
#endif
                                        // Error: The reported character sequence {0} is ambiguous. This represents the group separator character.
                                        return LogCalibrationInformation(
                                            InitializeTokenData(),
                                            InformationType.GroupSeparatorNotReliablyReadableInvariant,
                                            reportedCharacterSequence.ToControlPictures(),
                                            $"{_tokenDataValue.ToControlPictureString()}{expectedCharacter.ToControlPictures()} {'\u001D'.ToControlPictureString()}{reportedCharacterSequence[1].ToControlPictureString()}");
                                    }

                                    // Ambiguities concerning control characters may prevent reliable reading of
                                    // ISO/IEC 15434 barcodes or barcodes that contain other data. NB., For
                                    // non-invariant characters, this generally implies non-ASCII characters
                                    // because dead keys are generally used to input non-ASCII characters. In many
                                    // cases, we may know that the scanner keyboard layout does not have a key for
                                    // such a character, but that's OK because the character could still be entered
                                    // using a numeric keyboard mode.
                                    else {
                                        // Warning: The reported character sequence {0} is ambiguous. This may prevent reading of any additional data elements
                                        // included in a barcode.
                                        token = LogCalibrationInformation(
                                            InitializeTokenData(),
                                            InformationType.ControlCharacterMappingNonInvariants,
                                            reportedCharacterSequence.ToControlPictures(),
                                            $"{_tokenDataValue.ToControlPictureString()}{expectedCharacter[0].ToControlPictureString()} {'\u001D'.ToControlPictureString()}{reportedCharacterSequence[1].ToControlPictureString()}");
                                    }
                                }

                                // If ASCII 30 characters are not being emitted, test if the
                                // barcode can be read reliably.
                                token = DetermineIfFormat0605UnreadableWhenNoAscii30(
                                    token,
                                    reportedCharacterSequence.ToControlPictures(),
                                    expectedCharacter[0].ToControlPictureString());

                                // If ASCII 38 and/or ASCII 31 characters are not being emitted,
                                // test if EDI barcodes can be read reliably.
                                token = AssessFormatSupport
                                    ? DetermineIfEdiUnreadableWhenNoAscii28Or31(
                                        token,
                                        reportedCharacterSequence.ToControlPictures(),
                                        expectedCharacter[0].ToControlPictureString())
                                    : token;

                                _tokenExtendedDataDeadKeysMap.Add(
                                    reportedCharacterSequence,
                                    token.Data?.Value + expectedCharacter);
                            }

                            break;
                    }

                    break;
                case { Length: 0 }: {
                        // The character cannot be recognised.
                        var unrecognisedCharacter = AsciiChars[reportedCharacterIdx].ToInvariantString();

                        // Test to see if the character that maps to the dead key and unrecognised character are both invariant.
#if NET7_0_OR_GREATER
                        if (InvariantsMatchRegex().IsMatch(token.Data?.Value.ToInvariantString() + unrecognisedCharacter))
#else
                        if (InvariantsMatchRegex.IsMatch(token.Data?.Value.ToInvariantString() + unrecognisedCharacter))
#endif
                        {
                            /* For keyboards that modify the space bar to produce a character that is not the literal dead
                             * key character (e.g., produce " instead of ¨), this may not be an issue. We will ignore,
                             * if the unrecognised character has been detected as a dead key.
                             * */
                            if (_tokenExtendedDataDeadKeyCharacterMap.ContainsKey("\0" + unrecognisedCharacter)) {
                                break;
                            }

                            /* The dead key calibration barcode is used to test combinations of the characters that map to
                             * dead keys on the OS keyboard with other characters. In this case the second character is an
                             * invariant character. If this character occurs in combination with the character that maps to
                             * the dead key, the data cannot be read. However, we can infer the unrecognised character if
                             * this is the only occurrence of a dead-key combination in which the second character is
                             * unrecognised.
                             * */
                            if (firstError || _tokenExtendedDataDeadKeysMap.ContainsKey(reportedCharacterSequence)) {
                                // Error - Some key combinations that include invariant characters are not recognised: {0}
#pragma warning disable SA1118 // Parameter should not span multiple lines
                                return LogCalibrationInformation(
                                    InitializeTokenData(),
                                    InformationType.SomeDeadKeyCombinationsUnrecognisedForInvariants,
                                    null,
                                    (string.IsNullOrWhiteSpace(firstErrorSequence)
                                        ? string.Empty
                                        : firstErrorSequence.ToControlPictures() + " ") +
                                    _tokenDataValue.ToControlPicture() +
                                    unrecognisedCharacter[0].ToControlPictureString());
#pragma warning restore SA1118 // Parameter should not span multiple lines
                            }

                            firstError = true;
                            firstErrorSequence = _tokenDataValue + unrecognisedCharacter;

                            _tokenExtendedDataDeadKeysMap.Add(reportedCharacterSequence, unrecognisedCharacter);
                        }
                        else {
                            // Warning - Some combinations of non-invariant characters are not recognised: {0}
                            token = LogCalibrationInformation(
                                InitializeTokenData(),
                                InformationType.SomeNonInvariantCharacterCombinationsUnrecognised,
                                null,
                                _tokenDataValue.ToControlPictureString() + unrecognisedCharacter[0].ToControlPictureString());
                        }

                        break;
                    }
            }
        }

        return token;
    }

    /// <summary>
    ///   Determine if a Format 05 or 06 barcode is unreadable when
    ///   the scanner does not report an ASCII 30.
    /// </summary>
    /// <param name="token">The current calibration token.</param>
    /// <param name="reportedCharacterSequence">A reported character sequence.</param>
    /// <param name="expectedCharacter">The expected character.</param>
    /// <returns>The calibration token.</returns>
    private Token DetermineIfFormat0605UnreadableWhenNoAscii30(
        Token token,
        string reportedCharacterSequence,
        string expectedCharacter) {
        // If ASCII 30 characters are not being emitted, there may be a warning.
        return _tokenExtendedDataPotentialIsoIec15434Unreadable30

                   // Is there a potential conflict with the character map?
                   ? TestPotentialConflictWithCharacterMap()
                   : token;

        // Test if the reported sequence is \00 and the character map does not map
        // a reported 0 to another character.
        Token TestIfDeadKeySequence0NoMap() =>
            reportedCharacterSequence[1] == 48

                // Warning - Barcodes that use ISO/IEC 15434 syntax cannot be recognised.
                ? AssessFormatSupport
                    ? LogCalibrationInformation(
                        InitializeTokenData(),
                        InformationType.IsoIec15434SyntaxNotRecognised)
                    : token
                : token;

        // Test if the reported dead key sequence (e.g., \0à) reports a character (e.g., à)
        // that maps to a 0.
        Token TestIfDeadKeySequenceCharacterMapsTo0() =>
            _tokenExtendedDataCharacterMap[reportedCharacterSequence[1]] == 48

                // Warning - Barcodes that use ISO/IEC 15434 syntax cannot be recognised.
                ? AssessFormatSupport
                    ? LogCalibrationInformation(
                                InitializeTokenData(),
                                InformationType.IsoIec15434SyntaxNotRecognised)
                    : token

                // Warning: The reported character sequence {0} is ambiguous. This may prevent reading of any additional data elements
                // included in a barcode.
                : LogCalibrationInformation(
                    InitializeTokenData(),
                    InformationType.ControlCharacterMappingNonInvariants,
                    reportedCharacterSequence[1].ToControlPictureString(),
                    $"{_tokenDataValue.ToControlPictureString()}{expectedCharacter.ToControlPictures()} {_tokenExtendedDataCharacterMap[reportedCharacterSequence[1]]}");

        // Test for a potential conflict with the character map.
        Token TestPotentialConflictWithCharacterMap() =>
            _tokenExtendedDataCharacterMap.ContainsKey(reportedCharacterSequence[1])

                // If the reported dead key sequence (e.g. \0à) reports a character (à) that maps to a 0,
                // we can't disambiguate \0à from <RS>0 in the barcode which is also reported as \00.
                ? TestIfDeadKeySequenceCharacterMapsTo0()

                // If the reported sequence is \00 and the character map does not map a reported 0 to
                // another character, then we can't disambiguate \00 from <RS>0 in the barcode which
                // is also reported as \00.
                : TestIfDeadKeySequence0NoMap();
    }

    /// <summary>
    ///   Determine if EDI barcodes are unreadable when the scanner does not report an
    ///   ASCII 28 or an ASCII 31.
    /// </summary>
    /// <param name="token">The current calibration token.</param>
    /// <param name="reportedCharacterSequence">A reported character sequence.</param>
    /// <param name="expectedCharacter">The expected character.</param>
    /// <returns>The calibration token.</returns>
    private Token DetermineIfEdiUnreadableWhenNoAscii28Or31(
        Token token,
        string reportedCharacterSequence,
        string expectedCharacter) {
        // If ASCII 28 and/or ASCII 31 characters are not being emitted, there may be a warning.
        return _tokenExtendedDataUnreadableFs ||
               _tokenExtendedDataUnreadableUs

                   // Is there a potential conflict with the character map?
                   ? TestPotentialConflictWithCharacterMap()
                   : token;

        // Test if the reported sequence is \00 and the character map does not map
        // a reported 0 to another character.
        Token TestIfDeadKeySequence0NoMap() =>
            reportedCharacterSequence[1] == 48

                // Warning - Barcodes that use ISO/IEC 15434 syntax to represent EDI data cannot be reliably read.
                ? AssessFormatSupport
                    ? LogCalibrationInformation(
                        InitializeTokenData(),
                        InformationType.IsoIec15434EdiNotReliablyReadable)
                    : token
                : token;

        // Test if the reported dead key sequence (e.g., \0à) reports a character (e.g., à)
        // that maps to a 0.
        Token TestIfDeadKeySequenceCharacterMapsTo0() =>
            _tokenExtendedDataCharacterMap[reportedCharacterSequence[1]] == 48

                // Warning - Barcodes that use ISO/IEC 15434 syntax to represent EDI data cannot be reliably read.
                ? AssessFormatSupport
                    ? LogCalibrationInformation(
                        InitializeTokenData(),
                        InformationType.IsoIec15434EdiNotReliablyReadable)
                    : token

                // Warning: The reported character sequence {0} is ambiguous. This may prevent reading of any additional data elements
                // included in a barcode.
                : LogCalibrationInformation(
                    InitializeTokenData(),
                    InformationType.ControlCharacterMappingNonInvariants,
                    reportedCharacterSequence[1].ToControlPictureString(),
                    $"{_tokenDataValue.ToControlPictureString()}{expectedCharacter.ToControlPictures()} {_tokenExtendedDataCharacterMap[reportedCharacterSequence[1]]}");

        // Test for a potential conflict with the character map.
        Token TestPotentialConflictWithCharacterMap() =>
            _tokenExtendedDataCharacterMap.ContainsKey(reportedCharacterSequence[1])

                // If a reported dead key sequence (e.g. \0à) reports a character (à) that maps to a 0,
                // we can't disambiguate \0à from <FS>0 and/or <US>0 in the barcode which is also reported
                // as \00.
                ? TestIfDeadKeySequenceCharacterMapsTo0()

                // If the reported sequence is \00 and the character map does not map a reported 0 to
                // another character, then we can't disambiguate \00 from <FS>0 and/or <US>0 in the barcode which
                // is also reported as \00.
                : TestIfDeadKeySequence0NoMap();
    }

    /// <summary>
    ///   Process the dead keys detected during the baseline test.
    /// </summary>
    private void ProcessDeadKeys() {
        // Select the valid dead keys to enumerate. If a dead key on the barcode scanner keyboard layout matches
        // a dead key on the computer keyboard layout, it is not valid for enumeration.
        var validDeadKeys =
            (from deadKey in _tokenExtendedDataDeadKeysMap
             where _tokenExtendedDataScannerDeadKeysMap.All(sdk => sdk.Value != deadKey.Key)
             select deadKey)
           .ToList();

        // To support the generation of additional barcodes, add each dead key detected in the baseline test to a list of
        // dead key character mappings.
        validDeadKeys.ForEach(AddDeadKeyMapping);

        _tokenRemaining = validDeadKeys.Count;
        _tokenDataCalibrationsRemaining = _tokenExtendedDataDeadKeyCharacterMap.Count;
        return;

        void AddDeadKeyMapping(KeyValuePair<string, string> deadKey) {
            var (key, value) = deadKey;
            _tokenExtendedDataDeadKeyCharacterMap.Add(key, value[0]);

            /* We need to add entries to the character map to handle situations where a combination to two characters may map to
             * two dead key characters on the computer keyboard. In this case, we need an entry in the character map to handle
             * the second character mapping. On some keyboards, a dead key followed by a space results in the reporting of a
             * different character to the actual dead key literal character. E.g., ¨ followed by a space may be reported as ".
             * We account for this by simply ignoring the situation where the dead key literal character already appears in the
             * character map and then fixing this up later if we detect this issue.
             * */
            if (!_tokenExtendedDataCharacterMap.ContainsKey(key[1]) && key[1] != value[0]) {
                _tokenExtendedDataCharacterMap.Add(key[1], value[0]);
            }
        }
    }

    /// <summary>
    /// Resolve the warnings information for situations where we know there is a keyboard layout mismatch, but
    /// we don't know if this affects invariant or non-invariant characters.  This can specifically happen
    /// where keys on the barcode scanner keyboard layout map to dead keys on the computer keyboard layout, but
    /// no other mismatches are detected.
    /// </summary>
    /// <param name="token">The current calibration token.</param>
    /// <returns>The calibration token.</returns>
    private Token ResolveUnspecifiedKeyboardMismatches(Token token) {
        if (token.Warnings.All(w => w.InformationType != InformationType.NonCorrespondingKeyboardLayouts)) {
            return token;
        }

        // Ensure the keyboard match flag is correctly set.
        token = token with { KeyboardMatch = false };

        if (_tokenExtendedDataDeadKeysMap.Any(map => map.Value.Length == 1 && map.Value[0] switch {
            var c when c >= 32 && c < 35 => true,
            var c when c >= 37 && c < 64 => true,
            var c when c >= 65 && c < 91 => true,
            var c when c == 95 => true,
            var c when c >= 97 && c < 123 => true,
            var c when c == 127 => true,
            _ => false
        })) {
            token = LogCalibrationInformation(
                token,
                InformationType.NonCorrespondingKeyboardLayoutsForInvariants);
        }

        if (_tokenExtendedDataDeadKeysMap.Any(map => map.Value.Length == 1 &&
                                                     $"\0{map.Value[0]}" != map.Key &&
                                                     map.Value[0] switch {
                                                         var c when c == 32 => true,
                                                         var c when c >= 35 && c < 37 => true,
                                                         var c when c == 64 => true,
                                                         var c when c >= 91 && c < 95 => true,
                                                         var c when c == 96 => true,
                                                         var c when c >= 123 && c < 127 => true,
                                                         _ => false
                                                     })) {
            token = LogCalibrationInformation(
                token,
                InformationType.NonCorrespondingKeyboardLayoutsForNonInvariantCharacters);
        }

        return token;
    }

    /// <summary>
    ///   Create a de-duplicated character mapping.
    /// </summary>
    /// <param name="token">The current calibration token.</param>
    /// <param name="selectedCharacterKeyValuePairs">
    ///   A collection of dictionary entries for all processed characters.
    /// </param>
    /// <returns>The calibration token.</returns>
    private Token CreateDeduplicatedCharacterMap(
        Token token,
        IEnumerable<KeyValuePair<char, char>> selectedCharacterKeyValuePairs) {
        var characterKeyValuePairs =
            selectedCharacterKeyValuePairs as KeyValuePair<char, char>[] ??
            selectedCharacterKeyValuePairs.ToArray();

        var keyValuePairs =
            selectedCharacterKeyValuePairs as KeyValuePair<char, char>[]
         ?? [.. characterKeyValuePairs];

        if (keyValuePairs.Length == 0) {
            return token;
        }

        var duplicateKeys =
            from pairGroup in from pair in characterKeyValuePairs
                              group pair by pair.Key
                into pairGroup
                              select pairGroup
            where pairGroup.Count() > 1
            select pairGroup.Key;

        var duplicateNonInvariantPairs =
            from pair in characterKeyValuePairs
            where duplicateKeys.Any(dk => dk == pair.Key) && NonInvariantCharacters.Contains(pair.Value)
            select pair;

        var nonInvariantPairs =
            duplicateNonInvariantPairs as KeyValuePair<char, char>[]
         ?? duplicateNonInvariantPairs.ToArray();

        if (nonInvariantPairs.Length != 0) {
            // Remove the duplicates from the processed non-invariant characters
            nonInvariantPairs.ToList().ForEach(kvp => _processedNonInvariantCharacters?.Remove(kvp.Key));

            selectedCharacterKeyValuePairs =
                (_processedInvariantCharacters ?? new Dictionary<char, char>())
                    .Where(m => m.Key != '\0')
                    .Select(m => m)
                    .Concat(
                         (_processedNonInvariantCharacters ?? new Dictionary<char, char>())
                             .Where(m => m.Key != '\0')
                             .Select(m => m));

            keyValuePairs =
                selectedCharacterKeyValuePairs as KeyValuePair<char, char>[]
             ?? selectedCharacterKeyValuePairs.ToArray();
        }

        // Create the character map and detect ambiguous characters
        try {
            _tokenExtendedDataCharacterMap =
                keyValuePairs.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value);
        }
        catch (ArgumentException argEx) {
            var reportedChar = argEx.Message[^1..][0];
            var expectedChars = from kvp in keyValuePairs
                                where kvp.Key == reportedChar
                                select kvp.Value.ToControlPictureString();
            var expectedString = expectedChars.Aggregate((a, b) => $"{a} {b}");

            // The reported character {0} is ambiguous. There are multiple keys for the same
            // character, each representing a different expected character.
            LogCalibrationInformation(
                token,
                InformationType.MultipleKeys,
                reportedChar.ToControlPictureString(),
                expectedString);
        }

        return token;
    }

    /// <summary>
    ///   Determine if the scanner and operating system keyboard layouts match.
    /// </summary>
    /// <param name="token">The current calibration token.</param>
    /// <param name="caseConversionCharacteristics">
    ///   The case conversion characteristics of the system based on observations.
    /// </param>
    /// <returns>The calibration token.</returns>
    private Token DoDetermineIfKeyboardsMatch(
        Token token,
        CaseConversionCharacteristics caseConversionCharacteristics) {
        // Set an indicator representing if the barcode scanner and computer keyboards are in correspondence.
        return token with {
            KeyboardMatch =
            !(_processedInvariantCharacters?.Count == 0 && _processedNonInvariantCharacters?.Count == 0)
                ? TestIfNoAdditionalAsciiCharactersReported()
                : true
        };

        // Test if the case of upper-case characters was inverted.
        // If the keyboards match, we expect to find a difference of 32 between key and value for
        // each expected character
        static bool TestUpperCaseInverted(IDictionary<char, char> differences82) =>
            differences82
                .Where(kvp => kvp.Value > 64 && kvp.Value < 91)
                .All(kvp => kvp.Key == kvp.Value + 32);

        // Test if the case of lower-case characters was inverted.
        // If the keyboards match, we expect to find a difference of -32 between key and value for
        // each expected character
        static bool TestLowerCaseInverted(IDictionary<char, char> differences82) =>
            differences82
                .Where(kvp => kvp.Value > 96 && kvp.Value < 123)
                .All(kvp => kvp.Key == kvp.Value - 32);

        // Test if the case of both upper and lower-case characters was inverted.
        bool TestCaseInverted(IDictionary<char, char> differences82) =>
            TestUpperCaseInverted(differences82) && TestLowerCaseInverted(differences82);

        // Differences between reported and expected characters exist.
        // No differences were detected for non-invariant characters.
        // Lower case conversion to upper case was detected.
        // Additional upper case conversion to lower case was detected.
        // Either the case of some alphabetic characters were not inverted or additional differences exist.
        // Test if additional differences exist.
        bool? TestIfCaseInvertedWithOtherDifferences() =>
            _processedInvariantCharacters?.Count > 52 && TestCaseInverted(_processedInvariantCharacters)
                ? null
                : false;

        // Differences between reported and expected characters exist.
        // No differences were detected for non-invariant characters.
        // Lower case conversion to upper case was detected.
        // Additional upper case conversion to lower case was detected.
        // Test if the only detected differences are for inversion of case.
        bool? TestIfOnlyCaseInverted() =>
            _processedInvariantCharacters?.Count == 52 && TestCaseInverted(_processedInvariantCharacters)
                ? true
                : TestIfCaseInvertedWithOtherDifferences();

        // Differences between reported and expected characters exist.
        // No differences were detected for non-invariant characters.
        // Lower case conversion to upper case was detected.
        // No additional upper case conversion to lower case was detected.
        // Either some lower case characters were not inverted or additional differences exist.
        // Test if additional differences exist.
        bool? TestIfLowerCaseCharactersInvertedWithOtherDifferences() =>
            _processedInvariantCharacters?.Count > 26 &&
            TestLowerCaseInverted(_processedInvariantCharacters)
                ? null
                : false;

        // Differences between reported and expected characters exist.
        // No differences were detected for non-invariant characters.
        // Lower case conversion to upper case was detected.
        // No additional upper case conversion to lower case was detected.
        // Test if the only detected differences are for inversion of lower case characters.
        bool? TestIfOnlyLowerCaseCharactersInverted() =>
            _processedInvariantCharacters?.Count == 26 && TestLowerCaseInverted(_processedInvariantCharacters)
                ? true
                : TestIfLowerCaseCharactersInvertedWithOtherDifferences();

        // Differences between reported and expected characters exist.
        // No differences were detected for non-invariant characters.
        // Lower case conversion to upper case was detected.
        // Test if additional upper case conversion to lower case was detected.
        bool? TestIfLowerCaseConversionAlsoDetected() =>
            caseConversionCharacteristics.LowerCaseConversionDetected
                ? TestIfOnlyCaseInverted()
                : TestIfOnlyLowerCaseCharactersInverted();

        // Differences between reported and expected characters exist.
        // No differences were detected for non-invariant characters.
        // No lower case conversion to upper case was detected.
        // Upper case conversion to lower case was detected.
        // Either some upper case characters were not inverted or additional differences exist.
        // Test if additional differences exist.
        bool? TestIfUpperCaseCharactersInvertedWithOtherDifferences() =>
            _processedInvariantCharacters?.Count > 26 &&
            TestUpperCaseInverted(_processedInvariantCharacters)
                ? null
                : false;

        // Differences between reported and expected characters exist.
        // No differences were detected for non-invariant characters.
        // No lower case conversion to upper case was detected.
        // Upper case conversion to lower case was detected.
        // Test if the only detected differences are for inversion of upper case characters.
        bool? TestIfOnlyUpperCaseCharactersInverted() =>
            _processedInvariantCharacters?.Count == 26 &&
            TestUpperCaseInverted(_processedInvariantCharacters)
                ? true
                : TestIfUpperCaseCharactersInvertedWithOtherDifferences();

        // Differences between reported and expected characters exist.
        // No differences were detected for non-invariant characters.
        // No lower case conversion to upper case was detected.
        // Test if upper case conversion to lower case was detected.
        bool? TestIfLowerCaseConversionDetected() =>
            caseConversionCharacteristics.LowerCaseConversionDetected
                ? TestIfOnlyUpperCaseCharactersInverted()
                : false;

        // Differences between reported and expected characters exist.
        // No differences were detected for non-invariant characters.
        // Test if lower case conversion to upper case was detected.
        bool? TestIfUpperCaseConversionDetected() =>
            caseConversionCharacteristics.UpperCaseConversionDetected
                ? TestIfLowerCaseConversionAlsoDetected()
                : TestIfLowerCaseConversionDetected();

        // Differences between reported and expected characters exist.
        // Test that no differences were detected for non-invariant characters.
        bool? TestIfNoAdditionalAsciiCharactersReported() =>
            _processedNonInvariantCharacters?.Count == 0
                ? TestIfUpperCaseConversionDetected()
                : false;
    }

    /// <summary>
    ///   Infer the case conversion characteristics of the scanner when CAPS LOCK
    ///   is reported as being on.
    /// </summary>
    /// <param name="token">The calibration token.</param>
    /// <param name="caseConversionCharacteristics">The case conversion characteristics.</param>
    /// <param name="platform">The operating system platform.</param>
    /// <param name="setCapLockWarningAndIndicator">
    ///   A function that sets the CAPS LOCK indicator as well as a warning message.
    /// </param>
    /// <returns>The calibration token, populated to inferred information.</returns>
    private Token InferCaseConversionCapsLockOn(
        Token token,
        CaseConversionCharacteristics caseConversionCharacteristics,
        SupportedPlatform platform,
        Func<Token, EnvToken> setCapLockWarningAndIndicator) {
        // Warning - Caps Lock is switched on.
        token = LogCalibrationInformation(
            token,
            InformationType.CapsLockOn);

        // CAPS LOCK is on. Test that lower case characters are reported as upper case.
        return caseConversionCharacteristics.UpperCaseConversionDetected

            // On most platforms, upper case characters will be reported as lower case. Test
            // that this is detected.
            ? TestForLowerCaseConversion()

            // The scanner is probably configured to convert lower case to upper case or to
            // invert case (compensation) before transmitting the data. There are other
            // possibilities such as conversion in driver software or a software keyboard
            // wedge. These are not common scenarios, so we assume scanner configuration is
            // being used.
            : TestForScannerUpperCaseConversion();

        // CAPS LOCK is on. However, the case of inverted characters is not inverted. This
        // may be because the scanner is compensating for CAPS LOCK.
        Token TestForScannerCompensation() =>
            platform == SupportedPlatform.Macintosh

                // This is an unusual scenario. It could occur on a Mac for a keyboard that
                // is configured to handle Shift with Caps Lock in a similar way as other
                // platforms. We therefore assume there is no scanner compensation.
                ? token

                // The scanner is probably configured to invert case, compensating for CAPS
                // LOCK. Some scanners can detect the CAPS LOCK state and compensate
                // automatically. Others must be configured manually to compensate.

                // Information - Scanner may be configured to compensate for Caps Lock.
                : LogCalibrationInformation(
                    token,
                    InformationType.ScannerMayCompensateForCapsLock);

        // CAPS LOCK is on. However, lower case characters are not reported as upper case.
        // Test that upper case characters are reported as lower case.
        Token TestForScannerUpperCaseConversion() =>
            caseConversionCharacteristics.LowerCaseConversionDetected

                // Upper case characters are reported as lower case. This suggests that the
                // scanner is converting lower case characters to upper case before transmitting
                // them.

                // Warning - Scanner may be configured to convert characters to upper case.
                ? LogCalibrationInformation(
                    token,
                    InformationType.ScannerMayConvertToUpperCase)

                // For most platforms, the scanner appears to be compensating to CAPS LOCK by
                // inverting case. Test the platform to infer the behaviour of the scanner.
                : TestForScannerCompensation();

        // CAPS LOCK is on. Test the platform to infer if the scanner is configured to convert
        // characters to lower case.
        Token TestPlatformForScannerLowerCaseConversion() =>
            platform == SupportedPlatform.Macintosh

                // Apple Mac implements CAPS LOCK to perform lower-to-upper case conversion, only.
                ? token

                // Warning - Scanner may be configured to convert characters to lower case.
                : LogCalibrationInformation(
                    token,
                    InformationType.ScannerMayConvertToLowerCase);

        // CAPS LOCK is on. Test that upper case characters are reported as lower case.
        Token TestForLowerCaseConversion() =>
            caseConversionCharacteristics.LowerCaseConversionDetected

                // Character case is being inverted as expected.
                ? setCapLockWarningAndIndicator(token).End()

                // On most, but not all, platforms, this indicates that the scanner is configured
                // to convert upper case characters to lower case. CAPS LOCK is then inverting
                // this. Test the platform.
                : TestPlatformForScannerLowerCaseConversion();
    }

    /// <summary>
    ///   Infer the case conversion characteristics of the scanner when CAPS LOCK
    ///   is reported as being on.
    /// </summary>
    /// <param name="token">The calibration token.</param>
    /// <param name="caseConversionCharacteristics">The case conversion characteristics.</param>
    /// <param name="platform">The operating system platform.</param>
    /// <param name="capsLock">The CAPS LOCK indicator. This is either false or null.</param>
    /// <param name="setCapLockWarningAndIndicator">
    ///   A function that sets the CAPS LOCK indicator as well as a warning message.
    /// </param>
    /// <returns>The calibration token, populated to inferred information.</returns>
    private Token InferCaseConversionCapsLockOffOrUnknown(
        Token token,
        CaseConversionCharacteristics caseConversionCharacteristics,
        SupportedPlatform platform,
        bool? capsLock,
        Func<Token, EnvToken> setCapLockWarningAndIndicator) {
        // CAPS LOCK is off or unknown. Test if lower case characters are reported as
        // upper case.
        return caseConversionCharacteristics.UpperCaseConversionDetected

            // CAPS LOCK may be on, or the scanner may be configured to convert lower case
            // characters to upper case. Test to see if it is also converting upper case
            // characters to lower case.
            ? TestForCaseInversion()

            // Test to see if upper case characters are reported as lower case.
            : TestForLowerCaseConversion();

        // CAPS LOCK is unknown. However, the case of reported characters is being inverted.
        // Test the platform to determine if this is probably due to scanner configuration
        // or if it is more likely that CAPS LOCK may be switched on.
        Token TestPlatformForUnknownCapsLock() =>
            platform == SupportedPlatform.Macintosh

                // Most keyboards on Apple Mac implement CAPS LOCK to only convert lower case
                // characters to upper case. Therefore, the most likely explanation for
                // inversion is that the scanner is configured to invert case. This is not
                // certain, however. Some keyboard layouts support CAPS LOCK for case inversion.
                // We will issue a best-endeavours warning message.

                // Warning - Scanner may be configured to invert character case.
                ? LogCalibrationInformation(
                    token,
                    InformationType.ScannerMayInvertCase)

                // One scenario here is that CAPS LOCK is switched on, but the state has not
                // been reported to the calibration library. However, CAPS LOCK could be
                // switched off and the scanner could be configured to invert character case.
                // We will issue a best-endeavours warning message.

                // Warning - Caps Lock may be switched on.
                : setCapLockWarningAndIndicator(token).End();

        // CAPS LOCK is off or unknown. However, the case of reported characters is being inverted.
        // Test if the CAPS LOCK state is known.
        Token TestIfCapsLockKnownForCharacterInversion() =>
            capsLock is null

                // The CAPS LOCK state is unknown. Test the platform to infer the scanner behaviour.
                ? TestPlatformForUnknownCapsLock()

                // We know that CAPS LOCK is off, so the scanner is probably configured to invert
                // the case of reported characters before transmitting the data. There are other
                // possibilities such as conversion in driver software or a software keyboard wedge.
                // These are not common scenarios, so we assume scanner configuration is being used.

                // Warning - Scanner may be configured to invert character case.
                : LogCalibrationInformation(
                    token,
                    InformationType.ScannerMayInvertCase);

        // CAPS LOCK is off or unknown. Lower case characters are reported as upper case. Test if
        // upper case characters are reported as lower case.
        Token TestForCaseInversion() =>
            caseConversionCharacteristics.LowerCaseConversionDetected

                // CAPS LOCK may be on or the scanner may be configured to invert the case of
                // reported characters. Test if the CAPS LOCK state is known
                ? TestIfCapsLockKnownForCharacterInversion()

                // Warning - Scanner may be configured to convert characters to upper case.
                : LogCalibrationInformation(
                    token,
                    InformationType.ScannerMayConvertToUpperCase);

        // CAPS LOCK is off or unknown. Lower case characters are not reported as upper case.
        // Test to see if upper case characters are being reported as lower case.
        Token TestForLowerCaseConversion() =>
            caseConversionCharacteristics.LowerCaseConversionDetected

                // The scanner may be configured to convert upper case characters to lower case.
                // There are other possibilities such as conversion in driver software or a
                // software keyboard wedge. These are not common scenarios, so we assume scanner
                // configuration is being used.

                // Warning - Scanner may be configured to convert characters to lower case.
                ? LogCalibrationInformation(
                    token,
                    InformationType.ScannerMayConvertToLowerCase)

                // The most likely scenario here is that CAPS LOCK is off and the scanner is not
                // performing any case conversion. It is possible that CAPS LOCK is on and the
                // scanner is compensating. However, we won't issue a warning here.
                : token;
    }

    /// <summary>
    ///   Determine if the user has scanned the correct baseline barcode and if it is fully or
    ///   partially reported.
    /// </summary>
    /// <param name="token">The current calibration token.</param>
    /// <param name="data">The reported calibration data.</param>
    /// <param name="returnImmediately">
    ///   Indicates if the calibrator should return immediately on error.
    /// </param>
    /// <returns>The calibration token.</returns>
    private Token DetermineBarcodeProvenance(Token token, string data, out bool returnImmediately) {
        var immediateReturn = false;

        // Test the provenance of the calibration barcode.
        token = BarcodeProvenance(data) switch {
            Calibration.BarcodeProvenance.PartialBaseline =>

                // Warning - Partial data reported for calibration barcode.
                LogCalibrationInformation(token, InformationType.PartialCalibrationDataReported),
            Calibration.BarcodeProvenance.Unknown =>
                BarcodeProvenanceForDeadKey(),
            Calibration.BarcodeProvenance.NoData =>

                // Error - No calibration data was reported.
                LogErrorForImmediateReturn(token, InformationType.NoCalibrationDataReported),
            _ => token
        };

        returnImmediately = immediateReturn;
        return token;

        // Log an error and flag for immediate return.
        Token LogErrorForImmediateReturn(Token localToken, InformationType informationType) {
            immediateReturn = true;
            return LogCalibrationInformation(localToken, informationType);
        }

        // Test the provenance of a deadkey calibration barcode.
        Token BarcodeProvenanceForDeadKey() =>
            BarcodeProvenance(data, true) switch {
                Calibration.BarcodeProvenance.DeadKey or Calibration.BarcodeProvenance.PartialDeadkey =>

                    // Warning - The reported data is for the wrong calibration barcode.
                    LogCalibrationInformation(
                        token,
                        InformationType.IncorrectCalibrationDataReported),
                Calibration.BarcodeProvenance.Unknown =>

                    // Error - The reported data is unrecognised. The wrong barcode may have been scanned.
                    LogErrorForImmediateReturn(token, InformationType.UnrecognisedData),
                _ => token
            };
    }

    /// <summary>
    ///   Assesses the likely provenance of the calibration barcode. By 'provenance' we mean a check on
    ///   whether the barcode appears to be a genuine calibration barcode.
    /// </summary>
    /// <param name="data">The reported barcode data.</param>
    /// <param name="isDeadKey">If true, the barcode data should be for a deadkey barcode; otherwise it should be for a baseline barcode.</param>
    /// <returns>A provenance value.</returns>
    private BarcodeProvenance BarcodeProvenance(string data, bool isDeadKey = false) {
        if (string.IsNullOrWhiteSpace(data)) {
            // No calibration data was reported.
            return Calibration.BarcodeProvenance.NoData;
        }

        var testData = data;

        double hypotheticalMean; // The mean distance between repeating characters (space or ASCII 0) if keyboards match for all invariants.
        double hypotheticalStdDev; // The standard deviation if the hypothetical mean holds.
        int hypotheticalLength; // The expected length of the invariant segment if the hypothetical mean holds.
        int hypotheticalCount; // The expected length of counted characters if the hypothetical mean holds.
        double tolerance; // a tolerance on the width of the actual bell curve compared to the hypothetical curve - a simple multiple of the hypothetical standard deviation.
        var countCharacter = false; // If false, count non-repeating characters, otherwise count repeating characters.

        if (isDeadKey) {
            hypotheticalMean = 2.893617021276596;
            hypotheticalStdDev = 0.426977953;
            hypotheticalLength = 273;
            hypotheticalCount = 94;
            tolerance = 3.0; // A tolerance of 1.5 is sufficient to pass unit tests. However, other testing indicates we need a greater margin.
            countCharacter = true;

            // Remove any explicitly defined prefix.
            if (RemoveExplicitlyDefinedPrefix()) {
                return TestProvenance('\0');
            }

            // Best-endeavours approach to remove prefix.
            var firstAscii0 = testData.IndexOf('\u0000', StringComparison.Ordinal);

            if (firstAscii0 <= 0) {
                return TestProvenance('\0');
            }

            testData = testData[firstAscii0..];

            var lastAscii0 = testData.LastIndexOf('\u0000');

            testData = lastAscii0 > 220 &&
                       lastAscii0 < testData.Length - 3
                           ? testData[..(lastAscii0 + 2)]
                           : testData;

            return TestProvenance('\0');
        }

        hypotheticalMean = 1.975903614457831;
        hypotheticalStdDev = 0.19968916;
        hypotheticalLength = 165;
        hypotheticalCount = 82;

        /* The tolerance value is high, but is just sufficient to deal with Greek Polytonic
         * keyboards which yield 13 dead keys in testData. We don't know of any European
         * keyboards with a higher yield. It is entirely possible that some non-European
         * keyboards will not pass the test at this tolerance, and there may be a case for
         * allowing this value to be overridden programmatically.
         * */
        tolerance = 4.8;

        var splitData = data.Split(new string(' ', 3));

        // It should not be possible for Split to yield no entries, but we
        // will retain this as a defensive measure.
        if (splitData.Length == 0) {
            return Calibration.BarcodeProvenance.NoData;
        }

        var longestIdx = 0;
        var lastLongestLength = 0;

        // Get the longest value
        for (var sdIdx = 0; sdIdx < splitData.Length; sdIdx++) {
            var currentLength = splitData[sdIdx].Length;

            if (currentLength <= lastLongestLength) {
                continue;
            }

            longestIdx = sdIdx;
            lastLongestLength = currentLength;
        }

        testData = splitData[longestIdx];

        // Remove any explicitly defined prefix.
        if (RemoveExplicitlyDefinedPrefix()) {
            return TestProvenance(' ');
        }

        /* Baseline barcodes start with two spaces, and there should be an additional three spaces
         * (or maybe two) between the invariant and non-invariant characters. Start by finding
         * the last occurrence of two consecutive spaces in the barcode.
         */
        var twoSpacesIdx = testData.LastIndexOf(new string(' ', 2), StringComparison.Ordinal);

        // If this is greater than 140, look for the previous two consecutive spaces.
        if (twoSpacesIdx > 140) {
            hypotheticalMean = 1.958762886597938;
            hypotheticalStdDev = 0.19987109247774507;
            hypotheticalLength = 191;
            hypotheticalCount = 94;
            tolerance = 4;

            twoSpacesIdx = testData[..twoSpacesIdx]
               .LastIndexOf(
                    new string(' ', 2),
                    StringComparison.Ordinal);
        }

        if (twoSpacesIdx <= 0) {
            return TestProvenance(' ');
        }

        /* We have to account for the unlikely scenario of a barcode scanner that must
         * transmit an invariant character that is a dead key on the barcode scanner keyboard layout,
         * entered in combination with a modifier (usually Alt GR). In this case, if that
         * key combination does not translate to any character on the computer keyboard layout,
         * the system will report an ASCII 0, followed by a space (entered by the barcode scanner to
         * transmit the literal dead key character) followed by another space (from the barcode).
         * So, to deal with this, we will look to see if the character before the two spaces is
         * an ASCII 0 and if the length of the data from this point is short. If so, we will
         * look for a previous two spaces, and keep going until we get to the start of the string
         * or the criteria are no longer met. This is not entirely reliable, of course. If there
         * is a prefix, and the last character of the prefix is reported as an ASCII 0, the logic
         * will be defeated. This is extremely unlikely, but even if it happened, it is even more
         * unlikely that calibration is possible, anyway. So calibration will (almost certainly)
         * fail, possibly for the wrong 'stated' reason.
         * */
        while (twoSpacesIdx > 0 && testData[twoSpacesIdx - 1] == '\0' && testData[twoSpacesIdx..].Length < 165) {
            twoSpacesIdx = testData[..twoSpacesIdx]
               .LastIndexOf(
                    new string(' ', 2),
                    StringComparison.Ordinal);
        }

        testData = testData[twoSpacesIdx..];

        return TestProvenance(' ');

        // Test the provenance of the barcode.
        BarcodeProvenance TestProvenance(char character) {
            // Calculate the standard deviation and other parameters
            var stdDev = StdDevForRepeatingCharacters(testData, character, hypotheticalMean, out var count, countCharacter);
            var stdDevInRange = stdDev <= hypotheticalStdDev * tolerance;
            var dataLengthInRange = testData.Length >= hypotheticalLength * 0.85 && testData.Length < hypotheticalLength * 1.1;
            var characterCountInRange = count >= hypotheticalCount * 0.9 && count < hypotheticalCount * 1.1;
            var dataLengthIsShort = testData.Length < hypotheticalLength * 0.9;
            var shortHypotheticalCount = dataLengthIsShort ? hypotheticalCount * (testData.Length / (double)hypotheticalLength) : 0;
            var shortCharacterCountInRange = dataLengthIsShort && count >= shortHypotheticalCount * 0.9 && count < shortHypotheticalCount * 1.1;

            // If the standard deviation is acceptable, test the provenance of the barcode.
            return stdDevInRange
                ? TestInRange()
                : Calibration.BarcodeProvenance.Unknown;

            // Too few characters were reported. Test if this appears to be a baseline or deadkey barcode.
            BarcodeProvenance TestBarcodeTypeForShort() =>
                isDeadKey
                    ? Calibration.BarcodeProvenance.PartialDeadkey
                    : Calibration.BarcodeProvenance.PartialBaseline;

            // Test if this appears to be a baseline or deadkey barcode.
            BarcodeProvenance TestBarcodeTypeForInRange() =>
                isDeadKey
                    ? Calibration.BarcodeProvenance.DeadKey
                    : Calibration.BarcodeProvenance.Baseline;

            // Test if too few characters were reported.
            BarcodeProvenance TestShort() =>
                dataLengthIsShort && shortCharacterCountInRange
                    ? TestBarcodeTypeForShort()
                    : Calibration.BarcodeProvenance.Unknown;

            // Test to see if the number of reported characters is in the accepted range.
            BarcodeProvenance TestInRange() =>
                dataLengthInRange && characterCountInRange
                    ? TestBarcodeTypeForInRange()
                    : TestShort();
        }

        // Remove any explicitly defined prefix.
        bool RemoveExplicitlyDefinedPrefix() {
            /* The data could have prefixes, as we need to determine barcode provenance before we
             * process the data to handle prefixes. For baseline barcodes, there should not be
             * any suffixes as these will be split into another part of the array. NB. the logic
             * could be defeated by a very long suffix (longer than the main segment of barcode data).
             */
            var explicitPrefixFound =
                !string.IsNullOrEmpty(_tokenExtendedDataReportedPrefix) &&
                testData.StartsWith(_tokenExtendedDataReportedPrefix, StringComparison.Ordinal);

            testData = explicitPrefixFound
                ? testData[_tokenExtendedDataReportedPrefix!.Length..]
                : testData;

            return explicitPrefixFound;
        }
    }

    /// <summary>
    ///   Fix up the segments in the case where an expected reported prefix has been specified.
    /// </summary>
    /// <param name="segments">The segments.</param>
    /// <param name="tempSpaceHolder">The temporary space holder character.</param>
    private void FixUpExpectedReportedPrefix(List<string> segments, char tempSpaceHolder) {
        /* The use of prefixes can lead to edge conditions where the prefix is segmented prematurely. We can detect this fairly
         * reliably by checking to see if the second segment looks like it may contain invariant characters. To keep this
         * simple, we'll base the decision on a count of characters, with margin for error. Then we need to fix up the segments.
         * */
        int charSet82Idx;
        var foundCharSet82 = false;

        for (charSet82Idx = 0; charSet82Idx < segments.Count; charSet82Idx++) {
            if (segments[charSet82Idx].Length <= 50) {
                continue;
            }

            foundCharSet82 = true;
            break;
        }

        if (!foundCharSet82 || charSet82Idx <= 0) {
            return;
        }

        var seg0Builder = new StringBuilder(segments[0]);

        for (var idx = 1; idx < charSet82Idx; idx++) {
            var fixedUpSegment0 =
                seg0Builder.ToString().Replace(
                    tempSpaceHolder.ToString(),
                    string.Empty,
                    StringComparison.Ordinal);

            seg0Builder.Clear();
            seg0Builder.Append(fixedUpSegment0);
            seg0Builder.Append(segments[idx]);
        }

        segments[0] = seg0Builder.ToString().Replace(
            tempSpaceHolder.ToString(),
            string.Empty,
            StringComparison.Ordinal);
        var seg0Length = segments[0].Length;

        var inIdx = 0;

        for (var idx = charSet82Idx; idx < segments.Count; idx++) {
            if (idx == charSet82Idx) {
                var tempSegValue = segments[0] + segments[idx];

                tempSegValue = tempSegValue[..(_expectedReportedPrefix?.Length ?? 0)]
                                  .Contains(
                                       tempSpaceHolder,
                                       StringComparison.Ordinal)
                                   ? _expectedReportedPrefix +
                                     tempSegValue[tempSegValue.IndexOf(
                                                      tempSpaceHolder,
                                                      StringComparison.Ordinal)..]
                                   : $"{_expectedReportedPrefix}{tempSpaceHolder} {tempSegValue[seg0Length..]}";

                segments[inIdx++] = tempSegValue;
                continue;
            }

            segments[inIdx++] = segments[idx];

            if (inIdx == segments.Count - 1) {
                segments.RemoveAt(inIdx);
            }
        }

        for (var idx = segments.Count - charSet82Idx; idx < segments.Count; idx++) {
            segments[idx] = string.Empty;
        }
    }

    /// <summary>
    ///   Returns the barcode data for a dead key calibration barcode.
    /// </summary>
    /// <param name="value">The dead key.</param>
    /// <returns>The barcode data for a dead key calibration barcode.</returns>
    private string CreateDeadKeyCalibration(char value) {
        if (value == default(char)) {
            return string.Empty;
        }

        var barcodeDataContent = new StringBuilder();

        foreach (var c in AsciiChars) {
            barcodeDataContent.Append(value);
            barcodeDataContent.Append(_tokenExtendedDataUnrecognisedKeys.Contains(c) ? " " : c);
        }

        return barcodeDataContent.ToString();
    }

    /// <summary>
    ///   Detects various types of character issues in an unprocessed segment.
    /// </summary>
    /// <param name="token">The calibration token.</param>
    /// <param name="reportedSegments">The reported segments of character sequences contained in the baseline barcode.</param>
    /// <param name="expectedSegments">The actual segments of character sequences contained in the baseline barcode.</param>
    /// <param name="reportedSegmentsIndex">The index of the reported segment.</param>
    /// <param name="invariant">True, if the segment represents invariant characters; otherwise false.</param>
    /// <returns>The amended calibration token.</returns>
    private Token DetectCharacterIssues(
        Token token,
        IList<List<string>> reportedSegments,
        IReadOnlyList<List<string>> expectedSegments,
        int reportedSegmentsIndex,
        bool invariant = false) {
        var chainedSequences = new Dictionary<int, string>();
        var splitSequences = new Dictionary<int, IList<string>>();
        var segment = reportedSegments[reportedSegmentsIndex];

        // Detect any unrecognised characters or complex character sequences.
#pragma warning disable SA1118 // Parameter should not span multiple lines
        token = DetectUnrecognisedOrComplexCharacters(
            token,
            segment,
            reportedSegmentsIndex < expectedSegments.Count
                ? expectedSegments[reportedSegmentsIndex]
                : [],
            chainedSequences,
            splitSequences,
            invariant);
#pragma warning restore SA1118 // Parameter should not span multiple lines

        List<string> segmentOut;

        // If any chained dead key sequences have been detected, we will normalize them by removing multiple ASCII 0 characters.
        if (chainedSequences.Count > 0) {
            segmentOut = segment.Select((t, sequenceIdx) => chainedSequences.TryGetValue(sequenceIdx, out var value)
                    ? $"\u0000{value[value.IndexOf(value.First(c => c != '\u0000'), StringComparison.Ordinal)..]}"
                    : t)
                .ToList();

            // Update the reported segment with normalized content.
            reportedSegments[reportedSegmentsIndex] = segmentOut;
        }

        // If any sequences were split, update the reported segment.
        if (splitSequences.Count <= 0) {
            return token;
        }

        segmentOut = [];

        for (var sequenceIdx = 0; sequenceIdx < segment.Count; sequenceIdx++) {
            if (splitSequences.TryGetValue(sequenceIdx, out var value)) {
                segmentOut.AddRange(value);
            }
            else {
                segmentOut.Add(segment[sequenceIdx]);
            }
        }

        // Update the reported segment with the split sequences.
        reportedSegments[reportedSegmentsIndex] = segmentOut;

        return reportedSegments[reportedSegmentsIndex].Count != expectedSegments[reportedSegmentsIndex].Count

                   // Error - Some invariant characters cannot be detected.
                   ? LogCalibrationInformation(
                       token,
                       InformationType.UndetectedInvariantCharacters)
                   : token;
    }

    /// <summary>
    ///   Detect any difference in the reported and expected strings.
    /// </summary>
    /// <param name="token">The calibration token.</param>
    /// <param name="reportedPrintables">The list of reported printables.</param>
    /// <param name="expectedPrintables">The list of expected printables.</param>
    /// <param name="invariant">Indicates whether the invariant character set is being processed.</param>
    /// <param name="differences">A dictionary of processed state.</param>
    /// <returns>The amended calibration token.</returns>
    private Token DetectDifferences(
        Token token,
        IReadOnlyList<string> reportedPrintables,
        List<string> expectedPrintables,
        bool invariant,
        out IDictionary<char, char> differences) {
        differences = new Dictionary<char, char>();
        var regExBuilder = new StringBuilder();
        _invariantMappedCharacters ??= new StringBuilder();
        _nonInvariantMappedCharacters ??= new StringBuilder();

        for (var idxSequence = 0; idxSequence < reportedPrintables.Count; idxSequence++) {
            var reportedSequence = reportedPrintables[idxSequence];
            var reportedChar = reportedPrintables[idxSequence][0];
            var expectedChar = idxSequence < expectedPrintables.Count
                                    ? expectedPrintables[idxSequence][0]
                                    : default;

            // Process the character sequence that maps to ']' for AIM identifiers
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (!invariant && idxSequence == expectedPrintables.FindIndex(s => s == "]")) {
                // This is used as the first character of an AIM identifier. Check to see if it is a dead key.
                if (reportedSequence[0] == '\0') {
                    if (reportedSequence.Length > 1) {
                        _tokenExtendedDataAimFlagCharacterSequence = reportedChar.ToInvariantString() + reportedSequence[1..];
                        _tokenExtendedDataDeadKeysMap.TryAdd(reportedSequence, "]");

                        // Information - AIM identifiers are supported.
                        token = LogCalibrationInformation(token, InformationType.AimSupported);

                        // Warning - The barcode scanner and computer keyboard layouts do not correspond when representing AIM identifiers.
                        token = LogCalibrationInformation(token, InformationType.NonCorrespondingKeyboardLayoutsForAimIdentifier);
                    }
                    else {
                        _tokenExtendedDataAimFlagCharacterSequence = reportedSequence;

                        // Warning - AIM Identifiers cannot be recognised.
                        token = LogCalibrationInformation(token, InformationType.AimNotRecognised);
                    }
                }
                else {
                    // Information - AIM identifiers are supported.
                    _tokenExtendedDataAimFlagCharacterSequence = reportedChar.ToInvariantString();
                    token = LogCalibrationInformation(token, InformationType.AimSupported);

                    if (reportedChar != expectedChar) {
                        // Record the difference.
                        differences.Add(reportedChar, expectedChar);
                    }

                    token = reportedChar != expectedChar

                        // Warning - The barcode scanner and computer keyboard layouts do not correspond when representing AIM identifiers.
                        ? LogCalibrationInformation(
                            token,
                            InformationType.NonCorrespondingKeyboardLayoutsForAimIdentifier)
                        : token;
                }

                AppendReportedFlagCharacter(_nonInvariantMappedCharacters);

                regExBuilder.Append(reportedChar);

                continue;

                void AppendReportedFlagCharacter(StringBuilder mappedCharacters) =>
                    mappedCharacters.Append(reportedChar switch {
                        _ when reportedChar < 32 => (char)(reportedChar + 9216),
                        _ => reportedChar
                    });
            }

            if (reportedChar == '\0') {
                // Did we hit a dead key? If the second character is a space, this is a barcode scanner dead key.
                if (reportedPrintables[idxSequence].Length > 1 && reportedPrintables[idxSequence][1] != ' ') {
                    var key = reportedChar.ToInvariantString() + reportedPrintables[idxSequence][1];

                    if (_tokenExtendedDataDeadKeysMap.ContainsKey(key)) {
                        // If either expected character is non-invariant, we will ignore the problem.
                        var firstDeadKeyChar = _tokenExtendedDataDeadKeysMap.FirstOrDefault(kvp => kvp.Key == key).Value[0];

#if NET7_0_OR_GREATER
                        if (InvariantsMatchRegex().IsMatch(firstDeadKeyChar.ToInvariantString() + expectedChar))
#else
                        if (InvariantsMatchRegex.IsMatch(firstDeadKeyChar.ToInvariantString() + expectedChar))
#endif
                        {
                            // Error - The reported dead key character {0} is ambiguous. There are multiple dead keys
                            // for the same character, each representing a different expected character.
                            differences = new Dictionary<char, char>();
                            return LogCalibrationInformation(
                                token,
                                InformationType.DeadKeyMultipleKeys,
                                key.ToControlPictures(),
                                $"{firstDeadKeyChar.ToControlPictureString()} {expectedChar.ToControlPictureString()}");
                        }
                    }
                    else {
                        _tokenExtendedDataDeadKeysMap.Add(key, expectedChar.ToInvariantString());
                    }
                }

                var reportedCharOut = reportedPrintables[idxSequence].Length == 1
                    ? reportedPrintables[idxSequence][0]
                    : reportedPrintables[idxSequence][1];

                AppendReportedDeadKeyCharacter(invariant
                    ? _invariantMappedCharacters
                    : _nonInvariantMappedCharacters);

                continue;

                void AppendReportedDeadKeyCharacter(StringBuilder mappedCharacters) =>
                    mappedCharacters.Append(reportedCharOut switch {
                        _ when reportedCharOut < 32 => (char)(reportedCharOut + 9216),
                        _ => reportedCharOut
                    });
            }

            // If the reported sequence is longer than two characters, this is a ligature, and will
            // be recorded in the ligature map, so continue.
            if (reportedSequence.Length > 1) {
                continue;
            }

            AppendReportedCharacter(invariant
                ? _invariantMappedCharacters
                : _nonInvariantMappedCharacters);

            regExBuilder.Append(reportedChar);

            if (reportedChar == expectedChar) continue;

            if (differences.TryGetValue(reportedChar, out var differencesValue)) {
                if (differencesValue == expectedChar) {
                    continue;
                }

                // Check that any expected characters for this reported character are marked as unrecognised.
                if (!_tokenExtendedDataUnrecognisedKeys.Contains(differences[reportedChar])) {
                    _tokenExtendedDataUnrecognisedKeys.Add(differences[reportedChar]);
                }

                if (!_tokenExtendedDataUnrecognisedKeys.Contains(expectedChar)) {
                    _tokenExtendedDataUnrecognisedKeys.Add(expectedChar);
                }

                // Warning - The reported character {0} is ambiguous. There are multiple keys for the same
                // character, each representing a different expected character. However, at most, only one
                // of the expected characters is invariant.
                token = LogCalibrationInformation(
                    token,
                    InformationType.MultipleKeysNonInvariantCharacters,
                    reportedChar.ToControlPictureString(),
                    $"{differences[reportedChar]} {expectedChar.ToControlPictureString()}");
                differences[reportedChar] = char.MinValue;
            }
            else {
                differences.Add(reportedChar, expectedChar);
            }

            continue;

            void AppendReportedCharacter(StringBuilder mappedCharacters) =>
                mappedCharacters.Append(reportedChar switch {
                    _ when reportedChar < 32 => (char)(reportedChar + 9216),
                    _ => reportedChar
                });
        }

        // Remove any entries for unrecognised characters.
        differences = differences
                     .Where(d => d.Value != char.MinValue)
                     .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        _tokenExtendedDataReportedCharacters += regExBuilder;
        return token;
    }

    /// <summary>
    ///   Detect unrecognised characters, chained dead key characters, dead key sequences and ligatures.
    /// </summary>
    /// <param name="token">The calibration token.</param>
    /// <param name="segment">A list of sequences within a segment.</param>
    /// <param name="expectedSegment">A list of the expected sequences within a segment.</param>
    /// <param name="chainedSequences">A dictionary of chained dead key sequences. This is populated by this procedure.</param>
    /// <param name="splitSequences">
    ///   A dictionary of split sequences. These generally represent dead key sequences followed by
    ///   a normal sequence.
    /// </param>
    /// <param name="invariant">True, if the segment represents invariant characters; otherwise false.</param>
    /// <returns>The amended calibration token.</returns>
    // ReSharper disable once SuggestBaseTypeForParameter
    private Token DetectUnrecognisedOrComplexCharacters(
        Token token,
        IList<string> segment,
        IList<string> expectedSegment,
        Dictionary<int, string> chainedSequences,
        Dictionary<int, IList<string>> splitSequences,
        bool invariant = false) {
        var unrecognisedCharacters = new List<string>();
        var validSequences = new Dictionary<string, string>();
        chainedSequences.Clear();
        var offsetRight = 0;

        string? scannerDeadKeyFirstMapping = null;
        var firstPass = false;

        // For each character sequence in the segment, detect unrecognised characters and fix up input for dead keys,
        // producing the basis for the character map.
        for (var sequenceIdx = 0; sequenceIdx < segment.Count; sequenceIdx++) {
            // Get the character sequence.
            var sequence = segment[sequenceIdx];

            // If no character has been reported, or if a single ASCII 0 has been reported, the scanned character
            // could not be recognised by the barcode scanner, according to its keyboard layout.
            if (sequence.Length == 0 || sequence is ['\u0000']) {
                // This is an unrecognised character
                if (expectedSegment.Count != 0 && expectedSegment.Count > sequenceIdx + offsetRight) {
                    // Add the character to the list of unrecognised characters
                    unrecognisedCharacters.Add(expectedSegment[sequenceIdx + offsetRight]);
                }
            }
            else {
                // Don't handle here if this sequence represents a barcode scanner dead key
                // ReSharper disable once StyleCop.SA1408
                if (sequence is not ['\u0000', '\u0020']) {
                    // If any two sequences are identical, it will not be possible to disambiguate them. Detect any ambiguities, and
                    // return immediately if they are detected for invariant characters.
                    if (validSequences.TryGetValue(sequence, out var sequenceValue)) {
                        if (invariant) {
#pragma warning disable SA1118 // Parameter should not span multiple lines
                            return LogCalibrationInformation(
                                token,
                                sequence.Length == 1
                                    ? InformationType.MultipleKeys
                                    : InformationType.MultipleSequences,
                                sequence.ToControlPictures(),
                                $"{sequenceValue.ToControlPictures()} {expectedSegment[sequenceIdx + offsetRight].ToControlPictures()}");
#pragma warning restore SA1118 // Parameter should not span multiple lines
                        }

                        // Warning - Some reported character sequences are ambiguous. These characters do not represent invariant characters: {0}
                        token = LogCalibrationInformation(
                            token,
                            InformationType.NonInvariantCharacterSequence,
                            sequence.ToControlPictures(),
                            $"{validSequences[sequence].ToControlPictures()} {expectedSegment[sequenceIdx + offsetRight].ToControlPictures()}");
                    }
                    else {
                        if (expectedSegment.Count > sequenceIdx) {
                            validSequences.Add(sequence, expectedSegment[sequenceIdx]);
                        }
                    }
                }
            }

            /* The barcode scanner has its own keyboard layout which may vary from that used by the operating system. The character in the
             * baseline barcode may require entry on the barcode scanner 'keyboard' using a dead key - a key that apparently does nothing when
             * you press it, but which modifies the next key. Because this is the barcode scanner keyboard, the barcode scanner needs to output the
             * correct scan codes (key presses) to type in the literal dead key character. By convention, most keyboards support this
             * by pressing the dead key followed by the space bar. This works great unless the operating system keyboard layout does
             * not support any equivalent key to the dead key. This often happens, for example, with barcode scanner keyboard layouts that
             * use AltGr + Key to type in a dead key. There is a good chance that the same key combination on the operating system
             * keyboard is not supported. In this case, the reported character sequence will be an ASCII 0 (for the non-supported
             * key press) followed by a space. This is a problem if we have two or more instances of such barcode scanner dead key issues, as
             * there is no way to disambiguate. We have the same reported sequence of characters for two or more expected characters.
             * */

            // Test to see if this is a barcode scanner dead key that does not map to any key on the OS keyboard (e.g. AltGR-key combination)
            if (sequence is ['\u0000', '\u0020']) {
                // Check to see if we have a non-mapped barcode scanner dead key sequence before.
                if (scannerDeadKeyFirstMapping is not null) {
                    // 'invariant' indicates if we are processing the invariant characters segment. If so, we have to
                    // treat this as an error that prevents calibration.
                    if (invariant) {
                        token = expectedSegment.Count > sequenceIdx + offsetRight

                            // Error - The reported character sequence {0} is ambiguous. The same sequence is reported for multiple dead keys in the
                            // barcode scanner's keyboard layout.
                            ? LogCalibrationInformation(
                                token,
                                InformationType.MultipleSequencesForScannerDeadKey,
                                sequence.ToControlPictures(),
                                $"{scannerDeadKeyFirstMapping.ToControlPictures()} {expectedSegment[sequenceIdx + offsetRight].ToControlPictures()}")
                            : token;

                        // Error - The character {0} cannot be represented reliably because of incompatibility with the keyboard layout.
                        token = LogCalibrationInformation(
                            token,
                            InformationType.IncompatibleScannerDeadKey,
                            sequence.ToControlPictures(),
                            scannerDeadKeyFirstMapping.ToControlPictures());
                    }
                    else {
                        if (!firstPass) {
                            firstPass = true;
                            LogCalibrationInformation(
                                token,
                                InformationType.SomeNonInvariantCharactersUnrecognised,
                                null,
                                scannerDeadKeyFirstMapping.ToControlPictures());
                        }

                        token = expectedSegment.Count > sequenceIdx + offsetRight

                            // Warning - Some non-invariant characters are not recognised: {0}
                            ? LogCalibrationInformation(
                                token,
                                InformationType.SomeNonInvariantCharactersUnrecognised,
                                null,
                                expectedSegment[sequenceIdx + offsetRight].ToControlPictures())
                            : token;
                    }
                }
                else {
                    if (expectedSegment.Count > sequenceIdx + offsetRight) {
                        // Add the non-mapped barcode scanner dead key sequence to the barcode scanner dead keys collection.
                        scannerDeadKeyFirstMapping = expectedSegment[sequenceIdx + offsetRight];
                    }
                }
            }

            /* Some keyboards support dead key chaining in which a sequence of dead keys is pressed before they modify the final key.
             * Chaining dead keys is very rare. We know of no examples for European keyboards. One US example is a Cherokee Phonetic
             * keyboard where, for example, typing hna will yield the sequence \0\0Ꮏ. Cherokee script is syllabic rather than alphabetic.
             * We are only concerned with dead key chaining on the operating system keyboard layout, and not the barcode scanner keyboard
             * layout, which we detect elsewhere.
             *
             * To detect dead key chains, look for two or more ASCII 0s followed by a character.
             * */

            // If dead keys are chained, collect the sequence.
#if NET7_0_OR_GREATER
            if (ChainedDeadKeysFilter2Regex().Matches(sequence).Count > 0)
#else
            if (ChainedDeadKeysFilter2Regex.Matches(sequence).Count > 0)
#endif
            {
                // Dead key chaining is being used in this sequence. This will be normalized later for further processing.
                chainedSequences.Add(sequenceIdx, sequence);
            }

            /* If the character in the barcode is entered using a normal key in the barcode scanner keyboard layout, but the scan code sent to the
             * operating system is a dead key in its keyboard layout, the effect will be to consume the space delimiter between the current
             * reported character and the next character. This is because the space delimiter between the two characters will be modified
             * by the dead key. In most cases, the operating system outputs the literal dead character, without a space. On a few
             * keyboards, the character output by the operating system is not the literal dead key character. In these cases, the
             * character is typically a standard ASCII character that is reminiscent of the literal character. We have no way of detecting
             * this at this point in the code, but we will detect this issue later during calibration as a result of processing
             * additional barcodes.
             *
             * A dead key sequence (a sequence that starts with a dead key on the operating system keyboard layout) will always start with
             * an ASCII 0 followed by the literal dead key character (or some other character) followed by at least one more character
             * representing what would have been the next sequence, had the space not been modified (and effectively eliminated) by the
             * dead key. Of course, that next sequence might, itself, be a dead key sequence, so there could be multiple dead keys in
             * the sequence. There could also be unrecognised characters. In this case we may not be able to detect them here because of
             * the space delimiter loss. However, we will detect this later when we realize we have failed to identify the correct number
             * of characters.
             *  */

            // If a dead key sequence is followed by an unrecognised character, it will be just two characters, and there is no need to
            // process it. We only need to process longer sequences.
            if (sequence.Length < 2 || sequence is ['\u0000', _]) {
                continue;
            }

            // If the sequence starts with an ASCII 0 then it needs to be split into inner sequences.
            // NB. This is redundant, but has been retained in case it is needed in the future.
            if (sequence[0] == '\u0000') {
                // Calculate the offset to the index into the expected segment array.
                var splitSequenceOut = SplitSequence(sequence);
                offsetRight += splitSequenceOut.Count - 1;

                // Add the list of split sequences to the split sequence dictionary.
                splitSequences.Add(sequenceIdx, splitSequenceOut);
            }
            else
            {
                if (expectedSegment.Count <= sequenceIdx + offsetRight) continue;

                /* If we have multiple characters in the sequence, but the first one is not an ASCII 0, this must represent a
                 * ligature, unless the second character is a space. If the second character is a space, the most likely reason
                 * is that a character in the barcode has been entered using a dead key, followed by a space, in the barcode
                 * scanner keyboard.
                 */
                if (sequence is [_, ' ']) {
                    // We have probably already detected and registered the scanner dead key, but we will add it here, if necessary.
                    var key = expectedSegment[sequenceIdx + offsetRight].First().ToInvariantString();
                    if (!_tokenExtendedDataScannerDeadKeysMap.ContainsKey(key)) {
                        _tokenExtendedDataScannerDeadKeysMap.Add(
                            expectedSegment[sequenceIdx + offsetRight].First().ToInvariantString(),
                            sequence);
                    }
                }
                else {
                    /* A ligature occurs when a single key produces multiple characters. There is no known use of this
                         * on any European keyboards for modern languages, but it is used on some other keyboards. We will collect the
                         * ligature mapping.
                         * */
                    _tokenExtendedDataLigatureMap.Add(sequence, expectedSegment[sequenceIdx + offsetRight].First());
                }
            }
        }

        // Log errors and warnings for unrecognised characters.
        if (invariant) {
            // Error - Some invariant characters are not recognised by the barcode scanner in its current configuration: {0}
            return unrecognisedCharacters.Count > 0
                       ? LogCalibrationInformation(
                           token,
                           InformationType.SomeInvariantCharactersUnrecognised,
                           null,
                           unrecognisedCharacters.Aggregate((a, c) => $"{a} {c.ToControlPictures()}"))
                       : token;
        }

        return unrecognisedCharacters.Count > 0

                   // Warning - Some non-invariant characters are not recognised: {0}
                   ? LogCalibrationInformation(
                       token,
                       InformationType.SomeNonInvariantCharactersUnrecognised,
                       null,
                       unrecognisedCharacters.Aggregate((a, c) => $"{a} {c.ToControlPictures()}"))
                   : token;
    }

    /// <summary>
    ///   Fix up any sequence in which a dead key is followed by one or more ASCII control characters.
    /// </summary>
    /// <param name="input">
    ///   The input to be processed.
    /// </param>
    /// <returns>The fixed-up input.</returns>
    private string FixDeadKeyAsciiControlCharacterSequence(string input) {
        /* If the reported character sequence contains a dead key followed by one or more ASCII control
         * characters (including ASCII 0), the dead key literal will appear at the end of the sequence.
         * This will lead to incorrect parsing of the data. We need to move the literal character back
         * to immediately after the first ASCII 0.
         * */
        var deadKeyCharacters = new StringBuilder();
        char[] charsForEscape = ['.', '$', '^', '{', '[', '(', '|', ')', '*', '+', '?', '\\'];

        if (_tokenExtendedDataDeadKeysMap.Keys.Count <= 0) {
            return input;
        }

        foreach (var (key, value) in _tokenExtendedDataDeadKeysMap) {
            if (value.Length != 1) {
                continue;
            }

            var keyChar = key[1];

            deadKeyCharacters.Append(charsForEscape.Contains(keyChar) ? "\\" : string.Empty);
            deadKeyCharacters.Append(key[1]);
        }

        if (deadKeyCharacters.Length <= 0) return input;
        var matches = Regex.Matches(input, $"\u0000(?<cc>[\u0000-\u001F]*)(?<dkc>[{deadKeyCharacters}])");

        foreach (var match in matches.OfType<Match>()) {
            var reversed = $"\u0000{match.Groups["dkc"]}{match.Groups["cc"]}";
            input = $"{input[..match.Index]}{reversed}{input[(match.Index + match.Length)..]}";
        }

        return input;
    }

    /// <summary>
    ///   Return a list of expected segments of character sequences. They represent the characters in the baseline barcode.
    /// </summary>
    /// <returns>The list of expected segments of character sequences.</returns>
    private List<List<string>> ExpectedSegments() {
        // Create a list of expected characters (i.e., the characters expected as a result of successful mapping).
        string expected =
            "\u0020\u0020!\u0020\u0022\u0020%\u0020&\u0020'\u0020(\u0020)\u0020*\u0020+\u0020,\u0020-\u0020.\u0020/\u00200\u00201\u00202\u00203\u00204\u00205\u00206\u00207\u00208\u00209\u0020:\u0020;\u0020<\u0020=\u0020>\u0020?\u0020A\u0020B\u0020C\u0020D\u0020E\u0020F\u0020G\u0020H\u0020I\u0020J\u0020K\u0020L\u0020M\u0020N\u0020O\u0020P\u0020Q\u0020R\u0020S\u0020T\u0020U\u0020V\u0020W\u0020X\u0020Y\u0020Z\u0020_\u0020a\u0020b\u0020c\u0020d\u0020e\u0020f\u0020g\u0020h\u0020i\u0020j\u0020k\u0020l\u0020m\u0020n\u0020o\u0020p\u0020q\u0020r\u0020s\u0020t\u0020u\u0020v\u0020w\u0020x\u0020y\u0020z\u0020\u0020#\u0020$\u0020@\u0020[\u0020\u005C\u0020]\u0020^\u0020`\u0020{\u0020|\u0020}\u0020\u007E\u0020\u0020\u001D\u0020\u0020" +
                (AssessFormatSupport
                ? "\u001c\u0020\u0020\u001e\u0020\u0020\u001f\u0020\u0020\u0004\u0020\u0020"
                : "\u0020\u0020\u0020\u0020\u0020\u0020");

        var delimiter = new[] { new string('\u0020', 2) };
        _tokenExtendedDataReportedCharacters = string.Empty;

        // Split the expected characters into segments.
        var expectedSegmentList = expected.Split(delimiter, StringSplitOptions.None).ToList();
        var expectedSegments = new List<List<string>>();

        // Split each expected segment into character sequences.
        expectedSegmentList.ForEach(segment => expectedSegments.Add([.. segment.Split('\u0020')]));

        return expectedSegments;
    }

    /// <summary>
    ///   Initializes the keyboard calibrator from extended data stored in the calibration token.
    /// </summary>
    /// <param name="token">The calibration token.</param>
    /// <returns>The updated calibration token.</returns>
    private Token DoInitializeFromTokenData(Token token) {
        // Store token state
        _tokenBitmapStream = token.BitmapStream;
        _tokenSvg = token.Svg;
        _tokenRemaining = token.Remaining;
        _tokenSize = token.Size;
        _tokenKeyboardMatch = token.KeyboardMatch;
        _tokenCalibrationData = token.CalibrationData;
        _tokenSystemCapabilities = token.SystemCapabilities;
        _tokenCalibrationSessionAbandoned = token.CalibrationSessionAbandoned;

        if (token.ToJson().Replace(" ", string.Empty) == "{}") {
            _tokenDataBarcodeData = string.Empty;
            _tokenDataKey = default;
            _tokenDataValue = default;
            _tokenDataCalibrationsRemaining = default;
            _tokenDataSmallBarcodeSequenceIndex = default;
            _tokenDataSmallBarcodeSequenceCount = default;
            _tokenDataPrefix = default;
            _tokenDataSuffix = default;
            _tokenDataReportedCharacters = default;
        }
        else {
            // Store token state
            _tokenDataBarcodeData = token.Data?.BarcodeData ?? string.Empty;
            _tokenDataKey = token.Data?.Key;
            _tokenDataValue = token.Data?.Value ?? char.MinValue;
            _tokenDataCalibrationsRemaining = token.Data?.CalibrationsRemaining ?? 0;
            _tokenDataSmallBarcodeSequenceIndex = token.Data?.SmallBarcodeSequenceIndex ?? 0;
            _tokenDataSmallBarcodeSequenceCount = token.Data?.SmallBarcodeSequenceCount ?? 0;
            _tokenDataPrefix = (token.Data?.Prefix ?? string.Empty) == string.Empty ? _tokenDataPrefix : token.Data?.Prefix;
            _tokenDataSuffix = (token.Data?.Suffix ?? string.Empty) == string.Empty ? _tokenDataSuffix : token.Data?.Suffix;
            _tokenDataReportedCharacters = token.Data?.ReportedCharacters ?? string.Empty;
        }

        _tokenInformation?.Clear();
        _tokenWarnings?.Clear();
        _tokenErrors?.Clear();

        foreach (var information in token.Information) {
            _tokenInformation ??= [];
            _tokenInformation.Add(information);
        }

        foreach (var warning in token.Warnings) {
            _tokenWarnings ??= [];
            _tokenWarnings.Add(warning);
        }

        foreach (var error in token.Errors) {
            _tokenErrors ??= [];
            _tokenErrors.Add(error);
        }

        if (token.ExtendedData is null) {
            return token;
        }

        if (token.ToJson().Replace(" ", string.Empty) == "{}") {
            _tokenExtendedDataScannerDeadKeysMap = new Dictionary<string, string>();
            _tokenExtendedDataScannerUnassignedKeys = [];
            _tokenExtendedDataLigatureMap = new Dictionary<string, char>();
            _tokenExtendedDataDeadKeysMap = new Dictionary<string, string>();
            _tokenExtendedDataAimFlagCharacterSequence = "]";
            _tokenExtendedDataReportedCharacters = default;
            _tokenExtendedDataPrefix = default;
            _tokenExtendedDataCode = default;
            _tokenExtendedDataSuffix = default;
            _tokenExtendedDataReportedPrefix = default;
            _tokenExtendedDataReportedCode = default;
            _tokenExtendedDataReportedSuffix = default;
            _tokenExtendedDataKeyboardScript = default;
            _tokenExtendedDataScannerKeyboardPerformance = ScannerKeyboardPerformance.High;
            _tokenExtendedDataScannerCharactersPerSecond = 0;
            _tokenExtendedDataCharacterMap = new Dictionary<char, char>();
            _tokenExtendedDataDeadKeyCharacterMap = new Dictionary<string, char>();
            _tokenExtendedDataDeadKeyFixUp = [];
            _tokenExtendedDataPotentialIsoIec15434Unreadable30 = default;
            _tokenExtendedDataUnreadableFs = default;
            _tokenExtendedDataUnreadableUs = default;
            _tokenExtendedDataEotUnreadable = default;
            AssessFormatSupport = true;
            _tokenExtendedDataUnrecognisedKeys = [];
        }
        else {
            // Store token extended data
            _tokenExtendedDataScannerDeadKeysMap = token.ExtendedData.ScannerDeadKeysMap;
            _tokenExtendedDataScannerUnassignedKeys = token.ExtendedData.ScannerUnassignedKeys;
            _tokenExtendedDataLigatureMap = token.ExtendedData.LigatureMap;
            _tokenExtendedDataDeadKeysMap = token.ExtendedData.DeadKeysMap;
            _tokenExtendedDataAimFlagCharacterSequence = token.ExtendedData.AimFlagCharacterSequence;
            _tokenExtendedDataReportedCharacters = token.ExtendedData.ReportedCharacters;
            _tokenExtendedDataPrefix = token.ExtendedData.Prefix;
            _tokenExtendedDataCode = token.ExtendedData.Code;
            _tokenExtendedDataSuffix = token.ExtendedData.Suffix;
            _tokenExtendedDataReportedPrefix = token.ExtendedData.ReportedPrefix;
            _tokenExtendedDataReportedCode = token.ExtendedData.ReportedCode;
            _tokenExtendedDataReportedSuffix = token.ExtendedData.ReportedSuffix;
            _tokenExtendedDataKeyboardScript = token.ExtendedData.KeyboardScript;
            _tokenExtendedDataScannerKeyboardPerformance = token.ExtendedData.ScannerKeyboardPerformance;
            _tokenExtendedDataScannerCharactersPerSecond = token.ExtendedData.ScannerCharactersPerSecond;
            _tokenExtendedDataCharacterMap = token.ExtendedData.CharacterMap;
            _tokenExtendedDataDeadKeyCharacterMap = token.ExtendedData.DeadKeyCharacterMap;
            _tokenExtendedDataDeadKeyFixUp = token.ExtendedData.DeadKeyFixUp;
            _tokenExtendedDataPotentialIsoIec15434Unreadable30 = token.ExtendedData.PotentialIsoIec15434Unreadable30;
            _tokenExtendedDataUnreadableFs = token.ExtendedData.PotentialIsoIec15434EdiUnreadableFs;
            _tokenExtendedDataUnreadableUs = token.ExtendedData.PotentialIsoIec15434EdiUnreadableUs;
            _tokenExtendedDataEotUnreadable = token.ExtendedData.PotentialIsoIec15434EotUnreadable;
            AssessFormatSupport = token.ExtendedData.AssessFormat06Support;
            _tokenExtendedDataUnrecognisedKeys = token.ExtendedData.UnrecognisedKeys;
        }

        return token;
    }

    /// <summary>
    ///   Initializes the extended token data in the calibration token.
    /// </summary>
    /// <returns>Returns the token with initialized extended data.</returns>
    private Token InitializeTokenData() {
        var @out = new Token(
            _tokenDataBarcodeData,
            _tokenDataKey,
            _tokenDataValue,
            _tokenDataCalibrationsRemaining,
            _tokenDataSmallBarcodeSequenceIndex,
            _tokenDataSmallBarcodeSequenceCount,
            _tokenDataPrefix,
            _tokenDataSuffix,
            _tokenDataReportedCharacters,
            _tokenBitmapStream,
            _tokenSvg,
            _tokenRemaining,
            _tokenSize,
            _tokenKeyboardMatch,
            _tokenCalibrationData,
            _tokenSystemCapabilities,
            _tokenCalibrationSessionAbandoned,
            _tokenReportedPrefixSegment,
            _tokenReportedSuffix);

        _tokenInformation?.ForEach(information => @out.AddInformation(information));
        _tokenWarnings?.ForEach(warning => @out.AddInformation(warning));
        _tokenErrors?.ForEach(error => @out.AddInformation(error));

        @out = Token.SetExtendedData(
            @out,
            _tokenExtendedDataDeadKeysMap,
            _tokenExtendedDataDeadKeyCharacterMap,
            _tokenExtendedDataDeadKeyFixUp,
            _tokenExtendedDataScannerDeadKeysMap,
            _tokenExtendedDataScannerUnassignedKeys,
            _tokenExtendedDataCharacterMap,
            _tokenExtendedDataLigatureMap,
            _tokenExtendedDataUnrecognisedKeys,
            _tokenExtendedDataPrefix ?? string.Empty,
            _tokenExtendedDataCode ?? string.Empty,
            _tokenExtendedDataSuffix ?? string.Empty,
            _tokenExtendedDataReportedPrefix ?? string.Empty,
            _tokenExtendedDataReportedCode ?? string.Empty,
            _tokenExtendedDataReportedSuffix ?? string.Empty,
            _tokenExtendedDataKeyboardScript ?? string.Empty,
            _tokenExtendedDataScannerKeyboardPerformance,
            _tokenExtendedDataScannerCharactersPerSecond,
            _tokenExtendedDataAimFlagCharacterSequence ?? string.Empty,
            _tokenExtendedDataReportedCharacters ?? string.Empty,
            _tokenExtendedDataPotentialIsoIec15434Unreadable30,
            _tokenExtendedDataUnreadableFs,
            _tokenExtendedDataUnreadableUs,
            _tokenExtendedDataEotUnreadable,
            AssessFormatSupport,
            _tokenExtendedDataNonInvariantAmbiguities,
            _tokenExtendedDataInvariantGs1Ambiguities,
            _tokenExtendedDataNonInvariantUnrecognisedCharacters,
            _tokenExtendedDataInvariantGs1UnrecognisedCharacters);

        return @out;
    }

    /// <summary>
    ///   Log the calibration information.
    /// </summary>
    /// <param name="token">The current token.</param>
    /// <param name="type">The information type.</param>
    /// <param name="reportedData">The reported character(s).</param>
    /// <param name="expectedData">The expected character(s).</param>
    /// <returns>The calibration token with logged information.</returns>
    private Token LogCalibrationInformation(
        Token token,
        InformationType type,
        string? reportedData = "",
        string? expectedData = null)
    {
        var description = Resources.ResourceManager.GetString(
            $"CalibrationInformation_{(int)type}",
            Thread.CurrentThread.CurrentUICulture);

        ProcessReportedLogData(type, reportedData, expectedData);

        // Add data to formattable strings.
        // ReSharper disable once SwitchStatementMissingSomeCases
        switch (type)
        {
            case InformationType.KeyboardScript:
            case InformationType.Platform:
            case InformationType.MultipleKeysNonInvariantCharacters:
            case InformationType.MultipleKeysMultipleNonInvariantCharacters:
            case InformationType.FileSeparatorMappingIsoIec15434EdiNotReliablyReadable:
            case InformationType.ControlCharacterMappingNonInvariants:
            case InformationType.MultipleKeys:
            case InformationType.DeadKeyMultiMapping:
            case InformationType.DeadKeyMultipleKeys:
            case InformationType.MultipleSequences:
            case InformationType.MultipleSequencesForScannerDeadKey:
            case InformationType.GroupSeparatorNotReliablyReadableInvariant:
            case InformationType.RecordSeparatorNotReliablyReadableInvariant:
            case InformationType.EotCharacterNotReliablyReadableInvariant:
            case InformationType.FileSeparatorNotReliablyReadableInvariant:
            case InformationType.FileSeparatorNotReliablyReadableNonInvariant:
            case InformationType.UnitSeparatorNotReliablyReadableInvariant:
            case InformationType.UnitSeparatorNotReliablyReadableNonInvariant:
            case InformationType.LigatureCharacters:
                description = string.Format(CultureInfo.InvariantCulture, description ?? string.Empty, reportedData);
                break;
            case InformationType.AimTransmitted:
            case InformationType.AimMayBeTransmitted:
            case InformationType.EndOfLineTransmitted:
                AppendDescription(token.Information.Where(ci => ci.InformationType == type));
                break;
            case InformationType.SomeNonInvariantCharactersUnrecognised:
            case InformationType.SomeNonInvariantCharacterCombinationsUnrecognised:
                AppendDescription(
                    token.Warnings.Where(ci => ci.InformationType == type), expectedData);
                break;
            case InformationType.PrefixTransmitted:
            case InformationType.CodeTransmitted:
            case InformationType.SuffixTransmitted:
            case InformationType.DeadKeyMultiMappingNonInvariantCharacters:
            case InformationType.NonInvariantCharacterSequence:
                AppendDescription(token.Warnings.Where(ci => ci.InformationType == type));
                break;
            case InformationType.SomeInvariantCharactersUnrecognised:
            case InformationType.SomeDeadKeyCombinationsUnrecognisedForInvariants:
            case InformationType.IncompatibleScannerDeadKey:
                AppendDescription(
                    token.Errors.Where(ci => ci.InformationType == type),
                    expectedData);
                break;
            case InformationType.CalibrationFailed:
            case InformationType.CalibrationFailedUnexpectedly:
                AppendDescription(token.Warnings.Where(ci => ci.InformationType == type));
                break;
        }

        // Assign information if no entry yet exists.
        switch ((int)type)
        {
            case < 200 and >= 100:
                if (token.Information.All(ci => ci.InformationType != type))
                {
                    token.AddInformation(InformationLevel.Information, type, description);
                }

                break;
            case < 300 and >= 200:
                if (token.Warnings.All(ci => ci.InformationType != type))
                {
                    token.AddInformation(InformationLevel.Warning, type, description);
                }

                break;
            case < 400 and >= 300:
                // Add the 'Calibration failed' error.
                if (type != InformationType.CalibrationFailed
                    && type != InformationType.CalibrationFailedUnexpectedly
                    && token.Errors.All(ci => ci.InformationType != InformationType.CalibrationFailed))
                {
                    var errorDescription = Resources.ResourceManager.GetString(
                        $"CalibrationInformation_{(int)InformationType.CalibrationFailed}",
                        Thread.CurrentThread.CurrentUICulture);
                    errorDescription = string.Format(
                        CultureInfo.InvariantCulture,
                        errorDescription ?? string.Empty,
                        string.Empty);
                    token.AddInformation(
                        InformationLevel.Error,
                        InformationType.CalibrationFailed,
                        errorDescription);
                }

                if (token.Errors.All(ci => ci.InformationType != type))
                {
                    token.AddInformation(InformationLevel.Error, type, description);
                }

                break;
        }

        DoInitializeFromTokenData(token);
        return token;

        void AppendDescription(IEnumerable<Information> information, string? data = null)
        {
            var calibrationInformation = information as Information[] ?? information.ToArray();
            var descriptionData = string.IsNullOrEmpty(data) ? reportedData : data;

            if (!string.IsNullOrWhiteSpace(descriptionData) && calibrationInformation.Length == 1)
            {
                var enumerator = calibrationInformation.GetEnumerator();
                using var disposableEnumerator = enumerator as IDisposable;

                if (enumerator.MoveNext() && enumerator.Current is not null)
                {
                    ((Information)enumerator.Current).Description += $" {descriptionData}";
                }
            }

            description = string.Format(CultureInfo.InvariantCulture, description ?? string.Empty, descriptionData);
        }
    }

    /// <summary>
    /// Process the reported data when logging calibration information.
    /// </summary>
    /// <param name="type">The calibration information type.</param>
    /// <param name="reportedData">The reported character sequence.</param>
    /// <param name="expectedData">The expected character sequences.</param>
    /// <remarks>
    ///   This method assumes that the space is a delimiter between different expected character
    ///   sequences and not part of a character sequence.
    /// </remarks>
    private void ProcessReportedLogData(
        InformationType type,
        string? reportedData = "",
        string? expectedData = null) {
        if (expectedData is not { Length: > 0 }) {
            return;
        }

        var expectedCharacters = new char[expectedData.Length];

        for (var idx = 0; idx < expectedData.Length; idx++) {
            expectedCharacters[idx] = expectedData[idx].ToControlPicture();
        }

        var expectedSequences = new string(expectedCharacters).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (type) {
            case InformationType.MultipleKeysNonInvariantCharacters:
            case InformationType.MultipleKeysMultipleNonInvariantCharacters:
            case InformationType.DeadKeyMultiMappingNonInvariantCharacters:
            case InformationType.RecordSeparatorNotReliablyReadableInvariant:
            case InformationType.EotCharacterNotReliablyReadableInvariant:
            case InformationType.FileSeparatorNotReliablyReadableInvariant:
            case InformationType.FileSeparatorNotReliablyReadableNonInvariant:
            case InformationType.UnitSeparatorNotReliablyReadableInvariant:
            case InformationType.UnitSeparatorNotReliablyReadableNonInvariant:
            case InformationType.NonInvariantCharacterSequence:
                AddAmbiguity(_tokenExtendedDataNonInvariantAmbiguities);
                break;
            case InformationType.MultipleKeys:
            case InformationType.DeadKeyMultiMapping:
            case InformationType.DeadKeyMultipleKeys:
            case InformationType.MultipleSequences:
            case InformationType.MultipleSequencesForScannerDeadKey:
            case InformationType.GroupSeparatorNotReliablyReadableInvariant:
                AddAmbiguity(_tokenExtendedDataInvariantGs1Ambiguities);
                break;
            case InformationType.LigatureCharacters:
                break;
            case InformationType.SomeNonInvariantCharactersUnrecognised:
            case InformationType.SomeNonInvariantCharacterCombinationsUnrecognised:
                AddUnrecognisedCharacter(_tokenExtendedDataNonInvariantUnrecognisedCharacters);
                break;
            case InformationType.SomeInvariantCharactersUnrecognised:
            case InformationType.SomeDeadKeyCombinationsUnrecognisedForInvariants:
                AddUnrecognisedCharacter(_tokenExtendedDataInvariantGs1UnrecognisedCharacters);
                break;
            case InformationType.IncompatibleScannerDeadKey:
                AddUnrecognisedCharacter((int)expectedData[0] switch {
                    >= 0 and < 29 => _tokenExtendedDataNonInvariantUnrecognisedCharacters,
                    >= 31 and <= 32 => _tokenExtendedDataNonInvariantUnrecognisedCharacters,
                    < 35 => _tokenExtendedDataInvariantGs1UnrecognisedCharacters,
                    >= 35 and < 37 => _tokenExtendedDataNonInvariantUnrecognisedCharacters,
                    >= 37 and < 64 => _tokenExtendedDataInvariantGs1UnrecognisedCharacters,
                    64 => _tokenExtendedDataNonInvariantUnrecognisedCharacters,
                    < 91 => _tokenExtendedDataInvariantGs1UnrecognisedCharacters,
                    >= 91 and < 95 => _tokenExtendedDataNonInvariantUnrecognisedCharacters,
                    95 => _tokenExtendedDataInvariantGs1UnrecognisedCharacters,
                    96 => _tokenExtendedDataNonInvariantUnrecognisedCharacters,
                    < 123 => _tokenExtendedDataInvariantGs1UnrecognisedCharacters,
                    >= 123 => _tokenExtendedDataNonInvariantUnrecognisedCharacters
                });
                break;
        }

        return;

        // Record an unrecognised character.
        void AddUnrecognisedCharacter(ICollection<string> unrecognisedCharacters) {
            foreach (var sequence in expectedSequences) {
                if (!unrecognisedCharacters.Contains(sequence)) {
                    unrecognisedCharacters.Add(sequence);
                }
            }
        }

        // Record an ambiguity.
        void AddAmbiguity(Dictionary<string, IList<string>> ambiguities) {
            foreach (var sequence in expectedSequences) {
                if (!string.IsNullOrWhiteSpace(reportedData) && ambiguities.TryAdd(reportedData, [sequence])) {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(reportedData) && ambiguities.TryGetValue(reportedData, out var ambiguity)) {
                    if (ambiguity.Contains(sequence)) {
                        continue;
                    }

                    ambiguity.Add(sequence);
                }
                else {
                    var parameter = reportedData ?? "<null>";

                    throw new ArgumentException(
                        $"Unexpected error when recording ambiguity for reported character {parameter}");
                }
            }
        }
    }

    /// <summary>
    ///   Pre-processes the AIM identifier, if it exists. It may be necessary to pre-process the identifier
    ///   to ensure it is recognised before processing the rest of the input.
    /// </summary>
    /// <param name="input">The data transmitted by a barcode scanner.</param>
    /// <returns>The pre-processed AIM identifier, if an AIM identifier was reported.</returns>
    private string PreProcessAimIdentifier(string? input) {
        var processedPrefixData = ProcessReportedPrefix(input, out _, out _, out _);
        Match match;

        // Test to see if the candidate AIM identifier was a real AIM identifier or not.
        if (string.IsNullOrEmpty(processedPrefixData) || !MatchWithAimId().Success) {
            return string.Empty;
        }

        return match.Groups["characters"].Value;

        Match MatchWithAimId() =>

#if NET7_0_OR_GREATER
            match = AimIdentifierMatchRegex().Match(processedPrefixData);
#else
            match = AimIdentifierMatchRegex.Match(processedPrefixData);
#endif
    }

    /// <summary>
    ///   Process any AIM candidate according to the character map.
    /// </summary>
    /// <param name="token">The current calibration token.</param>
    /// <returns>The calibration token.</returns>
    private Token DoProcessAimCandidate(Token token) {
        // AIM identifiers will be reported according to the OS keyboard layout.
        if (token.ReportedPrefixSegment is null ||
            token.ReportedPrefixSegment.Count <= 0) {
            return token;
        }

        var idx = 0;
        var reportedPrefixData = new StringBuilder();

        while (idx < token.ReportedPrefixSegment.Count) {
            reportedPrefixData.Append(token.ReportedPrefixSegment[idx]);

            if (token.ReportedPrefixSegment[idx++][0] != '\u0000') {
                break;
            }
        }

        _ = ProcessReportedPrefix(
            reportedPrefixData.ToString(),
            out var rawData,
            out var normalisedData,
            out var aimIdUncertain);

        if (string.IsNullOrEmpty(normalisedData.aimId)) {
            // Warning - The barcode scanner is transmitting a prefix: [prefix]
            token = LogCalibrationInformation(token, InformationType.AimNotTransmitted);
        }
        else {
#pragma warning disable SA1118 // Parameter should not span multiple lines
            token = LogCalibrationInformation(
                token,
                aimIdUncertain
                    ? InformationType.AimMayBeTransmitted
                    : InformationType.AimTransmitted,
                normalisedData.aimId.ToControlPictures());
#pragma warning restore SA1118 // Parameter should not span multiple lines
        }

        if (!string.IsNullOrEmpty(normalisedData.prefix)) {
            // Warning - The barcode scanner is transmitting a prefix: [prefix]
            token = LogCalibrationInformation(
                token,
                InformationType.PrefixTransmitted,
                normalisedData.prefix.ToControlPictures());
        }

        if (!string.IsNullOrEmpty(normalisedData.code)) {
            // Warning - The barcode scanner is transmitting a prefix: [prefix]
            token = LogCalibrationInformation(
                token,
                InformationType.CodeTransmitted,
                normalisedData.code.ToControlPictures());
        }

        token = SetPrefixAndCode();

        _tokenExtendedDataPrefix = normalisedData.prefix;
        _tokenExtendedDataCode = normalisedData.code;
        _tokenExtendedDataReportedPrefix = rawData.prefix;
        _tokenExtendedDataReportedCode = rawData.prefix;

        return token;

        // Update the immutable token with the prefix and code - this code can
        // be replaced if Calibration Token is changed to a record.
        Token SetPrefixAndCode() {
#pragma warning disable SA1118 // Parameter should not span multiple lines
            return new Token(
                token,
                token.ExtendedData is not null
                    ? new TokenExtendedData(
                        deadKeysMap: token.ExtendedData.DeadKeysMap,
                        deadKeyCharacterMap: token.ExtendedData.DeadKeyCharacterMap,
                        deadKeyFixUp: token.ExtendedData.DeadKeyFixUp,
                        scannerDeadKeysMap: token.ExtendedData.ScannerDeadKeysMap,
                        scannerUnassignedKeys: token.ExtendedData.ScannerUnassignedKeys,
                        characterMap: token.ExtendedData.CharacterMap,
                        ligatureMap: token.ExtendedData.LigatureMap,
                        unrecognisedKeys: token.ExtendedData.UnrecognisedKeys,
                        prefix: normalisedData.prefix,
                        code: normalisedData.code,
                        suffix: token.ExtendedData.Suffix,
                        reportedPrefix: rawData.prefix,
                        reportedCode: rawData.code,
                        reportedSuffix: token.ExtendedData.ReportedSuffix,
                        keyboardScript: token.ExtendedData.KeyboardScript,
                        scannerKeyboardPerformance: token.ExtendedData.ScannerKeyboardPerformance,
                        scannerCharactersPerSecond: token.ExtendedData.ScannerCharactersPerSecond,
                        aimFlagCharacterSequence: token.ExtendedData.AimFlagCharacterSequence,
                        reportedCharacters: token.ExtendedData.ReportedCharacters,
                        potentialIsoIec15434Unreadable30: token.ExtendedData.PotentialIsoIec15434Unreadable30,
                        potentialIsoIec15434EdiUnreadableFs: token.ExtendedData.PotentialIsoIec15434EdiUnreadableFs,
                        potentialIsoIec15434EdiUnreadableUs: token.ExtendedData.PotentialIsoIec15434EdiUnreadableUs,
                        potentialIsoIec15434EotUnreadable: token.ExtendedData.PotentialIsoIec15434EotUnreadable,
                        assessFormat06Support: token.ExtendedData.AssessFormat06Support,
                        nonInvariantAmbiguities: token.ExtendedData.NonInvariantAmbiguities,
                        invariantGs1Ambiguities: token.ExtendedData.InvariantGs1Ambiguities,
                        nonInvariantUnrecognisedCharacters: token.ExtendedData.NonInvariantUnrecognisedCharacters,
                        invariantGs1UnrecognisedCharacters: token.ExtendedData.InvariantGs1UnrecognisedCharacters)
                    : null);
#pragma warning restore SA1118 // Parameter should not span multiple lines
        }
    }

    /// <summary>
    ///   Process a suffix according to the character map.
    /// </summary>
    /// <param name="token">The current calibration token.</param>
    /// <param name="reportedSuffix">The reported suffix character sequence.</param>
    /// <returns>The calibration token.</returns>
    private Token DoProcessSuffix(Token token, string reportedSuffix) {
        var extendedDataReportedSuffix = string.Empty;
        var extendedDataSuffix = string.Empty;

        var tokenOut = token.StartProcess()

        .If(TheScannerTransmittedASuffix)
            .Then(LogAWarningThatTheScannerTransmitsASuffix)
            .EndIf

        .End();

        _tokenExtendedDataReportedSuffix = extendedDataReportedSuffix;
        _tokenExtendedDataSuffix = extendedDataSuffix;
        return SetSuffix(tokenOut, extendedDataReportedSuffix);

        // Log information.
        EnvToken LogInformation(
            Token localToken,
            InformationType informationType,
            string data = "") =>
            () =>
                new Lazy<Token>(
                    LogCalibrationInformation(
                        localToken,
                        informationType,
                        data.ToControlPictures()));

        // Record and return any reported suffix.
        string TestAndSetReportedSuffix() {
            extendedDataReportedSuffix = reportedSuffix;
            return extendedDataReportedSuffix;
        }

        string NormalisedSuffix() {
            extendedDataSuffix = TestAndSetReportedSuffix().NormaliseCharacters(_tokenExtendedDataCharacterMap, _tokenExtendedDataDeadKeyCharacterMap, _tokenExtendedDataDeadKeysMap);
            return extendedDataSuffix;
        }

        // Log warning - The barcode scanner is transmitting a suffix: {0}.
        EnvToken LogAWarningThatTheScannerTransmitsASuffix(Token localToken) =>
            LogInformation(
                localToken,
                InformationType.SuffixTransmitted,
                NormalisedSuffix());

        // Test to see if the scanner transmitted a suffix.
        bool TheScannerTransmittedASuffix(Token localToken) =>
            !string.IsNullOrEmpty(reportedSuffix);

        // Update the immutable token with the suffix - this code can
        // be replaced if Calibration Token is changed to a record.
        Token SetSuffix(Token currentToken, string suffix) {
#pragma warning disable SA1118 // Parameter should not span multiple lines
            return new Token(
                currentToken,
                currentToken.ExtendedData is not null
                    ? new TokenExtendedData(
                        deadKeysMap: currentToken.ExtendedData.DeadKeysMap,
                        deadKeyCharacterMap: currentToken.ExtendedData.DeadKeyCharacterMap,
                        deadKeyFixUp: currentToken.ExtendedData.DeadKeyFixUp,
                        scannerDeadKeysMap: currentToken.ExtendedData.ScannerDeadKeysMap,
                        scannerUnassignedKeys: currentToken.ExtendedData.ScannerUnassignedKeys,
                        characterMap: currentToken.ExtendedData.CharacterMap,
                        ligatureMap: currentToken.ExtendedData.LigatureMap,
                        unrecognisedKeys: currentToken.ExtendedData.UnrecognisedKeys,
                        prefix: currentToken.ExtendedData.Prefix,
                        code: currentToken.ExtendedData.Code,
                        suffix: suffix,
                        reportedPrefix: currentToken.ExtendedData.ReportedPrefix,
                        reportedCode: currentToken.ExtendedData.ReportedCode,
                        reportedSuffix: currentToken.ExtendedData.ReportedSuffix,
                        keyboardScript: currentToken.ExtendedData.KeyboardScript,
                        scannerKeyboardPerformance: currentToken.ExtendedData.ScannerKeyboardPerformance,
                        scannerCharactersPerSecond: currentToken.ExtendedData.ScannerCharactersPerSecond,
                        aimFlagCharacterSequence: currentToken.ExtendedData.AimFlagCharacterSequence,
                        reportedCharacters: currentToken.ExtendedData.ReportedCharacters,
                        potentialIsoIec15434Unreadable30: currentToken.ExtendedData.PotentialIsoIec15434Unreadable30,
                        potentialIsoIec15434EdiUnreadableFs: currentToken.ExtendedData.PotentialIsoIec15434EdiUnreadableFs,
                        potentialIsoIec15434EdiUnreadableUs: currentToken.ExtendedData.PotentialIsoIec15434EdiUnreadableUs,
                        potentialIsoIec15434EotUnreadable: currentToken.ExtendedData.PotentialIsoIec15434EotUnreadable,
                        assessFormat06Support: currentToken.ExtendedData.AssessFormat06Support,
                        nonInvariantAmbiguities: currentToken.ExtendedData.NonInvariantAmbiguities,
                        invariantGs1Ambiguities: currentToken.ExtendedData.InvariantGs1Ambiguities,
                        nonInvariantUnrecognisedCharacters: currentToken.ExtendedData.NonInvariantUnrecognisedCharacters,
                        invariantGs1UnrecognisedCharacters: currentToken.ExtendedData.InvariantGs1UnrecognisedCharacters)
                    : null);
#pragma warning restore SA1118 // Parameter should not span multiple lines
        }
    }

    /// <summary>
    ///   Processes prefix data, normalizing it according the calibration character map,
    ///   converting into the correct sequence of characters. This method also returned parsed data for
    ///   any prefix, AIM ID and additional code or label.
    /// </summary>
    /// <param name="aimIdentifier">
    ///   The prefix data to be processed. This is data transmitted by the barcode scanner, as reported to
    ///   the application.
    /// </param>
    /// <param name="rawData">Raw data broken into prefix, AI and code.</param>
    /// <param name="normalisedData">Normalised data broken into prefix, AI and code.</param>
    /// <param name="aimIdUncertain">Indicates whether the AIM ID is uncertain (best endeavours).</param>
    /// <returns>A normalized string containing the processed prefix data.  The string is processed according
    /// to the calibration character map.</returns>
    private string ProcessReportedPrefix(
        string? aimIdentifier,
        out (string prefix, string aimId, string code) rawData,
        out (string prefix, string aimId, string code) normalisedData,
        out bool aimIdUncertain) {
        rawData = (string.Empty, string.Empty, string.Empty);
        normalisedData = (string.Empty, string.Empty, string.Empty);
        aimIdUncertain = false;

        if (string.IsNullOrEmpty(_tokenExtendedDataAimFlagCharacterSequence)) return aimIdentifier ?? string.Empty;

        var builder = new StringBuilder();
        var detectedAimFlagSequence = false;
        var initialIndex = 0;

        for (var idx = 0; idx < aimIdentifier?.Length; idx++) {
            if (aimIdentifier[idx] == _tokenExtendedDataAimFlagCharacterSequence[0]) {
                var flagCharacterSequence = _tokenExtendedDataAimFlagCharacterSequence;
                initialIndex = idx;

#pragma warning disable S127 // "for" loop stop conditions should be invariant
                for (var fcIdx = 1; fcIdx < _tokenExtendedDataAimFlagCharacterSequence.Length; fcIdx++) {
                    if (idx < aimIdentifier.Length - 1 && aimIdentifier[++idx] == _tokenExtendedDataAimFlagCharacterSequence[fcIdx]) continue;

                    flagCharacterSequence = string.Empty;
                    idx = initialIndex;
                }
#pragma warning restore S127 // "for" loop stop conditions should be invariant

                detectedAimFlagSequence = !string.IsNullOrEmpty(flagCharacterSequence);
                builder.Append(flagCharacterSequence);
            }
            else {
                builder.Append(aimIdentifier[idx]);
            }
        }

        var normalisedAimIdentifier = detectedAimFlagSequence
            ? builder.ToString().NormaliseCharacters(_tokenExtendedDataCharacterMap, _tokenExtendedDataDeadKeyCharacterMap, _tokenExtendedDataDeadKeysMap, _tokenExtendedDataAimFlagCharacterSequence)
            : string.Empty;

        Match match;

        var normalisedPrefix = string.Empty;
        var normalisedAimId = string.Empty;
        var normalisedCode = string.Empty;
        var rawAimId = string.Empty;
        var rawPrefix = string.Empty;
        var rawCode = string.Empty;
        var codeAsPrefix = false;

        // Test to see if the candidate AIM identifier was a real AIM identifier or not.
        if (string.IsNullOrEmpty(normalisedAimIdentifier) || !MatchWithAimId().Success) {
            if (MatchWithUnrecognisedFlag().Success) {
                normalisedPrefix = match.Groups["prefix"].Value;
                normalisedAimId = ']' + match.Groups["characters"].Value[1..];
                normalisedCode = match.Groups["code"].Value;
            }

            if (string.IsNullOrEmpty(normalisedAimId)) {
                // If the last three characters of the prefix start with an ASCII 0 or if the final characters
                // start with the detected AIM flag character sequence, followed by two characters, we will assume
                // that they represent an AIM identifier. Again, this is a best-endeavours approach and is not reliable.
                var aimTestLength = _tokenExtendedDataAimFlagCharacterSequence.Length + 2;

                if (normalisedPrefix.Length >= aimTestLength &&
                    normalisedPrefix[^aimTestLength..^2] == _tokenExtendedDataAimFlagCharacterSequence) {
                    normalisedAimId = normalisedPrefix[^aimTestLength..];
                    normalisedPrefix = normalisedPrefix[..^aimTestLength];
                }
                else if (normalisedPrefix.Length >= 3 && normalisedPrefix[^3..][0] == '\0') {
                    normalisedAimId = normalisedPrefix[^3..];
                    normalisedPrefix = normalisedPrefix[..^3];
                }
                else {
                    var normalisedPrefixOut = normalisedAimIdentifier;

                    if (string.IsNullOrEmpty(normalisedPrefixOut)) {
                        normalisedPrefixOut = builder.ToString().NormaliseCharacters(
                            _tokenExtendedDataCharacterMap,
                            _tokenExtendedDataDeadKeyCharacterMap,
                            _tokenExtendedDataDeadKeysMap,
                            _tokenExtendedDataAimFlagCharacterSequence);
                    }

                    rawData = (aimIdentifier ?? string.Empty, string.Empty, string.Empty);
                    normalisedData = (normalisedPrefixOut, string.Empty, string.Empty);
                    return normalisedPrefixOut;
                }
            }

            if (!string.IsNullOrEmpty(normalisedPrefix)) {
                // We need to work out how much of the current raw Prefix string constitutes the normalised Prefix
                for (var idx = 3; idx < rawPrefix.Length; idx++) {
                    if (aimIdentifier?[..idx]?.NormaliseCharacters(
                            _tokenExtendedDataCharacterMap,
                            _tokenExtendedDataDeadKeyCharacterMap,
                            _tokenExtendedDataDeadKeysMap,
                            _tokenExtendedDataAimFlagCharacterSequence) != normalisedPrefix) {
                        continue;
                    }

                    rawPrefix = aimIdentifier[..idx];
                    rawAimId = aimIdentifier[idx..];
                    break;
                }
            }
            else if (!string.IsNullOrEmpty(normalisedAimId)) {
                // We need to work out how much of the reported data string constitutes the normalised Aim ID
                for (var idx = 0; idx < aimIdentifier?.Length; idx++) {
                    if (aimIdentifier[..idx].NormaliseCharacters(
                            _tokenExtendedDataCharacterMap,
                            _tokenExtendedDataDeadKeyCharacterMap,
                            _tokenExtendedDataDeadKeysMap,
                            _tokenExtendedDataAimFlagCharacterSequence) != normalisedAimId) {
                        continue;
                    }

                    rawAimId = aimIdentifier[..idx];
                    break;
                }

                if (string.IsNullOrWhiteSpace(rawAimId)) rawAimId = aimIdentifier;
            }

            aimIdUncertain = true;
        }
        else {
            normalisedPrefix = match.Groups["prefix"].Value;
            normalisedAimId = match.Groups["characters"].Value;
            normalisedCode = match.Groups["code"].Value;

            rawAimId = detectedAimFlagSequence
                ? builder.ToString()[initialIndex..]
                : string.Empty;

            if (string.IsNullOrEmpty(normalisedPrefix) && !string.IsNullOrEmpty(normalisedCode)) {
                normalisedPrefix = normalisedCode;
                codeAsPrefix = true;
            }
            else {
                rawPrefix = detectedAimFlagSequence
                    ? builder.ToString()[..initialIndex]
                    : builder.ToString();
            }
        }

        if (!string.IsNullOrEmpty(normalisedCode)) {
            // We need to work out how much of the current raw AIM ID string constitutes the normalised AIM ID
            for (var idx = 3; idx < rawAimId?.Length; idx++) {
                if (rawAimId[..idx].NormaliseCharacters(
                        _tokenExtendedDataCharacterMap,
                        _tokenExtendedDataDeadKeyCharacterMap,
                        _tokenExtendedDataDeadKeysMap,
                        _tokenExtendedDataAimFlagCharacterSequence) != normalisedAimId) {
                    continue;
                }

                if (codeAsPrefix) {
                    rawPrefix = rawAimId[idx..];
                    normalisedCode = string.Empty;
                }
                else {
                    rawCode = rawAimId[idx..];
                }

                rawAimId = rawAimId[..idx];
                break;
            }
        }

        rawData = (rawPrefix, rawAimId ?? string.Empty, rawCode);
        normalisedData = (normalisedPrefix, normalisedAimId, normalisedCode);

        return normalisedAimIdentifier;

        Match MatchWithAimId() =>
#if NET7_0_OR_GREATER
            match = AimIdentifierMatchRegex().Match(normalisedAimIdentifier);
#else
            match = AimIdentifierMatchRegex.Match(normalisedAimIdentifier);
#endif
        Match MatchWithUnrecognisedFlag() =>
#if NET7_0_OR_GREATER
            match = AimIdentifierUnrecognisedFlagMatchRegex().Match(normalisedAimIdentifier);
#else
            match = AimIdentifierUnrecognisedFlagMatchRegex.Match(normalisedAimIdentifier);
#endif
    }

    /// <summary>
    ///   Process the reported ASCII control characters to determine any issues.
    /// </summary>
    /// <param name="token">The current calibration token.</param>
    /// <param name="reportedSegments">The reported segments of character sequences.</param>
    /// <param name="expectedSegments">The list of segments for expected characters.</param>
    /// <returns>The calibration token.</returns>
    // ReSharper disable once UnusedMethodReturnValue.Local
    private Token DoProcessAsciiControlCharacters(
        Token token,
        IList<List<string>> reportedSegments,
        IReadOnlyList<List<string>> expectedSegments) {
            for (var idx = Segments.GroupSeparatorSegment; idx < Segments.SuffixSegment; idx++) {
                if (reportedSegments[(int)idx].Count == 0) {
                    token = LogCalibrationInformation(
                        token,
                        idx switch
                        {
                            Segments.GroupSeparatorSegment => InformationType.GroupSeparatorNotReadable,
                            Segments.RecordSeparatorSegment => InformationType.RecordSeparatorNotReadable,
                            Segments.FileSeparatorSegment => InformationType.FileSeparatorNotReadable,
                            Segments.UnitSeparatorSegment => InformationType.UnitSeparatorNotReadable,
                            Segments.EndOfTransmissionSegment => InformationType.EotCharacterNotReadable,
                            _ => InformationType.IsoIec15434EdiNotReliablyReadable
                        });

                    continue;
                }

                var reportedControl = reportedSegments[(int)idx][0];
                var expectedControl = expectedSegments[(int)idx][0];
                var correspondence = true;

                if (reportedControl.StartsWith('\0')) {
                    if (reportedControl.Length > 1) {
                        // we need to account for any conflict with a previously logged 'null' mapping
                        if (_tokenExtendedDataCharacterMap.TryGetValue('\0', out char value)) {
    #if NET7_0_OR_GREATER
                            var ambiguousInvariant = InvariantsMatchRegex().IsMatch(value.ToInvariantString());
    #else
                            var ambiguousInvariant = InvariantsMatchRegex.IsMatch(_tokenExtendedDataCharacterMap['\0'].ToInvariantString());
    #endif
    #pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                            token = LogCalibrationInformation(
                                token,
                                idx switch {
                                    Segments.GroupSeparatorSegment => ambiguousInvariant
                                        ? InformationType.GroupSeparatorNotReliablyReadableInvariant
                                        : InformationType.ControlCharacterMappingNonInvariants,
                                    Segments.RecordSeparatorSegment => ambiguousInvariant
                                        ? InformationType.RecordSeparatorNotReliablyReadableInvariant
                                        : InformationType.ControlCharacterMappingNonInvariants,
                                    Segments.FileSeparatorSegment => ambiguousInvariant
                                        ? InformationType.FileSeparatorNotReliablyReadableInvariant
                                        : InformationType.FileSeparatorNotReliablyReadableNonInvariant,
                                    Segments.UnitSeparatorSegment => ambiguousInvariant
                                        ? InformationType.UnitSeparatorNotReliablyReadableInvariant
                                        : InformationType.UnitSeparatorNotReliablyReadableNonInvariant,
                                    Segments.EndOfTransmissionSegment => ambiguousInvariant
                                        ? InformationType.EotCharacterNotReliablyReadableInvariant
                                        : InformationType.ControlCharacterMappingNonInvariants,
                                },
                                reportedControl.ToControlPictures(),
                                $"{expectedControl.ToControlPictures()} {value.ToControlPicture()}");

                            token = LogAnyIsoIec54345SyntaxIssue(ambiguousInvariant);
                        }

                        if (!_tokenExtendedDataDeadKeysMap.ContainsKey(reportedControl))
                        {
                            _tokenExtendedDataDeadKeysMap.Add(reportedControl, expectedControl);
                            token = LogIsoIec15434SeparatorSupport(token, idx);
                            token = LogNonCorrespondenceForSeparators(token, idx);
                        }
                    else
                        {
    #if NET7_0_OR_GREATER
                            var ambiguousInvariant = InvariantsMatchRegex().IsMatch(_tokenExtendedDataDeadKeysMap[reportedControl]);
    #else
                            var ambiguousInvariant = InvariantsMatchRegex.IsMatch(_tokenExtendedDataDeadKeysMap[reportedControl]);
    #endif
                            token = LogCalibrationInformation(
                                token,
                                idx switch {
                                    Segments.GroupSeparatorSegment => ambiguousInvariant
                                        ? InformationType.GroupSeparatorNotReliablyReadableInvariant
                                        : ReplaceDeadKeyMap(),
                                    Segments.RecordSeparatorSegment => ambiguousInvariant
                                        ? InformationType.RecordSeparatorNotReliablyReadableInvariant
                                        : ReplaceDeadKeyMap(),
                                    Segments.FileSeparatorSegment => ambiguousInvariant
                                        ? InformationType.FileSeparatorNotReliablyReadableInvariant
                                        : InformationType.FileSeparatorNotReliablyReadableNonInvariant,
                                    Segments.UnitSeparatorSegment => ambiguousInvariant
                                        ? InformationType.UnitSeparatorNotReliablyReadableInvariant
                                        : InformationType.UnitSeparatorNotReliablyReadableNonInvariant,
                                    Segments.EndOfTransmissionSegment => ambiguousInvariant
                                        ? InformationType.EotCharacterNotReliablyReadableInvariant
                                        : ReplaceDeadKeyMap(),
                                },
                                reportedControl.ToControlPictures(),
                                $"{expectedControl.ToControlPictures()} {_tokenExtendedDataDeadKeysMap[reportedControl].ToControlPictures()}");

                            token = LogAnyIsoIec54345SyntaxIssue(ambiguousInvariant);

                            InformationType ReplaceDeadKeyMap() {
                                _tokenExtendedDataDeadKeysMap.Remove(reportedControl);
                                _tokenExtendedDataDeadKeysMap.Add(reportedControl, expectedControl);
                                return InformationType.ControlCharacterMappingNonInvariants;
                            }
    #pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                        }
                    }
                    else
                    {
                        // The control character is represented as a null character. Is this ambiguous?
                        if (_tokenExtendedDataCharacterMap.TryGetValue('\0', out var nullMapCharacter)) {
    #if NET7_0_OR_GREATER
                            var ambiguousInvariant = InvariantsMatchRegex().IsMatch(nullMapCharacter.ToInvariantString());
    #else
                            var ambiguousInvariant = InvariantsMatchRegex.IsMatch(nullMapCharacter.ToInvariantString());
    #endif

                            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                            switch (idx) {
                                case Segments.GroupSeparatorSegment:
                                    // If the existing character represented by a reported null is not an invariant, then
                                    // replace the mapping with a mapping to the GS.
                                    if (!ambiguousInvariant) {
                                        if (_tokenExtendedDataAimFlagCharacterSequence?[0] == '\0') {
                                            token = LogCalibrationInformation(
                                                token,
                                                InformationType.AimNotReadReliably);
                                        }

                                        _tokenExtendedDataCharacterMap.Remove('\0');
                                        _tokenExtendedDataCharacterMap.Add('\0', '\u001D');
                                    }

                                    token = AssessFormatSupport

                                        // Error: The reported character sequence {0} is ambiguous.This represents the group separator character.
                                        ? LogCalibrationInformation(
                                            token,
                                            ambiguousInvariant
                                                ? InformationType.GroupSeparatorNotReliablyReadableInvariant
                                                : InformationType.ControlCharacterMappingNonInvariants,
                                            '\0'.ToControlPictureString(),
                                            $"{'\u001D'.ToControlPictureString()} {nullMapCharacter.ToControlPictureString()}")
                                        : token;
                                    break;
                                case Segments.RecordSeparatorSegment:
                                    // If the existing character represented by a reported null is not an invariant and is not the GS,
                                    // then replace the mapping with a mapping to the RS.
                                    if (!ambiguousInvariant && nullMapCharacter is not '\u001D') {
                                        if (_tokenExtendedDataAimFlagCharacterSequence?[0] == '\0') {
                                            token = LogCalibrationInformation(
                                                token,
                                                InformationType.AimNotReadReliably);
                                        }

                                        _tokenExtendedDataCharacterMap.Remove('\0');
                                        _tokenExtendedDataCharacterMap.Add('\0', '\u001E');
                                    }

                                    // WARNING - The reported character sequence {0} is ambiguous. This represents the record separator character.
                                    token = AssessFormatSupport
                                            ? LogCalibrationInformation(
                                                token,
                                                ambiguousInvariant
                                                    ? InformationType.RecordSeparatorNotReliablyReadableInvariant
                                                    : InformationType.ControlCharacterMappingNonInvariants,
                                                '\0'.ToControlPictureString(),
                                                $"{'\u001E'.ToControlPictureString()} {nullMapCharacter.ToControlPictureString()}")
                                            : token;

                                    _tokenExtendedDataPotentialIsoIec15434Unreadable30 = true;
                                    break;
                                case Segments.FileSeparatorSegment:
                                    // The ambiguous character always takes precedence.
                                    _tokenExtendedDataUnreadableFs = true;

                                    // Warning: The file separator character cannot be reliably read.
                                    token = LogCalibrationInformation(
                                        token,
                                        ambiguousInvariant
                                            ? InformationType.FileSeparatorNotReliablyReadableInvariant
                                            : InformationType.FileSeparatorNotReliablyReadableNonInvariant,
                                        '\0'.ToControlPictureString(),
                                        $"{'\u001C'.ToControlPictureString()}");
                                    break;
                                case Segments.UnitSeparatorSegment:
                                    // The ambiguous character always takes precedence.
                                    _tokenExtendedDataUnreadableUs = true;

                                    // Warning: The unit separator character cannot be reliably read.
                                    token = LogCalibrationInformation(
                                        token,
                                        ambiguousInvariant
                                            ? InformationType.UnitSeparatorNotReliablyReadableInvariant
                                            : InformationType.UnitSeparatorNotReliablyReadableNonInvariant,
                                        '\0'.ToControlPictureString(),
                                        $"{'\u001F'.ToControlPictureString()}");
                                    break;
                                case Segments.EndOfTransmissionSegment:
                                    // If the existing character represented by a reported null is not an invariant and is not the GS or RS, then replace the mapping with a mapping to the EOT.
                                    if (!ambiguousInvariant && nullMapCharacter is not '\u001D' && nullMapCharacter is not '\u001E') {
                                        if (_tokenExtendedDataAimFlagCharacterSequence?[0] == '\0')
                                        {
                                            token = LogCalibrationInformation(
                                                token,
                                                InformationType.AimNotReadReliably);
                                        }

                                        _tokenExtendedDataCharacterMap.Remove('\0');
                                        _tokenExtendedDataCharacterMap.Add('\0', '\u0004');
                                    }

                                    // Warning: The end-of-transmission character cannot be reliably read.
                                    token = LogCalibrationInformation(
                                        token,
                                        ambiguousInvariant
                                            ? InformationType.EotCharacterNotReliablyReadableInvariant
                                            : InformationType.ControlCharacterMappingNonInvariants,
                                        '\0'.ToControlPictureString(),
                                        $"{'\u0004'.ToControlPictureString()}");
                                    break;
                            }
                        }
                        else {
                            _tokenExtendedDataCharacterMap.Add(reportedControl.First(), expectedControl.First());
                            token = LogNonCorrespondenceForSeparators(token, idx);
                            token = LogIsoIec15434SeparatorSupport(token, idx);
                        }
                    }
                }
                else {
                    switch (reportedControl.Length) {
                        case 0:
                            token = LogNonCorrespondenceForSeparators(token, idx);

                            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                            switch (idx) {
                                case Segments.GroupSeparatorSegment:
                                    // Error - No group separator is reported.
                                    return LogCalibrationInformation(
                                        token,
                                        InformationType.NoGroupSeparatorMapping);
                                case Segments.RecordSeparatorSegment:
                                    token = LogCalibrationInformation(
                                                token,
                                                InformationType.RecordSeparatorNotReadable);
                                    break;
                                case Segments.FileSeparatorSegment:
                                    _tokenExtendedDataUnreadableFs = true;

                                    // Warning: The file separator character cannot be reliably read.
                                    token = LogCalibrationInformation(
                                        token,
                                        InformationType.FileSeparatorNotReliablyReadable,
                                        '\0'.ToControlPictureString(),
                                        $"{'\u001C'.ToControlPictureString()}");
                                    break;
                                case Segments.UnitSeparatorSegment:
                                    _tokenExtendedDataUnreadableUs = true;

                                    // Warning: The unit separator character cannot be reliably read.
                                    token = LogCalibrationInformation(
                                        token,
                                        InformationType.UnitSeparatorNotReliablyReadable,
                                        '\0'.ToControlPictureString(),
                                        $"{'\u001F'.ToControlPictureString()}");
                                    break;
                                case Segments.EndOfTransmissionSegment:
                                    _tokenExtendedDataEotUnreadable = true;

                                    // Warning: The end-of-transmission character cannot be reliably read.
                                    token = LogCalibrationInformation(
                                        token,
                                        InformationType.EotNotReliablyReadable,
                                        '\0'.ToControlPictureString(),
                                        $"{'\u0004'.ToControlPictureString()}");
                                    break;
                            }

                            break;
                        case 1: {
                                var key = reportedControl.First();
                                correspondence = key == expectedControl.First();

                                if (_tokenExtendedDataCharacterMap.TryGetValue(key, out var characterMapValue))
                                {
    #if NET7_0_OR_GREATER
                                    var ambiguousInvariant = InvariantsMatchRegex().IsMatch(characterMapValue.ToInvariantString());
    #else
                                    var ambiguousInvariant = InvariantsMatchRegex.IsMatch(characterMapValue.ToInvariantString());
    #endif
                                    if (ambiguousInvariant)
                                    {
                                        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                                        switch (idx) {
                                            case Segments.GroupSeparatorSegment:
                                                // Error: The reported character sequence {0} is ambiguous. This represents the group separator character.
                                                return LogCalibrationInformation(
                                                    token,
                                                    InformationType.GroupSeparatorNotReliablyReadableInvariant,
                                                    key.ToControlPictureString(),
                                                    $"{expectedControl.ToControlPictures()} {characterMapValue.ToControlPictureString()}");
                                            case Segments.RecordSeparatorSegment:
                                                // Warning: The reported character sequence {0} is ambiguous.This represents the record separator character.
                                                token = LogCalibrationInformation(
                                                    token,
                                                    InformationType.RecordSeparatorNotReliablyReadableInvariant,
                                                    key.ToControlPictureString(),
                                                    $"{expectedControl.ToControlPictures()} {characterMapValue.ToControlPictureString()}");
                                                break;
                                            case Segments.FileSeparatorSegment:
                                                token = IsEdiCharacter(characterMapValue)

                                                    // Warning: The reported character {0} for a File Separator is ambiguous. Barcodes that use ISO/IEC 15434 syntax to represent
                                                    // EDI data cannot be reliably read.
                                                    ? LogCalibrationInformation(
                                                        token,
                                                        InformationType.FileSeparatorMappingIsoIec15434EdiNotReliablyReadable,
                                                        key.ToControlPictureString(),
                                                        $"{expectedControl.ToControlPictures()} {characterMapValue.ToControlPictureString()}")

                                                    // Warning: The reported character {0} for a File Separator is ambiguous. Barcodes that use ASCII 28 control
                                                    // characters may not be reliably read.
                                                    : LogCalibrationInformation(
                                                        token,
                                                        InformationType.FileSeparatorNotReliablyReadableInvariant,
                                                        key.ToControlPictureString(),
                                                        $"{expectedControl.ToControlPictures()} {characterMapValue.ToControlPictureString()}");
                                                break;
                                            case Segments.UnitSeparatorSegment:
                                                token = IsEdiCharacter(characterMapValue)

                                                    // Warning: The reported character {0} for a Unit Separator is ambiguous. Barcodes that use ISO/IEC 15434 syntax to represent
                                                    // EDI data cannot be reliably read.
                                                    ? LogCalibrationInformation(
                                                        token,
                                                        InformationType.UnitSeparatorMappingIsoIec15434EdiNotReliablyReadable,
                                                        key.ToControlPictureString(),
                                                        $"{expectedControl.ToControlPictures()} {characterMapValue.ToControlPictureString()}")

                                                    // Warning: The reported character {0} for a Unit Separator is ambiguous. Barcodes that use ASCII 31 control
                                                    // characters may not be reliably read.
                                                    : LogCalibrationInformation(
                                                        token,
                                                        InformationType.UnitSeparatorNotReliablyReadableInvariant,
                                                        key.ToControlPictureString(),
                                                        $"{expectedControl.ToControlPictures()} {characterMapValue.ToControlPictureString()}");
                                                break;
                                            case Segments.EndOfTransmissionSegment:
                                                // Warning: The reported character sequence {0} is ambiguous. The end-of-transmission character cannot be reliably read.
                                                token = LogCalibrationInformation(
                                                    token,
                                                    InformationType.EotCharacterNotReliablyReadableInvariant,
                                                    key.ToControlPictureString(),
                                                    $"{expectedControl.ToControlPictures()} {characterMapValue.ToControlPictureString()}");
                                                break;
                                        }
                                    }
                                    else {
                                        /* The ambiguity is for a non-invariant character. We will resolve it by omitting the
                                         * opportunity to resolve the non-invariant character. Replace the mapping in the
                                         * character map with one for the control character.
                                         * */
                                        var localToken = token;

                                        _tokenExtendedDataCharacterMap[key] = idx switch {
                                            Segments.GroupSeparatorSegment => _tokenExtendedDataCharacterMap[key] != 30
                                                                           && _tokenExtendedDataCharacterMap[key] != 04
                                                ? RaiseWarningAscii29((char)29)
                                                : ResolveForGs1(),
                                            Segments.RecordSeparatorSegment => _tokenExtendedDataCharacterMap[key] != 29
                                                                            && _tokenExtendedDataCharacterMap[key] != 04
                                                ? RaiseWarningAscii30((char)30)
                                                : ResolveForGs1(),
                                            Segments.EndOfTransmissionSegment => _tokenExtendedDataCharacterMap[key] != 29
                                                                              && _tokenExtendedDataCharacterMap[key] != 30
                                                ? RaiseWarningAscii04((char)04)
                                                : ResolveForGs1(),
                                            _ => RaiseWarningIsoIec15434(_tokenExtendedDataCharacterMap[key])
                                        };

                                        token = localToken;

                                        Token LogAimNoRead(Token localToken)                                    {
                                            if (key == _tokenExtendedDataAimFlagCharacterSequence?[0])                                        {
                                                localToken = LogCalibrationInformation(
                                                    localToken,
                                                    InformationType.AimNotReadReliably);
                                            }

                                            return localToken;
                                        }

                                        char RaiseWarningAscii29(char controlCharacter) {
                                            // Warning: The reported character sequence {0} is ambiguous. This may prevent reading of any additional data elements included in a barcode.
                                            localToken = LogAimNoRead(
                                                            LogCalibrationInformation(
                                                                        localToken,
                                                                        InformationType.ControlCharacterMappingNonInvariants,
                                                                        key.ToControlPictureString(),
                                                                        $"{expectedControl.ToControlPictures()} {_tokenExtendedDataCharacterMap[key].ToControlPictureString()}"));
                                            return controlCharacter;
                                        }

                                        char RaiseWarningAscii30(char controlCharacter) {
                                            // Warning: The reported character sequence {0} is ambiguous. This may prevent reading of any additional data elements included in a barcode.
                                            localToken = LogAimNoRead(LogCalibrationInformation(
                                                            localToken,
                                                            InformationType.ControlCharacterMappingNonInvariants,
                                                            key.ToControlPictureString(),
                                                            $"{expectedControl.ToControlPictures()} {_tokenExtendedDataCharacterMap[key].ToControlPictureString()}"));
                                            return controlCharacter;
                                        }

                                        char RaiseWarningAscii04(char controlCharacter) {
                                            // Warning: The reported character sequence {0} is ambiguous. This may prevent reading of any additional data elements included in a barcode.
                                            localToken = LogAimNoRead(LogCalibrationInformation(
                                                localToken,
                                                InformationType.ControlCharacterMappingNonInvariants,
                                                key.ToControlPictureString(),
                                                $"{expectedControl.ToControlPictures()} {_tokenExtendedDataCharacterMap[key].ToControlPictureString()}"));

                                            return controlCharacter;
                                        }

                                        char RaiseWarningIsoIec15434(char controlCharacter) {
                                            // Warning: The reported character {0} is ambiguous. Barcodes that use ISO/IEC 15434 syntax to represent
                                            // EDI data cannot be reliably read.
                                            localToken = AssessFormatSupport
                                                ? IsEdiCharacter(characterMapValue)

                                                   // Warning: The reported character {0} is ambiguous. Barcodes that use ISO/IEC 15434 syntax to represent
                                                   // EDI data cannot be reliably read.
                                                   ? LogCalibrationInformation(
                                                       token,
                                                       InformationType.IsoIec15434EdiNotReliablyReadable,
                                                       key.ToControlPictureString(),
                                                       $"{expectedControl.ToControlPictures()} {_tokenExtendedDataCharacterMap[key].ToControlPictureString()}")

                                                   : LogCalibrationInformation(
                                                             token,
                                                             idx switch {
                                                                 // Warning: The reported character {0} for a File Separator is ambiguous. Barcodes that use ASCII 28 control characters may not be reliably read.
                                                                 Segments.FileSeparatorSegment => InformationType.FileSeparatorNotReliablyReadableNonInvariant,

                                                                 // Warning: The reported character {0} for a Unit Separator is ambiguous. Barcodes that use ASCII 31 control characters may not be reliably read.
                                                                 Segments.UnitSeparatorSegment => InformationType.UnitSeparatorNotReliablyReadableNonInvariant,
                                                                 _ => throw new IndexOutOfRangeException("Segment index is out of range.")
                                                             },
                                                             key.ToControlPictureString(),
                                                             $"{expectedControl.ToControlPictures()} {_tokenExtendedDataCharacterMap[key].ToControlPictureString()}")
                                                : localToken;

                                            return controlCharacter;
                                        }

                                        char ResolveForGs1() {
                                            /* A combination of ASCII 29, ASCII 30 and ASCII 04 map to the same character. We will resolve
                                             * in favour of ASCII 29 (used in both GS1 and ANSI MH 10.8.2 barcodes). The map entry may
                                             * already be for ASCII 29, but we will set it again to ensure the correct outcome.
                                             * */
                                            _tokenExtendedDataCharacterMap[key] = '\u001d';

                                            return _tokenExtendedDataCharacterMap[key];
                                        }
                                    }
                                }
                                else if (key != expectedControl.First()) {
                                    correspondence = false;

                                    token = AsciiChars.Contains(key, StringComparison.Ordinal)
    #if NET7_0_OR_GREATER
                                        ? InvariantsMatchRegex().IsMatch(key.ToInvariantString())
    #else
                                        ? InvariantsMatchRegex.IsMatch(characterMapValue.ToInvariantString())
    #endif
                                            ? LogAmbiguityInvariant()
                                            : LogAmbiguityNonInvariant()
                                        : AddMapping();

                                    Token LogAmbiguityInvariant() =>
                                        idx switch {
                                            // Error: The reported character sequence {0} is ambiguous. This represents the group separator character.
                                            Segments.GroupSeparatorSegment => LogCalibrationInformation(
                                                                                token,
                                                                                InformationType.GroupSeparatorNotReliablyReadableInvariant,
                                                                                key.ToControlPictureString(),
                                                                                $"{expectedControl.ToControlPictures()} {key.ToControlPictureString()}"),

                                            // Warning: The reported character {0} is ambiguous. Barcodes that use ISO/IEC 15434 syntax cannot be read reliably.
                                            Segments.RecordSeparatorSegment => LogCalibrationInformation(
                                                                                token,
                                                                                InformationType.RecordSeparatorNotReliablyReadableInvariant,
                                                                                key.ToControlPictureString(),
                                                                                $"{expectedControl.ToControlPictures()} {key.ToControlPictureString()}"),
                                            Segments.EndOfTransmissionSegment => LogCalibrationInformationForEot(token),

                                            _ when AssessFormatSupport => IsEdiCharacter(characterMapValue)

                                                   // Warning: The reported character {0} is ambiguous. Barcodes that use ISO/IEC 15434 syntax to represent
                                                   // EDI data cannot be reliably read.
                                                   ? LogCalibrationInformation(
                                                       token,
                                                       InformationType.FileSeparatorMappingIsoIec15434EdiNotReliablyReadable,
                                                       key.ToControlPictureString(),
                                                       $"{expectedControl.ToControlPictures()} {key.ToControlPictureString()}")

                                                   : LogCalibrationInformation(
                                                       token,
                                                       idx switch {
                                                           // Warning: The reported character {0} for a File Separator is ambiguous. Barcodes that use ASCII 28 control characters may not be reliably read.
                                                           Segments.FileSeparatorSegment =>  InformationType.FileSeparatorNotReliablyReadableInvariant,

                                                           // Warning: The reported character {0} for a Unit Separator is ambiguous. Barcodes that use ASCII 31 control characters may not be reliably read.
                                                           Segments.UnitSeparatorSegment => InformationType.UnitSeparatorNotReliablyReadableInvariant,
                                                          _ => throw new IndexOutOfRangeException("Segment index is out of range.")
                                                       },
                                                       key.ToControlPictureString(),
                                                       $"{expectedControl.ToControlPictures()} {key.ToControlPictureString()}"),
                                            _ => token
                                        };

                                    // For GS, RS and EoT, we will log the non-invariant ASCII character as the ambiguity, favouring control
                                    // characters used in Format 05/06 barcodes. For FS and US, we will log the control character as ambiguous.
                                    Token MapControlCharacter(Token localToken) {
                                        _tokenExtendedDataCharacterMap.Add(key, expectedControl.First());
                                        return LogIsoIec15434SeparatorSupport(LogAimNoRead(localToken), idx);
                                    }

                                    Token LogAmbiguityNonInvariant() =>
                                        idx switch {
                                            // Warning: The reported character sequence {0} is ambiguous. This may prevent reading of any additional data elements included in a barcode.
                                            Segments.GroupSeparatorSegment => MapControlCharacter(
                                                                                LogCalibrationInformation(
                                                                                    token,
                                                                                    InformationType.ControlCharacterMappingNonInvariants,
                                                                                    key.ToControlPictureString(),
                                                                                    $"{expectedControl.ToControlPictures()} {key.ToControlPictureString()}")),

                                            // Warning: The reported character sequence {0} is ambiguous. This may prevent reading of any additional data elements included in a barcode.
                                            Segments.RecordSeparatorSegment => MapControlCharacter(
                                                                                LogCalibrationInformation(
                                                                                    token,
                                                                                    InformationType.ControlCharacterMappingNonInvariants,
                                                                                    key.ToControlPictureString(),
                                                                                    $"{expectedControl.ToControlPictures()} {key.ToControlPictureString()}")),
                                            Segments.EndOfTransmissionSegment => MapControlCharacter(LogCalibrationInformationForEot(token)),
                                            _ when AssessFormatSupport => IsEdiCharacter(characterMapValue)

                                                   // Warning: The reported character {0} is ambiguous. Barcodes that use ISO/IEC 15434 syntax to represent
                                                   // EDI data cannot be reliably read.
                                                   ? LogCalibrationInformation(
                                                       token,
                                                       InformationType.FileSeparatorMappingIsoIec15434EdiNotReliablyReadable,
                                                       key.ToControlPictureString(),
                                                       $"{expectedControl.ToControlPictures()} {key.ToControlPictureString()}")

                                                   : LogCalibrationInformation(
                                                       token,
                                                       idx switch {
                                                           // Warning: The reported character {0} for a File Separator is ambiguous. Barcodes that use ASCII 28 control characters may not be reliably read.
                                                           Segments.FileSeparatorSegment => InformationType.FileSeparatorNotReliablyReadableNonInvariant,

                                                           // Warning: The reported character {0} for a Unit Separator is ambiguous. Barcodes that use ASCII 31 control characters may not be reliably read.
                                                           Segments.UnitSeparatorSegment => InformationType.UnitSeparatorNotReliablyReadableNonInvariant,
                                                           _ => throw new IndexOutOfRangeException("Segment index is out of range.")
                                                       },
                                                       key.ToControlPictureString(),
                                                       $"{expectedControl.ToControlPictures()} {key.ToControlPictureString()}"),
                                            _ => token
                                        };

                                    Token LogCalibrationInformationForEot(Token localToken) {
    #if NET7_0_OR_GREATER
                                        if (InvariantsMatchRegex().IsMatch(key.ToString())) {
    #else
                                        if (InvariantsMatchRegex.IsMatch(expectedControl)) {
    #endif
                                            // Information: The reported character for an invariant character is the same as the character reported for the End-of-Transmission character.
                                            localToken = LogCalibrationInformation(
                                                localToken,
                                                InformationType.EotCharacterNotReliablyReadableInvariant,
                                                key.ToControlPictureString(),
                                                $"{expectedControl.ToControlPictures()} {key.ToControlPictureString()}");
                                        }
                                        else {
                                            // Warning: The reported character sequence {0} is ambiguous. This may prevent reading of any additional data elements included in a barcode.
                                            localToken = LogAimNoRead(
                                                            LogCalibrationInformation(
                                                            localToken,
                                                            InformationType.ControlCharacterMappingNonInvariants,
                                                            key.ToControlPictureString(),
                                                            $"{expectedControl.ToControlPictures()} {key.ToControlPictureString()}"));
                                        }

                                        return localToken;
                                    }

                                    Token LogAimNoRead(Token localToken) {
                                        if (key == _tokenExtendedDataAimFlagCharacterSequence?[0]) {
                                            localToken = LogCalibrationInformation(
                                                localToken,
                                                InformationType.AimNotReadReliably);
                                        }

                                        return localToken;
                                    }

                                    Token AddMapping() {
                                        _tokenExtendedDataCharacterMap.Add(key, expectedControl.First());
                                        token = LogIsoIec15434SeparatorSupport(token, idx);
                                        return token;
                                    }
                                }
                                else {
                                    token = LogIsoIec15434SeparatorSupport(token, idx);
                                }

                                break;
                            }

                        default:
                            // The control character has been reported as a ligature.  It must be added to the ligature map
                            _tokenExtendedDataLigatureMap.Add(reportedControl, expectedControl.First());
                            token = LogIsoIec15434SeparatorSupport(token, idx);
                            token = LogNonCorrespondenceForSeparators(token, idx);
                            break;
                    }
                }

                token = !correspondence
                            ? LogNonCorrespondenceForSeparators(token, idx)
                            : token;

#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                Token LogAnyIsoIec54345SyntaxIssue(bool ambiguousInvariant) =>
                    idx switch
                    {
                        Segments.RecordSeparatorSegment => LogForSeparator(ambiguousInvariant, InformationType.IsoIec15434SyntaxNotRecognised),
                        Segments.FileSeparatorSegment => LogForSeparator(ambiguousInvariant, InformationType.IsoIec15434EdiNotReliablyReadable),
                        Segments.UnitSeparatorSegment => LogForSeparator(ambiguousInvariant, InformationType.IsoIec15434EdiNotReliablyReadable),
                        Segments.EndOfTransmissionSegment => LogForSeparator(ambiguousInvariant, InformationType.IsoIec15434SyntaxNotRecognised)
                    };
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

                Token LogForSeparator(bool ambiguousInvariant, InformationType informationType) => ambiguousInvariant
                    ? LogCalibrationInformation(
                        token,
                        informationType,
                        reportedControl.ToControlPictures(),
                        $"{expectedControl.ToControlPictures()} {_tokenExtendedDataDeadKeysMap[reportedControl].ToControlPictures()}")
                    : _tokenExtendedDataDeadKeysMap[reportedControl] == "["
                        ? LogCalibrationInformation(
                            token,
                            informationType,
                            reportedControl.ToControlPictures(),
                            $"{expectedControl.ToControlPictures()} {_tokenExtendedDataDeadKeysMap[reportedControl].ToControlPictures()}")
                        : token;
            }

            return token;

            Token LogNonCorrespondenceForSeparators(Token calibrationToken, Segments idx) =>
                idx switch {
                    // Information: The barcode scanner and computer keyboard layouts do not correspond when representing Group Separators.
                    Segments.GroupSeparatorSegment =>
                        LogCalibrationInformation(
                            calibrationToken,
                            InformationType.NonCorrespondingKeyboardLayoutsGroupSeparator),

                    // Information: The barcode scanner and computer keyboard layouts do not correspond when representing File separators.
                    Segments.FileSeparatorSegment => AssessFormatSupport
                        ? LogCalibrationInformation(
                            calibrationToken,
                            InformationType.NonCorrespondingKeyboardLayoutsFileSeparator)
                        : calibrationToken,

                    // Information: The barcode scanner and computer keyboard layouts do not correspond when representing Record Separators.
                    Segments.RecordSeparatorSegment => AssessFormatSupport
                        ? LogCalibrationInformation(
                            calibrationToken,
                            InformationType.NonCorrespondingKeyboardLayoutsRecordSeparator)
                        : calibrationToken,

                    // Information: The barcode scanner and computer keyboard layouts do not correspond when representing Unit separators.
                    Segments.UnitSeparatorSegment => AssessFormatSupport
                        ? LogCalibrationInformation(
                            calibrationToken,
                            InformationType.NonCorrespondingKeyboardLayoutsUnitSeparator)
                        : calibrationToken,

                    // Information: The barcode scanner and computer keyboard layouts do not correspond when representing End-of-Transmission characters.
                    Segments.EndOfTransmissionSegment => AssessFormatSupport
                        ? LogCalibrationInformation(
                            calibrationToken,
                            InformationType.NonCorrespondingKeyboardLayoutsEotCharacter)
                        : calibrationToken,

                    _ => calibrationToken
                };

            Token LogIsoIec15434SeparatorSupport(Token calibrationToken, Segments idx) =>
                idx switch {
                    // Information: Group Separator characters are supported.
                    Segments.GroupSeparatorSegment => LogCalibrationInformation(
                        calibrationToken,
                        InformationType.GroupSeparatorSupported),

                    // Information: File separator characters are supported.
                    Segments.FileSeparatorSegment => LogCalibrationInformation(
                        calibrationToken,
                        InformationType.FileSeparatorSupported),

                    // Information: Record Separator characters are supported.
                    Segments.RecordSeparatorSegment => LogCalibrationInformation(
                        calibrationToken,
                        InformationType.RecordSeparatorSupported),

                    // Information: Unit separator characters are supported.
                    Segments.UnitSeparatorSegment => LogCalibrationInformation(
                        calibrationToken,
                        InformationType.UnitSeparatorSupported),

                    // Information: End-of-Transmission characters are supported.
                    Segments.EndOfTransmissionSegment => LogCalibrationInformation(
                        calibrationToken,
                        InformationType.EndOfTransmissionSupported),

                    _ => calibrationToken
                };

            bool IsEdiCharacter(char character) =>
                ((int)character) switch {
                    > 64 and <= 90 => true,
                    > 47 and <= 57 => true,
                    32 => true,
                    > 38 and <= 41 => true,
                    > 42 and <= 47 => true,
                    58 => true,
                    61 => true,
                    63 => true,
                    _ => false
                };
    }

    /// <summary>
    ///   Process the input for any incompatibility with the ISO/IEC 15434 message header.
    /// </summary>
    /// <param name="token">The current calibration token.</param>
    /// <returns>The calibration token.</returns>
    private Token DoProcessForIsoIec15434MessageHeaderIncompatibility(Token token) =>

        // Check for Format 05 and 06 incompatibility
        _tokenExtendedDataCharacterMap.All(kvp => kvp.Value != '[') &&
        _tokenExtendedDataDeadKeysMap.All(kvp => kvp.Value != "[") &&
        (
            _tokenExtendedDataCharacterMap.Any(kvp => kvp.Key == '[') ||
            !(_tokenExtendedDataReportedCharacters?.Contains('[', StringComparison.Ordinal) ?? false))

            // Warning - Barcodes that use ISO/IEC 15434 syntax cannot be recognised.
            ? LogCalibrationInformation(token, InformationType.IsoIec15434SyntaxNotRecognised)
            : token;

    /// <summary>
    ///   Process the input for any incompatibility with the ISO/IEC 15434 EDI barcodes.
    /// </summary>
    /// <param name="token">The current calibration token.</param>
    /// <returns>The calibration token.</returns>
    private Token DoProcessForIsoIec15434EdiIncompatibility(Token token) {
        switch (_tokenExtendedDataUnreadableFs) {
            case false when
                !_tokenExtendedDataUnreadableUs:
                return token;
            case true when
                _tokenExtendedDataUnreadableUs:
                // Warning - Barcodes that use ISO/IEC 15434 syntax to represent EDI data cannot be reliably read.
                return AssessFormatSupport
                    ? LogCalibrationInformation(
                        InitializeTokenData(),
                        InformationType.IsoIec15434EdiNotReliablyReadable)
                    : token;
        }

        // One of the ASCII 28 and ASCII 31 characters is reported as null.  We will only treat this as
        // unreliable read for EDI data is there is already a mapping for ASCII NULL.
        if (_tokenExtendedDataCharacterMap.ContainsKey('\0')) {
            // Warning - Barcodes that use ISO/IEC 15434 syntax to represent EDI data cannot be reliably read.
            return AssessFormatSupport
            ? LogCalibrationInformation(
                InitializeTokenData(),
                InformationType.IsoIec15434EdiNotReliablyReadable)
            : token;
        }

        // Add mapping for ASCII 28 or ASCII 31
        _tokenExtendedDataCharacterMap.Add('\0', _tokenExtendedDataUnreadableFs ? '\u001c' : '\u001f');

        return token;
    }

    private Token DoProcessForIsoIec15434FsIncompatibility(Token token) {
        if (_tokenExtendedDataUnreadableFs) {
            // Warning - The file separator character cannot be reliably read.
            return AssessFormatSupport
                ? LogCalibrationInformation(
                    InitializeTokenData(),
                    InformationType.FileSeparatorNotReliablyReadable)
                : token;
        }

        return token;
    }

    private Token DoProcessForIsoIec15434UsIncompatibility(Token token) {
        if (_tokenExtendedDataUnreadableUs) {
            // Warning - The unit separator character cannot be reliably read.
            return AssessFormatSupport
                ? LogCalibrationInformation(
                    InitializeTokenData(),
                    InformationType.UnitSeparatorNotReliablyReadable)
                : token;
        }

        return token;
    }

    private Token DoProcessForIsoIec15434EotIncompatibility(Token token) {
        if (_tokenExtendedDataEotUnreadable) {
            // Warning - The end-of-transmission character cannot be reliably read.
            return AssessFormatSupport
                ? LogCalibrationInformation(
                    InitializeTokenData(),
                    InformationType.EotNotReliablyReadable)
                : token;
        }

        return token;
    }

    /// <summary>
    ///   Process the input for any ambiguities which have been missed by previous processing.
    /// </summary>
    /// <param name="token">The current calibration token.</param>
    /// <returns>The calibration token.</returns>
    private Token DoProcessMissedAmbiguities(Token token) {
        // Check for missed ambiguities
        var duplicateReportedCharacters =
            (_tokenExtendedDataReportedCharacters ?? string.Empty).GroupBy(c => c)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

        if (duplicateReportedCharacters.Count <= 0) {
            return token;
        }

        foreach (var duplicateReportedCharacter in duplicateReportedCharacters) {
            var duplicateReportedCharacterAsString = duplicateReportedCharacter.ToInvariantString();

            // ReSharper disable once StringLiteralTypo
            const string expectedInvariantCharacters = @"!""%&'()*+,-./0123456789:;<=>?ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz]";
            const string expectedAdditionalAsciiCharacters = @"#$@[\]^`{|}~";

            var expectedInvariants =
                ExpectedCharacters(_invariantMappedCharacters ?? new StringBuilder(), expectedInvariantCharacters);
            var expectedAdditionalAscii =
                ExpectedCharacters(_nonInvariantMappedCharacters ?? new StringBuilder(), expectedAdditionalAsciiCharacters);
            token = expectedInvariants.Count > 0
                ? expectedInvariants.Count > 1
                    ? duplicateReportedCharacterAsString == _tokenExtendedDataAimFlagCharacterSequence

                        // Warning - AIM Identifiers cannot be recognised.  The reported character {0} is ambiguous. There are
                        // multiple keys for this character which represents the AIM flag character as well as other expected
                        // character(s).
                        ? LogCalibrationInformation(
                            token,
                            InformationType.MultipleKeysAimFlagCharacter,
                            duplicateReportedCharacterAsString[0].ToControlPictureString(),
                            AmbiguousMappedChars())

                        // Error - The reported character {0} is ambiguous. There are multiple keys for the same character, each
                        // representing a different expected character.
                        : LogCalibrationInformation(
                            token,
                            InformationType.MultipleKeys,
                            duplicateReportedCharacterAsString[0].ToControlPictureString(),
                            AmbiguousMappedChars())

                    // Warning - The reported character {0} is ambiguous. There are multiple keys for the same character, each
                    // representing a different expected character. However, at most, only one of the expected characters is invariant.
                    : LogCalibrationInformation(
                        token,
                        InformationType.MultipleKeysNonInvariantCharacters,
                        duplicateReportedCharacterAsString[0].ToControlPictureString(),
                        AmbiguousMappedChars())

                // Warning - Some reported characters are ambiguous. There are multiple keys for the same character, each representing
                // a different non-invariant expected character: {0}
                : LogCalibrationInformation(
                    token,
                    InformationType.MultipleKeysMultipleNonInvariantCharacters,
                    duplicateReportedCharacter.ToInvariantString(),
                    AmbiguousMappedChars());
            var aimFlag =
                _tokenExtendedDataAimFlagCharacterSequence?.Length == 2
                    ? _tokenExtendedDataAimFlagCharacterSequence[1]
                    : TestAimFlagLengthIs1();

            token = duplicateReportedCharacter == aimFlag

                // Warning - AIM Identifiers cannot be recognised.  The reported character {0} is ambiguous. There are
                // multiple keys for this character which represents the AIM flag character as well as other expected
                // character(s).
                ? LogCalibrationInformation(
                    token,
                    InformationType.MultipleKeysAimFlagCharacter,
                    duplicateReportedCharacter.ToControlPictureString(),
                    AmbiguousMappedChars())
                : token;
            continue;

            // Get a space-separated string of ambiguous expected characters to which a single ambiguous reported character maps.
            string AmbiguousMappedChars() =>
                string.Join(" ", expectedInvariants) +
                (expectedAdditionalAscii.Count > 0 ? " " + string.Join(" ", expectedAdditionalAscii) : string.Empty);

            char TestAimFlagLengthIs1() =>
                _tokenExtendedDataAimFlagCharacterSequence?.Length == 1
                    ? _tokenExtendedDataAimFlagCharacterSequence[0]
                    : (char)0;

            List<char> ExpectedCharacters(StringBuilder mappedCharacters, string allExpectedCharacters) {
                var characters = new Span<char>(mappedCharacters.ToString().ToCharArray());
                var indexes = new List<int>();

                for (var idx = 0; idx < characters.Length; idx++) {
                    if (characters[idx] == duplicateReportedCharacter) indexes.Add(idx);
                }

                return indexes.Select(t => allExpectedCharacters[t])
                    .Select(dummy => dummy.ToControlPicture()).ToList();
            }
        }

        return token;
    }

    /// <summary>
    /// Resets state for baseline calibration.
    /// </summary>
    private void ResetStateForBaselineCalibration() {
        _tokenExtendedDataDeadKeysMap.Clear();
        _tokenExtendedDataScannerDeadKeysMap.Clear();
        _tokenExtendedDataScannerUnassignedKeys.Clear();
        _tokenExtendedDataCharacterMap.Clear();
        _tokenExtendedDataLigatureMap.Clear();
        _tokenExtendedDataUnrecognisedKeys.Clear();
        _tokenExtendedDataAimFlagCharacterSequence = "]";
        _tokenExtendedDataPrefix = default;
        _tokenExtendedDataCode = default;
        _tokenExtendedDataSuffix = default;
        _tokenExtendedDataReportedPrefix = default;
        _tokenExtendedDataReportedCode = default;
        _tokenExtendedDataReportedSuffix = default;
        _tokenExtendedDataKeyboardScript = default;
        _tokenExtendedDataPotentialIsoIec15434Unreadable30 = false;
        _tokenExtendedDataUnreadableFs = false;
        _tokenExtendedDataUnreadableUs = false;
        _tokenExtendedDataEotUnreadable = false;
        _tokenCalibrationData = null;
        _tokenSystemCapabilities = null;
        _invariantMappedCharacters = null;
        _nonInvariantMappedCharacters = null;
        _processedInvariantCharacters = null;
        _processedNonInvariantCharacters?.Clear();
    }

    /// <summary>
    /// Tests if we are currently processing a small barcode sequence, but not yet at the last barcode.
    /// </summary>
    /// <param name="data">The reported data currently being processed.</param>
    /// <param name="token">The current token.</param>
    /// <returns>True, if currently processing a small barcode sequence, but not yet at the last barcode; otherwise false.</returns>
    private bool TryInSmallBarcodeSequence(ref string data, ref Token token) {
        if (!(token.Data?.SmallBarcodeSequenceCount > 0)) {
            return false;
        }

        _tokenDataPrefix ??= string.Empty;

        // Strip off any trailing Carriage Return or Line Feed characters. These are assumed
        // to have been added by the barcode scanner.
        data = token.Data.SmallBarcodeSequenceIndex < token.Data.SmallBarcodeSequenceCount
            ? data.StripTrailingCrLfs()
            : data;

        // Strip off any identical prefixes on second and subsequent barcodes. We are forced to
        // assume that there are no spaces in the prefix.
        data = token.Data.SmallBarcodeSequenceIndex > 1 && _tokenDataPrefix is not null
            ? SmallBarcodeSequencePrefix(data)
            : data;

        data = _tokenDataReportedCharacters += data;

        if (token.Data.SmallBarcodeSequenceIndex > 0 &&
            token.Data.SmallBarcodeSequenceIndex < token.Data.SmallBarcodeSequenceCount) {
            return true;
        }

        token = new Token(
            _tokenDataBarcodeData,
            string.Empty,
            size: _tokenSize,
            prefix: _tokenDataPrefix,
            suffix: _tokenDataSuffix);

        return false;

        // Get the small barcode sequence prefix.
        string SmallBarcodeSequencePrefix(string localData) =>
            _tokenDataPrefix.Length == 0
                ? localData[CalculateSmallBarcodeSequencePrefix(localData)..]
                : localData[_tokenDataPrefix.Length..];

        // Calculate the small barcode sequence prefix.
        int CalculateSmallBarcodeSequencePrefix(string localData) {
            var smallBarcodeSequencePrefix = 0;

            for (var idx = 0; idx < localData.Length; idx++) {
                if (localData[idx] == ' ') {
                    break;
                }

                if (_tokenDataReportedCharacters?[idx] == localData[idx]) {
                    smallBarcodeSequencePrefix++;
                }
                else {
                    break;
                }
            }

            _tokenDataPrefix = localData[..smallBarcodeSequencePrefix];
            return smallBarcodeSequencePrefix;
        }
    }
}