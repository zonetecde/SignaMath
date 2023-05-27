using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace SignaMath
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static string VariableName { get; set; } = "x";
        internal static MainWindow _MainWindow { get; set; }

        private const string VERSION = "1.0.0";

        public MainWindow()
        {
            InitializeComponent();

            _MainWindow = this;
            Run_Version.Text = VERSION;

            // Change la taille des lignes dans le tableau pour la taille souhaitée.
            HeightHeaderSlider.ValueChanged += (sender, e) =>
            {
                StackPanel_Row.Children.OfType<UserControl_TopRow>().ToList().ForEach(x =>
                {
                    x.Height = e.NewValue;
                });
            };
            HeightTableauDeVariationSlider.ValueChanged += (sender, e) =>
            {
                StackPanel_Row.Children.OfType<UserControl_TableauDeVariation>().ToList().ForEach(x =>
                {
                    x.Height = e.NewValue;
                });
            };
            HeightRowSlider.ValueChanged += (sender, e) =>
            {
                foreach (var control in StackPanel_Row.Children)
                {
                    if (control is UserControl_Row || control is UserControl_BottomRow)
                    {
                        ((FrameworkElement)control).Height = e.NewValue;
                    }
                }
            };
        }

        /// <summary>
        /// Méthode appelée lors du clic sur le bouton "Ajouter une ligne".
        /// Ajoute une ligne au tableau.
        /// </summary>
        private void Button_AddRow_Click(object sender, RoutedEventArgs e)
        {
            StackPanel_Row.Children.Add(new UserControl_Row((UserControl_TopRow)StackPanel_Row.Children[0], new Random().Next(999999)) { Height = HeightRowSlider.Value });

            button_AjoutLigneConcluante.IsEnabled = true;
            button_AjoutTableauVariation.IsEnabled = true;

            MoveDownConclusionRow();
        }

        /// <summary>
        /// Méthode appelée lors du clic sur le bouton "Ajouter une ligne concluante".
        /// Ajoute ou supprime une ligne concluante du tableau.
        /// </summary>
        internal void Button_AjoutLigneConcluante_Click(object sender, RoutedEventArgs? e)
        {
            if (button_AjoutLigneConcluante.Content.ToString()!.Contains("Ajouter"))
            {
                StackPanel_Row.Children.Add(new UserControl_BottomRow() { Height = HeightRowSlider.Value });
                button_AjoutLigneConcluante.Content = "Supprimer la ligne concluante";
            }
            else
            {
                StackPanel_Row.Children.Remove(StackPanel_Row.Children.OfType<UserControl_BottomRow>().ToList()[0]);
                button_AjoutLigneConcluante.Content = "Ajouter la ligne concluante";

                // supprime aussi le tableau de variation
                if (button_AjoutTableauVariation.Content.ToString()!.Contains("Supprimer"))
                {
                    button_AjoutTableauVariation.Content = "Ajouter le tableau de variation";
                    StackPanel_Row.Children.Remove(StackPanel_Row.Children.OfType<UserControl_TableauDeVariation>().ToList()[0]);
                }
            }
        }

        /// <summary>
        /// Méthode appelée lors du clic sur le bouton "Ajouter une valeur interdite".
        /// Ajoute une ligne au tableau avec une valeur interdite.
        /// </summary>
        private void Button_AddForbiddenValueRow_Click(object sender, RoutedEventArgs e)
        {
            StackPanel_Row.Children.Add(new UserControl_Row((UserControl_TopRow)StackPanel_Row.Children[0], new Random().Next(999999), true) { Height = HeightRowSlider.Value });

            MoveDownConclusionRow();
        }

        /// <summary>
        /// Déplace la ligne concluante et le tableau de variation vers la fin du tableau.
        /// </summary>
        private void MoveDownConclusionRow()
        {
            if (!button_AjoutLigneConcluante.Content.ToString()!.Contains("Ajouter"))
            {
                StackPanel_Row.Children.Remove(StackPanel_Row.Children.OfType<UserControl_BottomRow>().ToList()[0]);
                StackPanel_Row.Children.Add(new UserControl_BottomRow() { Height = HeightRowSlider.Value });
            }

            if (!button_AjoutTableauVariation.Content.ToString()!.Contains("Ajouter"))
            {
                StackPanel_Row.Children.Remove(StackPanel_Row.Children.OfType<UserControl_TableauDeVariation>().ToList()[0]);
                StackPanel_Row.Children.Add(new UserControl_TableauDeVariation(StackPanel_Row.Children.OfType<UserControl_BottomRow>().ToList()[0]) { Height = HeightRowSlider.Value });
            }
        }

        /// <summary>
        /// Méthode appelée lors du clic sur le bouton "Ajouter le tableau de variation".
        /// Ajoute ou supprime le tableau de variation du tableau.
        /// </summary>
        internal void Button_AjoutTableauVariation_Click(object sender, RoutedEventArgs? e)
        {
            if (button_AjoutTableauVariation.Content.ToString()!.Contains("Ajouter"))
            {
                if (button_AjoutLigneConcluante.Content.ToString()!.Contains("Ajouter"))
                {
                    Button_AjoutLigneConcluante_Click(this, null);
                }

                UserControl_TableauDeVariation uc = new UserControl_TableauDeVariation(StackPanel_Row.Children.OfType<UserControl_BottomRow>().First());
                StackPanel_Row.Children.Add(uc);

                button_AjoutTableauVariation.Content = "Supprimer le tableau de variation";
            }
            else
            {
                StackPanel_Row.Children.Remove(StackPanel_Row.Children.OfType<UserControl_TableauDeVariation>().ToList()[0]);
                button_AjoutTableauVariation.Content = "Ajouter le tableau de variation";
            }
        }

        /// <summary>
        /// Méthode appelée lors du clic sur le bouton "Enregistrer".
        /// Permet d'enregistrer le tableau au format PNG dans un dossier sélectionné par l'utilisateur.
        /// </summary>
        private void button_Save_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\Users";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    Extension.Extension.WriteToPng(viewBox_tableau, dialog.FileName + @"\tableau" + DateTime.Now.ToString("hh_mm_ss") + ".png");
                    try
                    {
                        Process.Start("explorer.exe", dialog.FileName);
                    }
                    catch { }
                }
                catch
                {
                    MessageBox.Show("Désolé, un problème est survenu !");
                }
            }
        }

        /// <summary>
        /// Méthode appelée lors du clic sur le bouton "Aide".
        /// Affiche la grille d'aide.
        /// </summary>
        private void button_Help_Click(object sender, RoutedEventArgs e)
        {
            Grid_Help.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Méthode appelée lors du clic sur le bouton "Fermer l'aide".
        /// Masque la grille d'aide.
        /// </summary>
        private void Button_CloseHelp_Click(object sender, RoutedEventArgs e)
        {
            Grid_Help.Visibility = Visibility.Collapsed;
        }
    }
}
