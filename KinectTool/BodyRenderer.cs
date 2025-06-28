using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace KinectTool
{
    /// <summary>
    /// 骨骼图形绘制
    /// </summary>
    public class BodyRenderer
    {
        private readonly CoordinateMapper coordinateMapper;

        private const double JointThickness = 3;
        private const double HandSize = 30;

        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        private List<Pen> bodyColors;

        private List<Tuple<JointType, JointType>> bones;

        private int displayWidth;
        private int displayHeight;

        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;

        public BodyRenderer(CoordinateMapper mapper, int colorWidth, int colorHeight)
        {
            this.coordinateMapper = mapper;
            this.displayWidth = colorWidth;
            this.displayHeight = colorHeight;

            this.drawingGroup = new DrawingGroup();
            this.imageSource = new DrawingImage(this.drawingGroup);

            // 初始化骨骼连接关系
            this.InitializeBones();

            // 初始化颜色
            this.bodyColors = new List<Pen>
            {
                new Pen(Brushes.Red, 6),
                new Pen(Brushes.Orange, 6),
                new Pen(Brushes.Green, 6),
                new Pen(Brushes.Blue, 6),
                new Pen(Brushes.Indigo, 6),
                new Pen(Brushes.Violet, 6)
            };
        }

        /// <summary>
        /// 初始化骨骼
        /// </summary>
        private void InitializeBones()
        {
            this.bones = new List<Tuple<JointType, JointType>>();

            // Torso
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Left Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // Right Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            // Left Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));
        }

        /// <summary>
        /// 处理一帧 BodyFrame 数据并更新绘制图像
        /// </summary>
        public void ProcessFrame(Body[] bodies)
        {
            if (bodies == null)
                return;

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // 清空画布并设置背景
                dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, this.displayWidth, this.displayHeight));

                int penIndex = 0;
                foreach (Body body in bodies)
                {
                    Pen drawPen = this.bodyColors[penIndex++ % this.bodyColors.Count];

                    if (body.IsTracked)
                    {
                        IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                        Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                        foreach (JointType jointType in joints.Keys)
                        {
                            CameraSpacePoint position = joints[jointType].Position;
                            if (position.Z < 0)
                            {
                                position.Z = 0.1f; // 避免负值
                            }

                            
                            ColorSpacePoint colorPoint = this.coordinateMapper.MapCameraPointToColorSpace(position);
                            jointPoints[jointType] = new Point(colorPoint.X, colorPoint.Y);
                        }

                        this.DrawBody(joints, jointPoints, dc, drawPen);
                        this.DrawHand(body.HandLeftState, jointPoints[JointType.HandLeft], dc);
                        this.DrawHand(body.HandRightState, jointPoints[JointType.HandRight], dc);
                    }
                }

                // 设置裁剪区域防止越界
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0, 0, this.displayWidth, this.displayHeight));
            }
        }

        private void DrawBody(
            IReadOnlyDictionary<JointType, Joint> joints, 
            IDictionary<JointType, Point> jointPoints, 
            DrawingContext dc, Pen drawingPen)
        {
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, dc, drawingPen);
            }

            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;
                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    dc.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);
                }
            }
        }


        private void DrawBone(
            IReadOnlyDictionary<JointType, Joint> joints, 
            IDictionary<JointType, Point> jointPoints, 
            JointType jt0, JointType jt1, 
            DrawingContext dc, Pen drawingPen)
        {
            Joint j0 = joints[jt0];
            Joint j1 = joints[jt1];

            if (j0.TrackingState == TrackingState.NotTracked || j1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            Pen drawPen = this.inferredBonePen;
            if (j0.TrackingState == TrackingState.Tracked && j1.TrackingState == TrackingState.Tracked)
            {
                drawPen = drawingPen;
            }

            dc.DrawLine(drawPen, jointPoints[jt0], jointPoints[jt1]);
        }

        private void DrawHand(HandState handState, Point handPosition, DrawingContext dc)
        {
            switch (handState)
            {
                case HandState.Closed:
                    dc.DrawEllipse(Brushes.Red, null, handPosition, HandSize, HandSize);
                    break;
                case HandState.Open:
                    dc.DrawEllipse(Brushes.Green, null, handPosition, HandSize, HandSize);
                    break;
                case HandState.Lasso:
                    dc.DrawEllipse(Brushes.Blue, null, handPosition, HandSize, HandSize);
                    break;
            }
        }

        /// <summary>
        /// 获取当前帧的绘制图像
        /// </summary>
        public ImageSource GetImageSource()
        {
            return this.imageSource;
        }
    }
}