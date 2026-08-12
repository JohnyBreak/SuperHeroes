using System;
using UnityEngine;

[Serializable]
public class UnitVelocity
{
    [SerializeField] private Vector3 _velocity;
    
    public Vector3 GetVelocity()
    {
        return _velocity;
    }
    
    public void AddVelocity(Vector3 additionalVelocity)
    {
        _velocity += additionalVelocity;
    }

    public void SetVelocity(Vector3 newVelocity)
    {
        _velocity = newVelocity;
    }

    public void SetXZVelocity(Vector3 newVelocity)
    {
        _velocity = new Vector3(newVelocity.x, _velocity.y, newVelocity.z);
    }
    
    public void ZeroXZVelocity()
    {
        _velocity = new Vector3(0, _velocity.y, 0);
    }
    
    public void ZeroVelocity()
    {
        _velocity = Vector3.zero;
    }
    
    public void ZeroYVelocity()
    {
        _velocity = new Vector3(_velocity.x, 0, _velocity.z);
    }

    public void SetYVelocity(float appliedY)
    {
        _velocity = new Vector3(_velocity.x, appliedY, _velocity.z);
    }
}
