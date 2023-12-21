// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalibrationTokenData.cs" company="Solidsoft Reply Ltd.">
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
// A set of data passed as part of a calibration token. This data is always provided with the token but is 
// primarily intended for internal use to track the enumeration of calibration barcodes.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

/// <summary>
///   A set of data passed as part of a calibration token. This data is always provided with the token but is
///   primarily intended for internal use to track the enumeration of calibration barcodes.
/// </summary>
public sealed class CalibrationTokenData : IEquatable<CalibrationTokenData>
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="CalibrationTokenData" /> class.
    /// </summary>
    internal CalibrationTokenData()
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="CalibrationTokenData" /> class.
    /// </summary>
    /// <param name="barcodeData">
    ///   The unsegmented barcode data for the current calibration data.
    /// </param>
    /// <param name="key">
    ///   The dead key currently being calibrated. Null indicates baseline calibration.
    /// </param>
    /// <param name="value">
    ///   The expected character for the current dead key being calibrated.
    /// </param>
    /// <param name="calibrationsRemaining">
    ///   A count of the estimated number of calibrations that still need to be performed during this session.
    /// </param>
    /// <param name="smallBarcodeSequenceIndex">
    ///   The index of the current small barcode in a sequence.
    /// </param>
    /// <param name="smallBarcodeSequenceCount">
    ///   The number of small barcodes that encode the current calibration data.
    /// </param>
    /// <param name="prefix">
    ///   The prefix for each small barcode in a sequence.
    /// </param>
    /// <param name="suffix">
    ///   The detected suffix.
    /// </param>
    /// <param name="reportedCharacters">
    ///   The reported characters for the current calibration barcode.
    /// </param>
    internal CalibrationTokenData(
        string barcodeData,
        string? key = null,
        char value = default,
        int calibrationsRemaining = -1,
        int smallBarcodeSequenceIndex = -1,
        int smallBarcodeSequenceCount = -1,
        string prefix = "",
        string suffix = "",
        string reportedCharacters = "")
    {
        BarcodeData = barcodeData;
        Key = key;
        Value = value;
        CalibrationsRemaining = calibrationsRemaining;
        SmallBarcodeSequenceIndex = smallBarcodeSequenceIndex;
        SmallBarcodeSequenceCount = smallBarcodeSequenceCount;
        Prefix = prefix;
        Suffix = suffix;
        ReportedCharacters = reportedCharacters;
    }

    /// <summary>
    ///   Gets a new key and value for an additional barcode.
    /// </summary>
    [JsonProperty("additionalBarcode", Order = 0)]
    public KeyValuePair<string, char> AdditionalBarcode { get; private set; }

    /// <summary>
    ///   Gets the unsegmented barcode data for the current calibration data.
    /// </summary>
    [JsonProperty("barcodeData", Order = 1)]
    public string? BarcodeData { get; private set; }

    /// <summary>
    ///   Gets a count of the known number of calibrations to be performed this session.
    /// </summary>
    [JsonProperty("calibrationRemaining", Order = 2)]
    public int CalibrationsRemaining { get; private set; }

    /// <summary>
    ///   Gets the dead key currently being calibrated. Null indicates baseline calibration.
    /// </summary>
    [JsonProperty("key", Order = 3)]
    public string? Key { get; private set; }

    /// <summary>
    ///   Gets the reported characters for the current calibration barcode. If the calibration barcode has been
    ///   segmented into multiple small barcodes, this property contains the content of the currently processed
    ///   small barcodes.
    /// </summary>
    [JsonProperty("reportedCharacters", Order = 4)]
    public string? ReportedCharacters { get; private set; }

    /// <summary>
    ///   Gets a value indicating whether it is necessary to reset the 'bar codes remaining' count when yielding bar codes.
    /// </summary>
    [JsonProperty("resetBarcodesRemainingCount", Order = 5)]
    public bool ResetBarcodesRemainingCount { get; private set; }

    /// <summary>
    ///   Gets the index number of the current small barcode barcode.
    /// </summary>
    /// <remarks>
    ///   Small barcodes are used when the calibrator limits the maximum number of characters in any one
    ///   barcode to less than the number of characters for inclusion in a calibration barcode. The
    ///   barcode is segmented into multiple barcodes. If a sequence of small barcodes is not used, this
    ///   property is set to -1.
    /// </remarks>
    [JsonProperty("smallBarcodeSequenceIndex", Order = 6)]
    public int SmallBarcodeSequenceIndex { get; private set; }

    /// <summary>
    ///   Gets the number of small barcodes for the current calibration.
    /// </summary>
    /// <remarks>
    ///   A sequence of small barcodes is used where the selected barcode size is too small
    ///   to carry the calibration data.
    /// </remarks>
    [JsonProperty("smallBarcodeSequenceCount", Order = 7)]
    public int SmallBarcodeSequenceCount { get; private set; }

    /// <summary>
    ///   Gets the prefix, if any, transmitted by the barcode scanner.
    /// </summary>
    [JsonProperty("prefix", Order = 8)]
    public string? Prefix { get; private set; }

    /// <summary>
    ///   Gets the suffix, if any, transmitted by the barcode scanner.
    /// </summary>
    [JsonProperty("suffix", Order = 9)]
    public string? Suffix { get; private set; }
    
    /// <summary>
    ///   Gets the expected character for the current dead key being calibrated.
    /// </summary>
    [JsonProperty("value", Order = 10)]
    public char Value { get; private set; }

    /// <summary>
    ///   Gets the latest serialization or deserialization error.
    /// </summary>
    [JsonIgnore]

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public string LatestError { get; private set; } = string.Empty;

    /// <summary>
    ///   Initializes the token data from a JSON string representing the serialized data.
    /// </summary>
    /// <param name="json">A JSON string representing the serialized data.</param>

    // ReSharper disable once UnusedMember.Global
    public static CalibrationTokenData FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new CalibrationTokenData();

        var calibrationTokenData = JsonConvert.DeserializeObject<CalibrationTokenData>(json);

        if (calibrationTokenData is null) return new CalibrationTokenData();

        return new CalibrationTokenData(
            calibrationTokenData.BarcodeData ?? string.Empty,
            calibrationTokenData.Key,
            calibrationTokenData.Value,
            calibrationTokenData.CalibrationsRemaining,
            calibrationTokenData.SmallBarcodeSequenceIndex,
            calibrationTokenData.SmallBarcodeSequenceCount,
            calibrationTokenData.Prefix ?? string.Empty,
            calibrationTokenData.Suffix ?? string.Empty,
            calibrationTokenData.ReportedCharacters ?? string.Empty);
    }

    /// <summary>
    ///   Override for the equality operator.
    /// </summary>
    /// <param name="calibrationTokenData1">The first calibration token data.</param>
    /// <param name="calibrationTokenData2">The second calibration token data.</param>
    /// <returns>True, if the calibration token data are equal; otherwise false.</returns>
    public static bool operator ==(
        CalibrationTokenData? calibrationTokenData1,
        CalibrationTokenData calibrationTokenData2) =>
        calibrationTokenData1?.Equals(calibrationTokenData2) ?? false;

    /// <summary>
    ///   Override for the inequality operator.
    /// </summary>
    /// <param name="calibrationTokenData1">The first calibration token data.</param>
    /// <param name="calibrationTokenData2">The second calibration token data.</param>
    /// <returns>True, if the calibration token data are not equal; otherwise false.</returns>
    public static bool operator !=(
        CalibrationTokenData? calibrationTokenData1,
        CalibrationTokenData calibrationTokenData2) =>
        !calibrationTokenData1?.Equals(calibrationTokenData2) ?? false;

    /// <summary>
    ///   Indicates whether the current calibration token data is equal to another calibration token data object.
    /// </summary>
    /// <param name="other">A calibration token data object to compare with this current calibration token data object.</param>
    /// <returns>true if the current calibration token data object is equal to the other parameter; otherwise, false.</returns>
    public bool Equals(CalibrationTokenData? other) =>
        other is not null &&
        (ReferenceEquals(this, other) || string.Equals(
             BarcodeData,
             other.BarcodeData,
             StringComparison.Ordinal) &&
         string.Equals(Key, other.Key, StringComparison.Ordinal) && Value == other.Value &&
         CalibrationsRemaining == other.CalibrationsRemaining &&
         ResetBarcodesRemainingCount == other.ResetBarcodesRemainingCount &&
         AdditionalBarcode.Equals(other.AdditionalBarcode) &&
         SmallBarcodeSequenceIndex == other.SmallBarcodeSequenceIndex &&
         SmallBarcodeSequenceCount == other.SmallBarcodeSequenceCount &&
         string.Equals(
             Prefix,
             other.Prefix,
             StringComparison.Ordinal) &&
         string.Equals(
             Suffix,
             other.Suffix,
             StringComparison.Ordinal) &&
         string.Equals(
             ReportedCharacters,
             other.ReportedCharacters,
             StringComparison.Ordinal));

    /// <summary>
    ///   Indicates whether the current calibration token data is equal to another object.
    /// </summary>
    /// <param name="obj">An object to compare with this current calibration token data object.</param>
    /// <returns>true if the current calibration token data object is equal to the other parameter; otherwise, false.</returns>
    public override bool Equals(object? obj) =>
        obj is not null &&
        (ReferenceEquals(this, obj) || obj is CalibrationTokenData tokenData && Equals(tokenData));

    /// <summary>
    ///   Returns a hash value for the current token.
    /// </summary>
    /// <returns>The hash value.</returns>
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode() =>
        Fnv.CreateHashFnv1A(
            BarcodeData,
            Key,
            Value,
            CalibrationsRemaining,
            ResetBarcodesRemainingCount,
            AdditionalBarcode,
            SmallBarcodeSequenceIndex,
            SmallBarcodeSequenceCount,
            Prefix,
            Suffix,
            ReportedCharacters);

    /// <summary>
    ///   Returns a JSON representation of the calibration token data.
    /// </summary>
    /// <returns>A JSON representation of the calibration token data.</returns>
    public override string ToString() =>
        ToJson();

    /// <summary>
    ///   Returns a JSON representation of the calibration token data.
    /// </summary>
    /// <param name="formatting">Specifies the formatting to be applied to the JSON.</param>
    /// <returns>A JSON representation of the calibration token data.</returns>

    // ReSharper disable once MemberCanBePrivate.Global
    public string ToJson(Formatting formatting = Formatting.None) =>
        JsonConvert.SerializeObject(
            this,
            formatting,
            new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = new CalibrationDataIgnoreEmptyEnumerableResolver()
            });

    /// <summary>
    ///   Handles errors in serialization and deserialization
    /// </summary>
    /// <param name="context">The streaming context.</param>
    /// <param name="errorContext">The error context</param>
    [OnError, SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once UnusedParameter.Global
#pragma warning disable CA1801 // Review unused parameters
    internal void OnError(StreamingContext context, ErrorContext errorContext)
#pragma warning restore CA1801 // Review unused parameters
    {
        var settings = new JsonSerializerSettings
                       {
                           StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                           DefaultValueHandling = DefaultValueHandling.Ignore,
                           ContractResolver = new CalibrationDataIgnoreEmptyEnumerableResolver()
                       };

        LatestError = JsonConvert.SerializeObject(errorContext, settings);
        errorContext.Handled = true;
    }

    /// <summary>
    /// Amend the reported characters.  This may be necessary in order to remove repeated suffixes in the
    /// reported character sequence when small barcode processing is used.
    /// </summary>
    /// <param name="amendedReportedCharacters">The amended reported characters.</param>
    internal void AmendReportedCharacters(string amendedReportedCharacters) {
        this.ReportedCharacters = amendedReportedCharacters;
    }
}