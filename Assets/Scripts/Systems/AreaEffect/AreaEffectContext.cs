using Core.Events;
using Systems.AI;
using Systems.Buff;
using Systems.Damage;
using Systems.Unit;
using Systems.Vision;

namespace Systems.AreaEffect
{
	// Behavior 回调时可用的服务引用集合
	public class AreaEffectContext
	{
		public IEventBus EventBus { get; }
		public IUnitService UnitService { get; }
		public IDamageService DamageService { get; }
		public IVisionService VisionService { get; }
		public IVisionCalculator VisionCalculator { get; }
		public IBuffService BuffService { get; }
        public IAIService AIService { get; }

		public AreaEffectContext(
			IEventBus eventBus,
			IUnitService unitService,
			IDamageService damageService,
			IVisionService visionService,
			IVisionCalculator visionCalculator,
			IBuffService buffService,
            IAIService aiService)
		{
			EventBus = eventBus;
			UnitService = unitService;
			DamageService = damageService;
			VisionService = visionService;
			VisionCalculator = visionCalculator;
			BuffService = buffService;
            AIService = aiService;
		}
	}
}
