using MvvmToolkit.Converter;

namespace MvvmToolkit
{
    public interface IOneWayBinder<S, out SProperty, in TProperty> 
    {
        void OneWay();
        
        
        void OneWay(IConvert<SProperty, TProperty> convert);
        
    }
}