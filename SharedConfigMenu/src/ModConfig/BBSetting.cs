using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml.Serialization;

namespace SharedModConfig
{
    public abstract class BBSetting
    {
        public string Name { get; set; }

        [XmlIgnore]
        public string Description { get; set; }
        [XmlIgnore]
        public abstract object DefaultValue { get; set; }
        [XmlIgnore]
        public string SectionTitle { get; set; }

        [XmlIgnore]
        internal GameObject LinkedGameObject;

        public abstract object GetValue();
        public abstract void SetValue(object value);
        public abstract void UpdateValue(bool noSave = false);
    }
}
