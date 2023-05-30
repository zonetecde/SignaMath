using SignaMath.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            List<string> lignes = DecouperExpression(function);

            List<string> expressions = new();
            if (lignes.Any())
            {
                for (int i = 0; i < lignes.Count; i++)
                {
                    string expression = lignes[i];

                    expression = SupprimerParentheses(expression);

                    List<string> nouvellesExpressions = new List<string>();

                    var decoupe = DecouperExpression(expression);
                    nouvellesExpressions.AddRange(decoupe);

                    nouvellesExpressions.ForEach(ligne =>
                    {
                        expressions.Add((lignes[i].Contains('/') ? 'd' : 'n') + ligne.Replace("/", string.Empty).Replace("*", string.Empty));
                    });
                }
            }

            for (int i = 0; i < expressions.Count; i++)
            {
                string? expression = expressions[i].Remove(0, 1);
                var decoupe = DecouperExpression(expression, true);
                if (decoupe.Count > 1)
                {
                    expressions.RemoveAt(i);
                    expressions.AddRange(decoupe.ConvertAll(x => expressions[i][0] + x));
                    i = 0;
                }
            }

            expressions.RemoveAll(x => x.Length == 1);

            return (expressions.ConvertAll(x => new ExpressionElement(x.StartsWith('d'), x.Remove(0, 1))));
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

        private static List<string> DecouperExpression(string nouvelleFonction, bool estDeuxieme = false)
        {
            if (estDeuxieme)
            {
                nouvelleFonction = SupprimerParentheses(nouvelleFonction);
            }

            List<string> lignes = new List<string>();
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

                    if (compteurParenthesesOuvertes == 0)
                    {
                        string ligne = nouvelleFonction.Substring(indexDebutLigne, i - indexDebutLigne + 1);
                        lignes.Add(ligne);
                        indexDebutLigne = i + 1;
                    }
                }
                else if (nouvelleFonction[i] == '/' && compteurParenthesesOuvertes == 0)
                {
                    string ligneInterdite = nouvelleFonction.Substring(indexDebutLigne, i - indexDebutLigne).Trim();
                    lignes.Add(ligneInterdite);
                    indexDebutLigne = i;
                }
            }

            if (indexDebutLigne < nouvelleFonction.Length)
            {
                string derniereLigne = nouvelleFonction.Substring(indexDebutLigne).Trim();
                lignes.Add(derniereLigne);
            }

            return lignes;
        }
    }
}
