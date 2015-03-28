using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Xml;
using System.Xml.Xsl;
using System.Text;
using System.IO;
using System.Collections;
using Microsoft.SharePoint.Utilities;

namespace KWizCom.SiteNavigationTree2013
{
	public enum TreeItemState {Selected, Unselected, Disabled};

	public class TreeItemStyles
	{
		public string SelectedCssName = "TreeItemSelected";
		public string UnSelectedCssName = "TreeItemUnSelected";
		public string DisabledCssName = "TreeItemDisabled";
		public string HoverCssName = "TreeItemHover";
	}


	[DefaultProperty("Text"),
	ToolboxData("<{0}:TreeControl runat=server></{0}:TreeControl>")]
	public class TreeControl : System.Web.UI.WebControls.WebControl, INamingContainer
	{
		#region Constants

		const string TreeXML_Root_TreeRoot = "<TreeRoot />";
		const string TreeXML_Root_TempRoot = "<TempRoot />";
		const string TreeXML_Elm_TreeItem = "TreeItem";
		const string TreeXML_Attribue_Locked = "locked";
		const string TreeXML_Attribue_Title = "Title";
		const string TreeXML_Attribue_Description = "Description";
		const string TreeXML_Attribue_ID = "ID";
		const string TreeXML_Attribue_Icon = "Icon";
		const string TreeXML_Attribue_IsExpanded = "isexpanded";
        const string TreeXML_Attribue_Link = "link";
        const string TreeXML_Attribue_OnClick = "onclick";
        const string TreeXML_Attribue_OnContextMenu = "oncontextmenu";
		const string TreeXML_Attribue_CSSClassName = "CSSClassName";
		const string TreeXML_Attribue_State = "State";
		const string TreeXML_Elm_Dummy = "Dummy";
		const string TreeXML_Attribue_OnLoadChildren = "OnLoadChildren";


		const string Exception_Message_DummyElmCannotHaveSiblings = "Dummy elements cannot have siblings.";
		const string Exception_Message_InvalidXslPath = "Invalid XslFilePath.";
		const string Exception_Message_CannotEditLockedElm = "Locked elements cannot be edited. This item may have a dummy element or is locked for another reason.";
		const string TreeHTML_ClientIDToken = "_ClientID_";
		const string DefaultScriptName = "TreeControlScript";
		const string DefaultStylesName = "TreeControlDefaultStyles";
		const string hdnOpenTreeNodesID = "hdnOpenTreeNodes";
		const string ResponseContentType_Gif = "Content-Type: image/gif";
		const string ResourceGifExtention = ".gif";
		const string StyleDisplay = "display";
		const string StyleDisplay_Value_None = "none";
		const string HtmlElm_DIV = "DIV";
		const string ScriptTag = "<SCRIPT LANGUAGE=\"Javascript\" SRC=\"/_layouts/15/SiteNavigationTree2013/TreeControl.js\"></SCRIPT>";
		const string DefaultStyleCSSTag = "<link rel=\"stylesheet\" type=\"text/css\" href=\"/_layouts/15/SiteNavigationTree2013/TreeControl.css\">";
		const string defaultXslFilePath = "SiteNavigationTree2013\\TreeControl.xsl";

		#endregion

		#region Members
		XmlDocument treeXml;
		System.Web.UI.HtmlControls.HtmlGenericControl treeDiv;
		System.Web.UI.WebControls.TextBox hdnOpenTreeNodes;

		public string ValidScriptClientID
		{
			get
			{
				return this.ClientID.Replace("{","_")
					.Replace("}","_")
					.Replace(":","_")
					.Replace("-","_")
					.Replace("%","_")
					.Replace("#","_");
			}
		}

		/// <summary>
		/// Resource caching
		/// </summary>
		private static Hashtable htResources = new Hashtable();
		#endregion

		#region properties
		string xslFilePath = defaultXslFilePath;
		public string XslFilePathRelativeToLayouts
		{
			get
			{
				return xslFilePath;
			}
			set
			{
				if( value != null && value.Trim() != string.Empty )
					xslFilePath = value;
				else
					throw new Exception(Exception_Message_InvalidXslPath);
			}
		}

		TreeItemStyles itemStyles = new TreeItemStyles();
		public TreeItemStyles ItemStyles
		{
			get
			{
				return itemStyles;
			}
		}
		#endregion

		#region Constructor
		public TreeControl()
		{
			this.PreRender += new EventHandler(TreeControl_PreRender);
		}
		#endregion

		#region tree XML XSL methods
		public XmlElement AddNode( ref XmlElement elmParent, string Title, string ToolTip, string ID, string IconUrl, string OnLoadChildren)
		{
			return AddNode( ref elmParent, Title, ToolTip, ID, IconUrl, false, OnLoadChildren);
		}
		public XmlElement AddNode( ref XmlElement elmParent, string Title, string ToolTip, string ID, string IconUrl
			, bool IsExpanded, string OnLoadChildren)
		{
			return AddNode( ref elmParent, Title, ToolTip, ID, IconUrl, IsExpanded, TreeItemState.Unselected, OnLoadChildren);
		}
		public XmlElement AddNode( ref XmlElement elmParent, string Title, string ToolTip, string ID, string IconUrl
			, bool IsExpanded, TreeItemState State, string OnLoadChildren)
		{
			return AddNode( ref elmParent, Title, ToolTip, ID, IconUrl, IsExpanded, State, "", "alert(this.id);", OnLoadChildren);
		}
		public XmlElement AddNode( ref XmlElement elmParent, string Title, string ToolTip, string ID, string IconUrl
            , bool IsExpanded, TreeItemState State, string Link, string OnClick, string OnLoadChildren)
		{
			return AddNode( ref elmParent, Title, ToolTip, ID, IconUrl, IsExpanded, State, Link, OnClick, "", OnLoadChildren);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Parent">The Parent Element this item will be added under. Send null to create a new root item.</param>
		/// <param name="Title">the title that will be displayed</param>
		/// <param name="ToolTip">the tool tip for the item</param>
		/// <param name="ID">for future use by the provider</param>
		/// <param name="IconUrl">The icon for the item will be displayed between the +- mark and the title</param>
		/// <param name="IsExpanded">True will show the item opened and all child items will be shown as well</param>
		/// <param name="State">TreeItemState will decide the css style for the item</param>
        /// <param name="Link">href on the item</param>
        /// <param name="OnClick">hadle click on the item</param>
        /// <param name="OnContextMenu">handle right click and click not on text (like WSS menu)</param>
		/// <returns>The new item that was created. for use to add children.</returns>
		public XmlElement AddNode( ref XmlElement elmParent, string Title, string ToolTip, string ID, string IconUrl
            , bool IsExpanded, TreeItemState State, string Link, string OnClick, string OnContextMenu, string OnLoadChildren)
		{
			this.EnsureChildControls();

			//default expanded state is saved by view state.
			IsExpanded = IsExpanded || (hdnOpenTreeNodes.Text.IndexOf(";" +  ID + ";") >= 0 );

			//Do we have a parent? if not - create.
			if(elmParent == null)
			{
				if( treeXml == null )
				{
					treeXml = new XmlDocument();
					treeXml.LoadXml(TreeXML_Root_TreeRoot);
				}

				elmParent = treeXml.FirstChild as XmlElement;
			}

			if( elmParent.GetAttribute(TreeXML_Attribue_Locked) == bool.TrueString.ToLower() )
				throw new Exception(Exception_Message_CannotEditLockedElm);

			//create new tree item under parent
			XmlElement treeItem =  elmParent.OwnerDocument.CreateElement(TreeXML_Elm_TreeItem);

			//fill new tree item with data.
			treeItem.SetAttribute(TreeXML_Attribue_Title, Title.Replace("\"","''"));
			treeItem.SetAttribute(TreeXML_Attribue_Description, ToolTip.Replace("\"","''"));
			treeItem.SetAttribute(TreeXML_Attribue_ID,ID);
			treeItem.SetAttribute(TreeXML_Attribue_Icon,IconUrl);

			treeItem.SetAttribute(TreeXML_Attribue_OnLoadChildren, OnLoadChildren);

			//overloads - empty is default
			treeItem.SetAttribute(TreeXML_Attribue_IsExpanded,IsExpanded.ToString().ToLower());
            treeItem.SetAttribute(TreeXML_Attribue_Link, string.IsNullOrEmpty(Link)? "#":Link);
            treeItem.SetAttribute(TreeXML_Attribue_OnClick, OnClick);
            treeItem.SetAttribute(TreeXML_Attribue_OnContextMenu, OnContextMenu);

			switch( State )
			{
				case TreeItemState.Selected:
					treeItem.SetAttribute(TreeXML_Attribue_CSSClassName, ItemStyles.SelectedCssName);
					break;
				case TreeItemState.Unselected:
					treeItem.SetAttribute(TreeXML_Attribue_CSSClassName,ItemStyles.UnSelectedCssName);
					break;
				case TreeItemState.Disabled:
					treeItem.SetAttribute(TreeXML_Attribue_CSSClassName,ItemStyles.DisabledCssName);
					break;
			}

			treeItem.SetAttribute(TreeXML_Attribue_State,State.ToString());

            bool isAdded = false;
		    foreach(XmlNode node in elmParent.ChildNodes)
            {
                if(treeItem.Attributes[TreeXML_Attribue_Title].Value.CompareTo(node.Attributes[TreeXML_Attribue_Title].Value) < 0)
                {
                    elmParent.InsertBefore(treeItem, node);
                    isAdded = true;
                    break;
                }
            }
            if(!isAdded)
			    elmParent.AppendChild(treeItem);
    	    return treeItem;
		}
		public void AddDummyNode(ref XmlElement elmParent)
		{
			if(elmParent != null)
			{
				//clear parent
				if( elmParent.ChildNodes.Count > 0 )
					throw new Exception(Exception_Message_DummyElmCannotHaveSiblings);

				//add dummy
				XmlElement dummy = elmParent.OwnerDocument.CreateElement(TreeXML_Elm_Dummy);
				elmParent.AppendChild(dummy);

				//lock parent
				elmParent.SetAttribute(TreeXML_Attribue_Locked,bool.TrueString.ToLower());
			}
		}

		public bool IsNodeOpen( ref XmlElement elmParent )
		{
			if(elmParent == null) return false;

			return elmParent.GetAttribute(TreeXML_Attribue_IsExpanded) == bool.TrueString.ToLower();
		}

		public void ExpandParentNodes(XmlElement elmParent)
		{
			while( elmParent != null )
			{
				elmParent.SetAttribute(TreeXML_Attribue_IsExpanded, bool.TrueString.ToLower());

				elmParent = elmParent.ParentNode as XmlElement;
			}
		}


		/// <summary>
		/// Returns the tree html
		/// </summary>
		/// <returns></returns>
		public string GetTreeHTML()
		{
			return GetTreeHTML(this.ValidScriptClientID);
		}
		public string GetTreeHTML(string ClientID)
		{
			return GetTreeHTML(treeXml, ClientID);
		}

		private string GetTreeHTML(XmlDocument treeXml, string ClientID)
		{
			string tree =  XslTransform(treeXml,this.XslFilePathRelativeToLayouts);

			return tree.Replace(TreeHTML_ClientIDToken, ClientID);
		}

//FOR FUTURE USE!!!!!!!!!! LAZY LOAD///////////////////////////////////
		public string GetChildrenHTML()
		{
			return GetChildrenHTML(this.ValidScriptClientID);
		}
		public string GetChildrenHTML(string ClientID)
		{
			XmlDocument tempDoc = new XmlDocument();
			tempDoc.LoadXml(TreeXML_Root_TempRoot);
			tempDoc.FirstChild.InnerXml = treeXml.FirstChild.InnerXml;
			
			return GetTreeHTML( tempDoc, ClientID );
		}
///////////////////////////////////////////////////////////////////////

		/// <summary>
		/// transforms xml document with the xsl in the given path.
		/// </summary>
		/// <param name="XmlDoc">XML To Transform</param>
		/// <param name="xslPath">XSL path to load</param>
		/// <returns>Transformed HTML</returns>
		public string XslTransform(XmlDocument XmlDoc, string xslPath)
		{
			XslTransform xslTransform = null;
			StringBuilder stmHtmlTreeView = new StringBuilder(10000);
			TextWriter txtHtmlOutput = new StringWriter(stmHtmlTreeView);

			try
			{
				if( Page.Cache.Get(xslPath) is XslTransform )
					xslTransform = Page.Cache.Get("DocMasterTreeXsl") as XslTransform;
			}
			catch{}

			if( xslTransform == null )//no cache
			{
				XmlDocument	xslDocument = new XmlDocument();
				string xslLocalPath = SPUtility.GetVersionedGenericSetupPath("Template\\LAYOUTS\\", 15) + xslPath;
				xslDocument.Load(xslLocalPath);

				xslTransform = new XslTransform();
				xslTransform.Load(xslDocument.CreateNavigator());

				try
				{
					//save to cache
					Page.Cache.Insert(xslPath, xslTransform);
				}
				catch{}
			}

			xslTransform.Transform(XmlDoc.CreateNavigator(), null, txtHtmlOutput, null);

			txtHtmlOutput.Close();

			return stmHtmlTreeView.ToString();
		}


		#endregion

		#region Implementation

		/// <summary>
		/// Render this control to the output parameter specified.
		/// </summary>
		/// <param name="output"> The HTML writer to write out to </param>
		protected override void Render(HtmlTextWriter output)
		{
			try
			{
				this.EnsureChildControls();
				this.RenderChildren(output);
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.Write(ex.ToString());
			}
		}

		protected override void CreateChildControls()
		{
			treeDiv = new System.Web.UI.HtmlControls.HtmlGenericControl(HtmlElm_DIV);
			this.Controls.Add(treeDiv);
			hdnOpenTreeNodes = new System.Web.UI.WebControls.TextBox();
			this.hdnOpenTreeNodes.Style[StyleDisplay] = StyleDisplay_Value_None;
			this.hdnOpenTreeNodes.ID = hdnOpenTreeNodesID;
			this.Controls.Add(hdnOpenTreeNodes);
		}

		#endregion

		#region EventHandlers
		private void TreeControl_PreRender(object sender, EventArgs e)
		{
			try
			{
				if( ! this.Page.ClientScript.IsClientScriptBlockRegistered(DefaultScriptName) )
					this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), DefaultScriptName, ScriptTag);

				if (!this.Page.ClientScript.IsClientScriptBlockRegistered(DefaultStylesName))
					this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), DefaultStylesName, DefaultStyleCSSTag);

				this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), this.ValidScriptClientID,
					@"
<script>
	var HoverStyle"+this.ValidScriptClientID+@" = """+this.itemStyles.HoverCssName+@""";
</script>
");

				this.EnsureChildControls();
				this.treeDiv.InnerHtml = GetTreeHTML();
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.Write(ex.ToString());
			}
		}
		#endregion
	}
}
