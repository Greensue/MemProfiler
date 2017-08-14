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

	public string _searchString= "";
	public string _sizeString= "";
	private static bool m_MinimalGUI = false;
	public bool TotoalMemoExist = false;

	public float TotalMemory;
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
		
		_gruopTable = new TableView(this, typeof(Group));

		_gruopTable.AddColumn("_name", "Name", 0.4f, TextAnchor.MiddleLeft);
		
		_gruopTable.AddColumn("_membCount", "MemCount", 0.2f, TextAnchor.MiddleCenter, "0.000");
		_gruopTable.AddColumn("_Size", "Size", 0.2f,TextAnchor.MiddleCenter,PAEditorConst.BytesFormatter);
		_gruopTable.AddColumn("_Percent", "Percent", 0.2f, TextAnchor.MiddleCenter, PAEditorConst.PercentsFormatter);
		_gruopTable.OnSelected += groupTabSelected;

		_ItemTable = new TableView(this, typeof(Item));
		_ItemTable.AddColumn("name", "Name", 0.5f, TextAnchor.MiddleLeft);
		_ItemTable.AddColumn("memorySize", "Size", 0.5f,TextAnchor.MiddleCenter,PAEditorConst.BytesFormatter);
		_ItemTable.OnSelected += itemTabSelected;



		_gruopTable.SetSortParams(2,true);
		_ItemTable.SetSortParams(1,true);



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

        this.SelectThing(itemObject._thingInMemory);
        Repaint();

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
		refreshTables();

	}


	public void refreshTables()
	{
		if (_gruopTable != null&&_unpackedCrawl!=null)
		{
			GetDataFromSnapShot getDataFromSnapShot = new GetDataFromSnapShot(_unpackedCrawl);
			getDataFromSnapShot.GetCompleteData(_searchString,_sizeString);
			
			TotalMemory = getDataFromSnapShot.MemTotalSize;
			TotoalMemoExist = true;
			List<object> entries = new List<object>();
			if (getDataFromSnapShot._group0 != null) 
			{
				//Debug.Log ("getDataFromSnapShot != null"); 
				foreach(Group gr in getDataFromSnapShot._group0)
				{
					entries.Add(gr);
				}
			}

			Group defaultGroup = new Group();

			
			defaultGroup = _gruopTable.RefreshData(entries) as Group;

			


				



				if(_ItemTable != null)
				{
					List<object> items = new List<object>();
					if(defaultGroup != null)
					
					foreach(Item item in defaultGroup._items)
					{
						items.Add(item);
					}

					_ItemTable.RefreshData(items);
				}
				
				

				//_gruopTable.Draw(new Rect(5, 22, m_DrawArea.width * 0.5f, m_DrawArea.height - 20));

		}
	}



	public void SelectThing(ThingInMemory thing)
	{
		_inspector.SelectThing(thing);
		
	}

	public void Update()
	{
		
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
	   
		if (GUILayout.Button("Take Snapshot",GUILayout.MinWidth(50),GUILayout.MaxWidth(100)))
		{
			TotoalMemoExist = false;
			UnityEditor.MemoryProfiler.MemorySnapshot.RequestNewSnapshot();
			
			
		}


		//searching box
		string enteredString = GUILayout.TextField(_searchString, 100, "ToolbarSeachTextField", GUILayout.MinWidth(200),GUILayout.MaxWidth(300));
		 if (enteredString != _searchString)
        {
            _searchString = enteredString;
           refreshTables();
        }
        if (GUILayout.Button("", "ToolbarSeachCancelButton"))
        {
            _searchString = "";
            GUI.FocusControl(null); // Remove focus if cleared
            refreshTables();
        }
        GUILayout.Label("Display Size > ");
        string enteredsizeString = GUILayout.TextField(_sizeString,25,GUILayout.MaxWidth(40));
        
    	if(_sizeString != enteredsizeString)
    	{
	    	_sizeString = enteredsizeString;
	        refreshTables();

    	}
        
        GUILayout.Label("mb");
		if(TotoalMemoExist)
		{
			GUILayout.Label(string.Format("Total memory: {0}",PAEditorUtil.FormatBytes(TotalMemory)));

		}
		else
		{
			GUILayout.Label(string.Format("Total memory: "));
		}
		
		GUILayout.EndHorizontal ();

	    if(_gruopTable != null && _unpackedCrawl!=null)
		_gruopTable.Draw(new Rect(5, 22, m_DrawArea.width * 0.5f, m_DrawArea.height - 20));



		if (_ItemTable != null && _unpackedCrawl!=null)
		{
			_ItemTable.Draw(new Rect(m_DrawArea.width * 0.51f,22,m_DrawArea.width * 0.48f, m_DrawArea.height - 20));
		}

		
		GUILayout.EndVertical();
		GUILayout.EndArea();
		if (_inspector!=null && _unpackedCrawl!=null)
				_inspector.Draw();




	}


	void OnDestroy()
    {
        if (_gruopTable != null)
            _gruopTable.Dispose();
        if (_ItemTable != null)
            _ItemTable.Dispose();

        _gruopTable = null;
        _ItemTable = null;
    }







}

}