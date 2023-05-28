using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaMath.Classes
{
    internal static class GlobalVariable
    {
        // Défini la ligne d'ordonné où l'on veut voir si la courbe la coupe
        // Par défaut : 0 -> Permet d'étudier le signe
        internal static int Y { get; set; } = 0;

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

        // Liste des signes dans l'ordre de la row concluante
        internal static List<char> RowConcluanteSigns { get; set; } = new();

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
            RowConcluanteSigns = new();

            // Met à jour toutes les rows du tableau de signe
            MainWindow.TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().ForEach(x => x.UpdateRow());
        }
    }
}
