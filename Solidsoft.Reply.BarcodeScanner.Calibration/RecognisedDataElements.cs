// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecognisedDataElement.cs" company="Solidsoft Reply Ltd.">
//   (c) 2023 Solidsoft Reply Ltd. All rights reserved.
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
// A recognised data element. 
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

/// <summary>
///   A recognised data element. 
/// </summary>
/// <remarks>
///   Optionally pass a list of recognised data elements to the Calibrator to constrain the GS1 application identifiers
///   and/or the ASC MH 10.8.2 data identifiers that the client software needs to recognise whe parsing data. This may
///   extend the range of transformation strategies that the calibrator can identify.
/// </remarks>
public class RecognisedDataElement {

    /// <summary>
    ///   Initializes a new instance of the <see cref="RecognisedDataElement"/> class.
    /// </summary>
    /// <param name="syntax">The syntax of the recognised data element.</param>
    /// <param name="identifier">The data element identifier.</param>
    public RecognisedDataElement(Syntax syntax, string identifier) {
        Syntax = syntax;
        Identifier = identifier;
    }

    /// <summary>
    ///   Gets the syntax of the recognised data element.
    /// </summary>
    public Syntax Syntax { get; }

    /// <summary>
    ///   Gets the identifier of the recognised data element.
    /// </summary>
    public string Identifier { get; }
}