namespace MvvmToolkit.Converter
{
    public class IntConvert : IConvert<int, string>, IConvert<int, float>, IConvert<int, double>
    {
        int IConvert<int, string>.Convert(object source)
        {
            throw new System.NotImplementedException();
        }

        int IConvert<int, float>.Convert(object source)
        {
            throw new System.NotImplementedException();
        }

        int IConvert<int, double>.Convert(object source)
        {
            throw new System.NotImplementedException();
        }

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