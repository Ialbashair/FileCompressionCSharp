using DataObjects;
using System;
using System.Collections.Generic;
using System.IO;

namespace LogicLayer
{
    public class Utils
    {
        // Simple custom made priority queue implementation - Used because PriorityQueue is not available in C# 7.3
        public class SimplePriorityQueue<T>
        {
            private readonly List<Tuple<T, int>> _elements = new List<Tuple<T, int>>();

            public int Count
            {
                get { return _elements.Count; }
            }

            public void Enqueue(T item, int priority)
            {
                _elements.Add(Tuple.Create(item, priority));
            }

            public bool TryDequeue(out T item, out int priority)
            {
                if (_elements.Count == 0)
                {
                    item = default(T);
                    priority = default(int);
                    return false;
                }

                int bestIndex = 0;
                for (int i = 1; i < _elements.Count; i++)
                {
                    if (_elements[i].Item2 < _elements[bestIndex].Item2)
                        bestIndex = i;
                }

                var bestElement = _elements[bestIndex];
                item = bestElement.Item1;
                priority = bestElement.Item2;
                _elements.RemoveAt(bestIndex);
                return true;
            }
        }
    }
}
