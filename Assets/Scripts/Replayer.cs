using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
#endif
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
                var currentHitData = _data.PathPoints[i];
#if UNITY_EDITOR
                Debug.LogError($"OnHitPoint#{i}, SimulationTime {_totalTime}, ExpectTime {currentHitData.Time}, Diff {_totalTime - currentHitData.Time}");
                simulationTime.Add(_totalTime);
                diffTime.Add(_totalTime - currentHitData.Time);
#endif
                HandleCollisionEvent(currentHitData);
                var positionEvaluate = CalculateMoveFunction(currentHitData.Pos, _data.PathPoints[i + 1].Pos,
                    _data.PathPoints[i + 1].Time - currentHitData.Time);
                yield return DoMove(positionEvaluate);
            }

            yield return new WaitForSeconds(0.5f);
#if UNITY_EDITOR
            Debug.LogError($"SimulationTime {string.Join(",", simulationTime)}");
            Debug.LogError($"DiffTime {string.Join(",", diffTime)}");
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

        PositionEvaluate CalculateMoveFunction(Vector3 startPos, Vector3 endPos, float time)
        {
            var v_y0 = (endPos.y - startPos.y) / time + 0.5f * Mathf.Abs(Physics.gravity.y * time);
            var v_x0 = (endPos.x - startPos.x) / time;
            return new PositionEvaluate(startPos, endPos, new Vector3(v_x0, v_y0, 0), time);
        }

        private void Update()
        {
            _totalTime += Time.deltaTime;
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