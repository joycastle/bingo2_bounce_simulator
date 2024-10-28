using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.GettingStarted;
using UnityEngine;

namespace DefaultNamespace
{
    public class Replayer : MonoBehaviour
    {
        private PathData _data;
        private float _totalTime;
#if BOUNCE_DEBUG
        private List<float> simulationTime = new ();
        private List<float> diffTime = new ();
        private List<int> pointType = new ();
        private List<string> pointDesc = new ();
#endif
        PositionTranslate _positionTranslate;
        private void Start()
        {
            _positionTranslate = gameObject.GetComponent<PositionTranslate>();
        }

        public void Init(PathData data)
        {
            _data = data;
            SetPosition(_data.PathPoints[0].GetPos());
            StartCoroutine(MoveLoop());
            // GenAllPositionEvaluate();
        }
        
        void SetPosition(Vector2 recordPos)
        {
            if (_positionTranslate != null)
            {
                transform.position = _positionTranslate.GetModifiedPos(recordPos);
            }
            else
            {
                transform.position = recordPos;
            }
        }

        private void Update()
        {
            _totalTime += Time.deltaTime;
            // var currentPositionEvaluate = _dictionary.LastOrDefault(x => x.Key <= _totalTime).Value;
            // if (currentPositionEvaluate == null)
            // {
            //     Debug.LogError($"No PositionEvaluate Found for {_totalTime}");
            //     return;
            // }
            // else
            // {
            //     SetPosition(currentPositionEvaluate.GetPosition(_totalTime));
            // }
        }
        
        IEnumerator MoveLoop()
        {
            for (int i = 0; i < _data.PathPoints.Count - 1; i++)
            {
                var currentData = _data.PathPoints[i];
                var nextData = _data.PathPoints[i + 1];
                var positionEvaluate = GetPositionEvaluateBetween(currentData, nextData, i);
                yield return DoMove(positionEvaluate);
            }

            // TODOJOE send finishEvent
#if BOUNCE_DEBUG
            // Debug.LogError($"SimulationTime {string.Join(",", simulationTime)}");
            // Debug.LogError($"DiffTime {string.Join(",", diffTime)}");
            // Debug.LogError($"PointType {string.Join(",", pointType)}");
            Debug.LogError($"[{string.Join(",", simulationTime)}];[{string.Join(",", diffTime)}];[{string.Join(",", pointType)}];[{string.Join(",", pointDesc)}]");
#endif
            Destroy(gameObject);
        }
        
        // SortedDictionary<float, PositionEvaluate> _dictionary = new ();
        // void GenAllPositionEvaluate()
        // {
        //     for (int i = 0; i < _data.PathPoints.Count - 1; i++)
        //     {
        //         var currentData = _data.PathPoints[i];
        //         var nextData = _data.PathPoints[i + 1];
        //         var positionEvaluate = GetPositionEvaluateBetween(currentData, nextData, i);
        //         _dictionary[currentData.GetTime()] = positionEvaluate;
        //     }
        // }

        PositionEvaluate GetPositionEvaluateBetween(HitPointData currentData, HitPointData nextData, int i)
        {
#if BOUNCE_DEBUG
            Debug.Log($"OnHitPoint#{i}-{currentData.Type}-{currentData.ID}, SimulationTime {_totalTime}, ExpectTime {currentData.GetTime()}, Diff {_totalTime - currentData.GetTime()}");
            simulationTime.Add(_totalTime);
            diffTime.Add((float) (_totalTime - currentData.GetTime()));
            pointType.Add((int) currentData.Type);
            pointDesc.Add($"{i}.{currentData.ID}");
#endif
            HandleCollisionEvent(currentData);
            PositionEvaluate positionEvaluate;
            if ((currentData.Type == EHitType.CollisionExit && nextData.Type == EHitType.CollisionEnter))
            {
                positionEvaluate = CalculateParabolaMoveFunction(currentData.GetPos(), nextData.GetPos(),
                    nextData.GetTime() - currentData.GetTime());
            }
            else if((currentData.Type == EHitType.CollisionEnter && nextData.Type == EHitType.CollisionStay) || 
                    (currentData.Type == EHitType.CollisionEnter && nextData.Type == EHitType.CollisionExit) ||
                    (currentData.Type == EHitType.CollisionStay && nextData.Type == EHitType.CollisionStay) ||
                    (currentData.Type == EHitType.CollisionStay && nextData.Type == EHitType.CollisionExit))
            {
                positionEvaluate = CalculateLinearMoveFunction(currentData.GetPos(), nextData.GetPos(),
                    nextData.GetTime() - currentData.GetTime());
            }
            else if ((currentData.Type == EHitType.CollisionStay && nextData.Type == EHitType.CollisionEnter) || //可能物体1的stay和物体2的enter连着
                     (currentData.Type == EHitType.CollisionExit && nextData.Type == EHitType.CollisionStay)) //导致物体2的stay和物体1的exit连着)
            {
                if (currentData.ID != nextData.ID)
                {
                    positionEvaluate = CalculateLinearMoveFunction(currentData.GetPos(), nextData.GetPos(),
                        nextData.GetTime() - currentData.GetTime());
                }
                else
                {
                    Debug.LogError($"CollisionWithSameBody State Order Error {i}: {currentData.ToString()} to {nextData.ToString()}");
                    return null;
                }
            }
            else
            {
                Debug.LogError($"Unsupport Move at Index {i}: {currentData.ToString()} to {nextData.ToString()}");
                return null;
            }

            return positionEvaluate;
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
                SetPosition(positionEvaluate.GetPosition(timeElapsed));
                yield return null;
                timeElapsed += Time.deltaTime;
            }
        }

        ParabolaEvaluate CalculateParabolaMoveFunction(Vector2 startPos, Vector2 endPos, float time)
        {
            var v_y0 = (endPos.y - startPos.y) / time + 0.5f * Mathf.Abs(Physics.gravity.y * time);
            var v_x0 = (endPos.x - startPos.x) / time;
            return new ParabolaEvaluate(startPos, endPos, new Vector2(v_x0, v_y0), time);
        }
        
        LinearEvaluate CalculateLinearMoveFunction(Vector2 startPos, Vector2 endPos, float time)
        {
            var theta = 0f;
            if (startPos.x == endPos.x)
            {
                theta = Mathf.PI / 2;
            }
            else
            {
                theta = Mathf.Atan((endPos.y - startPos.y) / (endPos.x - startPos.x));
            }

            var a = Physics.gravity.y * Mathf.Sin(theta);
            var distance = Vector2.Distance(startPos, endPos);
            var v0 = distance / time - 0.5f * a * time;
            var v_x0 = endPos.x > startPos.x ? Mathf.Abs(v0 * Mathf.Cos(theta)) : -Mathf.Abs(v0 * Mathf.Cos(theta));
            var v_y0 = endPos.y > startPos.y ? Mathf.Abs(v0 * Mathf.Sin(theta)) : -Mathf.Abs(v0 * Mathf.Sin(theta));
            return new LinearEvaluate(startPos, endPos, new Vector2(v_x0, v_y0), time);
        }

        public class PositionEvaluate
        {
            protected readonly Vector2 _startPosition;
            protected readonly Vector2 _endPosition;
            protected readonly Vector2 _startVelocity;
            protected readonly float _duration;
            protected Vector2 _simulatedError;
            
            public PositionEvaluate(Vector2 startPos, Vector2 endPos, Vector2 startVelocity, float duration)
            {
                _startPosition = startPos;
                _endPosition = endPos;
                _startVelocity = startVelocity;
                _duration = duration;
                var simulatedEndPosition = GetSimulatedPosition(_duration);
                _simulatedError = simulatedEndPosition - _endPosition;
                Debug.Log($"{GetType().Name} SimulatedError is [{_simulatedError.x},{_simulatedError.y}], Ratio {Vector2.Distance(simulatedEndPosition, startPos) / Vector2.Distance(endPos, startPos) - 1}");
            }
            
            public Vector2 GetPosition(float timeElapsed)
            {
                if (timeElapsed > _duration)
                {
                    return _endPosition;
                }
                
                var simulatedPosition = GetSimulatedPosition(timeElapsed);
                var fixErrorValue = GetFixErrorValue(timeElapsed, simulatedPosition);
                return simulatedPosition - fixErrorValue;
            }

            protected virtual Vector2 GetSimulatedPosition(float timeElapsed)
            {
                throw new NotImplementedException();
            }
            
            protected virtual Vector2 GetFixErrorValue(float timeElapsed, Vector2 simulatedPosition)
            {
                throw new NotImplementedException();
            }
            
            public float GetTime()
            {
                return _duration;
            }
        }
        
        public class ParabolaEvaluate : PositionEvaluate
        {
            public ParabolaEvaluate(Vector2 startPos, Vector2 endPos, Vector2 startVelocity, float duration) : base(startPos, endPos, startVelocity, duration)
            {
            }

            protected override Vector2 GetSimulatedPosition(float timeElapsed)
            {
                var yOffset = _startVelocity.y * timeElapsed + 0.5f * Physics.gravity.y * timeElapsed * timeElapsed;
                var xOffset = _startVelocity.x * timeElapsed;
                return new Vector2(_startPosition.x + xOffset, _startPosition.y + yOffset);
            }

            protected override Vector2 GetFixErrorValue(float timeElapsed, Vector2 simulatedPosition)
            {
                return Vector2.zero;
            }
        }
        
        public class LinearEvaluate : PositionEvaluate
        {
            public LinearEvaluate(Vector2 startPos, Vector2 endPos, Vector2 startVelocity, float duration) : base(startPos, endPos, startVelocity, duration)
            {
                
            }

            protected override Vector2 GetSimulatedPosition(float timeElapsed)
            {
                var theta = Mathf.Atan(_startVelocity.y / _startVelocity.x);
                var a = Physics.gravity.y * Mathf.Sin(theta);
                var a_x = a * Mathf.Cos(theta);
                var a_y = a * Mathf.Sin(theta);
                var xOffset = _startVelocity.x * timeElapsed + 0.5f * a_x * timeElapsed * timeElapsed;
                var yOffset = _startVelocity.y * timeElapsed + 0.5f * a_y * timeElapsed * timeElapsed;
                return new Vector2(_startPosition.x + xOffset, _startPosition.y + yOffset);
            }

            protected override Vector2 GetFixErrorValue(float timeElapsed, Vector2 simulatedPosition)
            {
                var totalDistance = Vector2.Distance(_startPosition, _endPosition + _simulatedError);
                var currentDistance = Vector2.Distance(simulatedPosition, _startPosition);
                var fixErrorValue = _simulatedError * (currentDistance / totalDistance);
                return fixErrorValue;
            }
        }
    }
}