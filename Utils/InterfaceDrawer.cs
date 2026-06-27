using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(InterfaceAttribute))]
public class InterfaceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attribute = (InterfaceAttribute)this.attribute;

        EditorGUI.BeginProperty(position, label, property);

        Object obj = EditorGUI.ObjectField(
            position,
            label,
            property.objectReferenceValue,
            typeof(MonoBehaviour),
            true);

        if (obj == null)
        {
            property.objectReferenceValue = null;
        }
        else if (attribute.InterfaceType.IsAssignableFrom(obj.GetType()))
        {
            property.objectReferenceValue = obj;
        }
        else
        {
            property.objectReferenceValue = null;
            Debug.LogWarning($"{obj.name} は {attribute.InterfaceType.Name} を実装していません");
        }

        EditorGUI.EndProperty();
    }
}
#endif