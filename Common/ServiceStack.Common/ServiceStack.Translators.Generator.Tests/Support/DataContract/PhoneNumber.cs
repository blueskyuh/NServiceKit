using System.Runtime.Serialization;

namespace ServiceStack.Translators.Generator.Tests.Support.DataContract
{
	[TranslateModel(typeof(Model.PhoneNumber))]
	public partial class PhoneNumber
	{
		[DataMember]
		public string Type { get; set; }

		[DataMember]
		public string Number { get; set; }
	}
}