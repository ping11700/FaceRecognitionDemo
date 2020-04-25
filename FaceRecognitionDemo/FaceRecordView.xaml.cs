using Camera_NET;
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

namespace FaceRecognitionDemo
{
    /// <summary>
    /// FaceRecordView.xaml 的交互逻辑
    /// </summary>
    public partial class FaceRecordView : UserControl
    {
        public FaceRecordView()
        {
            InitializeComponent();
        }
        FaceRecordVM mv { get { return this.DataContext as FaceRecordVM; } }


        /// <summary>
        /// 编辑 事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _editBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_dataGrid.IsReadOnly)
            {
                this._dataGrid.IsReadOnly = false;
                _editBtn.Content = "锁定";
            }
            else {
                this._dataGrid.IsReadOnly = true; ;
                _editBtn.Content = "编辑";
            }
        }
        /// <summary>
        /// 删除 事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _deletBtn_Click(object sender, RoutedEventArgs e)
        {
            mv.PIVMList.RemoveAt(_dataGrid.SelectedIndex) ;
        }

        /// <summary>
        /// 清空 事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _clearBtn_Click(object sender, RoutedEventArgs e)
        {
            mv.PIVMList.Clear();

            mv.IsEditBtnEnable = false;
            mv.IsDeletBtnEnable = false;
            mv.IsClearBtnEnable = false;
        }

        private void DataGrid_Selected(object sender, RoutedEventArgs e)
        {

        }
    }
}
