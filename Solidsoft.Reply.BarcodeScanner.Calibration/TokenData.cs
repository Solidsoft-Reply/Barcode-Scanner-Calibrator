// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TokenData.cs" company="Solidsoft Reply Ltd">
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
// A set of data passed as part of a calibration token. This data is always provided with the token but is
// primarily intended for internal use to track the enumeration of calibration barcodes.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

/// <summary>
///   A set of data passed as part of a calibration token. This data is always provided with the token but is
///   primarily intended for internal use to track the enumeration of calibration barcodes.
/// </summary>
public sealed record TokenData {
    /// <summary>
    ///   Initializes a new instance of the <see cref="TokenData" /> class.
    /// </summary>
    internal TokenData() {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="TokenData" /> class.
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
    internal TokenData(
        string barcodeData,
        string? key = null,
        char value = default,
        int calibrationsRemaining = -1,
        int smallBarcodeSequenceIndex = -1,
        int smallBarcodeSequenceCount = -1,
        string prefix = "",
        string suffix = "",
        string reportedCharacters = "") {
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
    public KeyValuePair<string, char> AdditionalBarcode { get; init; }

    /// <summary>
    ///   Gets the unsegmented barcode data for the current calibration data.
    /// </summary>
    [JsonProperty("barcodeData", Order = 1)]
    public string? BarcodeData { get; init; }

    /// <summary>
    ///   Gets a count of the known number of calibrations to be performed this session.
    /// </summary>
    [JsonProperty("calibrationRemaining", Order = 2)]
    public int CalibrationsRemaining { get; init; }

    /// <summary>
    ///   Gets the dead key currently being calibrated. Null indicates baseline calibration.
    /// </summary>
    [JsonProperty("key", Order = 3)]
    public string? Key { get; init; }

    /// <summary>
    ///   Gets the reported characters for the current calibration barcode. If the calibration barcode has been
    ///   segmented into multiple small barcodes, this property contains the content of the currently processed
    ///   small barcodes.
    /// </summary>
    [JsonProperty("reportedCharacters", Order = 4)]
    public string? ReportedCharacters { get; init; }

    /// <summary>
    ///   Gets a value indicating whether it is necessary to reset the 'bar codes remaining' count when yielding bar codes.
    /// </summary>
    [JsonProperty("resetBarcodesRemainingCount", Order = 5)]
    public bool ResetBarcodesRemainingCount { get; init; }

    /// <summary>
    ///   Gets the index number of the current small barcode.
    /// </summary>
    /// <remarks>
    ///   Small barcodes are used when the calibrator limits the maximum number of characters in any one
    ///   barcode to less than the number of characters for inclusion in a calibration barcode. The
    ///   barcode is segmented into multiple barcodes. If a sequence of small barcodes is not used, this
    ///   property is set to -1.
    /// </remarks>
    [JsonProperty("smallBarcodeSequenceIndex", Order = 6)]
    public int SmallBarcodeSequenceIndex { get; init; }

    /// <summary>
    ///   Gets the number of small barcodes for the current calibration.
    /// </summary>
    /// <remarks>
    ///   A sequence of small barcodes is used where the selected barcode size is too small
    ///   to carry the calibration data.
    /// </remarks>
    [JsonProperty("smallBarcodeSequenceCount", Order = 7)]
    public int SmallBarcodeSequenceCount { get; init; }

    /// <summary>
    ///   Gets the prefix, if any, transmitted by the barcode scanner.
    /// </summary>
    [JsonProperty("prefix", Order = 8)]
    public string? Prefix { get; init; }

    /// <summary>
    ///   Gets the suffix, if any, transmitted by the barcode scanner.
    /// </summary>
    [JsonProperty("suffix", Order = 9)]
    public string? Suffix { get; init; }

    /// <summary>
    ///   Gets the expected character for the current dead key being calibrated.
    /// </summary>
    [JsonProperty("value", Order = 10)]
    public char Value { get; init; }

    /// <summary>
    ///   Returns a token representing a JSON representation of the token.
    /// </summary>
    /// <param name="json">A JSON string representing the serialized token.</param>
    /// <returns>The deserialised token.</returns>
    // ReSharper disable once UnusedMember.Global
    public static TokenData FromJson(string json) {
        if (string.IsNullOrWhiteSpace(json)) return new TokenData();

        var calibrationTokenData = JsonConvert.DeserializeObject<TokenData>(json);

        if (calibrationTokenData is null) return new TokenData();

        return new TokenData(
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
            new JsonSerializerSettings {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ConstructorHandling = ConstructorHandling.Default,
                ContractResolver = new DataIgnoreEmptyEnumerableResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            });
}