using PropertyTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FaceRecognitionDemo
{
    public class FaceIdentificationVM : Observable
    {
        public FaceIdentificationVM()
        {

        }
        //个人信息页面
        public UserControl PersonInfoUC
        {
            get { return this.personInfoUC; }
            set { SetValue(ref this.personInfoUC, value, nameof(PersonInfoUC)); }
        }

        private UserControl personInfoUC;

    }
}

