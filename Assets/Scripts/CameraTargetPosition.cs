using DG.Tweening;
using UnityEngine;

public class CameraTargetPosition : MonoBehaviour
{
    private Tween _localPositionTween;
    public void ToggleWallPosition(bool wall)
    {
        Vector3 targetLocalPosition = wall
            ? new Vector3(0f, 0.22f, 0.5f)
            : new Vector3(0f, 1.66f, 0f);
        
        _localPositionTween?.Kill();
        
        _localPositionTween = transform
            .DOLocalMove(targetLocalPosition, 0.2f)
            .SetEase(Ease.OutQuad);
        
        //transform.localPosition = new Vector3(0, (wall) ? 0.22f : 1.66f, (wall) ? 0.5f : 0);
    }
    
    private void OnDestroy()
    {
        _localPositionTween?.Kill();
    }
}
