using System;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

namespace Vowgan.Contact.Physics
{
    
    [AttributeUsage(AttributeTargets.Field)]
    public class ContactCollisionPresetAttribute : PropertyAttribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public class MinMaxAttribute : PropertyAttribute { }
    
    
#if UNITY_EDITOR
    
    [CustomPropertyDrawer(typeof(ContactCollisionPresetAttribute))]
    public class ContactCollisionPresetDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            ObjectField field = new ObjectField(property.displayName);
            field.BindProperty(property);
            field.objectType = typeof(ContactCollisionPreset);
            return field;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.objectReferenceValue = EditorGUI.ObjectField(
                position, label, property.objectReferenceValue, typeof(ContactCollisionPreset), false);
            EditorGUI.EndProperty();
        }
    }
    
    [CustomPropertyDrawer(typeof(MinMaxAttribute))]
    public class MinMaxAttributeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            
            Vector2Field field = new Vector2Field();
            field.label = property.displayName;
            field.BindProperty(property);
            
            Label xInputLabel = field.Query("unity-x-input").First().Query<Label>().First();
            xInputLabel.text = "Min";
            xInputLabel.style.flexBasis = new StyleLength(28);

            Label yInputLabel = field.Query("unity-y-input").First().Query<Label>().First();
            yInputLabel.text = "Max";
            yInputLabel.style.flexBasis = new StyleLength(28);

            root.Add(field);
            root.Add(base.CreatePropertyGUI(property));

            return root;
        }
    }
    
#endif
}