namespace LJVoyage.ObservationToolkit.Runtime.Converter
{
    public class IntConvert : IConvert<int, string>, IConvert<int, float>, IConvert<int, double>
    {
        int IConvert<int, string>.ObjectConvertSource(object source)
        {
            throw new System.NotImplementedException();
        }

        int IConvert<int, float>.ObjectConvertSource(object source)
        {
            throw new System.NotImplementedException();
        }

        int IConvert<int, double>.ObjectConvertSource(object source)
        {
            throw new System.NotImplementedException();
        }

        string IConvert<int, string>.SourceConvertTarget(int source)
        {
            return source.ToString();
        }

        public int TargetConvertSource(double target)
        {
            throw new System.NotImplementedException();
        }

        public int TargetConvertSource(float target)
        {
            throw new System.NotImplementedException();
        }

        public int TargetConvertSource(string target)
        {
            throw new System.NotImplementedException();
        }

        float IConvert<int, float>.SourceConvertTarget(int source)
        {
            return source;
        }

        double IConvert<int, double>.SourceConvertTarget(int source)
        {
            return source;
        }
    }
}