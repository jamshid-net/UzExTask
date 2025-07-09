using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using ProjectTemplate.Shared.Extensions;

namespace ProjectTemplate.Application.Common.QueryFilter;
internal static class QueryFilterExpressionBuilder
{
    public static Expression<Func<T, bool>> BuildPredicate<T>(FilterRequest pageRequest)
    {
        if (!pageRequest.Filter.Any())
        {
            return x => true;
        }

        ParameterExpression param = Expression.Parameter(typeof(T), "x");
        Expression combined = pageRequest.Filter.Select(filter => BuildSinglePredicate<T>(param, filter))
                                                 .Aggregate<Expression, Expression>(Expression.Empty(), (_, predicate) =>
                                                                                    Expression.AndAlso(predicate, predicate));

        return Expression.Lambda<Func<T, bool>>(combined, param);
    }

    private static Expression BuildSinglePredicate<T>(ParameterExpression param, FilterObject filterObject)
    {
        //if (filterObject.Key.Contains(".multiLang", StringComparison.OrdinalIgnoreCase))
        //{
        //    return FilterForMultiLang(param, filterObject);
        //}

        if (filterObject.Key.EndsWith(".from", StringComparison.OrdinalIgnoreCase) ||
            filterObject.Key.EndsWith(".to", StringComparison.OrdinalIgnoreCase))
        {
            return FilterFromTo(param, filterObject);
        }

        MemberExpression member = Expression.Property(param, filterObject.Key);
        object filterValue = filterObject.Value;

        if (filterValue is JsonElement jsonElement)
        {
            filterValue = ConvertJsonElement(jsonElement, member.Type);
        }

        if (member.Type.IsEnum || IsNullableEnum(member.Type))
        {
            return FilterForEnum(filterValue, member);
        }

        if (member.Type == typeof(DateTime) || member.Type == typeof(DateTime?))
        {
            return FilterForDateTime(filterValue, member);
        }

        if (member.Type == typeof(DateOnly) || member.Type == typeof(DateOnly?))
        {
            return FilterForDateOnly(filterValue, member);
        }

        if (member.Type == typeof(string))
        {
            return FilterForString(filterValue, member);
        }

        if (IsNumericType(member.Type))
        {
            return FilterForNumerics(filterValue, member);
        }
        if (IsNumericArrayType(member.Type))
        {
            return FilterForMemberTypeIntArray(filterValue, member);
        }
        if (member.Type == typeof(bool))
        {
            return FilterForBoolean(filterValue, member);
        }

        throw new NotSupportedException($"Filter value type {filterValue.GetType()} is not supported.");
    }

    private static Expression FilterForBoolean(object filterValue, MemberExpression member)
    {
        ConstantExpression constantBoolean = Expression.Constant(filterValue, member.Type);
        return Expression.Equal(member, constantBoolean);
    }

    private static Expression FilterForMemberTypeIntArray(object filterValue, MemberExpression member)
    {
        var intValue = Convert.ToInt32(filterValue);
        var constant = Expression.Constant(intValue, typeof(int));

        MethodInfo containsMethod = typeof(Enumerable)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(int));

        // Build and return the expression: member.Contains(intValue)
        return Expression.Call(containsMethod, member, constant);
    }

    private static Expression FilterForNumerics(object filterValue, MemberExpression member)
    {
        if (filterValue is IEnumerable<object> numericArray)
        {
            var genericListType = typeof(List<>).MakeGenericType(member.Type);
            var numberList = Activator.CreateInstance(genericListType);

            // Get the 'Add' method for List<T>
            var addMethod = genericListType.GetMethod("Add");

            // Populate the list with converted values
            foreach (var value in numericArray)
            {
                var convertedValue = Convert.ChangeType(value, member.Type);
                addMethod?.Invoke(numberList, [convertedValue]);
            }

            // Create an expression that checks if the property is contained in the list
            MethodInfo containsMethod = genericListType.GetMethod("Contains", [member.Type])!;
            var listConstant = Expression.Constant(numberList);
            return Expression.Call(listConstant, containsMethod, member);
        }
        var targetType = Nullable.GetUnderlyingType(member.Type) ?? member.Type;
        var nonArrayFilterValue = Convert.ChangeType(filterValue, targetType);
        ConstantExpression constant = Expression.Constant(nonArrayFilterValue, member.Type);
        return Expression.Equal(member, constant);
    }


    #region FILTER_STRING
    private static Expression FilterForString(object filterValue, MemberExpression member)
    {

        // Check if the filter value is an array of strings
        if (filterValue is IEnumerable<object> stringArray)
        {
            // Convert the array to a List<string>
            var stringList = stringArray
                .Select(value => value is JValue jValue ?
                    jValue.ToString(CultureInfo.InvariantCulture) :
                    value.ToString()).ToList();

            var conditions = stringList.Select(str =>
                CreateLikeOrILikeExpression(member, str ?? string.Empty)
            ).ToList();

            // Combine all LIKE/ILIKE calls with "OR" to simulate "IN" behavior
            Expression combinedOrCondition = conditions.First();
            foreach (var condition in conditions.Skip(1))
            {
                combinedOrCondition = Expression.OrElse(combinedOrCondition, condition);
            }

            return combinedOrCondition;
        }
        else
        {
            // Handle single string value case
            string filterString = filterValue.ToString() ?? string.Empty;

            // Return the LIKE/ILIKE expression
            return CreateLikeOrILikeExpression(member, filterString);
        }
    }

    private static Expression CreateLikeOrILikeExpression(MemberExpression member, string filterString)
    {

        #region SQL_SERVER
        // SQL Server: Use LIKE with ToLower for case-insensitive matching
        MethodInfo toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;

        // Apply ToLower to the member expression (property being filtered)
        var lowerMember = Expression.Call(member, toLowerMethod);

        return Expression.Call(
            lowerMember,                           // Convert property to lowercase
            typeof(string).GetMethod("Contains", [typeof(string)])!,
            Expression.Constant(filterString.ToLower())  // Convert filter string to lowercase
        );
        #endregion
    }
    #endregion
    private static Expression FilterForDateOnly(object filterValue, MemberExpression member)
    {
        if (filterValue is string strFilter)
        {
            // Parse the input string to DateOnly
            DateOnly parsedDateOnly = DateOnly.ParseExact(strFilter, "MM.dd.yyyy", CultureInfo.InvariantCulture);

            // Create a constant expression for the parsed DateOnly
            ConstantExpression constantDateOnly = Expression.Constant(parsedDateOnly, typeof(DateOnly));

            // Check if the member is nullable
            var isNullable = member.Type == typeof(DateOnly?);

            // Handle nullable and non-nullable cases

            if (isNullable)
            {
                // For nullable DateOnly, first check if it has a value
                var hasValueProperty = Expression.Property(member, "HasValue");
                var nullCheck = Expression.Equal(hasValueProperty, Expression.Constant(true));

                // Access the Value property of the nullable DateOnly
                var valueProperty = Expression.Property(member, "Value");

                // Compare the Value property directly with the parsed DateOnly
                var valueComparison = Expression.Equal(valueProperty, constantDateOnly);

                // Combine the null check with the value comparison
                return Expression.AndAlso(nullCheck, valueComparison);
            }
            else
            {
                // For non-nullable DateOnly, directly compare with the parsed DateOnly
                return Expression.Equal(member, constantDateOnly);
            }
        }

        throw new ArgumentException("Filter value must be a string in 'MM.dd.yyyy' format.");
    }

    private static Expression FilterForDateTime(object filterValue, MemberExpression member)
    {
        if (filterValue is string strFilter)
        {
            DateTime parsedDateTime = DateTime.ParseExact(
                strFilter,
                DateFormat.ReadDateFormats,
                null,
                DateTimeStyles.None);

            DateTime rangeStart;
            DateTime rangeEnd;

            if (parsedDateTime is { Hour: 0, Minute: 0, Second: 0 })
            {

                rangeStart = new DateTime(parsedDateTime.Year, parsedDateTime.Month, parsedDateTime.Day, 0, 0, 0);
                rangeEnd = rangeStart.AddDays(1).AddSeconds(-1); // End of the day
            }
            else
            {

                rangeStart = new DateTime(parsedDateTime.Year, parsedDateTime.Month, parsedDateTime.Day, parsedDateTime.Hour, parsedDateTime.Minute, 0);
                rangeEnd = rangeStart.AddSeconds(59); // End of the minute
            }

            var isNullable = member.Type == typeof(DateTime?);

            // Create ConstantExpressions for the start and end of the range
            ConstantExpression constantRangeStart = Expression.Constant(rangeStart, typeof(DateTime));
            ConstantExpression constantRangeEnd = Expression.Constant(rangeEnd, typeof(DateTime));

            // Convert the member to DateTime if it's nullable
            Expression memberExpression = isNullable
                ? Expression.Convert(member, typeof(DateTime))
                : member;

            // Create expressions for comparing member with range start and end
            var greaterThanOrEqual = Expression.GreaterThanOrEqual(memberExpression, constantRangeStart);
            var lessThanOrEqual = Expression.LessThanOrEqual(memberExpression, constantRangeEnd);

            // Combine the two conditions with an AndAlso
            var rangeComparison = Expression.AndAlso(greaterThanOrEqual, lessThanOrEqual);

            // If the member is nullable, handle null cases
            if (isNullable)
            {
                var hasValueProperty = Expression.Property(member, "HasValue");
                var valueProperty = Expression.Property(member, "Value");

                var nullCheck = Expression.NotEqual(hasValueProperty, Expression.Constant(false));
                var valueComparison = Expression.AndAlso(
                    Expression.GreaterThanOrEqual(valueProperty, constantRangeStart),
                    Expression.LessThanOrEqual(valueProperty, constantRangeEnd)
                );

                rangeComparison = Expression.AndAlso(nullCheck, valueComparison);
            }

            return rangeComparison;
        }
        else
        {
            throw new ArgumentException("Filter value must be a string in valid date formats.");
        }
    }

    private static Expression FilterForEnum(object filterValue, MemberExpression member)
    {
        if (filterValue is IEnumerable<object> enumerable)
        {
            var enumValues = enumerable
                .Select(value => Enum.ToObject(member.Type, Convert.ChangeType(value, Enum.GetUnderlyingType(member.Type))))
                .ToArray();

            // Create a typed array of enum values
            var enumTypedArray = Array.CreateInstance(member.Type, enumValues.Length);
            Array.Copy(enumValues, enumTypedArray, enumValues.Length);

            // Build 'Contains' expression for enums
            MethodInfo containsMethod = typeof(Enumerable).GetMethods()
                .Single(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                .MakeGenericMethod(member.Type);

            return Expression.Call(null, containsMethod, Expression.Constant(enumTypedArray), member);
        }


        var enumType = Nullable.GetUnderlyingType(member.Type) ?? member.Type;

        var enumValue = Enum.ToObject(enumType, filterValue);

        ConstantExpression constantEnum = Expression.Constant(enumValue, member.Type);
        return Expression.Equal(member, constantEnum);
    }

    private static Expression FilterFromTo(ParameterExpression param, FilterObject filterObject)
    {
        var propertyKey = filterObject.Key.Split('.')[0];
        var memberExpression = Expression.Property(param, propertyKey);

        ConstantExpression? constantExpression = null;
        if ((memberExpression.Type == typeof(DateOnly) || (memberExpression.Type == typeof(DateOnly?)) ||
             (memberExpression.Type == typeof(DateTime) || (memberExpression.Type == typeof(DateTime?)))))
        {
            constantExpression = DateTimeAndDateOnlyExpression(memberExpression.Type, filterObject.Value);
        }
        if (IsNumericType(memberExpression.Type))
        {
            var targetType = Nullable.GetUnderlyingType(memberExpression.Type) ?? memberExpression.Type;
            var convertedValue = Convert.ChangeType(filterObject.Value, targetType);
            constantExpression = Expression.Constant(convertedValue, memberExpression.Type);
        }


        if (constantExpression == null)
        {
            throw new InvalidOperationException($"Property '{memberExpression.Member.Name}' is not of type for *from and to* filtering.");
        }

        return filterObject.Key.EndsWith(".from", StringComparison.OrdinalIgnoreCase) ?
            // Create expression for GreaterThanOrEqual
            Expression.GreaterThanOrEqual(memberExpression, constantExpression!) :
            // .to case
            // Create expression for LessThanOrEqual
            Expression.LessThanOrEqual(memberExpression, constantExpression!);
    }
    #region FILTER_FOR_MULTI_LANG
    //private static Expression FilterForMultiLang(ParameterExpression param, FilterObject filterObject)
    //{
    //    var propertyKey = filterObject.Key.Split('.')[0];

    //    var mainProperty = Expression.Property(param, propertyKey);

    //    if (mainProperty.Type != typeof(MultiLanguageField))
    //    {
    //        throw new InvalidOperationException($"Property '{propertyKey}' is not of type MultiLanguageField.");
    //    }


    //    var multiLanguageProperties = typeof(MultiLanguageField).GetProperties();
    //    Expression combinedOrExpression = (from property in multiLanguageProperties
    //                                       select Expression.Property(mainProperty, property.Name)
    //            into jsonPropertyAccess
    //                                       let toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!
    //                                       select Expression.Call(jsonPropertyAccess, toLowerMethod)
    //            into lowerProperty
    //                                       let exConstant = Expression.Constant(filterObject.Value.ToString()?.ToLower(), typeof(string))
    //                                       let containsMethod = typeof(string).GetMethod("Contains", [typeof(string)])!
    //                                       select Expression.Call(lowerProperty, containsMethod, exConstant))
    //        .Aggregate<MethodCallExpression, Expression>(null, (current, containsExpression) => current == null
    //            ? containsExpression
    //            : Expression.OrElse(current, containsExpression));


    //    return combinedOrExpression!;
    //}
    #endregion
    //private static object ConvertJsonElement(JsonElement jsonElement, Type targetType)
    //{
    //    Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

    //    return jsonElement.ValueKind switch
    //    {
    //        JsonValueKind.String => underlyingType == typeof(DateOnly)
    //            ? DateOnly.Parse(jsonElement.GetString() ?? string.Empty)
    //            : jsonElement.GetString()!,
    //        JsonValueKind.Number => Type.GetTypeCode(underlyingType) switch
    //        {
    //            TypeCode.Int32 => jsonElement.GetInt32(),
    //            TypeCode.Double => jsonElement.GetDouble(),
    //            TypeCode.Single => jsonElement.GetSingle(),
    //            TypeCode.Decimal => jsonElement.GetDecimal(),
    //            _ => throw new InvalidOperationException($"Cannot convert JSON number to {targetType}."),
    //        },
    //        JsonValueKind.True or JsonValueKind.False => jsonElement.GetBoolean(),
    //        JsonValueKind.Null when Nullable.GetUnderlyingType(targetType) != null => null,
    //        _ => throw new NotSupportedException($"JSON value kind {jsonElement.ValueKind} is not supported."),
    //    };
    //}

    private static object ConvertJsonElement(JsonElement jsonElement, Type targetType)
    {
        // Determine the base type (handle Nullable<T>)
        Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        bool isNullable = Nullable.GetUnderlyingType(targetType) != null;

        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.String:
                if (underlyingType == typeof(DateOnly))
                    return DateOnly.Parse(jsonElement.GetString() ?? throw new InvalidOperationException("Cannot parse null as DateOnly."));
                if (underlyingType == typeof(Guid))
                    return Guid.Parse(jsonElement.GetString() ?? throw new InvalidOperationException("Cannot parse null as Guid."));
                var str = jsonElement.GetString();
                if (str == null && !isNullable && targetType != typeof(string))
                    throw new InvalidOperationException($"Cannot assign null to non-nullable type {targetType}.");
                return str ?? string.Empty;

            case JsonValueKind.Array:
                 var listType = typeof(List<>).MakeGenericType(targetType);
                 var deserialized = JsonSerializer.Deserialize(jsonElement.GetRawText(), listType);
                 return (deserialized as System.Collections.IEnumerable)!.Cast<object>();

            case JsonValueKind.Number:
                return Type.GetTypeCode(underlyingType) switch
                {
                    TypeCode.Int32 => jsonElement.GetInt32(),
                    TypeCode.Double => jsonElement.GetDouble(),
                    TypeCode.Single => jsonElement.GetSingle(),
                    TypeCode.Decimal => jsonElement.GetDecimal(),
                    TypeCode.Int64 => jsonElement.GetInt64(),
                    TypeCode.UInt32 => jsonElement.GetUInt32(),
                    TypeCode.UInt64 => jsonElement.GetUInt64(),
                    _ => throw new InvalidOperationException($"Cannot convert JSON number to {targetType}."),
                };

            case JsonValueKind.True:
            case JsonValueKind.False:
                if (underlyingType != typeof(bool))
                    throw new InvalidOperationException($"Cannot convert JSON boolean to {targetType}.");
                return jsonElement.GetBoolean();

            case JsonValueKind.Null:
                // If not nullable, throw
                if (!isNullable && targetType != typeof(string))
                    throw new InvalidOperationException($"Cannot assign null to non-nullable type {targetType}.");
                return null!;

            default:
                throw new NotSupportedException($"JSON value kind {jsonElement.ValueKind} is not supported.");
        }
    }
    private static bool IsNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(double) || type == typeof(float) || type == typeof(decimal) || type == typeof(int?) || type == typeof(double?);
    }
    private static bool IsNumericArrayType(Type type)
    {
        return type == typeof(int[]) || type == typeof(List<int>) || type == typeof(int?[]) || type == typeof(List<int?>);
    }

    private static ConstantExpression DateTimeAndDateOnlyExpression(Type memberType, object? filterValue)
    {
        string dateOnlyFormat = "MM.dd.yyyy";

        var constant = filterValue switch
        {
            // Handle string input for DateOnly and DateOnly?
            string dateString when memberType == typeof(DateOnly) || memberType == typeof(DateOnly?) =>
                DateOnly.TryParseExact(dateString, dateOnlyFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)
                    ? Expression.Constant(memberType == typeof(DateOnly) ? parsedDate : (DateOnly?)parsedDate, memberType)
                    : throw new InvalidOperationException($"Cannot convert filter value '{filterValue}' to DateOnly. dateString: {dateString}"),

            // Handle string input for DateTime and DateTime?
            string dateString when memberType == typeof(DateTime) || memberType == typeof(DateTime?) =>
                DateTime.TryParseExact(dateString, DateFormat.ReadDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDateTime)
                    ? Expression.Constant(memberType == typeof(DateTime) ? parsedDateTime : (DateTime?)parsedDateTime, memberType)
                    : throw new InvalidOperationException($"Cannot convert filter value '{filterValue}' to DateTime. dateString: {dateString}"),

            // Handle direct DateOnly input
            DateOnly dateOnlyValue when memberType == typeof(DateOnly) || memberType == typeof(DateOnly?) =>
                Expression.Constant(memberType == typeof(DateOnly) ? dateOnlyValue : (DateOnly?)dateOnlyValue, memberType),

            // Handle direct DateTime input
            DateTime dateTimeValue when memberType == typeof(DateTime) || memberType == typeof(DateTime?) =>
                Expression.Constant(memberType == typeof(DateTime) ? dateTimeValue : (DateTime?)dateTimeValue, memberType),

            // Handle unsupported cases
            _ => throw new InvalidOperationException($"Cannot convert filter value '{filterValue}' to a supported date type.")
        };

        return constant;
    }
    private static bool IsNullableEnum(Type type)
    {

        Type? underlyingType = Nullable.GetUnderlyingType(type);
        return underlyingType is { IsEnum: true };
    }

}
