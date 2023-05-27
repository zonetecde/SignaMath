using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SignaMath.Extension
{
    internal static class Extension
    {
        internal static Brush PickBrush()
        {
            Brush result = Brushes.Transparent;

            Random rnd = new Random();

            Type brushesType = typeof(Brushes);

            PropertyInfo[] properties = brushesType.GetProperties();

            int random = rnd.Next(properties.Length);
            result = (Brush)properties[random].GetValue(null, null)!;

            return result;
        }

        internal static void WriteToPng(UIElement element, string filename)
        {
            var rect = new Rect(element.RenderSize);
            var visual = new DrawingVisual();

            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(new VisualBrush(element), null, rect);
            }

            var dpiX = 300; // Résolution horizontale en DPI
            var dpiY = 300; // Résolution verticale en DPI

            var pixelWidth = (int)(rect.Width * dpiX / 96);
            var pixelHeight = (int)(rect.Height * dpiY / 96);

            var bitmap = new RenderTargetBitmap(
                pixelWidth, pixelHeight, dpiX, dpiY, PixelFormats.Default);
            bitmap.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var file = File.OpenWrite(filename))
            {
                encoder.Save(file);
            }
        }

        internal static double StrToDouble(string expBrute)
        {
            double approximation;
            if (expBrute.Contains('/'))
            {
                // fraction toujours pas convertie
                approximation = ParseDoubleFromString(expBrute);
            }
            else
            {
                approximation = double.Parse(expBrute.Replace(",", "."), CultureInfo.InvariantCulture);
            }

            return approximation;
        }

        private static double ParseDoubleFromString(string num)
        {
            //removes multiple spces between characters, cammas, and leading/trailing whitespace
            num = Regex.Replace(num.Replace(",", ""), @"\s+", " ").Trim();
            double d = 0;
            int whole = 0;
            double numerator;
            double denominator;

            //is there a fraction?
            if (num.Contains("/"))
            {
                //is there a space?
                if (num.Contains(" "))
                {
                    //seperate the integer and fraction
                    int firstspace = num.IndexOf(" ");
                    string fraction = num.Substring(firstspace, num.Length - firstspace);
                    //set the integer
                    whole = int.Parse(num.Substring(0, firstspace));
                    //set the numerator and denominator
                    numerator = double.Parse(fraction.Split("/".ToCharArray())[0]);
                    denominator = double.Parse(fraction.Split("/".ToCharArray())[1]);
                }
                else
                {
                    //set the numerator and denominator
                    numerator = double.Parse(num.Split("/".ToCharArray())[0]);
                    denominator = double.Parse(num.Split("/".ToCharArray())[1]);
                }

                //is it a valid fraction?
                if (denominator != 0)
                {
                    d = whole + numerator / denominator;
                }
            }
            else
            {
                //parse the whole thing
                d = double.Parse(num.Replace(" ", ""));
            }

            return d;
        }
    }
}
