namespace PurpleFlowerCore
{
    public class HotReload
    {
        [UnityEditor.MenuItem("PFC/重载配置数据",false,1)]
        public static void Reload()
        {
            ConfigSystem.LoadAll();
        }
    }
}