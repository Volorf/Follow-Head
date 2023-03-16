using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.Events;
using Vector3 = UnityEngine.Vector3;

namespace Volorf.FollowHead
{
    public class FollowHead : MonoBehaviour
    {
        [Header("Warn up")]
        [SerializeField] private float warnupDuration = 0.1f;
        [SerializeField] private bool UpdateTransformAfterWarnUp = true;
        [SerializeField] private float warnupShowAnimDur = 1f;
        [SerializeField] private AnimationCurve warnupShowAnimCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        private bool _hasWarnupBeenFinished = false;
        [SerializeField] private UnityEvent onAfterWarnUp;

        [Space(16)]
        [Header("Positioning")]
        [SerializeField] private bool keepConstantDistance = false;
        public float DistanceFromCamera = 1f;
        public float DownOffset = 0f;
        [SerializeField] private bool isMirrored = false;

        [Space(16)] [Header("Animation")] 
        [SerializeField] private bool IsFollowingHead;
        public bool DoSmooth = true;
        [SerializeField] private float followHead = 0.5f;
        [SerializeField] private float lookAtHead = 0.5f;
        
        [Space(16)]
        [Header("Constraints")]
        [SerializeField] private bool freezeXAxisForLookingAtHead = false;
        [SerializeField] private bool stopUpdatingPosition = false;
        [SerializeField] private bool lockYPositionUpdate = true;
        [SerializeField] private bool stopUpdatingDirection = false;
        
        private bool _canFollow = true;
        private Vector3 _smoothPositionVelocity;
        private Vector3 _smoothForwardVelocity;
        private Transform _camera;

        public void StopFollowingHead() => _canFollow = false;
        public void ResumeFollowingHead() => _canFollow = true;

        private Vector3 _initialPos;
        private Vector3 _initialScale;

        public void StopFollowing()
        {
            _canFollow = false;
        }

        public void ResumeFollowing()
        {
            _canFollow = true;
        }
        
        public void SetDownOffset(float o)
        {
            DownOffset = o;
        }

        public void SetDistanceFromCamera(float d)
        {
            DistanceFromCamera = d;
        }

        private void Start()
        {
            _initialScale = transform.localScale;
            
            if (Camera.main != null)
            {
                _camera = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning("Haven't found the Main Camera.\nCheck if there is a camera in your scene and it has the 'MainCamera' tag.");
            }

            StartCoroutine(Warnup(warnupDuration, warnupShowAnimDur));
        }

        private Vector3 CalculateSnackBarPosition()
        {
            Vector3 forwardVec = _camera.forward;
            
            if (keepConstantDistance)
            {
                forwardVec = new Vector3(forwardVec.x, 0f, forwardVec.z).normalized;
                // print("forwardVec: " + forwardVec);
                lockYPositionUpdate = false;
                // Debug.DrawLine(_camera.position, _camera.position + forwardVec, Color.red, 1f);
            }

            return _camera.position + forwardVec * DistanceFromCamera + Vector3.up * (-1f * DownOffset);
        }

        private void Update()
        {
            if (!_canFollow || !IsFollowingHead) return;
            if (!_hasWarnupBeenFinished) return;
            
            ProcessPositionAndRotation(DoSmooth);

        }

        private void ProcessPositionAndRotation(bool doSmooth)
        {
            if (!stopUpdatingPosition)
            {
                Vector3 newPos = CalculateSnackBarPosition();
                if (lockYPositionUpdate) newPos.y = _initialPos.y;
                
                transform.position = doSmooth ? Vector3.SmoothDamp(transform.position, newPos, ref _smoothPositionVelocity, followHead) : newPos;
            }

            if (!stopUpdatingDirection)
            {
                Vector3 targetPosition = _camera.position;
                if (freezeXAxisForLookingAtHead) targetPosition.y = transform.position.y;
                
                Vector3 newForward = (targetPosition - transform.position).normalized * (isMirrored ? 1f : -1f);
                transform.forward = doSmooth ? Vector3.SmoothDamp(transform.forward, newForward, ref _smoothForwardVelocity, lookAtHead) : newForward;
            }
        }

        IEnumerator Warnup(float delay, float animDur)
        {
            transform.localScale = Vector3.zero;
            yield return new WaitForSeconds(delay);
            _initialPos = CalculateSnackBarPosition();
            _hasWarnupBeenFinished = true;
            
            if (UpdateTransformAfterWarnUp)
            {
                ProcessPositionAndRotation(false);
            }
            
            transform.position = _initialPos;

            float timer = 0f;

            while (timer <= animDur)
            {
                timer += Time.deltaTime;
                float koef = timer / animDur;
                float curvedKoef = warnupShowAnimCurve.Evaluate(koef);
                Vector3 newScale = Vector3.Lerp(Vector3.zero, _initialScale, curvedKoef);
                transform.localScale = newScale;
                yield return null;
            }
            
            onAfterWarnUp.Invoke();
        }
    }
}
