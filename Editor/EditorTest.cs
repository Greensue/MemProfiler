

 using UnityEditor;
 using UnityEngine;
 using System.Collections.Generic;

 
 public class MyWindow : EditorWindow
 {


 	static Dictionary<string,string> snapTest = new Dictionary<string,string>();
     bool groupEnabled;
     bool myBool = true;
     float myFloat = 1.23f;
     static string SanpName;
     
     // Add menu item named "My Window" to the Window menu
     [MenuItem("Window/My Window")]
     public static void ShowWindow()
     {
         //Show existing window instance. If one doesn't exist, make one.
        MyWindow window = (MyWindow)EditorWindow.GetWindow(typeof(MyWindow));
        window.Show();
     }
     
     void OnGUI()
     {
         
         
         groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
             myBool = EditorGUILayout.Toggle ("Toggle", myBool);
             myFloat = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
         EditorGUILayout.EndToggleGroup ();
         if(GUILayout.Button("new napshot",GUILayout.MaxWidth(100)))
         {
         	DialogWindow window = (DialogWindow)EditorWindow.GetWindow(typeof(DialogWindow));
        	window.Show();
        	

         }
         if(GUILayout.Button("Remove",GUILayout.MaxWidth(100)))
         {

         }
         GUILayout.BeginHorizontal ("Toolbar");
         foreach(string st in snapTest.Keys)
         {
         	if(GUILayout.Button(st,EditorStyles.toolbarButton,GUILayout.MaxWidth(100)))
	        {

	        }
         }
         GUILayout.EndHorizontal ();
         
         
         
         
     }

     public class DialogWindow : EditorWindow
	 {
	    
	    
	     public static void ShowWindow()
	     {
	         //Show existing window instance. If one doesn't exist, make one.
	        DialogWindow window = (DialogWindow)EditorWindow.GetWindow(typeof(DialogWindow));
	        window.ShowUtility();
	     }

	     public string myString;
	     
	     void OnGUI()
	     {
	         
	         myString = EditorGUILayout.TextField ("Text Field", myString);
	         if(GUILayout.Button("OK",GUILayout.MaxWidth(100))|| Input.GetKeyDown("enter"))
	         {
	         	SanpName = myString;
	         	if(snapTest!=null && SanpName!= null)
	         	{
 			        if(!snapTest.ContainsKey(SanpName))
	         		snapTest.Add(SanpName,SanpName);
	         		else
	         		{//string text = string.Format("object '{0}' selected. (col={1})", foo.Name, col);
        //Debug.Log(text);
        				ShowNotification(new GUIContent(SanpName+" is already exist, please try another name!"));
	         			Debug.LogWarningFormat(" '{0}' is already exist, please try another name!", SanpName);
	         		}

	         	}
	         	else
	         	{
	         		Debug.LogWarningFormat("the snapshot name could not be empty!");
	         		ShowNotification(new GUIContent("the snapshot name could not be empty!"));
	         	}
	         	Close();

	         }
	         
	         
	     }
	 }
 }