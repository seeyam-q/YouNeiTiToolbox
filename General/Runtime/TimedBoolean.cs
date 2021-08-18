using System;
using UnityEngine;

[Serializable]
public class TimedBoolean
{
    [SerializeField] private bool _value;
    public float Duration => Time.unscaledTime - _valueChangedTime;

    private float _valueChangedTime;

    public bool Value
    {
        get => _value;
        set
        {
            if (value != _value)
            {
                _value = value;
                _valueChangedTime = Time.unscaledTime;
            }
        }
    }

    public bool CheckValueForDuration(bool checkValue, float minDuration)
    {
        if (Value == checkValue)
        {
            if (Duration >= minDuration)
            {
                return true;
            }
        }

        return false;
    }

    public override string ToString()
    {
        return $"[{Value}] {Duration:0.00} ";
    }
}