using SignaMath.Classes;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SignaMath
{
    /// <summary>
    /// Logique d'interaction pour UserControl_TableauDeVariation.xaml
    /// </summary>
    public partial class UserControl_TableauDeVariation : UserControl
    {
        public UserControl_BottomRow UserControl_BottomRow { get; }

        public UserControl_TableauDeVariation(UserControl_BottomRow userControl_BottomRow)
        {
            InitializeComponent();

            TextBox_Expression.DirectWriting = true;

            TextBox_Expression.formulaControl_formatted.Formula = "f(" + MainWindow.VariableName + ")";
            TextBox_Expression.textBox_clear.Text = "f(" + MainWindow.VariableName + ")";


            UserControl_BottomRow = userControl_BottomRow;

            UpdateRow();
        }

        internal void UpdateRow()
        {
            Grid_RightSide.Children.Clear();
            Grid_RightSide.ColumnDefinitions.Clear();

            //place les colonnes de la row
            var UcTopRow = (UserControl_TopRow)MainWindow._MainWindow.StackPanel_Row.Children[0];

            for (int i = 0; i < UcTopRow.RightSideElements.Count + 1; i++)
            {
                if (i != UcTopRow.RightSideElements.Count)
                    if (!(UcTopRow.RightSideElements[i].Value > ColumnElement.IntervalleMin && UcTopRow.RightSideElements[i].Value < ColumnElement.IntervalleMax))
                        continue;

                Grid_RightSide.ColumnDefinitions.Add(new ColumnDefinition()); // contient le signe

                Grid gridElement = new Grid();


                Border container = new Border()
                {
                    BorderBrush = Brushes.Black,
                    // si c'est une valeur interdite
                    BorderThickness = i != UcTopRow.RightSideElements.Count ? UcTopRow.RightSideElements[i].ValeurInterdite! ? new Thickness(0, 0, 2, 0) : new Thickness(0) : new Thickness(0)
                };


                bool isPlus = ((Grid)((Border)UserControl_BottomRow.Grid_RightSide.Children[i]).Child).Children.OfType<Label>().First().Content.ToString() == "+";
                Image img = new Image()
                {
                    Source = isPlus ? img_arrowTemplateTop.Source : img_arrowTemplateBottom.Source
                };

                img.HorizontalAlignment = HorizontalAlignment.Stretch;
                img.Stretch = Stretch.Fill;
                RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
                gridElement.Children.Add(img);

                UserControl_FormulaTextBox label_formula = new UserControl_FormulaTextBox();
                label_formula.AllowEmpty = true;
                label_formula.textBox_clear.Text = string.Empty;
                label_formula.MinWidth = 50;
                label_formula.MinHeight = 50;
                label_formula.HorizontalAlignment = HorizontalAlignment.Right;
                label_formula.VerticalAlignment = isPlus ? VerticalAlignment.Top : VerticalAlignment.Bottom;
                label_formula.Margin = isPlus ? new Thickness(0, 0, -20, 0) : new Thickness(0, 10.5, -20, 5);
                gridElement.Children.Add(label_formula);

                if (i == 0)
                {
                    // On a une deuxieme labelFormula à ajouter au début du tableau
                    UserControl_FormulaTextBox label_formula2 = new UserControl_FormulaTextBox();
                    label_formula2.AllowEmpty = true;
                    label_formula2.textBox_clear.Text = string.Empty;
                    label_formula2.MinWidth = 50;
                    label_formula2.MinHeight = 50;
                    label_formula2.HorizontalAlignment = HorizontalAlignment.Left;
                    label_formula2.VerticalAlignment = isPlus ? VerticalAlignment.Bottom : VerticalAlignment.Top;
                    label_formula2.Margin = isPlus ? new Thickness(0, 10.5, -20, 5) : new Thickness(0, 0, -20, 0);
                    gridElement.Children.Add(label_formula2);
                }

                // Si ce n'est pas la dernière colonne
                if (i != UcTopRow.RightSideElements.Count)
                {
                    // si c'est une valeur interdite alors double barre
                    if (UcTopRow.RightSideElements[i].ValeurInterdite)
                    {
                        Border secondBar = new Border()
                        {
                            BorderBrush = Brushes.Black,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Margin = new Thickness(0, 0, 5, 0),
                            BorderThickness = new Thickness(0, 0, 2, 0)
                        };

                        gridElement.Children.Add(secondBar);
                    }
                }
                else
                {
                    // derniere colonne
                    label_formula.Margin = isPlus ? new Thickness(0, 0, 0, 0) : new Thickness(0, 10.5, 0, 5);
                }

                container.Child = gridElement;
                Grid.SetColumn(container, i);

                Grid_RightSide.Children.Add(container);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Supprime la ligne courante du tableau
            MainWindow._MainWindow.Button_AjoutTableauVariation_Click(this, null);
        }
    }
}
