using ClassLibrary;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
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

        private const string VERSION = "1.0.1";
        internal static string BASE_URL { get; } = "https://zoneck.bsite.net";
        private Software Software { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            _MainWindow = this;
            Run_Version.Text = VERSION;

            // Change la taille des lignes dans le tableau pour la taille souhaitée.
            HeightHeaderSlider.ValueChanged += (sender, e) =>
            {
                TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_TopRow>().ToList().ForEach(x =>
                {
                    x.Height = e.NewValue;
                });
            };
            HeightTableauDeVariationSlider.ValueChanged += (sender, e) =>
            {
                TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_TableauDeVariation>().ToList().ForEach(x =>
                {
                    x.Height = e.NewValue;
                });
            };
            HeightRowSlider.ValueChanged += (sender, e) =>
            {
                foreach (var control in TableauDeSigne.StackPanel_Row.Children)
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
            TableauDeSigne.StackPanel_Row.Children.Add(new UserControl_Row((UserControl_TopRow)TableauDeSigne.StackPanel_Row.Children[0], new Random().Next(999999)) { Height = HeightRowSlider.Value });

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
                TableauDeSigne.StackPanel_Row.Children.Add(new UserControl_BottomRow() { Height = HeightRowSlider.Value });
                button_AjoutLigneConcluante.Content = "Supprimer la ligne concluante";
            }
            else
            {
                TableauDeSigne.StackPanel_Row.Children.Remove(TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_BottomRow>().ToList()[0]);
                button_AjoutLigneConcluante.Content = "Ajouter la ligne concluante";

                // supprime aussi le tableau de variation
                if (button_AjoutTableauVariation.Content.ToString()!.Contains("Supprimer"))
                {
                    button_AjoutTableauVariation.Content = "Ajouter le tableau de variation";
                    TableauDeSigne.StackPanel_Row.Children.Remove(TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_TableauDeVariation>().ToList()[0]);
                }
            }
        }

        /// <summary>
        /// Méthode appelée lors du clic sur le bouton "Ajouter une valeur interdite".
        /// Ajoute une ligne au tableau avec une valeur interdite.
        /// </summary>
        private void Button_AddForbiddenValueRow_Click(object sender, RoutedEventArgs e)
        {
            TableauDeSigne.StackPanel_Row.Children.Add(new UserControl_Row((UserControl_TopRow)TableauDeSigne.StackPanel_Row.Children[0], new Random().Next(999999), true) { Height = HeightRowSlider.Value });

            button_AjoutLigneConcluante.IsEnabled = true;
            button_AjoutTableauVariation.IsEnabled = true;

            MoveDownConclusionRow();
        }

        /// <summary>
        /// Déplace la ligne concluante et le tableau de variation vers la fin du tableau.
        /// </summary>
        private void MoveDownConclusionRow()
        {
            if (!button_AjoutLigneConcluante.Content.ToString()!.Contains("Ajouter"))
            {
                TableauDeSigne.StackPanel_Row.Children.Remove(TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_BottomRow>().ToList()[0]);
                TableauDeSigne.StackPanel_Row.Children.Add(new UserControl_BottomRow() { Height = HeightRowSlider.Value });
            }

            if (!button_AjoutTableauVariation.Content.ToString()!.Contains("Ajouter"))
            {
                TableauDeSigne.StackPanel_Row.Children.Remove(TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_TableauDeVariation>().ToList()[0]);
                TableauDeSigne.StackPanel_Row.Children.Add(new UserControl_TableauDeVariation(TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_BottomRow>().ToList()[0]) { Height = HeightTableauDeVariationSlider.Value });
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

                UserControl_TableauDeVariation uc = new UserControl_TableauDeVariation(TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_BottomRow>().First())
                {
                    Height= HeightTableauDeVariationSlider.Value,
                };
                TableauDeSigne.StackPanel_Row.Children.Add(uc);

                button_AjoutTableauVariation.Content = "Supprimer le tableau de variation";
            }
            else
            {
                TableauDeSigne.StackPanel_Row.Children.Remove(TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_TableauDeVariation>().ToList()[0]);
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
                    MainWindow._MainWindow.Cursor = System.Windows.Input.Cursors.Arrow;
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

        /// <summary>
        /// Méthode appelée lors du clic sur le bouton "Copier".
        /// Copie dans le presse papier l'image du tableau
        /// </summary>
        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Extension.Extension.WriteToPng(viewBox_tableau, string.Empty, true);
            }
            catch
            {
                MessageBox.Show("Désolé, un problème est survenu !");
                MainWindow._MainWindow.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        /// <summary>
        /// Télécharge la nouvelle mise à jour
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_DownloadUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (HttpClient client = new())
                {
                    var progress = new Progress<int>(value =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            progressBar_downloadUpdate.Value = value;
                        });
                    });

                    string filePath = @"V_SignaMath.exe";
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    HttpResponseMessage response = await client.GetAsync(Software.DownloadLink, HttpCompletionOption.ResponseHeadersRead);

                    var contentLength = response.Content.Headers.ContentLength;

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            var buffer = new byte[4096];
                            var totalBytesRead = 0L;
                            var bytesRead = 0;
                            do
                            {
                                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                                await fileStream.WriteAsync(buffer, 0, bytesRead);

                                totalBytesRead += bytesRead;

                                if (contentLength.HasValue)
                                {
                                    var progressPercentage = (int)(100 * totalBytesRead / contentLength.Value);
                                    ((IProgress<int>)progress).Report(progressPercentage);
                                }
                            } while (bytesRead != 0);
                        }
                    }

                    Process.Start(filePath);
                    this.Close();
                }

            }
            catch
            {
                var ps = new ProcessStartInfo("www.github.com/zonetecde/SignaMath")
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(ps);
                MessageBox.Show("Désolé, il semble y avoir eu un problème avec l'installation de la dernière mise à jour !\nMerci de l'installer manuellement sur :\ngithub.com/zonetecde/SignaMath");
            }
        }

        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            // Récupère les infos sur le logiciel
            try
            {
                using (HttpClient client = new HttpClient(new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
                }))
                {
                    HttpResponseMessage response = await client.GetAsync(BASE_URL + "/api/Software/get-software-info?software=SignaMath");
                    if (response.IsSuccessStatusCode)
                    {
                        Software = JsonConvert.DeserializeObject<Software>(await response.Content.ReadAsStringAsync())!;

                        // Vérifie si on a la dernière version
                        if (Software.Version != VERSION)
                        {
                            Grid_NewUpdate.Visibility = Visibility.Visible;
                            richTextBox_news.Document.Blocks.Clear();
                            richTextBox_news.AppendText(Software.Features.Replace("\\n", "\n"));
                        }
                    }
                    else
                    {
                        // serveur problème
                    }

                }
            }
            catch
            {
                // Pas de connexion internet / Problème serveur
            }
        }

        /// <summary>
        /// Ferme la fenêtre de maj
        /// </summary>
        private void Button_CloseUpdate_Click(object sender, RoutedEventArgs e)
        {
            Grid_NewUpdate.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Supprime toutes les rows du tableau
        /// </summary>
        private void Button_ClearAll_Click(object sender, RoutedEventArgs e)
        {
            TableauDeSigne.StackPanel_Row.Children.RemoveRange(1, TableauDeSigne.StackPanel_Row.Children.Count - 1);
            TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_TopRow>().First().RightSideElements.Clear();
            TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_TopRow>().First().UpdateRightSideElement();
            TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_TopRow>().First().Txtbox_BornLeft.textBox_clear.Text = "-\\infty";
            TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_TopRow>().First().Txtbox_BornRight.textBox_clear.Text = "+\\infty";
            button_AjoutTableauVariation.Content = "Ajouter le tableau de variation";
            button_AjoutLigneConcluante.Content = "Ajouter la ligne concluante";
        }
    }
}
