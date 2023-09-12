using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Library.Service.Helpers
{

    public class ConvertExcelToImage
    {
        private class ImageDimension
        {
            public int LeftLower { get; set; }
            public int LeftUpper { get; set; }
            public int TopLower { get; set; }
            public int TopUpper { get; set; }

            public Image image { get; set; }

        }

        //tarek talukder--23May2020
        private List<IWorkbook> WorkBooks = null;
        private List<Dictionary<string, ImageDimension>> WorkBookImages = null;

        private float _height = 0, _width = 0;
        public ConvertExcelToImage(List<IWorkbook> WorkBookCollection, float HeightInMM, float WidthInMM)
        {
            WorkBooks = WorkBookCollection;
            _height = HeightInMM; _width = WidthInMM;
        }
        public void ConvertToImage()
        {
            WorkBookImages = new List<Dictionary<string, ImageDimension>>();
            ExcelToPdfConverter converter = null;
            PdfDocument pdfDocument = null;
            ExcelToPdfConverterSettings pdfConverterSettings = null;
            MemoryStream stream = null;
            PdfLoadedDocument sds = null;
            for (int i = 0; i < WorkBooks.Count; i++)
            {
                Dictionary<string, ImageDimension> imageData = new Dictionary<string, ImageDimension>();

                converter = new ExcelToPdfConverter(WorkBooks[i]);

                //Initialize PDF document
                pdfDocument = new PdfDocument();
                pdfConverterSettings = new ExcelToPdfConverterSettings();
                pdfConverterSettings.AutoDetectComplexScript = true;
                pdfConverterSettings.ExportQualityImage = true;
                pdfConverterSettings.EmbedFonts = true;
                //Convert Excel document into PDF document
                pdfDocument = converter.Convert(pdfConverterSettings);
                stream = new MemoryStream();
                pdfDocument.Save(stream);
                sds = new PdfLoadedDocument(stream);


                ImageExportSettings settings = new ImageExportSettings();
                settings.KeepAspectRatio = true;
                //settings.DpiX = 300;
                //settings.DpiY = 300;
                for (int p = 0; p < sds.Pages.Count; p++)
                {
                    using (var photoStream = new MemoryStream())
                    {
                        using (Image image = sds.ExportAsImage(p, settings))
                        {
                            try
                            {

                                ImageDimension dim = new ImageDimension();
                                dim.image = image;


                                Dictionary<string, ImageDimension> k = WorkBookImages.Where(ee => ee.ContainsKey(p.ToString()) == true).FirstOrDefault();
                                if (k == null)
                                {
                                    ScanImage(dim);
                                    imageData.Add(p.ToString(), CropImage(dim));
                                }
                                else
                                {
                                    dim.LeftLower = k[p.ToString()].LeftLower;
                                    dim.LeftUpper = k[p.ToString()].LeftUpper;
                                    dim.TopLower = k[p.ToString()].TopLower;
                                    dim.TopUpper = k[p.ToString()].TopUpper;

                                    imageData.Add(p.ToString(), CropImage(dim));
                                }

                            }
                            catch (Exception)
                            {

                            }
                        }
                    }

                }
                WorkBookImages.Add(imageData);
            }


        }

        private void ScanImage(ImageDimension imageDimension)
        {

            Bitmap bmp = (Bitmap)imageDimension.image;
            imageDimension.LeftLower = imageDimension.image.Width;
            imageDimension.TopLower = imageDimension.image.Height;
            for (int i = 0; i < imageDimension.image.Height; i++)
            {
                for (int j = 0; j < imageDimension.image.Width; j++)
                {
                    //Get the color at each pixel
                    Color pixelColor = bmp.GetPixel(j, i);

                    if (pixelColor.R < 220 || pixelColor.G < 220 || pixelColor.B < 220)
                    {
                        //min left lower
                        if (imageDimension.LeftLower > j)
                            imageDimension.LeftLower = j;

                        //min top 
                        if (imageDimension.TopLower > i)
                            imageDimension.TopLower = i;

                        //max left--to right side
                        if (imageDimension.LeftUpper < j)
                            imageDimension.LeftUpper = j;

                        //max top--to lower part of the image
                        if (imageDimension.TopUpper < i)
                            imageDimension.TopUpper = i;
                    }
                }
            }

        }
        private ImageDimension CropImage(ImageDimension imageDimension)
        {
            Bitmap bmp = (Bitmap)imageDimension.image;

            Rectangle crop = new Rectangle(imageDimension.LeftLower, imageDimension.TopLower, imageDimension.LeftUpper - imageDimension.LeftLower, imageDimension.TopUpper - imageDimension.TopLower);

            var bmp1 = new Bitmap(crop.Width, crop.Height);
            bmp.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

            using (var gr = Graphics.FromImage(bmp1))
            {
                gr.DrawImage(bmp, new Rectangle(0, 0, bmp1.Width, bmp1.Height), crop, GraphicsUnit.Pixel);
            }

            imageDimension.image = ResizeImage((Image)bmp1, mmToPixel(_height, bmp1.VerticalResolution), mmToPixel(_width, bmp1.HorizontalResolution));

            return imageDimension;
        }

        private Bitmap ResizeImage(Image image, int height, int width)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            //destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.Bicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        private int mmToPixel(float mm, float dpi)
        {
            return (int)Math.Round((mm / 25.4) * dpi);
        }


        public PdfDocument ConvertToPdf(float PaddingInPoint = 0)
        {
            this.ConvertToImage();

            PdfUnitConvertor converter = new PdfUnitConvertor();
            PdfDocument doc = new PdfDocument();
            doc.PageSettings.Margins = new PdfMargins { Bottom = 0, Left = 0, Right = 0, Top = 0 };
            PaddingInPoint = converter.ConvertFromPixels(PaddingInPoint, PdfGraphicsUnit.Point);
            float CurrentX = PaddingInPoint, CurrentY = PaddingInPoint;

            PdfSection pdfsection = doc.Sections.Add();
            pdfsection.PageSettings.Size = PdfPageSize.A4;
            pdfsection.PageSettings.Margins = new PdfMargins { Bottom = 0, Left = 10, Right = 0, Top = 10 };
            pdfsection.PageSettings.Size = new SizeF(794, 1123);
            pdfsection.PageSettings.Orientation = PdfPageOrientation.Landscape;
            PdfPage page = pdfsection.Pages.Add();
            PdfGraphics graphics = page.Graphics;


            for (int i = 0; i < WorkBookImages.Count; i++)
            {
                foreach (var ImageObject in WorkBookImages[i].Values)
                {



                    float ImageWidth = converter.ConvertFromPixels(ImageObject.image.Width, PdfGraphicsUnit.Point);
                    float ImageHeight = converter.ConvertFromPixels(ImageObject.image.Height, PdfGraphicsUnit.Point);

                    float PageWidth = converter.ConvertFromPixels(graphics.ClientSize.Width, PdfGraphicsUnit.Point);
                    float PageHeight = converter.ConvertFromPixels(graphics.ClientSize.Height, PdfGraphicsUnit.Point);

                    //first check whether the image is larger than the page
                    if (ImageWidth > PageWidth || ImageHeight > PageHeight)
                    {
                        CurrentX = PaddingInPoint; CurrentY = PaddingInPoint;
                        continue;
                    }
                    //check if the width is overflown on the right side
                    if (CurrentX + ImageWidth + PaddingInPoint > PageWidth)
                    {
                        //try to carrige return
                        if (CurrentY + ImageHeight + PaddingInPoint > PageHeight)
                        {
                            CurrentX = PaddingInPoint; CurrentY = PaddingInPoint;

                            pdfsection = doc.Sections.Add();
                            pdfsection.PageSettings.Size = PdfPageSize.A4;
                            pdfsection.PageSettings.Margins = new PdfMargins { Bottom = 0, Left = 10, Right = 0, Top = 10 };
                            pdfsection.PageSettings.Size = new SizeF(794, 1123);
                            pdfsection.PageSettings.Orientation = PdfPageOrientation.Landscape;
                            page = pdfsection.Pages.Add();
                            graphics = page.Graphics;


                        }
                        else
                        {
                            CurrentY += ImageHeight + PaddingInPoint;
                            CurrentX = PaddingInPoint;
                            if (CurrentY + ImageHeight + PaddingInPoint > PageHeight)
                            {
                                CurrentX = PaddingInPoint; CurrentY = PaddingInPoint;

                                pdfsection = doc.Sections.Add();
                                pdfsection.PageSettings.Size = PdfPageSize.A4;
                                pdfsection.PageSettings.Margins = new PdfMargins { Bottom = 0, Left = 10, Right = 0, Top = 10 };
                                pdfsection.PageSettings.Size = new SizeF(794, 1123);
                                pdfsection.PageSettings.Orientation = PdfPageOrientation.Landscape;
                                page = pdfsection.Pages.Add();
                                graphics = page.Graphics;

                            }
                        }
                    }


                    graphics.DrawImage(new PdfBitmap(ImageObject.image), CurrentX, CurrentY);
                    CurrentX += PaddingInPoint + ImageWidth;
                }
            }


            return doc;

        }
        public PdfDocument ConvertToPdfSinglePage(float PaddingInPoint = 0)
        {
            this.ConvertToImage();

            float CurrentX = PaddingInPoint, CurrentY = PaddingInPoint;
            PdfUnitConvertor converter = new PdfUnitConvertor();
            PdfDocument doc = new PdfDocument();
            doc.PageSettings.Margins = new PdfMargins { Bottom = 0, Left = 0, Right = 0, Top = 0 };




            PaddingInPoint = converter.ConvertFromPixels(PaddingInPoint, PdfGraphicsUnit.Point);
            for (int i = 0; i < WorkBookImages.Count; i++)
            {
                foreach (var ImageObject in WorkBookImages[i].Values)
                {



                    float ImageWidth = converter.ConvertFromPixels(ImageObject.image.Width, PdfGraphicsUnit.Point);
                    float ImageHeight = converter.ConvertFromPixels(ImageObject.image.Height, PdfGraphicsUnit.Point);



                    CurrentX = 0; CurrentY = 0;

                    PdfSection pdfsection = doc.Sections.Add();
                    pdfsection.PageSettings.Size = PdfPageSize.A4;
                    pdfsection.PageSettings.Margins = new PdfMargins { Bottom = 0, Left = 0, Right = 0, Top = 0 };
                    pdfsection.PageSettings.Size = new SizeF(ImageWidth, ImageHeight);
                    PdfPage page = pdfsection.Pages.Add();
                    PdfGraphics graphics = page.Graphics;




                    graphics.DrawImage(new PdfBitmap(ImageObject.image), CurrentX + PaddingInPoint, CurrentY + PaddingInPoint);
                    CurrentX += ImageWidth;
                }
            }



            return doc;
        }
    }

}
