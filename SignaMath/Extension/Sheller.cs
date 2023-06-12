using SignaMath.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SignaMath.Extension
{
    internal static class Sheller
    {
        /// <summary>
        /// Sépare une fonction mathématique donnée en toutes ses expressions
        /// à placer dans chaque ligne du tableau de signe
        /// </summary>
        /// <param name="function">La nouvelle fonction</param>
        internal static List<ExpressionElement> ShellFunction(string function)
        {
            // On enlève les espaces inutiles et les signes en trop
            function = function.Replace(" ", string.Empty).Replace(")*(", ")(");

            List<ExpressionElement> lignes = DecouperExpression(function);

            List<ExpressionElement> expressions = new();
            if (lignes.Any())
            {
                for (int i = 0; i < lignes.Count; i++)
                {
                    if (String.IsNullOrEmpty(lignes[i].Exposant))
                    {
                        string expression = lignes[i].Expression;

                        expression = SupprimerParentheses(expression);

                        List<ExpressionElement> nouvellesExpressions = new List<ExpressionElement>();

                        var decoupe = DecouperExpression(expression);
                        nouvellesExpressions.AddRange(decoupe);

                        nouvellesExpressions.ForEach(ligne =>
                        {
                            expressions.Add(new ExpressionElement(lignes[i].Expression.Contains('/'), ligne.Expression.Replace("/", string.Empty).Replace("*", string.Empty)));
                        });
                    }
                    else
                    {
                        // s'il y a un exposant on ne shell pas l'expression; on garde l'expression
                        // total avec l'exposant
                        expressions.Add(lignes[i]);
                    }
                }
            }

            for (int i = 0; i < expressions.Count; i++)
            {
                string? expression = expressions[i].Expression.Remove(0, 1);
                var decoupe = DecouperExpression(expression, true);
                if (decoupe.Count > 1)
                {
                    expressions.RemoveAt(i);
                    expressions.AddRange(decoupe.ConvertAll(x => new ExpressionElement(expressions[i].Interdite, x.Expression)));
                    i = 0;
                }
            }

            expressions.RemoveAll(x => x.Expression.Length == 1);

            return expressions;
        }

        private static string SupprimerParentheses(string expression)
        {
            if (expression.StartsWith("/(") || expression.StartsWith("*("))
            {
                expression = expression.Replace("/(", string.Empty).Replace("*(", string.Empty);
                expression = expression.Remove(expression.Length - 1);
            }
            else if (expression.StartsWith('('))
            {
                expression = expression.Remove(0, 1);
                expression = expression.Remove(expression.Length - 1);
            }

            return expression;
        }

        private static List<ExpressionElement> DecouperExpression(string nouvelleFonction, bool estDeuxieme = false)
        {
            if (estDeuxieme)
            {
                nouvelleFonction = SupprimerParentheses(nouvelleFonction);
            }

            List<ExpressionElement> lignes = new List<ExpressionElement>();
            int compteurParenthesesOuvertes = 0;
            int indexDebutLigne = 0;

            for (int i = 0; i < nouvelleFonction.Length; i++)
            {
                if (nouvelleFonction[i] == '(')
                {
                    compteurParenthesesOuvertes++;
                }
                else if (nouvelleFonction[i] == ')')
                {
                    compteurParenthesesOuvertes--;

                    if (compteurParenthesesOuvertes == 0 || i == nouvelleFonction.Length-1)
                    {
                        string exposant = string.Empty;

                        if (nouvelleFonction.Length > i + 1)
                            if (nouvelleFonction[i + 1] == '^')
                            {
                                // il y a un exposant à l'expression, on la garde en entière
                                // si l'exposant est 1 chiffre (ex : ^2)
                                if (nouvelleFonction[i + 2] != '(')
                                {
                                    exposant += nouvelleFonction[i + 2];
                                }
                                else
                                {
                                    // exposant entre les parenthèses
                                    bool endExp = false;
                                    int z = i + 3; // index dans la parenthèse
                                    int tempOuverte = 0;
                                    while (!endExp)
                                    {
                                        exposant += nouvelleFonction[z];
                                        if (nouvelleFonction[z + 1] == '(')
                                            tempOuverte++;
                                        else if (nouvelleFonction[z + 1] == ')' && tempOuverte > 0)
                                            tempOuverte--;
                                        else if (nouvelleFonction[z + 1] == ')')                                        
                                            // fin de l'exposant
                                            endExp = true;
                                        
                                        
                                        z++;
                                    }
                                }
                                
                            }

                        ExpressionElement exp = new ExpressionElement(true, nouvelleFonction.Substring(indexDebutLigne, i - indexDebutLigne + 1), exposant);
                        
                        // enlève l'exposant de la formule s'il y en a une
                        if (!String.IsNullOrEmpty(exp.Exposant))
                        {
                            i -= ("^" + exp.Exposant).Length;
                            nouvelleFonction = nouvelleFonction.Replace("^" + exp.Exposant, string.Empty);
                        }

                        lignes.Add(exp);
                        indexDebutLigne = i + 1;
                    }
                }
                else if (nouvelleFonction[i] == '/' && compteurParenthesesOuvertes == 0)
                {
                    string ligneInterdite = nouvelleFonction.Substring(indexDebutLigne, i - indexDebutLigne).Trim();
                    ExpressionElement exp = new ExpressionElement(true, ligneInterdite, string.Empty);
                    exp.Interdite = true;
                    lignes.Add(exp);
                    indexDebutLigne = i;
                }
            }

            if (indexDebutLigne < nouvelleFonction.Length)
            {
                string derniereLigne = nouvelleFonction.Substring(indexDebutLigne).Trim();
                ExpressionElement exp = new ExpressionElement(true, derniereLigne, string.Empty);
                lignes.Add(exp);
            }

            // enlève les exposants seul et les formules invalides
            lignes.RemoveAll(x => x.Expression.StartsWith('^') || String.IsNullOrEmpty(x.Expression) || (x.Expression.Count(y => y == '(') != x.Expression.Count(y => y == ')'))); 
            return lignes;
        }
    }
}
