// -----------------------------------------------------------------------
// <copyright file="ImageController.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Controllers
{
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Http;
    using Logic;

    /// <summary>
    /// Provides the ability to dynamically generate images.
    /// </summary>
    [RoutePrefix("api/images")]
    public class ImageController : BaseApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageController"/> class.
        /// </summary>
        /// <param name="service">Provides access to core services.</param>
        public ImageController(IBotService service) : base(service)
        {
        }

        /// <summary>
        /// Dynamically generates an image.
        /// </summary>
        /// <param name="value">A <see cref="string>"/> that will be used to generate the image.</param>
        /// <returns>A dynamically generated image.</returns>
        [HttpGet]
        [Route("dynamic")]
        public HttpResponseMessage GetImage(string value)
        {
            HttpResponseMessage response;

            using (Bitmap bmp = new Bitmap(65, 65, PixelFormat.Format32bppArgb))
            {
                bmp.MakeTransparent();

                using (Brush brush = new SolidBrush(Color.FromArgb(255, 0, 127, 255)))
                {
                    using (Graphics graphic = Graphics.FromImage(bmp))
                    {
                        graphic.SmoothingMode = SmoothingMode.AntiAlias;
                        graphic.FillEllipse(brush, 0, 0, 24, 24);

                        graphic.DrawString(value.Substring(0, 1).ToUpper(), new Font("Calibri (Body)", 16, FontStyle.Bold), Brushes.White, 0, 0);

                        using (MemoryStream stream = new MemoryStream())
                        {
                            bmp.Save(stream, ImageFormat.Png);
                            stream.Position = 0;

                            response = new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new ByteArrayContent(stream.ToArray())
                            };

                            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                            response.Content.Headers.ContentLength = stream.Length;

                            return response;
                        }
                    }
                }
            }
        }
    }
}