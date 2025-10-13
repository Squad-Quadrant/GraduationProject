using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PurpleFlowerCore.Resource
{
    public class AddressableModule
    {
        private readonly Dictionary<string, IEnumerator> _resDic = new();
        
        public void Load<T>(string name, Action<AsyncOperationHandle<T>> callBack)
        {
            string keyName = GetKeyName(name, typeof(T));
            AsyncOperationHandle<T> handle;
            if (_resDic.ContainsKey(keyName))
            {
                handle = (AsyncOperationHandle<T>)_resDic[keyName];
                if (handle.IsDone)
                {
                    callBack(handle);
                }
                else
                {
                    handle.Completed += operation =>
                    {
                        if (operation.Status == AsyncOperationStatus.Succeeded)
                        {
                            callBack(handle);
                        }
                    };
                }
                return;
            }

            handle = Addressables.LoadAssetAsync<T>(name);
            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    callBack(handle);
                }
                else
                {
                    PFCLog.Warning("Addressable", $"LoadAssetAsync Failed: {name}");
                    if(_resDic.ContainsKey(keyName))
                        _resDic.Remove(keyName);
                }
            };
            _resDic.Add(keyName, handle);
        }

        /// <summary>
        /// 加载多个或指定资源
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="callBack"></param>
        /// <param name="keys"></param>
        /// <typeparam name="T"></typeparam>
        public void Load<T>(Addressables.MergeMode mode, Action<T> callBack, params string[] keys)
        {
            List<string> list = new();
            foreach (var key in keys)
            {
                list.Add(key);
            }
            AsyncOperationHandle<IList<T>> handle;
            string keyName = keys.Aggregate("", (current, key) => current + (key + "_"));
            keyName += typeof(T).Name;
            if (_resDic.ContainsKey(keyName))
            {
                handle = (AsyncOperationHandle<IList<T>>)_resDic[keyName];
                if (handle.IsDone)
                {
                    foreach (var res in handle.Result)
                    {
                        callBack(res);
                    }
                }
                else
                {
                    handle.Completed += operation =>
                    {
                        if (operation.Status == AsyncOperationStatus.Succeeded)
                        {
                            foreach (var res in handle.Result)
                            {
                                callBack(res);
                            }
                        }
                    };
                }
                return;
            }
            
            handle = Addressables.LoadAssetsAsync(list, callBack, mode);
            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Failed)
                {
                    PFCLog.Error("Addressable", $"LoadAssetsAsync Failed: {keyName}");
                    if(_resDic.ContainsKey(keyName))
                        _resDic.Remove(keyName);
                }
            };
            _resDic.Add(keyName, handle);
        }
        
        public void Load<T>(Addressables.MergeMode mode, Action<AsyncOperationHandle<IList<T>>> callBack, params string[] keys)
        {
            List<string> list = new();
            foreach (var key in keys)
            {
                list.Add(key);
            }
            AsyncOperationHandle<IList<T>> handle;
            string keyName = keys.Aggregate("", (current, key) => current + (key + "_"));
            keyName += typeof(T).Name;
            if (_resDic.ContainsKey(keyName))
            {
                handle = (AsyncOperationHandle<IList<T>>)_resDic[keyName];
                if (handle.IsDone)
                {
                    callBack(handle);
                }
                else
                {
                    handle.Completed += operation =>
                    {
                        if (operation.Status == AsyncOperationStatus.Succeeded)
                        {
                            callBack(handle);
                        }
                    };
                }
                return;
            }
            
            handle = Addressables.LoadAssetsAsync<T>(list, obj => {}, mode);
            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Failed)
                {
                    PFCLog.Error("Addressable", $"LoadAssetsAsync Failed: {keyName}");
                    if(_resDic.ContainsKey(keyName))
                        _resDic.Remove(keyName);
                }
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    callBack(handle);
                }
            };
            _resDic.Add(keyName, handle);
        }
        
        public void Release<T>(string name)
        {
            string keyName = GetKeyName(name, typeof(T));
            if (_resDic.ContainsKey(keyName))
            {
                AsyncOperationHandle<T> handle = (AsyncOperationHandle<T>)_resDic[keyName];
                Addressables.Release(handle);
                _resDic.Remove(keyName);
            }
        }

        public void Release<T>(params string[] keys)
        {
            string keyName = keys.Aggregate("", (current, key) => current + (key + "_"));
            keyName += typeof(T).Name;
            if (_resDic.ContainsKey(keyName))
            {
                AsyncOperationHandle<IList<T>> handle = (AsyncOperationHandle<IList<T>>)_resDic[keyName];
                Addressables.Release(handle);
                _resDic.Remove(keyName);
            }
        }

        // public void Clear()
        // {
        //     _resDic.Clear();
        //     AssetBundle.UnloadAllAssetBundles(true);
        //     Resources.UnloadUnusedAssets();
        //     GC.Collect();
        // }
        
        private string GetKeyName(string name, Type type)
        {
            return name + "_" + type.Name;
        }
    }
}