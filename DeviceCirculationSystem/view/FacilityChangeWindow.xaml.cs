using System;
using System.Diagnostics;
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
    ///     FacilityChangeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FacilityChangeWindow
    {
        private readonly Facility _facility;
        private readonly DeviceStatus _status;

        public FacilityChangeWindow(Facility facility)
        {
            InitializeComponent();
            _status = facility.Status;
            _facility = facility;
            facility.DateTime = DateTime.Now;
            InitWidgetStatus(facility);
            switch (_status)
            {
                case DeviceStatus.Return:
                    SetReturnContent();
                    break;
                case DeviceStatus.Loan:
                    SetLoanContent();
                    break;
                case DeviceStatus.Input:
                    SetInputContent();
                    break;
                case DeviceStatus.Output:
                    SetOutputContent();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void InitWidgetStatus(Facility facility)
        {
            if (facility.Id != null)
                TextBoxId.Text = facility.Id;
            if (facility.ModelNum != null)
                TextBoxModelNum.Text = facility.ModelNum;
            if (facility.Category != null)
                TextBoxCategory.Text = facility.Category;
            if (facility.Name != null)
                TextBoxName.Text = facility.Name;
            if (facility.Parameter != null)
                TextBoxParameter.Text = facility.Parameter;
            if (facility.Price != null)
                TextBoxPrice.Text = facility.Price.ToString(CultureInfo.CurrentCulture);
            TextBoxUser.Text = facility.ToUser;
            TextBoxDateTime.Text = facility.DateTime.ToString(CultureInfo.CurrentCulture);

            TextBoxId.IsEnabled = false;
            TextBoxUser.IsEnabled = false;
            TextBoxDateTime.IsEnabled = false;
            switch (_status)
            {
                case DeviceStatus.Return:
                case DeviceStatus.Loan:
                case DeviceStatus.Output:
                    TextBoxId.IsEnabled = false;
                    TextBoxModelNum.IsEnabled = false;
                    TextBoxName.IsEnabled = false;
                    TextBoxCategory.IsEnabled = false;
                    TextBoxParameter.IsEnabled = false;
                    TextBoxPrice.IsEnabled = false;
                    break;
                case DeviceStatus.Input:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetLoanContent()
        {
            LabelOptContent.Content = "设备借出";
            BtnConfirm.Content = "借出确认";
        }

        private void SetInputContent()
        {
            LabelOptContent.Content = "设备入库";
            BtnConfirm.Content = "入库确认";
        }

        private void SetOutputContent()
        {
            LabelOptContent.Content = "设备出库";
            BtnConfirm.Content = "出库确认";
        }

        private void SetReturnContent()
        {
            LabelOptContent.Content = "设备归还";
            BtnConfirm.Content = "归还确认";
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            //判断“单价”和“数量”输入的合法性
            if (!(IsInt(TextBoxNum.Text) && (int.Parse(TextBoxNum.Text.Trim()) > 0)))
            {
                MessageBox.Show("输入的数量有误，请检查并重新输入！", "提示");
                return;
            }
            if (TextBoxNote.Text.Length >= 50)
            {
                MessageBox.Show("备注框中的文本量需小于50个字符！", "提示");
                return;
            }
            if (!(IsNumeric(TextBoxPrice.Text) && (double.Parse(TextBoxPrice.Text.Trim()) > 0)))
            {
                MessageBox.Show("输入的价格有误，请检查并重新输入！", "提示");
                return;
            }

            _facility.Num = int.Parse(TextBoxNum.Text);
            _facility.Note = TextBoxNote.Text;

            if (_status == DeviceStatus.Input)
            {
                _facility.Id = TextBoxId.Text.Trim();
                _facility.Category = TextBoxCategory.Text.Trim();
                _facility.Name = TextBoxName.Text.Trim();
                _facility.ModelNum = TextBoxModelNum.Text.Trim();
                _facility.Price = double.Parse(TextBoxPrice.Text.Trim());
                _facility.Parameter = TextBoxParameter.Text.Trim();
            }
            try
            {
                switch (_status)
                {
                    case DeviceStatus.Return:
                        BitkyMySql.ReturnFacilityToRepository(_facility);
                        break;
                    case DeviceStatus.Loan:
                        BitkyMySql.LoanFacilityFromRepository(_facility);
                        break;
                    case DeviceStatus.Input:
                        if (MessageBox.Show("您要入库该设备(器件)吗？入库成功后将直接添加到实验室库存！", "提示", MessageBoxButton.OKCancel) ==
                            MessageBoxResult.Cancel)
                            return;
                        BitkyMySql.FacilityInputToRepository(_facility);
                        break;
                    case DeviceStatus.Output:
                        if (MessageBox.Show("您要出库该设备(器件)吗？出库成功后该设备(器件)将从库存中移除！", "提示", MessageBoxButton.OKCancel) ==
                            MessageBoxResult.Cancel)
                            return;
                        BitkyMySql.FacilityOutputFromRepository(_facility);
                        break;
                }

                Close();
                switch (_status)
                {
                    case DeviceStatus.Return:
                        MessageBox.Show("归还成功！请查询最新库存信息！", "提示");
                        break;
                    case DeviceStatus.Loan:
                        MessageBox.Show("借出成功！请查询最新库存信息！", "提示");
                        break;
                    case DeviceStatus.Input:
                        MessageBox.Show("入库成功！请查询最新库存信息！", "提示");
                        break;
                    case DeviceStatus.Output:
                        MessageBox.Show("出库成功！请查询最新库存信息！", "提示");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (NumBelowZeroException)
            {
                MessageBox.Show("请数入正确的数量!", "提示");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     判断是正浮点数或自然数
        /// </summary>
        /// <param name="value">待匹配的文本</param>
        /// <returns>匹配结果</returns>
        public static bool IsNumeric(string value)
        {
            value = value.Trim();
            return Regex.IsMatch(value, @"^[1-9]\d*\.\d*$|^0\.\d*[1-9]\d*$|^[1-9]\d*$|^0$");
        }

        /// <summary>
        ///     判断是正整数
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