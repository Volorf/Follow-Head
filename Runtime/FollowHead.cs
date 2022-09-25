using UnityEngine;

namespace Volorf.FollowHead
{
    public class FollowHead : MonoBehaviour
    {
        [Header("Positioning")]
        [SerializeField] private float distanceFromHead = 1f;
        [SerializeField] private float downOffset = 0f;
        [SerializeField] private bool isMirrored = false;
        
        [Space(16)]
        [Header("Smoothness")]
        [SerializeField] private float followHeadSmoothness = 0.5f;
        [SerializeField] private float lookAtHeadSmoothness = 0.5f;
        
        [Space(16)]
        [SerializeField] private bool canFollow = true;
        
        private Vector3 _smoothPositionVelocity;
        private Vector3 _smoothForwardVelocity;
        private Transform _camera;
        

        public void StopFollowingHead() => canFollow = false;
        public void ResumeFollowingHead() => canFollow = true;

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
        }
    
        private Vector3 CalculateSnackBarPosition()
        {
            return _camera.position + _camera.forward * distanceFromHead + _camera.up * (-1f * downOffset);
        }

        private void Update()
        {
            if (!canFollow) return;
        
            Vector3 newPos = CalculateSnackBarPosition();
            transform.position = Vector3.SmoothDamp(transform.position, newPos, ref _smoothPositionVelocity, followHeadSmoothness);

            Vector3 newForward = (_camera.position - transform.position).normalized * (isMirrored ? 1f : -1f);
            transform.forward = Vector3.SmoothDamp(transform.forward, newForward, ref _smoothForwardVelocity, lookAtHeadSmoothness);
        }
    }
}
