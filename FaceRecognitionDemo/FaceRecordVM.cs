using PropertyTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionDemo
{
    public class FaceRecordVM: Observable
    {
        public FaceRecordVM()
        {
            PIVMList = new ObservableCollection<PersonInfoVM>();
        }

        public ObservableCollection<PersonInfoVM> PIVMList
        {
            get { return this.piVMList; }
            set { SetValue(ref this.piVMList, value, nameof(PIVMList));
              
                RaisePropertyChanged(nameof(IsEditBtnEnable));
                RaisePropertyChanged(nameof(IsDeletBtnEnable));
                RaisePropertyChanged(nameof(IsClearBtnEnable));
            }
        }

        /// <summary>
        /// 编辑按钮是否可用
        /// </summary>
        public bool IsEditBtnEnable
        {
            get { return this.isEditBtnEnable; }
            set { SetValue(ref this.isEditBtnEnable, value, nameof(IsEditBtnEnable)); }
        }

        /// <summary>
        /// 删除按钮是否可用
        /// </summary>
        public bool IsDeletBtnEnable
        {
            get { return this.isDeletBtnEnable; }
            set { SetValue(ref this.isDeletBtnEnable, value, nameof(IsDeletBtnEnable)); }
        }

        /// <summary>
        /// 清空按钮是否可用
        /// </summary>
        public bool IsClearBtnEnable
        {
            get { return this.isClearBtnEnable; }
            set { SetValue(ref this.isClearBtnEnable, value, nameof(IsClearBtnEnable)); }
        }

        /// <summary>
        /// 清空按钮是否可用
        /// </summary>
        public PersonInfoVM SelectDataGridItem
        {
            get { return this.selectDataGridItem; }
            set { SetValue(ref this.selectDataGridItem, value, nameof(SelectDataGridItem)); }
        }
        
        ObservableCollection<PersonInfoVM> piVMList;

        private bool isEditBtnEnable;
        private bool isDeletBtnEnable;
        private bool isClearBtnEnable; 
        private PersonInfoVM selectDataGridItem; 
    }
}

