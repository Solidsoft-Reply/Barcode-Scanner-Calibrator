// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BarcodeData.cs" company="Solidsoft Reply Ltd">
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
// Represents barcode data for calibration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
///   Represents barcode data for calibration.
/// </summary>
/// <remarks>
///   Initializes a new instance of the <see cref="BarcodeData" /> class.
/// </remarks>
/// <param name="data">The calibration data.</param>
/// <param name="maximumCharacters">The maximum number of characters in each barcode.</param>
public class BarcodeData(string data, int maximumCharacters = -1) {
    /// <summary>
    ///   The calibration data.
    /// </summary>
    private readonly string _data = data;

    /// <summary>
    ///   The maximum number of characters in each barcode.
    /// </summary>
    private readonly int _maximumCharacters = maximumCharacters;

    /// <summary>
    ///   A collection of strings representing individual characters in the barcode.
    /// </summary>
    private readonly string[] _parsedData = ParseData(data).ToArray();

    /// <summary>
    ///   A collection of calibration data segments for individual barcodes.
    /// </summary>
    private IEnumerable<string>? _segments;

    /// <summary>
    ///   Gets a collection of calibration data segments for individual barcodes.
    /// </summary>
    public IEnumerable<string> Segments {
        get {
            if (_segments != null) {
                return _segments;
            }

            _segments = ProcessSegments();
            return _segments;
        }
    }

    /// <summary>
    ///   Gets the count of barcode data segments.
    /// </summary>
    public int Count => _parsedData.Length == 0 ? 0 : Segments.Count();

    /// <summary>
    ///   Parses data to a collection of strings representing characters in the barcode.
    /// </summary>
    /// <param name="data">The data to be parsed.</param>
    /// <returns>A collection of strings representing characters in the barcode.</returns>
    private static IEnumerable<string> ParseData(string data) {
        return string.IsNullOrWhiteSpace(data)
            ? []
            : data.Select(char.ToString).ToList();
    }

    /// <summary>
    ///   Yields a collection of calibration data segments for individual barcodes.
    /// </summary>
    /// <returns>A data segment.</returns>
    private IEnumerable<string> ProcessSegments() {
        if (_parsedData.Length == 0) {
            yield break;
        }

        switch (_maximumCharacters) {
            case > 0: {
                    var totalLength = _parsedData.Length;
                    var boundaryCharacter = _parsedData[0];
                    var remainingLength = _parsedData.Length;
                    var currentSegmentStartIndex = 0;
                    var workingMaxCharacter = _maximumCharacters;
                    var adjustingLastSegments = false;

                    while (true) {
                        if (remainingLength >= workingMaxCharacter * 2) {
                            yield return DoIt();
                        }
#pragma warning disable S2589
                        else if (remainingLength <= 0) {
#pragma warning restore S2589
                            break;
                        }
                        else if (!adjustingLastSegments && remainingLength < workingMaxCharacter * 2) {
                            workingMaxCharacter = (int)Math.Ceiling((double)remainingLength / 2);
#pragma warning disable IDE0059

                            // ReSharper disable once RedundantAssignment
                            adjustingLastSegments = true;
#pragma warning restore IDE0059
                            yield return DoIt();
                        }
                        else {
                            yield return DoIt();
                        }

#pragma warning disable S1751
                        continue;
#pragma warning restore S1751

                        string DoIt() {
                            var terminateIndex = currentSegmentStartIndex + workingMaxCharacter;
                            terminateIndex = terminateIndex > totalLength
                                ? totalLength
                                : terminateIndex;
                            var candidateSegmentData =
                                _parsedData[
                                    currentSegmentStartIndex..terminateIndex];
                            var candidateSegmentDataLength = candidateSegmentData.Length;
                            var segmentDataBuilder = new StringBuilder();

                            while (terminateIndex < totalLength && candidateSegmentData[^1] == boundaryCharacter) {
                                candidateSegmentData = candidateSegmentData[..--candidateSegmentDataLength];
                            }

                            remainingLength -= candidateSegmentDataLength;

                            currentSegmentStartIndex += candidateSegmentDataLength;

                            foreach (var stringChar in candidateSegmentData) {
                                segmentDataBuilder.Append(stringChar);
                            }

                            workingMaxCharacter = _maximumCharacters;
                            adjustingLastSegments = remainingLength <= workingMaxCharacter;

                            return segmentDataBuilder.ToString();
                        }
                    }

                    break;
                }

            case < 0:
                yield return _data;
                break;
        }
    }
}