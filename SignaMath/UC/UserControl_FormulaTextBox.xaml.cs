using AngouriMath.Extensions;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SignaMath
{
    /// <summary>
    /// Cette classe définit un contrôle utilisateur qui représente une zone de texte de formule.
    /// </summary>
    public partial class UserControl_FormulaTextBox : UserControl
    {
        internal Action<string,bool>? FormulaChanged = null; // Action appelée lorsque la formule est modifiée
        internal bool DirectWriting = false; // Indique si ce qui est saisi par l'utilisateur est déjà en format LaTex
        internal bool AllowEmpty = false; // Indique si une formule vide est autorisée

        internal string DefaultFormula = string.Empty; // La formule donnée lors de la création

        internal string previousFormula = string.Empty;
        internal string previousText = string.Empty;

        public UserControl_FormulaTextBox()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                previousFormula = formulaControl_formatted.Formula;
                DefaultFormula = formulaControl_formatted.Formula;
                previousText = textBox_clear.Text;
            };
        }

        /// <summary>
        /// Gère l'événement de clic gauche sur la zone de texte de formule formatée.
        /// </summary>
        internal void formulaControl_formatted_MouseLeftButtonUp(object sender, MouseButtonEventArgs? e)
        {
            textBox_clear.Visibility = Visibility.Visible; // Affiche la zone de texte non formatée
            formulaControl_formatted.Visibility = Visibility.Collapsed; // Masque la zone de texte formatée
            textBox_clear.Focus(); // Donne le focus à la zone de texte non formatée
        }

        /// <summary>
        /// Gère l'événement de perte de focus de la zone de texte non formatée.
        /// </summary>
        private void textBox_clear_LostFocus(object sender, RoutedEventArgs? e)
        {
            // met la dernière valeur juste que l'utilisateur ai écrite
            formulaControl_formatted.Formula = previousFormula;
            textBox_clear.Text = previousText;
            textBox_clear.Visibility = Visibility.Collapsed;
            formulaControl_formatted.Visibility = Visibility.Visible;

            if(AllowEmpty && String.IsNullOrEmpty(textBox_clear.Text))
            {
                formulaControl_formatted.Formula = DefaultFormula;
            }
        }

        /// <summary>
        /// Gère l'événement de pression d'une touche dans la zone de texte non formatée.
        /// </summary>
        private void textBox_clear_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                textBox_clear_LostFocus(this, null); // Appelle la méthode de perte de focus pour la zone de texte non formatée
            }
            else if(e.Key == Key.Escape)
            {
                // valeur par défaut
                formulaControl_formatted.Formula = previousFormula;
                textBox_clear.Text = previousText;
                textBox_clear.Visibility = Visibility.Collapsed; 
                formulaControl_formatted.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Gère l'événement de modification du texte dans la zone de texte non formatée.
        /// </summary>
        private void textBox_clear_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string newFormula = textBox_clear.Text.Replace(',', '.').Trim();

                // si on a une exprresion du type x4, on remplace par x*4 car sinon c'est compté comme x^4.
                string pattern = @"x(\d)";
                string temp = newFormula;
                newFormula = Regex.Replace(newFormula, pattern, "x*$1");
                if (temp != newFormula)
                {
                    textBox_clear.Text = newFormula;
                    textBox_clear.CaretIndex = textBox_clear.Text.Length;
                    return;
                }

                string latexExp;

                // si c'est des formules spéciales où du directwriting
                if (textBox_clear.Text.Replace(" ", string.Empty) == "-\\infty" ||newFormula.Replace(" ", string.Empty) == "+\\infty" || DirectWriting || (AllowEmpty && string.IsNullOrEmpty(newFormula)))
                {
                    latexExp =newFormula;
                }
                else
                {
                    latexExp =newFormula.Latexise(); // Convertit la formule en format LaTex
                }

                if (IsValidFormula(latexExp))
                {
                    textBox_clear.Background = Brushes.Transparent; // Définit le fond de la zone de texte non formatée comme transparent
                    formulaControl_formatted.Formula = latexExp; // Définit la formule formatée

                    // si c'est un nbre à virgule infini alors n'affiche que les premiers digits
                    Regex regex = new Regex(@"^-?\d+\.\d+$");

                    if (regex.IsMatch(latexExp))
                    {
                        if (latexExp.Length > 5)
                            formulaControl_formatted.Formula = latexExp.Substring(0, 5) + "...";
                    }

                    if (FormulaChanged != null)
                    {
                        FormulaChanged(textBox_clear.Text, true); // Appelle l'action de notification de changement de formule

                        // Si on est jusqu'ici c'est que tout est bon, la formule est valide
                        previousText =newFormula;
                        previousFormula = formulaControl_formatted.Formula;
                    }
                }
                else
                {
                    textBox_clear.Background = Brushes.LightPink; // Définit le fond de la zone de texte non formatée comme rose clair en cas de formule invalide
                }
            }
            catch (Exception ex)
            {
                if (AllowEmpty && String.IsNullOrEmpty(textBox_clear.Text))
                {
                    formulaControl_formatted.Formula = string.Empty; // Efface la formule formatée
                }
                else
                {
                    textBox_clear.Background = Brushes.LightPink; // Définit le fond de la zone de texte non formatée comme rose clair en cas d'erreur
                    Console.WriteLine(ex.Message); // Affiche l'exception dans la console
                }
            }
        }

        /// <summary>
        /// Vérifie si une formule est valide en utilisant la zone de texte formatée.
        /// </summary>
        private bool IsValidFormula(string input)
        {
            formulaControl_formatted.Formula = input; // Définit la formule formatée
            return formulaControl_formatted.Errors.Count == 0; // Renvoie vrai si la formule est valide, sinon faux
        }

        /// <summary>
        /// Simule un clique pour trigger la formulaBox
        /// </summary>
        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            formulaControl_formatted_MouseLeftButtonUp(this, null);
        }
    }
}
