using AngouriMath;
using AngouriMath.Extensions;
using SignaMath.Classes;
using SignaMath.UC;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static AngouriMath.Entity;

namespace SignaMath
{
    /// <summary>
    /// Logique d'interaction pour UserControl_Row.xaml
    /// </summary>
    public partial class UserControl_Row : UserControl
    {
        internal UserControl_FormulaTextBox UC_borneMin;
        internal UserControl_FormulaTextBox UC_borneMax;
        public RowType RowType { get; }
        public int RowId { get; private set; }

        private static bool IsLooping = false;

        public UserControl_Row(RowType rowType, int rowId)
        {
            InitializeComponent();

            // Défini le type de la ligne
            RowType = rowType;
            // Id de la ligne
            RowId = rowId;

            // Lorsque l'utilisateur écris une nouvelle formule, trigger l'évènement FormulaChanged.
            // Si la row est une ligne concluante, on ne vérifie pas ce que l'utilisateur a marqué
            // donc il n'y a aucun event à trigger
            if (rowType != RowType.CONCLUANTE && rowType != RowType.TABLEAU_DE_VARIATION)
                TextBox_Expression.FormulaChanged += FormulaChanged;
            else
            {
                // Comme c'est une ligne concluante, on laisse l'utilisateur écrire ce qu'il veut comme nom de fonction
                // sans vérifier
                TextBox_Expression.DirectWriting = true;
                if (rowType == RowType.CONCLUANTE)
                {
                    TextBox_Expression.formulaControl_formatted.Formula = "f'(" + GlobalVariable.VariableName + ")";
                    TextBox_Expression.textBox_clear.Text = "f'(" + GlobalVariable.VariableName + ")";
                }
                else if (rowType == RowType.TABLEAU_DE_VARIATION)
                {
                    TextBox_Expression.formulaControl_formatted.Formula = "f(" + GlobalVariable.VariableName + ")";
                    TextBox_Expression.textBox_clear.Text = "f(" + GlobalVariable.VariableName + ")";

                    MenuItem_tableauDeVariation.IsEnabled = true;
                    if (GlobalVariable.TableauDeVariationFormule == null)
                    {
                        Loaded += (s, e) =>
                        {
                            // demande de saisir une formule pour le tableau de variation
                            MenuItem_TableauDeVariation_Click(this, null);
                        };
                    }
                    else
                    {
                        this.ToolTip = GlobalVariable.TableauDeVariationFormule;
                    }
                }
            }

            // Si c'est une row dans laquelle il faut marqué une expression,
            // met le Focus sur la textBox_Expression pour que l'utilisateur écrit une formule
            if ((rowType == RowType.MIDDLE || rowType == RowType.MIDDLE_INTERDITE) && GlobalVariable.AllowFocusWhenAdded)
            {
                this.Loaded += (sender, e) =>
                {
                    TextBox_Expression.formulaControl_formatted_MouseLeftButtonUp(this, null);
                };
            }

            // Si c'est la première row du tableau, alors on ajoute les 2 intervalles
            // et on set le nom de la variable
            if (RowType == RowType.HEADER)
            {
                // Set le nom de la variable est la limite à un de longueur max (= 1 caractère)
                TextBox_Expression.textBox_clear.Text = Char.ToString(GlobalVariable.VariableName);
                TextBox_Expression.textBox_clear.MaxLength = 1;
                MenuItem_delete.IsEnabled = false;

                // Créé les bornes
                UC_borneMin = new UserControl_FormulaTextBox();
                UC_borneMin.HorizontalAlignment = HorizontalAlignment.Left;
                UC_borneMin.Margin = new Thickness(20, 0, 0, 0);
                UC_borneMin.textBox_clear.Text = "-\\infty";
                UC_borneMin.DirectWriting = true;
                UC_borneMin.FormulaChanged += (newIntervalMin,c) => 
                {
                    double writedInterval = newIntervalMin.Replace(" ", string.Empty) == "-\\infty" ? double.MinValue : Extension.Extension.StrToDouble(newIntervalMin);
                    if (GlobalVariable.IntervalleMax >= writedInterval) 
                        GlobalVariable.IntervalleMin = writedInterval; 
                    else throw new Exception(); // intervalle min plus grand que intervalle max
                    GlobalVariable.UpdateColumn(); 
                };

                UC_borneMax = new();
                UC_borneMax.HorizontalAlignment = HorizontalAlignment.Right;
                UC_borneMax.Margin = new Thickness(0, 0, 20, 0);
                UC_borneMax.textBox_clear.Text = "+\\infty";
                UC_borneMax.DirectWriting = true;
                UC_borneMax.FormulaChanged += (newIntervalMax,c) => { 
                    double writedInterval = newIntervalMax.Replace(" ", string.Empty) == "+\\infty" ? double.MaxValue : Extension.Extension.StrToDouble(newIntervalMax);
                    if (GlobalVariable.IntervalleMin <= writedInterval)
                        GlobalVariable.IntervalleMax = writedInterval;
                    else throw new Exception(); // intervalle max plus petit que intervalle min
                    GlobalVariable.UpdateColumn(); 
                };

                Grid_RowContainer.Children.Add(UC_borneMin);
                Grid_RowContainer.Children.Add(UC_borneMax);
            }

            // Update tout le tableau si c'est une ligne concluante ou un tableau de variation pour mettre à jour ses valeurs
            if (RowType == RowType.CONCLUANTE || RowType == RowType.TABLEAU_DE_VARIATION)
                this.Loaded += (s, e) =>
                {
                    GlobalVariable.UpdateBoard();
                };
        }

        /// <summary>
        /// La TextBox gauche d'une ligne du tableau possède une nouvelle formule mathématique
        /// Change en conséquence le tableau
        /// </summary>
        /// <param name="newFormula">La nouvelle formule de l'utilisateur</param>
        internal void FormulaChanged(string newFormula, bool callUpdateBoard = true)
        {
            switch (RowType)
            {
                // Si c'est la première ligne du tableau, dans ce cas
                // c'est le nom de la variable qui a changé
                case RowType.HEADER:
                    // Vérifie que le nouveau nom de variable est tout sauf un chiffre
                    // et que la variable est composé que d'un caractère
                    // Interdit 'e' en tant que variable car 'e' est l'exponentielle 
                    if (char.IsDigit(Convert.ToChar(newFormula)) || newFormula.Length != 1 || newFormula == "e")
                        // Va donner une indications visuelle à l'utilisateur que 
                        // le nom de la variable écrite n'est pas correct.
                        throw new Exception();

                    char oldLetter = GlobalVariable.VariableName;
                    GlobalVariable.VariableName = Convert.ToChar(newFormula);

                    // Le nom de la variable est correcte, change donc toutes les formules
                    // des rows avec le nouveau nom de variable
                    MainWindow.TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().FindAll(x => x.RowType != RowType.HEADER).ForEach(uc =>
                    {
                        uc.TextBox_Expression.textBox_clear.Text = uc.TextBox_Expression.textBox_clear.Text.Replace(oldLetter, Convert.ToChar(newFormula));
                    });

                    // Change dans les paramètres globaux le nom de la variable par le nouveau

                    break;

                // Si c'est une ligne contenant une expression mathématique qui contribue
                // à l'étude du signe
                case RowType.MIDDLE:
                case RowType.MIDDLE_INTERDITE:
                    // Remplace les valeurs remarquables :
                    // Exponentielle :
                    newFormula = Extension.Extension.ReplaceValeurRemarquable(newFormula);

                    // On va recalculer avec la nouvelle formule ses solutions, donc
                    // on enlève les colonnes du tableau des anciennes solutions trouvées.
                    GlobalVariable.TableColumns.ForEach(x => x.FromRows.Remove(this.RowId));

                    // On transforme la formule donné par l'utilisateur en équation pour trouver
                    // les endroits où la courbe coupe la ligne à l'ordonné Y
                    string equation = newFormula + " = " + GlobalVariable.Y;

                    // Si l'équation n'a pas de x, c'est un seul nombre simple qui n'a pas de solution
                    // Dans ce cas, on skip toute la partie qui permet de trouver et d'ajouter les 
                    // solutions aux colonnes du tableau.
                    if (!equation.Contains(GlobalVariable.VariableName))
                        break;

                    // Trouve les résultats de l'équation en fonction de la variable à trouver
                    var solutions = GetAllEquationSolution(equation);

                    foreach (string solution in solutions)
                    {
                        // Calcul une approximation de la solution (dans le cas où elle contient un nombre aux décimals infinis)
                        double approximation = Extension.Extension.StrToDouble(solution.EvalNumerical().Stringize());

                        // On regarde si une colonne dans le tableau existe déjà contenant cette solution
                        var column = GlobalVariable.TableColumns.FirstOrDefault(x => x.Expression == solution);

                        // Si la colonne existe déjà, on ajoute cette row à la liste de la colonne 
                        // contenant toutes les rows pour laquelle elle est une solution
                        if (column != default)
                            column.FromRows.Add(RowId);

                        else
                        {
                            // Sinon, on ajoute une nouvelle colonne au tableau
                            // avec comme row concerné cette row-ci.
                            ColumnElement columnElement = new ColumnElement(solution, approximation, new List<int> { RowId });
                            GlobalVariable.TableColumns.Add(columnElement);
                        }
                    }
                           

                    break;
            }

            // Met à jour l'entiereté du tableau
            if(callUpdateBoard)
                GlobalVariable.UpdateBoard();
        }

        /// <summary>
        /// Dresse les colonnes de cette ligne et calcul son signe
        /// </summary>
        internal void UpdateRow()
        {
            // Commence par supprimer les anciennes colonnes placées
            Grid_RowColumns.Children.Clear();
            Grid_RowColumns.ColumnDefinitions.Clear();

            // Enlève toutes les colonnes qui ne sont pas comprise dans l'interval
            var tableColumns = new List<ColumnElement>(GlobalVariable.TableColumns);
            tableColumns.RemoveAll(x => x.Value <= GlobalVariable.IntervalleMin || x.Value >= GlobalVariable.IntervalleMax);

            // Ajoute, pour chaque colonne du tableau, une colonne dans la ligne
            // Le `i` va de 0 à `nombre de colonne + 1` car sinon il manquerait la dernière case dans la row
            for (int i = 0; i < tableColumns.Count + 1; i++)
            {
                // Ajoute la colonne
                Grid_RowColumns.ColumnDefinitions.Add(new ColumnDefinition());

                // Si c'est la ligne header
                if (RowType == RowType.HEADER)
                {
                    // Si ce n'est pas la dernière colonne
                    if (i != tableColumns.Count)
                    {
                        // Création du texte contenant la solution (le 'nombre' de la colonne)
                        UserControl_FormulaTextBox uc = new UserControl_FormulaTextBox()
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Right,
                        };

                        // Set le 'nombre' de la colonne
                        uc.textBox_clear.Text = tableColumns[i].Expression;
                        uc.formulaControl_formatted.ToolTip = tableColumns[i].Value;

                        // Permet de centrer le texte avec les colonnes
                        uc.Loaded += (sender, e) =>
                        {
                            uc.Margin = new Thickness(0, 0, (uc.ActualWidth / 2) * -1, 0);

                            // Regarde si le nombre de la colonne est trop grand pour la taille de la colonne
                            // Si oui, alors redimensionne la ligne
                            if(uc.formulaControl_formatted.ActualHeight > this.ActualHeight - 20)
                            {
                                this.Height = uc.formulaControl_formatted.ActualHeight + 15;
                            }
                        };

                        // Ajoute le texte à la colonne
                        Grid.SetColumn(uc, i);

                        // Ajoute le nombre à la row header
                        Grid_RowColumns.Children.Add(uc);
                    }
                    else
                    {
                        // La dernière colonne n'est là que pour qu'il n'y ai pas de décallage
                    }

                    continue;
                }

                // Ajoute le signe de la colonne à cette row si c'est une ligne de signe
                UserControl_CellSign userControl_CellSign = new UserControl_CellSign(i == tableColumns.Count, RowType);
                Grid.SetColumn(userControl_CellSign, i);
                Grid_RowColumns.Children.Add(userControl_CellSign);

                // Si c'est une ligne concluante
                if (RowType == RowType.CONCLUANTE)
                {
                    if (i != tableColumns.Count)
                    {
                        // Ajoute une seconde barre si la colonne contient une valeur interdite
                        if (tableColumns[i].ValeurInterdite)
                        {
                            userControl_CellSign.Border_SecondeBarre.Visibility = Visibility.Visible;
                        }
                        // Sinon c'est que la colonne est une colonne solution d'une expression du tableau
                        // donc qui se coupe en 0 au moins une fois
                        else
                        {
                            userControl_CellSign.Label_Zero.Visibility = Visibility.Visible;
                        }

                        // Set le signe de la colonne
                        userControl_CellSign.Label_Sign.Content = tableColumns[i].ColumnSign;
                    }
                    else
                    {
                        // Signe de la dernière colonne
                        userControl_CellSign.Label_Sign.Content = GlobalVariable.LastColumnSign;
                    }

                    continue;
                }

                // Si c'est un tableau de variation
                if (RowType == RowType.TABLEAU_DE_VARIATION)
                {
                    // Récupère le signe de la ligne concluante pour pouvoir orienter la flèche
                    bool isPlus = (((UserControl_CellSign)MainWindow.TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>()
                        .First(x => x.RowType == RowType.CONCLUANTE).Grid_RowColumns.Children[i]))
                            .Label_Sign.Content.ToString() == "+";

                    // Set l'image de la flèche
                    userControl_CellSign.Image_Arrow.Source = isPlus ? MainWindow._MainWindow.img_arrowTemplateTop.Source : MainWindow._MainWindow.img_arrowTemplateBottom.Source;
                    userControl_CellSign.Image_Arrow.Visibility = Visibility.Visible;

                    // Set les valeurs modifiables aux extrémités des flèches
                    userControl_CellSign.FormulaTextBoxRight.Visibility = Visibility.Visible;
                    userControl_CellSign.FormulaTextBoxRight.VerticalAlignment = isPlus ? VerticalAlignment.Top : VerticalAlignment.Bottom;
                    userControl_CellSign.FormulaTextBoxRight.Margin = isPlus ? new Thickness(0, 0, -20, 0) : new Thickness(0, 10.5, -25, 5);

                    // Écris la valeur si une fonction est entrée
                    try
                    {
                        if (i != tableColumns.Count)
                        {
                            if (!tableColumns[i].ValeurInterdite && !String.IsNullOrEmpty(GlobalVariable.TableauDeVariationFormule)) // !valeur interdite
                            {
                                userControl_CellSign.FormulaTextBoxRight.textBox_clear.Text = ReplaceVariableAndCalculate(GlobalVariable.TableauDeVariationFormule, tableColumns[i].Expression);
                            }
                        }
                        else
                        {
                            if (GlobalVariable.IntervalleMax != double.MaxValue && !String.IsNullOrEmpty(GlobalVariable.TableauDeVariationFormule)) // !infini 
                            {
                                userControl_CellSign.FormulaTextBoxRight.textBox_clear.Text = ReplaceVariableAndCalculate(GlobalVariable.TableauDeVariationFormule, GlobalVariable.IntervalleMax.ToString(CultureInfo.InvariantCulture));
                            }
                        }
                    }
                    catch
                    {
                        // erreur 
                    }

                    // Ajoute la valeur à gauche de la flèche de la première cellule
                    if (i == 0)
                    {
                        userControl_CellSign.FormulaTextBoxLeft.Visibility = Visibility.Visible;
                        userControl_CellSign.FormulaTextBoxLeft.VerticalAlignment = isPlus ? VerticalAlignment.Bottom : VerticalAlignment.Top;
                        userControl_CellSign.FormulaTextBoxLeft.Margin = isPlus ? new Thickness(0, 10.5, -20, 5) : new Thickness(0, 0, -20, 0);

                        if (GlobalVariable.IntervalleMin != double.MinValue && !String.IsNullOrEmpty(GlobalVariable.TableauDeVariationFormule)) // !infini 
                        {
                            userControl_CellSign.FormulaTextBoxLeft.textBox_clear.Text = ReplaceVariableAndCalculate(GlobalVariable.TableauDeVariationFormule, GlobalVariable.IntervalleMin.ToString(CultureInfo.InvariantCulture));
                        }
                    }

                    // Si ce n'est pas la dernière colonne
                    if (i != tableColumns.Count)
                    {
                        // si c'est une valeur interdite alors double barre
                        if (tableColumns[i].ValeurInterdite)
                        {
                            userControl_CellSign.Border_SecondeBarre.Visibility = Visibility.Visible;
                            // Pour pas que le texte rentre dans la double barre
                            userControl_CellSign.FormulaTextBoxRight.Margin = isPlus ? new Thickness(0, 0, 0, 0) : new Thickness(0, 10.5, 0, 5);
                        }
                        else
                        {
                            // Sinon on met pas du tout de barre
                            userControl_CellSign.Border_Main.BorderThickness = new Thickness(0, 0, 0, 0);
                        }
                    }
                    else
                    {
                        // Dernière colonne ou double barre, on change la margin pour pas que le label sorte du tableau
                        userControl_CellSign.FormulaTextBoxRight.Margin = isPlus ? new Thickness(0, 0, 0, 0) : new Thickness(0, 10.5, 0, 5);
                    }

                    continue;
                }

                // Si c'est une row d'expression
                if (RowType == RowType.MIDDLE || RowType == RowType.MIDDLE_INTERDITE)
                {
                    // Si l'expression contient du sqrt() avec du x à l'intérieur, on calcul et trouve l'intervalle
                    // de définition de la fonction et l'impose
                    if (TextBox_Expression.textBox_clear.Text.Contains("sqrt("))
                    {
                        // récupère toutes les racines carrés (ex : sqrt(5x+4) -> 5x+4)
                        string pattern = @"sqrt\((.*?)\)";
                        MatchCollection matches = Regex.Matches(TextBox_Expression.textBox_clear.Text, pattern);

                        if (matches.Count > 0)
                        {
                            List<string> solutions = new();

                            foreach (Match match in matches)
                            {
                                string value = match.Groups[1].Value;

                                // regarde s'il y a du x dans la racine
                                // Sinon la racine est constante
                                if (value.Contains(GlobalVariable.VariableName))
                                {
                                    // dans ce cas calcul l'équation de 'value = 0' pour savoir quand
                                    // commence la fonction, car racine carré ne prend pas de nombre négatif
                                    solutions.AddRange( GetAllEquationSolution(value + " = 0"));
                                }
                                else
                                {
                                    // Vérifie que le nombre dans la racine est positif
                                    Entity expression = value.ToEntity();
                                    Entity result = expression.EvalNumerical();

                                    if (Extension.Extension.StrToDouble(result.EvalNumerical().Stringize()) < 0)
                                        throw new Exception();
                                    
                                }
                            }

                            // prend la solution la plus grande
                            if (solutions.Any())
                            {
                                var max_solution = solutions.Max(x => Extension.Extension.StrToDouble(x.EvalNumerical().Stringize()));
                                var solution = solutions.First(x => Extension.Extension.StrToDouble(x.EvalNumerical().Stringize()) == max_solution);

                                double approximation = Extension.Extension.StrToDouble(solution.EvalNumerical().Stringize());

                                // change l'intervalle min
                                if (GlobalVariable.IntervalleMin < approximation && !IsLooping)
                                {
                                    IsLooping = true;
                                    var headerRow = (MainWindow.TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>()
                                        .First(x => x.RowType == RowType.HEADER));
                                    headerRow.UC_borneMin.formulaControl_formatted.Formula = approximation.ToString();
                                    GlobalVariable.IntervalleMin = approximation;
                                    tableColumns.RemoveAll(x => x.Value <= GlobalVariable.IntervalleMin || x.Value >= GlobalVariable.IntervalleMax);
                                    IsLooping = false;
                                }
                            }
                        }
                    }

                    // Si l'expression n'a aucune solution c'est quelle est toujours pos ou toujours neg
                    if (!tableColumns.Any(x => x.FromRows.Contains(RowId)) && this.RowType != RowType.CONCLUANTE)
                    {
                        // Soit c'est une expression qui contient du x :
                        if (TextBox_Expression.textBox_clear.Text.Contains(GlobalVariable.VariableName))
                        {
                            // Dans ce cas on remplace x par n'importe quel nombre; elle sera toujours du même signe
                            string formule = TextBox_Expression.textBox_clear.Text;
                            char cellSign = GetSign(formule, GlobalVariable.IntervalleMin);
                            userControl_CellSign.Label_Sign.Content = cellSign;
                        }

                        // Soit c'est une expression seul comme -3/4 :
                        else
                        {
                            // Dans ce cas on convert l'expression en nombre
                            string expBrute = TextBox_Expression.textBox_clear.Text;
                            // StrToDouble va renvoyer soit 1 soit -1 en fonction du signe final de l'expression
                            double approximation = Extension.Extension.StrToDouble(expBrute, true);
                            userControl_CellSign.Label_Sign.Content = approximation >= 0 ? '+' : '-';

                            // Dans ce cas on change le signe de toutes les colonnes existantes
                            // Car elles seront tous affecté par le même signe
                            foreach(var colonne in GlobalVariable.TableColumns)
                            {
                                colonne.ColumnSign = RegleDesSignes(colonne.ColumnSign, approximation >= 0 ? '+' : '-');
                            }

                            // Si il n'y a aucune colonne, alors on change le signe de la dernière colonne
                            // car c'est la dernière colonne
                            if(!GlobalVariable.TableColumns.Any())
                            {
                                GlobalVariable.LastColumnSign = RegleDesSignes(GlobalVariable.LastColumnSign, approximation >= 0 ? '+' : '-');
                            }
                        }

                        continue;
                    }

                    // Si ce n'est pas la dernière colonne
                    if (i != tableColumns.Count)
                    {
                        // Si l'expression de cette row s'annulle au numéro de la colonne, on place un 0
                        // (en d'autre terme, si c'est une solution)
                        if (tableColumns[i].FromRows.Contains(RowId))
                        {
                            // Si c'est une ligne de valeur interdite, on ne met pas de `0` mais une double barre
                            if (RowType == RowType.MIDDLE_INTERDITE)
                            {
                                // Set la colonne entière en tant que valeur interdite (pour le tableau de variation)
                                tableColumns[i].ValeurInterdite = true;

                                // Affiche la seconde barre
                                userControl_CellSign.Border_SecondeBarre.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                // Ce n'est pas une valeur interdite, on affiche le '0'
                                userControl_CellSign.Label_Zero.Visibility = Visibility.Visible;
                            }
                        }

                        // Enfin, place le signe de l'expression entre la colonne d'avant et la colonne actuelle
                        // càd, la colonne actuelle - un nombre très petit
                        // On récupère donc la formule de l'expression de cette row
                        string formule = Extension.Extension.ReplaceValeurRemarquable(TextBox_Expression.textBox_clear.Text);
                        // On set la valeur qui va remplacer 'x' par la valeur de la colonne - un nombre très petit
                        double variable = tableColumns[i].Value - 0.00000000001;
                        // Ainsi, on récupère le signe du résultat
                        char cellSign = GetSign(formule, variable);
                        userControl_CellSign.Label_Sign.Content = cellSign;

                        // Met à jour le signe de la colonne entière pour la ligne concluante
                        // en usant de la règle des signes
                        tableColumns[i].ColumnSign = RegleDesSignes(tableColumns[i].ColumnSign, cellSign);

                    }
                    // Si c'est la dernière colonne (= après on a la borne max)
                    else
                    {
                        // si c'est la dernière colonne, on prend la variable de la colonne d'avant mais avec un + cette fois
                        // pour être entre la dernière colonne et (;) borne max
                        // TODO: ajouter la borne max à une colonne à part entière
                        string formule = Extension.Extension.ReplaceValeurRemarquable(TextBox_Expression.textBox_clear.Text);
                        double variable = tableColumns[i - 1].Value + 0.00000000001;
                        char cellSign = GetSign(formule, variable);
                        userControl_CellSign.Label_Sign.Content = cellSign;

                        GlobalVariable.LastColumnSign = RegleDesSignes(GlobalVariable.LastColumnSign, cellSign);
                    }
                }
            }

        }

        /// <summary>
        /// Remplace dans la formule donnée la variable par le nombre donné est calcul le résultat
        /// </summary>
        /// <param name="expression">L'expression avec x</param>
        /// <param name="expression">Le nombre qui va remplacer la variable</param>
        /// <returns>L'expression calculée</returns>
        private string ReplaceVariableAndCalculate(string expression, string x)
        {
            Entity expr = expression.ToEntity();
            Entity replacedExpr = expr.Substitute(Char.ToString(GlobalVariable.VariableName), x.ToEntity());
            Number result = replacedExpr.Simplify().EvalNumerical();
            return result.ToString();
        }

        /// <summary>
        /// Effectue la règle des signes
        /// </summary>
        /// <param name="premier">Le premier signe</param>
        /// <param name="second">Le second string</param>
        /// <returns>Le signe résultant</returns>
        private static char RegleDesSignes(char premier, char second)
        {
            switch (premier)
            {
                case '+':
                    if (second == '-') // + et - = -
                        return '-';
                    break;
                case '-':
                    if (second == '-') // - et - = +
                        return '+';
                    break;
            }

            return premier;
        }

        /// <summary>
        /// Retourne une liste de toutes les solutions réelles d'une équation
        /// </summary>
        /// <param name="equation">L'équation</param>
        /// <returns>Liste de toutes les solutions</returns>
        private List<string> GetAllEquationSolution(string equation)
        {
            List<string> solutions_str = new List<string>();

            Entity equationExpression = MathS.FromString(equation);
            Entity solutions = equationExpression.Solve(Char.ToString(GlobalVariable.VariableName));
            foreach (var solution in solutions!.Stringize().Replace("{ ", string.Empty).Replace(" }", string.Empty).Split(','))
            {
                if (!String.IsNullOrEmpty(solution))
                {
                    string strApproximative = solution.EvalNumerical().Stringize();

                    // Si la solution est une solution parmis les réels (= pas de variable 'i')
                    if (!strApproximative.Contains('i'))
                    {
                        solutions_str.Add(solution);
                    }
                }
            }

            return solutions_str;
        }


        /// <summary>
        /// Donné une expression avec `x` et une variable, retourne le signe du résultat
        /// </summary>
        /// <param name="formule">La formule où il faut rempalcer `x`</param>
        /// <param name="variable">La variable qui va remplacer `x` dans la formule</param>
        /// <returns>Le signe du résultat : '+' ou '-'</returns>
        private char GetSign(string formule, double variable)
        {
            char sign = 'Ø';

            try
            {
                // Obtiens le signe du résultat
                Entity expr = formule.ToEntity();

                // Remplace la VariableName par la variable
                Entity replacedExpr = expr.Substitute(Char.ToString(GlobalVariable.VariableName), variable);
                var result = replacedExpr.EvalNumerical().Stringize().Replace("{ ", string.Empty).Replace(" }", string.Empty);
                double app = Extension.Extension.StrToDouble(result);
                sign = app >= GlobalVariable.Y ? '+' : '-'; ;
            }
            catch
            {
                //TextBox_Expression.formulaControl_formatted_MouseLeftButtonUp(this,null);
            }

            return sign;
        }

        /// <summary>
        /// Supprime la ligne du tableau
        /// </summary>
        private void MenuItem_DeleteRow_Click(object sender, RoutedEventArgs? e)
        {
            // S'enlève de toutes les colonnes
            if (RowType == RowType.CONCLUANTE)
            {
                MainWindow._MainWindow.Button_AjoutLigneConcluante_Click(this, null);
            }
            else
            {
                GlobalVariable.TableColumns.ForEach(x => x.FromRows.Remove(this.RowId));

                MainWindow._MainWindow.UC_TableauDeSigne.StackPanel_Row.Children.Remove(this);

                GlobalVariable.UpdateBoard();
            }
        }        
        
        /// <summary>
        /// Demande de saisir la formule du tableau de variation
        /// </summary>
        private void MenuItem_TableauDeVariation_Click(object sender, RoutedEventArgs? e)
        {
            var result = Microsoft.VisualBasic.Interaction.InputBox("Entrez la formule pour calculer les valeurs du tableau de variation \n\nUtilisez la même variable que celle dans le tableau de signe, par défaut `x`", "Formule", "");
            
            if(!String.IsNullOrEmpty(result))
            {
                if(result.Contains(GlobalVariable.VariableName))
                {
                    // teste la formule
                    try
                    {
                        ReplaceVariableAndCalculate(result, "0");

                        // test passé sans erreur
                        GlobalVariable.TableauDeVariationFormule = result;
                        this.ToolTip = GlobalVariable.TableauDeVariationFormule;
                        this.UpdateRow();
                    }
                    catch
                    {
                        MessageBox.Show("Échec : vérifiez la formule.", "Échec");
                    }
                }
                else
                {
                    MessageBox.Show("Échec : aucune variable entrée.");
                }
            }
            else
            {
                GlobalVariable.TableauDeVariationFormule = string.Empty;
            }
        }

        /// <summary>
        /// Met le focus sur la zone de texte
        /// </summary>
        private void Border_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBox_Expression.formulaControl_formatted_MouseLeftButtonUp(this, null);
        }

        /// <summary>
        /// Affiche bouton 'supprimer' si c'est une ligne supprimable
        /// </summary>
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (RowType != RowType.HEADER)
                button_Supprimer.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Cache bouton 'supprimer'
        /// </summary>
        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            // cache bouton 'supprimer'
            button_Supprimer.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Supprime la ligne du tableau
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // supprime la row
            MenuItem_DeleteRow_Click(this, null);
        }
    }
}
