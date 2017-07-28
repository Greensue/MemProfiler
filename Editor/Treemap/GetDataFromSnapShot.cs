using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Editor.Treemap;
using MemoryProfilerWindow;
using UnityEngine;


namespace Treemap
{
   public class GetDataFromSnapShot
   {
   	    CrawledMemorySnapshot _unpackedCrawl;
   	    private float memTltalSize = 0.0f;
   	    private Dictionary<string, Group> _groups = new Dictionary<string, Group>();
		private List<Item> _items = new List<Item>();
		public List<Group> _group0 = new List<Group>();
		public GetDataFromSnapShot(CrawledMemorySnapshot _unpackedCrawl)
		{
			this._unpackedCrawl = _unpackedCrawl;
		}

		public float MemTotalSize
		{
			get{
				return memTltalSize;
			}

				

		}

		public Dictionary<string, Group> Groups
		{
			get
			{
				return _groups;
			}
		}
		public List<Item> AllItems
		{
			get
			{
				return _items;
			}
		}


		public void GetCompleteData()
		{
			_items.Clear();
			_groups.Clear();
			_group0.Clear();

			foreach (ThingInMemory thingInMemory in _unpackedCrawl.allObjects)
			{
				string groupName = GetGroupName(thingInMemory);
				if (groupName.Length == 0)
					continue;

				if (!_groups.ContainsKey(groupName))
				{
					Group newGroup = new Group();
					newGroup._name = groupName;
					newGroup._items = new List<Item>();
					_groups.Add(groupName, newGroup);
				}

				Item item = new Item(thingInMemory, _groups[groupName]);
				_items.Add(item);
				_groups[groupName]._items.Add(item);
			}

			foreach (Group group in _groups.Values)
			{
				group._items.Sort();
				
				memTltalSize += group.totalMemorySize;
				Debug.Log(group._name);
			}

			foreach (Group group in _groups.Values)
			{

				group._membCount = group._items.Count;
				group._Size = group.totalMemorySize;
				group._Percent = ""+group.totalMemorySize/memTltalSize;
				_group0.Add(group);

				
			}


			_items.Sort();

		}

		public string GetGroupName(ThingInMemory thing)
		{
			if (thing is NativeUnityEngineObject)
				return (thing as NativeUnityEngineObject).className;
			if (thing is ManagedObject)
				return (thing as ManagedObject).typeDescription.name;
			return thing.GetType().FullName;
		}








   }






















}