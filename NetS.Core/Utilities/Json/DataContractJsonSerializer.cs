using System.IO;
using System.Text;

namespace NetS.Core.Utilities.Json
{
	public class DataContractJsonSerializer : IJsonSerializer
	{
		/// <inheritdoc />
		public string Serialize(object obj)
		{
			var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
			using (var stream = new MemoryStream())
			{
				serializer.WriteObject(stream, obj);
				return Encoding.Default.GetString(stream.ToArray());
			}
		}

		/// <inheritdoc />
		public T Deserialize<T>(string json)
		{
			using (var stream = new MemoryStream(Encoding.Default.GetBytes(json)))
			{
				var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
				return (T)serializer.ReadObject(stream);
			}
		}
	}
}
