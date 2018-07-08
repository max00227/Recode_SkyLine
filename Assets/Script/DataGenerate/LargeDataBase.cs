namespace model.data
{
	public partial class LargeDataBase : IdentifiableEntity
    {
		 System.Int32 _id;

		public System.Int32 id { get { return this._id; } set { this._id = value; } }


		public System.String name { get; set;}
		public System.String detail { get; set;}
    }
}
