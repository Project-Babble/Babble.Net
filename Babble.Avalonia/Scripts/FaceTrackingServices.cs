using Babble.Core;
using Microsoft.Extensions.Logging;
using System.Reflection;
using VRCFaceTracking.Core.Contracts.Services;

namespace Hypernex.ExtendedTracking;

// From https://github.com/TigersUniverse/Hypernex.Unity/blob/main/Assets/Scripts/ExtendedTracking/FaceTrackingServices.cs
public static class FaceTrackingServices
{
    public class FTLogger : ILogger
    {
        private string p;

        public FTLogger(string c) => p = $"[{c}] ";
        
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Task.Run(new Action(() =>
            {
                switch (logLevel)
                {
                    case LogLevel.Information:
                        BabbleCore.Instance.Logger.LogInformation(p + state);
                        break;
                    case LogLevel.Warning:
                        BabbleCore.Instance.Logger.LogWarning(p + state);
                        break;
                    case LogLevel.Error:
                        BabbleCore.Instance.Logger.LogError(p + state);
                        break;
                    case LogLevel.Critical:
                        BabbleCore.Instance.Logger.LogCritical(eventId, exception.Message);
                        break;
                    default:
                        BabbleCore.Instance.Logger.LogDebug(p + state);
                        break;
                }
            }));
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => new _();
    }
    
    private class _ : IDisposable{public void Dispose(){}}
    
    public class FTLoggerFactory: ILoggerFactory
    {
        public void Dispose(){}

        public ILogger CreateLogger(string categoryName) => new FTLogger(categoryName);

        public void AddProvider(ILoggerProvider provider){}
    }

    public class FTDispatcher : IDispatcherService
    {
        public void Run(Action action) => Task.Run(action);
    }

    public class FTSettings : ILocalSettingsService
    {
        public Task<T> ReadSettingAsync<T>(string key)
        {
            return default;
        }

        public Task SaveSettingAsync<T>(string key, T value)
        {
            return default;
        }

        public Task<T> ReadSettingAsync<T>(string key, T defaultValue = default(T), bool forceLocal = false)
        {
            return default;
        }

        public Task SaveSettingAsync<T>(string key, T value, bool forceLocal = false) =>
            SaveSettingAsync(key, value);

        // Why do I have to do this? Why not just Serialize the object??
        private Dictionary<MemberInfo, SavedSettingAttribute> GetSavedSettings(object target)
        {
            //Dictionary<MemberInfo, SavedSettingAttribute> members = new();
            //Type targetType = target.GetType();
            //foreach (FieldInfo fieldInfo in targetType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic |
            //                                                     BindingFlags.Public))
            //{
            //    SavedSettingAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(SavedSettingAttribute))
            //        .Select(x => (SavedSettingAttribute) x).ToArray();
            //    if(attributes.Length <= 0) continue;
            //    members.Add(fieldInfo, attributes[0]);
            //}
            //foreach (PropertyInfo propertyInfo in targetType.GetProperties(BindingFlags.Instance |
            //                                                               BindingFlags.NonPublic |
            //                                                               BindingFlags.Public))
            //{
            //    SavedSettingAttribute[] attributes = propertyInfo.GetCustomAttributes(typeof(SavedSettingAttribute))
            //        .Select(x => (SavedSettingAttribute) x).ToArray();
            //    if(attributes.Length <= 0) continue;
            //    members.Add(propertyInfo, attributes[0]);
            //}
            //return members;
            return new Dictionary <MemberInfo, SavedSettingAttribute>();
        }

        public Task Save(object target)
        {
            //Dictionary<string, object> values = new();
            //foreach (KeyValuePair<MemberInfo,SavedSettingAttribute> savedSetting in GetSavedSettings(target))
            //{
            //    object value;
            //    value = savedSetting.Key is FieldInfo
            //        ? ((FieldInfo) savedSetting.Key).GetValue(target)
            //        : ((PropertyInfo) savedSetting.Key).GetValue(target);
            //    value ??= savedSetting.Value.Default();
            //    if(value == null) continue;
            //    values.Add(savedSetting.Value.GetName(), value);
            //}
            //return SaveSettingAsync(target.GetType().FullName!.Replace(".", ""), values);
            return Task.CompletedTask;
        }

        public Task Load(object target)
        {
            //Dictionary<string, object> values =
            //    ReadSettingAsync<Dictionary<string, object>>(target.GetType().FullName!.Replace(".", "")).Result;
            //if (values == null) return Task.CompletedTask;
            //foreach (KeyValuePair<MemberInfo,SavedSettingAttribute> savedSetting in GetSavedSettings(target))
            //{
            //    if (savedSetting.Key is FieldInfo)
            //    {
            //        FieldInfo fieldInfo = (FieldInfo) savedSetting.Key;
            //        object value;
            //        if (!values.TryGetValue(savedSetting.Value.GetName(), out value))
            //            value = savedSetting.Value.Default();
            //        fieldInfo.SetValue(target, Convert.ChangeType(value, fieldInfo.FieldType));
            //    }
            //    else if (savedSetting.Key is PropertyInfo)
            //    {
            //        PropertyInfo propertyInfo = (PropertyInfo) savedSetting.Key;
            //        object value;
            //        if (!values.TryGetValue(savedSetting.Value.GetName(), out value))
            //            value = savedSetting.Value.Default();
            //        propertyInfo.SetValue(target, Convert.ChangeType(value, propertyInfo.PropertyType));
            //    }
            //}
            return Task.CompletedTask;
        }
    }

    public class BabbleIdentity : IIdentityService
    {
        // TODO: Make more unique
        public string GetUniqueUserId() => "Babble App";
    }
}