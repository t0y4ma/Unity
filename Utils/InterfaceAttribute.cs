using System;
using UnityEngine;

#if UNITY_EDITOR
public class InterfaceAttribute : PropertyAttribute
{
    public Type InterfaceType;

    public InterfaceAttribute(Type interfaceType)
    {
        InterfaceType = interfaceType;
    }
}
#endif