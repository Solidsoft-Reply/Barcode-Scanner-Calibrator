// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Solidsoft Reply Ltd">
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
// Extension methods for Data Matrix barcode processing.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration.DataMatrix;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

/// <summary>
///   Extension methods for Data Matrix barcode processing.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class Extensions {
    /// <summary>
    ///   Tests for a substring within the string builder.
    /// </summary>
    /// <param name="value">The string builder.</param>
    /// <param name="substring">The substring.</param>
    /// <returns>True, if the string builder contains the substring; otherwise false.</returns>
    // ReSharper disable once UnusedMember.Global
    public static bool Contains(this StringBuilder? value, string substring) {
        return value is not null && value.ToString().Contains(substring, StringComparison.Ordinal);
    }

    /// <summary>
    ///   Returns the maximum number of bytes that can be encoded in a Data Matrix barcode of a given size.
    /// </summary>
    /// <param name="size">The size of the Data Matrix barcode.</param>
    /// <returns>The maximum number of bytes that can be encoded in a Data Matrix barcode of the given size.</returns>
    public static int MaxCapacity(this Size size) {
        return size switch {
            Size.Dm10X10 => CalculateMaxCapacity(1),
            Size.Dm12X12 => CalculateMaxCapacity(3),
            Size.Dm14X14 => CalculateMaxCapacity(6),
            Size.Dm16X16 => CalculateMaxCapacity(10),
            Size.Dm18X18 => CalculateMaxCapacity(16),
            Size.Dm20X20 => CalculateMaxCapacity(20),
            Size.Dm22X22 => CalculateMaxCapacity(26),
            Size.Dm24X24 => CalculateMaxCapacity(34),
            Size.Dm26X26 => CalculateMaxCapacity(42),
            Size.Dm32X32 => CalculateMaxCapacity(60),
            Size.Dm36X36 => CalculateMaxCapacity(84),
            Size.Dm40X40 => CalculateMaxCapacity(112),
            Size.Dm44X44 => CalculateMaxCapacity(142),
            Size.Dm48X48 => CalculateMaxCapacity(172),
            Size.Dm52X52 => CalculateMaxCapacity(202),
            Size.Dm64X64 => CalculateMaxCapacity(277),
            Size.Dm72X72 => CalculateMaxCapacity(365),
            Size.Dm80X80 => CalculateMaxCapacity(453),
            Size.Dm88X88 => CalculateMaxCapacity(573),
            Size.Dm96X96 => CalculateMaxCapacity(693),
            Size.Dm104X104 => CalculateMaxCapacity(813),
            Size.Dm120X120 => CalculateMaxCapacity(1047),
            Size.Dm132X132 => CalculateMaxCapacity(1301),
            Size.Dm144X144 => CalculateMaxCapacity(1555),
            Size.Dm8X18 => CalculateMaxCapacity(3),
            Size.Dm8X32 => CalculateMaxCapacity(8),
            Size.Dm12X26 => CalculateMaxCapacity(14),
            Size.Dm12X36 => CalculateMaxCapacity(20),
            Size.Dm16X36 => CalculateMaxCapacity(30),
            Size.Dm16X48 => CalculateMaxCapacity(72),
            Size.Dm8X48 => CalculateMaxCapacity(16),
            Size.Dm8X64 => CalculateMaxCapacity(22),
            Size.Dm12X64 => CalculateMaxCapacity(41),
            Size.Dm16X64 => CalculateMaxCapacity(60),
            Size.Dm24X48 => CalculateMaxCapacity(78),
            Size.Dm24X64 => CalculateMaxCapacity(106),
            Size.Dm26X40 => CalculateMaxCapacity(68),
            Size.Dm26X48 => CalculateMaxCapacity(88),
            Size.Dm26X64 => CalculateMaxCapacity(116),
            _ => -1
        };

        static int CalculateMaxCapacity(int mb) {
            return mb - 0;
        }
    }
}