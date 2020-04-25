using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyTools;

namespace FaceRecognitionDemo
{
    public  class PersonInfoVM : Observable,IPresonInformation
    {
        public PersonInfoVM()
        {
            isPersonInfoReadOnly = false;

            PersonSchoolLabel = "学校";
            PersonClassLabel = "班级";
            PersonNameLabel = "姓名";
            PersonGenderLabel = "性别";
            PersonAgeLabel = "年龄";
            PersonNumberLabel = "学号";
        }

        //个人信息是否只读
        public bool IsPersonInfoReadOnly
        {
            get { return this.isPersonInfoReadOnly; }
            set { this.SetValue(ref this.isPersonInfoReadOnly, value, nameof(IsPersonInfoReadOnly)); }
        }
        

        //红外照片
        public string InfraredSourceImage
        {
            get { return this.infraredSourceImage; }
            set { this.SetValue(ref this.infraredSourceImage, value, nameof(InfraredSourceImage)); }
        }

        //自然照片
        public string NaturalSourceImage
        {
            get { return this.naturalSourceImage; }
            set { this.SetValue(ref this.naturalSourceImage, value, nameof(NaturalSourceImage)); }
        }

        //自然照片编号
        public int NaturalImageID
        {
            get { return this.naturalImageID; }
            set { this.SetValue(ref this.naturalImageID, value, nameof(NaturalImageID)); }
        }

        //人脸特征
        public IntPtr FaceCharacteristics
        {
            get { return this.faceCharacteristics; }
            set { this.SetValue(ref this.faceCharacteristics, value, nameof(FaceCharacteristics));
                FaceCharacteristicsAction?.Invoke();
            }
        }

        public string FaceRecordBtnName { get; set; }

        #region personInforma 个人信息
        //学校
        public string PersonSchoolLabel { get; set; }
        public string PersonSchool
        {
            get { return this.personSchool; }
            set { this.SetValue(ref this.personSchool, value, nameof(PersonSchool)); }
        }
        //班级
        public string PersonClassLabel { get; set; }
        public string PersonClass
        {
            get { return this.personClass; }
            set { this.SetValue(ref this.personClass, value, nameof(PersonClass)); }
        }
        //姓名
        public string PersonNameLabel { get; set; }
        public string PersonName
        {
            get { return this.personName; }
            set { this.SetValue(ref this.personName, value, nameof(PersonName)); }
        }
        //性别
        public string PersonGenderLabel { get; set; }
        public string PersonGender
        {
            get { return this.personGender; }
            set { this.SetValue(ref this.personGender, value, nameof(PersonGender)); }
        }
        //年龄
        public string PersonAgeLabel { get; set; }
        public string PersonAge
        {
            get { return this.personAge; }
            set { this.SetValue(ref this.personAge, value, nameof(PersonAge)); }
        }
        //学号
        public string PersonNumberLabel { get; set; }
        public string PersonNumber
        {
            get { return this.personNumber; }
            set { this.SetValue(ref this.personNumber, value, nameof(PersonNumber)); }
        }

        /// <summary>
        /// 人脸特征值变化Action
        /// </summary>
        public Action FaceCharacteristicsAction;

        #endregion
        private bool isPersonInfoReadOnly;

        private string infraredSourceImage;
        private string naturalSourceImage;
        private int naturalImageID = -1;
        private IntPtr faceCharacteristics;

        private string personSchool;
        private string personClass;
        private string personName;
        private string personGender;
        private string personAge;
        private string personNumber;

        public void ClearePersonInf()
        {
            infraredSourceImage = null;
            NaturalSourceImage = null;
            NaturalImageID = -1;
            FaceCharacteristics = IntPtr.Zero;

            PersonSchool = null;
            PersonClass = null;
            PersonName = null;
            PersonGender = null;
            PersonAge = null;
            PersonNumber = null;
        }

        public object Clone()
        {
            PersonInfoVM pivm = new PersonInfoVM()
            {
                PersonSchoolLabel = this.PersonSchoolLabel,
                PersonClassLabel = this.PersonClassLabel,
                PersonNameLabel = this.PersonNameLabel,
                PersonGenderLabel = this.PersonGenderLabel,
                PersonAgeLabel = this.PersonAgeLabel,
                PersonNumberLabel = this.PersonNumberLabel,

                naturalSourceImage = this.NaturalSourceImage,
                infraredSourceImage = this.InfraredSourceImage,
                naturalImageID = this.NaturalImageID,
                faceCharacteristics = this.FaceCharacteristics,

                personSchool = this.PersonSchool,
                personClass = this.PersonClass,
                personName = this.PersonName,
                personGender = this.PersonGender,
                personAge = this.PersonAge,
                personNumber = this.PersonNumber,
              
            };
            return pivm;
        }

    }
}
