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
    public partial class RenameToolWindow : Window
    {
        public RenameToolWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
        
        private string GetNewName(string originName)
        {
            string prefix = text_prefix.Text;
            string maintain = text_maintain.Text;
            string newMaintain = text_newMaintain.Text;
            string suffix = text_suffix.Text;

            string result = "";
            if (maintain.Length == 0)
            {
                result = originName;
            }
            else
            {
                string[] reResult = Regex.Split(originName, maintain);
                if (reResult.Length == 1)
                {
                    return originName;
                }
                result = newMaintain;
                for (int i = 1; i < reResult.Length; i++)
                {
                    result += reResult[i];
                }
            }

            if (!result.StartsWith(prefix))
            {
                result = prefix + result;
            }
            if (!result.EndsWith(suffix))
            {
                result += suffix;
            }
            return result;
        }

        private void RenameSingle(object sender, RoutedEventArgs e)
        {
            MFnDependencyNode node = new MFnDependencyNode(BasicFunc.GetSelectedObject(0));
            node.setName(GetNewName(node.name));

        }

        private void RenameHierachy(object sender, RoutedEventArgs e)
        {
            BasicFunc.IterateSelectedDags((dag) =>
            {
                List<MDagPath> dags = BasicFunc.GetHierachyAll(dag);
                foreach (MDagPath d in dags)
                {
                    MFnDependencyNode node = new MFnDependencyNode(d.node);
                    node.setName(GetNewName(node.name));
                }
            });
        }

        private void RenameSelected(object sender, RoutedEventArgs e)
        {
            BasicFunc.IterateSelectedObjects((mo) =>
            {
                MFnDependencyNode node = new MFnDependencyNode(mo);
                node.setName(GetNewName(node.name));
            });
        }
    }
}
