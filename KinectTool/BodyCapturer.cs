using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KinectTool
{
    /// <summary>
    /// Kinect 骨骼数据采集器
    /// </summary>
    public class BodyCapturer : KinectCapturer
    {

        /// <summary>
        /// 骨骼数据捕获
        /// </summary>
        private BodyFrameReader bodyReader;

        /// <summary>
        /// 骨骼数据帧到达事件
        /// </summary>
        public event Action<Body[]> FrameArrived;

        /// <summary>
        /// 坐标映射器
        /// </summary>
        private CoordinateMapper coordinateMapper;
        
        /// <summary>
        /// 初始化
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            try
            {
                coordinateMapper = Sensor.CoordinateMapper;
                bodyReader = Sensor.BodyFrameSource.OpenReader();
                bodyReader.FrameArrived += OnBodyFrameArrived;
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
        }

        /// <summary>
        /// 骨骼数据帧到达事件处理方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame == null) return;

                Body[] bodies = new Body[frame.BodyCount];
                frame.GetAndRefreshBodyData(bodies);

                FrameArrived?.Invoke(bodies);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            if (bodyReader != null)
            {
                bodyReader.FrameArrived -= OnBodyFrameArrived;
                bodyReader.Dispose();
                bodyReader = null;
            }
            base.Dispose();
        }

        public CoordinateMapper GetCoordinateMapper()
        {
            return coordinateMapper;
        }
    }
}
