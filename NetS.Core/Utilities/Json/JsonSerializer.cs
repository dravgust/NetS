using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NetS.Core.Utilities.Json
{
    public class JsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerSettings _settings;

        public JsonSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
	            MissingMemberHandling = MissingMemberHandling.Ignore,

				ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
		}

        public JsonSerializer(IList<JsonConverter> converters) : this()
        {
            Guard.NotNull(converters, nameof(converters));

            _settings.Converters = converters;
        }

	    /// <inheritdoc />
		public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, _settings);
        }

		/// <inheritdoc />
		public T Deserialize<T>(string json)
        {
			return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
