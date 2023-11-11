// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAdvice.cs" company="Solidsoft Reply Ltd.">
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
// Represents an ordered sequence of advice items.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable UnusedMemberInSuper.Global
namespace Solidsoft.Reply.BarcodeScanner.Calibration;

/// <summary>
/// Represents an ordered sequence of advice items.
/// </summary>
public interface IAdvice<out TAdviceItem, TAdviceType> 
    where TAdviceItem : IAdviceItem<TAdviceType> 
    where TAdviceType : Enum {
    /// <summary>
    ///   Gets an ordered collection of advice items.
    /// </summary>
    /// <returns>An ordered collection of advice items.</returns>
    public IEnumerable<TAdviceItem> Items { get; }
}