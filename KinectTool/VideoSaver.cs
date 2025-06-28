using Microsoft.SqlServer.Server;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace KinectTool
{
    /// <summary>
    /// Kinect 视频保存器
    /// </summary>
    public class VideoSaver : IDisposable
    {
        /// <summary>
        /// 视频写入器
        /// </summary>
        private VideoWriter videoWriter;

        /// <summary>
        /// 是否正在录制
        /// </summary>
        public bool IsRecording { get; private set; }

        /// <summary>
        /// 目标尺寸
        /// </summary>
        private readonly OpenCvSharp.Size targetSize;

        /// <summary>
        /// 视频文件路径
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// 录制开始事件
        /// </summary>
        public event Action RecordingStarted;

        /// <summary>
        /// 录制结束事件
        /// </summary>
        public event Action RecordingStopped;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="parentDir">保存视频的目录</param>
        /// <param name="width">视频宽度</param>
        /// <param name="height">视频高度</param>
        /// <param name="fps">帧率</param>
        public VideoSaver(string parentDir, int width, int height, double fps = 30.0)
        {
            // 创建目录
            if (!Directory.Exists(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }

            // 生成文件路径
            this.FilePath = Path.Combine(parentDir, $"video_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");

            // 初始化视频写入器
            this.targetSize = new OpenCvSharp.Size(width, height);
            this.videoWriter = new VideoWriter(
                this.FilePath,
                FourCC.XVID,
                fps,
                this.targetSize,
                true
            );

            if (!this.videoWriter.IsOpened())
            {
                throw new IOException("无法打开视频写入器，可能权限不足或路径无效。");
            }
        }

        /// <summary>
        /// 写入一帧图像到视频
        /// </summary>
        public void WriteFrame(WriteableBitmap image)
        {

            if (!IsRecording)
                return;

            try
            {
                using(Mat bgraMat = image.ToMat())
                {
                    using(Mat mat=new Mat())
                    {
                        Cv2.CvtColor(bgraMat, mat, ColorConversionCodes.BGRA2BGR);
                        if (mat.Empty()) return;

                        // 如果尺寸不匹配则缩放
                        if (mat.Size() != this.targetSize)
                        {
                            using (var resized = new Mat())
                            {
                                Cv2.Resize(mat, resized, this.targetSize, 0, 0, InterpolationFlags.Linear);
                                this.videoWriter.Write(resized);
                            }
                        }
                        else
                        {
                            this.videoWriter.Write(mat);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入视频帧失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 开始录制
        /// </summary>
        public void Start()
        {
            if (!IsRecording)
            {
                this.IsRecording = true;
                this.RecordingStarted?.Invoke();
            }
        }

        /// <summary>
        /// 停止录制
        /// </summary>
        public void Stop()
        {
            if (IsRecording)
            {
                this.IsRecording = false;
                this.RecordingStopped?.Invoke();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.Stop();
            if (this.videoWriter != null)
            {
                this.videoWriter.Release(); // 释放底层资源
                this.videoWriter.Dispose(); // 释放对象
                this.videoWriter = null;
            }
        }
    }
}