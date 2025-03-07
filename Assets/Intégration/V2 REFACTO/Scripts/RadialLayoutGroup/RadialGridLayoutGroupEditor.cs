using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(RadialGridLayoutGroup))]
public class RadialGridLayoutGroupEditor : Editor
{
    SerializedProperty m_PaddingProp;
    SerializedProperty m_CellSizeProp;
    SerializedProperty m_SpacingProp;
    
    SerializedProperty m_StartAxisProp;
    SerializedProperty m_ChildAlignmentProp;
    SerializedProperty m_ConstraintProp;
    SerializedProperty m_ConstraintCountProp;

    SerializedProperty m_RotateY;
    SerializedProperty m_RotateZ;
    SerializedProperty m_CircularOffsetX;
    SerializedProperty m_CircularOffsetY;
    SerializedProperty m_IgnoreCellsize;
    SerializedProperty m_RadialShape;
    SerializedProperty m_ChildAlignmentOrder;
    SerializedProperty m_CirclelRadiusMultiplier;
    SerializedProperty m_fDistanceProp;
    SerializedProperty m_MinAngleProp;
    SerializedProperty m_MaxAngleProp;
    SerializedProperty m_StartAngleProp;
    SerializedProperty m_ElementCountProp;

    void OnEnable()
    {
        m_PaddingProp = serializedObject.FindProperty("m_Padding");
        m_CellSizeProp = serializedObject.FindProperty("m_CellSize");
        m_SpacingProp = serializedObject.FindProperty("m_Spacing");
        m_StartAxisProp = serializedObject.FindProperty("m_StartAxis");
        m_ChildAlignmentProp = serializedObject.FindProperty("m_ChildAlignment");
        m_ConstraintProp = serializedObject.FindProperty("m_Constraint");
        m_ConstraintCountProp = serializedObject.FindProperty("m_ConstraintCount");

        m_RotateY = serializedObject.FindProperty("m_RotateY");
        m_RotateZ = serializedObject.FindProperty("m_RotateZ");
        m_CircularOffsetX = serializedObject.FindProperty("m_CircularOffsetX");
        m_CircularOffsetY = serializedObject.FindProperty("m_CircularOffsetY");
        m_IgnoreCellsize = serializedObject.FindProperty("m_IgnoreCellsize");
        m_RadialShape = serializedObject.FindProperty("m_RadialShape");
        m_ChildAlignmentOrder = serializedObject.FindProperty("m_ChildAlignmentOrder");
        m_CirclelRadiusMultiplier = serializedObject.FindProperty("m_CirclelRadiusMultiplier");
        m_fDistanceProp = serializedObject.FindProperty("m_fDistance");
        m_MinAngleProp = serializedObject.FindProperty("m_MinAngle");
        m_MaxAngleProp = serializedObject.FindProperty("m_MaxAngle");
        m_StartAngleProp = serializedObject.FindProperty("m_StartAngle");
        m_ElementCountProp = serializedObject.FindProperty("m_ElementCount");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_PaddingProp);

        // if true, don't display Cell Size
        if (!m_IgnoreCellsize.boolValue)
        {
            EditorGUILayout.PropertyField(m_CellSizeProp);
        }
        
        EditorGUILayout.PropertyField(m_SpacingProp);
        EditorGUILayout.PropertyField(m_StartAxisProp);
        EditorGUILayout.PropertyField(m_ChildAlignmentProp);
        EditorGUILayout.PropertyField(m_ConstraintProp);

        if (m_ConstraintProp.enumValueIndex == (int)GridLayoutGroup.Constraint.FixedColumnCount ||
            m_ConstraintProp.enumValueIndex == (int)GridLayoutGroup.Constraint.FixedRowCount)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_ConstraintCountProp);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(m_RotateY);
        EditorGUILayout.PropertyField(m_RotateZ);
        EditorGUILayout.PropertyField(m_CircularOffsetX);
        EditorGUILayout.PropertyField(m_CircularOffsetY);
        EditorGUILayout.PropertyField(m_IgnoreCellsize);
        if (m_IgnoreCellsize.boolValue)// if true, dont display F-Distance
        {
            EditorGUILayout.PropertyField(m_fDistanceProp);
        }
        EditorGUILayout.PropertyField(m_RadialShape);
        EditorGUILayout.PropertyField(m_ChildAlignmentOrder);
        EditorGUILayout.PropertyField(m_CirclelRadiusMultiplier);
        EditorGUILayout.PropertyField(m_MinAngleProp);
        EditorGUILayout.PropertyField(m_MaxAngleProp);
        EditorGUILayout.PropertyField(m_StartAngleProp);
        EditorGUILayout.PropertyField(m_ElementCountProp);

        serializedObject.ApplyModifiedProperties();
    }
}
