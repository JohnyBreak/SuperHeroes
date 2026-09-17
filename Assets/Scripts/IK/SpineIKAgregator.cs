using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class SpineIKAgregator : MonoBehaviour
{
    [SerializeField] private Rig _rig;
    [SerializeField] private HandsCurveRaycaster _hands;
    [SerializeField] private SpineIKSolver _spine;

    private void Awake()
    {
        Disable();
    }

    [ContextMenu("Enable")]
    public void Enable()
    {
        _rig.weight = 1;
        _hands.Enable();
        _spine.Enable();
    }
    
    [ContextMenu("Disable")]
    public void Disable()
    {
        _rig.weight = 0;
        _hands.Disable();
        _spine.Disable();
    }
}
