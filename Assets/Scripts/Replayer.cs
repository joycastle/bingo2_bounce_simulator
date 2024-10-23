using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.GettingStarted;
using UnityEngine;

namespace DefaultNamespace
{
    public class Replayer : MonoBehaviour
    {
        private PathData _data;
#if UNITY_EDITOR
        private float _totalTime;
        private List<float> simulationTime = new ();
        private List<float> diffTime = new ();
        private List<int> pointType = new ();
        private List<string> pointDesc = new ();
#endif
        public void Init(PathData data)
        {
            _data = data;
            transform.position = _data.PathPoints[0].Pos;
            StartCoroutine(MoveLoop());
        }
        
        private void Update()
        {
            _totalTime += Time.deltaTime;
        }

        IEnumerator MoveLoop()
        {
            for (int i = 0; i < _data.PathPoints.Count - 1; i++)
            {
                
                var currentData = _data.PathPoints[i];
                var nextData = _data.PathPoints[i + 1];
#if UNITY_EDITOR
                Debug.LogError($"OnHitPoint#{i}, SimulationTime {_totalTime}, ExpectTime {currentData.Time}, Diff {_totalTime - currentData.Time}");
                simulationTime.Add(_totalTime);
                diffTime.Add(_totalTime - currentData.Time);
                pointType.Add((int) currentData.Type);
                pointDesc.Add($"{i}.{currentData.ID}");
#endif
                HandleCollisionEvent(currentData);
                PositionEvaluate positionEvaluate;
                if ((currentData.Type == EHitType.CollisionExit && nextData.Type == EHitType.CollisionEnter))
                {
                    positionEvaluate = CalculateParabolaMoveFunction(currentData.Pos, nextData.Pos,
                        nextData.Time - currentData.Time);
                }
                else if((currentData.Type == EHitType.CollisionEnter && nextData.Type == EHitType.CollisionStay) || 
                        (currentData.Type == EHitType.CollisionStay && nextData.Type == EHitType.CollisionStay) ||
                        (currentData.Type == EHitType.CollisionStay && nextData.Type == EHitType.CollisionExit) ||
                        (currentData.Type == EHitType.CollisionEnter && nextData.Type == EHitType.CollisionExit))
                {
                    positionEvaluate = CalculateLinearMoveFunction(currentData.Pos, nextData.Pos,
                        nextData.Time - currentData.Time);
                }
                else
                {
                    Debug.LogError($"Unsupport move type from {currentData.Type} to {nextData.Type}");
                    yield break;
                }
                
                yield return DoMove(positionEvaluate);
            }

            // TODOJOE send finishEvent
#if UNITY_EDITOR
            // Debug.LogError($"SimulationTime {string.Join(",", simulationTime)}");
            // Debug.LogError($"DiffTime {string.Join(",", diffTime)}");
            // Debug.LogError($"PointType {string.Join(",", pointType)}");
            Debug.LogError($"[{string.Join(",", simulationTime)}];[{string.Join(",", diffTime)}];[{string.Join(",", pointType)}];[{string.Join(",", pointDesc)}]");
#endif
            Destroy(gameObject);
        }

        void HandleCollisionEvent(HitPointData data)
        {
            PathDataManager.FromIdentifier(data.ID, out var frameType, out var id); 
            if(frameType == EFrameType.Bumper && data.Type == EHitType.CollisionEnter)
            {
                //TODOJOE sendEvent to UI
                Debug.Log($"OnHitWith {frameType}#{id}");
            }
        }

        IEnumerator DoMove(PositionEvaluate positionEvaluate)
        {
            var timeElapsed = 0f;
            while (timeElapsed < positionEvaluate.GetTime())
            {
                transform.position = positionEvaluate.GetPosition(timeElapsed);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        ParabolaEvaluate CalculateParabolaMoveFunction(Vector3 startPos, Vector3 endPos, float time)
        {
            var v_y0 = (endPos.y - startPos.y) / time + 0.5f * Mathf.Abs(Physics.gravity.y * time);
            var v_x0 = (endPos.x - startPos.x) / time;
            // return new PositionEvaluate(startPos, endPos, new Vector3(v_x0, v_y0, 0), time);
            return new ParabolaEvaluate(startPos, endPos, new Vector3(v_x0, v_y0, 0), time);
        }
        
        LinearEvaluate CalculateLinearMoveFunction(Vector3 startPos, Vector3 endPos, float time)
        {
            var theta = 0f;
            Vector2 startVelocity = Vector2.zero;
            if (startPos.x == endPos.x)
            {
                theta = Mathf.PI / 2;
            }
            else
            {
                theta = Mathf.Atan((endPos.y - startPos.y) / (endPos.x - startPos.x));
            }
            var assumeVelocity = 0.1f;
            var v_x0 = assumeVelocity * Mathf.Cos(theta);
            var v_y0 = assumeVelocity * Mathf.Sin(theta);
            startVelocity = new Vector2(v_x0, v_y0);
            return new LinearEvaluate(startPos, endPos, startVelocity, time);
        }

        public class PositionEvaluate
        {
            protected readonly Vector3 _startPosition;
            protected readonly Vector3 _endPosition;
            protected readonly Vector3 _startVelocity;
            protected readonly float _duration;
            
            public PositionEvaluate(Vector3 startPos, Vector3 endPos, Vector3 startVelocity, float duration)
            {
                _startPosition = startPos;
                _endPosition = endPos;
                _startVelocity = startVelocity;
                _duration = duration;
            }
            
            public virtual Vector2 GetPosition(float timeElapsed)
            {
                return Vector2.zero;
            }
            
            public float GetTime()
            {
                return _duration;
            }
        }
        
        public class ParabolaEvaluate : PositionEvaluate
        {
            public ParabolaEvaluate(Vector3 startPos, Vector3 endPos, Vector3 startVelocity, float duration) : base(startPos, endPos, startVelocity, duration)
            {
            }

            public override Vector2 GetPosition(float timeElapsed)
            {
                if (timeElapsed > _duration)
                {
                    return _endPosition;
                }
                
                var yOffset = _startVelocity.y * timeElapsed + 0.5f * Physics.gravity.y * timeElapsed * timeElapsed;
                var xOffset = _startVelocity.x * timeElapsed;
                return new Vector2(_startPosition.x + xOffset, _startPosition.y + yOffset);
            }
        }
        
        public class LinearEvaluate : PositionEvaluate
        {
            public LinearEvaluate(Vector3 startPos, Vector3 endPos, Vector3 startVelocity, float duration) : base(startPos, endPos, startVelocity, duration)
            {
            }

            public override Vector2 GetPosition(float timeElapsed)
            {
                var theta = Mathf.Atan(_startVelocity.y / _startVelocity.x);
                var a = Physics.gravity.y * Mathf.Sin(theta);
                var a_x = a * Mathf.Cos(theta);
                var a_y = a * Mathf.Sin(theta);
                var xOffset = _startVelocity.x * timeElapsed + 0.5f * a_x * timeElapsed * timeElapsed;
                var yOffset = _startVelocity.y * timeElapsed + 0.5f * a_y * timeElapsed * timeElapsed;
                return new Vector2(_startPosition.x + xOffset, _startPosition.y + yOffset);
                // return Vector2.Lerp(_startPosition, _endPosition, timeElapsed / GetTime());
            }
        }
    }
}