using MathExpressions.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace MathExpressions
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Коллекция формул для отображения
        /// </summary>
        public ObservableCollection<Formula> Formulas { get; } = new ObservableCollection<Formula>();

        #region SelectedFormula Property
        private Formula selectedFormula;
                        
        public Formula SelectedFormula
        {
            get { return selectedFormula; }
            set
            {
                selectedFormula = value;
                OnPropertyChanged("SelectedFormula");
            }
        }
        #endregion
        

        public MainWindow()
        {
            InitializeComponent();

            UpdateFormulasOnDisplay();

            DataContext = this;
        }

        /// <summary>
        /// Обновляет список формул на экране из БД
        /// </summary>
        private void UpdateFormulasOnDisplay()
        {
            var formulas = SqliteDataAccess.LoadFormulas();

            Formulas.Clear();

            foreach (var formula in formulas)
            {
                Formulas.Add(formula);
            }

            Result_TextBox.Text = "";
        }
        

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFormula != null)
            {
                Result_TextBox.Text = "";
                try
                {
                    var result = SelectedFormula.Solve();
                    Result_TextBox.Text = result.ToString();
                }
                catch (DivideByZeroException ex)
                {
                    MessageBox.Show(ex.Message,
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (InvalidOperationException) 
                {
                    // Срабатывает, если стек, используемый для вычисления формулы, пустой и была попытка удалить объект из него
                    MessageBox.Show("Ошибка. Проверьте корректность формулы",
                       "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void AddFormulaButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputFormulaDialog();
            if (dialog.ShowDialog() == true)
            {
                string expression = dialog.FormulaText;

                // Проверим реуглярным выражением введенные символы
                bool isValid = Regex.IsMatch(expression, @"^[a-zA-Z\d\s\+\-\/\*\(\)]+$");

                if (!isValid)
                {
                    MessageBox.Show("В формуле обнаружены недопустимые символы!", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверим правильность расстановки скобок
                if (!IsBracketsCorrect(expression))
                {
                    MessageBox.Show("В формуле обнаружены ошибки, проверьте скобки!",
                                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                
                try
                {
                    var formula = new Formula(expression);                    

                    // Сохраним в базу данных результат
                    SqliteDataAccess.AddFormula(formula);

                    // Обновим данные на экране
                    UpdateFormulasOnDisplay();
                }
                catch (System.InvalidOperationException)
                {
                    MessageBox.Show("В формуле обнаружены ошибки, проверьте скобки!", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Проверка на корректную расстановку скобок
        /// </summary>        
        private static bool IsBracketsCorrect(string expression)
        {
            bool isCorrect = true;
            Stack<char> stack = new Stack<char>();

            foreach (var item in expression.ToCharArray())
            {
                if (item == '(')
                {
                    stack.Push(item);
                }
                else if (item == ')')
                {
                    if (stack.Count == 0)
                    {
                        isCorrect = false;
                        break;
                    }                                     
                    
                    stack.Pop();                    
                }
            }

            if (stack.Count > 0)
            {
                isCorrect = false;
            }

            return isCorrect;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
