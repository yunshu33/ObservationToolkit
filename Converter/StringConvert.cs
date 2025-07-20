namespace MvvmToolkit.Converter
{
    public class StringConvert : IConvert<string, int>, IConvert<string, float>, IConvert<string, double>
    {
        int IConvert<string, int>.Convert(string source)
        {
            return int.Parse(source);
        }

        float IConvert<string, float>.Convert(string source)
        {
            return float.Parse(source);
        }

        double IConvert<string, double>.Convert(string source)
        {
            return double.Parse(source);
        }
    }
}