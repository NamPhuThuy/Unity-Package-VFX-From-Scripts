using System;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Object that auto rotate through time
/// </summary>

public class ObjRotateable : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] float speed = 360f;
    private Vector3 _rotateDirection;
    private float _directionMultiply = 1;

    
    [Header("Flags")]
    [SerializeField] private bool isRotateX = false;
    [SerializeField] private bool isRotateY = false;
    [SerializeField] private bool isRotateZ = false;
    [SerializeField] private bool isRotateSpaceWorld = false;
    [SerializeField] private bool isUseUnscaledDeltaTime = false;
    private Space _space;

    #region MonoBehaviour

    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        if (isUseUnscaledDeltaTime)
        {
            transform.Rotate((speed * Time.unscaledDeltaTime) * _rotateDirection, _space);
        }
        else transform.Rotate((speed * Time.deltaTime) * _rotateDirection, _space);
    }

    #endregion

    #region Private Methods

    private void Setup()
    {
        _rotateDirection = new Vector3(isRotateX ? 1 : 0, isRotateY ? 1 : 0, isRotateZ ? 1 : 0) * _directionMultiply;
        _space = isRotateSpaceWorld ? Space.World : Space.Self;
    }

    #endregion

    #region Public Methods

    public void UpdateRotateDirection()
    {
        Setup();
    }

    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(ObjRotateable))]
public class RotateableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Add custom text at the highest place
        EditorGUILayout.LabelField("Rotate around a specific axis (notice the color of the axis when press W-key)", EditorStyles.boldLabel);
        
        DrawDefaultInspector();

        ObjRotateable objRotateable = (ObjRotateable)target;

        if (GUILayout.Button("Update Rotate Direction"))
            objRotateable.UpdateRotateDirection();
    }
}
#endif