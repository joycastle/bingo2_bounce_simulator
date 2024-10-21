using System.Collections;
using UnityEngine;

namespace DefaultNamespace
{
    public class Replayer : MonoBehaviour
    {
        private PathData _data;
        public void Init(PathData data)
        {
            _data = data;
            transform.position = _data.PathPoints[0].Pos;
            StartCoroutine(MoveLoop());
        }

        IEnumerator MoveLoop()
        {
            for (int i = 0; i < _data.PathPoints.Count - 1; i++)
            {
                var positionEvaluate = CalculateMoveFunction(_data.PathPoints[i].Pos, _data.PathPoints[i + 1].Pos,
                    _data.PathPoints[i + 1].Time - _data.PathPoints[i].Time);
                yield return DoMove(positionEvaluate);
            }
        }

        IEnumerator DoMove(PositionEvaluate positionEvaluate)
        {
            var timeElapsed = 0f;
            while (timeElapsed <positionEvaluate.GetTime())
            {
                transform.position = positionEvaluate.GetPosition(timeElapsed);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        PositionEvaluate CalculateMoveFunction(Vector3 startPos, Vector3 endPos, float time)
        {
            var v_y0 = (endPos.y - startPos.y) / time + 0.5f * Mathf.Abs(Physics.gravity.y * time);
            var v_x0 = (endPos.x - startPos.x) / time;
            return new PositionEvaluate(startPos, endPos, new Vector3(v_x0, v_y0, 0), time);
        }

        public class PositionEvaluate
        {
            private readonly Vector3 _startPosition;
            private readonly Vector3 _endPosition;
            private readonly Vector3 _startVelocity;
            private readonly float _duration;
            
            public PositionEvaluate(Vector3 startPos, Vector3 endPos, Vector3 startVelocity, float duration)
            {
                _startPosition = startPos;
                _endPosition = endPos;
                _startVelocity = startVelocity;
                _duration = duration;
            }

            public Vector3 GetPosition(float timeElapsed)
            {
                if (timeElapsed > _duration)
                {
                    return _endPosition;
                }
                
                var yOffset = _startVelocity.y * timeElapsed + 0.5f * Physics.gravity.y * timeElapsed * timeElapsed;
                var xOffset = _startVelocity.x * timeElapsed;
                return new Vector3(_startPosition.x + xOffset, _startPosition.y + yOffset, _startPosition.z);
            }
            
            public float GetTime()
            {
                return _duration;
            }
        }
    }
}