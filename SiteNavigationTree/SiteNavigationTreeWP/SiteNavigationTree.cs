#define DEBUGER
using System.Collections;
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml.Serialization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebPartPages;
using System.Xml;

namespace KWizCom.Sharepoint.WebParts.SiteNavigationTree
{
	/// <summary>
	/// Description for WebPart1.
	/// </summary>
	[DefaultProperty("Text"),
	ToolboxData("<{0}:SiteNavigationTree runat=server></{0}:SiteNavigationTree>"),
	XmlRoot(Namespace="http://www.KWizCom.com/SiteNavigationTree")]
	public class SiteNavigationTree : Microsoft.SharePoint.WebPartPages.WebPart
	{
		#region members
		SPWeb currentWeb;
        SPSite treeRootSite;
        SPWeb treeRootWeb;
        bool needDispose = false;
        #endregion

		#region controls
		KWizCom.Web.UI.WebControls.TreeControl theTreeControl;
		#endregion

		#region Properties
		#region Tree Properties
		
		#region RootUrl
		private const string defaultRootUrl = "";

		private string rootUrl = defaultRootUrl;

		[Browsable(true),
		Category("Misc"),
		DefaultValue(defaultRootUrl),
		WebPartStorage(Storage.Personal),
		FriendlyName("Root Web URL"),
		Description("The URL to the site the tree will start from")]
		public string RootUrl
		{
			get
			{
				return rootUrl;
			}

			set
			{
				rootUrl = value;
			}
		}
		
		#endregion

		#region ShowEntireSiteCollection
		private const bool defaultShowEntireSiteCollection = true;

		private bool showEntireSiteCollection = defaultShowEntireSiteCollection;

		[Browsable(true),
		Category("Misc"),
		DefaultValue(defaultShowEntireSiteCollection),
		WebPartStorage(Storage.Personal),
		FriendlyName("Show Entire Site Collection"),
		Description("Uncheck this to load the tree from the current location only")]
		public bool ShowEntireSiteCollection
		{
			get
			{
				return showEntireSiteCollection;
			}

			set
			{
				showEntireSiteCollection = value;
			}
		}
		
		#endregion

        #region ShowErrors
        private const bool defaultShowErrors = true;

        private bool showErrors = defaultShowErrors;

        [Browsable(true),
        Category("Misc"),
        DefaultValue(defaultShowErrors),
        WebPartStorage(Storage.Personal),
        FriendlyName("Show Errors"),
        Description("When checked, shows all errors on the page. Uncheck this once you are done configuring your web part.")]
        public bool ShowErrors
        {
            get
            {
                return showErrors;
            }

            set
            {
                showErrors = value;
            }
        }

        #endregion

		#region ExpandLevels
		private const int defaultExpandLevels = 1;

		private int expandLevels = defaultExpandLevels;

		[Browsable(true),
		Category("Misc"),
		DefaultValue(defaultExpandLevels),
		WebPartStorage(Storage.Personal),
		FriendlyName("Number or expanded levels"),
		Description("Enter the number of levels that should be expanded by default")]
		public int ExpandLevels
		{
			get
			{
				return expandLevels;
			}

			set
			{
				expandLevels = value;
			}
		}
		
		#endregion

		#region TreeIcon
		private const string defaultTreeIcon = "";

		private string treeIcon = defaultTreeIcon;

		[Browsable(true),
		Category("Misc"),
		DefaultValue(defaultTreeIcon),
		WebPartStorage(Storage.Personal),
		FriendlyName("Current site tree icon"),
		Description("enter url for the icon that will be displayed in the tree web part")]
		public string TreeIcon
		{
			get
			{
				return treeIcon;
			}
			set
			{
				treeIcon = value;
				try
				{
					if( value != SPControl.GetContextWeb(Context).Properties["treeicon"])
					{
						SPControl.GetContextWeb(Context).AllowUnsafeUpdates = true;
						SPControl.GetContextWeb(Context).Properties["treeicon"] = value;
						SPControl.GetContextWeb(Context).Properties.Update();
					}
				}
				catch{}
			}
		}
		
		#endregion

		#region tree styles

		#region SelectedCssName
		private const string defaultSelectedCssName = "";

		private string selectedCssName = defaultSelectedCssName;

		[Browsable(true),
		Category("Tree Style"),
		DefaultValue(defaultSelectedCssName),
		WebPartStorage(Storage.Personal),
		FriendlyName("Selected Item Class Name"),
		Description("Selected Item CSS Class Name")]
		public string SelectedCssName
		{
			get
			{
				return selectedCssName;
			}

			set
			{
				selectedCssName = value;
			}
		}
		
		#endregion

		#region UnSelectedCssName
		private const string defaultUnSelectedCssName = "";

		private string unSelectedCssName = defaultUnSelectedCssName;

		[Browsable(true),
		Category("Tree Style"),
		DefaultValue(defaultUnSelectedCssName),
		WebPartStorage(Storage.Personal),
		FriendlyName("Unselected Item Class Name"),
		Description("Unselected Item CSS Class Name")]
		public string UnSelectedCssName
		{
			get
			{
				return unSelectedCssName;
			}

			set
			{
				unSelectedCssName = value;
			}
		}
		
		#endregion

		#region HoverCssName
		private const string defaultHoverCssName = "";

		private string hoverCssName = defaultHoverCssName;

		[Browsable(true),
		Category("Tree Style"),
		DefaultValue(defaultHoverCssName),
		WebPartStorage(Storage.Personal),
		FriendlyName("Hover Item Class Name"),
		Description("Item CSS Class Name During Mouse Hover")]
		public string HoverCssName
		{
			get
			{
				return hoverCssName;
			}

			set
			{
				hoverCssName = value;
			}
		}
		
		#endregion

		#endregion

        #region ShowDescription
        private bool _ShowDescription = false;
        [Browsable(true),
        Category("Misc"),
        DefaultValue(false),
        WebPartStorage(Storage.Personal),
       FriendlyName("Show Description"),
        Description("Thee view will display [Title]-[Description]")]
        public bool ShowDescription
        {
            get
            {
                return _ShowDescription;
            }
            set
            {
                _ShowDescription = value;
            }
        }
        #endregion
		#endregion
		#endregion

		#region Ctor
		public SiteNavigationTree()
		{
			this.Init += new EventHandler(SiteNavigationTree_Init);
			this.Load += new EventHandler(SiteNavigationTree_Load);
			this.PreRender += new EventHandler(SiteNavigationTree_PreRender);
		}
		#endregion

		#region overrides
		public override ToolPart[] GetToolParts()
		{
			try
			{
				ToolPart[] toolparts = new ToolPart[3];
				KWizComToolPart kwizcomToolPart = new KWizComToolPart();

				CustomPropertyToolPart custom = new CustomPropertyToolPart();
				WebPartToolPart wptp = new WebPartToolPart();
				
				toolparts[0] = kwizcomToolPart;
				toolparts[1] = custom;
				toolparts[2] = wptp;
				return toolparts;
			}
			catch(Exception ex)
			{AddError(ex);}
			return null;
		}

		protected override void CreateChildControls()
		{
			theTreeControl = new KWizCom.Web.UI.WebControls.TreeControl();

			if( this.SelectedCssName != string.Empty )
				theTreeControl.ItemStyles.SelectedCssName = this.SelectedCssName;
			if( this.UnSelectedCssName != string.Empty )
				theTreeControl.ItemStyles.UnSelectedCssName = this.UnSelectedCssName;
			//			if( this.DisabledCssName != string.Empty )
			//				theTreeControl.ItemStyles.DisabledCssName = this.DisabledCssName ;
			if( this.HoverCssName != string.Empty )
				theTreeControl.ItemStyles.HoverCssName = this.HoverCssName;
			theTreeControl.XslFilePath = this.Page.Server.MapPath("/_wpresources/KWizCom.Sharepoint.WebParts.SiteNavigationTree/1.0.0.0__96e74440bad80f40/treecontrol.xsl");
			this.Controls.Add( theTreeControl );
		}

		/// <summary>
		/// Render this Web Part to the output parameter specified.
		/// </summary>
		/// <param name="output"> The HTML writer to write out to </param>
		protected override void RenderWebPart(HtmlTextWriter output)
		{
			try
			{
				this.EnsureChildControls();
				this.RenderChildren(output);
			}
			catch(Exception ex)
			{
				AddError(ex);
			}
		}
		#endregion

		#region general methods
        void AddError(Exception ex)
        {
            if (this.ShowErrors)
                Page.Response.Write(ex.ToString().Replace("\n", "<br>"));
        }

		void Initialize()
		{
			try
			{
				this.EnsureChildControls();

                this.currentWeb = SPContext.Current.Web;

				if( this.RootUrl.Trim() != string.Empty )
				{
					try
					{
                        this.treeRootSite = new SPSite( new Uri(Page.Request.Url, this.RootUrl, true).ToString().Replace("%3A",":").Replace("%20"," ") );
                        this.treeRootSite.CatchAccessDeniedException = false;//dont prompt for login, fail here.

                        if (this.ShowEntireSiteCollection)
                            this.treeRootWeb = this.treeRootSite.RootWeb;
                        else
    						this.treeRootWeb = this.treeRootSite.OpenWeb();
                        this.needDispose = true;
					}
					catch(Exception exx)
					{
                        if (this.treeRootSite != null)
                        {
                            this.treeRootSite.Dispose();
                            this.treeRootSite = null;
                        }
                        if (this.treeRootWeb != null)
                        {
                            this.treeRootWeb.Dispose();
                            this.treeRootWeb = null;
                        }
                        AddError(exx);
					}
				}
				else
				{
                    this.needDispose = false;

                    this.treeRootSite = SPContext.Current.Site;
                    if (this.ShowEntireSiteCollection)
                        this.treeRootWeb = SPContext.Current.Site.RootWeb;
                    else
                        this.treeRootWeb = this.currentWeb;
				}
			}
			catch(Exception ex)
			{
				AddError(ex);
			}
		}

		void BuildTree()
		{
			try
			{
				BuildTree(null, this.treeRootWeb, 0);
			}
			catch(Exception ex)
			{
				AddError(ex);
			}
		}

		void BuildTree(XmlElement parent, SPWeb web, int level)
		{
			KWizCom.Web.UI.WebControls.TreeItemState state = KWizCom.Web.UI.WebControls.TreeItemState.Unselected;
			if( web.ID == this.currentWeb.ID )
			{
				state = KWizCom.Web.UI.WebControls.TreeItemState.Selected;
                
				this.theTreeControl.ExpandParentNodes(parent);
			}

			bool isExpanded = level < this.ExpandLevels;

			string iconUrl = "/_layouts/images/STSICON.GIF";
			try
			{
				string iconFile = web.Properties["treeicon"];
				if( iconFile!=null && iconFile!="" )
					iconUrl = iconFile;
			}
			catch{}
            string title = web.Title;
            if (this.ShowDescription && !string.IsNullOrEmpty(web.Description))
            {
                title += " - " + web.Description;
            }
            XmlElement current = this.theTreeControl.AddNode(ref parent, title, web.Description, web.ID.ToString(),
				iconUrl,isExpanded,state,"window.location.href=\""+web.Url+"\";","","SNT_GetChildren('"+web.ID.ToString("D")+"','"+theTreeControl.ValidScriptClientID+"');");

			if( this.theTreeControl.IsNodeOpen(ref current) )
			{
				foreach(SPWeb sub in web.GetSubwebsForCurrentUser())
					BuildTree(current,sub, level+1);
			}
			else if(web.GetSubwebsForCurrentUser().Count > 0)
			{
				this.theTreeControl.AddDummyNode(ref current);
			}
		}

		void LoadTreeChildren(string ParentWebID, string TreeClientID)
		{
			string childrenHTML = "";

			Initialize();

			SPWeb parentWeb = treeRootSite.OpenWeb(new Guid(ParentWebID));

			foreach(SPWeb sub in parentWeb.GetSubwebsForCurrentUser())
				BuildTree(null,sub, this.ExpandLevels);

			childrenHTML = theTreeControl.GetChildrenHTML(TreeClientID);

			Page.Response.Clear();
			Page.Response.Write(childrenHTML);
			Page.Response.End();
		}
		#endregion

		#region event handlers

		private void SiteNavigationTree_Load(object sender, EventArgs e)
		{
			try
			{
				this.GetResourceRequest();
			}
			catch(Exception ex)
			{
				AddError(ex);
			}
		}

		private void SiteNavigationTree_PreRender(object sender, EventArgs e)
		{
			try
			{
				if( ! Page.ClientScript.IsClientScriptBlockRegistered("sitenavigationtree_script") )
				{
                    Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "sitenavigationtree_script", "<script src='" + this.ClassResourcePath + "/script.js'></script>");
				}
				Initialize();
				BuildTree();
			}
			catch(Exception ex)
			{
				AddError(ex);
			}

            if (this.needDispose)
            {
                if (this.treeRootSite != null)
                    this.treeRootSite.Dispose();
                if (this.treeRootWeb != null)
                    this.treeRootWeb.Dispose();
                this.treeRootSite = null;
                this.treeRootWeb = null;
            }
		}

		private void SiteNavigationTree_Init(object sender, EventArgs e)
		{
			try
			{
				switch(Page.Request["SitesTreeAction"])
				{
					case "GetChildren":
						LoadTreeChildren(Page.Request["ParentID"],Page.Request["TreeClientID"]);
						break;
				}
			}
			catch(System.Threading.ThreadAbortException)
			{}
			catch(Exception ex)
			{
                AddError(ex);
            }
		}

		#endregion

		#region resources
		/// <summary>
		/// Resource caching
		/// </summary>
		const string ResponseContentType_Gif = "Content-Type: image/gif";
		const string ResourceGifExtention = ".gif";
		private static Hashtable htResources = new Hashtable();
		private void GetResourceRequest()
		{
			try
			{
				//Load Resources from htResources hashtable (gif js css)
				string requestedResource = Page.Request.QueryString["SiteNavigationTree"];
				if( requestedResource != null && requestedResource != string.Empty )
				{
					//Add to cache
					if( !htResources.ContainsKey(requestedResource) ||
						htResources[requestedResource] != null )
					{
						//get resource stream into string
						System.IO.Stream strm = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("KWizCom.Sharepoint.WebParts.SiteNavigationTree." + requestedResource.ToLower());
						byte[] strmBytes = new byte[strm.Length];
						strm.Read( strmBytes, 0, (int)strm.Length );

						htResources[requestedResource] = strmBytes;
					}

					if( requestedResource.ToLower().EndsWith(ResourceGifExtention) )
					{
						Page.Response.ContentType = ResponseContentType_Gif;
					}

					Page.Response.Clear();
					Page.Response.BinaryWrite( (byte[])htResources[requestedResource] );
					Page.Response.End();
				}
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.Write(ex.ToString());
			}
		}
		#endregion

	}
}
