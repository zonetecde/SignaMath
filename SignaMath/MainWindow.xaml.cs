﻿using ClassLibrary;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using SignaMath.Classes;
using SignaMath.UC;
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
        internal static MainWindow _MainWindow { get; set; }

        private const string VERSION = "1.0.3";
        internal static string BASE_URL { get; } = "https://zoneck.bsite.net";
        private Software Software { get; set; }

        internal static UserControl_TableauDeSigne TableauDeSigne { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            _MainWindow = this;
            Run_Version.Text = VERSION;
            TableauDeSigne = UC_TableauDeSigne;

            // Change la taille des lignes dans le tableau pour la taille souhaitée.
            HeightHeaderSlider.ValueChanged += (sender, e) =>
            {
                UC_TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().ForEach(x =>
                {
                    if(x.RowType == RowType.HEADER)
                        x.Height = e.NewValue;
                });
            };
            HeightTableauDeVariationSlider.ValueChanged += (sender, e) =>
            {
                UC_TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().ForEach(x =>
                {
                    if (x.RowType == RowType.TABLEAU_DE_VARIATION)
                        x.Height = e.NewValue;
                });
            };
            HeightRowSlider.ValueChanged += (sender, e) =>
            {
                UC_TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().ForEach(x =>
                {
                    if (x.RowType != RowType.HEADER)
                        x.Height = e.NewValue;
                });
            };
        }

        /// <summary>
        /// Méthode appelée lors du clic sur le bouton "Ajouter une ligne".
        /// Ajoute une ligne au tableau.
        /// </summary>
        private void Button_AddRow_Click(object sender, RoutedEventArgs e)
        {
            UC_TableauDeSigne.StackPanel_Row.Children.Add(new UserControl_Row(RowType.MIDDLE, new Random().Next(999999)) { Height = HeightRowSlider.Value });

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
                UC_TableauDeSigne.StackPanel_Row.Children.Add(new UserControl_Row(RowType.CONCLUANTE, new Random().Next(999999)) { Height = HeightRowSlider.Value });
                button_AjoutLigneConcluante.Content = "Supprimer la ligne concluante";
            }
            else
            {
                UC_TableauDeSigne.StackPanel_Row.Children.Remove(UC_TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().First(x => x.RowType == RowType.CONCLUANTE));
                button_AjoutLigneConcluante.Content = "Ajouter la ligne concluante";

                // supprime aussi le tableau de variation
                if (button_AjoutTableauVariation.Content.ToString()!.Contains("Supprimer"))
                {
                    button_AjoutTableauVariation.Content = "Ajouter le tableau de variation";
                    UC_TableauDeSigne.StackPanel_Row.Children.Remove(UC_TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().FindAll(x => x.RowType == RowType.TABLEAU_DE_VARIATION).First());
                }
            }
        }

        /// <summary>
        /// Méthode appelée lors du clic sur le bouton "Ajouter une valeur interdite".
        /// Ajoute une ligne au tableau avec une valeur interdite.
        /// </summary>
        private void Button_AddForbiddenValueRow_Click(object sender, RoutedEventArgs e)
        {
            UC_TableauDeSigne.StackPanel_Row.Children.Add(new UserControl_Row(RowType.MIDDLE_INTERDITE, new Random().Next(999999)) { Height = HeightRowSlider.Value });

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
                UC_TableauDeSigne.StackPanel_Row.Children.Remove(UC_TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().First(x => x.RowType == RowType.CONCLUANTE));
                UC_TableauDeSigne.StackPanel_Row.Children.Add(new UserControl_Row(RowType.CONCLUANTE, new Random().Next(999999)) { Height = HeightRowSlider.Value });
            }

            if (!button_AjoutTableauVariation.Content.ToString()!.Contains("Ajouter"))
            {
                UC_TableauDeSigne.StackPanel_Row.Children.Remove(UC_TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().First(x => x.RowType == RowType.TABLEAU_DE_VARIATION));
                UC_TableauDeSigne.StackPanel_Row.Children.Add(new UserControl_Row(RowType.TABLEAU_DE_VARIATION, new Random().Next(999999))
                {
                    Height = HeightTableauDeVariationSlider.Value,
                });
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

                UC_TableauDeSigne.StackPanel_Row.Children.Add(new UserControl_Row(RowType.TABLEAU_DE_VARIATION, new Random().Next(999999))
                {
                    Height = HeightTableauDeVariationSlider.Value,
                });

                button_AjoutTableauVariation.Content = "Supprimer le tableau de variation";
            }
            else
            {
                UC_TableauDeSigne.StackPanel_Row.Children.Remove(UC_TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().FindAll(x => x.RowType == RowType.TABLEAU_DE_VARIATION).First());
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
                    MainWindow._MainWindow.Cursor = System.Windows.Input.Cursors.Wait;
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
                finally
                {
                    MainWindow._MainWindow.Cursor = System.Windows.Input.Cursors.Arrow;
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
                MainWindow._MainWindow.Cursor = System.Windows.Input.Cursors.Wait;
                Extension.Extension.WriteToPng(viewBox_tableau, string.Empty, true);
            }
            catch
            {
                MessageBox.Show("Désolé, un problème est survenu !");
            }
            finally
            {
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
        private void Button_ResetBoard_Click(object sender, RoutedEventArgs e)
        {
            TableauDeSigne.StackPanel_Row.Children.RemoveRange(1, TableauDeSigne.StackPanel_Row.Children.Count -1);
            GlobalVariable.TableColumns.Clear();
            button_AjoutTableauVariation.Content = "Ajouter le tableau de variation";
            button_AjoutLigneConcluante.Content = "Ajouter la ligne concluante";
            button_AjoutTableauVariation.IsEnabled = false;
            button_AjoutLigneConcluante.IsEnabled = false;

            GlobalVariable.UpdateBoard();
        }
    }
}
