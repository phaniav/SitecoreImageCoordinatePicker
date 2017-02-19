<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ContentAuditTool.aspx.cs" Inherits="Vhs.ContentAuditTool.sitecore.admin.ContentAudit" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Content Audit Tool</title>
	<style type="text/css">
		.divWaiting {
			position: absolute;
			background-color: #FAFAFA;
			z-index: 2147483647 !important;
			-ms-opacity: 0.8;
			opacity: 0.8;
			overflow: hidden;
			text-align: center;
			font-weight: bold;
			top: 0;
			left: 0;
			height: 100%;
			width: 100%;
			padding-top: 20%;
		}

		.warning {
			color: red;
		}
	</style>
</head>
<body>
	<form id="form1" runat="server">
	<asp:ScriptManager ID="ScriptManager" AsyncPostBackErrorMessage="Ajax Failed" runat="server">
	</asp:ScriptManager>
	<h1>Content Audit Tool</h1>
	<div>
		Database <asp:DropDownList ID="DatabasesDDL" AutoPostBack="True" runat="server" />
	</div>
	<br />
	<asp:UpdatePanel ID="UpdatePanel" UpdateMode="Conditional" runat="server">
		<ContentTemplate>
			<div>
				Site <asp:DropDownList ID="SitesDDL" AutoPostBack="True" runat="server" />
				<span class="warning"><asp:Literal ID="SiteListWarningLiteral" runat="server" /></span>
			</div>
			<br />
			<div>
				<asp:Label ID="SupportedLanguagesLabel" Text="Supported Language(s):" runat="server" />
				<span class="warning"><asp:Literal ID="SupportedLanguagesLiteral" runat="server" /></span>
			</div>
			<h3>Unsupported Versions</h3>
			<asp:GridView ID="UnsupportedVersionsGridView" AutoGenerateColumns="False" EmptyDataText="No records Found"
				DataKeyNames="Id, Language" AllowPaging="True" PageSize="16" runat="server">
				<Columns>
					<asp:TemplateField>
						<ItemTemplate>
							<asp:LinkButton ID="DeleteLinkButton" CommandName="Delete" Text="Delete" runat="server"
								OnClientClick="return confirm('Are you sure you want to delete this language version?');" />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField HeaderText="Language Version" DataField="Language">
						<ItemStyle HorizontalAlign="Center"></ItemStyle>
					</asp:BoundField>
					<asp:BoundField HeaderText="Sitecore Path" DataField="Path" />
				</Columns>
				 <AlternatingRowStyle BackColor="#DCDDE3" />
			</asp:GridView>
		</ContentTemplate>
		<Triggers>
			<asp:AsyncPostBackTrigger ControlID="SitesDDL" />
			<asp:AsyncPostBackTrigger ControlID="DatabasesDDL" />
			<asp:AsyncPostBackTrigger ControlID="UnsupportedVersionsGridView" />
		</Triggers>
	</asp:UpdatePanel>
	<asp:UpdateProgress ID="UpdateProgress1" runat="server">
		<ProgressTemplate>
			<div class="divWaiting">
				<asp:Label ID="ProcessingLabel" runat="server" Text="PROCESSING... " />
			</div>
		</ProgressTemplate>
	</asp:UpdateProgress>
	</form>
</body>
</html>
