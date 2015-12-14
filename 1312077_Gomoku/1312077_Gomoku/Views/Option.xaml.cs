using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace _1312077_Gomoku.Views
{
    /// <summary>
    /// Interaction logic for Option.xaml
    /// </summary>
    public partial class Option : Window
    {
        public Option()
        {
            InitializeComponent();           
        }

        public int m_option = 0;        

        private void PvsP_Click(object sender, RoutedEventArgs e)
        {
            m_option = 1;
            DialogResult = true;
        }

        private void PvsAI_Click(object sender, RoutedEventArgs e)
        {
            m_option = 2;
            DialogResult = true;
        }

        private void P_Online_Click(object sender, RoutedEventArgs e)
        {
            m_option = 3;
            DialogResult = true;
        }

        private void AI_Online_Click(object sender, RoutedEventArgs e)
        {
            m_option = 4;
            DialogResult = true;
        }
    }
}
