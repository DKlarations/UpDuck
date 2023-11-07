using System.Collections;
using UnityEngine;
using Cinemachine;

public class CameraFollow : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    private Vector3 targetOffset;
    private bool isAdjusting = false;

    [SerializeField] private float horizontalOffset = 1f; // Default offset for horizontal direction
    [SerializeField] private float verticalOffsetJump = 2f; // Adjust for jump
    [SerializeField] private float verticalOffsetGround = 0f; // Adjust for ground

    public void AdjustCameraForDirection(bool facingRight)
    {
        // Update the horizontal component of the targetOffset
        targetOffset.x = facingRight ? Mathf.Abs(horizontalOffset) : -Mathf.Abs(horizontalOffset);
        StartAdjustment();
    }

    public void AdjustCameraForJump(bool isJumping)
    {
        // Update the vertical component of the targetOffset
        targetOffset.y = isJumping ? verticalOffsetJump : verticalOffsetGround;
        StartAdjustment();
    }

    private void StartAdjustment()
    {
        // Start or restart the adjustment as necessary
        if (isAdjusting)
        {
            // Interrupt the current coroutine if needed
            StopAllCoroutines();
        }
        StartCoroutine(SmoothAdjust());
    }

    private IEnumerator SmoothAdjust()
    {
        isAdjusting = true;
        CinemachineFramingTransposer transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        Vector3 initialOffset = transposer.m_TrackedObjectOffset;

        float elapsed = 0f;
        float duration = 0.3f; // Adjust as needed
        while (!Mathf.Approximately(transposer.m_TrackedObjectOffset.x, targetOffset.x) || 
               !Mathf.Approximately(transposer.m_TrackedObjectOffset.y, targetOffset.y))
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);
            transposer.m_TrackedObjectOffset = Vector3.Lerp(initialOffset, targetOffset, t);
            yield return null;
        }

        // Ensure exact target is set
        transposer.m_TrackedObjectOffset = targetOffset;
        isAdjusting = false;
    }
}
