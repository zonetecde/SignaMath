using AngouriMath;
using AngouriMath.Extensions;
using SignaMath.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SignaMath
{
    /// <summary>
    /// Logique d'interaction pour UserControl_Row.xaml
    /// </summary>
    public partial class UserControl_Row : UserControl
    {
        internal List<string> RightSideElement = new List<string>();
        public UserControl_TopRow UcTopRow { get; set; }
        public int RowId { get; private set; }
        public bool ValeurInterdite { get; }

        public UserControl_Row(UserControl_TopRow UcTopRow, int rowId, bool valeurInterdite = false)
        {
            InitializeComponent();
            this.UcTopRow = UcTopRow;
            RowId = rowId;
            ValeurInterdite = valeurInterdite;

            // Change le tableau de cellSign en fonction de la nouvelle formule entrée
            TextBox_Expression.FormulaChanged += (equation) =>
            {
                equation += " = 0";

                // si l'équation n'a pas de x, c'est un seul nombre simple qui n'a pas de solution
                Entity? solutions = null;

                if (equation.Contains(MainWindow.VariableName))
                {
                    Entity equationExpression = MathS.FromString(equation);
                    solutions = equationExpression.Solve(MainWindow.VariableName);
                }

                // S'enlève de toutes les colonnes
                UcTopRow.RightSideElements.ForEach(x => x.FromRows.Remove(this.RowId));

                // S'ajoute les colonnes nécessaire pour dresser le tableau de cellSign de cette expression
                if (solutions != null) // = pas de solution
                    foreach (var solution in solutions!.Stringize().Split(','))
                    {
                        var _solution = solution.Replace("{ ", string.Empty).Replace(" }", string.Empty);

                        string strApproximative = _solution.EvalNumerical().Stringize();

                        if (!strApproximative.Contains('i'))
                        {
                            double approximation = Extension.Extension.StrToDouble(strApproximative);

                            // si la colonne existe déjà on s'ajoute à la liste des rows concerné, sinon on ajoute la colonne
                            var column = UcTopRow.RightSideElements.FirstOrDefault(x => x.Expression == _solution);
                            if (column != default)
                            {
                                // La colonne existe déjà, on ajoute son Id
                                column.FromRows.Add(rowId);
                            }
                            else
                            {
                                // Aucune colonne existe
                                ColumnElement columnElement = new ColumnElement(_solution, approximation, 0, new List<int> { rowId });
                                UcTopRow.RightSideElements.Add(columnElement);
                            }
                        }
                        else
                        {
                            // Une solution existe, mais elle n'est pas réel
                        }
                    }

                UcTopRow.UpdateRightSideElement();
            };


            this.Loaded += (sender, e) =>
            {
                // Curseur pour écrire une formule
                TextBox_Expression.formulaControl_formatted_MouseLeftButtonDown(this, null);
            };
        }

        internal void UpdateRow()
        {
            Grid_RightSide.Children.Clear();
            Grid_RightSide.ColumnDefinitions.Clear();

            // place les colonnes de la row
            for (int i = 0; i < UcTopRow.RightSideElements.Count + 1; i++)
            {
                // Vérifie que la colonne est comprise entre les bornes
                if (i != UcTopRow.RightSideElements.Count)
                    if (!(UcTopRow.RightSideElements[i].Value > ColumnElement.IntervalleMin && UcTopRow.RightSideElements[i].Value < ColumnElement.IntervalleMax))
                    {
                        if (UcTopRow.RightSideElements[i].Value < ColumnElement.IntervalleMax)
                        {
                            // càd que la dernière colonne etait celle juste avant, donc on set le signe de la dernière colonne
                        }
                        continue;
                    }

                Grid_RightSide.ColumnDefinitions.Add(new ColumnDefinition()); // contient le cellSign

                Grid gridElement = new Grid();

                Label label_signe = new Label()
                {
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    FontSize = 24
                };

                label_signe.Cursor = Cursors.Hand;

                label_signe.PreviewMouseLeftButtonUp += (sender, e) =>
                {
                    label_signe.Content = label_signe.Content.ToString() == "+" ? '-' : '+';
                };

                gridElement.Children.Add(label_signe);

                Border container = new Border()
                {
                    BorderBrush = Brushes.Black,
                    // pour pas qu'il y a un mauvais rendu visuelle sur la dernière cellule droite il y a une condition
                    BorderThickness = i != UcTopRow.RightSideElements.Count ? new Thickness(0, 0, 2, 0) : new Thickness(0)
                };


                // Si ce n'est pas la dernière colonne
                if (i != UcTopRow.RightSideElements.Count)
                {
                    // Si l'expression de cette row s'annule au nombre de la colonne, on place un 0
                    if (UcTopRow.RightSideElements[i].FromRows.Contains(RowId))
                    {
                        // si c'est une valeur interdite pas de 0 mais double barre
                        if (ValeurInterdite)
                        {
                            UcTopRow.RightSideElements[i].ValeurInterdite = true;

                            Border secondBar = new Border()
                            {
                                BorderBrush = Brushes.Black,
                                HorizontalAlignment = HorizontalAlignment.Right,
                                Margin = new Thickness(0, 0, 5, 0),
                                BorderThickness = new Thickness(0, 0, 2, 0)
                            };

                            gridElement.Children.Add(secondBar);
                        }
                        else
                        {
                            // Ce n'est pas une valeur interdite on rajoute 0
                            Label label_0 = new Label();
                            label_0.VerticalAlignment = VerticalAlignment.Center;
                            label_0.HorizontalAlignment = HorizontalAlignment.Right;
                            label_0.Margin = new Thickness(0, 0, -12, 0);
                            label_0.FontSize = 24;
                            label_0.Content = "0";
                            gridElement.Children.Add(label_0);
                        }
                    }

                    // Enfin, place le cellSign de l'expression pour la borne donné
                    string formule = TextBox_Expression.textBox_clear.Text;
                    double variable = UcTopRow.RightSideElements[i].Value - 0.00000000001;

                    char cellSign = GetSign(formule, variable);
                    label_signe.Content = cellSign;

                    char columnSign = UcTopRow.RightSideElements[i].ColumnSign;

                    switch (columnSign)
                    {
                        case '+':
                            if (cellSign == '-')
                                UcTopRow.RightSideElements[i].ColumnSign = '-';
                            break;
                        case '-':
                            if (cellSign == '-')
                                UcTopRow.RightSideElements[i].ColumnSign = '+';
                            break;
                    }


                }
                else
                {
                    // Si c'est un nombre seul (ex : 5) (car il n'y a pas de colonne)
                    if (UcTopRow.RightSideElements.Count == 0)
                    {
                        // on convert l'expression (elle peut être une fraction, comme -3/4)
                        string expBrute = TextBox_Expression.textBox_clear.Text;
                        double approximation = Extension.Extension.StrToDouble(expBrute);

                        label_signe.Content = approximation >= 0 ? '+' : '-';
                    }
                    else
                    {
                        // si c'est la dernière colonne, on prend la variable de la colonne d'avant mais avec un + cette fois
                        string formule = TextBox_Expression.textBox_clear.Text;
                        double variable = UcTopRow.RightSideElements[i - 1].Value + 0.00000000001;

                        char cellSign = GetSign(formule, variable);
                        label_signe.Content = cellSign;

                        switch (ColumnElement.LastColumnSign)
                        {
                            case '+':
                                if (cellSign == '-')
                                    ColumnElement.LastColumnSign = '-';
                                break;
                            case '-':
                                if (cellSign == '-')
                                    ColumnElement.LastColumnSign = '+';
                                break;
                        }

                    }
                }

                container.Child = gridElement;
                Grid.SetColumn(container, i);

                Grid_RightSide.Children.Add(container);
            }

        }

        private static char GetSign(string formule, double variable)
        {
            Entity expr = formule.ToEntity();

            // Remplace x par -5.3
            Entity replacedExpr = expr.Substitute(MainWindow.VariableName, variable);
            // Évalue l'expression
            var result = replacedExpr.EvalNumerical().Stringize().Replace("{ ", string.Empty).Replace(" }", string.Empty);

            double approximation = Extension.Extension.StrToDouble(result);

            // Obtiens le cellSign du résultat
            char sign = approximation >= 0 ? '+' : '-';
            return sign;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // S'enlève de toutes les colonnes
            UcTopRow.RightSideElements.ForEach(x => x.FromRows.Remove(this.RowId));

            MainWindow._MainWindow.StackPanel_Row.Children.Remove(this);

            UcTopRow.UpdateRightSideElement();
        }

        private void Border_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBox_Expression.formulaControl_formatted_MouseLeftButtonDown(this, null);
        }
    }
}
