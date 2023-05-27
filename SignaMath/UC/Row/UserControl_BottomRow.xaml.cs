using SignaMath.Classes;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SignaMath
{
    /// <summary>
    /// Logique d'interaction pour UserControl_BottomRow.xaml
    /// </summary>
    public partial class UserControl_BottomRow : UserControl
    {
        public UserControl_BottomRow()
        {
            InitializeComponent();

            TextBox_Expression.DirectWriting = true;

            TextBox_Expression.formulaControl_formatted.Formula = "f'(" + MainWindow.VariableName + ")";
            TextBox_Expression.textBox_clear.Text = "f'(" + MainWindow.VariableName + ")";

            UpdateRow();
        }

        // Met à jour la ligne inférieure avec les colonnes correspondantes
        internal void UpdateRow()
        {
            Grid_RightSide.Children.Clear();
            Grid_RightSide.ColumnDefinitions.Clear();

            // Récupère la première ligne du tableau de variation
            var UcTopRow = (UserControl_TopRow)MainWindow._MainWindow.StackPanel_Row.Children[0];

            for (int i = 0; i < UcTopRow.RightSideElements.Count + 1; i++)
            {
                // Vérifie si la valeur de la colonne est dans l'intervalle
                if (i != UcTopRow.RightSideElements.Count)
                {
                    if (!(UcTopRow.RightSideElements[i].Value > ColumnElement.IntervalleMin && UcTopRow.RightSideElements[i].Value < ColumnElement.IntervalleMax))
                        continue;
                }

                Grid_RightSide.ColumnDefinitions.Add(new ColumnDefinition());

                Grid gridElement = new Grid();

                Label label_signe = new Label()
                {
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    FontSize = 24
                };

                label_signe.Cursor = Cursors.Hand;

                // Inverse le signe lorsque le label est cliqué
                label_signe.PreviewMouseLeftButtonUp += (sender, e) =>
                {
                    label_signe.Content = label_signe.Content.ToString() == "+" ? '-' : '+';
                    try
                    {
                        // Met à jour le tableau de variation
                        MainWindow._MainWindow.StackPanel_Row.Children.OfType<UserControl_TableauDeVariation>().First().UpdateRow();
                    }
                    catch
                    {
                        // Aucun tableau de variation trouvé
                    }
                };

                gridElement.Children.Add(label_signe);

                Border container = new Border()
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = i != UcTopRow.RightSideElements.Count ? new Thickness(0, 0, 2, 0) : new Thickness(0)
                };

                if (i != UcTopRow.RightSideElements.Count)
                {
                    if (UcTopRow.RightSideElements[i].ValeurInterdite)
                    {
                        // Ajoute une deuxième barre pour les valeurs interdites
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
                        Label label_0 = new Label();
                        label_0.VerticalAlignment = VerticalAlignment.Center;
                        label_0.HorizontalAlignment = HorizontalAlignment.Right;
                        label_0.Margin = new Thickness(0, 0, -12, 0);
                        label_0.FontSize = 24;
                        label_0.Content = "0";
                        gridElement.Children.Add(label_0);
                    }

                    // Définit le signe en fonction de la colonne correspondante
                    label_signe.Content = UcTopRow.RightSideElements[i].ColumnSign;
                }
                else
                {
                    // Utilise le signe de la dernière colonne
                    label_signe.Content = ColumnElement.LastColumnSign;
                }

                container.Child = gridElement;
                Grid.SetColumn(container, i);

                Grid_RightSide.Children.Add(container);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Supprime la ligne courante du tableau
            MainWindow._MainWindow.Button_AjoutLigneConcluante_Click(this, null);
        }
    }
}