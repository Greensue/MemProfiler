using UnityEngine;
using System.Collections;
using UnityEditor;


public class EditorTest : EditorWindow {

	// Use this for initialization
	void Start () {
		Debug.Log("Start");
	
	}
	
	// Update is called once per frame
	void Update () {
		Debug.Log("Update");
	
	}

	void Awake()
	{
		Debug.Log("Awake");
	}


	[MenuItem("Window/EditorTest")]
	public static void Init()
	{
		EditorTest window = (EditorTest)EditorWindow.GetWindow(typeof(EditorTest));

		window.Show();
		Debug.Log("MenuItem window");
	}


	public void OnGUI()
	{
		Debug.Log("OnGUI");
	}
}
