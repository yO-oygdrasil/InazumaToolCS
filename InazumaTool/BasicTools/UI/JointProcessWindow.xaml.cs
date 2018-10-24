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

using Autodesk.Maya.OpenMayaAnim;
using Autodesk.Maya;
using Autodesk.Maya.OpenMaya;
using System.Text.RegularExpressions;

namespace InazumaTool.BasicTools.UI
{
    /// <summary>
    /// Interaction logic for test.xaml
    /// </summary>
    public partial class JointProcessWindow : Window
    {
        public JointProcessWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
                
        private void numericPreview(object sender, TextCompositionEventArgs e)
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

        List<MFnIkJoint> joints = new List<MFnIkJoint>(); 


        private void AddJoints(object sender, RoutedEventArgs e)
        {
            BasicFunc.IterateSelectedDags((dag) =>
            {
                joints.Add(new MFnIkJoint(dag));
            }, MFn.Type.kJoint);



            Slider newSlider = new Slider { Style = this.FindResource("NormalSlider") as Style, Name= "joints};
            
            //newSlider.

            grid_jointSliders.Children.Add(new Label { Content = "Label" });
        }



    }
}
