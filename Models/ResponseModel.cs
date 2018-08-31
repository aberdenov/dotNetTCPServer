namespace Models
{
	public class ResponseModel<Tmodel>
		{
			public string result { get; set; }
			public string description { get; set; }
			public Tmodel data { get; set; }
			
			public ResponseModel(string result, string description, Tmodel data)
			{
				this.result = result;
				this.description = description;
				this.data = data; 
			}

			
		}
}