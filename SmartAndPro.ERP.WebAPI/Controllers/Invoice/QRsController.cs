using BarcodeStandard;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using QRCoder;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SkiaSharp;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using ZXing;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using ZXing.QrCode;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using Type = BarcodeStandard.Type;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;

namespace SmartAndPro.ERP.WebAPI.Controllers.Invoice
{
    [Route("api/invoice/[controller]")]
    [ApiController]
[Authorize]
[PermissionAuthorize("SalesInvoice.Read")]
    public class QRsController : ControllerBase
    {
        #region Feilds

        #endregion
        
        #region Constructors
        public QRsController()
        {
            
        }
        #endregion

        #region APIs
        [HttpGet("qrcode/{url}")]
        public IActionResult GenerateQRCodeS(string url)
        {
            string decodedUrl = Uri.UnescapeDataString(url);

            if (!decodedUrl.StartsWith("http://") && !decodedUrl.StartsWith("https://"))
            {
                decodedUrl = "https://" + decodedUrl;
            }

            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Width = 100,
                    Height = 100
                }
            };

            var pixelData = writer.Write(decodedUrl);
            var bitmap = new Bitmap(pixelData.Width, pixelData.Height);
            var rectangle = new Rectangle(0, 0, pixelData.Width, pixelData.Height);
            var bitmapData = bitmap.LockBits(rectangle, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
            bitmap.UnlockBits(bitmapData);

            return File(BitmapToByteArray(bitmap), "image/png");
        }
        
        [HttpGet("barcode/{data}")]
        public IActionResult GenerateBarCode(string data)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                Barcode barcode = new Barcode
                {
                    IncludeLabel = true
                };

                SKColorF black = new SKColorF(0, 0, 0);
                SKColorF white = new SKColorF(1, 1, 1);

                using (SKImage barcodeImage = barcode.Encode(Type.Code128, data, black, white, 300, 150))
                using (SKData dataStream = barcodeImage.Encode(SKEncodedImageFormat.Png, 100))  // Encode the SKImage as PNG
                {
                    // Return the PNG as a file
                    return File(dataStream.ToArray(), "image/png");
                }
            }
        }
        #endregion
        
        #region Methods
        private byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }
        #endregion
    }
}

