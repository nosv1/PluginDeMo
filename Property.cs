using System;
using SimHub.Plugins;
using SimHub.Plugins.DataPlugins.RGBDriver.LedsContainers.Groups;

namespace PluginDeMo_v2
{
    public class Property<T>
    {
        //// plugin manager ////
        public PluginManager PluginManager { get; set; }

        //// plugin manger attributes ////
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public string Name => $"{Prefix}{Suffix}";
        public Type PluginType { get; set; }
        public Func<T> ValueFunc { get; set; }
        public T Value => ValueFunc.Invoke();

        //// update attributes ////
        public int UpdateRate { get; set; } = 0; // in milliseconds
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public bool UpdateNeeded => (DateTime.Now - LastUpdated).TotalMilliseconds > UpdateRate;

        public Property(
            PluginManager pluginManager,
            string prefix,
            string suffix,
            Type pluginType,
            Func<T> valueFunc,
            int updateRate = 0
        )
        {
            PluginManager = pluginManager;
            Prefix = prefix;
            Suffix = suffix;
            PluginType = pluginType;
            ValueFunc = valueFunc;
            UpdateRate = updateRate;
            pluginManager.AddProperty(Name, PluginType, Value);
        }

        public void Update()
        {
            if (!UpdateNeeded)
                return;

            // if (Value.GetType() != PluginType)
            //     throw new Exception(
            //         $"Property {Name} is of type {PluginType} but value is of type {Value.GetType()}"
            //     );

            LastUpdated = DateTime.Now;
            PluginManager.SetPropertyValue(Name, PluginType, Value);
        }
    }
}
