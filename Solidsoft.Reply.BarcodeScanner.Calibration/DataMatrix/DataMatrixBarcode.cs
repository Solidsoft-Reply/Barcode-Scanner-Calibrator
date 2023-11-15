// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataMatrixBarcode.cs" company="Solidsoft Reply Ltd.">
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
// Creates a data matrix EC200 barcode as a stream of image data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

/* This code uses the ImageSharp library from SixLabors. This is a platform-independent 2D graphics library
 * with no reliance on unmanaged code. However, there are some drawbacks. It is not CLS-compliant, so there
 * could be issues when calling this code from certain languages. In addition, at the time of writing
 * (August 2023), there are significant performance issues when calling this code in V8's implementation of
 * WebAssembly (Chrome, Edge, Opera, etc.). ImageSharp depends on SIMD for high performance, but this is not
 * yet supported on WASM. NB. performance was not tested on other WASM implementations (e.g., Firefox), but
 * the same issues can be expected. In a WASM environment, use a Javascript library (e.g., bwip-js) to draw
 * barcodes.
 */
using System.Globalization;

using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Pbm;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;

namespace Solidsoft.Reply.BarcodeScanner.Calibration.DataMatrix;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;

using Color = Color;
using Image = Image;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using SixLabors.ImageSharp.PixelFormats;

using ZXing.Datamatrix;

using ZXing;
using SixLabors.ImageSharp.Processing;

/// <summary>
///   Creates a data matrix EC200 barcode as a stream of image data.
/// </summary>
[ExcludeFromCodeCoverage]
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class DataMatrixBarcode : IDisposable
{
    /// <summary>
    ///   Used to lock when creating a barcode;
    /// </summary>
    private static readonly object CreateBarcodeLockObject = new();

    /// <summary>
    ///   Indicates whether Dispose already been called.
    /// </summary>
    private bool _disposed;

    /// <summary>
    ///   Barcode module width multiplier.
    /// </summary>
    private float _multiplier = 1.0F;

    /// <summary>
    ///   Initializes a new instance of the <see cref="DataMatrixBarcode" /> class.
    /// </summary>
    public DataMatrixBarcode()
    {
    }

    /// <summary>
    ///   Finalizes an instance of the <see cref="DataMatrixBarcode" /> class.
    /// </summary>
    ~DataMatrixBarcode()
    {
        Dispose(false);
    }

    /// <summary>
    ///   Gets or sets the barcode foreground color.
    /// </summary>
    public Color ForegroundColor { get; set; } = Color.Black;

    /// <summary>
    ///   Gets or sets the barcode background color.
    /// </summary>
    public Color BackgroundColor { get; set; } = Color.White;

    /// <summary>
    ///   Gets or sets the image format.
    /// </summary>
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public IImageFormat ImageFormat { get; set; } = PngFormat.Instance;

    public float Multiplier
    {
        // ReSharper disable once UnusedMember.Global
        get => _multiplier;
        set
        {
            value = value switch
            {
                < 0.8f => 0.8f,
                > 20.0f => 20,
                _ => (float)Math.Round(value, 2)
            };

            _multiplier = value;
        }
    }

    /// <summary>
    ///   Creates a barcode and returns it as a readonly PNG stream.
    /// </summary>
    /// <param name="barcodeData">The barcode data. Encoded using Zint rules.</param>
    /// <returns>A stream containing PNG content.</returns>
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "Reviewed. Suppression is OK here.")]
    public Stream CreateBarcode(string barcodeData)
    {
        return CreateBarcode(barcodeData, BackgroundColor, ForegroundColor, ImageFormat);
    }

    /// <summary>
    ///   Creates a barcode and returns it as a readonly stream.
    /// </summary>
    /// <param name="barcodeData">The barcode data. Encoded using Zint rules.</param>
    /// <param name="imageFormat">The image format.</param>
    /// <returns>A stream containing PNG content.</returns>
    // ReSharper disable once UnusedMember.Global
    public Stream CreateBarcode(string barcodeData, IImageFormat imageFormat)
    {
        return CreateBarcode(barcodeData, BackgroundColor, ForegroundColor, imageFormat);
    }

    /// <summary>
    ///   Creates a barcode and returns it as a readonly stream.
    /// </summary>
    /// <param name="barcodeData">The barcode data. Encoded using Zint rules.</param>
    /// <param name="backgroundColor">The background colour of the barcode</param>
    /// <param name="foregroundColor">The foreground colour of the barcode</param>
    /// <returns>A stream containing PNG content.</returns>
    // ReSharper disable once UnusedMember.Global
    public Stream CreateBarcode(string barcodeData, Color backgroundColor, Color foregroundColor)
    {
        return CreateBarcode(barcodeData, backgroundColor, foregroundColor, ImageFormat);
    }

    /// <summary>
    ///   Creates a barcode and returns it as a readonly stream.
    /// </summary>
    /// <param name="barcodeData">The barcode data. Encoded using Zint rules.</param>
    /// <param name="backgroundColor">The background colour of the barcode.</param>
    /// <param name="foregroundColor">The foreground colour of the barcode.</param>
    /// <param name="imageFormat">The image format.</param>
    /// <returns>A stream containing PNG content.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public Stream CreateBarcode(
        string barcodeData, 
        Color backgroundColor, 
        Color foregroundColor,
        IImageFormat? imageFormat)
    {
        // Creating a barcode is not thread-safe, so this method is synchronized.
        lock (CreateBarcodeLockObject)
        {
            if (string.IsNullOrEmpty(barcodeData))
            {
                return new MemoryStream(Array.Empty<byte>(), false);
            }

            var (modulesX, modulesY) = CalculateDataMatrixModuleSize(barcodeData);

            // Workaround for issues with zXing
            if (modulesY != modulesX)
            {
                modulesX = modulesY = Convert.ToInt32((modulesY > modulesX ? modulesY : modulesX) * 0.85);
            }

            // Create a Data Matrix barcode writer
            var barcodeWriter = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.DATA_MATRIX,
                Options = new DatamatrixEncodingOptions
                {
                    Width = (modulesX + 2) * (int)_multiplier,
                    Height = (modulesY + 2) * (int)_multiplier,
                    Margin = 1,
                    SymbolShape = ZXing.Datamatrix.Encoder.SymbolShapeHint.FORCE_SQUARE
                }
            };

            // Generate the barcode
            var pixelData = barcodeWriter.Write(barcodeData);

            // Create a new image with the desired background color
            using var backgroundImage = new Image<Rgba32>(Configuration.Default, pixelData.Width, pixelData.Height, backgroundColor);

            // Draw the barcode on the background image using the desired foreground color
            backgroundImage.Mutate(context =>
            {
                // Load the barcode image from the pixel data
                using var barcodeImage = Image.LoadPixelData<Rgba32>(pixelData.Pixels, pixelData.Width, pixelData.Height);

                for (var y = 0; y < pixelData.Height; y++)
                {
                    for (var x = 0; x < pixelData.Width; x++)
                    {
                        var currentPixel = barcodeImage[x, y];

                        barcodeImage[x, y] = currentPixel switch
                        {
                            _ when currentPixel.Equals(Rgba32.ParseHex("000000FF")) => foregroundColor,
                            _ when currentPixel.Equals(Rgba32.ParseHex("FFFFFFFF")) => backgroundColor,
                            _ => backgroundColor
                        };
                    }
                }

                context.DrawImage(barcodeImage, new Point(0, 0), 1);
            });

            // Write the image to a MemoryStream
            var memoryStream = new MemoryStream();

            if (imageFormat != null)
            {
                backgroundImage.Save(memoryStream, imageFormat.Name.ToUpper(CultureInfo.InvariantCulture) switch
                {
                    "PNG" => new PngEncoder(),
                    "BMP" => new BmpEncoder(),
                    "GIF" => new GifEncoder(),
                    "JPEG" => new JpegEncoder(),
                    "PBM" => new PbmEncoder(),
                    "TGA" => new TgaEncoder(),
                    "TIFF" => new TiffEncoder(),
                    "WEBP" => new WebpEncoder(),
                    _ => new PngEncoder()
                });
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }

    /// <summary>
    ///   Public implementation of Dispose method for the ZintNetLib object.
    /// </summary>
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "Reviewed. Suppression is OK here.")]
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///   Protected implementation of Dispose method for the ZintNetLib object.
    /// </summary>
    /// <param name="disposing">Indicates whether the object is being disposed explicitly.</param>
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "Reviewed. Suppression is OK here.")]
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Free any other managed objects here. 
        }

        // Free any unmanaged objects here. 
        _disposed = true;
    }

    /// <summary>
    ///   Calculate the Data Matrix module size.
    /// </summary>
    /// <param name="barcodeData">The barcode data.</param>
    /// <returns>The number of horizontal and vertical modules in the barcode.</returns>
    private static (int modulesX, int modulesY) CalculateDataMatrixModuleSize(string barcodeData)
    {
        // Create an instance of the DataMatrixWriter
        var dataMatrixWriter = new DataMatrixWriter();

        // Encode the barcode data without specifying the size
        var encodedData = dataMatrixWriter.encode(barcodeData, BarcodeFormat.DATA_MATRIX, 0, 0, null);

        return (encodedData.Width, encodedData.Height);
    }
}