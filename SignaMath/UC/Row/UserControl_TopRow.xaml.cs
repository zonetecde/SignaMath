using SignaMath.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SignaMath
{
    /// <summary>
    /// Logique d'interaction pour UserControl_TopRow.xaml
    /// </summary>
    public partial class UserControl_TopRow : UserControl
    {
        internal List<ColumnElement> RightSideElements = new List<ColumnElement>();

        public UserControl_TopRow()
        {
            InitializeComponent();

            // Place les éléments
            TextBox_VariableName.textBox_clear.Text = "x"; // valeur par défaut
            TextBox_VariableName.textBox_clear.MaxLength = 1; // valeur par défaut

            TextBox_VariableName.FormulaChanged = (newVariable) =>
            {
                // si c'est un chiffre il y a une erreur
                if (char.IsDigit(Convert.ToChar(newVariable)))
                {
                    throw new Exception(); 
                }

                // Change toutes les variables déjà écrite
                MainWindow._MainWindow.TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().ForEach(uc =>
                {
                    uc.TextBox_Expression.textBox_clear.Text = uc.TextBox_Expression.textBox_clear.Text.Replace(MainWindow.VariableName, newVariable);
                });
                MainWindow._MainWindow.TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_BottomRow>().ToList().ForEach(uc =>
                {
                    uc.TextBox_Expression.textBox_clear.Text = uc.TextBox_Expression.textBox_clear.Text.Replace(MainWindow.VariableName, newVariable);
                });

                MainWindow.VariableName = newVariable;
            };

            Txtbox_BornLeft.textBox_clear.Text = "-\\infty";
            Txtbox_BornLeft.DirectWriting = true;
            Txtbox_BornLeft.FormulaChanged += (newIntervalMin) => { ColumnElement.IntervalleMin = newIntervalMin.Replace(" ", string.Empty) == "-\\infty" ? double.MinValue : Extension.Extension.StrToDouble(newIntervalMin); UpdateRightSideElement(); };
            Txtbox_BornRight.textBox_clear.Text = "+\\infty";
            Txtbox_BornRight.DirectWriting = true;
            Txtbox_BornRight.FormulaChanged += (newIntervalMax) => { ColumnElement.IntervalleMax = newIntervalMax.Replace(" ", string.Empty) == "+\\infty" ? double.MaxValue : Extension.Extension.StrToDouble(newIntervalMax); UpdateRightSideElement(); };

            UpdateRightSideElement();
        }

        internal void UpdateRightSideElement()
        {
            Grid_RightSide.ColumnDefinitions.Clear();
            Grid_RightSide.Children.Clear();

            RightSideElements = RightSideElements.OrderBy(x => x.Value).ToList();
            RightSideElements = RightSideElements.DistinctBy(x => x.Expression).ToList(); // Enlève les doublons
            RightSideElements.RemoveAll(x => x.FromRows.Count == 0); // Enlève les expressions utilisés dans aucune ligne

            // Set sa position
            RightSideElements.ForEach(x =>
            {
                x.Position = (byte)RightSideElements.IndexOf(x);
            });

            for (int i = 0; i < RightSideElements.Count; i++)
            {
                // Vérifie que la colonne est comprise entre les bornes
                if (!(RightSideElements[i].Value > ColumnElement.IntervalleMin && RightSideElements[i].Value < ColumnElement.IntervalleMax))
                    continue;

                // Ajout de la colonne
                Grid_RightSide.ColumnDefinitions.Add(new ColumnDefinition());

                //Rectangle r = new Rectangle();
                //r.Fill = Extension.PickBrush();
                //Grid.SetColumn(r, i);
                //Grid_RightSide.Children.Add(r);

                // Création du texte
                UserControl_FormulaTextBox uc = new UserControl_FormulaTextBox()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                };

                // Permet de centrer le nombre avec les colonnes
                uc.Loaded += (sender, e) =>
                {
                    uc.Margin = new Thickness(0, 0, (uc.ActualWidth / 2) * -1, 0);
                };

                // Ajoute le texte à la colonne
                Grid.SetColumn(uc, i);

                uc.textBox_clear.Text = RightSideElements[i].Expression;

                Grid_RightSide.Children.Add(uc);
            }

            // Colonne qui permet de tout centrer comme il le faut
            Grid_RightSide.ColumnDefinitions.Add(new ColumnDefinition());

            // On a update le header, maintenant il faut update toutes les rows
            if (MainWindow._MainWindow != null)
            {
                // on reset les signes de toutes les colonnes
                RightSideElements.ForEach(x => x.ColumnSign = '+');
                ColumnElement.LastColumnSign = '+';

                MainWindow._MainWindow.TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().ForEach(x => x.UpdateRow());
                // update ensuite la colonne final
                MainWindow._MainWindow.TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_BottomRow>().ToList().ForEach(x => x.UpdateRow());
                // Update tableau de variation
                MainWindow._MainWindow.TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_TableauDeVariation>().ToList().ForEach(x => x.UpdateRow());
            }
        }
    }
}
