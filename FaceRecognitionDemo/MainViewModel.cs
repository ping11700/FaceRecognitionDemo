using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using PropertyTools;
using System.Windows.Media.Imaging;

namespace FaceRecognitionDemo
{
    public class MainViewModel:Observable
    {
        public MainViewModel()
        {
            mainTitle = "人脸识别软件Demo";
            ThresholdValue = 0.8f;

            resolutionList = new List<string>();
            cameraList = new List<string>();
        }

        //功能页面
        public UserControl UC
        {
            get { return this.uc; }
            set { SetValue(ref this.uc, value, nameof(UC)); }
        }

        //主标题
        public string MainTitle
        {
            get { return this.mainTitle; }
            set { SetValue(ref this.mainTitle, value, nameof(MainTitle)); }
        }


        //功能页面标题
        public string UCTitle
        {
            get { return this.ucTitle; }
            set { SetValue(ref this.ucTitle, value, nameof(UCTitle)); }
        }


        //跟踪信息
        public string TraceMessage
        {
            get { return this.traceMessage; }
            set { SetValue(ref this.traceMessage, value, nameof(TraceMessage)); }
        }

        //分辨率列表
        public List<string> ResolutionList
        {
            get { return this.resolutionList; }
            set { SetValue(ref this.resolutionList, value, nameof(ResolutionList)); }
        }


        //选中的分辨率
        public string SelectedResolution
        {
            get { return this.selectedResolution; }
            set
            {
                SetValue(ref this.selectedResolution, value, nameof(SelectedResolution));
            }
        }

        //相机列表
        public List<string> CameraList
        {
            get { return this.cameraList; }
            set { SetValue(ref this.cameraList, value, nameof(CameraList)); }
        }

        //选中的相机
        public string SelectedCamera
        {
            get { return this.selectedCamera; }
            set
            {
                SetValue(ref this.selectedCamera, value, nameof(SelectedCamera));
            }
        }


        //截图
        public BitmapImage ShotSourceImage
        {
            get { return this.shotSourceImage; }
            set{ SetValue(ref this.shotSourceImage, value, nameof(ShotSourceImage)); }
        }

        #region 个人信息
        public bool IsShowPersonInfoUC
        {
            get { return this.isShowPersonInfoUC; }
            set { SetValue(ref this.isShowPersonInfoUC, value, nameof(IsShowPersonInfoUC)); }
        }

        //个人信息页面
        public UserControl PersonInfoUC
        {
            get { return this.personInfoUC; }
            set { SetValue(ref this.personInfoUC, value, nameof(PersonInfoUC)); }
        }

        //是否摄像头可用
        public bool IsCameraEnable
        {
            get { return this.isCameraEnable; }
            set { SetValue(ref this.isCameraEnable, value, nameof(IsCameraEnable)); }
        }

        
        //是否录入模式
        public bool IsFaceRecordMode
        {
            get { return this.isFaceRecordMode; }
            set { SetValue(ref this.isFaceRecordMode, value, nameof(IsFaceRecordMode)); }
        }

        //是否识别模式
        public bool IsFaceIdentificationMode
        {
            get { return this.isFaceIdentificationMode; }
            set { SetValue(ref this.isFaceIdentificationMode, value, nameof(IsFaceIdentificationMode)); }
        }

        //阈值
        public float ThresholdValue
        {
            get { return this.thresholdValue; }
            set { SetValue(ref this.thresholdValue, value, nameof(ThresholdValue)); }
        }

        #endregion


        #region Button  是否可用

        //拍照录入 是否可用
        public bool IsPGBtnEnable
        {
            get { return this.isPGBtnEnable; }
            set { SetValue(ref this.isPGBtnEnable, value, nameof(IsPGBtnEnable)); }
        }
        //选照录入 是否可用
        public bool IsSIBtnEnable
        {
            get { return this.isSIBtnEnable; }
            set { SetValue(ref this.isSIBtnEnable, value, nameof(IsSIBtnEnable)); }
        }
        //拍照识别 是否可用
        public bool IsPGIBtnEnable
        {
            get { return this.isPGIBtnEnable; }
            set { SetValue(ref this.isPGIBtnEnable, value, nameof(IsPGIBtnEnable)); }
        }
        //选照识别 是否可用
        public bool IsSIIBtnEnable
        {
            get { return this.isSIIBtnEnable; }
            set { SetValue(ref this.isSIIBtnEnable, value, nameof(IsSIIBtnEnable)); }
        }


        #endregion



        /// <summary>
        /// 选中相机Action
        /// </summary>
        //public Action SelectedCameraAction;

        /// <summary>
        /// 选中分辨率Action
        /// </summary>
        //public Action SelectedResolutionAction;
        

        private UserControl uc;
        private string mainTitle; 
        private string ucTitle;
        private string traceMessage;

        private List<string> resolutionList;
        private string selectedResolution;
        private List<string> cameraList;
        private string selectedCamera;

        private BitmapImage shotSourceImage;

        private bool isShowPersonInfoUC;
        private UserControl personInfoUC;
        private bool isCameraEnable;
        private bool isFaceRecordMode;
        private bool isFaceIdentificationMode;

        private float thresholdValue;


        private bool isPGBtnEnable;
        private bool isSIBtnEnable;
        private bool isPGIBtnEnable;
        private bool isSIIBtnEnable;
    }
}
