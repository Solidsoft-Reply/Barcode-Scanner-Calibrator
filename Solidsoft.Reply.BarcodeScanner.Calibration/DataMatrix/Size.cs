// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Size.cs" company="Solidsoft Reply Ltd">
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
// Enumeration of Data Matrix code symbol sizes.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable UnusedMember.Global
namespace Solidsoft.Reply.BarcodeScanner.Calibration.DataMatrix;

/// <summary>
///   Enumeration of Data Matrix code symbol sizes.
/// </summary>
public enum Size {
    /// <summary>
    ///   Automatic sizing.
    /// </summary>
    Automatic = 0,

    /// <summary>
    ///   Data Matrix square 10x10.
    /// </summary>
    Dm10X10,

    /// <summary>
    ///   Data Matrix square 12x12.
    /// </summary>
    Dm12X12,

    /// <summary>
    ///   Data Matrix square 14 x 14.
    /// </summary>
    Dm14X14,

    /// <summary>
    ///   Data Matrix square 16 x 16.
    /// </summary>
    Dm16X16,

    /// <summary>
    ///   Data Matrix square 18 x 18.
    /// </summary>
    Dm18X18,

    /// <summary>
    ///   Data Matrix square 20 x 20.
    /// </summary>
    Dm20X20,

    /// <summary>
    ///   Data Matrix square 22 x 22.
    /// </summary>
    Dm22X22,

    /// <summary>
    ///   Data Matrix square 24 x 24.
    /// </summary>
    Dm24X24,

    /// <summary>
    ///   Data Matrix square 26 x 26.
    /// </summary>
    Dm26X26,

    /// <summary>
    ///   Data Matrix square 32 x 32.
    /// </summary>
    Dm32X32,

    /// <summary>
    ///   Data Matrix square 36 x 36.
    /// </summary>
    Dm36X36,

    /// <summary>
    ///   Data Matrix square 40 x 40.
    /// </summary>
    Dm40X40,

    /// <summary>
    ///   Data Matrix square 44 x 44.
    /// </summary>
    Dm44X44,

    /// <summary>
    ///   Data Matrix square 48 x 48.
    /// </summary>
    Dm48X48,

    /// <summary>
    ///   Data Matrix square 52 x 52.
    /// </summary>
    Dm52X52,

    /// <summary>
    ///   Data Matrix square 64 x 64.
    /// </summary>
    Dm64X64,

    /// <summary>
    ///   Data Matrix square 72 x 72.
    /// </summary>
    Dm72X72,

    /// <summary>
    ///   Data Matrix square 80 x 80.
    /// </summary>
    Dm80X80,

    /// <summary>
    ///   Data Matrix square 88 x 88.
    /// </summary>
    Dm88X88,

    /// <summary>
    ///   Data Matrix square 96 x 96.
    /// </summary>
    Dm96X96,

    /// <summary>
    ///   Data Matrix square 104 x 104.
    /// </summary>
    Dm104X104,

    /// <summary>
    ///   Data Matrix square 120 x 120.
    /// </summary>
    Dm120X120,

    /// <summary>
    ///   Data Matrix square 132 x 132.
    /// </summary>
    Dm132X132,

    /// <summary>
    ///   Data Matrix square 144 x144.
    /// </summary>
    Dm144X144,

    /// <summary>
    ///   Data Matrix rectangular 8 x 18.
    /// </summary>
    Dm8X18,

    /// <summary>
    ///   Data Matrix rectangular 8 x 32.
    /// </summary>
    Dm8X32,

    /// <summary>
    ///   Data Matrix rectangular 12 x 26.
    /// </summary>
    Dm12X26,

    /// <summary>
    ///   Data Matrix rectangular 12 x 36.
    /// </summary>
    Dm12X36,

    /// <summary>
    ///   Data Matrix rectangular 16 x 36.
    /// </summary>
    Dm16X36,

    /// <summary>
    ///   Data Matrix rectangular 16 x 48.
    /// </summary>
    Dm16X48,

    /// <summary>
    ///   Data Matrix rectangular extension 8 x 48;
    /// </summary>
    Dm8X48,

    /// <summary>
    ///   Data Matrix rectangular extension 8 x 64.
    /// </summary>
    Dm8X64,

    /// <summary>
    ///   Data Matrix rectangular extension 12 x 64.
    /// </summary>
    Dm12X64,

    /// <summary>
    ///   Data Matrix rectangular extension 16 x 64.
    /// </summary>
    Dm16X64,

    // Not yet supported.
    // DM24X32,
    // DM24X36,

    /// <summary>
    ///   Data Matrix rectangular extension 24 x 48.
    /// </summary>
    Dm24X48,

    /// <summary>
    ///   Data Matrix rectangular extension 24 x 64.
    /// </summary>
    Dm24X64,

    // Not yet supported.
    // DM26X32,

    /// <summary>
    ///   Data Matrix rectangular extension 26 x 40.
    /// </summary>
    Dm26X40,

    /// <summary>
    ///   Data Matrix rectangular extension 26 x 48.
    /// </summary>
    Dm26X48,

    /// <summary>
    ///   Data Matrix rectangular extension 26 x 64.
    /// </summary>
    Dm26X64,
}