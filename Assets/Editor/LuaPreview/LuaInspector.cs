using UnityEngine;
using UnityEditor;
using System.IO;
using System;

/// <summary>
/// lua文本预览绘制方式
/// </summary>
[CustomAsset(".lua")] //绘制方式绑定以.lua为后缀的资源文件
public class LuaInspector : Editor
{
    private string content;
    private string path;

    private bool show = false; //是否开启资源预览--编辑器存执key
    private string showKey = "LuaInspectorShown"; //编辑器存执key
    private void OnEnable()
    { 
        if (Selection.activeObject!=null)
        {
            path = Application.dataPath + "/" + AssetDatabase.GetAssetPath(Selection.activeObject).Substring(7);
        }
         
        try
        {
            TextReader tr = new StreamReader(path);

            content = tr.ReadToEnd();

            tr.Close();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public override void OnInspectorGUI()
    {
        show = EditorPrefs.GetBool(showKey);
        show = GUILayout.Toggle(show, "Show Lua Content");
        
        EditorPrefs.SetBool(showKey, show);
        if (show) //这里为了性能，加了开关
        {
            GUILayout.Label("注：Unity绘制预览有文本长度限制!");
            GUILayout.Label(content);
        }
    }
}

