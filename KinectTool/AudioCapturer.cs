using Microsoft.Kinect;
using System;
using System.Collections.Generic;

namespace KinectTool
{
    /// <summary>
    /// kinect 音频采集器
    /// </summary>
    public class AudioCapturer : KinectCapturer
    {
        /// <summary>
        /// 音频帧到达事件
        /// </summary>
        public event Action<byte[],uint> FrameArrived;

        /// <summary>
        /// 音频帧捕获
        /// </summary>
        private AudioBeamFrameReader audioReader;
        
        /// <summary>
        /// 初始化
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            try
            {
                audioReader = Sensor.AudioSource.OpenReader();
                audioReader.FrameArrived += OnAudioFrameArrived;

                Console.WriteLine("AudioCapturer Initialized");
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
        }

        /// <summary>
        /// 音频帧到达事件处理方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAudioFrameArrived(object sender, AudioBeamFrameArrivedEventArgs e)
        {
            // 获取音频波束帧列表
            using (var frameList = e.FrameReference.AcquireBeamFrames())
            {
                if (frameList == null || frameList.Count == 0)
                {
                    return; // 帧列表为空，跳过处理
                }
                IReadOnlyList<AudioBeamSubFrame> subFrameList = frameList[0].SubFrames;

                try
                {
                    foreach (var subFrame in subFrameList)
                    {
                        uint frameLength = subFrame.FrameLengthInBytes;
                        byte[] audioBuffer = new byte[frameLength];
                        subFrame.CopyFrameDataToArray(audioBuffer);

                        // 触发帧到达事件
                        this.FrameArrived?.Invoke(audioBuffer, frameLength);
                    }
                }
                catch (Exception ex)
                {
                    // 捕获异常并通知上层
                    OnErrorOccurred(ex);
                }
            }
        }


        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            if (audioReader != null)
            {
                audioReader.FrameArrived -= OnAudioFrameArrived;
                audioReader.Dispose();
                audioReader = null;
            }

            base.Dispose();
        }
    }
}
