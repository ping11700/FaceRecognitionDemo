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
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MianWindowLoad;
            this.Closed += WindowClose;
        }


        public FaceRecognitionControl FRC { get; private set; }

        public FaceRecognizeEngine FRE { get; private set; }

        private void MianWindowLoad(object sender, RoutedEventArgs e)
        {
            FRC = new FaceRecognitionControl();

            FRE = new FaceRecognizeEngine();

            FRE.FRC = FRC;//fr control 传入至fr Engine中

            FRC.MainView = this;

            FRE.InitCamera();
          
            this._rgbVideoSource.PlayingFinished += new AForge.Video.PlayingFinishedEventHandler(FRE.VideoSource_PlayingFinished);
            this._irVideoSource.PlayingFinished += new AForge.Video.PlayingFinishedEventHandler(FRE.VideoSource_PlayingFinished);
            this._rgbVideoSource.Paint += FRE.VideoSource_Paint;
            this._irVideoSource.Paint += FRE.IrVideoSource_Paint;

            this.DataContext = FRC.MainVM;
        }

        private void WindowClose(object sender ,EventArgs e)
        {
            FRC = null;

            FRE.WindowClosed();
            FRE = null;
        }


        /// <summary>
        /// 主界面:人脸录入Btn事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _mainViewFRBtn_Click(object sender, RoutedEventArgs e)
        {
            FRC.OpenFaceRecord();
            _personInfoMainViewGrid.Visibility = Visibility.Visible;
            _mainViewPGBtn.Visibility = Visibility.Visible;
            _mainViewSIBtn.Visibility = Visibility.Visible;
            _mainViewPGIBtn.Visibility = Visibility.Collapsed;
            _mainViewSIIBtn.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 主界面:人脸识别Btn事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _mainViewFIBtn_Click(object sender, RoutedEventArgs e)
        {
            FRC.OpenFaceIdentification();
            _personInfoMainViewGrid.Visibility = Visibility.Collapsed;
            _mainViewPGBtn.Visibility = Visibility.Collapsed;
            _mainViewSIBtn.Visibility = Visibility.Collapsed;
            _mainViewPGIBtn.Visibility = Visibility.Visible;
            _mainViewSIIBtn.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 主界面:人脸查询Btn事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _mainViewFFBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 主界面:模式切换Btn事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _mainViewMSBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 启动/关闭摄像头事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _mainView_StartCamera_Btn_Click(object sender, RoutedEventArgs e)
        {
            FRE.Start_CloseVideo();
        }

        /// <summary>
        /// 拍取照片录入事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _mainViewPGBtn_Click(object sender, RoutedEventArgs e)
        {
            FRE.ImportImgesFun(false);
        }

        /// <summary>
        /// 选取照片录入(可多张照片)事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _mainViewSIBtn_Click(object sender, RoutedEventArgs e)
        {
            FRE.ImportImgesFun(true);
        }


        /// <summary>
        /// 拍取照片识别事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _mainViewPGIBtn_Click(object sender, RoutedEventArgs e)
        {
            FRE.ChooseImgeDistinguish(false);
        }

        /// <summary>
        /// 选取照片识别事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _mainViewSIIBtn_Click(object sender, RoutedEventArgs e)
        {
            FRE.ChooseImgeDistinguish(true);
        }

        /// <summary>
        /// 选择相机事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _camera_comboBox_Selected(object sender, RoutedEventArgs e)
        {
            FRE.ResetCamera();
        }


        /// <summary>
        /// 选择分辨率事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _resolutionList_comboBox_Selected(object sender, RoutedEventArgs e)
        {
            FRE.ResetCamera();
        }


        /// <summary>
        /// 改变阈值 keyDown事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _txtThreshold_KeyDown(object sender, KeyEventArgs e)
        {
            FRE.Input_Threshold_KeyDown(sender, e);
        }

        /// <summary>
        /// 改变阈值 keyUp事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _txtThreshold_KeyUp(object sender, KeyEventArgs e)
        {
            FRE.Input_Threshold_KeyUp(sender, e);
        }
    }
}
