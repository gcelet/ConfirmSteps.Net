namespace ConfirmSteps.Net.Tests.Sql;

using System.Collections;
using System.Data.Common;

/// <summary>
/// A <see cref="DbDataReader"/> with no columns and no rows, paired with <see cref="FakeDbCommand"/> for tests
/// that only care about the command's parameters, never its result rows.
/// </summary>
internal sealed class FakeDbDataReader : DbDataReader
{
    public override int Depth => 0;

    public override int FieldCount => 0;

    public override bool HasRows => false;

    public override bool IsClosed => false;

    public override int RecordsAffected => -1;

    public override object this[int ordinal] => throw new NotSupportedException();

    public override object this[string name] => throw new NotSupportedException();

    public override bool GetBoolean(int ordinal) => throw new NotSupportedException();

    public override byte GetByte(int ordinal) => throw new NotSupportedException();

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) =>
        throw new NotSupportedException();

    public override char GetChar(int ordinal) => throw new NotSupportedException();

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) =>
        throw new NotSupportedException();

    public override string GetDataTypeName(int ordinal) => throw new NotSupportedException();

    public override DateTime GetDateTime(int ordinal) => throw new NotSupportedException();

    public override decimal GetDecimal(int ordinal) => throw new NotSupportedException();

    public override double GetDouble(int ordinal) => throw new NotSupportedException();

    public override IEnumerator GetEnumerator() => Enumerable.Empty<object>().GetEnumerator();

    public override Type GetFieldType(int ordinal) => throw new NotSupportedException();

    public override float GetFloat(int ordinal) => throw new NotSupportedException();

    public override Guid GetGuid(int ordinal) => throw new NotSupportedException();

    public override short GetInt16(int ordinal) => throw new NotSupportedException();

    public override int GetInt32(int ordinal) => throw new NotSupportedException();

    public override long GetInt64(int ordinal) => throw new NotSupportedException();

    public override string GetName(int ordinal) => throw new NotSupportedException();

    public override int GetOrdinal(string name) => throw new NotSupportedException();

    public override string GetString(int ordinal) => throw new NotSupportedException();

    public override object GetValue(int ordinal) => throw new NotSupportedException();

    public override int GetValues(object[] values) => 0;

    public override bool IsDBNull(int ordinal) => throw new NotSupportedException();

    public override bool NextResult() => false;

    public override bool Read() => false;
}
