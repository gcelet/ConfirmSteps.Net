namespace ConfirmSteps.Net.Tests.Sql;

using System.Collections;
using System.Data.Common;

/// <summary>
/// A bare <see cref="DbParameterCollection"/> backed by a plain list, paired with <see cref="FakeDbParameter"/>
/// and <see cref="FakeDbCommand"/>.
/// </summary>
internal sealed class FakeDbParameterCollection : DbParameterCollection
{
    private readonly List<DbParameter> parameters = new();

    public override int Count => parameters.Count;

    public override object SyncRoot => this;

    public override int Add(object value)
    {
        parameters.Add((DbParameter)value);

        return parameters.Count - 1;
    }

    public override void AddRange(Array values)
    {
        foreach (object value in values)
        {
            Add(value);
        }
    }

    public override void Clear()
    {
        parameters.Clear();
    }

    public override bool Contains(object value)
    {
        return parameters.Contains((DbParameter)value);
    }

    public override bool Contains(string value)
    {
        return IndexOf(value) >= 0;
    }

    public override void CopyTo(Array array, int index)
    {
        ((ICollection)parameters).CopyTo(array, index);
    }

    public override IEnumerator GetEnumerator()
    {
        return parameters.GetEnumerator();
    }

    public override int IndexOf(object value)
    {
        return parameters.IndexOf((DbParameter)value);
    }

    public override int IndexOf(string parameterName)
    {
        return parameters.FindIndex(p => p.ParameterName == parameterName);
    }

    public override void Insert(int index, object value)
    {
        parameters.Insert(index, (DbParameter)value);
    }

    public override void Remove(object value)
    {
        parameters.Remove((DbParameter)value);
    }

    public override void RemoveAt(int index)
    {
        parameters.RemoveAt(index);
    }

    public override void RemoveAt(string parameterName)
    {
        RemoveAt(IndexOf(parameterName));
    }

    protected override DbParameter GetParameter(int index)
    {
        return parameters[index];
    }

    protected override DbParameter GetParameter(string parameterName)
    {
        return parameters[IndexOf(parameterName)];
    }

    protected override void SetParameter(int index, DbParameter value)
    {
        parameters[index] = value;
    }

    protected override void SetParameter(string parameterName, DbParameter value)
    {
        parameters[IndexOf(parameterName)] = value;
    }
}
