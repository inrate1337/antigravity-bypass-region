using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
namespace patcher_antigravity
{
    public partial class MainWindow : Window
    {
        private int pageIndex = 0;
        public MainWindow()
        {
            InitializeComponent();
            ExtractAndLoadVideo();
        }
        private void ExtractAndLoadVideo()
        {
            try
            {
                string tempVideoPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "2e6e57eafa14e9920ae8038254bcc23c.mp4");
                if (!System.IO.File.Exists(tempVideoPath))
                {
                    using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("patcher_antigravity.Assets.2e6e57eafa14e9920ae8038254bcc23c.mp4"))
                    {
                        if (stream != null)
                        {
                            using (var fileStream = System.IO.File.Create(tempVideoPath))
                            {
                                stream.CopyTo(fileStream);
                            }
                        }
                    }
                }
                BackgroundVideo.Source = new Uri(tempVideoPath);
                BackgroundVideo.MediaOpened += BackgroundVideo_MediaOpened;
                BackgroundVideo.Play();
            }
            catch (Exception)
            {
                BackgroundVideo_MediaOpened(null, null);
            }
        }
        private async void BackgroundVideo_MediaOpened(object sender, RoutedEventArgs e)
        {
            await System.Threading.Tasks.Task.Delay(1200);
            var logoTransformGroup = LogoImage.RenderTransform as TransformGroup;
            if (logoTransformGroup == null) return;
            var logoScale = logoTransformGroup.Children[0] as ScaleTransform;
            var logoTranslate = logoTransformGroup.Children[1] as TranslateTransform;
            var textTranslate = LogoText.RenderTransform as TranslateTransform;
            var paginationTranslate = PaginationPanel.RenderTransform as TranslateTransform;
            var nextTranslate = NextButton.RenderTransform as TranslateTransform;
            if (nextTranslate == null) return;
            int totalFrames = 75; 
            int currentFrame = 0;
            int textStartFrame = 10; 
            int textDuration = 40;   
            int uiStartFrame = 40;   
            int uiDuration = 35;     
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16); 
            timer.Tick += (s, args) =>
            {
                currentFrame++;
                double logoProgress = Math.Min(1.0, (double)currentFrame / (totalFrames - textStartFrame));
                double logoEase = 1.0 - Math.Pow(1.0 - logoProgress, 4); 
                LogoImage.Opacity = logoEase;
                logoScale.ScaleX = 0.8 + (0.2 * logoEase);
                logoScale.ScaleY = 0.8 + (0.2 * logoEase);
                logoTranslate.Y = 30 * (1.0 - logoEase);
                if (currentFrame > textStartFrame)
                {
                    double textProgress = Math.Min(1.0, (double)(currentFrame - textStartFrame) / textDuration);
                    double textEase = 1.0 - Math.Pow(1.0 - textProgress, 4);
                    LogoText.Opacity = textEase;
                    textTranslate.Y = 20 * (1.0 - textEase);
                }
                if (currentFrame > uiStartFrame)
                {
                    double uiProgress = Math.Min(1.0, (double)(currentFrame - uiStartFrame) / uiDuration);
                    double uiEase = 1.0 - Math.Pow(1.0 - uiProgress, 4);
                    PaginationPanel.Opacity = uiEase;
                    paginationTranslate.Y = 10 * (1.0 - uiEase);
                    NextButton.Opacity = uiEase;
                    nextTranslate.X = -15 * (1.0 - uiEase);
                }
                if (currentFrame >= totalFrames)
                {
                    timer.Stop();
                    LogoImage.Opacity = 1.0;
                    logoScale.ScaleX = 1.0;
                    logoScale.ScaleY = 1.0;
                    logoTranslate.Y = 0;
                    LogoText.Opacity = 1.0;
                    textTranslate.Y = 0;
                    PaginationPanel.Opacity = 1.0;
                    paginationTranslate.Y = 0;
                    NextButton.Opacity = 1.0;
                    nextTranslate.X = 0;
                }
            };
            timer.Start();
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void PrevButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (pageIndex > 0) TransitionToPage(pageIndex - 1);
        }
        private void NextButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (pageIndex < 2) TransitionToPage(pageIndex + 1);
        }
        private void TransitionToPage(int targetPage)
        {
            if (pageIndex == targetPage) return;
            int oldPage = pageIndex;
            pageIndex = targetPage;
            var activeColor = (Color)ColorConverter.ConvertFromString("#E8EAED");
            var inactiveColor = (Color)ColorConverter.ConvertFromString("#5f6368");
            var dot1Brush = new SolidColorBrush(((SolidColorBrush)Dot1.Background).Color);
            var dot2Brush = new SolidColorBrush(((SolidColorBrush)Dot2.Background).Color);
            var dot3Brush = new SolidColorBrush(((SolidColorBrush)Dot3.Background).Color);
            Dot1.Background = dot1Brush;
            Dot2.Background = dot2Brush;
            Dot3.Background = dot3Brush;
            var easing = new QuarticEase { EasingMode = EasingMode.EaseOut };
            TimeSpan duration = TimeSpan.FromMilliseconds(700);
            var dot1ColorAnim = new ColorAnimation { To = pageIndex == 0 ? activeColor : inactiveColor, Duration = duration, EasingFunction = easing };
            var dot2ColorAnim = new ColorAnimation { To = pageIndex == 1 ? activeColor : inactiveColor, Duration = duration, EasingFunction = easing };
            var dot3ColorAnim = new ColorAnimation { To = pageIndex == 2 ? activeColor : inactiveColor, Duration = duration, EasingFunction = easing };
            dot1Brush.BeginAnimation(SolidColorBrush.ColorProperty, dot1ColorAnim);
            dot2Brush.BeginAnimation(SolidColorBrush.ColorProperty, dot2ColorAnim);
            dot3Brush.BeginAnimation(SolidColorBrush.ColorProperty, dot3ColorAnim);
            var dot1WidthAnim = new DoubleAnimation { To = pageIndex == 0 ? 15 : 5, Duration = duration, EasingFunction = easing };
            var dot2WidthAnim = new DoubleAnimation { To = pageIndex == 1 ? 15 : 5, Duration = duration, EasingFunction = easing };
            var dot3WidthAnim = new DoubleAnimation { To = pageIndex == 2 ? 15 : 5, Duration = duration, EasingFunction = easing };
            Dot1.BeginAnimation(Border.WidthProperty, dot1WidthAnim);
            Dot2.BeginAnimation(Border.WidthProperty, dot2WidthAnim);
            Dot3.BeginAnimation(Border.WidthProperty, dot3WidthAnim);
            double p0TargetX = (0 < pageIndex) ? -400 : (0 > pageIndex) ? 400 : 0;
            double p1TargetX = (1 < pageIndex) ? -400 : (1 > pageIndex) ? 400 : 0;
            double p2TargetX = (2 < pageIndex) ? -400 : (2 > pageIndex) ? 400 : 0;
            ContentTranslate.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation { To = p0TargetX, Duration = duration, EasingFunction = easing });
            Page2Translate.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation { To = p1TargetX, Duration = duration, EasingFunction = easing });
            Page3Translate.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation { To = p2TargetX, Duration = duration, EasingFunction = easing });
            ContentPanel.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation { To = pageIndex == 0 ? 1 : 0, Duration = duration, EasingFunction = easing });
            Page2Panel.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation { To = pageIndex == 1 ? 1 : 0, Duration = duration, EasingFunction = easing });
            Page3Panel.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation { To = pageIndex == 2 ? 1 : 0, Duration = duration, EasingFunction = easing });
            ContentPanel.IsHitTestVisible = pageIndex == 0;
            Page2Panel.IsHitTestVisible = pageIndex == 1;
            Page3Panel.IsHitTestVisible = pageIndex == 2;
            var blurAnim = new DoubleAnimationUsingKeyFrames();
            blurAnim.KeyFrames.Add(new EasingDoubleKeyFrame(25, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(250)), new QuarticEase { EasingMode = EasingMode.EaseOut }));
            blurAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(duration), new QuarticEase { EasingMode = EasingMode.EaseOut }));
            double skewAngle = (targetPage > oldPage) ? 25 : -25;
            var skewAnim = new DoubleAnimationUsingKeyFrames();
            skewAnim.KeyFrames.Add(new EasingDoubleKeyFrame(skewAngle, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(250)), new QuarticEase { EasingMode = EasingMode.EaseOut }));
            skewAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(duration), new QuarticEase { EasingMode = EasingMode.EaseOut }));
            ContentBlur.BeginAnimation(BlurEffect.RadiusProperty, blurAnim);
            ContentSkew.BeginAnimation(SkewTransform.AngleXProperty, skewAnim);
            Page2Blur.BeginAnimation(BlurEffect.RadiusProperty, blurAnim);
            Page2Skew.BeginAnimation(SkewTransform.AngleXProperty, skewAnim);
            Page3Blur.BeginAnimation(BlurEffect.RadiusProperty, blurAnim);
            Page3Skew.BeginAnimation(SkewTransform.AngleXProperty, skewAnim);
            var btnFadeIn = new DoubleAnimation { To = 1, Duration = duration, EasingFunction = easing };
            var btnFadeOut = new DoubleAnimation { To = 0, Duration = duration, EasingFunction = easing };
            bool showPrev = pageIndex == 1;
            bool showNext = pageIndex < 2;
            PrevButton.IsHitTestVisible = showPrev;
            NextButton.IsHitTestVisible = showNext;
            PrevButton.BeginAnimation(UIElement.OpacityProperty, showPrev ? btnFadeIn : btnFadeOut);
            NextButton.BeginAnimation(UIElement.OpacityProperty, showNext ? btnFadeIn : btnFadeOut);
            var nextBgOpacityAnim = new DoubleAnimation { To = pageIndex == 1 ? 1 : 0, Duration = duration, EasingFunction = easing };
            NextBtnGradientBg.BeginAnimation(UIElement.OpacityProperty, nextBgOpacityAnim);
            var currentStrokeColor = NextPath.Stroke is SolidColorBrush sb ? sb.Color : (Color)ColorConverter.ConvertFromString("#9AA0A6");
            var nextPathBrush = new SolidColorBrush(currentStrokeColor);
            NextPath.Stroke = nextPathBrush;
            var nextPathColorAnim = new ColorAnimation { To = pageIndex == 1 ? (Color)ColorConverter.ConvertFromString("#202124") : (Color)ColorConverter.ConvertFromString("#9AA0A6"), Duration = duration, EasingFunction = easing };
            nextPathBrush.BeginAnimation(SolidColorBrush.ColorProperty, nextPathColorAnim);
            if (pageIndex == 2)
            {
                var rotateAnim = new DoubleAnimation 
                { 
                    By = 360, 
                    Duration = TimeSpan.FromSeconds(1.5), 
                    RepeatBehavior = RepeatBehavior.Forever
                };
                ProgressRotation.BeginAnimation(RotateTransform.AngleProperty, rotateAnim);
                RunPatcher();
            }
            else
            {
                ProgressRotation.BeginAnimation(RotateTransform.AngleProperty, null);
            }
            var paginationFadeAnim = new DoubleAnimation
            {
                To = pageIndex == 2 ? 0 : 1,
                Duration = duration,
                EasingFunction = easing
            };
            var paginationSlideAnim = new DoubleAnimation
            {
                To = pageIndex == 2 ? 10 : 0,
                Duration = duration,
                EasingFunction = easing
            };
            PaginationPanel.BeginAnimation(UIElement.OpacityProperty, paginationFadeAnim);
            var paginationTranslate = PaginationPanel.RenderTransform as TranslateTransform;
            if (paginationTranslate != null)
            {
                paginationTranslate.BeginAnimation(TranslateTransform.YProperty, paginationSlideAnim);
            }
        }
        private void BackgroundVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            BackgroundVideo.Position = TimeSpan.Zero;
            BackgroundVideo.Play();
        }
        private async void RunPatcher()
        {
            var patcher = new patcher_antigravity.Core.PatcherService();
            patcher.OnProgress += msg => 
            {
            };
            patcher.OnError += err => 
            {
                Dispatcher.Invoke(() => {
                    UpdateProgressText(err, "#FF5252");
                    ProgressRotation.BeginAnimation(RotateTransform.AngleProperty, null);
                });
            };
            patcher.OnSuccess += async () => 
            {
                Dispatcher.Invoke(() => {
                    var fadeOutText = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(300) };
                    ProgressText.BeginAnimation(UIElement.OpacityProperty, fadeOutText);
                });
                await Task.Delay(400);
                Dispatcher.Invoke(() => {
                    var slideDown = new DoubleAnimation { 
                        To = 30, 
                        Duration = TimeSpan.FromMilliseconds(600), 
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } 
                    };
                    var fadeOutPanel = new DoubleAnimation { 
                        To = 0, 
                        Duration = TimeSpan.FromMilliseconds(600), 
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } 
                    };
                    Page3Translate.BeginAnimation(TranslateTransform.YProperty, slideDown);
                    Page3Panel.BeginAnimation(UIElement.OpacityProperty, fadeOutPanel);
                    var fadeOutPagination = new DoubleAnimation { 
                        To = 0, 
                        Duration = TimeSpan.FromMilliseconds(400) 
                    };
                    PaginationPanel.BeginAnimation(UIElement.OpacityProperty, fadeOutPagination);
                    var fadeOutWindow = new DoubleAnimation 
                    { 
                        To = 0, 
                        Duration = TimeSpan.FromMilliseconds(500),
                        BeginTime = TimeSpan.FromMilliseconds(600),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                    };
                    fadeOutWindow.Completed += (s, e) => 
                    {
                        Application.Current.Shutdown();
                    };
                    this.BeginAnimation(UIElement.OpacityProperty, fadeOutWindow);
                });
            };
            await patcher.RunPatchAsync();
        }
        private void UpdateProgressText(string newText, string colorHex)
        {
            var fadeOut = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(150) };
            fadeOut.Completed += (s, e) => 
            {
                ProgressText.Text = newText;
                ProgressText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
                var fadeIn = new DoubleAnimation { To = 1, Duration = TimeSpan.FromMilliseconds(150) };
                ProgressText.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
            ProgressText.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }
}