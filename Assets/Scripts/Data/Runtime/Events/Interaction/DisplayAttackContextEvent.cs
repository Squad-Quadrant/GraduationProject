using System.Collections.Generic;
using Core.Events;
using Systems.Damage;

namespace Data.Runtime.Events.Interaction
{
	public readonly struct DisplayAttackContextEvent : IEvent
	{
		public DamageExecutingContext Context { get; }
		public string OwnerID { get; }
        public Dictionary<BodyPartType, DamageExecutingContext>  ContextDic { get; }

		private DisplayAttackContextEvent(DamageExecutingContext context, string ownerID,  Dictionary<BodyPartType, DamageExecutingContext> contextDic)
		{
			Context = context;
			OwnerID = ownerID;
            ContextDic = contextDic;
		}

		public static DisplayAttackContextEvent Valid(DamageExecutingContext context, string ownerId, Dictionary<BodyPartType, DamageExecutingContext> contextDic)
            => new(context, ownerId, contextDic);

		public static DisplayAttackContextEvent Invalid() => new(null, string.Empty, null);
	}
}
