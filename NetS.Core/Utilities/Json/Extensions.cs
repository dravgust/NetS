namespace NetS.Core.Utilities.Json
{
    public static class Extensions
    {
        public static string ToJSON(this object obj, IJsonSerializer jsonSerializer = null)
        {
            var serializer = jsonSerializer ?? new JsonSerializer();
            return serializer.Serialize(obj);
        }

        public static T FromJSON<T>(this string json, IJsonSerializer jsonSerializer = null)
        {
	        var serializer = jsonSerializer ?? new JsonSerializer();
			return serializer.Deserialize<T>(json);
        }
    }
}
