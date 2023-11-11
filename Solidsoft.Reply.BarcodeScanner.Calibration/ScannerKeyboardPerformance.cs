// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScannerKeyboardPerformance.cs" company="Solidsoft Reply Ltd.">
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
// 'Traffic Light' assessment of the performance of the barcode scanner keyboard input
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

/// <summary>
///   'Traffic Light' assessment of the performance of the barcode scanner keyboard input.
/// </summary>
public enum ScannerKeyboardPerformance
{
    /// <summary>
    ///   The performance of the keyboard is within the bounds of the 'high performance' threshold.
    /// </summary>
    High = 0,

    /// <summary>
    ///   The performance of the keyboard is within the bounds of the 'medium performance' threshold.
    /// </summary>
    Medium = 800,

    /// <summary>
    ///   The performance of the keyboard is within the bounds of the 'low performance' threshold.
    /// </summary>
    Low = 2000
}