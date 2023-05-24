using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Heap
{
    public class PriorityQueue<T>
    {
        private readonly List<T> heap;
        private readonly Comparer<T> comparer;

        public PriorityQueue() : this(Comparer<T>.Default) { }

        public PriorityQueue(Comparer<T> comparer)
        {
            this.heap = new List<T>();
            this.comparer = comparer;
        }

        public PriorityQueue(IEnumerable<T> collection, Comparer<T> comparer) : this(comparer)
        {
            foreach (var item in collection)
            {
                Enqueue(item);
            }
        }

        public int Count => heap.Count;

        public void Enqueue(T item)
        {
            heap.Add(item);
            int currentIndex = heap.Count - 1;

            while (currentIndex > 0)
            {
                int parentIndex = (currentIndex - 1) / 2;

                if (comparer.Compare(heap[parentIndex], item) <= 0)
                {
                    break;
                }

                heap[currentIndex] = heap[parentIndex];
                currentIndex = parentIndex;
            }

            heap[currentIndex] = item;
        }

        public T Dequeue()
        {
            if (heap.Count == 0)
            {
                throw new InvalidOperationException("Queue is empty.");
            }

            T result = heap[0];
            T lastItem = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);

            if (heap.Count == 0)
            {
                return result;
            }

            int currentIndex = 0;
            while (currentIndex < heap.Count / 2)
            {
                int leftChildIndex = 2 * currentIndex + 1;
                int rightChildIndex = 2 * currentIndex + 2;
                int minChildIndex = rightChildIndex < heap.Count && comparer.Compare(heap[rightChildIndex], heap[leftChildIndex]) < 0
                    ? rightChildIndex
                    : leftChildIndex;

                if (comparer.Compare(lastItem, heap[minChildIndex]) <= 0)
                {
                    break;
                }

                heap[currentIndex] = heap[minChildIndex];
                currentIndex = minChildIndex;
            }

            heap[currentIndex] = lastItem;
            return result;
        }

        public T Peek()
        {
            if (heap.Count == 0)
            {
                throw new InvalidOperationException("Queue is empty.");
            }

            return heap[0];
        }

        public bool IsEmpty()
        {
            return heap.Count == 0;
        }

        public void Clear()
        {
            heap.Clear();
        }
    }
}