using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

using DaggerLib.UI;

namespace DaggerLib.Core
{
    public class DaggerPinCollection<T> :
            IList<T>, IList,
            ICollection<T>, ICollection,
            IEnumerable<T>, IEnumerable
        where T : DaggerBasePin
    {

        // DaggerNode containing these pins
        internal DaggerNode _parentNode;

        // DaggerGraph containing these pins (_parentNode and _parentGraph are mutually exclusive)
        internal DaggerGraph _parentGraph;

        public event DaggerPinAdded PinAdded;
        public event DaggerPinRemoved PinRemoved;

        private List<T> innerList;
        private bool sorted = true;

        public DaggerPinCollection(DaggerNode parentNode)
        {
            _parentNode = parentNode;
            innerList = new List<T>();
        }

        public DaggerPinCollection(DaggerGraph parentGraph)
        {
            _parentGraph = parentGraph;
            innerList = new List<T>();
        }

        /// <summary>
        /// Gets a list of all pins that have a connection
        /// </summary>
        public List<T> ConnectedPins
        {
            get
            {
                List<T> pins = new List<T>();

                foreach (T pin in innerList)
                {
                    if ((pin as DaggerBasePin).IsConnected)
                    {
                        pins.Add(pin);
                    }
                }

                return pins;
            }
        }

        /// <summary>
        /// Get a pin by it's name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public T this[string name]
        {
            get
            {
                T outpin = default(T);

                foreach (T pin in innerList)
                {
                    if (name == (pin as DaggerBasePin).Name)
                    {
                        outpin = pin;
                        break;
                    }
                }

                return outpin;
            }
        }

        /// <summary>
        /// Get a pin by it's guid
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal T this[Guid guid]
        {
            get
            {
                T outpin = default(T);

                foreach (T pin in innerList)
                {
                    if (guid == (pin as DaggerBasePin).InstanceGuid)
                    {
                        outpin = pin;
                        break;
                    }
                }

                return outpin;
            }
        }

        /// <summary>
        /// Get list of pins available based on context of Mutex Group and connection status
        /// </summary>
        public List<T> MutexAvailablePins
        {
            get
            {
                List<T> outlist = new List<T>();
                foreach (T pin in innerList)
                {
                    if ((pin as DaggerBasePin).MutexAvailable)
                    {
                        outlist.Add(pin);
                    }
                }
                return outlist;
            }
        }

        /// <summary>
        /// Gets the list of pins in the collection
        /// </summary>
        public List<T> List
        {
            get
            {
                List<T> outlist = new List<T>();
                foreach (T pin in innerList)
                {
                    outlist.Add(pin);
                }
                return outlist;
            }
        }

        public virtual void Clear()
        {
            if (!this.OnClear()) { return; }
            this.innerList.Clear();
            this.OnClearComplete();
        }

        /// <summary>
        /// Append a numerical value to a pin's name to make it unique from existing names
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string UniqueName(string name)
        {
            string newname = name;
            int count = 2;
            while (this[newname] != null)
            {
                newname = name + " " + count.ToString();
                count++;
            }

            return newname;
        }

        #region Notification Events

        protected virtual bool OnClear()
        {
            return true;
        }

        protected virtual void OnClearComplete()
        {
        }

        protected virtual bool OnInsert(int index, T value)
        {
            if (this[(value as DaggerBasePin).Name] != null)
            {
                (value as DaggerBasePin).Name = UniqueName((value as DaggerBasePin).Name);
            }

            (value as DaggerBasePin)._parentGraph = _parentGraph;
            (value as DaggerBasePin)._parentNode = _parentNode;

            return true;
        }

        protected virtual void OnInsertComplete(
            int index, T value)
        {
            if (PinAdded != null)
            {
                PinAdded(this, value as DaggerBasePin);
            }
        }

        protected virtual bool OnRemove(
            int index, T value)
        {
            (value as DaggerBasePin)._parentGraph = null;
            (value as DaggerBasePin)._parentNode = null;

            return true;
        }

        protected virtual void OnRemoveComplete(
            int index, T value)
        {
            if (PinRemoved != null)
            {
                PinRemoved(this, value as DaggerBasePin);
            }
        }

        protected virtual bool OnSet(
            int index, T oldValue, T value)
        {

            return true;
        }

        protected virtual void OnSetComplete(
            int index, T oldValue, T value)
        {
        }

        protected virtual bool OnValidate(T value)
        {
            return true;
        }
        #endregion

        #region IList<T> Members

        public virtual int IndexOf(T item)
        {
            return innerList.IndexOf(item);
        }

        public virtual void Insert(int index, T item)
        {
            if (!OnValidate(item)) return;
            if (!OnInsert(index, item)) return;
            innerList.Insert(index, item);
            OnInsertComplete(index, item);
        }

        public virtual void RemoveAt(int index)
        {
            T value = innerList[index];

            if (!OnValidate(value)) return;
            if (!OnRemove(index, value)) return;
            innerList.RemoveAt(index);
            OnRemoveComplete(index, value);
        }

        public virtual T this[int index]
        {
            get
            {
                return innerList[index];
            }

            set
            {
                T oldValue = innerList[index];

                if (!OnValidate(value)) return;
                if (!OnSet(index, oldValue, value)) return;
                innerList[index] = value;
                OnSetComplete(index, oldValue, value);
            }
        }

        #endregion

        #region ICollection<T> Members

        public virtual void Add(T item)
        {
            if (!OnValidate(item)) return;
            if (!OnInsert(innerList.Count, item)) return;
            innerList.Add(item);
            OnInsertComplete(innerList.Count - 1, item);
        }

        public virtual bool Contains(T item)
        {
            return innerList.Contains(item);
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        public virtual int Count
        {
            get { return innerList.Count; }
        }

        public virtual bool IsReadOnly
        {
            get { return ((ICollection<T>)innerList).IsReadOnly; }
        }

        public virtual bool Remove(T item)
        {
            int index = innerList.IndexOf(item);

            if (index < 0) return false;

            if (!OnValidate(item)) return false;
            if (!OnRemove(index, item)) return false;
            innerList.Remove(item);

            OnRemoveComplete(index, item);
            return true;
        }
        #endregion

        #region IEnumerable<T> Members

        public virtual IEnumerator<T> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }
        #endregion

        #region IList Members

        public virtual int Add(object value)
        {
            int index = innerList.Count;

            if (!OnValidate((T)value)) return -1;
            if (!OnInsert(index, (T)value)) return -1;

            index = ((IList)innerList).Add(value);
            OnInsertComplete(index, (T)value);

            (value as DaggerBasePin)._parentNode = _parentNode;
            (value as DaggerBasePin)._parentGraph = _parentGraph;

            return index;
        }

        public virtual bool Contains(object value)
        {
            return ((IList)innerList).Contains(value);
        }

        public virtual int IndexOf(object value)
        {
            return ((IList)innerList).IndexOf(value);
        }

        public virtual void Insert(int index, object value)
        {
            if (!OnValidate((T)value)) return;
            if (!OnInsert(index, (T)value)) return;
            ((IList)innerList).Insert(index, value);
            OnInsertComplete(index, (T)value);
        }
        public virtual bool IsFixedSize
        {
            get { return ((IList)innerList).IsFixedSize; }
        }


        public virtual void Remove(object value)
        {
            int index = innerList.IndexOf((T)value);

            if (index < 0) return;

            if (!OnValidate((T)value)) return;
            if (!OnRemove(index, (T)value)) return;
            ((IList)innerList).Remove(value);

            (value as DaggerBasePin)._parentNode = null;
            (value as DaggerBasePin)._parentGraph = null;

            OnRemoveComplete(index, (T)value);
        }

        object IList.this[int index]
        {
            get
            {
                return innerList[index];
            }

            set
            {
                T oldValue = innerList[index];
                if (!OnValidate((T)value)) return;
                if (!OnSet(index, oldValue, (T)value)) return;
                innerList[index] = (T)value;
                OnSetComplete(index, oldValue, (T)value);
            }
        }
        #endregion

        #region ICollection Members

        public virtual void CopyTo(Array array, int index)
        {
            ((ICollection)innerList).CopyTo(array, index);
        }

        public virtual bool IsSynchronized
        {
            get { return ((ICollection)innerList).IsSynchronized; }
        }

        public virtual object SyncRoot
        {
            get { return ((ICollection)innerList).SyncRoot; }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)innerList).GetEnumerator();
        }

        #endregion
    }
}
