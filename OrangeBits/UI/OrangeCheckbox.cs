using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace OrangeBits.UI
{
	/// <summary>
	/// Checkbox that only allows the ViewModel to set the third state in the Tri checkbox
	/// </summary>
	public class OrangeCheckbox : CheckBox
	{
		//--------------------------------------------------------------------------
		//
		//	Overridden Methods
		//
		//--------------------------------------------------------------------------

		#region OnToggle
		/// <summary>
		/// only allow the checkbox to be checked or unchecked via 
		/// </summary>
		protected override void OnToggle()
		{
			if (!this.IsChecked.HasValue || this.IsChecked.Value)
			{
				this.IsChecked = false;
			}
			else
			{
				this.IsChecked = true;
			}
		}
		#endregion
	}
}
