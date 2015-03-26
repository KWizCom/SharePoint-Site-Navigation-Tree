using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.Utilities;
using System.Xml;

namespace KWizCom.SiteNavigationTree2013.Site_Navigation_Tree
{
	[ToolboxItemAttribute(false)]
	public class Site_Navigation_Tree : System.Web.UI.WebControls.WebParts.WebPart
	{
		#region members
		SPWeb currentWeb;
        List<SPSite> treeRootSites = new List<SPSite>();
        List<SPWeb> treeRootWebs = new List<SPWeb> ();
        #endregion

		#region controls
		TreeControl theTreeControl;
		#endregion

		#region Properties
		#region Tree Properties
		
	    #region RootTitle
		private const string defaultRootTitle = "";
		private string rootTitle = defaultRootTitle;
		[WebBrowsable(true),
        Personalizable(PersonalizationScope.Shared),
        Category("KWizCom Tree Settings"),
		DefaultValue(defaultRootTitle),
        WebDisplayName("Root Title"),
        WebDescription("Root Title")]
		public string RootTitle
		{
			get
			{
				return rootTitle;
			}

			set
			{
				rootTitle = value;
			}
		}
	    #endregion
	    #region RootTitleUrl
		private const string defaultRootTitleUrl = "";
		private string rootTitleUrl = defaultRootTitleUrl;
		[WebBrowsable(true),
        Personalizable(PersonalizationScope.Shared),
        Category("KWizCom Tree Settings"),
		DefaultValue(defaultRootTitle),
        WebDisplayName("Root Url"),
        WebDescription("Root Url")]
		public string RootTitleUrl
		{
			get
			{
				return rootTitleUrl;
			}

			set
			{
				rootTitleUrl = value;
			}
		}
	    #endregion
	
        #region RootUrl
		private const string defaultRootUrl = "";
		private string rootUrl = defaultRootUrl;
		[WebBrowsable(true),
        Personalizable(PersonalizationScope.Shared),
        Category("KWizCom Tree Settings"),
		DefaultValue(defaultRootUrl),
        WebDisplayName("Tree Web URL"),
        WebDescription("The URL to the site the tree will start from. You can enter multiple root sites separated with a semicolon (;).")]
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

        [WebBrowsable(true),
        Personalizable(PersonalizationScope.Shared),
        Category("KWizCom Tree Settings"),
		DefaultValue(defaultShowEntireSiteCollection),
        WebDisplayName("Show Entire Site Collection"),
        WebDescription("Uncheck this to load the tree from the current location only")]
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

        [WebBrowsable(true),
        Personalizable(PersonalizationScope.Shared),
        Category("KWizCom Tree Settings"),
        DefaultValue(defaultShowErrors),
        WebDisplayName("Show Errors"),
        WebDescription("When checked, shows all errors on the page. Uncheck this once you are done configuring your web part.")]
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

        [WebBrowsable(true),
        Personalizable(PersonalizationScope.Shared),
        Category("KWizCom Tree Settings"),
		DefaultValue(defaultExpandLevels),
        WebDisplayName("Number or expanded levels"),
        WebDescription("Enter the number of levels that should be expanded by default")]
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

        [WebBrowsable(true),
        Personalizable(PersonalizationScope.Shared),
        Category("KWizCom Tree Settings"),
		DefaultValue(defaultTreeIcon),
        WebDisplayName("Current site tree icon"),
        WebDescription("enter url for the icon that will be displayed in the tree web part")]
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

        [WebBrowsable(true),
        Personalizable(PersonalizationScope.Shared),
		Category("KWizCom Tree Style"),
		DefaultValue(defaultSelectedCssName),
        WebDisplayName("Selected Item Class Name"),
        WebDescription("Selected Item CSS Class Name")]
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

        [WebBrowsable(true),
        Personalizable(PersonalizationScope.Shared),
        Category("KWizCom Tree Style"),
		DefaultValue(defaultUnSelectedCssName),
        WebDisplayName("Unselected Item Class Name"),
        WebDescription("Unselected Item CSS Class Name")]
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

        [WebBrowsable(true),
        Personalizable(PersonalizationScope.Shared),
        Category("KWizCom Tree Style"),
		DefaultValue(defaultHoverCssName),
        WebDisplayName("Hover Item Class Name"),
        WebDescription("Item CSS Class Name During Mouse Hover")]
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
        [WebBrowsable(true),
        Personalizable(PersonalizationScope.Shared),
        Category("KWizCom Tree Settings"),
        DefaultValue(false),
        WebDisplayName("Show Description"),
        WebDescription("Thee view will display [Title]-[Description]")]
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
		public Site_Navigation_Tree()
		{
			this.Init += new EventHandler(SiteNavigationTree_Init);
			this.PreRender += new EventHandler(SiteNavigationTree_PreRender);
		}
		#endregion

		#region overrides

		protected override void CreateChildControls()
		{
			theTreeControl = new TreeControl();

			if( this.SelectedCssName != string.Empty )
				theTreeControl.ItemStyles.SelectedCssName = this.SelectedCssName;
			if( this.UnSelectedCssName != string.Empty )
				theTreeControl.ItemStyles.UnSelectedCssName = this.UnSelectedCssName;
			if( this.HoverCssName != string.Empty )
				theTreeControl.ItemStyles.HoverCssName = this.HoverCssName;
			this.Controls.Add( theTreeControl );
		}

		/// <summary>
		/// Render this Web Part to the output parameter specified.
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
                        string[] rootSites = this.RootUrl.Split(';');
                        foreach (string rootSite in rootSites)
                        {
                            try
                            {
                                SPSite treeRootSite = new SPSite(new Uri(Page.Request.Url, rootSite, true).ToString().Replace("%3A", ":").Replace("%20", " "));
                                treeRootSite.CatchAccessDeniedException = false;//dont prompt for login, fail here.
                                treeRootSites.Add(treeRootSite);
                                if (this.ShowEntireSiteCollection)
                                    treeRootWebs.Add(treeRootSite.RootWeb);
                                else
                                    treeRootWebs.Add(treeRootSite.OpenWeb());
                            }
                            catch (Exception exx)
                            {
                                AddError(exx);
                            }
                        }
					}
					catch(Exception exx)
					{
                        AddError(exx);
                    }
				}
				else
				{
                    SPSite treeRootSite = new SPSite(SPContext.Current.Site.Url);
                    treeRootSites.Add(treeRootSite);
                    if (this.ShowEntireSiteCollection)
                        treeRootWebs.Add(treeRootSite.RootWeb);
                    else
                        treeRootWebs.Add(treeRootSite.OpenWeb());
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
                XmlElement current = null;
                if (!string.IsNullOrEmpty(this.RootTitleUrl))
                {
                    string iconUrl = "/_layouts/15/images/STSICON.GIF";
                    string webDescription = "";
                    string webID = "rootid";
                    string webTitle = string.IsNullOrEmpty(this.RootTitle) ? this.RootTitleUrl : this.RootTitle;
                    try
                    {
                        using (SPSite site = new SPSite(this.RootTitleUrl))
                        {
                            using (SPWeb web = site.OpenWeb())
                            {
                                string iconFile = web.Properties["treeicon"];
                                if (iconFile != null && iconFile != "")
                                    iconUrl = iconFile;
                                webDescription = web.Description;
                                webID = web.ID.ToString();
                                webTitle = string.IsNullOrEmpty(this.RootTitle) ? web.Title : this.RootTitle;
                            }
                        }
                    }
                    catch { }
                    current = this.theTreeControl.AddNode(ref current, webTitle, webDescription, webID,
                        iconUrl, true, TreeItemState.Unselected, SPEncode.HtmlEncode(this.RootTitleUrl), "", "", "");
                    //foreach (SPWeb sub in this.treeRootWeb.GetSubwebsForCurrentUser())
                    //    BuildTree(current, sub, level + 1);
                }
                //else
                //{
                //    BuildTree(current, this.treeRootWeb, 0);
                //}
                foreach (SPWeb treeRootWeb in this.treeRootWebs)
                {
                    int level = 0;
                    if (this.treeRootWebs.Count == 1)
                    {
                        foreach (SPWeb sub in treeRootWeb.GetSubwebsForCurrentUser())
                            BuildTree(current, sub, level + 1);
                    }
                    else
                    {
                        BuildTree(current, treeRootWeb, 0);
                    }
                }
            }
			catch(Exception ex)
			{
				AddError(ex);
			}
		}

		void BuildTree(XmlElement parent, SPWeb web, int level)
		{
			TreeItemState state = TreeItemState.Unselected;
			if( web.ID == this.currentWeb.ID )
			{
				state = TreeItemState.Selected;
                
				this.theTreeControl.ExpandParentNodes(parent);
			}

			bool isExpanded = level < this.ExpandLevels;

			string iconUrl = "/_layouts/15/images/STSICON.GIF";
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
				iconUrl,isExpanded,state,SPEncode.HtmlEncode(web.Url),"","","SNT_GetChildren('"+web.ID.ToString("D")+"','"+theTreeControl.ValidScriptClientID+"');");

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

            foreach (SPSite treeRootSite in this.treeRootSites)
            {
                try
                {
                    SPWeb parentWeb = treeRootSite.OpenWeb(new Guid(ParentWebID));
                    if (parentWeb.Exists)
                    {
                        foreach (SPWeb sub in parentWeb.GetSubwebsForCurrentUser())
                            BuildTree(null, sub, this.ExpandLevels);
                        break;
                    }
                }
                catch
                {
                }
            }
			

			childrenHTML = theTreeControl.GetChildrenHTML(TreeClientID);

			Page.Response.Clear();
			Page.Response.Write(childrenHTML);
			Page.Response.End();
		}
		#endregion

		#region event handlers

		private void SiteNavigationTree_PreRender(object sender, EventArgs e)
		{
			try
			{
				if( ! Page.ClientScript.IsClientScriptBlockRegistered("sitenavigationtree_script") )
				{
                    Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "sitenavigationtree_script", "<script src='/_layouts/15/SiteNavigationTree2013/script.js'></script>");
				}
				Initialize();
				BuildTree();
			}
			catch(Exception ex)
			{
				AddError(ex);
			}

            foreach (SPWeb web in treeRootWebs)
            {
                web.Dispose();
            }
            foreach (SPSite site in treeRootSites)
            {
                site.Dispose();
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
	}

}
