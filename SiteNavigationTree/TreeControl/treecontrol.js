var TC_ImageName_Minus = "minus.gif";
var TC_ImageName_Plus = "plus.gif";
var TC_ImageName_Blank = "blank.gif";
var TC_Display_None = "none";
var TC_Img_State_Expand = "expand";
var TC_Img_State_Collapse = "collapse";
var TC_DummyID = "DummyID";
var TC_LoadedID = "LoadedID";
var TC_hdnOpenTreeNodes  = "_hdnOpenTreeNodes";
var TC_ProgressBarImg = "<img src='?TreeControlResource=docmaster_save.gif'>";
var TC_OpenTreeNodesDelimeter = ";";
var TC_ItemSpan_HoverStyle = "HoverStyle";
var TC_Prefix_CellChildLoad = "<table BORDER=\"0\" CELLPADDING=\"0\" CELLSPACING=\"0\" WIDTH=\"1%\" >";
var TC_Suffix_CellChildLoad = "</table>";

function TC_ShowSubFolders(imgObj, bIsHasChildren, ClientID)
{
	try
	{
		if(bIsHasChildren == true)
		{
			var pDataRow = imgObj.parentElement.parentElement.nextSibling;
			if (pDataRow.style.display == TC_Display_None)
			{
				if( pDataRow.all(TC_DummyID + imgObj.id) != null )
				{
					TC_OpenDummyNode(pDataRow,imgObj);
					try
					{
						pDataRow.all(TC_DummyID + imgObj.id).id = TC_LoadedID + imgObj.id;
					}
					catch(e)
					{
//					alert(e);
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

		var oOpenTreeNodes = document.all(ClientID + TC_hdnOpenTreeNodes);

		if(imgObj.state == TC_Img_State_Expand)
		{
			imgObj.src = imgObj.src.replace(/plus.gif/gi,TC_ImageName_Minus);
			imgObj.state = TC_Img_State_Collapse;

			//add to open items...
			if( oOpenTreeNodes )
			{
				oOpenTreeNodes.value += TC_OpenTreeNodesDelimeter + imgObj.id + TC_OpenTreeNodesDelimeter;
			}
		}
		else
		{
			imgObj.src = imgObj.src.replace(/minus.gif/gi,TC_ImageName_Plus);
			imgObj.state = TC_Img_State_Expand;

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
		var oHiddenColumn = oSelRow.all["hiddencolumn"];
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
		var oHiddenColumn = oSelRow.all["hiddencolumn"];
		if( oHiddenColumn != null )
			oHiddenColumn.style.visibility = "hidden";
		
		oSelRow.className = oSelRow.oldCss;
	}
	catch(e)
	{}
}
function pause(numberMillis)
{
	//TODO: Find solution for explorer 7
	//var result = window.showModalDialog('?TreeControlResource=pause&Time=' + numberMillis);

	//var dialogScript = 'window.setTimeout( function () { window.close(); }, ' + numberMillis + ');';
	//var result = window.showModalDialog('javascript:document.writeln(\"<script>' + dialogScript + '</script>\");');
}
function TC_OpenDummyNode(pDataRow, imgObj)
{
	//show loading image
	pDataRow.all(TC_DummyID + imgObj.id).innerHTML = TC_ProgressBarImg;
	//show child row
	pDataRow.style.display = '';
	//let the image load...
	pause(1);
	
	var strChildren = null;
	try
	{eval("strChildren = " + pDataRow.all(TC_DummyID + imgObj.id).OnLoadChildren + ";");}
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
		pDataRow.all(TC_DummyID + imgObj.id).innerHTML  = '';
		pDataRow.style.display = TC_Display_None;
		return;
	}
	else//load results
	{	//						<tr>	<table>
		//insert resutls into pDataRow.parentElement.innerHTML
		try
		{
			pDataRow.all(TC_DummyID + imgObj.id).innerHTML = TC_Prefix_CellChildLoad + strChildren + TC_Suffix_CellChildLoad;
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
	var oOpenTreeNodes = document.all(ClientID + TC_hdnOpenTreeNodes);
//	alert(oOpenTreeNodes.value);
	oOpenTreeNodes.value = "";
//	alert(oOpenTreeNodes.value);
	var tblObj = document.getElementById('tableLabel');
	var imgObj = tblObj.parentElement.parentElement.firstChild.firstChild;
//	alert(imgObj.src);
//	alert(imgObj.id);
	TC_ShowSubFolders(imgObj, true, ClientID);
	window.location.reload();
//	alert(document.all(ClientID + TC_hdnOpenTreeNodes).value);
}