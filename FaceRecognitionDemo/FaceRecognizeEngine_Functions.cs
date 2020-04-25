using ArcSoftFace.SDKUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace FaceRecognitionDemo
{
    public partial class FaceRecognizeEngine
    {
        /// <summary>
        /// 允许误差范围
        /// </summary>
        private int allowAbleErrorRange = 40;

        /// <summary>
        /// 相似度
        /// </summary>
        private float threshold = 0.8f;

        #region 阈值相关
        /// <summary>
        /// 阈值文本框键按下事件，检测输入内容是否正确，不正确不能输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Input_Threshold_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //阻止从键盘输入键
            e.Handled = true;
            //是数值键，回退键，.能输入，其他不能输入
            if( (e.Key >=Key.A &&e.Key <=Key.Z) ||
                (e.Key >= Key.D0 && e.Key <= Key.D9) ||
                (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
                 e.Key == Key.Enter || e.Key == Key.Decimal || e.Key == Key.OemPeriod)
            {
                //渠道当前文本框的内容
                string thresholdStr = FRC.MainView._txtThreshold.Text.Trim();
                int countStr = 0;
                int startIndex = 0;
                //如果当前输入的内容是否是“.”
                if (e.Key == Key.Decimal)
                {
                    countStr = 1;
                }
                //检测当前内容是否含有.的个数
                if (thresholdStr.IndexOf('.', startIndex) > -1)
                {
                    countStr += 1;
                }
                //如果输入的内容已经超过12个字符，
                if (e.Key != Key.Enter && (thresholdStr.Length > 12 || countStr > 1))
                {
                    return;
                }
                e.Handled = false;
            }
        }

        /// <summary>
        /// 阈值文本框键抬起事件，检测阈值是否正确，不正确改为0.8f
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Input_Threshold_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //如果输入的内容不正确改为默认值
            if (!float.TryParse(FRC.MainView._txtThreshold.Text.Trim(), out threshold))
            {
                threshold = 0.8f;
            }
            if (threshold >= 1.0f)
            {
                threshold = 1.0f;
            }
            FRC.MainVM.ThresholdValue = threshold;

        }
        #endregion

        #region 窗体关闭
        /// <summary>
        /// 窗体关闭事件
        /// </summary>
        public void WindowClosed()
        {
            try
            {
                if (FRC.MainView._rgbVideoSource.IsRunning)
                {
                    Start_CloseVideo(); //关闭摄像头
                }

                //销毁引擎
                int retCode = ASFFunctions.ASFUninitEngine(pImageEngine);
                Console.WriteLine("UninitEngine pImageEngine Result:" + retCode);
              
                //销毁引擎
                retCode = ASFFunctions.ASFUninitEngine(pVideoEngine);
                Console.WriteLine("UninitEngine pVideoEngine Result:" + retCode);

                //销毁引擎
                retCode = ASFFunctions.ASFUninitEngine(pVideoRGBImageEngine);
                Console.WriteLine("UninitEngine pVideoImageEngine Result:" + retCode);

                //销毁引擎
                retCode = ASFFunctions.ASFUninitEngine(pVideoIRImageEngine);
                Console.WriteLine("UninitEngine pVideoIRImageEngine Result:" + retCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine("UninitEngine pImageEngine Error:" + ex.Message);
            }
        }
        #endregion

        #region 公用方法
        /// <summary>
        /// 恢复使用/禁用控件列表控件
        /// </summary>
        /// <param name="isEnable"></param>
        /// <param name="controls">控件列表</param>
        private void ControlsEnable(bool isEnable, params Control[] controls)
        {
            if (controls == null || controls.Length <= 0)
            {
                return;
            }
            foreach (Control control in controls)
            {
                control.Enabled = isEnable;
            }
        }

        /// <summary>
        /// 得到feature比较结果
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        private int CompareFeature(IntPtr feature, out float similarity)
        {
            int result = -1;
            similarity = 0f;
            //如果人脸库不为空，则进行人脸匹配
            if (ImagesFeatureList != null && ImagesFeatureList.Count > 0)
            {
                for (int i = 0; i < ImagesFeatureList.Count; i++)
                {
                    //调用人脸匹配方法，进行匹配
                    ASFFunctions.ASFFaceFeatureCompare(pVideoRGBImageEngine, feature, ImagesFeatureList[i], ref similarity);
                    if (similarity >= threshold)
                    {
                        result = i;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 追加公用方法
        /// </summary>
        /// <param name="message"></param>
        private void AppendText(string message)
        {
            FRC.MainView._logTextBox.AppendText(message);
        }

        /// <summary>
        /// 判断参数0与参数1是否在误差允许范围内
        /// </summary>
        /// <param name="arg0">参数0</param>
        /// <param name="arg1">参数1</param>
        /// <returns></returns>
        private bool JudgeInAllowErrorRange(float arg0, float arg1)
        {
            bool rel = false;
            if (arg0 > arg1 - allowAbleErrorRange && arg0 < arg1 + allowAbleErrorRange)
            {
                rel = true;
            }
            return rel;
        }

        /// <summary>
        /// 将照片导入到路径集合
        /// </summary>
        /// <param name="isOffline">是否为本地导入</param>
        /// <param name="IsMultiSelect">是否为多张导入</param>
        /// <returns> List<string>imageFileNames 路径集合</returns>
        private List<string> ImportImages(bool isOffline, bool IsMultiSelect = false)
        {
            List<string> imagePathLsit = new List<string>();
            List<string> imagePathListTemp = new List<string>();
            //本地导入图片
            if (isOffline)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "选择图片";
                openFileDialog.Filter = "图片文件|*.bmp;*.jpg;*.jpeg;*.png";
                openFileDialog.Multiselect = IsMultiSelect;
                openFileDialog.FileName = string.Empty;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    imagePathListTemp = openFileDialog.FileNames.ToList(); ;

                    AppendText("本地导入成功!");
                    AppendText("\n");
                }
            }

            //相机截图
            else
            {
                Bitmap bitmap = null;
                try
                {
                    bitmap = FRC.MainView._rgbVideoSource.GetCurrentVideoFrame();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, @"Error while getting a snapshot");
                }

                if (bitmap == null)
                    return imagePathListTemp;

                //截取图片
                string imageName = DateTime.Now.Ticks.ToString() + ".png";
                UtilTool.Save_BitmapImage_Into_File(UtilTool.BitmapToBitmapImage(bitmap),
                                                    imageName,
                                                    FRC.PersonInfoPath);
                AppendText("导入成功!");
                AppendText("\n");

                imagePathListTemp.Add(Path.Combine(FRC.PersonInfoPath, imageName));
            }

            //保存图片路径并显示
            foreach (var imageFileName in imagePathListTemp)
            {
                //图片格式判断
                if (CheckImage(imageFileName))
                {
                    imagePathLsit.Add(imageFileName);
                }
            }
            return imagePathLsit;
        }

        /// <summary>
        /// 校验图片
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        private bool CheckImage(string imagePath)
        {
            if (imagePath == null)
            {
                AppendText("图片不存在，请确认后再导入\r\n");
                return false;
            }
            try
            {
                //判断图片是否正常，如将其他文件把后缀改为.jpg，这样就会报错
                Image image = UtilTool.ReadImageFromFile(imagePath);
                if (image == null)
                {
                    throw new Exception();
                }
                else
                {
                    image.Dispose();
                }
            }
            catch
            {
                AppendText(string.Format("{0} 图片格式有问题，请确认后再导入\r\n", imagePath));
                return false;
            }
            FileInfo fileCheck = new FileInfo(imagePath);
            if (fileCheck.Exists == false)
            {
                AppendText(string.Format("{0} 不存在\r\n", fileCheck.Name));
                return false;
            }
            else if (fileCheck.Length > MaxSize)
            {
                AppendText(string.Format("{0} 图片大小超过2M，请压缩后再导入\r\n", fileCheck.Name));
                return false;
            }
            else if (fileCheck.Length < 2)
            {
                AppendText(string.Format("{0} 图像质量太小，请重新选择\r\n", fileCheck.Name));
                return false;
            }
            return true;
        }
        #endregion
    }
}
