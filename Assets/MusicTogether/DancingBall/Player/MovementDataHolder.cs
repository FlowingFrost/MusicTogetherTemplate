using JetBrains.Annotations;

namespace MusicTogether.DancingBall.Player
{
    public class MovementDataHolder
    {
        public double TimeStamp => Data.Time;
        public double EndTime;
        public bool NeedTap => Data.NeedTap;
        
        public MovementData Data;
        [CanBeNull] public IClickTipObject ClickTipObject;

        public MovementDataHolder(MovementData data, [CanBeNull] IClickTipObject clickTipObject)
        {
            Data = data;
            ClickTipObject = clickTipObject;
        }
        
        public void Initialize()
        {
            ClickTipObject?.Activate(Data.Time, TimeStamp, EndTime);
        }
        
        public void Update(double currentTime)
        {
            ClickTipObject?.UpdateState(currentTime);
        }

        public void DeActivate()
        {
            ClickTipObject?.Deactivate();
        }
    }
}