using Futile;
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RXProfiler))]
public class RXProfilerEditor : Editor
{
    public static Type FLOAT = typeof(float);
    public static Type INT = typeof(int);
    public static Type STRING = typeof(string);
    public static Type COLOR = typeof(Color);
    public static Type VECTOR2 = typeof(Vector2);

    public void OnEnable()
    {
        if( FearsomeMonstrousBeast.instance != null )
        {
            FearsomeMonstrousBeast.instance.SignalUpdate += HandleSignalUpdate;
        }
    }

    public void OnDisable()
    {
        if(FearsomeMonstrousBeast.instance != null)
        {
            FearsomeMonstrousBeast.instance.SignalUpdate -= HandleSignalUpdate;
        }
    }
    
    override public void OnInspectorGUI() 
    {
        foreach(KeyValuePair<Type, List<WeakReference>> entry in RXProfiler.instancesByType)
        {
            int instanceCount = entry.Value.Count;
            
            if(instanceCount > 0)
            {
                GUILayout.Label(entry.Key.Name + " : " + instanceCount,EditorStyles.boldLabel);
            }
        }
    }

    private void HandleSignalUpdate() 
    {
        if(Time.frameCount % 30 == 0) //update every 30 frames
        {
            Repaint();
        }
    }
}

#endif