﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;

namespace RevitFamilyBrowser
{
    public static class Tools
    {
        public static void CollectFamilyData(Autodesk.Revit.DB.Document doc)
        {
            FilteredElementCollector families;
            Properties.Settings.Default.CollectedData = string.Empty;
            families = new FilteredElementCollector(doc).OfClass(typeof(Family));
            string temp = string.Empty;

            foreach (var item in families)
            {
                if (!(item.Name.Contains("Standart") ||
                      item.Name.Contains("Mullion")))
                {
                    Family family = item as Family;
                    FamilySymbol symbol;
                    temp += item.Name;
                    ISet<ElementId> familySymbolId = family.GetFamilySymbolIds();
                    foreach (ElementId id in familySymbolId)
                    {
                        symbol = family.Document.GetElement(id) as FamilySymbol;
                        {
                            temp += "#" + symbol.Name;
                        }
                    }
                    temp += "\n";
                }
            }
            Properties.Settings.Default.CollectedData = temp;
        }

        public static void CreateImages(Autodesk.Revit.DB.Document doc)
        {
            //TaskDialog.Show("Create Image", "Process Images");
            FilteredElementCollector collector;
            collector = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance));
            int Instances = 0;

            foreach (FamilyInstance fi in collector)
            {
                try
                {
                    ElementId typeId = fi.GetTypeId();
                    ElementType type = doc.GetElement(typeId) as ElementType;
                    System.Drawing.Size imgSize = new System.Drawing.Size(200, 200);
                    Instances++;
                    //------------Prewiew Image-----
                    Bitmap image = type.GetPreviewImage(imgSize);

                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(ConvertBitmapToBitmapSource(image)));
                    encoder.QualityLevel = 25;

                    string TempImgFolder = System.IO.Path.GetTempPath() + "FamilyBrowser\\";
                    if (!System.IO.Directory.Exists(TempImgFolder))
                    {
                        System.IO.Directory.CreateDirectory(TempImgFolder);
                    }
                    string filename = TempImgFolder + type.Name + ".bmp";
                    FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write);
                    encoder.Save(file);
                    file.Close();
                }
                //TODO
                catch (Exception)
                {
                    ;
                }
            }
        }

        private static BitmapSource ConvertBitmapToBitmapSource(Bitmap bmp)
        {
            return System.Windows.Interop.Imaging
                .CreateBitmapSourceFromHBitmap(
                    bmp.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
        }

        public static BitmapSource GetImage(IntPtr bm)
        {
            BitmapSource bmSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bm,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            return bmSource;
        }
    }
}
