using AngouriMath;
using AngouriMath.Extensions;
using ClassLibrary;
using HonkSharp.Fluency;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using SignaMath.Classes;
using SignaMath.Extension;
using SignaMath.UC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using static AngouriMath.Entity;

namespace SignaMath
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static MainWindow _MainWindow { get; set; }

        private const string VERSION = "1.1.4";
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
                ChangeRowHeight(e.NewValue, RowType.HEADER);
            };
            HeightTableauDeVariationSlider.ValueChanged += (sender, e) =>
            {
                ChangeRowHeight(e.NewValue, RowType.TABLEAU_DE_VARIATION);
            };
            HeightRowSlider.ValueChanged += (sender, e) =>
            {
                ChangeRowHeight(e.NewValue, RowType.MIDDLE);
                ChangeRowHeight(e.NewValue, RowType.MIDDLE_INTERDITE);
                ChangeRowHeight(e.NewValue, RowType.CONCLUANTE);
            };

            // Set la formula textbox
            InitFormulaBox();

            formulaTextBox_fName.FormulaChanged += ChangeFonctionName;
            formulaTextBox_content.FormulaChanged += NewFunctionWrited;
            formulaTextBox_tabDeVariation.FormulaChanged += FonctionTabDeVarChanged;

            // Change le côté droit de l'équation
            formulaBox_y.FormulaChanged += (newY, c) =>
            {
                var d = Extension.Extension.StrToDouble(newY);
                
                GlobalVariable.Y = d;
                GlobalVariable.UpdateColumn();
            };
        }

        /// <summary>
        /// Une fonction pour calculer les valeurs du tableau de variation a été entrée
        /// </summary>
        /// <param name="function">La nouvelle fonction</param>
        /// <param name="callUpdateBoard"></param>
        private void FonctionTabDeVarChanged(string function, bool callUpdateBoard = true)
        {
            if (string.IsNullOrEmpty(function))
            {
                GlobalVariable.TableauDeVariationFormule = string.Empty;
                return;
            }

            if (function.Contains(GlobalVariable.VariableName))
            {
                GlobalVariable.TableauDeVariationFormule = function;

                if(!button_AjoutTableauVariation.Content.ToString()!.Contains("Ajouter"))
                {
                    ((UserControl_Row)TableauDeSigne.StackPanel_Row.Children[TableauDeSigne.StackPanel_Row.Children.Count - 1])
                        .UpdateRow();
                }
                else
                {
                    if(TableauDeSigne.StackPanel_Row.Children.Count >1) // si on a au moins une colonne
                        Button_AjoutTableauVariation_Click(this, null);
                }
            }
            else
            {
                throw new Exception("_La formule doit posséder au moins une occurrence de '" + GlobalVariable.VariableName +"'.");
            }
        }

        /// <summary>
        /// Un nouveau nom de fonction a été donné
        /// Change en conséquence le nom de la fonction et de la variable
        /// </summary>
        /// <param name="newFuncName"></param>
        private void ChangeFonctionName(string newFuncName, bool callUpdateBoard = true)
        {
            if (!String.IsNullOrEmpty(newFuncName))
            {
                // Vérifie le format de la chaine
                newFuncName = newFuncName.Trim();
                if (newFuncName.Contains("="))
                {
                    // enlève l'égal et tout ce qui va après
                    newFuncName = newFuncName.Substring(0, newFuncName.IndexOf('=')).Trim();
                }

                formulaTextBox_fName.formulaControl_formatted.Formula = newFuncName + " =";

                // vérifie que le string est au format a(b)
                string pattern = @"^[A-Za-z]\([A-Za-z]\)$";

                if (Regex.IsMatch(newFuncName, pattern))
                {
                    // change la variable
                    string newVariable = Regex.Match(newFuncName, @"\(([^)]*)\)").Groups[1].Value;
                    if (newVariable.Length == 1)
                    {
                        if (newVariable != "e") // e est une variable réservé à l'expo
                        {
                            // change la lettre de la fonction
                            UC_TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().ForEach(x =>
                            {
                                if (x.RowType == RowType.HEADER)
                                    x.TextBox_Expression.textBox_clear.Text = newVariable;
                            });
                        }
                        else
                            throw new Exception("_La variable ne peut être 'e' car 'e' est réservé pour l'exponentielle");
                    }

                    if (!char.IsDigit(newFuncName[0]))
                    {
                        // change la lettre de la fonction
                        GlobalVariable.FonctionName = newFuncName[0];

                        UC_TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().ForEach(x =>
                        {
                            if (x.RowType == RowType.CONCLUANTE)
                                x.TextBox_Expression.textBox_clear.Text = newFuncName[0].ToString() + "'(" + newVariable + ")";
                            if (x.RowType == RowType.TABLEAU_DE_VARIATION)
                                x.TextBox_Expression.textBox_clear.Text = newFuncName[0].ToString() + "(" + newVariable + ")";
                        });
                    }

                    return;
                }

            }
            throw new Exception("_Le format du nom de la fonction doit être `f(x)`");       
        }

        /// <summary>
        /// Une nouvelle fonction a été écris par l'utilisateur, 
        /// dresse donc son tableau de signe
        /// </summary>
        /// <param name="newFunction">La nouvelle fonction</param>
        private void NewFunctionWrited(string newFunction, bool callUpdateBoard = true)
        {
            if (string.IsNullOrEmpty(newFunction))
            {
                return;
            }

            var rows = Sheller.ShellFunction(newFunction);

            GlobalVariable.AllowFocusWhenAdded = false;

            // clear le tableau
            Button_ResetBoard_Click(this, null);

            // Ajoute les nouvelles rows
            foreach(var row in rows)
            {
                if(row.Interdite)
                {
                    Button_AddForbiddenValueRow_Click(this, null);
                    TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().Last().TextBox_Expression.textBox_clear.Text = row.Expression;
                }
                else
                {
                    Button_AddRow_Click(this, null);
                    TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().Last().TextBox_Expression.textBox_clear.Text = row.Expression;
                }
            }

            // si une formule du tab de variation est entrée on affiche directe le tab de variation
            if (String.IsNullOrEmpty(GlobalVariable.TableauDeVariationFormule))
            {
                Button_AjoutLigneConcluante_Click(this, null);
            }
            else
            {
                Button_AjoutTableauVariation_Click(this, null);
            }

            GlobalVariable.AllowFocusWhenAdded = true;
        }


        /// <summary>
        /// Change la height d'une colonne si elle admet la condition de Type
        /// </summary>
        /// <param name="newValue">La nouvelle valeur</param>
        /// <param name="rowType">La condition type</param>
        private void ChangeRowHeight(double newValue, RowType rowType)
        {
            UC_TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().ForEach(x =>
            {
                if (x.RowType == rowType)
                    x.Height = newValue;
            });
        }

        /// <summary>
        /// Méthode appelée lors du clic sur le bouton "Ajouter une ligne".
        /// Ajoute une ligne au tableau.
        /// </summary>
        private void Button_AddRow_Click(object sender, RoutedEventArgs? e)
        {
            // Supprime toutes les rows avec une textbox vide
            TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().Where(x => String.IsNullOrEmpty(x.TextBox_Expression.textBox_clear.Text)).ToList().ForEach(x => TableauDeSigne.StackPanel_Row.Children.Remove(x));

            UC_TableauDeSigne.StackPanel_Row.Children.Add(new UserControl_Row(RowType.MIDDLE, new Random().Next(999999)) { Height = HeightRowSlider.Value });

            button_AjoutLigneConcluante.IsEnabled = true;
            button_AjoutTableauVariation.IsEnabled = true;

            MoveDownConclusionRow();
        }

        /// <summary>
        /// Méthode appelée lors du clic sur le bouton "Ajouter une valeur interdite".
        /// Ajoute une ligne au tableau avec une valeur interdite.
        /// </summary>
        private void Button_AddForbiddenValueRow_Click(object sender, RoutedEventArgs? e)
        {
            // Supprime toutes les rows avec une textbox vide
            TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().Where(x => String.IsNullOrEmpty(x.TextBox_Expression.textBox_clear.Text)).ToList().ForEach(x => TableauDeSigne.StackPanel_Row.Children.Remove(x));

            UC_TableauDeSigne.StackPanel_Row.Children.Add(new UserControl_Row(RowType.MIDDLE_INTERDITE, new Random().Next(999999)) { Height = HeightRowSlider.Value });

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
            // Supprime toutes les rows avec une textbox vide
            TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().Where(x => String.IsNullOrEmpty(x.TextBox_Expression.textBox_clear.Text)).ToList().ForEach(x => TableauDeSigne.StackPanel_Row.Children.Remove(x));

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
        /// Méthode appelée lors du clic sur le bouton "Ajouter le tableau de variation".
        /// Ajoute ou supprime le tableau de variation du tableau.
        /// </summary>
        internal void Button_AjoutTableauVariation_Click(object sender, RoutedEventArgs? e)
        {
            // Supprime toutes les rows avec une textbox vide
            TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().Where(x => String.IsNullOrEmpty(x.TextBox_Expression.textBox_clear.Text)).ToList().ForEach(x => TableauDeSigne.StackPanel_Row.Children.Remove(x));

            if (button_AjoutTableauVariation.Content.ToString()!.Contains("Ajouter"))
            {
                if (button_AjoutLigneConcluante.Content.ToString()!.Contains("Ajouter"))
                {
                    Button_AjoutLigneConcluante_Click(this, null);
                }

                UserControl_Row uc = new UserControl_Row(RowType.TABLEAU_DE_VARIATION, new Random().Next(999999))
                {
                    Height = HeightTableauDeVariationSlider.Value,
                };

                UC_TableauDeSigne.StackPanel_Row.Children.Add(uc);

                button_AjoutTableauVariation.Content = "Supprimer le tableau de variation";
            }
            else
            {
                UC_TableauDeSigne.StackPanel_Row.Children.Remove(UC_TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().FindAll(x => x.RowType == RowType.TABLEAU_DE_VARIATION).First());
                button_AjoutTableauVariation.Content = "Ajouter le tableau de variation";
            }
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
        /// Init les formulas box de la fenêtre principale
        /// </summary>
        private void InitFormulaBox()
        {
            InitSpecificFormulaBox(formulaTextBox_content, "ecrire\\:une\\:fonction");
            InitSpecificFormulaBox(formulaTextBox_tabDeVariation, "ecrire\\:une\\:formule");

            formulaTextBox_fName.formulaControl_formatted.Formula = "f(x) = ";
            formulaTextBox_fName.DirectWriting = true;

            formulaBox_y.textBox_clear.Text = GlobalVariable.Y.ToString();
            formulaBox_y.textBox_clear.HorizontalContentAlignment = HorizontalAlignment.Left;
            formulaBox_y.formulaControl_formatted.HorizontalContentAlignment = HorizontalAlignment.Left;
        }

        private void InitSpecificFormulaBox(UserControl_FormulaTextBox f, string str)
        {
            
            f.textBox_clear.Visibility = Visibility.Hidden;
            f.formulaControl_formatted.Formula = str;
            f.AllowEmpty = true;
            f.textBox_clear.MinWidth = formulaTextBox_content.formulaControl_formatted.MinWidth;
            f.textBox_clear.HorizontalAlignment = HorizontalAlignment.Left;
            f.formulaControl_formatted.HorizontalAlignment = HorizontalAlignment.Left;
            f.formulaControl_formatted.Visibility = Visibility.Visible;
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
            Grid_Information.Visibility = Visibility.Collapsed;
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
        private void Button_ResetBoard_Click(object sender, RoutedEventArgs? e)
        {
            TableauDeSigne.StackPanel_Row.Children.RemoveRange(1, TableauDeSigne.StackPanel_Row.Children.Count - 1);
            GlobalVariable.TableColumns.Clear();

            button_AjoutTableauVariation.Content = "Ajouter le tableau de variation";
            button_AjoutLigneConcluante.Content = "Ajouter la ligne concluante";
            button_AjoutTableauVariation.IsEnabled = false;
            button_AjoutLigneConcluante.IsEnabled = false;
            var headerRow = (MainWindow.TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>()
                                                        .First(x => x.RowType == RowType.HEADER));

            GlobalVariable.TableColumns.Clear();
            headerRow.UC_borneMin.textBox_clear.Text = @"-\infty";
            headerRow.UC_borneMax.textBox_clear.Text = @"+\infty";

            if (e != null)
            {
                formulaTextBox_content.formulaControl_formatted.Formula = "ecrire\\:une\\:fonction";
                formulaTextBox_tabDeVariation.formulaControl_formatted.Formula = "ecrire\\:une\\:formule";
                formulaTextBox_fName.textBox_clear.Text = "f(x)";
                formulaBox_y.textBox_clear.Text = "0";
                GlobalVariable.VariableName = 'x';
            }

            GlobalVariable.UpdateBoard();
        }

        /// <summary>
        /// Actualise le tableau
        /// </summary>
        internal void button_RefreshBoard_Click(object sender, RoutedEventArgs? e)
        {
            GlobalVariable.UpdateColumn();
        }

        /// <summary>
        /// Simplifie l'expression dans la formulaBox de fonction
        /// </summary>
        private void Button_Simplify_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Entity expr = Clipboard.GetText().ToEntity();
                Entity result = expr.Simplify();
                SetCopiedFormulaTextBox(result.ToString());
            }
            catch
            {
                // Simplification impossible
                ShowQuickMessage("Simplification impossible, vérifiez la formule dans votre presse-papiers");
            }
        }

        /// <summary>
        /// Dérive l'expression dans la formulaBox de fonction
        /// </summary>
        private void Button_Derive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Entity expr = Clipboard.GetText().ToEntity();
                Entity result = expr.Differentiate(Char.ToString(GlobalVariable.VariableName)).Simplify();
                SetCopiedFormulaTextBox(result.ToString());
            }
            catch
            {
                // Dérivation impossible
                ShowQuickMessage("Dérivation impossible, vérifiez la formule dans votre presse-papiers");
            }
        }

        /// <summary>
        /// Affiche et set un texte à la formula textbox permettant de copier les résulats de la dérivation,
        /// factorisation etc
        /// </summary>
        /// <param name="result"></param>
        private void SetCopiedFormulaTextBox(string result)
        {
            textBox_copiedFormula.Text = result;
            Grid_copiedFormula.Visibility = Visibility.Visible;
            textBox_copiedFormula.Focus();
            textBox_copiedFormula.SelectAll();
        }

        /// <summary>
        /// Primitive l'expression dans la formulaBox de fonction
        /// </summary>
        private void Button_Primitive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Entity expr = Clipboard.GetText().ToEntity();
                Entity result = expr.Integrate(Char.ToString(GlobalVariable.VariableName));
                if(result is not Integralf)
                    SetCopiedFormulaTextBox(result.Simplify().ToString());
            }
            catch
            {
                // Primitive impossible
                ShowQuickMessage("Primitive impossible, vérifiez la formule dans votre presse-papiers");
            }
        }        
        
        /// <summary>
        /// Factorise l'expression dans la formulaBox de fonction
        /// </summary>
        private void Button_Factorize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Entity expr =   Clipboard.GetText().ToEntity();
                Entity result = expr.Factorize();
                SetCopiedFormulaTextBox(result.ToString());
            }
            catch
            {
                // Factorisation impossible
                ShowQuickMessage("Factorisation impossible, vérifiez la formule dans votre presse-papiers");
            }
        }

        /// <summary>
        /// Affiche la page d'information
        /// </summary>
        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Grid_Information.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Lien hypertext cliqué
        /// </summary>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var ps = new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }

        /// <summary>
        /// Stop trigger de la textbox pour copier une formule
        /// </summary>
        private void textBox_copiedFormula_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus is Button)
            {
                e.Handled = true; // Empêche la perte de focus
            }
            else
            {
                Grid_copiedFormula.Visibility = Visibility.Hidden;
            }
        }


        /// <summary>
        /// Stop trigger de la textbox pour copier une formule
        /// </summary>
        private void textBox_copiedFormula_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Escape)
            {
                Grid_copiedFormula.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Affiche la page pour envoyer un commentaire
        /// </summary>
        private void Hyperlink_RequestNavigate_SendSuggestion(object sender, RequestNavigateEventArgs e)
        {
            Grid_SendSuggestion.Visibility = Grid_SendSuggestion.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// M'envoie un commentaire
        /// </summary>
        private async void Button_SendComment_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TextBox_nom.Text))
            {
                TextBox_nom.BorderBrush = Brushes.Red;
                return;
            }
            else
            {
                TextBox_nom.ClearValue(TextBox.BorderBrushProperty);
            }

            string richText = new TextRange(richtextbox_commentaire.Document.ContentStart, richtextbox_commentaire.Document.ContentEnd).Text;
            if (String.IsNullOrEmpty(richText) || richText == "\r\n")
            {
                richtextbox_commentaire.BorderBrush = Brushes.Red;
                return;
            }
            else
            {
                richtextbox_commentaire.ClearValue(RichTextBox.BorderBrushProperty);
            }

            label_thanks.Visibility = Visibility.Visible;

            var url = $"{BASE_URL}/api/Software/send-me-a-message?software=SignaMath&email=" + TextBox_nom.Text + "&message="+ richText;
            using var httpClient = new HttpClient(new HttpClientHandler() { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; } });
            var response = await httpClient.PostAsync(url, new StringContent(""));

            if (response.StatusCode != HttpStatusCode.OK)
            {
                label_thanks.Content = "Désolé, un problème est survenu lors de l'envoi. Merci de l'envoyer manuellement à zonedetec@gmail.com !";
            }
        }

        /// <summary>
        /// L'utilisateur ne veut pas appuyer sur entrée pour valider / live writing
        /// </summary>
        private void CheckBox_LivreWriting_CheckChanged(object sender, RoutedEventArgs e)
        {
            GlobalVariable.OnlyEnter = !((CheckBox)sender).IsChecked!.Value;
        }

        /// <summary>
        /// Copie la formule à laquelle une opération vient d'être effectuée
        /// </summary>
        private void Button_CopyFormula_Click(object sender, RoutedEventArgs e)
        {
            ShowQuickMessage("Copié !");
            Clipboard.SetText(textBox_copiedFormula.Text);
            textBox_copiedFormula.Focus();
            Grid_copiedFormula.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Affiche un message dans l'infobulle en haut de l'écran pour 2 secondes
        /// </summary>
        private void ShowQuickMessage(string msg)
        {
            border_infoBulle.Visibility = Visibility.Visible;
            textBlock_InfoBulle.Text = msg;

            Timer t = new Timer(2000);
            t.Elapsed += (s,e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    border_infoBulle.Visibility = Visibility.Hidden;
                });
            };
            t.Start();
        }

        /// <summary>
        /// Affiche la zone de texte libre
        /// </summary>
        private void Label_ShowFreeTextZone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetCopiedFormulaTextBox(textBox_copiedFormula.Text);
        }

        /// <summary>
        /// Ajoute une ligne au tableau
        /// </summary>
        private void MenuItem_AddRow_Click(object sender, RoutedEventArgs e)
        {
            Button_AddRow_Click(this, null);
        }

        /// <summary>
        /// Ajoute une ligne de valeur interdite au tableau
        /// </summary>
        private void MenuItem_AddForbiddenValueRow_Click(object sender, RoutedEventArgs e)
        {
            Button_AddForbiddenValueRow_Click(this, null);
        }

        /// <summary>
        /// Ajoute une ligne concluante
        /// </summary>
        private void MenuItem_AddConclusionRow_Click(object sender, RoutedEventArgs e)
        {
            Button_AjoutLigneConcluante_Click(this, null);
        }

        /// <summary>
        /// Ajoute le tableau de variation
        /// </summary>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Button_AjoutTableauVariation_Click(this, null);
        }
    }
}
