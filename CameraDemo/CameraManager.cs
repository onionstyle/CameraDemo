using Microsoft.Xna.Framework.Media;
using PhoneXamlDirect3DApp1Comp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using Windows.Storage;

namespace CameraDemo
{

    class CameraManager
    {
        public void SetSliderVisiblity(Slider slider)
        {
            if (_selecter != 0 || _selectCategory.Tag == "Particle")
            {
                slider.Visibility = Visibility.Visible;
            }
            else
            {
                slider.Visibility = Visibility.Collapsed;
            }
        }

        //启动动画
        public void Star_Animation(Grid grid)
        {  
            DoubleAnimation myDoubleAnimationY = new DoubleAnimation();
            myDoubleAnimationY.Duration = new Duration(TimeSpan.FromSeconds((float)1));
            //0表示初始未变化
            myDoubleAnimationY.From = 100;
            //变化的值
            myDoubleAnimationY.To = 0;
            //设置要变化的对象
            Storyboard.SetTarget(myDoubleAnimationY, grid.RenderTransform as CompositeTransform);
            //设置要变化的属性
            Storyboard.SetTargetProperty(myDoubleAnimationY, new PropertyPath(CompositeTransform.TranslateYProperty));
            Storyboard sb = new Storyboard();
            sb.Duration = myDoubleAnimationY.Duration;
            sb.Children.Add(myDoubleAnimationY);
            // 开始变化
            sb.Begin();
        
        }

        /// <summary>
        /// 保存的变化动画
        /// </summary>
        /// <param name="p">结束时的位置</param>
        public void Create_And_Run_Animation(Grid grid, Point p, double width, double height)
        {
            //变化矩阵
            CompositeTransform moveTransform = grid.RenderTransform as CompositeTransform;
            //创建两个DoubleAnimation于用于X,y的变化
            DoubleAnimation myDoubleAnimationX = new DoubleAnimation();
            DoubleAnimation myDoubleAnimationY = new DoubleAnimation();
            //创建两个DoubleAnimation于用于长宽的变化
            DoubleAnimation myDoubleAnimationScanleX = new DoubleAnimation();
            DoubleAnimation myDoubleAnimationScanleY = new DoubleAnimation();

            //变化的时间
            myDoubleAnimationX.Duration = new Duration(TimeSpan.FromSeconds((float)0.3));
            myDoubleAnimationY.Duration = myDoubleAnimationX.Duration;
            myDoubleAnimationScanleX.Duration = myDoubleAnimationX.Duration;
            myDoubleAnimationScanleY.Duration = myDoubleAnimationX.Duration;

            //0表示初始未变化
            myDoubleAnimationX.From = 0;
            myDoubleAnimationY.From = 0;
            myDoubleAnimationScanleX.From = 1;
            myDoubleAnimationScanleY.From = 1;

            //变化的值
            myDoubleAnimationX.To = p.X - grid.Margin.Left;
            myDoubleAnimationY.To = p.Y - grid.Margin.Top;
            //变换的比例
            double tempRateX = width / grid.Width;
            double tempRateY = height / grid.Height;
            myDoubleAnimationScanleX.To = tempRateX;
            myDoubleAnimationScanleY.To = tempRateY;

            //设置要变化的对象
            Storyboard.SetTarget(myDoubleAnimationX, moveTransform);
            Storyboard.SetTarget(myDoubleAnimationY, moveTransform);
            Storyboard.SetTarget(myDoubleAnimationScanleX, moveTransform);
            Storyboard.SetTarget(myDoubleAnimationScanleY, moveTransform);

            //设置要变化的属性
            Storyboard.SetTargetProperty(myDoubleAnimationX, new PropertyPath(CompositeTransform.TranslateXProperty));
            Storyboard.SetTargetProperty(myDoubleAnimationY, new PropertyPath(CompositeTransform.TranslateYProperty));
            Storyboard.SetTargetProperty(myDoubleAnimationScanleX, new PropertyPath(CompositeTransform.ScaleXProperty));
            Storyboard.SetTargetProperty(myDoubleAnimationScanleY, new PropertyPath(CompositeTransform.ScaleYProperty));

            //变化主体
            Storyboard sb = new Storyboard();
            sb.Duration = myDoubleAnimationX.Duration;

            sb.Children.Add(myDoubleAnimationX);
            sb.Children.Add(myDoubleAnimationY);
            sb.Children.Add(myDoubleAnimationScanleX);
            sb.Children.Add(myDoubleAnimationScanleY);

            // 开始变化
            sb.Begin();

            sb.Completed += delegate
            {
              //  grid.Background = new SolidColorBrush(Colors.Transparent);
                //等待变化结束设置图片
                moveTransform.TranslateX = 0;
                moveTransform.TranslateY = 0;
                moveTransform.ScaleX = 1;
                moveTransform.ScaleY = 1;
            };
        }

        public void SaveImage(WriteableBitmap bmp)
        {
            string fileName = String.Format("MTXX_{0:yyyyMMddHHmmss}.jpg", DateTime.Now);
            try
            {
                using (var myStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var myFileStream = myStore.CreateFile(fileName))
                    {
                        //图片存成图片流
                        Extensions.SaveJpeg(bmp, myFileStream, bmp.PixelWidth, bmp.PixelHeight, 0, 85);

                        myFileStream.Seek(0, SeekOrigin.Begin);
                        //存到保存图片文件夹内 
                        library.SavePictureToCameraRoll(fileName, myFileStream);
                    }
                }
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageStream:" + ex.Message);
            }
        }

        public void DrawingSurfaceLoad(DrawingSurface drawingSurface)
        {
            // Set window bounds in dips
            m_d3dInterop.WindowBounds = new Windows.Foundation.Size(
                (float)drawingSurface.ActualWidth,
                (float)drawingSurface.ActualHeight
                );

            // Set native resolution in pixels
            m_d3dInterop.NativeResolution = new Windows.Foundation.Size(
                (float)Math.Floor(drawingSurface.ActualWidth * Application.Current.Host.Content.ScaleFactor / 100.0f + 0.5f),
                (float)Math.Floor(drawingSurface.ActualHeight * Application.Current.Host.Content.ScaleFactor / 100.0f + 0.5f)
                );

            // Set render resolution to the full native resolution
            m_d3dInterop.RenderResolution = m_d3dInterop.NativeResolution;
    
            // Hook-up native component to DrawingSurface
            drawingSurface.SetContentProvider(m_d3dInterop.CreateContentProvider());
            drawingSurface.SetManipulationHandler(m_d3dInterop);
        }

        public void Process(int[] data, int width, int height)
        {  //调用算法
            m_d3dInterop.SetData(data, width, height);
        }

        public void SliderValueChanged(double value)
        {
            m_d3dInterop.SetAlpha((float)(value / 100));
        }

        /// <summary>
        /// 滑动改变
        /// </summary>
        /// <param name="isLeft">滑动方向 </param>
        /// <param name="scrollViewer">自动调整的控件</param>
        /// <param name="width">单个间隔的宽度</param>
        public int TranslationChanged(bool isLeft,ScrollViewer scrollViewer,double width)
        {
            int newSelected;
            if (!isLeft)
            {
                newSelected = _selecter + 1;
            }
            else
            {
                newSelected = _selecter - 1;
            }
            
            //循环切换
            if (newSelected < 0) newSelected = _effects.Count - 1;
            if (newSelected >= _effects.Count) newSelected = 0;
            
            //自动调整位置
            if (newSelected >= 3)
            {
                //判断方向
                if (scrollViewer.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled)
                {
                    scrollViewer.ScrollToHorizontalOffset(width * (newSelected - 2));
                }
                else
                {
                    scrollViewer.ScrollToVerticalOffset(width * (newSelected - 2));
                }
            }
            else
            {
                if (scrollViewer.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled)
                {
                    scrollViewer.ScrollToHorizontalOffset(0);
                }
                else
                {
                    scrollViewer.ScrollToVerticalOffset(0);
                }
            }

            //变化效果
            SelectedChanged(newSelected);
            
            return newSelected;
        }

        /// <summary>
        /// 改变选中
        /// </summary>
        public void SelectedChanged(int newSelected)
        {
            if ( newSelected >= 0 && newSelected < _effects.Count)
            {
                _effects[_selecter].BorderBrush = new SolidColorBrush(Colors.White);
                _effects[newSelected].BorderBrush = new SolidColorBrush(Color.FromArgb(255, 171, 216, 249));
                _selecter = newSelected;
                
                switch (_selectCategory.Tag)
                {
                    case "Effect":
                    case "Len":
                        m_d3dInterop.ChangeEffect(DictionaryClass.Instance.ContentToEffectType[_effects[newSelected].Content], "Assets/Camera/Effect/");
                        break;
                    case "Particle":
                        m_d3dInterop.ChangeParticle(DictionaryClass.Instance.ContentToParticleType[_effects[newSelected].Content].Key,  DictionaryClass.Instance.ContentToParticleType[_effects[newSelected].Content].Value);
                        break;
                }
            }
        }

        async public void CategoryChanged(string category)
        {
            if(_selectCategory!=null)
            {
               _selectCategory.Background = new SolidColorBrush(Colors.White);
               _selectCategory.Foreground = new SolidColorBrush(Colors.Black);
            }
            DictionaryClass.Instance.CategoryData[category].Background = new SolidColorBrush(Color.FromArgb(255, 80, 137, 194));
            DictionaryClass.Instance.CategoryData[category].Foreground = new SolidColorBrush(Colors.White);
            _selectCategory = DictionaryClass.Instance.CategoryData[category];
           
           await ReadFile("Assets/"+category);
        }

        /// <summary>
        /// 读取图片文件,绑定到EffectData
        /// </summary>
        /// <returns></returns>
        private async Task ReadFile(string fileFolder)
        {
            try
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(fileFolder);
                IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                if (files.Count > 0)
                {
                    //清楚原来绑定
                    _effects.Clear();
                    _selecter =0;

                    //添加原图
                    string originPath="Assets/origin.png";
                    Uri originUri = new Uri(originPath, UriKind.Relative);
                    BitmapImage originBmp = new BitmapImage();
                    originBmp.SetSource(Application.GetResourceStream(originUri).Stream);
                    Effect originEffect = new Effect()
                    {
                        Tag = _effects.Count.ToString(),
                        SourcePath = originBmp,
                        Content = DictionaryClass.Instance.FileToContent["origin.png"],
                        BorderBrush = new SolidColorBrush(Color.FromArgb(255, 80, 137, 194))
                    };
                    _effects.Add(originEffect);


                    foreach (var item in files)
                    {
                        Uri uri = new Uri(item.Path, UriKind.Relative);
                        StreamResourceInfo resourceInfo = Application.GetResourceStream(uri);
                        BitmapImage bmp = new BitmapImage();
                        bmp.SetSource(resourceInfo.Stream);
                        Effect effect = new Effect()
                        {
                            Tag = _effects.Count.ToString(),
                            SourcePath = bmp,
                            Content = DictionaryClass.Instance.FileToContent[item.Name],
                            BorderBrush = new SolidColorBrush(Colors.White)
                        };
                        _effects.Add(effect);
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());            
            }
        }


        CategoryClass _selectCategory;
        MediaLibrary library = new MediaLibrary();
        private Direct3DInterop m_d3dInterop= new Direct3DInterop();
        /// <summary>
        /// 现在选中
        /// </summary>
        private int _selecter = 0;
        private ObservableCollection<Effect> _effects;
        public ObservableCollection<Effect> EffectData
        {
            get { return _effects ?? (_effects = new ObservableCollection<Effect>()); }
        }
    }
}
