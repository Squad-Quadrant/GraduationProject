using DG.Tweening;

namespace Systems.Time
{
	public interface ITimeService
	{
		void SlowMotion(float scale, float durationRealTime, Ease recoverEase = Ease.OutCubic);

		void SetTimeScale(float scale);

		void ResetTimeScale();
	}
}
