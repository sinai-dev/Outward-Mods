using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMapDetails
{
    // Custom class for enemy markers that you can hover over to reveal the name.
    public class EnemyMarker : MapWorldMarker
    {
        public Character LinkedCharacter
        {
            set => m_character = value;
            get
            {
                if (!m_character)
                    m_character = this.GetComponentInParent<Character>();
                return m_character;
            }
        }
        private Character m_character;

        public new bool Anchored = true;
        public new string Text = "";
        public new bool ShowCircle = false;
        public new bool ShowBackground = false;
        public new bool AlignLeft = false;
        private Vector2 m_adjustedMapPosition;

        public new Vector2 MapPosition
        {
            get;
            set;
        }

        public new float MarkerWidth
        {
            get;
            set;
        }

        public new Vector2 AdjustedMapPosition => this.m_adjustedMapPosition;

        internal void Start()
        {
            this.MarkerWidth = (float)(this.Text.Length * 15);
        }

        internal void OnEnable()
        {
            MoreMapDetails.EnemyMarkers.Add(this);

            LinkedCharacter = this.GetComponentInParent<Character>();
        }

        internal void OnDisable()
        {
            if (MoreMapDetails.EnemyMarkers.Contains(this))
                MoreMapDetails.EnemyMarkers.Remove(this);
        }

        public new void CalculateMapPosition(MapDependingScene _sceneSettings, int _id, float _zoom)
        {
            Vector2 vector = base.transform.position.xz();

            vector.x = vector.x * _sceneSettings.MarkerScale.x + _sceneSettings.MarkerOffset.x;
            vector.y = vector.y * _sceneSettings.MarkerScale.y + _sceneSettings.MarkerOffset.y;
            vector *= _zoom;

            if (_sceneSettings.Rotation != 0f)
                vector = (Quaternion.Euler(0f, _sceneSettings.Rotation, 0f) * new Vector3(vector.x, 0f, vector.y)).xz();

            this.MapPosition = vector;
        }

        public new void AdjustMapPosition(float _markerHeight = 0f, List<MapWorldMarker> _markers = null)
        {
            this.m_adjustedMapPosition = this.MapPosition;
        }

        public new static int Sort(MapWorldMarker _m1, MapWorldMarker _m2)
        {
            if (_m1.Anchored)
                return -1;

            if (_m2.Anchored)
                return 1;

            if (_m1.MapPosition.y > _m2.MapPosition.y)
                return -1;

            return 1;
        }

        //private bool CheckMarkerPosOverlap(Vector2 _data1, Vector2 _data2)
        //{
        //    return _data2.x.InRange(_data1.x, _data1.y, false) || _data2.y.InRange(_data1.x, _data1.y, false) || _data1.x.InRange(_data2.x, _data2.y, false) || _data1.x.InRange(_data2.x, _data2.y, false);
        //}
    }

}