using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializeReferenceDropdownAttribute))]
public class SerializeReferenceDropdownPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var labelPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(labelPosition, label);

        var buttonPosition = new Rect(position.x + EditorGUIUtility.labelWidth + 2, position.y, position.width - EditorGUIUtility.labelWidth - 2, EditorGUIUtility.singleLineHeight);

        string typeName = "Null (Select Type)";
        if (!string.IsNullOrEmpty(property.managedReferenceFullTypename))
        {
            var parts = property.managedReferenceFullTypename.Split(' ');
            if (parts.Length > 1) typeName = parts[1].Split('.').Last();
        }

        if (GUI.Button(buttonPosition, typeName, EditorStyles.popup))
        {
            ShowTypeSelector(property);
        }

        EditorGUI.PropertyField(position, property, GUIContent.none, true);

        EditorGUI.EndProperty();
    }

    private void ShowTypeSelector(SerializedProperty property)
    {
        GenericMenu menu = new GenericMenu();

        Type fieldType = GetFieldType(fieldInfo);

        if (fieldType == null) return;

        var types = TypeCache.GetTypesDerivedFrom(fieldType)
            .Where(p => !p.IsAbstract && !p.IsInterface)
            .ToList();

        menu.AddItem(new GUIContent("Null"), false, () =>
        {
            property.managedReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
        });

        foreach (var type in types)
        {
            menu.AddItem(new GUIContent(type.Name), false, () =>
            {
                property.managedReferenceValue = Activator.CreateInstance(type);
                property.serializedObject.ApplyModifiedProperties();
            });
        }

        menu.ShowAsContext();
    }

    private Type GetFieldType(FieldInfo fieldInfo)
    {
        if (fieldInfo == null) return null;
        Type type = fieldInfo.FieldType;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            return type.GetGenericArguments()[0];
        }
        else if (type.IsArray)
        {
            return type.GetElementType();
        }
        return type;
    }
}
