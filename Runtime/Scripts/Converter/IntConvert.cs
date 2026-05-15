namespace Voyage.ObservationToolkit.Runtime.Converter
{
    /// <summary>
    /// int 与 string / float / double 的内置转换器。
    /// </summary>
    public class IntConvert : IConvert<int, string>, IConvert<int, float>, IConvert<int, double>
    {
        /// <summary>
        /// 将 object 转换为 int。
        /// </summary>
        int IConvert<int, string>.ObjectConvertSource(object source)
        {
            return ConversionUtility.Convert<int>(source);
        }

        /// <summary>
        /// 将 object 转换为 int。
        /// </summary>
        int IConvert<int, float>.ObjectConvertSource(object source)
        {
            return ConversionUtility.Convert<int>(source);
        }

        /// <summary>
        /// 将 object 转换为 int。
        /// </summary>
        int IConvert<int, double>.ObjectConvertSource(object source)
        {
            return ConversionUtility.Convert<int>(source);
        }

        /// <summary>
        /// 将 int 转换为 string。
        /// </summary>
        string IConvert<int, string>.SourceConvertTarget(int source)
        {
            return ConversionUtility.Convert<string>(source);
        }

        /// <summary>
        /// 将 double 转换回 int。
        /// </summary>
        public int TargetConvertSource(double target)
        {
            return ConversionUtility.ToInt(target);
        }

        /// <summary>
        /// 将 float 转换回 int。
        /// </summary>
        public int TargetConvertSource(float target)
        {
            return ConversionUtility.ToInt(target);
        }

        /// <summary>
        /// 将 string 转换回 int。
        /// </summary>
        public int TargetConvertSource(string target)
        {
            return ConversionUtility.Convert<int>(target);
        }

        /// <summary>
        /// 将 int 转换为 float。
        /// </summary>
        float IConvert<int, float>.SourceConvertTarget(int source)
        {
            return source;
        }

        /// <summary>
        /// 将 int 转换为 double。
        /// </summary>
        double IConvert<int, double>.SourceConvertTarget(int source)
        {
            return source;
        }
    }
}
