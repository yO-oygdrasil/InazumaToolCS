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
                
        private void NumericPreview(object sender, TextCompositionEventArgs e)
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
        List<Slider> sliders = new List<Slider>();
        //List<StackPanel> stackPanels = new List<StackPanel>();
        //class JointPanel
        //{
        //    public MFnIkJoint joint;
        //    public StackPanel stackPanel;
        //    public Button button;
        //    public float current;
        //    public JointPanel last = null, next = null;
        //    public JointPanel(MFnIkJoint joint)
        //    {
                
        //    }

        //}
        //List<JointPanel> jointPanels = new List<JointPanel>();


        private void AddJoints(object sender, RoutedEventArgs e)
        {
            

        }
        
        private void LoadJoints(object sender, RoutedEventArgs e)
        {

            joints.Clear();
            //stackPanels.Clear();
            grid_jointSliders.Children.Clear();
            sliders.Clear();

            BasicFunc.IterateSelectedDags((dag) =>
            {
                joints.Add(new MFnIkJoint(dag));
            }, MFn.Type.kJoint);

            //<StackPanel Orientation="Horizontal">
            //    <Button Style="{StaticResource NormalButton}" Width="80" Height="40" Click="LoadJoints">Joint 0</Button>
            //    <Slider Style="{StaticResource NormalSlider}" Name="test"/>
            //</StackPanel>

            MVector lastJointWorldPos = joints[joints.Count - 1].getTranslation(MSpace.Space.kWorld);
            MVector firstJointWorldPos = joints[0].getTranslation(MSpace.Space.kWorld);
            double totalLength = (lastJointWorldPos - firstJointWorldPos).length;
            for (int i = 0; i < joints.Count; i++)
            {
                StackPanel panel = new StackPanel { Orientation = Orientation.Horizontal };
                Button btn = new Button { Style = this.FindResource("NormalButton") as Style, Width = 80, Height = 40, Name = "jointBtn_" + i,Content=joints[i].absoluteName };
                int index = i;//local variable is important
                btn.Click += new RoutedEventHandler((obj, ea) =>
                {
                    //choose
                    BasicFunc.Select(joints[index].dagPath);
                });
                panel.Children.Add(btn);

                if (i != 0 && i != joints.Count - 1)
                {
                    MVector jointWorldPos = joints[i].getTranslation(MSpace.Space.kWorld);
                    Slider newSlider = new Slider { Style = this.FindResource("NormalSlider") as Style, Name = "jointSlider_" + i };
                    newSlider.Value = (jointWorldPos - firstJointWorldPos).length / totalLength;
                    //newSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>((obj, rpce) =>
                    //{
                    //     .NewValue
                    //});
                    panel.Children.Add(newSlider);
                    sliders.Add(newSlider);
                }                
                else
                {
                    sliders.Add(null);
                }
                grid_jointSliders.Children.Add(panel);
            }


            //newSlider.
        }

        private void OrientFix(object sender, RoutedEventArgs e)
        {
            if (joints == null && joints.Count == 0)
            {
                return;
            }
            if (joints.Count == 1)
            {
                //set for the one
                
            }
            MVector lastJointWorldPos = joints[joints.Count - 1].getTranslation(MSpace.Space.kWorld);
            MVector firstJointWorldPos = joints[0].getTranslation(MSpace.Space.kWorld);
            string[] lines = System.IO.File.ReadAllLines("D:\temp\testValue.txt");
            float valueX = float.Parse(lines[0]);
            float valueY = float.Parse(lines[1]);
            float valueZ = float.Parse(lines[2]);

            for (int i = 0; i < joints.Count - 1; i++)
            {
                //joints[i].setOrientation(new MEulerRotation(lastJointWorldPos - firstJointWorldPos));
                joints[i].setOrientation(new MEulerRotation(valueX, valueY, valueZ));
            }
        }

        private void UpdateJoints(object sender, RoutedEventArgs e)
        {
            if (joints == null && joints.Count < 2)
            {
                return;
            }
            MFnIkJoint lastJoint = joints[joints.Count - 1];
            MVector lastJointWorldPos = lastJoint.getTranslation(MSpace.Space.kWorld);
            MVector firstJointWorldPos = joints[0].getTranslation(MSpace.Space.kWorld);
            MVector direct = lastJointWorldPos - firstJointWorldPos;
            for (int i = 1; i < joints.Count - 1; i++)
            {
                if (i >= sliders.Count || sliders[i] == null)
                {
                    
                    continue;
                }
                double percent = sliders[i].Value;
                JointProcess.MoveSkinJointsTool(joints[i].dagPath);
                joints[i].setTranslation(direct * percent + firstJointWorldPos, MSpace.Space.kWorld);
            }
            JointProcess.MoveSkinJointsTool(lastJoint.dagPath);
            lastJoint.setTranslation(lastJointWorldPos, MSpace.Space.kWorld);
        }

        private void ClearJoints(object sender, RoutedEventArgs e)
        {
            joints.Clear();
            //stackPanels.Clear();
            grid_jointSliders.Children.Clear();
            sliders.Clear();
        }
    }
}
