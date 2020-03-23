using System.Collections.Generic;

namespace NetS.Core.Builder.Feature
{
    public interface IFeatureCollection
    {
        List<IFeatureRegistration> FeatureRegistrations { get; }

        IFeatureRegistration AddFeature<TImplementation>() where TImplementation : class, IApplicationFeature;
    }
}
