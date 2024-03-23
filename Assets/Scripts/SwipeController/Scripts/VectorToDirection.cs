using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GG.Infrastructure.Utils.Swipe
{
    public class VectorToDirection
    {
        private List<DirectionId> _directions;

        public VectorToDirection(List<DirectionId> swipeParameters)
        {
            _directions = new List<DirectionId>(swipeParameters);
        }

        public string GetSwipeId(Vector3 direction)
        {
            float minAngle = float.MaxValue;
            int index = -1;

            for (int i = 0; i < _directions.Count; ++i)
            {
                float angle = Vector3.Angle(direction, _directions[i].Direction);
                if (angle < minAngle)
                {
                    minAngle = angle;
                    index = i;
                }
            }

            return _directions[index].Id;
        }
    }
}
