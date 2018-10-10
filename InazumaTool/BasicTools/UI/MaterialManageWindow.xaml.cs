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
using System.Text.RegularExpressions;

namespace InazumaTool.BasicTools.UI
{
    /// <summary>
    /// Interaction logic for test.xaml
    /// </summary>
    public partial class MaterialManageWindow : Window
    {
        public MaterialManageWindow()
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

        private void ConvertToRSMats(object sender, RoutedEventArgs e)
        {
            MaterialManage.ConvertToRSMaterial(new MFnDependencyNode(BasicFunc.GetSelectedObject(0)), (bool)bto_deleteOriginMats.IsChecked);
        }

        
    }
}
