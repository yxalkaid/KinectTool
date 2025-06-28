using Microsoft.Kinect;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KinectTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 是否正在录制
        /// </summary>
        private bool IsRecording = false;

        /// <summary>
        /// 是否已连接
        /// </summary>
        private bool IsConnected = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // 执行关闭前的操作
            var result = MessageBox.Show(
                this,
                "确定要退出吗？",
                "确认",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            // 清理资源
            DisposeAll();

            base.OnClosing(e);
        }

        private void DisposeAll()
        {
            // 释放视频采集资源
            videoSaver?.Dispose();
            videoSaver = null;
            videoCapturer?.Dispose();
            videoCapturer = null;

            // 释放音频采集资源
            audioSaver?.Dispose();
            audioSaver = null;
            audioCapturer?.Dispose();
            audioCapturer = null;

            // 释放骨骼数据采集资源
            bodySaver?.Dispose();
            bodySaver = null;
            bodyCapturer?.Dispose();
            bodyCapturer = null;
        }

        private void initButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsConnected == false)
            {
                this.InitializeVideoCapturer();
                this.InitializeAudioCapturer();
                this.InitializeBodyCapturer();
            }
            else
            {
                this.DisposeAll();
            }

            this.IsConnected = !this.IsConnected;
            this.initButton.Content = this.IsConnected ? "Disconnect" : "Connect";
            string info = this.IsConnected ? "已连接设备" : "已断开连接";
            MessageBox.Show(
                this,
                info,
                "确认",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsRecording == false)
            {
                this.StartVideoSaver();
                this.StartAudioSaver();
                this.StartBodySaver();
            }
            else
            {
                this.StopVideoSaver();
                this.StopAudioSaver();
                this.StopBodySaver();
            }

            this.IsRecording = !this.IsRecording;
            this.startButton.Content = this.IsRecording ? "Stop" : "Start";
            string info = this.IsRecording ? "已开始录制" : "已停止录制";
            MessageBox.Show(
                this,
                info,
                "确认",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        /// <summary>
        /// 处理视频帧
        /// </summary>
        /// <param name="frame"></param>
        private void HandleVideoFrame(WriteableBitmap image)
        {
            if (image == null) 
                return;

            if (this.videoImage.Dispatcher.CheckAccess())
            {
                this.videoImage.Source = image;
            }
            else
            {
                this.Dispatcher.Invoke(() => this.videoImage.Source = image);
            }
        }

        /// <summary>
        /// 处理骨骼数据帧
        /// </summary>
        /// <param name="filteredBodies"></param>
        private void HandleBodyFrame(Body[] bodies)
        {
            if (bodies == null || this.bodyRenderer == null)
                return;

            // 渲染骨骼图形
            this.bodyRenderer.ProcessFrame(bodies);

            // 获取渲染后的图像
            var skeletonImage = this.bodyRenderer.GetImageSource();


            if (this.bodyImage.Dispatcher.CheckAccess())
            {
                this.bodyImage.Source = skeletonImage;
            }
            else
            {
                this.Dispatcher.Invoke(() => this.bodyImage.Source = skeletonImage);
            }
        }
    }
}
