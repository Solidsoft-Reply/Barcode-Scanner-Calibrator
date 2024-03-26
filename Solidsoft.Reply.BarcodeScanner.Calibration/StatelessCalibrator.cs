// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatelessCalibrator.cs" company="Solidsoft Reply Ltd.">
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
// Calibrates for a given combination of barcode scanner and OS keyboard layouts.
// Supports a stateless model suitable for client/server scenarios where no session
// state is maintained across multiple calls into the server.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

////#define Diagnostics

// ReSharper disable UnusedMember.Global
#pragma warning disable S1751
#pragma warning disable S3626

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System;
using System.Collections.Generic;
using System.IO;

using DataMatrix;
using Parsers.Common;

/// <summary>
///   Manages the calibration for a given combination of barcode scanner and OS keyboard layouts.
/// </summary>
///<remarks>
/// Supports a stateless model suitable for client/server scenarios where no session state is maintained
/// across multiple calls into the server.
/// </remarks>
public class StatelessCalibrator
{
    /// <summary>
    /// Internal instance of the <see cref="Calibrator"/> class.
    /// </summary>
    private readonly Calibrator _calibrator;

    /// <summary>
    ///   Initializes a new instance of the <see cref="StatelessCalibrator" /> class.
    /// </summary>
    /// <param name="assumption">The assumption made concerning the use of calibration in client systems.</param>
    public StatelessCalibrator(
        Assumption assumption = Assumption.Calibration)
    {
        _calibrator = new Calibrator(assumption);
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="StatelessCalibrator" /> class.
    /// </summary>
    /// <param name="calibrationsData">The calibration data.</param>
    /// <param name="assumption">The assumption made concerning the use of calibration in client systems.</param>
    // ReSharper disable once UnusedMember.Global
    public StatelessCalibrator(
        Data? calibrationsData, 
        Assumption assumption = Assumption.Calibration)
    {
        _calibrator = new Calibrator(calibrationsData, assumption);
    }

    /// <summary>
    ///   Gets the type of calibration barcode currently being processed.
    /// </summary>
    public BarcodeType CurrentBarcodeType => _calibrator.CurrentBarcodeType;

    /// <summary>
    ///   Gets or sets the Calibration configuration data.
    /// </summary>
    public Data? CalibrationData
    {
        get => _calibrator.CalibrationData;

        set => _calibrator.CalibrationData = value;
    }

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
        _calibrator.SetReportedPrefix(reportedPrefix);

    /// <summary>
    ///   Returns the system capabilities for the current calibration.
    /// </summary>
    /// <param name="capsLock">
    ///   Optional. Indicates if the keyboard Caps Lock was on or off when calibration was carried out.
    /// </param>
    /// <returns>The system capabilities for the current calibration.</returns>
    public SystemCapabilities? SystemCapabilities(bool? capsLock = null) => _calibrator.SystemCapabilities(capsLock);

    /// <summary>
    ///   Gets or sets a value indicating whether to assess Format nn support.
    /// </summary>
    public bool AssessFormatnnSupport
    {
        // ReSharper disable once UnusedMember.Global
        get => _calibrator.AssessFormatnnSupport;

        set => _calibrator.AssessFormatnnSupport = value;
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
    public IList<Stream> BaselineBarcodes(float multiplier = 1F, Size size = Size.Automatic) => _calibrator.BaselineBarcodes(multiplier, size);

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
    public IList<string> BaselineBarcodeData(Size size = Size.Automatic) =>
        _calibrator.BaselineBarcodeData(size);

    /// <summary>
    ///   Calibrates for a given combination of barcode scanner and OS keyboard layouts.
    /// </summary>
    /// <param name="data">The reported input after scanning the calibration barcode.</param>
    /// <param name="token">The current calibration token.</param>
    /// <param name="capsLock">Indicates if Caps Lock is switched on.</param>
    /// <param name="platform">The platform on which the system resides.</param>
    /// <param name="dataEntryTimeSpan">The time span specifying how long it took from the start of the scan to submitting the data.</param>
    /// <param name="preProcessors">The pre-processor functions, provided as a delegate.</param>
    /// <returns>The updated calibration token.</returns>

    // ReSharper disable once UnusedMember.Global
    public Token Calibrate(int[] data, Token token, bool? capsLock = null,
        SupportedPlatform platform = SupportedPlatform.Windows, TimeSpan dataEntryTimeSpan = default,
        Preprocessor? preProcessors = null) =>
        _calibrator.Calibrate(data, token, capsLock, platform, dataEntryTimeSpan, preProcessors);

    /// <summary>
    ///   Calibrates for a given combination of barcode scanner and OS keyboard layouts.
    /// </summary>
    /// <param name="data">The reported input after scanning the calibration barcode.</param>
    /// <param name="token">The current calibration token.</param>
    /// <param name="capsLock">Indicates if Caps Lock is switched on.</param>
    /// <param name="platform">The platform on which the system resides.</param>
    /// <param name="dataEntryTimeSpan">The time span specifying how long it took from the start of the scan to submitting the data.</param>
    /// <param name="preProcessors">The pre-processor functions, provided as a delegate.</param>
    /// <returns>The updated calibration token.</returns>
    public Token Calibrate(string? data, Token token, bool? capsLock = null,
        SupportedPlatform platform = SupportedPlatform.Windows, TimeSpan dataEntryTimeSpan = default,
        Preprocessor? preProcessors = null) =>
        _calibrator.Calibrate(data, token, capsLock, platform, dataEntryTimeSpan, preProcessors);

    /// <summary>
    ///   Return the next calibration token.
    /// </summary>
    /// <param name="token">The current calibration token.</param>
    /// <param name="multiplier">The size multiplier.</param>
    /// <param name="size">The size of data matrix required.</param>
    /// <returns>The next calibration token.</returns>

    // ReSharper disable once UnusedMember.Global
    public Token NextCalibrationToken(
        Token token = default,
        float multiplier = 1F,
        Size size = Size.Automatic) =>
            _calibrator.NextCalibrationToken(token, multiplier, size);

    /// <summary>
    ///   Return the next calibration token.
    /// </summary>
    /// <param name="generateImages">Indicates whether the library should generate barcode images.</param>
    /// <param name="token">The current calibration token.</param>
    /// <returns>The next calibration token.</returns>

    // ReSharper disable once UnusedMember.Global
    public Token NextCalibrationToken(
        bool generateImages,
        Token token = default) =>
        _calibrator.NextCalibrationToken(generateImages, token);

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
        Size size = Size.Automatic) => _calibrator.SupplementalBarcodes(multiplier, size);

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
        Size size = Size.Automatic) => _calibrator.SupplementalBarcodeData(size);

    /// <summary>
    ///   Gets a value indicating whether pre-processing of barcode scanner input is required.
    /// </summary>
    public bool IsProcessingRequired => _calibrator.IsProcessingRequired;

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
    public string ProcessInput(string? input, out IList<PreprocessorException>? exceptions) => _calibrator.ProcessInput(input, out exceptions);

    /// <summary>
    /// gets or sets a collection of recognised data elements.
    /// </summary>
    /// <remarks>
    /// Optionally pass a list of recognised data elements to the Calibrator to constrain the GS1 application identifiers
    /// and/or the ASC MH 10.8.2 data identifiers that the client software needs to recognise whe parsing data. This may
    /// /extend the range of transformation strategies that the calibrator can identify.
    /// </remarks>
    // ReSharper disable once UnusedMember.Global
    public IList<RecognisedDataElement> RecognisedDataElements
    {
        get => _calibrator.RecognisedDataElements;

        set => _calibrator.RecognisedDataElements = value;
    }
}