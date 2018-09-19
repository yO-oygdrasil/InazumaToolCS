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

using Autodesk.Maya;
using Autodesk.Maya.OpenMaya;

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

        private void LogSth(object sender, RoutedEventArgs e)
        {
            Debug.Log("yeah! wpf work");
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
    }
}
