// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalibrationAssumption.cs" company="Solidsoft Reply Ltd.">
//   (c) 2020 Solidsoft Reply Ltd. All rights reserved.
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
// The assumption made concerning the use of calibration in client systems. The mode controls the
// perspective from which advice is provided based on a stated assumption, or lack of assumption,
// about the capabilities of client systems.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

/// <summary>
///   The assumption made concerning the use of calibration in client systems. The mode controls the
///   perspective from which advice is provided based on a stated assumption, or lack of assumption,
///   about the capabilities of client systems.
/// </summary>
public enum Assumption
{
    /// <summary>
    ///   Provide advice from the perspective of a user whose client system may not support calibration.
    ///   In this case, the advice must address the lowest-common-denominator of client systems that do
    ///   not process the character input to recover the original data in a barcode. However, the advice
    ///   is provided in an agnostic fashion, with no assumptions about the capabilities of any client
    ///   system. Use this mode in scenarios where the client system capabilities are unknown.
    /// </summary>
    Agnostic = 0,

    /// <summary>
    ///   Provide advice from the perspective of a user whose client system supports calibration.
    /// </summary>
    Calibration = 1,

    /// <summary>
    ///   Provide advice from the perspective of a user whose client system does not support calibration.
    ///   In this case, the advice addresses only the lowest-common-denominator of client systems that do
    ///   not process the character input to recover the original data in a barcode.
    /// </summary>
    NoCalibration = 2
}