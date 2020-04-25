using Camera_NET;
using DirectShowLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FaceRecognitionDemo
{
    public partial class FaceRecognitionControl
    {

        //个人信息存放路径    
        public readonly string PersonInfoPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "SavedPersonInfo");

        //主界面ViewModel
        public MainViewModel MainVM { get; set; }

        //主界面
        public MainWindow MainView { get; set; }

        //人脸录入ViewModel
        public FaceRecordVM FRVM { get; set; }

        //人脸录入View
        public FaceRecordView FRView { get; set; }

        //人脸识别ViewModel
        public FaceIdentificationVM FIVM { get; set; }

        //人脸识别View
        public FaceIdentificationView FIView { get; set; }

        //个人信息View
        public PersonInfoView PIView { get; set; }

        //个人信息ViewModel
        public PersonInfoVM PIVM { get; set; }

        //Devices
        public DsDevice[] DDArray { get; set; }

        //相机分辨率集合
        public ResolutionList Resolutions { get; set; }


        public FaceRecognitionControl()
        {
            Init();
            OpenFaceRecord();//初始化软件打开人脸录入界面
        }


        //初始化
        private void Init()
        {
            MainVM = new MainViewModel();

            //personInfo
            PIVM = new PersonInfoVM();
            PIView = new PersonInfoView() { DataContext = PIVM };

            //人脸库
            FRVM = new FaceRecordVM();
            FRView = new FaceRecordView() { DataContext = FRVM };

            //识别结果
            FIVM = new FaceIdentificationVM();
            FIView = new FaceIdentificationView() { DataContext = FIVM };

            FIVM.PersonInfoUC = new PersonInfoView();

            PIVM.FaceCharacteristicsAction += UploadFaceRecord;
            UtilTool.CreateDirectory_FullControl(PersonInfoPath);   //创建用户信息文件夹
        }

        //Open FaceRecord 打开人脸录入
        public void OpenFaceRecord()
        {
            MainVM.PersonInfoUC = PIView; //personInfo 传入MainWindow

            MainVM.UCTitle = "人脸录入";
            MainVM.UC = FRView;

        }

        //上传保存personInfo, FaceRecord 完成人脸录入
        public void UploadFaceRecord()
        {
            Application.Current.Dispatcher.Invoke(
                (Action)(() =>
                {
                    FRVM.PIVMList.Add((PersonInfoVM)PIVM.Clone());

                    ComplateFaceRecord();
                    MainView._logTextBox.AppendText("人脸录入完成");
                    MainView._logTextBox.AppendText("\n");
                 
                }));
            FRVM.IsEditBtnEnable = FRVM.IsDeletBtnEnable
               = FRVM.IsClearBtnEnable = FRVM.PIVMList.Count > 0;
        }



        //完成 FaceRecord 结束人脸录入
        public void ComplateFaceRecord()
        {
            PIVM.PersonName = null;
            PIVM.PersonNumber = null;
            PIVM.PersonGender = null;
            PIVM.PersonClass = null;
        }

        //Open FaceIdentification 打开人脸识别
        public void OpenFaceIdentification()
        {

          
            
            MainVM.UCTitle = "人脸识别";
            MainVM.UC = FIView;

        }

        //Close FaceIdentification 关闭人脸识别
        public void CloseFaceIdentification()
        {
            FIVM = null;
            FIView = null;

            MainVM.UC = null;
            MainView._logTextBox.AppendText("人脸识别完成");
        }
    }
}
