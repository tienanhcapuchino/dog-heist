using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// Gán tham chiếu vào field [SerializeField] private bằng SerializedObject, giống thao tác kéo thả trong Inspector.
    /// Ném lỗi ngay khi field không tồn tại hoặc sai kiểu, để phát hiện sớm khi field trong code gameplay bị đổi tên.
    /// </summary>
    internal static class SerializedWiring
    {
        public static void Assign(Object target, string propertyPath, Object value)
        {
            var serialized = new SerializedObject(target);
            var property = FindReferenceProperty(serialized, target, propertyPath);

            property.objectReferenceValue = value;
            EnsureAccepted(property, target, propertyPath, value);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void AssignArray(Object target, string propertyPath, IReadOnlyList<Object> values)
        {
            var serialized = new SerializedObject(target);
            var property = FindProperty(serialized, target, propertyPath);
            if (!property.isArray)
            {
                throw new ArgumentException($"[Greybox] {target.GetType().Name}.{propertyPath} không phải mảng.", nameof(propertyPath));
            }

            property.arraySize = values.Count;
            for (var i = 0; i < values.Count; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                element.objectReferenceValue = values[i];
                EnsureAccepted(element, target, $"{propertyPath}[{i}]", values[i]);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SerializedProperty FindReferenceProperty(SerializedObject serialized, Object target, string propertyPath)
        {
            var property = FindProperty(serialized, target, propertyPath);
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                throw new ArgumentException($"[Greybox] {target.GetType().Name}.{propertyPath} không phải field tham chiếu.", nameof(propertyPath));
            }

            return property;
        }

        private static SerializedProperty FindProperty(SerializedObject serialized, Object target, string propertyPath)
        {
            var property = serialized.FindProperty(propertyPath);
            if (property == null)
            {
                throw new ArgumentException(
                    $"[Greybox] {target.GetType().Name} không có field '{propertyPath}'. Field có bị đổi tên không?",
                    nameof(propertyPath));
            }

            return property;
        }

        // Unity âm thầm gán null khi kiểu không khớp, nên phải kiểm tra lại sau khi gán.
        private static void EnsureAccepted(SerializedProperty property, Object target, string propertyPath, Object value)
        {
            if (value != null && property.objectReferenceValue != value)
            {
                throw new ArgumentException(
                    $"[Greybox] {target.GetType().Name}.{propertyPath} không nhận kiểu {value.GetType().Name}.",
                    nameof(value));
            }
        }
    }
}
