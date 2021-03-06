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

namespace DeviceCirculationSystem.view
{
    /// <summary>
    /// KyCombox.xaml 的交互逻辑
    /// </summary>
    public partial class KyCombox : UserControl
    {
        private string _hintLalelShow = "label";

        public string text
        {
            get { return comboBox.Text; }
            set { comboBox.Text = value; }
        }

        public string labelText
        {
            get { return label.Content.ToString(); }
            set
            {
                _hintLalelShow = value;
                label.Content = value;
            }
        }

        public KyCombox()
        {
            InitializeComponent();
        }

        public void addItem(string msg)
        {
            ComboBoxItem comboBoxItem = new ComboBoxItem();
            comboBoxItem.Content = msg;
            comboBox.Items.Add(comboBoxItem);
        }

        private void comboBox_MouseEnter(object sender, MouseEventArgs e)
        {
        }

        private void comboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if (comboBox.Text.Equals(""))
            {
                label.Content = _hintLalelShow;
            }
            else
            {
                label.Content = "";
            }
        }

        private void comboBox_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void comboBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (comboBox.Text.Equals(""))
            {
                label.Content = _hintLalelShow;
            }
            else
            {
                label.Content = "";
            }
        }
    }
}