using System;
using System.ComponentModel;
using System.Globalization;

namespace Voyage.ObservationToolkit.Runtime.Converter
{
    /// <summary>
    /// 统一类型转换工具。
    /// </summary>
    public static class ConversionUtility
    {
        /// <summary>
        /// 将任意值转换为目标泛型类型。
        /// </summary>
        public static TTarget Convert<TTarget>(object value)
        {
            return (TTarget)Convert(value, typeof(TTarget));
        }

        /// <summary>
        /// 将任意值转换为指定目标类型。
        /// </summary>
        public static object Convert(object value, Type targetType)
        {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));

            var nullableType = Nullable.GetUnderlyingType(targetType);
            var actualTargetType = nullableType ?? targetType;

            if (value == null)
            {
                return targetType.IsValueType && nullableType == null ? Activator.CreateInstance(targetType) : null;
            }

            var sourceType = value.GetType();
            if (actualTargetType.IsAssignableFrom(sourceType))
            {
                return value;
            }

            if (actualTargetType.IsEnum)
            {
                if (value is string enumText)
                {
                    return Enum.Parse(actualTargetType, enumText, true);
                }

                return Enum.ToObject(actualTargetType, value);
            }

            if (actualTargetType == typeof(string))
            {
                return System.Convert.ToString(value, CultureInfo.InvariantCulture);
            }

            if (actualTargetType == typeof(int))
            {
                if (value is float floatValue)
                {
                    return ToInt(floatValue);
                }

                if (value is double doubleValue)
                {
                    return ToInt(doubleValue);
                }
            }

            try
            {
                return System.Convert.ChangeType(value, actualTargetType, CultureInfo.InvariantCulture);
            }
            catch
            {
                var targetConverter = TypeDescriptor.GetConverter(actualTargetType);
                if (targetConverter.CanConvertFrom(sourceType))
                {
                    return targetConverter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
                }

                var sourceConverter = TypeDescriptor.GetConverter(sourceType);
                if (sourceConverter.CanConvertTo(actualTargetType))
                {
                    return sourceConverter.ConvertTo(null, CultureInfo.InvariantCulture, value, actualTargetType);
                }

                throw;
            }
        }

        /// <summary>
        /// 将 float 转为 int，使用 AwayFromZero，避免 System.Convert.ToInt32 的银行家舍入造成 Slider 体验不一致。
        /// </summary>
        public static int ToInt(float value)
        {
            return (int)Math.Round(value, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 将 double 转为 int，使用 AwayFromZero。
        /// </summary>
        public static int ToInt(double value)
        {
            return (int)Math.Round(value, MidpointRounding.AwayFromZero);
        }
    }
}
