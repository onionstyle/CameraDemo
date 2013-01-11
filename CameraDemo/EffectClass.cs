
using System.ComponentModel;
using System.Windows.Media;
namespace CameraDemo
{
    public class Effect : INotifyPropertyChanged
    {
        private string _tag;
        private string _content;
        private ImageSource _sourcePath;
        private Brush _borderPath;

        public string Tag
        {
            get { return _tag; }
            set
            {
                _tag = value;
                RaisePropertyChanged("Tag");
            }
        }
        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                RaisePropertyChanged("Content");
            }
        
        }

        /// <summary>
        /// 图片源
        /// </summary>
        public ImageSource SourcePath
        {
            get { return _sourcePath; }
            set
            {
                _sourcePath = value;
                RaisePropertyChanged("SourcePath");
            }
        }
        public Brush BorderBrush
        {
            get { return _borderPath; }
            set
            {
                _borderPath = value;
                RaisePropertyChanged("BorderBrush");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
