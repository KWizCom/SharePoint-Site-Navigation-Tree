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
		//send post to page.
		//Create an instance of XMLHTTP
		var objHTTP = new ActiveXObject("MSXML2.XMLHTTP");

		// Open connection to the Action Page
		objHTTP.Open ("POST", requestUrl, false); //Declare the action file
		// Set the SOAP-Specific HTTP Headers
		objHTTP.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
		objHTTP.setRequestHeader("Content-Type", "text/xml; charset=utf-8");

		//Post the SOAP Request to the Action Page
		objHTTP.send( requestBody );

		var strRes = objHTTP.responseText;
		
		return strRes;
	}
	catch(e)
	{
		return "Error|Request failed.";
	}
}
