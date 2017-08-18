﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemoryProfilerWindow;
using UnityEngine;

namespace Treemap
{
	public class CompareSnapshot
	{
		List<CrawledMemorySnapshot> _unpackedsnapshots = new List<CrawledMemorySnapshot>();
		string _searchString;
		string _sizeString;

		private float commonTltalSize = 0.0f;
		private float diff0TltalSize = 0.0f;
		private float diff1TltalSize = 0.0f;


		public List<Group> gruopIn0NotIn1 = new List<Group>();
		public List<Group> gruopIn1NotIn0 = new List<Group>();
		public List<Group> commonGroup = new List<Group>();

		private Dictionary<string,Group> gruopDiff0= new Dictionary <string,Group>();
		private Dictionary<string,Group> gruopDiff1= new Dictionary <string,Group>();
		private Dictionary<string,Group> gruopCommon= new Dictionary <string,Group>();


		private Dictionary<string,Item> itemsDiff0= new Dictionary <string,Item>();
		private Dictionary<string,Item> itemsDiff1= new Dictionary <string,Item>();
		private Dictionary<string,Item> itemsCommon= new Dictionary <string,Item>();





		public CompareSnapshot(List<CrawledMemorySnapshot> snapshots,string searchString, string sizeString)
		{
			_unpackedsnapshots = snapshots;
			_searchString = searchString;
			_sizeString = sizeString;
		}

		public void Compare()
		{
			if(_unpackedsnapshots.Count>=2)
			{
Debug.Log("_unpackedsnapshots.Count"+_unpackedsnapshots.Count);
				GetDataFromSnapShot _snapshot0 = new GetDataFromSnapShot(_unpackedsnapshots[0],_searchString,_sizeString);
				GetDataFromSnapShot _snapshot1 = new GetDataFromSnapShot(_unpackedsnapshots[1],_searchString,_sizeString);
				_snapshot0.GetCompleteData();
				_snapshot1.GetCompleteData();

Debug.Log ("love you  _snapshot0.AllItems.Count"+_snapshot0.AllItems.Count);
				foreach(Item item in _snapshot0.AllItems.Values)
				{
					if (_snapshot1.AllItems.ContainsKey(item.name))
					{
						if(!gruopCommon.ContainsKey(item._group._name))
						{
							Group newGroup = new Group();
							newGroup._name = item._group._name;
							newGroup._items = new List<Item>();
							gruopCommon.Add(newGroup._name,newGroup);
						}
						gruopCommon[item._group._name]._items.Add(item);

						itemsCommon.Add(item.name,item);
					}

				}

				foreach(Item item in _snapshot0.AllItems.Values)
				{
					if(!itemsCommon.ContainsKey(item.name))
					{
						itemsDiff0.Add(item.name,item);
						if(!gruopDiff0.ContainsKey(item._group._name))
						{
							Group newGroup = new Group();
							newGroup._name = item._group._name;
							newGroup._items = new List<Item>();
							gruopDiff0.Add(newGroup._name,newGroup);
						}
						gruopDiff0[item._group._name]._items.Add(item);
						
					}
				}


				foreach(Item item in _snapshot1.AllItems.Values)
				{
					if(!itemsCommon.ContainsKey(item.name))
					{
						itemsDiff1.Add(item.name,item);
						if(!gruopDiff1.ContainsKey(item._group._name))
						{
							Group newGroup = new Group();
							newGroup._name = item._group._name;
							newGroup._items = new List<Item>();
							gruopDiff1.Add(newGroup._name,newGroup);
						}
						gruopDiff1[item._group._name]._items.Add(item);
						
					}
				}

				

				UpdateTheGruopInf(gruopDiff0,diff0TltalSize,gruopIn0NotIn1);
				UpdateTheGruopInf(gruopDiff1,diff1TltalSize,gruopIn1NotIn0);
				UpdateTheGruopInf(gruopCommon,commonTltalSize,commonGroup);
			}







		}



		void UpdateTheGruopInf(Dictionary<string,Group> _groups,float memTltalSize,List<Group> gruopIn)
		{
			foreach (Group group in _groups.Values)
			{
				group._items.Sort();
				
				memTltalSize += group.totalMemorySize;
				//Debug.Log(group._name);
			}

			foreach (Group group in _groups.Values)
			{

				group._membCount = group._items.Count;
				group._Size = group.totalMemorySize;
				group._Percent = (float)Math.Round(100*group.totalMemorySize/memTltalSize,2);
				gruopIn.Add(group);

				
			}

		}


	}






}