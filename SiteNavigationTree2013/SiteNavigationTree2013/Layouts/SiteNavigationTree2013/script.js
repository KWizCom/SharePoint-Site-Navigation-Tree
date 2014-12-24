function SNT_GetChildren(parentID, treeClientID)
{
    var requestUrl = window.location.href.split("?")[0];
    requestUrl = requestUrl.split("#")[0];

    var requestBody = "SitesTreeAction=GetChildren&ParentID="+escape(parentID)+"&TreeClientID="+escape(treeClientID);
    
    var children = SNT_PostRequestAndGetResult(requestUrl,requestBody);
    
    return children;
}

function SNT_PostRequestAndGetResult(requestUrl,requestBody)
{
	try
	{
	    var objHTTP = new XMLHttpRequest();

		objHTTP.open ("GET", requestUrl + "?" + requestBody, false);

		objHTTP.send();

		var strRes = objHTTP.responseText;
		
		return strRes;
	}
	catch(e)
	{
		return "Error|Request failed.";
	}
}
