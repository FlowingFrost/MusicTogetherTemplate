using LiteGameFrame.CoreInfrastructure;
using LiteGameFrame.NGPStateMachine;
using UnityEngine;
using UnityEngine.Playables;

namespace MusicTogether.LevelManagement
{
    public class SimpleLevelManager : Singleton<SimpleLevelManager>, ILevelManager, IStateHandler
    {
        public double LevelProgress => levelDirector.time;
        private double levelProgress;
        public PlayableDirector levelDirector;
        public void OnStateEnter(StateContext context)
        {
            levelDirector.Stop();
            levelDirector.Play();
        }

        public void OnStateUpdate(StateContext context)
        {
            levelProgress = levelDirector.time;
        }

        public void OnStateExit(StateContext context)
        {
            levelDirector.Stop();
        }
    }
}