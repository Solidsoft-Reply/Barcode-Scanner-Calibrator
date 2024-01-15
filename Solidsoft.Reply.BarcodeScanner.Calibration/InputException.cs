// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InputException.cs" company="Solidsoft Reply Ltd.">
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
// An input exception.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System;
using System.Runtime.Serialization;

/// <summary>
///   An input exception.
/// </summary>
[Serializable]
public class InputException : Exception
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="InputException" /> class.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once MemberCanBeProtected.Global
    public InputException()
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="InputException" /> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    // ReSharper disable once MemberCanBeProtected.Global
    public InputException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="InputException" /> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    // ReSharper disable once MemberCanBeProtected.Global
    public InputException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="InputException" /> class.
    /// </summary>
    /// <param name="errorNumber">
    ///   The error number.
    /// </param>
    /// <param name="message">
    ///   The message.
    /// </param>
    /// <param name="isFatal">
    ///   Indicates whether the exception is fatal.
    /// </param>
    public InputException(int errorNumber, string message, bool isFatal)
        : base(message)
    {
        ErrorNumber = errorNumber;
        IsFatal = isFatal;
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="InputException" /> class.
    /// </summary>
    /// <param name="errorNumber">
    ///   The error number.
    /// </param>
    /// <param name="message">
    ///   The message.
    /// </param>
    /// <param name="isFatal">
    ///   Indicates whether the exception is fatal.
    /// </param>
    /// <param name="innerException">
    ///   The inner exception.
    /// </param>
    public InputException(int errorNumber, string message, bool isFatal, Exception innerException)
        : base(message, innerException) {
        ErrorNumber = errorNumber;
        IsFatal = isFatal;
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="InputException" /> class.
    /// </summary>
    /// <param name="info">The serialization information.</param>
    /// <param name="context">The streaming context.</param>
    [Obsolete("Formatter serialisation has been deprecated in .NET.", DiagnosticId = "SYSLIB0051")]
    protected InputException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    /// <summary>
    ///   Gets the error number.
    /// </summary>
    public int ErrorNumber { get; }

    /// <summary>
    ///   Gets a value indicating whether the error is fatal (further parsing is aborted).
    /// </summary>
    public bool IsFatal { get; }
}