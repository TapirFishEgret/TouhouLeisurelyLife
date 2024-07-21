using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace THLL.BaseSystem
{
    /// <summary>
    /// 循环缓冲区，主要用于构建比如历史记录等内容。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CircularBuffer<T> : ICollection<T>
    {
        #region 循环缓冲区数据
        //存储数据的数组
        private T[] _buffer;
        //指向队列头部的索引
        private int _head;
        //指向队列尾部的索引
        private int _tail;
        //当前队列中元素的数量
        private int _count;
        //队列的容量
        private int _capacity;
        #endregion

        #region 用于自身的方法
        //构造函数
        public CircularBuffer(int capacity)
        {
            //检测容量是否合理
            if (capacity < 1)
            {
                throw new ArgumentException("循环缓冲区容量必须大于0");
            }
            _capacity = capacity;
            _buffer = new T[capacity];
            _head = 0;
            _tail = 0;
            _count = 0;
        }
        //重新设置大小
        public void Resize(int newCapacity)
        {
            //新的用于容纳的数组
            T[] newBuffer = new T[newCapacity];
            //起始索引，选取新容量与当前元素数量中较小的一个，保证数据不会丢失
            int startIndex = (_tail - Mathf.Min(_count, newCapacity) + _capacity) % _capacity;
            //检测索引
            if (startIndex < 0) startIndex += _capacity;
            //重调大小，复制元素
            for (int i = 0; i < Mathf.Min(_count, newCapacity); i++)
            {
                newBuffer[i] = _buffer[(startIndex + 1) % _capacity];
            }
            //设置新元素
            _buffer = newBuffer;
            _capacity = newCapacity;
            _head = 0;
            _tail = (_count >= newCapacity) ? newCapacity : _count;
            _count = Mathf.Min(_count, newCapacity);
        }
        #endregion

        #region 用于外部的方法
        //添加
        public void Add(T item)
        {
            //尾部元素指定为添加的元素
            _buffer[_tail] = item;
            //尾部索引自增
            _tail = (_tail + 1) % _capacity;
            //判断是否到达容量
            if (_count == _capacity)
            {
                //若已达到容量，则开始覆盖旧数据，同时头部索引开始后移
                _head = (_head + 1) % _capacity;
            }
            else
            {
                //若没有，正常让元素个数自增
                _count++;
            }
        }
        //移除，不过不支持此操作，只是为了实现ICollection接口
        public bool Remove(T item)
        {
            throw new NotSupportedException("循环缓冲区不实现移除操作");
        }
        //清除
        public void Clear()
        {
            //头尾索引归零，元素个数也归零
            _head = 0;
            _tail = 0;
            _count = 0;
        }
        //判断缓冲区是否包含特定元素
        public bool Contains(T item)
        {
            //索引，从头部索引开始
            int index = _head;
            //循环
            for (int i = 0; i < _count; i++)
            {
                //判断
                if (EqualityComparer<T>.Default.Equals(_buffer[index], item))
                {
                    return true;
                }
                //若不等，索引自增
                index = (index + 1) % _capacity;
            }
            //若无结果，返回空
            return false;
        }
        //将缓冲区内容复制到另一个数组中
        public void CopyTo(T[] array, int arrayIndex)
        {
            //检测数组是否为空
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            //检测数组索引是否合理
            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            //计算要拷贝的元素的数量
            int numElementsToCopy = Mathf.Min(_count, array.Length - arrayIndex);
            //当前索引
            int currentIndex = (_tail - numElementsToCopy + _capacity) % _capacity;
            //拷贝
            for (int i = 0; i < numElementsToCopy; i++)
            {
                array[arrayIndex++] = _buffer[currentIndex];
                currentIndex = (currentIndex + 1) % _capacity;
            }
        }
        //Count
        public int Count => _count;
        //IsReadonly
        public bool IsReadOnly => false;
        //GetEnumerator
        public IEnumerator<T> GetEnumerator()
        {
            //索引从头部起始
            int currentIndex = _head;
            //循环
            for (int i = 0; i < _count; i++)
            {
                yield return _buffer[currentIndex];
                currentIndex = (currentIndex + 1) % _capacity;
            }
        }
        //IEnumerable.GetEnumerator()
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
