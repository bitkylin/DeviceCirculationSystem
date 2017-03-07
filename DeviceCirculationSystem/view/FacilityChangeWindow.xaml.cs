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
        private IMainView _view;

        public FacilityChangeWindow(Facility facility, IMainView view)
        {
            InitializeComponent();
            _status = facility.status;
            _facility = facility;
            _view = view;
            facility.dateTime = DateTime.Now;
            initWidgetStatus(facility);
            switch (_status)
            {
                case DeviceStatus.RETURN:
                    setReturnContent();
                    break;
                case DeviceStatus.LOAN:
                    setLoanContent();
                    break;
                case DeviceStatus.INPUT:
                    setInputContent();
                    break;
                case DeviceStatus.OUTPUT:
                    setOutputContent();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void initWidgetStatus(Facility facility)
        {
            if (facility.id != null)
                TextBoxId.Text = facility.id;
            if (facility.modelNum != null)
                TextBoxModelNum.Text = facility.modelNum;
            if (facility.category != null)
                TextBoxCategory.Text = facility.category;
            if (facility.name != null)
                TextBoxName.Text = facility.name;
            if (facility.parameter != null)
                TextBoxParameter.Text = facility.parameter;
            if (facility.price != null)
                TextBoxPrice.Text = facility.price.ToString(CultureInfo.CurrentCulture);
            TextBoxUser.Text = facility.toUser;
            TextBoxDateTime.Text = facility.dateTime.ToString(CultureInfo.CurrentCulture);

            TextBoxId.IsEnabled = false;
            TextBoxUser.IsEnabled = false;
            TextBoxDateTime.IsEnabled = false;
            switch (_status)
            {
                case DeviceStatus.RETURN:
                case DeviceStatus.LOAN:
                case DeviceStatus.OUTPUT:
                    TextBoxId.IsEnabled = false;
                    TextBoxModelNum.IsEnabled = false;
                    TextBoxName.IsEnabled = false;
                    TextBoxCategory.IsEnabled = false;
                    TextBoxParameter.IsEnabled = false;
                    TextBoxPrice.IsEnabled = false;
                    break;
                case DeviceStatus.INPUT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void setLoanContent()
        {
            LabelOptContent.Content = "设备借出";
            BtnConfirm.Content = "借出确认";
        }

        private void setInputContent()
        {
            LabelOptContent.Content = "设备入库";
            BtnConfirm.Content = "入库确认";
        }

        private void setOutputContent()
        {
            LabelOptContent.Content = "设备出库";
            BtnConfirm.Content = "出库确认";
        }

        private void setReturnContent()
        {
            LabelOptContent.Content = "设备归还";
            BtnConfirm.Content = "归还确认";
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            //判断“单价”和“数量”输入的合法性
            if (!(isInt(TextBoxNum.Text) && (int.Parse(TextBoxNum.Text.Trim()) > 0)))
            {
                MessageBox.Show("输入的数量有误，请检查并重新输入！", "提示");
                return;
            }
            if (TextBoxNote.Text.Length >= 50)
            {
                MessageBox.Show("备注框中的文本量需小于50个字符！", "提示");
                return;
            }
            if (!(isNumeric(TextBoxPrice.Text) && (double.Parse(TextBoxPrice.Text.Trim()) > 0)))
            {
                MessageBox.Show("输入的价格有误，请检查并重新输入！", "提示");
                return;
            }

            _facility.num = int.Parse(TextBoxNum.Text);
            _facility.note = TextBoxNote.Text;

            if (_status == DeviceStatus.INPUT)
            {
                _facility.id = TextBoxId.Text.Trim();
                _facility.category = TextBoxCategory.Text.Trim();
                _facility.name = TextBoxName.Text.Trim();
                _facility.modelNum = TextBoxModelNum.Text.Trim();
                _facility.price = double.Parse(TextBoxPrice.Text.Trim());
                _facility.parameter = TextBoxParameter.Text.Trim();
            }
            try
            {
                switch (_status)
                {
                    case DeviceStatus.RETURN:
                        KyMySql.returnFacilityToRepository(_facility);
                        break;
                    case DeviceStatus.LOAN:
                        KyMySql.loanFacilityFromRepository(_facility);
                        break;
                    case DeviceStatus.INPUT:
                        if (MessageBox.Show("您要入库该设备(器件)吗？入库成功后将直接添加到实验室库存！", "提示", MessageBoxButton.OKCancel) ==
                            MessageBoxResult.Cancel)
                            return;
                        KyMySql.facilityInputToRepository(_facility);
                        break;
                    case DeviceStatus.OUTPUT:
                        if (MessageBox.Show("您要出库该设备(器件)吗？出库成功后该设备(器件)将从库存中移除！", "提示", MessageBoxButton.OKCancel) ==
                            MessageBoxResult.Cancel)
                            return;
                        KyMySql.facilityOutputFromRepository(_facility);
                        break;
                }

                Close();
                switch (_status)
                {
                    case DeviceStatus.RETURN:
                        MessageBox.Show("归还成功！已刷新最新的库存信息！", "提示");
                        break;
                    case DeviceStatus.LOAN:
                        MessageBox.Show("借出成功！已刷新最新的库存信息！", "提示");
                        break;
                    case DeviceStatus.INPUT:
                        MessageBox.Show("入库成功！已刷新最新的库存信息！", "提示");
                        break;
                    case DeviceStatus.OUTPUT:
                        MessageBox.Show("出库成功！已刷新最新的库存信息！", "提示");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                _view.refreshQueryStorage();
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
        public static bool isNumeric(string value)
        {
            value = value.Trim();
            return Regex.IsMatch(value, @"^[1-9]\d*\.\d*$|^0\.\d*[1-9]\d*$|^[1-9]\d*$|^0$");
        }

        /// <summary>
        ///     判断是正整数
        /// </summary>
        /// <param name="value">待匹配的文本</param>
        /// <returns>匹配结果</returns>
        public static bool isInt(string value) //判断是正整数
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