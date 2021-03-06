using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemoryProfilerWindow;
using Treemap;
using UnityEditor.Graphs;
using UnityEngine;

namespace Treemap
{
	public class Item : IComparable<Item>
	{
		public Group _group;
		public Rect _position;
		public int _index;

		public ThingInMemory _thingInMemory;

		public float memorySize;
		//{ get { return _thingInMemory.size; } }
		public string name ;
		//{ get { return _thingInMemory.caption; } }
		public Color color { get { return _group.color; } }

		public Item(ThingInMemory thingInMemory, Group group)
		{
			_thingInMemory = thingInMemory;
			_group = group;
			memorySize = _thingInMemory.size;
			name = _thingInMemory.caption;
		}

		public int CompareTo(Item other)
		{
			return (int)(_group != other._group ? other._group.totalMemorySize - _group.totalMemorySize : other.memorySize - memorySize);
		}

		public bool IsEqual(Item other)
		{
			return this.name.Equals(other.name);
			//this._group._name.Equals(other._group._name) && 

		}
	}
}
