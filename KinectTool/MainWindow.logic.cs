using System.Drawing;
using System.Net.Sockets;
using System.Windows;

namespace KinectTool
{
    public partial class MainWindow
    {
        /// <summary>
        /// 视频保存目录
        /// </summary>
        private readonly string videoDir = "D:/Downloads/Kinect";

        /// <summary>
        /// 音频保存目录
        /// </summary>
        private readonly string audioDir = "D:/Downloads/Kinect";

        /// <summary>
        /// 骨骼数据保存目录
        /// </summary>
        private readonly string bodyDir = "D:/Downloads/Kinect";

        /// <summary>
        /// 视频采集器
        /// </summary>
        private VideoCapturer videoCapturer;

        /// <summary>
        /// 音频采集器
        /// </summary>
        private AudioCapturer audioCapturer;

        /// <summary>
        /// 骨骼数据采集器
        /// </summary>
        private BodyCapturer bodyCapturer;

        /// <summary>
        /// 视频保存器
        /// </summary>
        private VideoSaver videoSaver;

        /// <summary>
        /// 音频保存器
        /// </summary>
        private AudioSaver audioSaver;

        /// <summary>
        /// 骨骼数据保存器
        /// </summary>
        private BodySaver bodySaver;

        /// <summary>
        /// 骨骼图形渲染器
        /// </summary>
        private BodyRenderer bodyRenderer;

        /// <summary>
        /// 初始化视频采集器
        /// </summary>
        private void InitializeVideoCapturer()
        {
            if (videoCapturer == null)
            {
                videoCapturer = new VideoCapturer();
                videoCapturer.FrameArrived += HandleVideoFrame;
                videoCapturer.Initialize();
                videoCapturer.Start();
            }
        }

        /// <summary>
        /// 初始化音频采集器
        /// </summary>
        private void InitializeAudioCapturer()
        {
            if (audioCapturer == null)
            {
                audioCapturer = new AudioCapturer();
                //audioCapturer.FrameArrived += HandleAudioFrame;
                audioCapturer.Initialize();
                audioCapturer.Start();
            }
        }

        /// <summary>
        /// 初始化骨骼数据采集器
        /// </summary>
        private void InitializeBodyCapturer()
        {
            if (bodyCapturer == null)
            {
                bodyCapturer = new BodyCapturer();
                bodyCapturer.FrameArrived += HandleBodyFrame;
                bodyCapturer.Initialize();

                bodyRenderer = new BodyRenderer(
                      bodyCapturer.GetCoordinateMapper(),
                      1920, 1080
                );

                bodyCapturer.Start();
            }
        }

        /// <summary>
        /// 初始化RFID数据采集器
        /// </summary>
        private void InitializeRFIDCapturer()
        {
            if(udpClient == null)
            {
                udpClient = new UdpClient();
                SendCommand("init");
            }
        }

        /// <summary>
        /// 开始视频录制
        /// </summary>
        /// <returns></returns>
        private bool StartVideoSaver()
        {
            if (videoCapturer == null)
            {
                return false;
            }

            if (videoSaver != null)
            {
                return false;
            }

            videoSaver = new VideoSaver(
                videoDir,
                1920, 1080
            );
            videoCapturer.FrameArrived += videoSaver.WriteFrame;
            videoSaver.Start();

            return true;
        }


        /// <summary>
        /// 停止视频录制
        /// </summary>
        /// <returns></returns>
        private bool StopVideoSaver()
        {
            if (videoSaver == null)
            {
                return false;
            }

            videoSaver.Dispose();
            videoCapturer.FrameArrived -= videoSaver.WriteFrame;
            videoSaver = null;

            return true;
        }

        /// <summary>
        /// 开始音频录制
        /// </summary>
        /// <returns></returns>
        private bool StartAudioSaver()
        {
            if (audioCapturer == null)
            {
                return false;
            }

            if (audioSaver != null)
            {
                return false;
            }

            audioSaver = new AudioSaver(
                audioDir
            );
            audioCapturer.FrameArrived += audioSaver.WriteFrame;
            audioSaver.Start();

            return true;
        }

        /// <summary>
        /// 停止音频录制
        /// </summary>
        /// <returns></returns>
        private bool StopAudioSaver()
        {
            if (audioSaver == null)
            {
                return false;
            }

            audioSaver.Dispose();
            audioCapturer.FrameArrived -= audioSaver.WriteFrame;
            audioSaver = null;

            return true;
        }

        /// <summary>
        /// 开始骨骼数据录制
        /// </summary>
        private bool StartBodySaver()
        {
            if (bodyCapturer == null)
            {
                return false;
            }

            if (bodySaver != null)
            {
                return false;
            }

            bodySaver = new BodySaver(
                bodyDir
            );
            bodyCapturer.FrameArrived += bodySaver.WriteFrame;
            bodySaver.Start();

            return true;
        }

        /// <summary>
        /// 停止骨骼数据录制
        /// </summary>
        /// <returns></returns>
        private bool StopBodySaver()
        {
            if (bodySaver == null)
            {
                return false;
            }

            bodySaver.Dispose();
            bodyCapturer.FrameArrived -= bodySaver.WriteFrame;
            bodySaver = null;

            return true;
        }

        /// <summary>
        /// 开始RFID数据录制
        /// </summary>
        private bool StartRFIDSaver()
        {
            if(udpClient == null)
            {
                return false;
            }

            this.SendCommand("start");
            return true;
        }

        /// <summary>
        /// 停止RFID数据录制
        /// </summary>
        /// <returns></returns>
        private bool StopRFIDSaver()
        {
            if(udpClient == null)
            {
                return false;
            }

            this.SendCommand("stop");
            return true;
        }
    }
}
