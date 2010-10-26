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

namespace KWizCom.Web.UI.WebControls
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
		const string TreeControlResourceRequest = "TreeControlResource";
		const string hdnOpenTreeNodesID = "hdnOpenTreeNodes";
		const string ResponseContentType_Gif = "Content-Type: image/gif";
		const string ResourceGifExtention = ".gif";
		const string StyleDisplay = "display";
		const string StyleDisplay_Value_None = "none";
		const string HtmlElm_DIV = "DIV";
		const string ScriptFormat = "<SCRIPT LANGUAGE=\"Javascript\" SRC=\"{0}?TreeControlResource=TreeControl.js\"></SCRIPT>";
		const string DefaultStyleNameFormat = "<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}?TreeControlResource=TreeControl.css\">";

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
		const string defaultXslFilePath = "TreeControl.xsl";
		string xslFilePath = defaultXslFilePath;
		public string XslFilePath
		{
			get
			{
				//In case we wand the default value - it is embedded in the control, so recall this page and request the resource.
				if( xslFilePath == defaultXslFilePath )
				{
					return new Uri(this.Page.Request.Url, Page.Request.Url.AbsolutePath + "?" + TreeControlResourceRequest + "=" + xslFilePath).ToString();
				}
				else
				{
					return xslFilePath;
				}
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
			this.Init += new EventHandler(TreeControl_Init);
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
			return AddNode( ref elmParent, Title, ToolTip, ID, IconUrl, IsExpanded, State, "alert(this.id);", OnLoadChildren);
		}
		public XmlElement AddNode( ref XmlElement elmParent, string Title, string ToolTip, string ID, string IconUrl
			, bool IsExpanded, TreeItemState State, string OnClick, string OnLoadChildren)
		{
			return AddNode( ref elmParent, Title, ToolTip, ID, IconUrl, IsExpanded, State, OnClick, "", OnLoadChildren);
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
		/// <param name="OnClick">hadle click on the item</param>
		/// <param name="OnContextMenu">handle right click and click not on text (like WSS menu)</param>
		/// <returns>The new item that was created. for use to add children.</returns>
		public XmlElement AddNode( ref XmlElement elmParent, string Title, string ToolTip, string ID, string IconUrl
			, bool IsExpanded, TreeItemState State, string OnClick, string OnContextMenu, string OnLoadChildren)
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
			elmParent.AppendChild(treeItem);

			//fill new tree item with data.
			treeItem.SetAttribute(TreeXML_Attribue_Title, Title.Replace("\"","''"));
			treeItem.SetAttribute(TreeXML_Attribue_Description, ToolTip.Replace("\"","''"));
			treeItem.SetAttribute(TreeXML_Attribue_ID,ID);
			treeItem.SetAttribute(TreeXML_Attribue_Icon,IconUrl);

			treeItem.SetAttribute(TreeXML_Attribue_OnLoadChildren, OnLoadChildren);

			//overloads - empty is default
			treeItem.SetAttribute(TreeXML_Attribue_IsExpanded,IsExpanded.ToString().ToLower());
			treeItem.SetAttribute(TreeXML_Attribue_OnClick,OnClick);
			treeItem.SetAttribute(TreeXML_Attribue_OnContextMenu,OnContextMenu);

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
			string tree =  XslTransform(treeXml,this.XslFilePath);

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
				XmlUrlResolver urlResolver = new XmlUrlResolver();
				//Set default credentials
				urlResolver.Credentials = System.Net.CredentialCache.DefaultCredentials;
				xslDocument.XmlResolver = urlResolver;
				xslDocument.Load(xslPath);

				xslTransform = new XslTransform();
				xslTransform.Load(xslDocument.CreateNavigator(),urlResolver, null);

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
		private void TreeControl_Init(object sender, EventArgs e)
		{
			try
			{
				LoadResourceRequest();
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.Write(ex.ToString());
			}
		}
		private void TreeControl_PreRender(object sender, EventArgs e)
		{
			try
			{
				if( ! this.Page.IsClientScriptBlockRegistered(DefaultScriptName) )
					this.Page.RegisterClientScriptBlock(DefaultScriptName, string.Format(ScriptFormat, Page.Request.Url.AbsolutePath));

				if( ! this.Page.IsClientScriptBlockRegistered(DefaultStylesName) )
					this.Page.RegisterClientScriptBlock(DefaultStylesName, string.Format(DefaultStyleNameFormat, Page.Request.Url.AbsolutePath));

				this.Page.RegisterClientScriptBlock(this.ValidScriptClientID,
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

		#region Custom Methods
		private void LoadResourceRequest()
		{
			//Load Resources from htResources hashtable (gif js css)
			string TreeControlResource = Page.Request[TreeControlResourceRequest];
			if( TreeControlResource != null && TreeControlResource != string.Empty )
			{
				string key = htResources[TreeControlResource] as string;
				switch(key)
				{
					case "pause":
						string time = Page.Request[TreeControlResourceRequest];
						if( time == null || time == "")
							time = "10";
						htResources[TreeControlResource] = System.Text.Encoding.UTF8.GetBytes("<script>window.setTimeout( function () { window.close(); }, "+time+");</script>");
						break;
					default:
						if( !htResources.ContainsKey(TreeControlResource) ||
							htResources[TreeControlResource] != null )
						{
							//get resource stream into string
							System.IO.Stream strm = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(this.GetType(), TreeControlResource.ToLower());
							byte[] strmBytes = new byte[strm.Length];
							strm.Read( strmBytes, 0, (int)strm.Length );

							htResources[TreeControlResource] = strmBytes;
						}
						break;
				}

				if( TreeControlResource.ToLower().EndsWith(ResourceGifExtention) )
				{
					Page.Response.ContentType = ResponseContentType_Gif;
				}

				Page.Response.Clear();
				Page.Response.BinaryWrite( (byte[])htResources[TreeControlResource] );
				Page.Response.End();
			}
		}
		#endregion

	}
}
