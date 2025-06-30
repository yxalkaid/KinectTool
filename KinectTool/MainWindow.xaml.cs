using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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

        /// <summary>
        /// UDP
        /// </summary>
        private UdpClient udpClient;

        /// <summary>
        /// 监听者 IP
        /// </summary>
        private readonly string serverIp = "127.0.0.1";

        /// <summary>
        ///  监听者端口
        /// </summary>
        private readonly int serverPort = 8081;

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

            // 释放UDP资源
            if (udpClient != null)
            {
                SendCommand("close");
            }
            udpClient?.Dispose();
            udpClient = null;
        }

        /// <summary>
        /// 发送UDP命令
        /// </summary>
        /// <param name="command"></param>
        private void SendCommand(string command)
        {
            try
            {
                if(this.udpClient != null)
                {
                    byte[] data = Encoding.UTF8.GetBytes(command);
                    udpClient.Send(data, data.Length, serverIp, serverPort);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送命令失败：{ex.Message}");
            }
        }

        private void initButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsConnected == false)
            { 

                if (this.RFIDSync.IsChecked == true)
                {
                    this.InitializeRFIDCapturer();
                    Console.WriteLine("初始化RFID数据采集器成功");
                }

                if (this.videoCheckBox.IsChecked == true)
                {
                    this.InitializeVideoCapturer();
                    Console.WriteLine("初始化视频采集器成功");
                }
                if (this.audioCheckBox.IsChecked == true)
                {
                    this.InitializeAudioCapturer();
                    Console.WriteLine("初始化音频采集器成功");
                }
                if (this.bodyCheckBox.IsChecked == true)
                {
                    this.InitializeBodyCapturer();
                    Console.WriteLine("初始化骨骼数据采集器成功");
                }
            }
            else
            {
                this.DisposeAll();
            }

            this.videoCheckBox.IsEnabled = IsConnected;
            this.audioCheckBox.IsEnabled = IsConnected;
            this.bodyCheckBox.IsEnabled = IsConnected;
            this.RFIDSync.IsEnabled = IsConnected;

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
            if (this.IsConnected == false)
            {
                MessageBoxResult result= MessageBox.Show(
                    this,
                    "请先连接设备",
                    "提示",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information
                );
                
                if (result == MessageBoxResult.Yes)
                {
                    return;
                }
            }

            if (this.IsRecording == false)
            {
                this.StartRFIDSaver();

                this.StartVideoSaver();
                this.StartAudioSaver();
                this.StartBodySaver();
            }
            else
            {
                this.StopRFIDSaver();

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
