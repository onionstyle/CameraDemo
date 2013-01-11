
using System.ComponentModel;
using System.Windows.Media;
namespace CameraDemo
{
    class CategoryClass : INotifyPropertyChanged
    {
        private string _tag;
        private string _content;
        private Brush _foreground;
        private Brush _backgroud;

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
        public Brush Foreground
        {
            get { return _foreground; }
            set
            {
                _foreground = value;
                RaisePropertyChanged("Foreground");
            }
        }
        
        public Brush Background
        {
            get { return _backgroud; }
            set
            {
                _backgroud = value;
                RaisePropertyChanged("Background");
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
