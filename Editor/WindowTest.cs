﻿using System.Collections;
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
	private bool _compareRegistered = false;
	[NonSerialized]
	UnityEditor.MemoryProfiler.PackedMemorySnapshot _snapshot;
	private Dictionary<string,UnityEditor.MemoryProfiler.PackedMemorySnapshot> _snapshots = new Dictionary<string,UnityEditor.MemoryProfiler.PackedMemorySnapshot>();

	[SerializeField]
	PackedCrawlerData _packedCrawled;

	[NonSerialized]
	CrawledMemorySnapshot _unpackedCrawl;


	Dictionary<string,Inspector> compareInspectors = new Dictionary<string,Inspector>();
	List<Inspector> listCompareInspectors = new List<Inspector>();


	Dictionary<string,CrawledMemorySnapshot> unpackedsnapshots = new Dictionary<string,CrawledMemorySnapshot>();
	List<CrawledMemorySnapshot> listUnpcakSnapshots = new List<CrawledMemorySnapshot>();
	public Inspector _inspector;
	public string _searchString= "";
	public string _sizeString= "";

	public string _compareSearchString= "";
	public string _compareSizeString= "";
	private static bool m_MinimalGUI = false;
	public bool TotoalMemoExist = false;
	public bool guiEnable;
	public int snapshotNum=0;

	public float TotalMemory;
	public int toolbarInt = 0;
	//public TableViewDemoWindow instance = new TableViewDemoWindow();
	private Rect m_DrawArea = new Rect(0, 20, 600, 400);
	private  Rect _compareDrawArea = new Rect(0, 20, 600, 400);
	public string[] toolbarStrings = new string[] {"SnapshotDisplay", "Snapshots Compare", "Stataics and Anasys"};
	GUILayoutOption [] options = new GUILayoutOption[]{GUILayout.MaxWidth(500)};
	

	public TableView _gruopTable;
	public TableView _ItemTable;
	public TableView commonGroupTable;
	public TableView commonItemTable;
	public TableView diffGroupTable0;
	public TableView diffItemTable0;
	public TableView diffGroupTable1;
	public TableView diffItemTable1;


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
		_ItemTable.AddColumn("name", "Name", 0.7f, TextAnchor.MiddleLeft);
		_ItemTable.AddColumn("memorySize", "Size", 0.3f,TextAnchor.MiddleCenter,PAEditorConst.BytesFormatter);
		_ItemTable.OnSelected += itemTabSelected;


		commonGroupTable = new TableView(this,typeof(Group));
		commonGroupTable.AddColumn("_name", "(COMMON) Name", 0.4f, TextAnchor.MiddleLeft);	
		commonGroupTable.AddColumn("_membCount", "MemCount", 0.2f, TextAnchor.MiddleCenter, "0.000");
		commonGroupTable.AddColumn("_Size", "Size", 0.2f,TextAnchor.MiddleCenter,PAEditorConst.BytesFormatter);
		commonGroupTable.AddColumn("_Percent", "Percent", 0.2f, TextAnchor.MiddleCenter, PAEditorConst.PercentsFormatter);
		commonGroupTable.OnSelected += commonGroupTableSelected;


		
		commonItemTable = new TableView(this, typeof(Item));
		commonItemTable.AddColumn("name", "Name", 0.7f, TextAnchor.MiddleLeft);
		commonItemTable.AddColumn("memorySize", "Size", 0.3f,TextAnchor.MiddleCenter,PAEditorConst.BytesFormatter);
		commonItemTable.OnSelected += commomItemTabSelected;










		_gruopTable.SetSortParams(2,true);
		commonGroupTable.SetSortParams(2,true);
		_ItemTable.SetSortParams(1,true);



	}



	void commonGroupTableSelected(object selected, int col)
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

		commonGroupTable.RefreshData(itemEntries);
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

	public void CompareInitial()
	{
		if (!_compareRegistered)
			{
				UnityEditor.MemoryProfiler.MemorySnapshot.OnSnapshotReceived += CompareIncomingSnapshot;
				_compareRegistered = true;
			}
		
		/*if (_unpackedCrawl == null && _packedCrawled != null && _packedCrawled.valid)
			Unpack();*/ 
	}


	void Unpack()
	{
		_unpackedCrawl = CrawlDataUnpacker.Unpack(_packedCrawled);
		_inspector = new Inspector(this, _unpackedCrawl, _snapshot);
		refreshTables();

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
	void commomItemTabSelected(object selected, int col)
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
	void CompareIncomingSnapshot(PackedMemorySnapshot snapshot)
	{
		_unpackedCrawl = CrawlDataUnpacker.Unpack(_packedCrawled);
		_inspector = new Inspector(this, _unpackedCrawl, _snapshot);

		unpackedsnapshots.Add("NewSnapshot"+snapshotNum,_unpackedCrawl);
		listUnpcakSnapshots.Add(_unpackedCrawl);
		listCompareInspectors.Add(_inspector);
		refreshTables();
		compareRefreshTables();
	}



	


	public void refreshTables()
	{
		if (_gruopTable != null&&_unpackedCrawl!=null)
		{
			GetDataFromSnapShot getDataFromSnapShot = new GetDataFromSnapShot(_unpackedCrawl,_searchString,_sizeString);
			getDataFromSnapShot.GetCompleteData();
			
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
				
			
		}
	}

	public void compareRefreshTables()
	{
		CompareSnapshot _compareSnapshot = new CompareSnapshot(listUnpcakSnapshots, _compareSearchString, _compareSizeString);
		_compareSnapshot.Compare();
		if(commonGroupTable != null)
		{
			List<object> entries = new List<object>();
			if (_compareSnapshot.commonGroup != null) 
			{
				//Debug.Log ("getDataFromSnapShot != null"); 
				foreach(Group gr in _compareSnapshot.commonGroup)
				{
					entries.Add(gr);
				}
			}

			Group defaultGroup = new Group();
			defaultGroup = commonGroupTable.RefreshData(entries) as Group;
			if(commonItemTable!= null)
			{
				List<object> items = new List<object>();
				if(defaultGroup != null)
				
				foreach(Item item in defaultGroup._items)
				{
					items.Add(item);
				}

				commonItemTable.RefreshData(items);
			}
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
		toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings, options);
		if(toolbarInt == 0 )
		{
			drawSnapshotDisplay();
		}
		if(toolbarInt == 1 )
		{
			drawSnapshotCompare();

		}
		



	}

	public void drawSnapshotCompare()
	{
		
		
		
		GUIStyle background = "AnimationCurveEditorBackground";
		_compareDrawArea.width = this.position.width - PAEditorConst.InspectorWidth;
		_compareDrawArea.height = this.position.height;
		GUILayout.BeginArea(_compareDrawArea,background);
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal ("Toolbar");
		if(GUILayout.Button("Take Snapshot",EditorStyles.toolbarButton))
		{
			//snapshots.Add();
			UnityEditor.MemoryProfiler.MemorySnapshot.RequestNewSnapshot();

		}

		string enteredString = GUILayout.TextField(_compareSearchString, 100, "ToolbarSeachTextField", GUILayout.MinWidth(200),GUILayout.MaxWidth(300));
		 if (enteredString != _compareSearchString)
        {
            _compareSearchString = enteredString;
           compareRefreshTables();
        }
        if (GUILayout.Button("", "ToolbarSeachCancelButton"))
        {
            _compareSearchString = "";
            GUI.FocusControl(null); // Remove focus if cleared
            compareRefreshTables();
        }
        GUILayout.Space(20);
        GUILayout.Label("Display Size >");

        string enteredsizeString = GUILayout.TextField(_sizeString,100,GUILayout.MaxWidth(40));
        GUILayout.Label("mb ");

        if(_compareSizeString != enteredsizeString)
    	{
	    	_compareSizeString = enteredsizeString;
	        compareRefreshTables();
    	}
    	GUILayout.Space(30);
		GUILayout.EndHorizontal();

		if(commonGroupTable != null )
		commonGroupTable.Draw(new Rect(5, 22, _compareDrawArea.width * 0.5f, _compareDrawArea.height*0.3f));

		if (commonItemTable != null )
		{
			commonItemTable.Draw(new Rect(_compareDrawArea.width * 0.51f,22,_compareDrawArea.width * 0.48f, _compareDrawArea.height*0.3f));
		}

		
		GUILayout.EndVertical();
		GUILayout.EndArea();
		if (_inspector!=null && _unpackedCrawl!=null)
				_inspector.Draw();



	

	}


	public void drawSnapshotDisplay()
	{
		GUIStyle background = "AnimationCurveEditorBackground";
		m_DrawArea.width = this.position.width - PAEditorConst.InspectorWidth;
		m_DrawArea.height = this.position.height;
		GUILayout.BeginArea(m_DrawArea,background);
		GUILayout.BeginVertical();

		GUILayout.BeginHorizontal ("Toolbar");
		if (GUILayout.Button("Take Snapshot",EditorStyles.toolbarButton))
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
        GUILayout.Space(20);
        GUILayout.Label("Display Size >");

        string enteredsizeString = GUILayout.TextField(_sizeString,100,GUILayout.MaxWidth(40));
        GUILayout.Label("mb ");

        if(_sizeString != enteredsizeString)
    	{
	    	_sizeString = enteredsizeString;
	        refreshTables();

    	}


		if(TotoalMemoExist)
		{
			GUILayout.Label(string.Format("Total memory: {0}",PAEditorUtil.FormatBytes(TotalMemory)),GUILayout.MinWidth(200));

		}
		else
		{
			GUILayout.Label(string.Format("Total memory: "));
		}
		GUILayout.Space(30);
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