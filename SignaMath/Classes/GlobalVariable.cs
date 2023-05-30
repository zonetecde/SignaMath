using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace SignaMath.Classes
{
    internal static class GlobalVariable
    {
        // Défini si la row ajouté doit être focus
        // Utilisé lorsque l'utilisateur écris une fonction entière
        internal static bool AllowFocusWhenAdded { get; set; } = true;

        // Défini la ligne d'ordonné où l'on veut voir si la courbe la coupe
        // Par défaut : 0 -> Permet d'étudier le signe
        internal static double Y { get; set; } = 0;

        // Défini le nom de la variable dans l'équation. Elle peut être qu'un 
        // caractère non-numérique.
        internal static char VariableName { get; set; } = 'x';

        // Contient toutes les colonnes-solutions du tableau de signe
        internal static List<ColumnElement> TableColumns { get; set; } = new List<ColumnElement>();

        // Contient le signe de la dernière colonne du tableau;
        // comme est n'est pas dans TableColumns il faut la sauvegardé autre part
        internal static char LastColumnSign { get; set; } = '+';

        // Les intervalles du tableau
        // double.MinValue = -\infty et double.MaxValue = +\infty
        internal static double IntervalleMin { get; set; } = double.MinValue;
        internal static double IntervalleMax { get; set; } = double.MaxValue;

        // Formule qui va être utilisé pour le tableau de variation
        internal static string? TableauDeVariationFormule { get; set; } = null;


        /// <summary>
        /// Met à jour l'entiereté du tableau de signe en plaçant les
        /// signes et en dressant le tableau de variation
        /// </summary>
        internal static void UpdateBoard()
        {
            TableColumns = TableColumns.OrderBy(x => x.Value).ToList(); // Trie les solutions dans l'ordre croissant
            TableColumns = TableColumns.DistinctBy(x => x.Expression).ToList(); // Enlève les doublons
            TableColumns.RemoveAll(x => x.FromRows.Count == 0); // Enlève les solutions utilisés dans aucune ligne

            // On reset le signe de toutes les colonnes (car elles vont être recalculer)
            TableColumns.ForEach(x => x.ColumnSign = '+');
            LastColumnSign= '+';

            // Met à jour toutes les rows du tableau de signe
            MainWindow.TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().ForEach(x => x.UpdateRow());
        }

        /// <summary>
        /// Met à jour l'entiereté des calculs du tableau de signe
        /// </summary>
        internal static void UpdateColumn()
        {
            MainWindow.TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().ForEach(x => {
                x.FormulaChanged(x.TextBox_Expression.textBox_clear.Text, false);
            });

            UpdateBoard();
        }
    }
}
