﻿using System;
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

using Autodesk.Maya;
using Autodesk.Maya.OpenMaya;
using System.Text.RegularExpressions;

namespace InazumaTool.BasicTools.UI
{
    /// <summary>
    /// Interaction logic for test.xaml
    /// </summary>
    public partial class TestWPFWindow : Window
    {
        public TestWPFWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ConvertToUDIM(object sender, RoutedEventArgs e)
        {
            MaterialManage.CombineToUDIM(BasicFunc.GetSelectedObjectList(), text_prename.Text, text_newFolder.Text, int.Parse(text_uCount.Text));
        }

        private void AllPreset_Click(object sender, RoutedEventArgs e)
        {
            Debug.Log("yeah [all] button click!");
        }

        private void ResultGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ResultGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void text_uCount_preview(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");

            if (re.IsMatch(e.Text))
            {
                int value = int.Parse(e.Text);
                if (value > 0 && value < 10)
                {
                    e.Handled = true;
                    return;
                }
            }
            e.Handled = false;

        }
    }
}
