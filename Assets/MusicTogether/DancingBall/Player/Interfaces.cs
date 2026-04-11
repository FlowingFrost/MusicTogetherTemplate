namespace MusicTogether.DancingBall.Player
{
    public interface IClickTipObject
    {
        public double BeginTime { get; }
        public double StandardClickTime { get; }
        public double EndTime { get; }
        public void Activate(double beginTime, double standardClickTime, double endTime);
        public void OnClicked(double currentTime);
        public bool UpdateState(double currentTime);
        public void Deactivate();
    }

    /// <summary>
    /// 通用动画事件数据（烘焙后供播放读取）
    /// </summary>
    public interface IAnimationEventData
    {
        int RoadIndex { get; }
        int DataIndex { get; }

        /// <summary>
        /// 动画开始时间
        /// </summary>
        double BeginTime { get; }

        /// <summary>
        /// 当前块点击时间（若无点击事件，可与 BeginTime 相同或由实现方定义）
        /// </summary>
        double ClickTime { get; }

        /// <summary>
        /// 动画结束时间
        /// </summary>
        double EndTime { get; }

        /// <summary>
        /// 动画进入可播放状态
        /// </summary>
        void OnBegin(double currentTime);

        /// <summary>
        /// 当前块被点击
        /// </summary>
        void OnClicked(double currentTime);

        /// <summary>
        /// 动画结束
        /// </summary>
        void OnEnd(double currentTime);

        /// <summary>
        /// 动画更新（非脚本动画由播放器驱动）
        /// </summary>
        void OnUpdate(double currentTime);

        /// <summary>
        /// 动画是否仍在活动
        /// </summary>
        bool IsActive { get; }
    }
}