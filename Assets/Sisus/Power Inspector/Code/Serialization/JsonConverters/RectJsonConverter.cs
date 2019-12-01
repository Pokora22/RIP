using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sisus
{
	public class RectJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == Types.Rect;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var rect = (Rect)value;
			writer.WriteStartObject();
			writer.WritePropertyName("x");
			writer.WriteValue(rect.x);
			writer.WritePropertyName("y");
			writer.WriteValue(rect.y);
			writer.WritePropertyName("width");
			writer.WriteValue(rect.width);
			writer.WritePropertyName("height");
			writer.WriteValue(rect.height);
			writer.WriteEndObject();
		}
		
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var jobject = JObject.Load(reader);
			var rect = default(Rect);
			var enumerator = jobject.GetEnumerator();
			enumerator.MoveNext();
			rect.x = enumerator.Current.Value.Value<float>();
			enumerator.MoveNext();
			rect.y = enumerator.Current.Value.Value<float>();
			enumerator.MoveNext();
			rect.width = enumerator.Current.Value.Value<float>();
			enumerator.MoveNext();
			rect.height = enumerator.Current.Value.Value<float>();
			return rect;
		}
	}
}