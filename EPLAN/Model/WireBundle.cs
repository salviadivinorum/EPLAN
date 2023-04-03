using System.Collections.Generic;

namespace EPLAN.Model
{
	/// <summary>
	/// Wire bundle structure
	/// </summary>
	public class WireBundle
	{
		/// <summary>
		/// All wires (as circles) inside the Wire bundle (incleding the Wire bundle circle)
		/// </summary>
		public List<Circle> Circles { get; set; } = new List<Circle>();
	}
}
