using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PocketSql
{
    public class EngineParameterCollection : IDataParameterCollection
    {
        private readonly List<IDbDataParameter> parameters = new List<IDbDataParameter>();

        public int Add(object value)
        {
            parameters.Add((IDbDataParameter)value);
            return parameters.Count - 1;
        }

        public bool IsReadOnly => false;
        public bool IsFixedSize => false;
        public bool IsSynchronized => false;
        public int Count => parameters.Count;
        public void Clear() => parameters.Clear();
        public bool Contains(object value) => parameters.Contains(value as IDbDataParameter);
        public bool Contains(string parameterName) => parameters.Any(x => x.ParameterName == parameterName);
        public void Insert(int index, object value) => parameters.Insert(index, value as IDbDataParameter);
        public int IndexOf(object value) => parameters.IndexOf(value as IDbDataParameter);
        public int IndexOf(string parameterName) => parameters.FindIndex(x => x.ParameterName == parameterName);
        public void Remove(object value) => parameters.Remove(value as IDbDataParameter);
        public void RemoveAt(int index) => parameters.RemoveAt(index);
        public void RemoveAt(string parameterName) => parameterName.Remove(IndexOf(parameterName));
        public IEnumerator GetEnumerator() => parameters.GetEnumerator();
        public void CopyTo(Array array, int index) => parameters.CopyTo((IDbDataParameter[])array, index);
        public object SyncRoot { get; } = new object();

        object IList.this[int index]
        {
            get => parameters[index];
            set => parameters[index] = (IDbDataParameter)value;
        }

        object IDataParameterCollection.this[string parameterName]
        {
            get => parameters.First(x => x.ParameterName == parameterName);
            set => parameters[IndexOf(parameterName)] = (IDbDataParameter)value;
        }
    }
}
