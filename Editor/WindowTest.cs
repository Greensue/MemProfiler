using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Net;
using System;
using Treemap;
using UnityEditor.MemoryProfiler;
using UnityEngine.Assertions;

namespace MemoryProfilerWindow
{
public class WindowTest : EditorWindow {


	[NonSerialized]
	private bool _registered = false;
	[NonSerialized]
	UnityEditor.MemoryProfiler.PackedMemorySnapshot _snapshot;

	[SerializeField]
	PackedCrawlerData _packedCrawled;

	[NonSerialized]
	CrawledMemorySnapshot _unpackedCrawl;
	public Inspector _inspector;
	
	private static bool m_MinimalGUI = false;
	public int toolbarInt = 0;
	//public TableViewDemoWindow instance = new TableViewDemoWindow();
	private Rect m_DrawArea = new Rect(0, 20, 600, 400);
	public string[] toolbarStrings = new string[] {"SnapshotDisplay", "Snapshots Compare", "Stataics and Anasys"};

	GUILayoutOption [] options = new GUILayoutOption[]{GUILayout.MaxWidth(500)};
	[MenuItem("Window/ProfilerTest")]
    public static void Init()
	{
		
		// Get existing open window or if none, make a new one:

		WindowTest window = (WindowTest)EditorWindow.GetWindow(typeof(WindowTest));
		window.titleContent = new GUIContent("MemoryProfiler");

		window.Show();
	}


	void Awake()
	{
		// create the table with a specified object type
		//_table = new TableView(this, typeof(Group));

		// setup the description for content
		
		//_table.AddColumn("Time_B", "Time_B", 0.15f, TextAnchor.MiddleCenter, "0.0");

		// add test data
		// List<object> entries = new List<object>();
		// for (int i = 0; i < 100; i++)
		// 	entries.Add(FooItem.MakeRandom());
		// _table.RefreshData(entries);

		// // register the event-handling function
		// _table.OnSelected += TableView_Selected;
	}



	public void Initialize()
	{
		if (!_registered)
		{
			UnityEditor.MemoryProfiler.MemorySnapshot.OnSnapshotReceived += IncomingSnapshot;
			_registered = true;
		}
		_table = new TableView(this, typeof(Group));

		_table.AddColumn("_name", "Name", 0.4f, TextAnchor.MiddleLeft);
		//_table.AddColumn("Count_A", "Count_A", 0.1f);
		_table.AddColumn("_membCount", "MemCount", 0.2f, TextAnchor.MiddleCenter, "0.000");
		_table.AddColumn("_Size", "Size", 0.2f);
		_table.AddColumn("_Percent", "Percent", 0.2f, TextAnchor.MiddleCenter, "0.000");

		
		if (_unpackedCrawl == null && _packedCrawled != null && _packedCrawled.valid)
			Unpack();
	}


	void TableView_Selected(object selected, int col)
	{
		FooItem foo = selected as FooItem;
		if (foo == null)
		{
			Debug.LogErrorFormat("the selected object is not a valid one. ({0} expected, {1} got)",
				typeof(FooItem).ToString(), selected.GetType().ToString());
			return;
		}

		string text = string.Format("object '{0}' selected. (col={1})", foo.Name, col);
		Debug.Log(text);
		ShowNotification(new GUIContent(text));
	}
	

		void IncomingSnapshot(PackedMemorySnapshot snapshot)
		{
			_snapshot = snapshot;

			_packedCrawled = new Crawler().Crawl(_snapshot);
			Unpack();
		}


		void Unpack()
		{
			_unpackedCrawl = CrawlDataUnpacker.Unpack(_packedCrawled);
			_inspector = new Inspector(this, _unpackedCrawl, _snapshot);
			//_treeMapView = new TreeMapView(this, _unpackedCrawl);
			GetDataFromSnapShot getDataFromSnapShot = new GetDataFromSnapShot(_unpackedCrawl);
			getDataFromSnapShot.GetCompleteData();
			List<object> entries = new List<object>();
			if (getDataFromSnapShot._group0 != null) {
				Debug.Log ("getDataFromSnapShot != null");
				foreach(Group gr in getDataFromSnapShot._group0)
				{
					entries.Add(gr);
				}
			}
			Debug.Log (entries.Count);
			_table.RefreshData(entries);
		    //_table.RefreshData(getDataFromSnapShot._group0);

		// register the event-handling function
		//_table.OnSelected += TableView_Selected;

		}


		public void SelectThing(ThingInMemory thing)
		{
			_inspector.SelectThing(thing);
			//_treeMapView.SelectThing(thing);
		}



	public void OnGUI()
	{
		Initialize();
		GUIStyle background = "AnimationCurveEditorBackground";
		toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings, options);





		GUILayout.BeginArea(m_DrawArea,background);
		GUILayout.BeginVertical();
			GUILayout.BeginHorizontal ();
		   GUILayout.Label(string.Format("Total memory: 289MB"));
			if (GUILayout.Button("Take Snapshot"))
			{
				UnityEditor.MemoryProfiler.MemorySnapshot.RequestNewSnapshot();
			}
			GUILayout.EndHorizontal ();

		if (_inspector != null)
				_inspector.Draw();

		//GUILayout.BeginArea(new Rect(20, 20, position.width * 0.8f, position.height - 80));
		if (_table != null)
				
			_table.Draw(new Rect(0, 20, m_DrawArea.width * 0.5f, m_DrawArea.height - 20));
		//GUILayout.EndArea();

		GUILayout.EndVertical();
		//GUILayout.Button("Click me",GUILayout.MaxWidth(100));
		//GUILayout.Button("Or me",GUILayout.MaxWidth(100));

		GUILayout.EndArea();




	}


public TableView _table;




}

}