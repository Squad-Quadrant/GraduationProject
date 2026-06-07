using System.Collections.Generic;
using Core.Events;
using Systems.Map;
using Systems.Map.SceneActor;

namespace Data.Runtime.Events.Map
{
    public struct UpdateGunLineEvent : IEvent
    {
        public Systems.Unit.Unit attacker;
        public Systems.Unit.Unit target;

        public List<WallKey> heightWalls;
        public List<SceneActorBase> sceneActors;

        public UpdateGunLineEvent(Systems.Unit.Unit attacker, Systems.Unit.Unit target, List<WallKey> heightWalls, List<SceneActorBase> sceneActors = null)
        {
            this.attacker = attacker;
            this.target = target;
            this.heightWalls = heightWalls;
            this.sceneActors = sceneActors;
        }
    }
}