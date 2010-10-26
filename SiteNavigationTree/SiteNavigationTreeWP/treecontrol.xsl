<?xml version="1.0" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output encoding="utf-8" omit-xml-declaration="yes" />
	<xsl:template match="/">
		<xsl:for-each select=".">
			<xsl:apply-templates />
		</xsl:for-each>
	</xsl:template>
	<xsl:template match="TreeRoot">
		<TABLE BORDER="0" CELLPADDING="0" CELLSPACING="0" WIDTH="1%">
			<xsl:for-each select=".">
				<xsl:apply-templates />
			</xsl:for-each>
		</TABLE>
	</xsl:template>
	<xsl:template match="TreeItem">
		<xsl:param name='cChildren' select='count(*)' />
		<TR style="height: 17px;">
			<TD ALIGN="center" width="16px" valign="middle" height="17" nowrap="1">
				<xsl:choose>
					<xsl:when test='$cChildren &gt; 0'>
						<xsl:choose>
							<xsl:when test="@isexpanded = 'false'">
								<img state="expand" src="?TreeControlResource=plus.gif" BORDER="0" ONCLICK="TC_ShowSubFolders(this, true, '_ClientID_');">
									<xsl:attribute name="ID">
										<xsl:value-of select="@ID" />
									</xsl:attribute>
								</img>
							</xsl:when>
							<xsl:otherwise>
								<img state="collapse" src="?TreeControlResource=minus.gif" BORDER="0" ONCLICK="TC_ShowSubFolders(this, true, '_ClientID_');">
									<xsl:attribute name="ID">
										<xsl:value-of select="@ID" />
									</xsl:attribute>
								</img>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:when>
					<xsl:otherwise>
						<img src="?TreeControlResource=blank.gif" BORDER="0" width="16" height="16" />
					</xsl:otherwise>
				</xsl:choose>
			</TD>
			<TD width="2px">
				<img src="?TreeControlResource=blank.gif" BORDER="0" width="2" height="17" />
			</TD>
			<TD valign="middle" height="17">
				<xsl:choose>
					<xsl:when test="@Icon = ''">
						<img src="?TreeControlResource=blank.gif" BORDER="0" width="16" height="16" />
					</xsl:when>
					<xsl:otherwise>
						<IMG BORDER="0" width="16" height="16">
							<xsl:attribute name="SRC">
								<xsl:value-of select="@Icon" />
							</xsl:attribute>
						</IMG>
					</xsl:otherwise>
				</xsl:choose>
			</TD>
			<TD width="2px">
				<img src="?TreeControlResource=blank.gif" BORDER="0" width="2" height="17" />
			</TD>
			<TD width="100%" valign="middle" height="17" style="padding-left: 1px; padding-right: 1px;"> <!-- &amp;nbsp; -->
				<TABLE id="tableLabel" style="padding-top: 0px;padding-bottom: 0px;" cellpadding="0" cellspacing="0"
					onmouseover="TC_ItemOver(this, '_ClientID_');" onmouseout="TC_ItemOut(this, '_ClientID_');">
					<xsl:attribute name="class">
						<xsl:value-of select="@CSSClassName" />
					</xsl:attribute>
					<xsl:choose>
						<xsl:when test="@oncontextmenu = ''"></xsl:when>
						<xsl:otherwise>
							<xsl:attribute name="onclick">
								<xsl:value-of select="@oncontextmenu" />
							</xsl:attribute>
							<xsl:attribute name="oncontextmenu">
								<xsl:value-of select="@oncontextmenu" />
							</xsl:attribute>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:attribute name="title">
						<xsl:value-of select="@Description" />
					</xsl:attribute>
					<xsl:attribute name="ID">
						<xsl:value-of select="@ID" />
					</xsl:attribute>
					<TR>
						<TD nowrap="1">
							<SPAN style="cursor:hand;" class="item">
								<a href="#">
									<xsl:attribute name="id">
										<xsl:value-of select="@ID" />
									</xsl:attribute>
									<xsl:attribute name="onclick">
										<xsl:value-of select="@onclick" />
									</xsl:attribute>
									<xsl:value-of select="@Title" />
								</a>
							</SPAN>
						</TD><!-- display none will never show a menu arrow -->
						<TD class="ms-menuimagecell" id="hiddencolumn" style="display:none;visibility:hidden;">
							<SPAN id="spanhiddencolumn" style="cursor:hand;" class="item">
								<IMG id="imagehiddencolumn" SRC="?TreeControlResource=downarrw.gif" />
							</SPAN>
						</TD>
					</TR>
				</TABLE>
			</TD>
		</TR>
		<xsl:if test='$cChildren &gt; 0'>
			<TR style="height: 17px;">
				<xsl:if test="@isexpanded = 'false'">
					<xsl:attribute name="style">display:none;</xsl:attribute>
				</xsl:if>
				<TD width="16px">
					<img src="?TreeControlResource=blank.gif" BORDER="0" width="16" height="16" />
				</TD>
				<TD width="2px">
					<img src="?TreeControlResource=blank.gif" BORDER="0" width="2" height="17" />
				</TD>
				<TD COLSPAN="3" width="1px">
					<TABLE BORDER="0" CELLPADDING="0" CELLSPACING="0" WIDTH="1%">
						<tr>
							<td coslpan="4">
								<xsl:attribute name="OnLoadChildren">
									<xsl:value-of select="@OnLoadChildren" />
								</xsl:attribute>
								<xsl:choose>
									<xsl:when test="@isexpanded = 'false' and not(@OnLoadChildren = '') ">
										<xsl:attribute name="id">DummyID<xsl:value-of select="@ID" /></xsl:attribute>
									</xsl:when>
									<xsl:otherwise>
										<xsl:attribute name="id">LoadedID<xsl:value-of select="@ID" /></xsl:attribute>
									</xsl:otherwise>
								</xsl:choose>
								<table BORDER="0" CELLPADDING="0" CELLSPACING="0" WIDTH="1%">
									<xsl:for-each select=".">
										<xsl:apply-templates />
									</xsl:for-each>
								</table>
							</td>
						</tr>
					</TABLE>
				</TD>
			</TR>
		</xsl:if>
	</xsl:template>
	<!-- Dummy - for children that should not be loaded -->
	<xsl:template match="Dummy"></xsl:template>
</xsl:stylesheet>