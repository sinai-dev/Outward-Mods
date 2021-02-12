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
    public class FloatSetting : BBSetting
    {
        [XmlIgnore]
        public override object DefaultValue { get { return m_defaultValue; } set { m_defaultValue = (float)value; } }
        [XmlIgnore]
        private float m_defaultValue = 0f;

        [XmlIgnore]
        public bool ShowPercent = false;
        [XmlIgnore]
        public int RoundTo = 0;
        [XmlIgnore]
        public float MinValue = 0;
        [XmlIgnore]
        public float MaxValue = 100;
        [XmlIgnore]
        public float Increment = -1;

        public float m_value;

        [XmlIgnore]
        private Text m_text;
        [XmlIgnore]
        private Slider m_slider;

        public override object GetValue()
        {
            return m_value;
        }

        public override void SetValue(object value)
        {
            m_value = Mathf.Clamp((float)value, MinValue, MaxValue);

            if (RoundTo >= 0)
            {
                m_value = (float)Math.Round(m_value, RoundTo);
            }

            if (Increment > 0)
            {
                m_value = Increment * Mathf.Floor(m_value / Increment);
            }

            if (m_text != null && m_slider != null)
            {
                string s = m_value + (ShowPercent ? "%" : "");
                m_text.text = s;
                m_slider.value = m_value;
            }
        }

        public override void UpdateValue(bool noSave = false)
        {
            if (this.LinkedGameObject)
            {
                if (!m_text || !m_slider)
                {
                    m_text = LinkedGameObject.transform.Find("HorizontalViewport").Find("Horizontal_Group").Find("SliderValue").GetComponent<Text>();
                    m_slider = LinkedGameObject.GetComponentInChildren<Slider>();
                }

                if (Increment > 0)
                {
                    m_slider.value = Increment * Mathf.Floor(m_slider.value / Increment);
                }

                float formattedValue = RoundTo >= 0 ? (float)Math.Round(m_slider.value, RoundTo) : m_slider.value;
                
                string s = formattedValue + (ShowPercent ? "%" : "");
                m_text.text = s;

                if (!noSave && m_value != formattedValue)
                {
                    m_value = formattedValue;
                }
            }
        }
    }
}
