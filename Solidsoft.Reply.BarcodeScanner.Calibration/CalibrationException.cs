﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalibrationException.cs" company="Solidsoft Reply Ltd.">
//   (c) 2018-2024 Solidsoft Reply Ltd. All rights reserved.
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
// Represents a calibration exception.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System;
using System.Runtime.Serialization;

/// <summary>
///   Represents a calibration exception.
/// </summary>
[Serializable]
#pragma warning disable S3871
internal class CalibrationException : Exception
#pragma warning restore S3871
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="CalibrationException" /> class.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public CalibrationException()
        : base(string.Empty)
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="CalibrationException" /> class.
    /// </summary>
    /// <param name="message">
    ///   The message.
    /// </param>
    public CalibrationException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="CalibrationException" /> class.
    /// </summary>
    /// <param name="message">
    ///   The message.
    /// </param>
    /// <param name="innerException">
    ///   The inner exception.
    /// </param>
    public CalibrationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="CalibrationException" /> class.
    /// </summary>
    /// <param name="info">The serialization information.</param>
    /// <param name="context">The streaming context.</param>
#if NET5_0_OR_GREATER
    [Obsolete("Formatter serialisation has been deprecated in .NET.", DiagnosticId = "SYSLIB0051")]
#endif
    protected CalibrationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}