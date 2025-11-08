namespace PurpleFlowerCore.Config
{
    public abstract class ConfigData
    {
        protected ConfigData()
        {
            ConfigSystem.RegisterConfig(this);
        }
        
        public virtual void Load()
        {
            
        }
    }
}