// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Solidsoft Reply Ltd.">
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
public static class Extensions
{
    /// <summary>
    ///   Tests for a substring within the string builder
    /// </summary>
    /// <param name="value">The string builder.</param>
    /// <param name="substring">The substring.</param>
    /// <returns>True, if the string builder contains the substring; otherwise false.</returns>
    // ReSharper disable once UnusedMember.Global
    public static bool Contains(this StringBuilder? value, string substring)
    {
        return value is not null && value.ToString().Contains(substring, StringComparison.Ordinal);
    }

    /// <summary>
    ///   Returns the maximum number of bytes that can be encoded in a Data Matrix barcode of a given size.
    /// </summary>
    /// <param name="dataMatrixSize">The size of the Data Matrix barcode.</param>
    /// <returns>The maximum number of bytes that can be encoded in a Data Matrix barcode of the given size.</returns>
    public static int MaxCapacity(this DataMatrixSize dataMatrixSize)
    {
        return dataMatrixSize switch
               {
                   DataMatrixSize.Dm10X10   => CalculateMaxCapacity(1),
                   DataMatrixSize.Dm12X12   => CalculateMaxCapacity(3),
                   DataMatrixSize.Dm14X14   => CalculateMaxCapacity(6),
                   DataMatrixSize.Dm16X16   => CalculateMaxCapacity(10),
                   DataMatrixSize.Dm18X18   => CalculateMaxCapacity(16),
                   DataMatrixSize.Dm20X20   => CalculateMaxCapacity(20),
                   DataMatrixSize.Dm22X22   => CalculateMaxCapacity(26),
                   DataMatrixSize.Dm24X24   => CalculateMaxCapacity(34),
                   DataMatrixSize.Dm26X26   => CalculateMaxCapacity(42),
                   DataMatrixSize.Dm32X32   => CalculateMaxCapacity(60),
                   DataMatrixSize.Dm36X36   => CalculateMaxCapacity(84),
                   DataMatrixSize.Dm40X40   => CalculateMaxCapacity(112),
                   DataMatrixSize.Dm44X44   => CalculateMaxCapacity(142),
                   DataMatrixSize.Dm48X48   => CalculateMaxCapacity(172),
                   DataMatrixSize.Dm52X52   => CalculateMaxCapacity(202),
                   DataMatrixSize.Dm64X64   => CalculateMaxCapacity(277),
                   DataMatrixSize.Dm72X72   => CalculateMaxCapacity(365),
                   DataMatrixSize.Dm80X80   => CalculateMaxCapacity(453),
                   DataMatrixSize.Dm88X88   => CalculateMaxCapacity(573),
                   DataMatrixSize.Dm96X96   => CalculateMaxCapacity(693),
                   DataMatrixSize.Dm104X104 => CalculateMaxCapacity(813),
                   DataMatrixSize.Dm120X120 => CalculateMaxCapacity(1047),
                   DataMatrixSize.Dm132X132 => CalculateMaxCapacity(1301),
                   DataMatrixSize.Dm144X144 => CalculateMaxCapacity(1555),
                   DataMatrixSize.Dm8X18    => CalculateMaxCapacity(3),
                   DataMatrixSize.Dm8X32    => CalculateMaxCapacity(8),
                   DataMatrixSize.Dm12X26   => CalculateMaxCapacity(14),
                   DataMatrixSize.Dm12X36   => CalculateMaxCapacity(20),
                   DataMatrixSize.Dm16X36   => CalculateMaxCapacity(30),
                   DataMatrixSize.Dm16X48   => CalculateMaxCapacity(72),
                   DataMatrixSize.Dm8X48    => CalculateMaxCapacity(16),
                   DataMatrixSize.Dm8X64    => CalculateMaxCapacity(22),
                   DataMatrixSize.Dm12X64   => CalculateMaxCapacity(41),
                   DataMatrixSize.Dm16X64   => CalculateMaxCapacity(60),
                   DataMatrixSize.Dm24X48   => CalculateMaxCapacity(78),
                   DataMatrixSize.Dm24X64   => CalculateMaxCapacity(106),
                   DataMatrixSize.Dm26X40   => CalculateMaxCapacity(68),
                   DataMatrixSize.Dm26X48   => CalculateMaxCapacity(88),
                   DataMatrixSize.Dm26X64   => CalculateMaxCapacity(116),
                   _                        => -1
               };

        static int CalculateMaxCapacity(int mb)
        {
            return mb - 0;
        }
    }
}