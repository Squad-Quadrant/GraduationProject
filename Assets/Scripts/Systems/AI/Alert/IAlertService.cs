namespace Systems.AI.Alert
{
	public interface IAlertService
	{
		EAlertLevel GetAlertLevel(string unitId);
	}
}
