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
	private bool _compare = false;
	private GUISkin myskin;
	[NonSerialized]
	UnityEditor.MemoryProfiler.PackedMemorySnapshot _snapshot;
	private Dictionary<string,UnityEditor.MemoryProfiler.PackedMemorySnapshot> _snapshots = new Dictionary<string,UnityEditor.MemoryProfiler.PackedMemorySnapshot>();

	[SerializeField]
	PackedCrawlerData _packedCrawled;

	//[NonSerialized]
	static CrawledMemorySnapshot _unpackedCrawl;


	Dictionary<string,Inspector> compareInspectors = new Dictionary<string,Inspector>();
	List<Inspector> listCompareInspectors = new List<Inspector>();
	List<string> _Keys = new List<string>();


	Dictionary<string,CrawledMemorySnapshot> activeUnpackedsnapshots = new Dictionary<string,CrawledMemorySnapshot>();
	static Dictionary<string,CrawledMemorySnapshot> unpackedsnapshots = new Dictionary<string,CrawledMemorySnapshot>();
	static string SanpName;
	List<CrawledMemorySnapshot> listUnpcakSnapshots = new List<CrawledMemorySnapshot>();
	public Inspector _inspector;
	public string _searchString= "";
	public string _sizeString= "";

	public string _compareSearchString= "";
	public string _compareSizeString= "";
	private static bool m_MinimalGUI = false;
	public bool TotoalMemoExist = false;
	public bool guiEnable;
	

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

	public TableView moreGruopTab;
	public TableView moreItemTab;


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
		InitialTable(ref _gruopTable,ref _ItemTable,"","_gruopTable","_ItemTable");	
		InitialTable(ref commonGroupTable,ref commonItemTable,"(COMMON)","commonGroupTable","commonItemTable");		
		InitialTable(ref diffGroupTable0,ref diffItemTable0,"(SPECIAL IN0)","diffGroupTable0","diffItemTable0");
		InitialTable(ref diffGroupTable1,ref diffItemTable1,"(SPECIAL IN1)","diffGroupTable1","diffItemTable1");
		InitialTable(ref moreGruopTab,ref moreItemTab,"(COMMON)","moreThan2Guop","moreThan2Item");

	}
	void OnEnable()
     {
     	 myskin = (GUISkin)Resources.Load("Myskin");
     
     }

	void InitialTable(ref TableView grouptab, ref TableView itemTab,string model,string G_name,string I_name)
	{
		grouptab = new TableView(this,typeof(Group),G_name);
		grouptab.AddColumn("_name", model+"Name", 0.4f, TextAnchor.MiddleLeft);	
		grouptab.AddColumn("_membCount", "MemCount", 0.2f, TextAnchor.MiddleCenter, "0.000");
		grouptab.AddColumn("_Size", "Size", 0.2f,TextAnchor.MiddleCenter,PAEditorConst.BytesFormatter);
		grouptab.AddColumn("_Percent", "Percent", 0.2f, TextAnchor.MiddleCenter, PAEditorConst.PercentsFormatter);
		grouptab.SetSortParams(2,true);
		itemTab = new TableView(this, typeof(Item),I_name);
		itemTab.AddColumn("name", "Name", 0.7f, TextAnchor.MiddleLeft);
		itemTab.AddColumn("memorySize", "Size", 0.3f,TextAnchor.MiddleCenter,PAEditorConst.BytesFormatter);
		itemTab.SetSortParams(1,true);
		grouptab.OnSelected += groupTabSelected;
		itemTab.OnSelected += itemTabSelected;
		

	}



	/*void commonGroupTableSelected(object selected, int col)
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
	}*/




	public void Initialize()
	{
		if (!_registered)
		{
			UnityEditor.MemoryProfiler.MemorySnapshot.OnSnapshotReceived += IncomingSnapshot;
			UnityEditor.MemoryProfiler.MemorySnapshot.OnSnapshotReceived -= CompareIncomingSnapshot;
			_registered = true;
		}
		if(_compareRegistered)
		{
			UnityEditor.MemoryProfiler.MemorySnapshot.OnSnapshotReceived -= CompareIncomingSnapshot;
			_compareRegistered = false;

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
		if(_registered)
		{
			UnityEditor.MemoryProfiler.MemorySnapshot.OnSnapshotReceived -= IncomingSnapshot;
			_registered = false;
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


	void groupTabSelected(object selected, int col,string _name)
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
		if(_name.Equals("_gruopTable"))
		{
			_ItemTable.RefreshData(itemEntries);

		}
		if(_name.Equals("commonGroupTable"))
		{
			commonItemTable.RefreshData(itemEntries);

		}
		if(_name.Equals("diffGroupTable0"))
		{
			diffItemTable0.RefreshData(itemEntries);

		}
		if(_name.Equals("diffGroupTable1"))
		{
			diffItemTable1.RefreshData(itemEntries);

		}
		if(_name.Equals("moreThan2Guop"))
		{
			moreItemTab.RefreshData(itemEntries);

		}





	}


	void itemTabSelected(object selected, int col,string s)
	{

        var itemObject = selected as Item;
        if (itemObject == null)
            return;

        this.SelectThing(itemObject._thingInMemory);
        Repaint();

	}
	/*void commomItemTabSelected(object selected, int col)
	{

        var itemObject = selected as Item;
        if (itemObject == null)
            return;

        this.SelectThing(itemObject._thingInMemory);
        Repaint();

	}*/
	
	

	void IncomingSnapshot(PackedMemorySnapshot snapshot)
	{
		_snapshot = snapshot;

		_packedCrawled = new Crawler().Crawl(_snapshot);
		Unpack();
	}
	void CompareIncomingSnapshot(PackedMemorySnapshot snapshot)
	{
		_snapshot = snapshot;

		_packedCrawled = new Crawler().Crawl(_snapshot);
		_unpackedCrawl = CrawlDataUnpacker.Unpack(_packedCrawled);
		_inspector = new Inspector(this, _unpackedCrawl, _snapshot);

			DialogWindow _window = new DialogWindow();
			_window = (DialogWindow)EditorWindow.GetWindow(typeof(DialogWindow));
		    _window.ShowUtility();
		//listUnpcakSnapshots.Add(_unpackedCrawl);
		listCompareInspectors.Add(_inspector);
		refreshTables();
		//compareRefreshTables();
	}

	public class DialogWindow : EditorWindow
	 {
	     public string myString;
	     
	     void OnGUI()
	     {
	         
	         myString = EditorGUILayout.TextField ("Text Field", myString);
	         if(GUILayout.Button("OK",GUILayout.MaxWidth(100))|| Input.GetKeyDown(KeyCode.Return))
	         {
	         	SanpName = myString;
	         	if(unpackedsnapshots!=null && SanpName!= null)
	         	{
 			        if(!unpackedsnapshots.ContainsKey(SanpName))
	         			unpackedsnapshots.Add(SanpName,_unpackedCrawl);
	         		else
	         		{
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
		listUnpcakSnapshots.Clear();
		foreach(string snapKey in _Keys)
		{
			listUnpcakSnapshots.Add(activeUnpackedsnapshots[snapKey]);

		}
		Debug.Log ("love you"+listUnpcakSnapshots.Count);
		CompareSnapshot _compareSnapshot = new CompareSnapshot(listUnpcakSnapshots, _compareSearchString, _compareSizeString);
		_compareSnapshot.Compare();
		Debug.Log ("love you"+_compareSnapshot.commonGroup.Count);
		
		
		Refesh(commonGroupTable,commonItemTable,_compareSnapshot.commonGroup);
		Refesh(diffGroupTable0,diffItemTable0,_compareSnapshot.gruopIn0NotIn1);
		Refesh(diffGroupTable1,diffItemTable1,_compareSnapshot.gruopIn1NotIn0); 
		Refesh(moreGruopTab,moreItemTab,_compareSnapshot.moreCommonGroup);

	}


	public void Refesh(TableView grupTab,TableView itemTab,List<Group> m_group)
	{
		if(grupTab != null)
		{
			List<object> entries = new List<object>();
			if (m_group != null) 
			{
				//Debug.Log ("getDataFromSnapShot != null"); 
				foreach(Group gr in m_group)
				{
					entries.Add(gr);
				}
			}

			Group defaultGroup = new Group();
			defaultGroup = grupTab.RefreshData(entries) as Group;
			if(itemTab!= null)
			{
				List<object> items = new List<object>();
				if(defaultGroup != null)
				
				foreach(Item item in defaultGroup._items)
				{
					items.Add(item);
				}

				itemTab.RefreshData(items);
			}
		}

	}








	public void SelectThing(ThingInMemory thing)
	{
		_inspector.SelectThing(thing);
		
	}

	



	public void OnGUI()
	{
		
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

	


	public void drawSnapshotDisplay()
	{
		Initialize();
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
        _unpackedCrawl = null;
        unpackedsnapshots.Clear();
        SanpName = null;
    }
    public void drawSnapshotCompare()
	{
		CompareInitial();
		GUIStyle background = "AnimationCurveEditorBackground";




		_compareDrawArea.width = this.position.width - PAEditorConst.InspectorWidth;




		_compareDrawArea.height = this.position.height;
		GUILayout.BeginArea(_compareDrawArea,background);
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal ("Toolbar");
		if(GUILayout.Button("Take Snapshot",EditorStyles.toolbarButton))
		{
			UnityEditor.MemoryProfiler.MemorySnapshot.RequestNewSnapshot();

		}
		if(GUILayout.Button("Remove",EditorStyles.toolbarButton))
		{
			foreach(string st in activeUnpackedsnapshots.Keys)
         	{
         		unpackedsnapshots.Remove(st);
         	}

         	activeUnpackedsnapshots.Clear();


		}
		if(GUILayout.Button("Compare",EditorStyles.toolbarButton))
		{
			compareRefreshTables();
			_compare = true;

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

        string enteredsizeString = GUILayout.TextField(_compareSizeString,100,GUILayout.MaxWidth(40));
        GUILayout.Label("mb ");

        if(_compareSizeString != enteredsizeString)
    	{
	    	_compareSizeString = enteredsizeString;
	        compareRefreshTables();
    	}
    	GUILayout.Space(30);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		foreach(string st in unpackedsnapshots.Keys)
        {
         	GUIStyle _buttonStyle = new GUIStyle();
         	if(!activeUnpackedsnapshots.ContainsKey(st))
	        {
	         	_buttonStyle = GUI.skin.button;

	        }
	        else
	        {
	        	_buttonStyle = myskin.button;
	        }


         	if(GUILayout.Button(st,_buttonStyle,GUILayout.MaxWidth(100)))
	        {
	        	if(!activeUnpackedsnapshots.ContainsKey(st))
	        	{
		        	activeUnpackedsnapshots.Add(st,unpackedsnapshots[st]);
		        	_Keys.Add(st);
		        	_compare = false;

		        	
	        	}
	        	else
	        	{
	        		activeUnpackedsnapshots.Remove(st);
	        		_Keys.Remove(st);
	        		_compare = false;
	        	}

	        }
        }






		GUILayout.EndHorizontal();

		






        
        if(_compare && activeUnpackedsnapshots.Count == 2)
        {
        	//compareRefreshTables();
        	float f = 50f;
        	if(commonGroupTable != null )
			commonGroupTable.Draw(new Rect(5,f , _compareDrawArea.width * 0.5f, _compareDrawArea.height*0.3f));

			if (commonItemTable != null )
			{
				commonItemTable.Draw(new Rect(_compareDrawArea.width * 0.51f,f,_compareDrawArea.width * 0.48f, _compareDrawArea.height*0.3f));
			}

			if(diffGroupTable0 != null )
			diffGroupTable0.Draw(new Rect(5, _compareDrawArea.height*0.31f+f, _compareDrawArea.width * 0.5f, _compareDrawArea.height*0.3f));

			if (diffItemTable0 != null )
			{
				diffItemTable0.Draw(new Rect(_compareDrawArea.width * 0.51f,_compareDrawArea.height*0.31f+f,_compareDrawArea.width * 0.48f, _compareDrawArea.height*0.3f));
			}

			if(diffGroupTable1 != null )
			diffGroupTable1.Draw(new Rect(5, _compareDrawArea.height*0.62f+f, _compareDrawArea.width * 0.5f, _compareDrawArea.height*0.3f));

			if (diffItemTable1 != null )
			{
				diffItemTable1.Draw(new Rect(_compareDrawArea.width * 0.51f,_compareDrawArea.height*0.62f+f,_compareDrawArea.width * 0.48f, _compareDrawArea.height-20));
			}

        }
        if(_compare && activeUnpackedsnapshots.Count >2)
        {
        	
        	float f = 50f;
        	if(moreGruopTab != null )
			moreGruopTab.Draw(new Rect(5,f , _compareDrawArea.width * 0.5f, _compareDrawArea.height*0.8f));

			if (moreItemTab != null )
			{
				moreItemTab.Draw(new Rect(_compareDrawArea.width * 0.51f,f,_compareDrawArea.width * 0.48f, _compareDrawArea.height*0.8f));
			}
        }
		


		
		GUILayout.EndVertical();
		GUILayout.EndArea();
		if (_inspector!=null && _unpackedCrawl!=null)
				_inspector.Draw();



	

	}

	





}

}