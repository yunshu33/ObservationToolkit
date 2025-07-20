namespace MvvmToolkit.Converter
{
    public class IntConvert : IConvert<int, string>, IConvert<int, float>, IConvert<int, double>
    {
        string IConvert<int, string>.Convert(int source)
        {
            return source.ToString();
        }

        float IConvert<int, float>.Convert(int source)
        {
            return source;
        }

        double IConvert<int, double>.Convert(int source)
        {
            return source;
        }
    }
}