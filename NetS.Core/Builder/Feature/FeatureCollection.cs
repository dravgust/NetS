using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NetS.Core.Builder.Feature
{
    public class FeatureCollection : IFeatureCollection
    {
        private readonly List<IFeatureRegistration> _featureRegistrations;

        public List<IFeatureRegistration> FeatureRegistrations
        {
            get { return this._featureRegistrations; }
        }

        public FeatureCollection()
        {
            this._featureRegistrations = new List<IFeatureRegistration>();
        }

        public IFeatureRegistration AddFeature<TImplementation>() where TImplementation : class, IApplicationFeature
        {
            if(_featureRegistrations.Any(f => f.FeatureType == typeof(TImplementation)))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Feature of type {0} has already been registered.", typeof(TImplementation).FullName));

            var featureRegistration = new FeatureRegistration<TImplementation>();
            _featureRegistrations.Add(featureRegistration);

            return featureRegistration;
        }
    }
}
