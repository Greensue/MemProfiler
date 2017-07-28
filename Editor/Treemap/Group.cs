using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Editor.Treemap;
using MemoryProfilerWindow;
using UnityEngine;


namespace Treemap 
{
	public class Group //: IComparable<Group>
	{
		public string _name;
		public Rect _position;
		public List<Item> _items;
		public string _Size;
		public int _membCount;
		public string _Percent;

		private float _totalMemorySize = -1;

		public float totalMemorySize
		{
			get
			{
				if (_totalMemorySize != -1)
					return _totalMemorySize;

				float result = 0f;
				foreach (Item item in _items)
				{
					result += item.memorySize;
				}
				_totalMemorySize = result;
				return result;
			}
		}

		public int MembCount
		{
			get
			{
			 	return _items.Count;
			}
		}

		

		public float[] memorySizes
		{
			get
			{
				float[] result = new float[_items.Count];
				for (int i = 0; i < _items.Count; i++)
				{
					result[i] = _items[i].memorySize;
				}
				return result;
			}
		}

		public Color color
		{
			get { return Utility.GetColorForName(_name); }
		}

		/*public int CompareTo(Group other)
		{
			return (int)(other.totalMemorySize - totalMemorySize);
		}*/
	}
}
