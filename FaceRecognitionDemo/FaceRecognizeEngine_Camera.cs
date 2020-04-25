using AForge.Video.DirectShow;
using ArcSoftFace.Entity;
using ArcSoftFace.SDKModels;
using ArcSoftFace.SDKUtil;
using FaceRecognitionDemo.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceRecognitionDemo
{
    public partial class FaceRecognizeEngine
    {

        /// <summary>
        /// 视频输入设备信息
        /// </summary>
        private FilterInfoCollection FilterInfoCollection { get; set; }

        /// <summary>
        /// RGB 摄像头索引
        /// </summary>
        private int RgbCameraIndex { get; set; }
        /// <summary>
        /// IR 摄像头索引
        /// </summary>
        private int IrCameraIndex { get; set; }

        /// <summary>
        /// RGB摄像头设备
        /// </summary>
        private VideoCaptureDevice RgbDeviceVideo { get; set; }
        /// <summary>
        /// IR摄像头设备
        /// </summary>
        private VideoCaptureDevice IrDeviceVideo { get; set; }

        /// <summary>
        /// 是否是双目摄像
        /// </summary>
        private bool isDoubleShot = false;
        
        

        /// <summary>
        /// 摄像头初始化
        /// </summary>
        public void InitCamera()
        {
            FRC.MainVM.CameraList.Clear();
            FRC.MainVM.ResolutionList.Clear();

            FilterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            //如果没有可用摄像头，“启用摄像头”按钮禁用，否则使可用
            if (FilterInfoCollection.Count == 0) return;
            //添加相机
            foreach (FilterInfo fi in FilterInfoCollection)
            {
                FRC.MainVM.CameraList.Add(fi.Name);
            }
            if (FRC.MainVM.SelectedCamera == null)
                FRC.MainVM.SelectedCamera = FRC.MainVM.CameraList[0];
            int CameraIndex = FRC.MainVM.CameraList.IndexOf(
               FRC.MainVM.CameraList.SingleOrDefault(x => x == FRC.MainVM.SelectedCamera));
            VideoCaptureDevice deviceVideo = new VideoCaptureDevice(FilterInfoCollection[CameraIndex].MonikerString);
            foreach (var resolution in deviceVideo.VideoCapabilities)
            {
                FRC.MainVM.ResolutionList.Add(resolution.FrameSize.Height
                    + "*" + resolution.FrameSize.Width);
            }
            if (FRC.MainVM.SelectedResolution == null)
                FRC.MainVM.SelectedResolution = FRC.MainVM.ResolutionList[0];

            FRC.MainVM.IsCameraEnable = FilterInfoCollection.Count > 0;

            FRC.MainVM.IsPGBtnEnable = false;       //拍照录入
            FRC.MainVM.IsSIBtnEnable = true;        //选照录入
            FRC.MainVM.IsPGIBtnEnable = false;      //拍照识别
            FRC.MainVM.IsSIIBtnEnable = true;       //选照识别
        }

        /// <summary>
        /// 选择相机或分辨率时,重新设置Camera
        /// </summary>
        internal void ResetCamera()
        {

        }

        #region 视频检测相关(<摄像头按钮点击事件、摄像头Paint事件、特征比对、摄像头播放完成事件>)

        /// <summary>
        /// 启动/关闭摄像头
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Start_CloseVideo()
        {
            //在点击开始的时候再坐下初始化检测，防止程序启动时有摄像头，在点击摄像头按钮之前将摄像头拔掉的情况
            InitCamera();
            FRC.MainView._mainView_StartCamera_Btn.Content = "关闭摄像头";

            //必须保证有可用摄像头
            if (FilterInfoCollection.Count == 0)
            {
                MessageBox.Show("未检测到摄像头，请确保已安装摄像头或驱动!");
                return;
            }
            if (FRC.MainView._rgbVideoSource.IsRunning || FRC.MainView._irVideoSource.IsRunning)
            {
                FRC.MainView._mainView_StartCamera_Btn.Content = "启用摄像头";
                //关闭摄像头
                if (FRC.MainView._irVideoSource.IsRunning)
                {
                    FRC.MainView._irVideoSource.SignalToStop();
                    FRC.MainView._irVideoSource.Hide();
                    FRC.MainView._irWinFormsHost.Visibility = System.Windows.Visibility.Collapsed;
                }
                if (FRC.MainView._rgbVideoSource.IsRunning)
                {
                    FRC.MainView._rgbVideoSource.SignalToStop();
                    FRC.MainView._rgbVideoSource.Hide();
                    FRC.MainView._rgbWinFormsHost.Visibility = System.Windows.Visibility.Collapsed;
                }
                //“选择识别图”、“开始匹配”按钮可用，阈值控件禁用
                FRC.MainVM.IsPGBtnEnable = false;        //拍照录入
                FRC.MainVM.IsPGIBtnEnable = false;       //拍照识别
            }
            else
            {
                if (isCompare)
                {
                    //比对结果清除
                    for (int i = 0; i < ImagesFeatureList.Count; i++)
                    {
                        //_imageList.Items[i].Text = string.Format("{0}号", i);
                    }
                    //_lblCompareInfo.Text = "";
                    isCompare = false;
                }
                //“选择识别图”、“开始匹配”按钮禁用，阈值控件可用，显示摄像头控件
                FRC.MainVM.IsPGBtnEnable = true;        //拍照录入
                FRC.MainVM.IsPGIBtnEnable = true;       //拍照识别
                FRC.MainView._txtThreshold.IsReadOnly = false;      //可编辑阈值

                FRC.MainView._rgbWinFormsHost.Visibility = System.Windows.Visibility.Visible;
                FRC.MainView._irWinFormsHost.Visibility = System.Windows.Visibility.Visible;
                FRC.MainView._rgbVideoSource.Show();
                FRC.MainView._irVideoSource.Show();
                //获取filterInfoCollection的总数
                int maxCameraCount = FilterInfoCollection.Count;
                //如果配置了两个不同的摄像头索引
                if (RgbCameraIndex != IrCameraIndex && maxCameraCount >= 2)
                {
                    //RGB摄像头加载
                    RgbDeviceVideo = new VideoCaptureDevice(FilterInfoCollection[RgbCameraIndex < maxCameraCount ? RgbCameraIndex : 0].MonikerString);
                    RgbDeviceVideo.VideoResolution = RgbDeviceVideo.VideoCapabilities[0];
                    FRC.MainView._rgbVideoSource.VideoSource = RgbDeviceVideo;
                    FRC.MainView._rgbVideoSource.Start();

                    //IR摄像头
                    IrDeviceVideo = new VideoCaptureDevice(FilterInfoCollection[IrCameraIndex < maxCameraCount ? IrCameraIndex : 0].MonikerString);
                    IrDeviceVideo.VideoResolution = IrDeviceVideo.VideoCapabilities[0];
                    FRC.MainView._irVideoSource.VideoSource = IrDeviceVideo;
                    FRC.MainView._irVideoSource.Start();
                    //双摄标志设为true
                    isDoubleShot = true;
                }
                else
                {
                    //仅打开RGB摄像头，IR摄像头控件隐藏
                    RgbDeviceVideo = new VideoCaptureDevice(FilterInfoCollection[RgbCameraIndex <= maxCameraCount ? RgbCameraIndex : 0].MonikerString);
                    RgbDeviceVideo.VideoResolution = RgbDeviceVideo.VideoCapabilities[0];
                    FRC.MainView._rgbVideoSource.VideoSource = RgbDeviceVideo;
                    FRC.MainView._rgbVideoSource.Start();
                    FRC.MainView._irVideoSource.Hide();
                    FRC.MainView._irWinFormsHost.Visibility=System.Windows.Visibility.Collapsed;
                }
            }
        } 

        private FaceTrackUnit trackRGBUnit = new FaceTrackUnit();
        private FaceTrackUnit trackIRUnit = new FaceTrackUnit();
        private Font font = new Font(FontFamily.GenericSerif, 10f, FontStyle.Bold);
        private SolidBrush yellowBrush = new SolidBrush(Color.Yellow);
        private SolidBrush blueBrush = new SolidBrush(Color.Blue);
        private bool isRGBLock = false;
        private bool isIRLock = false;
        private MRECT allRect = new MRECT();
        private object rectLock = new object();

        /// <summary>
        /// RGB摄像头Paint事件，图像显示到窗体上，得到每一帧图像，并进行处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void VideoSource_Paint(object sender, PaintEventArgs e)
        {
            if (FRC.MainView._rgbVideoSource.IsRunning)
            {
                //得到当前RGB摄像头下的图片
                Bitmap bitmap = FRC.MainView._rgbVideoSource.GetCurrentVideoFrame();
                if (bitmap == null)
                {
                    return;
                }
                //检测人脸，得到Rect框
                ASF_MultiFaceInfo multiFaceInfo = UtilToolFace.DetectFace(pVideoEngine, bitmap);
                //得到最大人脸
                ASF_SingleFaceInfo maxFace = UtilToolFace.GetMaxFace(multiFaceInfo);
                //得到Rect
                MRECT rect = maxFace.faceRect;
                //检测RGB摄像头下最大人脸
                Graphics g = e.Graphics;
                float offsetX = FRC.MainView._rgbVideoSource.Width * 1f / bitmap.Width;
                float offsetY = FRC.MainView._rgbVideoSource.Height * 1f / bitmap.Height;
                float x = rect.left * offsetX;
                float width = rect.right * offsetX - x;
                float y = rect.top * offsetY;
                float height = rect.bottom * offsetY - y;
                //根据Rect进行画框
                g.DrawRectangle(Pens.Red, x, y, width, height);
                if (trackRGBUnit.message != "" && x > 0 && y > 0)
                {
                    //将上一帧检测结果显示到页面上
                    g.DrawString(trackRGBUnit.message, font, trackRGBUnit.message.Contains("活体") ? blueBrush : yellowBrush, x, y - 15);
                }

                //保证只检测一帧，防止页面卡顿以及出现其他内存被占用情况
                if (isRGBLock == false)
                {
                    isRGBLock = true;
                    //异步处理提取特征值和比对，不然页面会比较卡
                    ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
                    {
                        if (rect.left != 0 && rect.right != 0 && rect.top != 0 && rect.bottom != 0)
                        {
                            try
                            {
                                lock (rectLock)
                                {
                                    allRect.left = (int)(rect.left * offsetX);
                                    allRect.top = (int)(rect.top * offsetY);
                                    allRect.right = (int)(rect.right * offsetX);
                                    allRect.bottom = (int)(rect.bottom * offsetY);
                                }

                                bool isLiveness = false;

                                //调整图片数据，非常重要
                                ImageInfo imageInfo = UtilTool.ReadImage(bitmap);
                                if (imageInfo == null)
                                {
                                    return;
                                }
                                int retCode_Liveness = -1;
                                //RGB活体检测
                                ASF_LivenessInfo liveInfo = UtilToolFace.LivenessInfo_RGB(pVideoRGBImageEngine, imageInfo, multiFaceInfo, out retCode_Liveness);
                                //判断检测结果
                                if (retCode_Liveness == 0 && liveInfo.num > 0)
                                {
                                    int isLive = UtilToolMemory.PtrToStructure<int>(liveInfo.isLive);
                                    isLiveness = (isLive == 1) ? true : false;
                                }
                                if (imageInfo != null)
                                {
                                    UtilToolMemory.Free(imageInfo.imgData);
                                }
                                if (isLiveness)
                                {
                                    //提取人脸特征
                                    IntPtr feature = UtilToolFace.ExtractFaceFeature(pVideoRGBImageEngine, bitmap, maxFace);
                                    float similarity = 0f;
                                    //得到比对结果
                                    int result = CompareFeature(feature, out similarity);
                                    UtilToolMemory.Free(feature);
                                    if (result > -1)
                                    {
                                        //将比对结果放到显示消息中，用于最新显示
                                        trackRGBUnit.message = string.Format(" {0}号 {1},{2}", result, similarity, string.Format("RGB{0}", isLiveness ? "活体" : "假体"));
                                    }
                                    else
                                    {
                                        //显示消息
                                        trackRGBUnit.message = string.Format("RGB{0}", isLiveness ? "活体" : "假体");
                                    }
                                }
                                else
                                {
                                    //显示消息
                                    trackRGBUnit.message = string.Format("RGB{0}", isLiveness ? "活体" : "假体");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            finally
                            {
                                if (bitmap != null)
                                {
                                    bitmap.Dispose();
                                }
                                isRGBLock = false;
                            }
                        }
                        else
                        {
                            lock (rectLock)
                            {
                                allRect.left = 0;
                                allRect.top = 0;
                                allRect.right = 0;
                                allRect.bottom = 0;
                            }
                        }
                        isRGBLock = false;
                    }));
                }
            }
        }

        /// <summary>
        /// RGB摄像头Paint事件,同步RGB人脸框，对比人脸框后进行IR活体检测
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void IrVideoSource_Paint(object sender, PaintEventArgs e)
        {
            if (isDoubleShot && FRC.MainView._irVideoSource.IsRunning)
            {
                //如果双摄，且IR摄像头工作，获取IR摄像头图片
                Bitmap irBitmap = FRC.MainView._irVideoSource.GetCurrentVideoFrame();
                if (irBitmap == null)
                {
                    return;
                }
                //得到Rect
                MRECT rect = new MRECT();
                lock (rectLock)
                {
                    rect = allRect;
                }
                float irOffsetX = FRC.MainView._irVideoSource.Width * 1f / irBitmap.Width;
                float irOffsetY = FRC.MainView._irVideoSource.Height * 1f / irBitmap.Height;
                float offsetX = FRC.MainView._irVideoSource.Width * 1f / FRC.MainView._rgbVideoSource.Width;
                float offsetY = FRC.MainView._irVideoSource.Height * 1f / FRC.MainView._rgbVideoSource.Height;
                //检测IR摄像头下最大人脸
                Graphics g = e.Graphics;

                float x = rect.left * offsetX;
                float width = rect.right * offsetX - x;
                float y = rect.top * offsetY;
                float height = rect.bottom * offsetY - y;
                //根据Rect进行画框
                g.DrawRectangle(Pens.Red, x, y, width, height);
                if (trackIRUnit.message != "" && x > 0 && y > 0)
                {
                    //将上一帧检测结果显示到页面上
                    g.DrawString(trackIRUnit.message, font, trackIRUnit.message.Contains("活体") ? blueBrush : yellowBrush, x, y - 15);
                }

                //保证只检测一帧，防止页面卡顿以及出现其他内存被占用情况
                if (isIRLock == false)
                {
                    isIRLock = true;
                    //异步处理提取特征值和比对，不然页面会比较卡
                    ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
                    {
                        if (rect.left != 0 && rect.right != 0 && rect.top != 0 && rect.bottom != 0)
                        {
                            bool isLiveness = false;
                            try
                            {
                                //得到当前摄像头下的图片
                                if (irBitmap != null)
                                {
                                    //检测人脸，得到Rect框
                                    ASF_MultiFaceInfo irMultiFaceInfo = UtilToolFace.DetectFace(pVideoIRImageEngine, irBitmap);
                                    if (irMultiFaceInfo.faceNum <= 0)
                                    {
                                        return;
                                    }
                                    //得到最大人脸
                                    ASF_SingleFaceInfo irMaxFace = UtilToolFace.GetMaxFace(irMultiFaceInfo);
                                    //得到Rect
                                    MRECT irRect = irMaxFace.faceRect;
                                    //判断RGB图片检测的人脸框与IR摄像头检测的人脸框偏移量是否在误差允许范围内
                                    if (JudgeInAllowErrorRange(rect.left * offsetX / irOffsetX, irRect.left) && JudgeInAllowErrorRange(rect.right * offsetX / irOffsetX, irRect.right)
                                        && JudgeInAllowErrorRange(rect.top * offsetY / irOffsetY, irRect.top) && JudgeInAllowErrorRange(rect.bottom * offsetY / irOffsetY, irRect.bottom))
                                    {
                                        int retCode_Liveness = -1;
                                        //将图片进行灰度转换，然后获取图片数据
                                        ImageInfo irImageInfo = UtilTool.ReadBMP_IR(irBitmap);
                                        if (irImageInfo == null)
                                        {
                                            return;
                                        }
                                        //IR活体检测
                                        ASF_LivenessInfo liveInfo = UtilToolFace.LivenessInfo_IR(pVideoIRImageEngine, irImageInfo, irMultiFaceInfo, out retCode_Liveness);
                                        //判断检测结果
                                        if (retCode_Liveness == 0 && liveInfo.num > 0)
                                        {
                                            int isLive = UtilToolMemory.PtrToStructure<int>(liveInfo.isLive);
                                            isLiveness = (isLive == 1) ? true : false;
                                        }
                                        if (irImageInfo != null)
                                        {
                                            UtilToolMemory.Free(irImageInfo.imgData);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            finally
                            {
                                trackIRUnit.message = string.Format("IR{0}", isLiveness ? "活体" : "假体");
                                if (irBitmap != null)
                                {
                                    irBitmap.Dispose();
                                }
                                isIRLock = false;
                            }
                        }
                        else
                        {
                            trackIRUnit.message = string.Empty;
                        }
                        isIRLock = false;
                    }));
                }
            }
        }

        /// <summary>
        /// 摄像头播放完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reason"></param>
        public void VideoSource_PlayingFinished(object sender, AForge.Video.ReasonToFinishPlaying reason)
        {
            try
            {
                Control.CheckForIllegalCrossThreadCalls = false;
                //_chooseImgBtn.Enabled = true;
                //_matchBtn.Enabled = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #endregion
    }
}
