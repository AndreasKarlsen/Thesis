﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evaluation.Common
{
    interface IHashMap<in K,V>
    {
        bool ContainsKey(K key);
        V Get(K key);
        void Add(K key, V value);
        bool AddIfAbsent(K key, V value);
        bool Remove(K k);
        V this[K key] { get; set; }
        int Size { get; }      
    }
}