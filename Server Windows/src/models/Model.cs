using Newtonsoft.Json;

namespace Server_Windows.src.models
{
	abstract class Model
	{
		public string Serialize() => JsonConvert.SerializeObject(this);
	}
}
