using System;
using Microsoft.Kinect;

namespace KinectTool
{
    /// <summary>
    /// kinect 数据采集基类
    /// </summary>
    public abstract class KinectCapturer : IDisposable
    {
        /// <summary>
        /// 采集状态
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 错误发生事件
        /// </summary>
        public event Action<Exception> ErrorOccurred;

        /// <summary>
        /// 状态变更事件
        /// </summary>
        public event Action<bool> StatusChanged;

        /// <summary>
        /// kinect 传感器
        /// </summary>
        protected KinectSensor Sensor { get; private set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Initialize()
        {
            try
            {
                this.Sensor = KinectSensor.GetDefault();
                if (this.Sensor == null)
                {
                    throw new Exception("未检测到Kinect设备");
                }

                OnStatusChanged(true);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
        }


        /// <summary>
        /// 启动采集
        /// </summary>
        public virtual void Start()
        {
            if (Sensor != null && !Sensor.IsOpen)
            {
                Sensor.Open();
                IsRunning = true;
            }
        }


        /// <summary>
        /// 停止采集
        /// </summary>
        public virtual void Stop()
        {
            if (this.Sensor != null && this.Sensor.IsOpen)
            {
                this.Sensor.Close();
                this.IsRunning = false;
            }
        }


        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            Stop();
            this.Sensor = null;
        }


        /// <summary>
        /// 触发错误事件
        /// </summary>
        /// <param name="ex"></param>
        protected void OnErrorOccurred(Exception ex)
        {
            ErrorOccurred?.Invoke(ex);
        }

        /// <summary>
        /// 触发状态变更事件
        /// </summary>
        /// <param name="isRunning"></param>
        protected void OnStatusChanged(bool isRunning)
        {
            IsRunning = isRunning;
            StatusChanged?.Invoke(isRunning);
        }
    }
}

