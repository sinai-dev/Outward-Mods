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
    public class StringSetting : BBSetting
    {
        [XmlIgnore]
        public override object DefaultValue { get { return m_defaultValue; } set { m_defaultValue = (string)value; } }
        [XmlIgnore]
        private string m_defaultValue = "";

        public string m_value;

        [XmlIgnore]
        private InputField m_text;

        public override object GetValue()
        {
            return m_value ?? DefaultValue;
        }

        public override void SetValue(object value)
        {
            m_value = value.ToString();

            if (m_text != null)
            {
                m_text.text = m_value;
            }
        }

        public override void UpdateValue(bool noSave = false)
        {
            if (this.LinkedGameObject)
            {
                if (m_text == null)
                {
                    m_text = LinkedGameObject.GetComponentInChildren<InputField>();
                }

                if (!noSave && !string.IsNullOrEmpty(m_text.text) && m_text.text != m_value)
                {
                    m_value = m_text.text;
                }
            }
        }
    }
}
