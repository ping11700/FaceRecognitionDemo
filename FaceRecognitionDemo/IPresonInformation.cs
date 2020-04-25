using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyTools;

namespace FaceRecognitionDemo
{
    public interface IPresonInformation
    {
        //红外光图片
        string InfraredSourceImage { get; set; }
        
        //自然光图片
        string NaturalSourceImage { get; set; }

        //自然光图片编号
        int NaturalImageID { get; set; }

        //人脸特征
        IntPtr FaceCharacteristics { get; set; }

        //学校
        string PersonSchool { get; set; }
      
        //班级
        string PersonClass { get; set; }
     
        //姓名
         string PersonName{ get; set; }
       
        //性别
         string PersonGender { get; set; }
       
        //年龄
         string PersonAge { get; set; }
       
        //学号
         string PersonNumber { get; set; }

    }
}
