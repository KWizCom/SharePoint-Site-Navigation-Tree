using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebPartPages;

namespace KWizCom.Sharepoint.WebParts.SiteNavigationTree
{
	/// <summary>
	/// Description of the toolpart. Override the GetToolParts method in your WebPart
	/// class to invoke this toolpart. To establish a reference to the Web Part 
	/// the user has selected, use the ParentToolPane.SelectedWebPart property.
	/// </summary>
	public class KWizComToolPart: Microsoft.SharePoint.WebPartPages.ToolPart
	{
		Image kWizComImage;
		/// <summary>
		/// Constructor for the class.
		/// </summary>
		public KWizComToolPart()
		{
			   this.Title = "KWizCom";
		}

		protected override void CreateChildControls()
		{
			kWizComImage = new Image();
			kWizComImage.BorderWidth = new Unit(0);
			kWizComImage.ToolTip = "KWizCom, Knowledge Worker Components";
			kWizComImage.AlternateText = "KWizCom, Knowledge Worker Components";
			kWizComImage.ImageUrl = Page.Request.Url.ToString().Split('?')[0] + "?SiteNavigationTree=logoKWizCom.gif";
			this.Controls.Add(kWizComImage);

			this.Controls.Add(new LiteralControl("<br><span class='ms-descriptiontext'>Product Version: "+Constants.Version+"</span>"));
		}


		///	<summary>
		///	This method is called by the ToolPane object to apply property changes to the selected Web Part. 
		///	</summary>
		public override void ApplyChanges()
		{
			// Apply property values here.
		}

		/// <summary>
		///	If the ApplyChanges method succeeds, this method is called by the ToolPane object
		///	to refresh the specified property values in the toolpart user interface.
		/// </summary>
		public override void SyncChanges()
		{
			// Sync with the new property changes here.
		}
		
		/// <summary>
		///	This method is called by the ToolPane object if the user discards changes to the selected Web Part. 
		/// </summary>
		public override void CancelChanges()
		{
		}

		/// <summary>
		/// Render this tool part to the output parameter specified.
		/// </summary>
		/// <param name="output">The HTML writer to write out to </param>
		protected override void RenderToolPart(HtmlTextWriter output)
		{
			this.EnsureChildControls();
			this.RenderChildren(output);
		}
	}											
}
								
