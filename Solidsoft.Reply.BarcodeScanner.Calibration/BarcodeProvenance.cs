// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalibrationBarcodeProvenance.cs" company="Solidsoft Reply Ltd.">
//   (c) 2020 Solidsoft Reply Ltd.
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
// The likely provenance of the calibration barcode. This is determined through statistical analysis of its data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

/// <summary>
///   The likely provenance of the calibration barcode. This is determined through statistical analysis of its data.
/// </summary>
internal enum BarcodeProvenance
{
    /// <summary>
    ///   The barcode appears to be a fully-reported baseline barcode.
    /// </summary>
    Baseline,

    /// <summary>
    ///   The barcode appears to be a fully-reported dad key barcode.
    /// </summary>
    DeadKey,

    /// <summary>
    ///   The barcode appears to be a partially-reported baseline barcode.
    /// </summary>
    PartialBaseline,

    /// <summary>
    ///   The barcode appears to be a partially-reported dead key barcode.
    /// </summary>
    PartialDeadkey,

    /// <summary>
    ///   The barcode appears not to be a calibration barcode.
    /// </summary>
    Unknown,

    /// <summary>
    ///   No barcode data was reported.
    /// </summary>
    NoData
}