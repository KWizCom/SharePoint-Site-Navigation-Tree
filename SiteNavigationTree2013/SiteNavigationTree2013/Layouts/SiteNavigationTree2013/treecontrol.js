﻿var TC_ImageName_Minus = "minus.gif";
var TC_ImageName_Plus = "plus.gif";
var TC_ImageName_Blank = "blank.gif";
var TC_Display_None = "none";
var TC_Img_State_Expand = "expand";
var TC_Img_State_Collapse = "collapse";
var TC_DummyID = "DummyID";
var TC_LoadedID = "LoadedID";
var TC_hdnOpenTreeNodes  = "_hdnOpenTreeNodes";
var TC_ProgressBarImg = "<img src='/_layouts/15/images/SiteNavigationTree2013/progress.gif'>";
var TC_OpenTreeNodesDelimeter = ";";
var TC_ItemSpan_HoverStyle = "HoverStyle";
var TC_Prefix_CellChildLoad = "<table BORDER=\"0\" CELLPADDING=\"0\" CELLSPACING=\"0\" WIDTH=\"1%\" >";
var TC_Suffix_CellChildLoad = "</table>";

function TC_ShowSubFolders(imgObj, bIsHasChildren, ClientID)
{
	try {
		if(bIsHasChildren == true)
		{
		    var pDataRow = imgObj.parentNode.parentNode.nextSibling;
			if (pDataRow.style.display == TC_Display_None)
			{
			    if (TC_FindChildById(pDataRow, TC_DummyID + imgObj.id) != null)
				{
					TC_OpenDummyNode(pDataRow,imgObj);
					try
					{
					    TC_FindChildById(pDataRow, TC_DummyID + imgObj.id).id = TC_LoadedID + imgObj.id;
					}
					catch(e)
					{
					}
				}
				else
				{
					//show child row
					pDataRow.style.display = '';
				}
			}
			else
			{
				pDataRow.style.display  = TC_Display_None;
			}
		}

		var oOpenTreeNodes = document.getElementById(ClientID + TC_hdnOpenTreeNodes);

		if(imgObj.getAttribute("state") == TC_Img_State_Expand)
		{
			imgObj.src = imgObj.src.replace(/plus.gif/gi,TC_ImageName_Minus);
			imgObj.setAttribute(state,TC_Img_State_Collapse);

			//add to open items...
			if( oOpenTreeNodes )
			{
				oOpenTreeNodes.value += TC_OpenTreeNodesDelimeter + imgObj.id + TC_OpenTreeNodesDelimeter;
			}
		}
		else
		{
			imgObj.src = imgObj.src.replace(/minus.gif/gi,TC_ImageName_Plus);
			imgObj.setAttribute(state,TC_Img_State_Expand);

			//remove from open items...
			if( oOpenTreeNodes )
			{
				while( oOpenTreeNodes.value.indexOf(TC_OpenTreeNodesDelimeter + imgObj.id + TC_OpenTreeNodesDelimeter) >= 0 )
				{
					oOpenTreeNodes.value = oOpenTreeNodes.value.replace(TC_OpenTreeNodesDelimeter + imgObj.id + TC_OpenTreeNodesDelimeter, "");
				}
			}
		}
	}
	catch(e)
	{
	}
}

function TC_ItemOver(oSelRow, clientID)
{
	try
	{
	    var oHiddenColumn = TC_FindChildById(oSelRow,"hiddencolumn");
		if( oHiddenColumn != null )
			oHiddenColumn.style.visibility = "visible";
		
		oSelRow.oldCss = oSelRow.className;
		oSelRow.className = "ms-selectedtitle" ;
		eval("oSelRow.className = " + TC_ItemSpan_HoverStyle + clientID);
	}
	catch(e)
	{}
}

function TC_ItemOut(oSelRow)
{
	try
	{
	    var oHiddenColumn = TC_FindChildById(oSelRow,"hiddencolumn");
		if( oHiddenColumn != null )
			oHiddenColumn.style.visibility = "hidden";
		
		oSelRow.className = oSelRow.oldCss;
	}
	catch(e)
	{}
}
function TC_OpenDummyNode(pDataRow, imgObj)
{
	//show loading image
    TC_FindChildById(pDataRow,TC_DummyID + imgObj.id).innerHTML = TC_ProgressBarImg;
	//show child row
	pDataRow.style.display = '';
	
	var strChildren = null;
	try
	{ eval("strChildren = " + TC_FindChildById(pDataRow, TC_DummyID + imgObj.id).getAttribute("OnLoadChildren") + ";"); }
	catch(e){strChildren = null}

	//If data was returned:
	if( strChildren == null )//error
	{
		//close this row and set image back to +.
		pDataRow.style.display = TC_Display_None;
		return;
	}
	else if( strChildren == "" )//no children
	{
		//remove +/-, remove child row.
		imgObj.src = imgObj.src.replace(/plus.gif/gi,TC_ImageName_Blank);
		imgObj.onclick = "";
		TC_FindChildById(pDataRow,TC_DummyID + imgObj.id).innerHTML = '';
		pDataRow.style.display = TC_Display_None;
		return;
	}
	else//load results
	{	//						<tr>	<table>
	    //insert resutls into pDataRow.parentNode.innerHTML
		try
		{
		    TC_FindChildById(pDataRow,TC_DummyID + imgObj.id).innerHTML = TC_Prefix_CellChildLoad + strChildren + TC_Suffix_CellChildLoad;
		}
		catch(e)
		{
			//the same as if( strChildren == null )
			pDataRow.style.display = TC_Display_None;
			return;
		}
	}
}
function TC_CollapseAll(ClientID)
{
	//collapse all the items besides the selected one
	var oOpenTreeNodes = document.getElementById(ClientID + TC_hdnOpenTreeNodes);
//	alert(oOpenTreeNodes.value);
	oOpenTreeNodes.value = "";
//	alert(oOpenTreeNodes.value);
	var tblObj = document.getElementById('tableLabel');
	var imgObj = tblObj.parentNode.parentNode.firstChild.firstChild;
//	alert(imgObj.src);
//	alert(imgObj.id);
	TC_ShowSubFolders(imgObj, true, ClientID);
	window.location.reload();
	//	alert(document.getElementById(ClientID + TC_hdnOpenTreeNodes).value);
}

function TC_FindChildById(elmParent, elmId) {
    //is it me?
    if (elmParent.id == elmId)
        return elmParent;
    //search children recursive
    for (var i = 0; i < elmParent.childNodes.length; i++) {
        var result = TC_FindChildById(elmParent.childNodes[i], elmId);
        if (result != null) return result;
    }
    //not found - return
    return null;
}