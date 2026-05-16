namespace VoyageForge.ObservationToolkit.Runtime.Converter
{
    /// <summary>
    /// string 与 int / float / double 的内置转换器。
    /// </summary>
    public class StringConvert : IConvert<string, int>, IConvert<string, float>, IConvert<string, double>
    {
        /// <summary>
        /// 将 object 转换为 string。
        /// </summary>
        string IConvert<string, int>.ObjectConvertSource(object source)
        {
            return ConversionUtility.Convert<string>(source);
        }

        /// <summary>
        /// 将 object 转换为 string。
        /// </summary>
        string IConvert<string, float>.ObjectConvertSource(object source)
        {
            return ConversionUtility.Convert<string>(source);
        }

        /// <summary>
        /// 将 object 转换为 string。
        /// </summary>
        string IConvert<string, double>.ObjectConvertSource(object source)
        {
            return ConversionUtility.Convert<string>(source);
        }

        /// <summary>
        /// 将 string 转换为 int。
        /// </summary>
        int IConvert<string, int>.SourceConvertTarget(string source)
        {
            return ConversionUtility.Convert<int>(source);
        }

        /// <summary>
        /// 将 double 转换回 string。
        /// </summary>
        public string TargetConvertSource(double target)
        {
            return ConversionUtility.Convert<string>(target);
        }

        /// <summary>
        /// 将 float 转换回 string。
        /// </summary>
        public string TargetConvertSource(float target)
        {
            return ConversionUtility.Convert<string>(target);
        }

        /// <summary>
        /// 将 int 转换回 string。
        /// </summary>
        public string TargetConvertSource(int target)
        {
            return ConversionUtility.Convert<string>(target);
        }

        /// <summary>
        /// 将 string 转换为 float。
        /// </summary>
        float IConvert<string, float>.SourceConvertTarget(string source)
        {
            return ConversionUtility.Convert<float>(source);
        }

        /// <summary>
        /// 将 string 转换为 double。
        /// </summary>
        double IConvert<string, double>.SourceConvertTarget(string source)
        {
            return ConversionUtility.Convert<double>(source);
        }
    }
}
