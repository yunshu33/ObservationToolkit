using Voyage.ObservationToolkit.Runtime.Converter;

namespace Voyage.ObservationToolkit.Runtime
{
    public interface IOneWayBinder<S, SProperty, TProperty>
    {
        IDisposableBinding OneWay();


        IDisposableBinding OneWay(IConvert<SProperty, TProperty> convert);
        
        
        void Unbind();
    }
}