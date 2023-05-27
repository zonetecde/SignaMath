using AngouriMath.Extensions;
using System;
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
        internal Action<string>? FormulaChanged = null; // Action appelée lorsque la formule est modifiée
        internal bool DirectWriting = false; // Indique si ce qui est saisi par l'utilisateur est déjà en format LaTex
        internal bool AllowEmpty = false; // Indique si une formule vide est autorisée

        public UserControl_FormulaTextBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gère l'événement de clic gauche sur la zone de texte de formule formatée.
        /// </summary>
        internal void formulaControl_formatted_MouseLeftButtonDown(object sender, MouseButtonEventArgs? e)
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
            if (textBox_clear.Background == Brushes.Transparent)
            {
                textBox_clear.Visibility = Visibility.Collapsed; // Masque la zone de texte non formatée
                formulaControl_formatted.Visibility = Visibility.Visible; // Affiche la zone de texte formatée
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
        }

        /// <summary>
        /// Gère l'événement de modification du texte dans la zone de texte non formatée.
        /// </summary>
        private void textBox_clear_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string latexExp;

                if (textBox_clear.Text.Replace(" ", string.Empty) == "-\\infty" || textBox_clear.Text.Replace(" ", string.Empty) == "+\\infty" || DirectWriting)
                {
                    latexExp = textBox_clear.Text.Replace(',', '.'); // Remplace les virgules par des points dans la formule LaTex
                }
                else
                {
                    latexExp = textBox_clear.Text.Replace(',', '.').Latexise(); // Convertit la formule en format LaTex
                }

                if (IsValidFormula(latexExp))
                {
                    textBox_clear.Background = Brushes.Transparent; // Définit le fond de la zone de texte non formatée comme transparent
                    formulaControl_formatted.Formula = latexExp; // Définit la formule formatée
                    if (FormulaChanged != null)
                    {
                        FormulaChanged(textBox_clear.Text); // Appelle l'action de notification de changement de formule
                    }
                }
                else
                {
                    textBox_clear.Background = Brushes.LightPink; // Définit le fond de la zone de texte non formatée comme rose clair en cas de formule invalide
                }
            }
            catch (Exception ex)
            {
                if (!AllowEmpty)
                {
                    textBox_clear.Background = Brushes.LightPink; // Définit le fond de la zone de texte non formatée comme rose clair en cas d'erreur
                    Console.WriteLine(ex.Message); // Affiche l'exception dans la console
                }
                else
                {
                    formulaControl_formatted.Formula = string.Empty; // Efface la formule formatée
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
    }
}
