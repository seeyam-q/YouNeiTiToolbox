using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FortySevenE
{
    public static class DictionaryExtensions
    {
		public static void CreateNewOrUpdateExisting<TKey, TValue>( this IDictionary<TKey, TValue> map, TKey key, TValue value)
		{
			map[key] = value;
		}
	}
}
