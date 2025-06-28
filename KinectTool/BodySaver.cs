using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KinectTool
{
    /// <summary>
    /// kinect 骨骼数据保存器
    /// </summary>
    public class BodySaver : IDisposable
    {
        /// <summary>
        /// 骨骼数据写入器
        /// </summary>
        private StreamWriter bodyWriter;

        /// <summary>
        /// 是否正在录制
        /// </summary>
        public bool IsRecording { get; private set; }

        /// <summary>
        /// 文件保存路径
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// 录制开始事件
        /// </summary>
        public event Action RecordingStarted;

        /// <summary>
        /// 录制停止事件
        /// </summary>
        public event Action RecordingStopped;

        /// <summary>
        /// 需要保存的关节点列表
        /// </summary>
        private readonly List<JointType> RequiredJoints = new List<JointType>
        {
            JointType.SpineBase, JointType.Neck,
            JointType.HipLeft, JointType.KneeLeft,
            JointType.HipRight, JointType.KneeRight,
            JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft,
            JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight
        };

        public BodySaver(string parentDir)
        {
            if (!Directory.Exists(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }

            this.FilePath = Path.Combine(parentDir, $"body_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            FileStream fileStream = new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write);
            this.bodyWriter = new StreamWriter(fileStream) { AutoFlush = false };

            if (this.bodyWriter == null)
            {
                throw new IOException("无法打开骨骼数据写入器，可能权限不足或路径无效。");
            }

            // 写入CSV文件的表头
            WriteHeader();
        }

        /// <summary>
        /// 写入CSV文件的表头
        /// </summary>
        private void WriteHeader()
        {
            List<string> header = new List<string> { "TrackingId", "Timestamp" };
            foreach (var jointType in RequiredJoints)
            {
                header.Add($"{jointType}_X");
                header.Add($"{jointType}_Y");
                header.Add($"{jointType}_Z");
            }
            bodyWriter.WriteLine(string.Join(",", header));
        }

        /// <summary>
        /// 写入一帧骨骼数据到文件
        /// </summary>
        /// <param name="filteredBodies">包含过滤后骨骼数据的列表</param>
        public void WriteFrame(Body[] bodies)
        {
            if (!IsRecording)
                return;

            try
            {
                foreach (var body in bodies)
                {
                    if (body != null && body.IsTracked)
                    {
                        List<string> row = new List<string>();

                        // 1. 添加用户唯一标识
                        row.Add(body.TrackingId.ToString());
                        row.Add(DateTime.Now.ToString("o"));

                        // 2. 按照 _requiredJoints 的顺序添加所有指定关节的坐标
                        foreach (var jointType in RequiredJoints)
                        {
                            var position = body.Joints[jointType].Position;
                            row.Add(position.X.ToString("F6"));
                            row.Add(position.Y.ToString("F6"));
                            row.Add(position.Z.ToString("F6"));
                        }

                        string line = string.Join(",", row);
                        bodyWriter.WriteLine(line);
                    }
                }

                // 当缓存的数据量较大时，手动刷新到文件
                if (bodyWriter.BaseStream.Length > 1024 * 1024) // 1MB
                {
                    bodyWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入 CSV 数据失败: {ex.Message}");
            }
        }

        public void Start()
        {
            if (!IsRecording)
            {
                this.IsRecording = true;
                this.RecordingStarted?.Invoke();
            }
        }

        public void Stop()
        {
            if (IsRecording)
            {
                this.IsRecording = false;
                this.RecordingStopped?.Invoke();
            }
        }

        public void Dispose()
        {
            this.Stop();
            if (this.bodyWriter != null)
            {
                this.bodyWriter.Flush();
                this.bodyWriter.Dispose();
                this.bodyWriter = null;
            }
        }
    }
}
