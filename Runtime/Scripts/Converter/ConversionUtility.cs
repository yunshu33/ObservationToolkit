using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Voyage.ObservationToolkit.Runtime.Converter
{
    /// <summary>
    /// 统一类型转换工具。
    /// 常见类型走快速路径，复杂类型才回退到 TypeConverter。
    /// </summary>
    public static class ConversionUtility
    {
        /// <summary>
        /// TypeConverter 缓存，避免高频转换时反复调用 TypeDescriptor.GetConverter。
        /// </summary>
        private static readonly Dictionary<Type, TypeConverter> ConverterCache = new();

        /// <summary>
        /// TypeConverter 缓存锁。绑定通常在主线程使用，但这里保持线程安全。
        /// </summary>
        private static readonly object ConverterCacheLock = new();

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

            if (TryFastConvert(value, actualTargetType, out var fastValue))
            {
                return fastValue;
            }

            if (actualTargetType.IsEnum)
            {
                if (value is string enumText)
                {
                    return Enum.Parse(actualTargetType, enumText, true);
                }

                return Enum.ToObject(actualTargetType, value);
            }

            try
            {
                return System.Convert.ChangeType(value, actualTargetType, CultureInfo.InvariantCulture);
            }
            catch
            {
                var targetConverter = GetConverter(actualTargetType);
                if (targetConverter.CanConvertFrom(sourceType))
                {
                    return targetConverter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
                }

                var sourceConverter = GetConverter(sourceType);
                if (sourceConverter.CanConvertTo(actualTargetType))
                {
                    return sourceConverter.ConvertTo(null, CultureInfo.InvariantCulture, value, actualTargetType);
                }

                throw;
            }
        }

        /// <summary>
        /// 常见类型快速转换。
        /// 这些类型覆盖 UGUI 绑定最常见的 int、float、double、bool、string 场景。
        /// </summary>
        private static bool TryFastConvert(object value, Type targetType, out object result)
        {
            result = null;

            if (targetType == typeof(string))
            {
                result = System.Convert.ToString(value, CultureInfo.InvariantCulture);
                return true;
            }

            if (targetType == typeof(int))
            {
                if (value is float floatValue)
                {
                    result = ToInt(floatValue);
                    return true;
                }

                if (value is double doubleValue)
                {
                    result = ToInt(doubleValue);
                    return true;
                }

                if (value is string stringValue)
                {
                    result = int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue)
                        ? intValue
                        : 0;
                    return true;
                }
            }

            if (targetType == typeof(float))
            {
                if (value is int intValue)
                {
                    result = (float)intValue;
                    return true;
                }

                if (value is double doubleValue)
                {
                    result = (float)doubleValue;
                    return true;
                }

                if (value is string stringValue)
                {
                    result = float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatValue)
                        ? floatValue
                        : 0f;
                    return true;
                }
            }

            if (targetType == typeof(double))
            {
                if (value is int intValue)
                {
                    result = (double)intValue;
                    return true;
                }

                if (value is float floatValue)
                {
                    result = (double)floatValue;
                    return true;
                }

                if (value is string stringValue)
                {
                    result = double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue)
                        ? doubleValue
                        : 0d;
                    return true;
                }
            }

            if (targetType == typeof(bool) && value is string boolText)
            {
                result = bool.TryParse(boolText, out var boolValue) && boolValue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取并缓存 TypeConverter。
        /// </summary>
        private static TypeConverter GetConverter(Type type)
        {
            lock (ConverterCacheLock)
            {
                if (!ConverterCache.TryGetValue(type, out var converter))
                {
                    converter = TypeDescriptor.GetConverter(type);
                    ConverterCache[type] = converter;
                }

                return converter;
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
