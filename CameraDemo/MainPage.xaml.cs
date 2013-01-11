using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CameraDemo.Resources;
using System.Threading;
using Microsoft.Devices;
using System.Windows.Media;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;

namespace CameraDemo
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            Initialize();
        }

        public void Initialize()
        {
            LayoutRoot.Width = Application.Current.Host.Content.ActualWidth;
            LayoutRoot.Height = Application.Current.Host.Content.ActualHeight;
            this.OrientationChanged += MainPage_OrientationChanged;
            _cameraManager = new CameraManager();
            CategoryView.ItemsSource = DictionaryClass.Instance.CategoryData.Values;
            ItemsView.ItemsSource = _cameraManager.EffectData;
            _cameraManager.CategoryChanged("Effect");
        }

        /// <summary>
        /// 通过Landscape方向及镜头前后判断旋转角度及移动位置
        /// </summary>
        /// <param name="isLeft">是否LandscapeLeft</param>
        /// <param name="isFacing">是否镜头</param>
        protected void LandscapeOrientation(bool isLeft, bool isFacing)
        {
            if ((isFacing && isLeft) || (!isFacing && !isLeft))
            {
                DrawingSurface.Margin = new Thickness(160, 0, 0, 0);
                drawingSurface_CompositeTransform.Rotation = 90;
            }
            else
            {
                drawingSurface_CompositeTransform.Rotation = 270;
            }

            //使得所有都是宽大于高的比例
            if (LayoutRoot.Height > LayoutRoot.Width)
            {
                double temp = LayoutRoot.Width;
                LayoutRoot.Width = LayoutRoot.Height;
                LayoutRoot.Height = temp;
            }
        }

        protected void MainPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            bool isFacing = false;
            if (cam != null && cam.CameraType == CameraType.FrontFacing)
            {
                isFacing = true;
                //左右翻转
                drawingSurface_CompositeTransform.ScaleX = -1;
                drawingSurface_CompositeTransform.Rotation = 180;
            }
            else
            {
                drawingSurface_CompositeTransform.Rotation = 0;
            }

            DrawingSurface.Margin = new Thickness(0, 0, 0, 0);
            switch (e.Orientation)
            {
                case PageOrientation.LandscapeRight:
                    VisualStateManager.GoToState(this, "Landscape", true);
                    LandscapeOrientation(false, isFacing);
                    break;
                case PageOrientation.LandscapeLeft:
                    VisualStateManager.GoToState(this, "Landscape", true);
                    LandscapeOrientation(true, isFacing);
                    break;
                case PageOrientation.PortraitDown:
                case PageOrientation.PortraitUp:
                    if (LayoutRoot.Width > LayoutRoot.Height)
                    {
                        double temp = LayoutRoot.Width;
                        LayoutRoot.Width = LayoutRoot.Height;
                        LayoutRoot.Height = temp;
                    }
                    VisualStateManager.GoToState(this, "Portrait", true);
                    break;
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (!PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing))
            {
                SwitchCamera.Visibility = Visibility.Collapsed;
            }

            SwitchCamera_Tap(null, null);

            // 拍照按钮事件
            CameraButtons.ShutterKeyHalfPressed += OnButtonHalfPress;
            CameraButtons.ShutterKeyPressed += OnButtonFullPress;
            CameraButtons.ShutterKeyReleased += OnButtonRelease;

            _cameraManager.Star_Animation(grid);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            pumpARGBFrames = false;
            cam.Dispose();
            cam.Initialized -= cam_Initialized;
            cam = null;
            CameraButtons.ShutterKeyHalfPressed-= OnButtonHalfPress;
            CameraButtons.ShutterKeyPressed -= OnButtonFullPress;
            CameraButtons.ShutterKeyReleased -= OnButtonRelease;
        }

        private void DrawingSurface_Loaded(object sender, RoutedEventArgs e)
        {
            _cameraManager.DrawingSurfaceLoad(sender as DrawingSurface);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (pumpARGBFrames)
            {
                _cameraManager.SliderValueChanged((sender as Slider).Value);
            }
        }

        private void LayoutRoot_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            if (e.IsInertial)
            {
                e.Complete();
                return;
            }
        }

        private void LayoutRoot_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            Point p = e.TotalManipulation.Translation;
            double length = Math.Sqrt(p.X * p.X + p.Y + p.Y);
            if (length > 20)
            {
                //与点击位置相距大于20,切换效果
                if (this.Orientation == PageOrientation.PortraitUp)
                {
                    p.X = -p.X;
                }
                // 判断左滑动往左切换
                if (p.X < 0)
                {
                   _cameraManager.TranslationChanged(true, scrollViewer, 110);
                }
                else
                {
                   _cameraManager.TranslationChanged(false, scrollViewer, 110);
                }
                _cameraManager.SetSliderVisiblity(slider);
            }
        }

        private void btnCategory_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            _cameraManager.CategoryChanged(element.Tag.ToString());
            scrollViewer.ScrollToHorizontalOffset(0);
            scrollViewer.ScrollToVerticalOffset(0);
        }

        /// <summary>
        /// 选择效果
        /// </summary>
        private void Effect_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            int uid = Convert.ToInt32(element.Tag);
            _cameraManager.SelectedChanged(uid);
            _cameraManager.SetSliderVisiblity(slider);

        }

        private void SwitchCamera_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                //是否有摄像头
                if ((PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true) ||
                   (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing) == true))
                {
                    //是否已创建了
                    if (cam != null)
                    {
                        pumpARGBFrames = false;
                        cam.Initialized -= cam_Initialized;
                        cam.Dispose();
                    }

                    //是否支持前置相机 且 当前已在用后置相机
                    if (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing) && cam != null && cam.CameraType == CameraType.Primary)
                    {
                        cam = null;
                        cam = new Microsoft.Devices.PhotoCamera(CameraType.FrontFacing);
                        drawingSurface_CompositeTransform.ScaleX = -1;
                        switch (this.Orientation)
                        {
                            case PageOrientation.PortraitUp:
                            case PageOrientation.PortraitDown:
                                drawingSurface_CompositeTransform.Rotation = 180;
                                break;
                            case PageOrientation.LandscapeLeft:
                                DrawingSurface.Margin = new Thickness(160, 0, 0, 0);
                                drawingSurface_CompositeTransform.Rotation = 90;
                                break;
                            case PageOrientation.LandscapeRight:
                                drawingSurface_CompositeTransform.Rotation = 270;
                                break;
                        }
                    }
                    else
                    {
                        if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary))
                        {
                            cam = null;
                            cam = new Microsoft.Devices.PhotoCamera(CameraType.Primary);
                        }
                    }
                    VideoBrush viewfinderBrush = new VideoBrush();
                    cam.Initialized += new EventHandler<Microsoft.Devices.CameraOperationCompletedEventArgs>(cam_Initialized);

                    viewfinderBrush.SetSource(cam);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void cam_Initialized(object sender, CameraOperationCompletedEventArgs e)
        {
            if (e.Succeeded)
            {
                try
                {
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        pumpARGBFrames = true;
                        ARGBFramesThread = new System.Threading.Thread(RunUpdate);
                        ARGBFramesThread.Start();
                        //启动完按钮才可用
                        btnBinding.IsEnabled = true;
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }

            }
        }

        private void OnButtonHalfPress(object sender, EventArgs e)
        {
            if (cam != null)
            {
                if (pumpARGBFrames)
                {
                    try
                    {
                        cam.Focus();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
        }

        private void OnButtonFullPress(object sender, EventArgs e)
        {
            if (cam != null)
            {
                if (pumpARGBFrames)
                {
                    try
                    {
                        Save_Tapped(null, null);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("OnButtonFullPress:" + ex.Message);
                    }
                }
            }
        }

        private void OnButtonRelease(object sender, EventArgs e)
        {
            if (cam != null)
            {
                if (pumpARGBFrames)
                {
                    try
                    {
                        cam.CancelFocus();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
        }

        private void Save_Tapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (cam != null)
            {
                if (cam.IsFocusAtPointSupported == true)
                {
                    try
                    {
                        // Determine location of tap.
                        System.Windows.Point tapLocation = e.GetPosition(DrawingSurface);

                        // Determine focus point.
                        double focusXPercentage = tapLocation.X / DrawingSurface.Width;
                        double focusYPercentage = tapLocation.Y / DrawingSurface.Height;

                        cam.FocusAtPoint(focusXPercentage, focusYPercentage);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                //关闭RunUpdate线程操作
                pumpARGBFrames = false;
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                 {
                     _cameraManager.Create_And_Run_Animation(DrawGrid, new Point(DrawGrid.Height / 2, DrawGrid.Width / 2), 1, 1);
                 });
                WriteableBitmap bmp = new WriteableBitmap(DrawingSurface, null);
                _cameraManager.SaveImage(bmp);
                //_cameraManager.SaveImage(bmp);
                
                //启动RunUpdate线程操作
                pumpARGBFrames = true;
                ARGBFramesThread = new System.Threading.Thread(RunUpdate);
                ARGBFramesThread.Start();
                pauseFramesEvent.Set();
            }
        }

        private void RunUpdate()
        {
            int[] ARGBPx = new int[(int)cam.PreviewResolution.Width * (int)cam.PreviewResolution.Height];
            while (pumpARGBFrames)
            {
                try
                {
                    pauseFramesEvent.WaitOne();
                    if (!pumpARGBFrames) return;
                    cam.GetPreviewBufferArgb32(ARGBPx);
                    pauseFramesEvent.Reset();
                    Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        if (pumpARGBFrames)
                        {
                            _cameraManager.Process(ARGBPx, (int)cam.PreviewResolution.Width, (int)cam.PreviewResolution.Height);
                        }
                        pauseFramesEvent.Set();
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("RunUpdate:" + ex.Message);
                }
            }
        }

        private bool pumpARGBFrames;
        private static ManualResetEvent pauseFramesEvent = new ManualResetEvent(true);
        private Thread ARGBFramesThread;
        CameraManager _cameraManager;
        private PhotoCamera cam;
    }
}