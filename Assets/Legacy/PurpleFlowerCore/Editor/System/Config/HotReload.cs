namespace PurpleFlowerCore
{
    public class HotReload
    {
        [UnityEditor.MenuItem("PFC/配置数据热重载",false,1)]
        public static void Reload()
        {
            ConfigSystem.LoadAll();
        }
    }
}