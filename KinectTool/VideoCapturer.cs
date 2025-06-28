using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace KinectTool
{
    /// <summary>
    /// kinect 视频采集器
    /// </summary>
    public class VideoCapturer : KinectCapturer
    {
        /// <summary>
        /// 视频帧到达事件
        /// </summary>
        public event Action<WriteableBitmap> FrameArrived;

        /// <summary>
        /// 视频帧捕获
        /// </summary>
        private ColorFrameReader frameReader;

        /// <summary>
        /// 位图
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            try
            {
                // 配置彩色帧源
                var colorSource = this.Sensor.ColorFrameSource;

                // 创建帧读取器
                this.frameReader = colorSource.OpenReader();
                frameReader.FrameArrived += OnColorFrameArrived;

                // 初始化位图
                colorBitmap = new WriteableBitmap(
                    colorSource.FrameDescription.Width,
                    colorSource.FrameDescription.Height,
                    96, 96, PixelFormats.Bgra32, null
                );
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
        }

        /// <summary>
        /// 视频帧到达事件处理方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame == null) return;

                int width = frame.FrameDescription.Width;
                int height = frame.FrameDescription.Height;

                try
                {
                    this.colorBitmap.Lock();

                    // 直接复制到 BackBuffer
                    frame.CopyConvertedFrameDataToIntPtr(
                        this.colorBitmap.BackBuffer,
                        (uint)(width * height * 4),
                        ColorImageFormat.Bgra);

                    this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
                }
                finally
                {
                    this.colorBitmap.Unlock();
                }

                FrameArrived?.Invoke(colorBitmap);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            // 释放帧读取器
            if (frameReader != null)
            {
                frameReader.FrameArrived -= OnColorFrameArrived;
                frameReader.Dispose();
                frameReader = null;
            }

            // 释放位图
            if (colorBitmap != null)
            {
                colorBitmap = null;
            }

            base.Dispose();
        }
    }
}