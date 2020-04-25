using ArcSoftFace.Entity;
using ArcSoftFace.SDKModels;
using ArcSoftFace.SDKUtil;
using FaceRecognitionDemo.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace FaceRecognitionDemo
{
    public partial class FaceRecognizeEngine
    {

        /// <summary>
        /// 引擎Handle
        /// </summary>
        private IntPtr pImageEngine = IntPtr.Zero;

        /// <summary>
        /// 视频引擎Handle
        /// </summary>
        private IntPtr pVideoEngine = IntPtr.Zero;

        /// <summary>
        /// RGB视频引擎 FR Handle 处理   FR和图片引擎分开，减少强占引擎的问题
        /// </summary>
        private IntPtr pVideoRGBImageEngine = IntPtr.Zero;

        /// <summary>
        /// IR视频引擎 FR Handle 处理   FR和图片引擎分开，减少强占引擎的问题
        /// </summary>
        private IntPtr pVideoIRImageEngine = IntPtr.Zero;

        /// <summary>
        /// 保存对比图片的列表
        /// </summary>
        private List<string> ImagePathList { get; set; }

        /// <summary>
        /// 人脸特征列表
        /// </summary>
        public List<IntPtr> ImagesFeatureList { get; set; }

        /// <summary>
        /// 图片人脸特征
        /// </summary>
        private IntPtr ImageFeature { get; set; }

        /// <summary>
        /// 图片最大大小
        /// </summary>
        public readonly long MaxSize = 1024 * 1024 * 2;

        private bool isCompare;

        /// <summary>
        /// FaceRecognitionControl类传入this
        /// </summary>
        public FaceRecognitionControl FRC { get; set; }

        public FaceRecognizeEngine()
        {
            Init();
            InitEngine();
        }

        //初始化this
        private void Init()
        {
            ImagePathList = new List<string>();
            ImagesFeatureList = new List<IntPtr>();
        }

        //初始化Engine
        private void InitEngine()
        {
            AppSettingsReader reader = new AppSettingsReader();
            string appId = (string)reader.GetValue("APP_ID", typeof(string));
            string sdkKey64 = (string)reader.GetValue("SDKKEY64", typeof(string));
            string sdkKey32 = (string)reader.GetValue("SDKKEY32", typeof(string));
            RgbCameraIndex = (int)reader.GetValue("RGB_CAMERA_INDEX", typeof(int));
            IrCameraIndex = (int)reader.GetValue("IR_CAMERA_INDEX", typeof(int));
            //判断CPU位数
            var is64CPU = Environment.Is64BitProcess;

            //在线激活引擎    如出现错误，1.请先确认从官网下载的sdk库已放到对应的bin中，2.当前选择的CPU为x86或者x64
            int retCode = 0;
            try
            {
                retCode = ASFFunctions.ASFActivation(appId, is64CPU ? sdkKey64 : sdkKey32);
            }
            catch (Exception ex)
            {
                //禁用相关功能按钮
                //ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);

                if (ex.Message.Contains("无法加载 DLL"))
                {
                    MessageBox.Show("请将sdk相关DLL放入bin文件夹中!");
                }
                else
                {
                    MessageBox.Show("激活引擎失败!");
                }
                return;
            }
            Console.WriteLine("Activate Result:" + retCode);

            //初始化引擎
            uint detectMode = DetectionMode.ASF_DETECT_MODE_IMAGE;
            //Video模式下检测脸部的角度优先值
            int videoDetectFaceOrientPriority = ASF_OrientPriority.ASF_OP_0_HIGHER_EXT;
            //Image模式下检测脸部的角度优先值
            int imageDetectFaceOrientPriority = ASF_OrientPriority.ASF_OP_0_ONLY;
            //人脸在图片中所占比例，如果需要调整检测人脸尺寸请修改此值，有效数值为2-32
            int detectFaceScaleVal = 16;
            //最大需要检测的人脸个数
            int detectFaceMaxNum = 5;
            //引擎初始化时需要初始化的检测功能组合
            int combinedMask = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_AGE | FaceEngineMask.ASF_GENDER | FaceEngineMask.ASF_FACE3DANGLE;
            //初始化引擎，正常值为0，其他返回值请参考http://ai.arcsoft.com.cn/bbs/forum.php?mod=viewthread&tid=19&_dsign=dbad527e
            retCode = ASFFunctions.ASFInitEngine(detectMode, imageDetectFaceOrientPriority, detectFaceScaleVal, detectFaceMaxNum, combinedMask, ref pImageEngine);
            Console.WriteLine("InitEngine Result:" + retCode);
            MessageBox.Show((retCode == 0) ? "引擎初始化成功!\n" : string.Format("引擎初始化失败!错误码为:{0}\n", retCode));

            if (retCode != 0)
            {
                //禁用相关功能按钮
                //ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
            }

            //初始化视频模式下人脸检测引擎
            uint detectModeVideo = DetectionMode.ASF_DETECT_MODE_VIDEO;
            int combinedMaskVideo = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION;
            retCode = ASFFunctions.ASFInitEngine(detectModeVideo, videoDetectFaceOrientPriority, detectFaceScaleVal, detectFaceMaxNum, combinedMaskVideo, ref pVideoEngine);
            //RGB视频专用FR引擎
            detectFaceMaxNum = 1;
            combinedMask = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_LIVENESS;
            retCode = ASFFunctions.ASFInitEngine(detectMode, imageDetectFaceOrientPriority, detectFaceScaleVal, detectFaceMaxNum, combinedMask, ref pVideoRGBImageEngine);

            //IR视频专用FR引擎
            combinedMask = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_IR_LIVENESS;
            retCode = ASFFunctions.ASFInitEngine(detectMode, imageDetectFaceOrientPriority, detectFaceScaleVal, detectFaceMaxNum, combinedMask, ref pVideoIRImageEngine);

            Console.WriteLine("InitVideoEngine Result:" + retCode);
        }

        #region 注册人脸
        private object locker = new object();
        /// <summary>
        /// 导入照片录入
        /// </summary>
        /// <param name="IsOfflineRecord">是否为本地导入</param>
        public void ImportImgesFun(bool IsOfflineRecord)
        {
            lock (locker)
            {
                List<string> imagePathListTemp = ImportImages(IsOfflineRecord);
                if (imagePathListTemp.Count <= 0) return;
                FRC.PIVM.NaturalSourceImage = imagePathListTemp[0];

                var numStart = ImagePathList.Count;
                int isGoodImage = 0;

                //人脸检测以及提取人脸特征
                ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
                {
                    //人脸检测和剪裁
                    for (int i = 0; i < imagePathListTemp.Count; i++)
                    {
                        Image image = UtilTool.ReadImageFromFile(imagePathListTemp[i]);
                        if (image == null)
                        {
                            continue;
                        }
                        if (image.Width > 1536 || image.Height > 1536)
                        {
                            image = UtilTool.ScaleImage(image, 1536, 1536);
                        }
                        if (image == null)
                        {
                            continue;
                        }
                        if (image.Width % 4 != 0)
                        {
                            image = UtilTool.ScaleImage(image, image.Width - (image.Width % 4), image.Height);
                        }
                        //人脸检测
                        ASF_MultiFaceInfo multiFaceInfo = UtilToolFace.DetectFace(pImageEngine, image);
                        //判断检测结果
                        if (multiFaceInfo.faceNum > 0)
                        {
                            ImagePathList.Add(imagePathListTemp[i]);
                            MRECT rect = UtilToolMemory.PtrToStructure<MRECT>(multiFaceInfo.faceRects);
                            image = UtilTool.CutImage(image, rect.left, rect.top, rect.right, rect.bottom);
                        }
                        else
                        {
                            if (image != null)
                            {
                                image.Dispose();
                            }
                            continue;
                        }
                        //显示人脸
                        FRC.MainView.Dispatcher.Invoke(new Action(delegate
                        {
                            if (image == null)
                            {
                                image = UtilTool.ReadImageFromFile(imagePathListTemp[i]);

                                if (image.Width > 1536 || image.Height > 1536)
                                {
                                    image = UtilTool.ScaleImage(image, 1536, 1536);
                                }
                            }
                            //_imageLists.Images.Add(imagePathListTemp[i], image);
                            //_imageList.Items.Add((numStart + isGoodImage) + "号", imagePathListTemp[i]);
                            isGoodImage += 1;
                            if (image != null)
                            {
                                image.Dispose();
                            }
                        }));
                    }

                    //提取人脸特征
                    for (int i = numStart; i < ImagePathList.Count; i++)
                    {
                        ASF_SingleFaceInfo singleFaceInfo = new ASF_SingleFaceInfo();
                        Image image = UtilTool.ReadImageFromFile(ImagePathList[i]);
                        if (image == null)
                        {
                            continue;
                        }
                        IntPtr feature = UtilToolFace.ExtractFaceFeature(pImageEngine, image, out singleFaceInfo);

                        FRC.PIVM.FaceCharacteristics = feature;

                        FRC.MainView.Dispatcher.Invoke(new Action(delegate
                        {
                            if (singleFaceInfo.faceRect.left == 0 && singleFaceInfo.faceRect.right == 0)
                            {
                                AppendText(string.Format("{0}号未检测到人脸\r\n", i));
                            }
                            else
                            {
                                AppendText(string.Format("已提取{0}号人脸特征值，[left:{1},right:{2},top:{3},bottom:{4},orient:{5}]\r\n", i, singleFaceInfo.faceRect.left, singleFaceInfo.faceRect.right, singleFaceInfo.faceRect.top, singleFaceInfo.faceRect.bottom, singleFaceInfo.faceOrient));
                                ImagesFeatureList.Add(feature);
                            }
                           
                        }));
                        if (image != null)
                        {
                            image.Dispose();
                        }
                    }
                    FRC.MainView.Dispatcher.Invoke(new Action(delegate
                    {
                        FRC.MainVM.IsCameraEnable = true;

                        //FRC.MainVM.IsPGBtnEnable = true;
                        FRC.MainVM.IsSIBtnEnable = true;
                        //FRC.MainVM.IsPGIBtnEnable = true;
                        FRC.MainVM.IsSIIBtnEnable = true;
                    }));
                
                }));

                //保存个人信息数据
                //FRC.UploadFaceRecord();
            }
        }
        #endregion

        #region 选择图片识别
        /// <summary>
        /// “选择图片识别”
        /// </summary>
        public void ChooseImgeDistinguish(bool IsOffline )
        {
            //判断引擎是否初始化成功
            if (pImageEngine == IntPtr.Zero)
            {
                //禁用相关功能按钮
                FRC.MainVM.IsSIIBtnEnable = false;
                FRC.MainVM.IsPGIBtnEnable = false;
                //ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
                MessageBox.Show("请先初始化引擎!");
                return;
            }

            List<string> imagePathList = ImportImages(IsOffline, false);//单张照片
            if (imagePathList.Count <= 0) return ;
            string selectedImagePath = imagePathList[0];

            DateTime detectStartTime = DateTime.Now;
            AppendText(string.Format("------------------------------开始检测，时间:{0}------------------------------\n", detectStartTime.ToString("yyyy-MM-dd HH:mm:ss:ms")));

            //获取文件，拒绝过大的图片
            FileInfo fileInfo = new FileInfo(selectedImagePath);
            if (fileInfo.Length > MaxSize)
            {
                MessageBox.Show("图像文件最大为2MB，请压缩后再导入!");
                AppendText(string.Format("------------------------------检测结束，时间:{0}------------------------------\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                AppendText("\n");
                return;
            }

            Image srcImage = UtilTool.ReadImageFromFile(selectedImagePath);
            if (srcImage == null)
            {
                MessageBox.Show("图像数据获取失败，请稍后重试!");
                AppendText(string.Format("------------------------------检测结束，时间:{0}------------------------------\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                AppendText("\n");
                return;
            }
            if (srcImage.Width > 1536 || srcImage.Height > 1536)
            {
                srcImage = UtilTool.ScaleImage(srcImage, 1536, 1536);
            }
            if (srcImage == null)
            {
                MessageBox.Show("图像数据获取失败，请稍后重试!");
                AppendText(string.Format("------------------------------检测结束，时间:{0}------------------------------\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                AppendText("\n");
                return;
            }
            //调整图像宽度，需要宽度为4的倍数
            if (srcImage.Width % 4 != 0)
            {
                srcImage = UtilTool.ScaleImage(srcImage, srcImage.Width - (srcImage.Width % 4), srcImage.Height);
            }
            //调整图片数据，非常重要
            ImageInfo imageInfo = UtilTool.ReadImage(srcImage);
            if (imageInfo == null)
            {
                MessageBox.Show("图像数据获取失败，请稍后重试!");
                AppendText(string.Format("------------------------------检测结束，时间:{0}------------------------------\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                AppendText("\n");
                return;
            }
            //人脸检测
            ASF_MultiFaceInfo multiFaceInfo = UtilToolFace.DetectFace(pImageEngine, imageInfo);
            //年龄检测
            int retCode_Age = -1;
            ASF_AgeInfo ageInfo = UtilToolFace.DetectAge(pImageEngine, imageInfo, multiFaceInfo, out retCode_Age);
            //性别检测
            int retCode_Gender = -1;
            ASF_GenderInfo genderInfo = UtilToolFace.DetectGender(pImageEngine, imageInfo, multiFaceInfo, out retCode_Gender);

            //3DAngle检测
            int retCode_3DAngle = -1;
            ASF_Face3DAngle face3DAngleInfo = UtilToolFace.DetectFace3DAngle(pImageEngine, imageInfo, multiFaceInfo, out retCode_3DAngle);

            UtilToolMemory.Free(imageInfo.imgData);

            if (multiFaceInfo.faceNum < 1)
            {
                srcImage = UtilTool.ScaleImage(srcImage, (int)FRC.FIView._faceIdentificationImageBox.Width, (int)FRC.FIView._faceIdentificationImageBox.Height);
                ImageFeature = IntPtr.Zero;
                FRC.FIView._faceIdentificationImageBox.Source = UtilTool.BitmapToBitmapImage(srcImage);
                AppendText(string.Format("{0} - 未检测出人脸!\n\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")));
                AppendText(string.Format("------------------------------检测结束，时间:{0}------------------------------\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                AppendText("\n");
                return;
            }

            MRECT temp = new MRECT();
            int ageTemp = 0;
            int genderTemp = 0;
            int rectTemp = 0;

            //标记出检测到的人脸
            for (int i = 0; i < multiFaceInfo.faceNum; i++)
            {
                MRECT rect = UtilToolMemory.PtrToStructure<MRECT>(multiFaceInfo.faceRects + UtilToolMemory.SizeOf<MRECT>() * i);
                int orient = UtilToolMemory.PtrToStructure<int>(multiFaceInfo.faceOrients + UtilToolMemory.SizeOf<int>() * i);
                int age = 0;

                if (retCode_Age != 0)
                {
                    AppendText(string.Format("年龄检测失败，返回{0}!\n\n", retCode_Age));
                }
                else
                {
                    age = UtilToolMemory.PtrToStructure<int>(ageInfo.ageArray + UtilToolMemory.SizeOf<int>() * i);
                }

                int gender = -1;
                if (retCode_Gender != 0)
                {
                    AppendText(string.Format("性别检测失败，返回{0}!\n\n", retCode_Gender));
                }
                else
                {
                    gender = UtilToolMemory.PtrToStructure<int>(genderInfo.genderArray + UtilToolMemory.SizeOf<int>() * i);
                }

                int face3DStatus = -1;
                float roll = 0f;
                float pitch = 0f;
                float yaw = 0f;
                if (retCode_3DAngle != 0)
                {
                    AppendText(string.Format("3DAngle检测失败，返回{0}!\n\n", retCode_3DAngle));
                }
                else
                {
                    //角度状态 非0表示人脸不可信
                    face3DStatus = UtilToolMemory.PtrToStructure<int>(face3DAngleInfo.status + UtilToolMemory.SizeOf<int>() * i);
                    //roll为侧倾角，pitch为俯仰角，yaw为偏航角
                    roll = UtilToolMemory.PtrToStructure<float>(face3DAngleInfo.roll + UtilToolMemory.SizeOf<float>() * i);
                    pitch = UtilToolMemory.PtrToStructure<float>(face3DAngleInfo.pitch + UtilToolMemory.SizeOf<float>() * i);
                    yaw = UtilToolMemory.PtrToStructure<float>(face3DAngleInfo.yaw + UtilToolMemory.SizeOf<float>() * i);
                }

                int rectWidth = rect.right - rect.left;
                int rectHeight = rect.bottom - rect.top;

                //查找最大人脸
                if (rectWidth * rectHeight > rectTemp)
                {
                    rectTemp = rectWidth * rectHeight;
                    temp = rect;
                    ageTemp = age;
                    genderTemp = gender;
                }
                AppendText(string.Format("{0} - 人脸坐标:[left:{1},top:{2},right:{3},bottom:{4},orient:{5},roll:{6},pitch:{7},yaw:{8},status:{11}] Age:{9} Gender:{10}\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), rect.left, rect.top, rect.right, rect.bottom, orient, roll, pitch, yaw, age, (gender >= 0 ? gender.ToString() : ""), face3DStatus));
            }

            AppendText(string.Format("{0} - 人脸数量:{1}\n\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), multiFaceInfo.faceNum));

            DateTime detectEndTime = DateTime.Now;
            AppendText(string.Format("------------------------------检测结束，时间:{0}------------------------------\n", detectEndTime.ToString("yyyy-MM-dd HH:mm:ss:ms")));
            AppendText("\n");
            ASF_SingleFaceInfo singleFaceInfo = new ASF_SingleFaceInfo();
            //提取人脸特征
            ImageFeature = UtilToolFace.ExtractFaceFeature(pImageEngine, srcImage, out singleFaceInfo);

            //清空上次的匹配结果
            for (int i = 0; i < ImagesFeatureList.Count; i++)
            {
                //_imageList.Items[i].Text = string.Format("{0}号", i);
            }
            //获取缩放比例
            float scaleRate = UtilTool.GetWidthAndHeight(srcImage.Width, srcImage.Height, (int)FRC.FIView._faceIdentificationImageBox.Width, (int)FRC.FIView._faceIdentificationImageBox.Height);
            //缩放图片
            srcImage = UtilTool.ScaleImage(srcImage, (int)FRC.FIView._faceIdentificationImageBox.Width, (int)FRC.FIView._faceIdentificationImageBox.Height);
            //添加标记
            srcImage = UtilTool.MarkRectAndString(srcImage,
                (int)(temp.left * scaleRate),
                (int)(temp.top * scaleRate),
                (int)(temp.right * scaleRate) - (int)(temp.left * scaleRate),
                (int)(temp.bottom * scaleRate) - (int)(temp.top * scaleRate),
                ageTemp, genderTemp,
                (int)FRC.FIView._faceIdentificationImageBox.Width);

            //显示标记后的图像
            FRC.FIView._faceIdentificationImageBox.Source = UtilTool.BitmapToBitmapImage(srcImage);

            MatchImage();
        }
        #endregion

        #region 开始匹配
        /// <summary>
        /// 匹配
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MatchImage()
        {
            if (ImagesFeatureList.Count == 0)
            {
                MessageBox.Show("请注册人脸!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (ImageFeature == IntPtr.Zero)
            {
                if (FRC.FIView._faceIdentificationImageBox.Source == null)
                {
                    MessageBox.Show("请选择识别图!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("比对失败，识别图未提取到特征值!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
            //标记已经做了匹配比对，在开启视频的时候要清除比对结果
            isCompare = true;
            float compareSimilarity = 0f;
            int compareNum = -1;
            AppendText(string.Format("------------------------------开始比对，时间:{0}------------------------------\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));

            ImagesFeatureList.Clear();
            foreach (var item in FRC.FRVM.PIVMList)
            {
                ImagesFeatureList.Add(item.FaceCharacteristics);
            } 

            for (int i = 0; i < ImagesFeatureList.Count; i++)
            {
                IntPtr feature = ImagesFeatureList[i];
                float similarity = 0f;
                int ret = ASFFunctions.ASFFaceFeatureCompare(pImageEngine, ImageFeature, feature, ref similarity);
                //增加异常值处理
                if (similarity.ToString().IndexOf("E") > -1)
                {
                    similarity = 0f;
                }
                AppendText(string.Format("与{0}号比对结果:{1}\r\n", i, similarity));

                //选择最合适的图片
                if (similarity > compareSimilarity && similarity >= threshold)
                {

                    compareSimilarity = similarity;
                    compareNum = i;
                }

                //_imageList.Items[i].Text = string.Format("{0}号({1})", i, similarity);
            }
            FRC.FIVM.PersonInfoUC.DataContext = FRC.FRVM.PIVMList.SingleOrDefault(x => x.FaceCharacteristics == ImagesFeatureList[compareNum]);

            if (compareSimilarity > 0)
            {
                //_lblCompareInfo.Text = " " + compareNum + "号," + compareSimilarity;
            }
            AppendText(string.Format("------------------------------比对结束，时间:{0}------------------------------\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
        }
        #endregion
        
    }
}
