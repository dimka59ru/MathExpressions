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
using System.Windows.Shapes;

namespace MathExpressions
{
    /// <summary>
    /// Логика взаимодействия для InputFormulaDialog.xaml
    /// </summary>
    public partial class InputFormulaDialog : Window
    {
        public InputFormulaDialog()
        {
            InitializeComponent();
        }

        public string FormulaText
        {
            get { return FormulaTextBox.Text; }
            set { FormulaTextBox.Text = value; }
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
