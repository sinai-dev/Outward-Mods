using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Xml.Serialization;

namespace SharedModConfig
{
    [Serializable]
    public class BoolSetting : BBSetting
    {
        [XmlIgnore]
        public override object DefaultValue { get { return m_defaultValue; } set { m_defaultValue = (bool)value; } }
        [XmlIgnore]
        private bool m_defaultValue = false;

        public bool m_value;

        [XmlIgnore]
        private Toggle m_toggleButton;

        public override object GetValue()
        {
            return m_value;
        }

        public override void SetValue(object value)
        {
            m_value = (bool)value;

            if (m_toggleButton != null)
            {
                m_toggleButton.isOn = m_value;
            }
        }

        public override void UpdateValue(bool noSave = false)
        {
            if (this.LinkedGameObject)
            {
                if (!m_toggleButton)
                {
                    m_toggleButton = LinkedGameObject.GetComponentInChildren<Toggle>();
                }

                if (!noSave && m_value != m_toggleButton.isOn)
                {
                    m_value = m_toggleButton.isOn;
                }
            }
        }
    }
}
