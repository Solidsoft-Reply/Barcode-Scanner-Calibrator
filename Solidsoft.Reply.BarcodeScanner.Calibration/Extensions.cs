﻿// --------------------------------------------------------------------------------------------------------------------
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
// Extension methods.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System.Globalization;

/// <summary>
/// Extension methods.
/// </summary>
internal static class Extensions {

    /// <summary>
    ///   Converts the value of this instance to its equivalent string representation using culture-invariant format
    ///   information.
    /// </summary>
    /// <param name="thisCharacter">The character to be converted.</param>
    /// <returns>A culture-invariant string.</returns>
    public static string ToInvariantString(this char thisCharacter) {
        return thisCharacter.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///   Maps from one Nullable<T> to another. This operation creates a functor over nullable structs.
    /// </summary>
    /// <typeparam name="TSource">The non-nullable source type.</typeparam>
    /// <typeparam name="TResult">The non-nullable resulting type.</typeparam>
    /// <param name="value">The nullable source value.</param>
    /// <param name="func">The function to be lifted into the functor.</param>
    /// <returns>The nullable resulting value.</returns>
    public static TResult? Map<TSource, TResult>(this TSource? value, Func<TSource, TResult> func)
        where TSource : struct
        where TResult : struct
        => value switch {
            null => null,
            _ => func(value.Value)
        };

}