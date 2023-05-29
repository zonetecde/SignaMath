using AngouriMath;
using AngouriMath.Extensions;
using SignaMath.Classes;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static AngouriMath.Entity;

namespace SignaMath.Extension
{
    internal static class Extension
    {
        // Méthode qui retourne une brosse de couleur aléatoire
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

        // Méthode qui écrit un élément UI dans un fichier PNG ou le copie dans le presse-papiers
        internal static void WriteToPng(UIElement element, string filename, bool copyClipboard = false)
        {
            // Création d'un rectangle pour définir la taille de l'élément
            var rect = new Rect(element.RenderSize);
            var visual = new DrawingVisual();

            // Ouverture du contexte graphique pour dessiner l'élément dans le rectangle
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(new VisualBrush(element), null, rect);
            }

            var dpiX = 300; // Résolution horizontale en DPI
            var dpiY = 300; // Résolution verticale en DPI

            // Calcul des dimensions en pixels en fonction de la résolution
            var pixelWidth = (int)(rect.Width * dpiX / 96);
            var pixelHeight = (int)(rect.Height * dpiY / 96);

            // Création d'une image de rendu avec les dimensions calculées
            var bitmap = new RenderTargetBitmap(
                pixelWidth, pixelHeight, dpiX, dpiY, PixelFormats.Default);
            bitmap.Render(visual);

            // Création d'un encodeur pour le format PNG
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            if (!copyClipboard)
            {
                // Sauvegarde de l'image dans un fichier
                using (var file = File.OpenWrite(filename))
                {
                    encoder.Save(file);
                }
            }
            else
            {
                // Copie de l'image dans le presse-papiers
                Clipboard.SetImage(bitmap);
            }
        }

        /// <summary>
        /// Remplace les valeurs remarquables dans une expression
        /// </summary>
        /// <param name="newFormula">L'expression mathématiques</param>
        internal static string ReplaceValeurRemarquable(string newFormula)
        {
            return newFormula.Replace("e", "(" + Math.Exp(1).ToString(CultureInfo.InvariantCulture) + ")");
        }


        /// <summary>
        /// Converti une expression mathématique en son résultat
        /// </summary>
        /// <param name="expBrute">Expression mathématique brute</param>
        /// <param name="isAlone">Indique si on s'intéresse uniquement au signe</param>
        /// <returns>Résultat de la conversion</returns>
        internal static double StrToDouble(string expBrute, bool isAlone = false)
        {
            expBrute = ReplaceValeurRemarquable(expBrute).Replace(",", ".");
            double approximation;
            try
            {
                if (expBrute.Contains('/'))
                {
                    // Fraction non encore convertie
                    approximation = ParseDoubleFromString(expBrute);
                }
                else
                {
                    // Conversion directe en utilisant le séparateur de décimales approprié
                    Entity replacedExpr = expBrute;
                    var result = replacedExpr.EvalNumerical().Stringize().Replace("{ ", string.Empty).Replace(" }", string.Empty);
                    double app = double.Parse(result, CultureInfo.InvariantCulture);

                    approximation = app;
                }
            }
            catch
            {
                // Si on ne s'intéresse qu'au signe
                if (isAlone)
                {
                    // Le signe est déterminé en comptant le nombre de signes négatifs présents
                    approximation = expBrute.Count(x => x == '-') % 2 == 0 ? 1 : -1;
                }
                else
                {
                    // Si la conversion échoue et qu'on ne s'intéresse pas uniquement au signe, on lance une exception
                    throw new Exception();
                }
            }

            return approximation;
        }

        // Méthode privée pour extraire un nombre réel d'une chaîne de caractères
        private static double ParseDoubleFromString(string num)
        {
            // Supprime les espaces multiples entre les caractères, les virgules et les espaces en début et fin de chaîne
            num = Regex.Replace(num.Replace(",", ""), @"\s+", " ").Trim();
            double d = 0;
            int whole = 0;
            double numerator;
            double denominator;

            // Y a-t-il une fraction ?
            if (num.Contains("/"))
            {
                // Y a-t-il un espace ?
                if (num.Contains(" "))
                {
                    // Sépare l'entier et la fraction
                    int firstspace = num.IndexOf(" ");
                    string fraction = num.Substring(firstspace, num.Length - firstspace);
                    // Définit l'entier
                    whole = int.Parse(num.Substring(0, firstspace));
                    // Définit le numérateur et le dénominateur
                    numerator = double.Parse(fraction.Split("/".ToCharArray())[0]);
                    denominator = double.Parse(fraction.Split("/".ToCharArray())[1]);
                }
                else
                {
                    // Définit le numérateur et le dénominateur
                    numerator = double.Parse(num.Split("/".ToCharArray())[0]);
                    denominator = double.Parse(num.Split("/".ToCharArray())[1]);
                }

                // Est-ce une fraction valide ?
                if (denominator != 0)
                {
                    d = whole + numerator / denominator;
                }
            }
            else
            {
                // Conversion directe de la chaîne en nombre réel en supprimant les espaces
                d = double.Parse(num.Replace(" ", ""));
            }

            return d;
        }
    }
}
