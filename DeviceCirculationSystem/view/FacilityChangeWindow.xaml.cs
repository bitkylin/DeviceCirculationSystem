using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using DeviceCirculationSystem.bean;
using DeviceCirculationSystem.bean.@enum;
using DeviceCirculationSystem.Util;

namespace DeviceCirculationSystem.view
{
    /// <summary>
    /// FacilityChangeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FacilityChangeWindow : Window
    {
        private readonly DeviceStatus _status;
        private readonly Facility _facility;

        public FacilityChangeWindow(Facility facility)
        {
            InitializeComponent();
            _status = facility.Status;
            _facility = facility;
            facility.DateTime = DateTime.Now;
            SetCommonContent(facility);
            switch (_status)
            {
                case DeviceStatus.Input:
                    SetInputContent();
                    break;
                case DeviceStatus.Output:
                    SetOutputContent();
                    break;
            }
        }

        private void SetCommonContent(Facility facility)
        {
            TextBoxId.Text = facility.Id;
            TextBoxModelNum.Text = facility.ModelNum;
            TextBoxName.Text = facility.Name;
            TextBoxParameter.Text = facility.Parameter;
            TextBoxPrice.Text = facility.Price.ToString(CultureInfo.CurrentCulture);
            TextBoxDateTime.Text = facility.DateTime.ToString(CultureInfo.CurrentCulture);
            TextBoxNote.Text = facility.Note;
            TextBoxUser.Text = facility.ToUser;


            TextBoxId.IsEnabled = false;
            TextBoxModelNum.IsEnabled = false;
            TextBoxName.IsEnabled = false;
            TextBoxParameter.IsEnabled = false;
            TextBoxPrice.IsEnabled = false;
            TextBoxUser.IsEnabled = false;
            TextBoxDateTime.IsEnabled = false;
        }

        private void SetOutputContent()
        {
            LabelOptContent.Content = "设备借出";
            BtnConfirm.Content = "借出确认";
        }

        private void SetInputContent()
        {
            LabelOptContent.Content = "设备归还";
            BtnConfirm.Content = "归还确认";
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            //判断“单价”和“数量”输入的合法性
            if (!(IsInt(TextBoxNum.Text) && int.Parse(TextBoxNum.Text) > 0))
            {
                MessageBox.Show("输入数字有误，请检查并重新输入！", "提示");
            }
            else if (TextBoxNote.Text.Length >= 50)
            {
                MessageBox.Show("备注框中的文本量需小于50个字符！", "提示");
            }
            else
            {
                _facility.Num = int.Parse(TextBoxNum.Text);
                _facility.Note = TextBoxNote.Text;
                try
                {
                    switch (_status)
                    {
                        case DeviceStatus.Input:
                            BitkyMySql.GotoDatebaseOnFacility(_facility);
                            break;
                        case DeviceStatus.Output:
                            BitkyMySql.GoOutFromDatebaseOnFacility(_facility);
                            break;
                    }

                    Close();
                    switch (_status)
                    {
                        case DeviceStatus.Input:
                            MessageBox.Show("归还成功！请查询最新库存信息！", "提示");
                            break;
                        case DeviceStatus.Output:
                            MessageBox.Show("借出成功！请查询最新库存信息！", "提示");
                            break;
                    }
                }
                catch (NumBelowZeroException)
                {
                    MessageBox.Show("请数入正确的数量!", "提示");
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 判断是正浮点数或正整数
        /// </summary>
        /// <param name="value">待匹配的文本</param>
        /// <returns>匹配结果</returns>
        public static bool IsNumeric(string value)
        {
            value = value.Trim();
            return Regex.IsMatch(value, @"^[1-9]\d*\.\d*|0\.\d*[1-9]\d*|[1-9]\d*$");
        }

        /// <summary>
        /// 判断是正整数
        /// </summary>
        /// <param name="value">待匹配的文本</param>
        /// <returns>匹配结果</returns>
        public static bool IsInt(string value) //判断是正整数
        {
            value = value.Trim();
            return Regex.IsMatch(value, @"^[1-9]\d*$");
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}