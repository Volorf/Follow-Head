using UnityEngine;

namespace Volorf.FollowHead
{
    public class FollowHead : MonoBehaviour
    {
        [Space(16)]
        [Header("Positioning")]
        [SerializeField] private float distanceFromCamera = 1f;
        [SerializeField] private float downOffset = 0f;
        [SerializeField] private bool isMirrored = false;
        
        [Space(16)]
        [Header("Smoothness")]
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

        private void Start()
        {
            if (Camera.main != null)
            {
                _camera = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning("Haven't found the Main Camera.\nCheck if there is a camera in your scene and it has the 'MainCamera' tag.");
            }
            
            _initialPos = CalculateSnackBarPosition();
        }
    
        private Vector3 CalculateSnackBarPosition()
        {
            return _camera.position + _camera.forward * distanceFromCamera + _camera.up * (-1f * downOffset);
        }

        private void Update()
        {
            ProcessPositionAndRotation();
        }

        private void ProcessPositionAndRotation()
        {
            if (!_canFollow) return;
            
            if (!stopUpdatingPosition)
            {
                Vector3 newPos = CalculateSnackBarPosition();
                if (lockYPositionUpdate) newPos.y = _initialPos.y;
                transform.position = Vector3.SmoothDamp(transform.position, newPos, ref _smoothPositionVelocity, followHead);
            }

            if (!stopUpdatingDirection)
            {
                Vector3 targetPosition = _camera.position;
                if (freezeXAxisForLookingAtHead) targetPosition.y = transform.position.y;
                
                Vector3 newForward = (targetPosition - transform.position).normalized * (isMirrored ? 1f : -1f);
                transform.forward = Vector3.SmoothDamp(transform.forward, newForward, ref _smoothForwardVelocity, lookAtHead);
            }
        }
    }
}
