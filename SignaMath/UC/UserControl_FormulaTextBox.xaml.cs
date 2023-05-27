using AngouriMath.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SignaMath
{
    /// <summary>
    /// Logique d'interaction pour UserControl_FormulaTextBox.xaml
    /// </summary>
    public partial class UserControl_FormulaTextBox : UserControl
    {
        internal Action<string>? FormulaChanged = null;
        internal bool DirectWriting = false; // Ce que l'utilisateur écris est déjà en LaTex
        internal bool AllowEmpty = false;

        public UserControl_FormulaTextBox()
        {
            InitializeComponent();
        }

        internal void formulaControl_formatted_MouseLeftButtonDown(object sender, MouseButtonEventArgs? e)
        {
            textBox_clear.Visibility = Visibility.Visible;
            formulaControl_formatted.Visibility = Visibility.Collapsed;
            textBox_clear.Focus();
        }

        private void textBox_clear_LostFocus(object sender, RoutedEventArgs? e)
        {
            if (textBox_clear.Background == Brushes.Transparent)
            {
                textBox_clear.Visibility = Visibility.Collapsed;
                formulaControl_formatted.Visibility = Visibility.Visible;
            }
        }

        private void textBox_clear_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                textBox_clear_LostFocus(this, null);
            }
        }

        private void textBox_clear_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Bordure rouge si formule invalide
            try
            {
                string latexExp;

                if (textBox_clear.Text.Replace(" ", string.Empty) == "-\\infty" || textBox_clear.Text.Replace(" ", string.Empty) == "+\\infty" || DirectWriting)
                {
                    latexExp = textBox_clear.Text.Replace(',', '.');
                }

                else latexExp = textBox_clear.Text.Replace(',', '.').Latexise();

                if (IsValidFormula(latexExp))
                {
                    textBox_clear.Background = Brushes.Transparent;
                    formulaControl_formatted.Formula = latexExp;
                    if (FormulaChanged != null)

                        FormulaChanged(textBox_clear.Text);
                }
                else
                {
                    textBox_clear.Background = Brushes.LightPink;
                }
            }
            catch (Exception ex)
            {
                if (!AllowEmpty)
                {
                    textBox_clear.Background = Brushes.LightPink;

                    Console.WriteLine(ex.Message);
                }
                else
                {
                    formulaControl_formatted.Formula = string.Empty;
                }
            }
        }

        private bool IsValidFormula(string input)
        {
            formulaControl_formatted.Formula = input;
            return formulaControl_formatted.Errors.Count == 0;
        }
    }
}
