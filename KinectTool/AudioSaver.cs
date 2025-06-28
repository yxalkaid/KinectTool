using NAudio.Wave;
using System;
using System.IO;

namespace KinectTool
{
    /// <summary>
    /// Kinect 音频保存器
    /// </summary>
    public class AudioSaver : IDisposable
    {
        /// <summary>
        /// 音频写入器
        /// </summary>
        private WaveFileWriter audioWriter;

        /// <summary>
        /// 是否正在录制
        /// </summary>
        public bool IsRecording { get; private set; }

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


        public AudioSaver(string parentDir, int sampleRate=16000, int bitCount=16,int mono=1)
        {
            // 创建目录
            if (!Directory.Exists(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }

            // 生成文件路径
            this.FilePath = Path.Combine(parentDir, $"audio_{DateTime.Now:yyyyMMdd_HHmmss}.wav");

            // 初始化音频写入器
            var waveFormat = new WaveFormat(sampleRate, bitCount, mono); // 16kHz, 16-bit, mono
            audioWriter = new WaveFileWriter(this.FilePath, waveFormat);
            if (!this.audioWriter.CanWrite)
            {
                throw new IOException("无法打开音频写入器，可能权限不足或路径无效。");
            }
        }

        /// <summary>
        /// 写入音频帧
        /// </summary>
        public void WriteFrame(byte[] frame,uint frameLength)
        {
            if (!IsRecording)
                return;

            float[] floatSamples = new float[frameLength / 4];
            Buffer.BlockCopy(frame, 0, floatSamples, 0, (int)frameLength);

            short[] pcmSamples = new short[floatSamples.Length];
            for (int i = 0; i < floatSamples.Length; i++)
            {
                pcmSamples[i] = (short)(floatSamples[i] * short.MaxValue);
            }

            // 写入音频文件
            audioWriter?.WriteSamples(pcmSamples, 0, pcmSamples.Length);
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
            audioWriter?.Dispose();
            audioWriter = null;
        }
    }
}
