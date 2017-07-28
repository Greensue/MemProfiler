using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System;

public class PAEditorUtil
{
    public static object FieldValue(object obj, FieldInfo fieldInfo)
    {
        if (obj == null)
            return "";
        if (fieldInfo == null)
        {
            return "";
            Debug.Log("fieldInfo == null");
        }

        return fieldInfo.GetValue(obj);
    }
    public static string FieldToString(object obj, FieldInfo fieldInfo, string fmt)
    {
        object val = FieldValue(obj, fieldInfo);
        if (val == null)
            return "";

        if (fmt == PAEditorConst.BytesFormatter)
            return PAEditorUtil.FormatBytes((float)val);
        if (fmt == PAEditorConst.PercentsFormatter)
            return PAEditorUtil.PercentsFormatter((float)val);
        if (val is float)
            return ((float)val).ToString(fmt);
        if (val is double)
            return ((double)val).ToString(fmt);
        return val.ToString();
    }

    public static string PercentsFormatter(float val)
    {
        return "%"+val;

    }

    public static string FormatBytes(float bytes)
        {

            if (bytes < 0)
                return "error bytes";
        
            if (bytes<1024)
            {
                return String.Format("{0:F}",bytes) + "b";
            }   
            else if (bytes < 1024 * 1024)
            {   
                return String.Format("{0:F}",bytes/1024) + "kb";
            }
            else {
                return String.Format("{0:F}",bytes/1024/1024) + "mb";
            }
        }


    public static string GetRandomString()
    {
        string path = Path.GetRandomFileName();
        path = path.Replace(".", ""); // Remove period.
        return path;
    }

    private static Dictionary<Color, Texture2D> s_colorTextures = new Dictionary<Color, Texture2D>();
    public static Texture2D getColorTexture(Color c)
    {
        Texture2D tex = null;
        s_colorTextures.TryGetValue(c, out tex);
        if(tex ==null) //Texture2D对象在游戏结束时为null
        {
            tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, c);
            tex.Apply();

            s_colorTextures[c] = tex;
        }
        return tex;
    }

    public static void DrawLabel(string content, GUIStyle style)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(content, style, GUILayout.Width(style.CalcSize(new GUIContent(content)).x + 3));
        EditorGUILayout.EndHorizontal();
    }
}
