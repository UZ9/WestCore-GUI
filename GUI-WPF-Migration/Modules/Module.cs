using System.Collections.Generic;
using System.Windows.Controls;
using Charts;

namespace GUI_WPF_Migration.Modules
{
    /// <summary>
    /// Abstract class containing the fundamentals of any module for the GUI
    /// </summary>
    public abstract class Module
    {
        /// <summary>
        /// For modules to access their live data, a dictionary is used to hold all necessary variables
        /// </summary>
        public Dictionary<string, object> VarMap;

        /// <summary>
        /// Represents the container the module will be placed in. Used for adding WPF visual elements
        /// </summary>
        public Border ModuleContainer;

        protected Module(Border moduleContainer)
        {
            ModuleContainer = moduleContainer;

            VarMap = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes the module using a configMap <see cref="Dictionary{TKey, TValue}"/>. 
        /// 
        /// The method is called as soon as the <see cref="ChartManager"/> receives the proper configuration data.
        /// </summary>
        /// <param name="title">The title of the module</param>
        /// <param name="configMap">The config values for the module</param>
        public abstract void Initialize(string title, Dictionary<string, object> configMap);

        /// <summary>
        /// Called every time new data is received from the PROS program
        /// </summary>
        public abstract void Update();

    }
}
