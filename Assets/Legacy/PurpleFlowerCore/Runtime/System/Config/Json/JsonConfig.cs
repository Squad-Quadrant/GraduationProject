using System.Collections.Generic;
using System.IO;
using LitJson;

namespace PurpleFlowerCore.Config
{
    public abstract class JsonConfig<T> : ConfigData where T : class
    {
        protected abstract string GetLoadPath();
        private Dictionary<string ,T> _data;
        public T this[string key] => GetItem(key);
        public T this[int key] => GetItem(key);
        
        public T GetItem(string key)
        {
            if (_data == null)
            {
                Load();
            }
            return _data[key];
        }
        
        public T GetItem(int key)
        {
            return GetItem(key.ToString());
        }

        public override void Load()
        {   
            _data = new Dictionary<string, T>();
            var path = GetLoadPath();
            var json = File.ReadAllText(path);
            var data = JsonMapper.ToObject<Dictionary<string ,T>>(json);
            foreach (var kv in data)
            {
                _data.Add(kv.Key, kv.Value);
            }
        }
    }
}
