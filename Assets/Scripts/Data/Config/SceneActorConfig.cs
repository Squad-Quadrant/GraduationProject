using System;
using UnityEngine;

namespace Data.Config
{
    [CreateAssetMenu(fileName = "SceneActorConfig", menuName = "Game/SceneActorConfig")]
    public class SceneActorConfig : ScriptableObject
    {
        public int ID;
        public SceneActorType SceneActorType;
        public SceneActorDirection Direction;
        public bool IsWalkable;
    }

    public enum SceneActorType
    {
        Table,
        Obstacle,
    }
    
    public enum SceneActorDirection
    {
        None,
        North,
        East,
        South,
        West
    }
}