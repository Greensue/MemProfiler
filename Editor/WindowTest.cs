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
	public TableView _gruopTable;
	public TableView _ItemTable;
	


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
		//_gruopTable = new TableView(this, typeof(Group));

		// setup the description for content
		
		//_gruopTable.AddColumn("Time_B", "Time_B", 0.15f, TextAnchor.MiddleCenter, "0.0");

		// add test data
		// List<object> entries = new List<object>();
		// for (int i = 0; i < 100; i++)
		// 	entries.Add(FooItem.MakeRandom());
		// _gruopTable.RefreshData(entries);

		// // register the event-handling function
		// _gruopTable.OnSelected += TableView_Selected;
		_gruopTable = new TableView(this, typeof(Group));

		_gruopTable.AddColumn("_name", "Name", 0.4f, TextAnchor.MiddleLeft);
		//_gruopTable.AddColumn("Count_A", "Count_A", 0.1f);
		_gruopTable.AddColumn("_membCount", "MemCount", 0.2f, TextAnchor.MiddleCenter, "0.000");
		_gruopTable.AddColumn("_Size", "Size", 0.2f,TextAnchor.MiddleCenter,PAEditorConst.BytesFormatter);
		_gruopTable.AddColumn("_Percent", "Percent", 0.2f, TextAnchor.MiddleCenter, PAEditorConst.PercentsFormatter);
		_gruopTable.OnSelected += groupTabSelected;

		_ItemTable = new TableView(this, typeof(Item));
		_ItemTable.AddColumn("name", "Name", 0.5f, TextAnchor.MiddleLeft);
		_ItemTable.AddColumn("memorySize", "Size", 0.5f,TextAnchor.MiddleCenter,PAEditorConst.BytesFormatter);
		_ItemTable.OnSelected += itemTabSelected;



	}



	public void Initialize()
	{
		if (!_registered)
		{
			UnityEditor.MemoryProfiler.MemorySnapshot.OnSnapshotReceived += IncomingSnapshot;
			_registered = true;
		}
		
		
		if (_unpackedCrawl == null && _packedCrawled != null && _packedCrawled.valid)
			Unpack();
	}


	void groupTabSelected(object selected, int col)
	{
		Group gr = selected as Group;
		if(gr == null)
		{
			return;
		}

		List<object> itemEntries = new List<object>();
		foreach(Item item in gr._items)
		{
			itemEntries.Add(item);
		}

		_ItemTable.RefreshData(itemEntries);


	}


	void itemTabSelected(object selected, int col)
	{

        var itemObject = selected as Item;
        if (itemObject == null)
            return;
        WindowTest window = (WindowTest)EditorWindow.GetWindow(typeof(WindowTest));

        window.SelectThing(itemObject._thingInMemory);

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
			/*GetDataFromSnapShot getDataFromSnapShot = new GetDataFromSnapShot(_unpackedCrawl);
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
			_gruopTable.RefreshData(entries);
			this.Repaint();*/


		    //_gruopTable.RefreshData(getDataFromSnapShot._group0);

		// register the event-handling function
		//_gruopTable.OnSelected += TableView_Selected;

		}


		public void SelectThing(ThingInMemory thing)
		{
			_inspector.SelectThing(thing);
			
		}



	public void OnGUI()
	{
		Initialize();
		GUIStyle background = "AnimationCurveEditorBackground";
		toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings, options);




		m_DrawArea.width = this.position.width - PAEditorConst.InspectorWidth;
		m_DrawArea.height = this.position.height;
		
		GUILayout.BeginArea(m_DrawArea,background);
		GUILayout.BeginVertical();
			GUILayout.BeginHorizontal ();
		   GUILayout.Label(string.Format("Total memory: 289MB"));
			if (GUILayout.Button("Take Snapshot"))
			{
				UnityEditor.MemoryProfiler.MemorySnapshot.RequestNewSnapshot();
				
			}
			GUILayout.EndHorizontal ();

			
			if (_gruopTable != null&&_unpackedCrawl!=null)
			{
				GetDataFromSnapShot getDataFromSnapShot = new GetDataFromSnapShot(_unpackedCrawl);
				getDataFromSnapShot.GetCompleteData();
				List<object> entries = new List<object>();
				if (getDataFromSnapShot._group0 != null) 
				{
					//Debug.Log ("getDataFromSnapShot != null");
					foreach(Group gr in getDataFromSnapShot._group0)
					{
						entries.Add(gr);
					}
				}
					//Debug.Log (entries.Count);
					_gruopTable.RefreshData(entries);

					_gruopTable.Draw(new Rect(5, 22, m_DrawArea.width * 0.5f, m_DrawArea.height - 20));

			}
			if (_ItemTable != null)
			{
				Debug.Log("_ItemTable != null");

				
				_ItemTable.Draw(new Rect(m_DrawArea.width * 0.51f,22,m_DrawArea.width * 0.48f, m_DrawArea.height - 20));
			}

				
			    

		//GUILayout.BeginArea(new Rect(20, 20, position.width * 0.8f, position.height - 80));
		
		//GUILayout.EndArea();

		GUILayout.EndVertical();
		//GUILayout.Button("Click me",GUILayout.MaxWidth(100));
		//GUILayout.Button("Or me",GUILayout.MaxWidth(100));

		GUILayout.EndArea();
		if (_inspector != null)
				_inspector.Draw();




	}







}

}