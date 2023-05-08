namespace ConfirmSteps.Internal;

using System.Linq.Expressions;
using System.Reflection;

internal static class ReflectionHelper
{
    internal static void SetProperty<T>(Expression<Func<T, object>> property, T data, object? value)
    {
        // TODO: See to improve robutness of this method
        LambdaExpression expression = property;
        MemberExpression? memberExpression = expression.Body is UnaryExpression unaryExpression
            ? unaryExpression.Operand as MemberExpression
            : expression.Body as MemberExpression;
        string propertyName = memberExpression!.Member.Name;
        PropertyInfo? propertyInfo = typeof(T).GetProperty(propertyName);
        Type propertyValueType = Nullable.GetUnderlyingType(propertyInfo!.PropertyType) ?? propertyInfo.PropertyType;

        if (value != null)
        {
            value = Convert.ChangeType(value, propertyValueType);
        }

        propertyInfo!.SetValue(data, value);
    }
}