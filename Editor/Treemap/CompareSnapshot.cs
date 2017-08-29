using System;
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

		public List<Group> gruopIn0NotIn1 = new List<Group>();
		public List<Group> gruopIn1NotIn0 = new List<Group>();
		public List<Group> commonGroup = new List<Group>();
		public List<Group> moreCommonGroup = new List<Group>();

		private Dictionary<string,Group> gruopDiff0= new Dictionary <string,Group>();
		private Dictionary<string,Group> gruopDiff1= new Dictionary <string,Group>();
		private Dictionary<string,Group> gruopCommon= new Dictionary <string,Group>();

		private Dictionary<string,Group> moreGroup = new Dictionary <string,Group>();


		private Dictionary<string,Item> itemsDiff0= new Dictionary <string,Item>();
		private Dictionary<string,Item> itemsDiff1= new Dictionary <string,Item>();
		private List<Item> itemsCommon= new List<Item>();
		private List<Item> moreItems= new List <Item>();





		public CompareSnapshot(List<CrawledMemorySnapshot> snapshots,string searchString, string sizeString)
		{
			_unpackedsnapshots = snapshots;
			_searchString = searchString;
			_sizeString = sizeString;
		}

		public void Compare()
		{
			if(_unpackedsnapshots.Count==2)
			{
Debug.Log("_unpackedsnapshots.Count"+_unpackedsnapshots.Count);
				GetDataFromSnapShot _snapshot0 = new GetDataFromSnapShot(_unpackedsnapshots[0],_searchString,_sizeString);
				GetDataFromSnapShot _snapshot1 = new GetDataFromSnapShot(_unpackedsnapshots[1],_searchString,_sizeString);
				_snapshot0.GetCompleteData();
				_snapshot1.GetCompleteData();

Debug.Log ("love you  _snapshot0.AllItems.Count"+_snapshot0.AllItems.Count);
                int Count0 = _snapshot0.AllItems.Count;
                int Count1 = _snapshot1.AllItems.Count;
				for(int i=0; i<Count0; i++)
				{

					for(int j=0; j<Count1; j++)
					{
						if(_snapshot0.AllItems[i].IsEqual(_snapshot1.AllItems[j]))
						{
							itemsCommon.Add(_snapshot0.AllItems[i]);
							_snapshot0.AllItems.Remove(_snapshot0.AllItems[i]);
							_snapshot1.AllItems.Remove(_snapshot1.AllItems[j]);
							j--;
							i--;
							Count1--;
							Count0--;
							break;

						}
						

					}

				}
				GetGroupAndItemResult(_snapshot0.AllItems,gruopDiff0);
				GetGroupAndItemResult(_snapshot1.AllItems,gruopDiff1);
				GetGroupAndItemResult(itemsCommon,gruopCommon);
				UpdateTheGruopInf(gruopDiff0,gruopIn0NotIn1);
				UpdateTheGruopInf(gruopDiff1,gruopIn1NotIn0);
				UpdateTheGruopInf(gruopCommon,commonGroup);
			}
			if(_unpackedsnapshots.Count>2)
			{
				int snapshotNum = _unpackedsnapshots.Count;
				GetDataFromSnapShot []  dataArray = new GetDataFromSnapShot[snapshotNum];
				for(int i=0; i<snapshotNum; i++)
				{
					dataArray[i] = new GetDataFromSnapShot(_unpackedsnapshots[i],_searchString,_sizeString);
					dataArray[i].GetCompleteData();
				}
				moreItems = dataArray[0].AllItems;



				for(int i=0; i<snapshotNum && moreItems.Count>= 0; i++)
				{
					moreItems = compareItems(moreItems,dataArray[i].AllItems);
				}
				GetGroupAndItemResult(moreItems,moreGroup);
				UpdateTheGruopInf(moreGroup,moreCommonGroup);
			}







		}

		void GetGroupAndItemResult(List<Item>_Items, Dictionary<string,Group> _Groups)
		{
			foreach(Item item in _Items)
			{
				
					//itemsDiff0.Add(item.name,item);
				if(!_Groups.ContainsKey(item._group._name))
				{
					Group newGroup = new Group();
					newGroup._name = item._group._name;
					newGroup._items = new List<Item>();
					_Groups.Add(newGroup._name,newGroup);
				}
				_Groups[item._group._name]._items.Add(item);
					
				
			}

		}

		List<Item> compareItems(List<Item> Items0, List<Item> Items1)
		{
			List<Item> _Ite0 = new List<Item>();
			foreach(Item ite in Items0)
			{
				foreach(Item it in Items1)
				{
					if(ite.IsEqual(it))
					{
						_Ite0.Add(ite);

						break;

					}
				}
			}
			return _Ite0;

		}



		void UpdateTheGruopInf(Dictionary<string,Group> _groups,List<Group> gruopIn)
		{
			float memTltalSize = 0f;
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