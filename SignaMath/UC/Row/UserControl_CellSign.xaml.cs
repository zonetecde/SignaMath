using SignaMath.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SignaMath.UC
{
    /// <summary>
    /// Logique d'interaction pour UserControl_CellSign.xaml
    /// </summary>
    public partial class UserControl_CellSign : UserControl
    {
        public UserControl_CellSign(bool isLastRow, RowType rowType)
        {
            InitializeComponent();

            Border_Main.BorderThickness = !isLastRow ? new Thickness(0, 0, 2, 0) : new Thickness(0);
            RowType = rowType;

            FormulaTextBoxLeft.AllowEmpty = true;
            FormulaTextBoxLeft.textBox_clear.Text = string.Empty;
            FormulaTextBoxRight.textBox_clear.Text = string.Empty;
            FormulaTextBoxRight.AllowEmpty = true;
        }

        public RowType RowType { get; }

        private void Label_Sign_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Label_Sign.Content = Label_Sign.Content.ToString() == "+" ? '-' : '+';

            if(RowType == RowType.CONCLUANTE)
            {
                // Met à jour le tableau de variation
                MainWindow.TableauDeSigne.StackPanel_Row.Children.OfType<UserControl_Row>().ToList().FindAll(x => x.RowType == RowType.TABLEAU_DE_VARIATION).ForEach(x => { x.UpdateRow(); });
            }
        }
    }
}
