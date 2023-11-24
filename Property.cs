using System;
using SimHub.Plugins;

namespace PluginDeMo_v2
{
    public class Property<T>
    {
        //// plugin manager ////
        public PluginManager PluginManager { get; set; }

        //// plugin manger attributes ////
        public string Name { get; set; }
        public Type PluginType { get; set; }
        public Func<T> ValueFunc { get; set; }
        public T Value => ValueFunc.Invoke();

        //// update attributes ////
        public int UpdateRate { get; set; } = 0; // in milliseconds
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public bool UpdateRequired => (DateTime.Now - LastUpdated).TotalMilliseconds > UpdateRate;

        public Property(
            PluginManager pluginManager,
            string name,
            Func<T> valueFunc,
            int updateRate = 0
        )
        {
            PluginManager = pluginManager;
            Name = name;
            PluginType = valueFunc.Invoke().GetType();
            ValueFunc = valueFunc;
            UpdateRate = updateRate;
            pluginManager.AddProperty(Name, PluginType, Value);
        }

        public void Update(bool isPlayer = false)
        {
            if (!UpdateRequired)
                return;

            if (Value.GetType() != PluginType)
                throw new Exception(
                    $"Property {Name} is of type {PluginType} but value is of type {Value.GetType()}"
                );

            LastUpdated = DateTime.Now;
            PluginManager.SetPropertyValue(Name, PluginType, ValueFunc.Invoke());
        }
    }
}
