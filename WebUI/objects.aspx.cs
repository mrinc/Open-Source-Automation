﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OSAE;

public partial class home : System.Web.UI.Page
{
    Logging logging = Logging.GetLogger("Web UI");

    public void RaisePostBackEvent(string eventArgument)
    {
        string[] args = eventArgument.Split('_');

        if (args[0] == "gvObjects")
        {
            hdnSelectedRow.Text = args[1];
            panelEditForm.Visible = true;
            panelPropForm.Visible = false;
            hdnSelectedPropRow.Text = "";
            hdnSelectedObjectName.Text = gvObjects.DataKeys[Int32.Parse(hdnSelectedRow.Text)]["object_name"].ToString();
            loadDDLs();
            loadProperties();
            loadDetails();
        }
        else if (args[0] == "gvProperties")
        {
            hdnSelectedPropRow.Text = args[1];
            panelPropForm.Visible = true;
            hdnSelectedPropName.Text = gvProperties.DataKeys[Int32.Parse(hdnSelectedPropRow.Text)]["property_name"].ToString();
            lblPropName.Text = hdnSelectedPropName.Text;
            txtPropValue.Text = gvProperties.DataKeys[Int32.Parse(hdnSelectedPropRow.Text)]["property_value"].ToString();
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        gvObjects.DataSource = OSAESql.RunSQL("SELECT object_id, container_name, object_name, object_type, state_name, last_updated, address FROM osae_v_object order by container_name, object_name");
        gvObjects.DataBind();
    }

    protected void Page_PreRender(object sender, EventArgs e)
    {
        if (hdnSelectedRow.Text != "")
        {
            gvObjects.Rows[Int32.Parse(hdnSelectedRow.Text)].Attributes.Remove("onmouseout");
            gvObjects.Rows[Int32.Parse(hdnSelectedRow.Text)].Style.Add("background", "lightblue");
        }
        if (hdnSelectedPropRow.Text != "")
        {
            gvProperties.Rows[Int32.Parse(hdnSelectedPropRow.Text)].Attributes.Remove("onmouseout");
            gvProperties.Rows[Int32.Parse(hdnSelectedPropRow.Text)].Style.Add("background", "lightblue");
        }
    }

    protected void gvObjects_RowDataBound(object sender, System.Web.UI.WebControls.GridViewRowEventArgs e)
    {
        if ((e.Row.RowType == DataControlRowType.DataRow))
        {
            e.Row.Attributes.Add("onmouseover", "this.style.cursor='hand';this.style.background='lightblue';");
            if (e.Row.RowState == DataControlRowState.Alternate)
                e.Row.Attributes.Add("onmouseout", "this.style.background='#fcfcfc url(../images/grd_alt.png) repeat-x top';");
            else
                e.Row.Attributes.Add("onmouseout", "this.style.background='none';");
            e.Row.Attributes.Add("onclick", ClientScript.GetPostBackClientHyperlink(this, "gvObjects_" + e.Row.RowIndex.ToString()));
        }

    }

    protected void gvProperties_RowDataBound(object sender, System.Web.UI.WebControls.GridViewRowEventArgs e)
    {
        if ((e.Row.RowType == DataControlRowType.DataRow))
        {
            e.Row.Attributes.Add("onmouseover", "this.style.cursor='hand';this.style.background='lightblue';");
            if (e.Row.RowState == DataControlRowState.Alternate)
                e.Row.Attributes.Add("onmouseout", "this.style.background='#fcfcfc url(../images/grd_alt.png) repeat-x top';");
            else
                e.Row.Attributes.Add("onmouseout", "this.style.background='none';");
            e.Row.Attributes.Add("onclick", ClientScript.GetPostBackClientHyperlink(this, "gvProperties_" + e.Row.RowIndex.ToString()));
        }

    }

    protected void ddlState_SelectedIndexChanged(object sender, EventArgs e)
    {
        OSAEObjectStateManager.ObjectStateSet(hdnSelectedObjectName.Text, ddlState.SelectedItem.Value, "Web UI");
        lblAlert.Text = "State set successfully to " + ddlState.SelectedItem.Text;
        alert.Visible = true;
    }
    protected void ddlMethod_SelectedIndexChanged(object sender, EventArgs e)
    {
        OSAEMethodManager.MethodQueueAdd(hdnSelectedObjectName.Text, ddlMethod.SelectedItem.Value, "", "", "Web UI");
        lblAlert.Text = "Method successfuly executed: " + ddlMethod.SelectedItem.Text;
        alert.Visible = true;
    }
    protected void ddlEvent_SelectedIndexChanged(object sender, EventArgs e)
    {
        logging.EventLogAdd(hdnSelectedObjectName.Text, ddlEvent.SelectedItem.Value);
        lblAlert.Text = "Event set successfully to " + ddlEvent.SelectedItem.Text;
        alert.Visible = true;
    }

    private void loadDDLs()
    {
        OSAEObject obj = OSAEObjectManager.GetObjectByName(hdnSelectedObjectName.Text);

        ddlState.DataSource = OSAESql.RunSQL("SELECT state_label as Text, state_name as Value FROM osae_object_type_state ts INNER JOIN osae_object o ON o.object_type_id = ts.object_type_id where object_name = '" + hdnSelectedObjectName.Text + "'"); ;
        ddlState.DataBind();
        if (ddlState.Items.Count == 0)
            divState.Visible = false;
        else
            divState.Visible = true;
        ddlState.SelectedValue = obj.State.Value;

        ddlMethod.DataSource = OSAESql.RunSQL("SELECT method_label as Text, method_name as Value FROM osae_object_type_method ts INNER JOIN osae_object o ON o.object_type_id = ts.object_type_id where object_name = '" + hdnSelectedObjectName.Text + "'"); ;
        ddlMethod.DataBind();
        if (ddlMethod.Items.Count == 0)
            divMethod.Visible = false;
        else
            divMethod.Visible = true;
        ddlMethod.Items.Insert(0, new ListItem(String.Empty, String.Empty));

        ddlEvent.DataSource = OSAESql.RunSQL("SELECT event_label as Text, event_name as Value FROM osae_object_type_event ts INNER JOIN osae_object o ON o.object_type_id = ts.object_type_id where object_name = '" + hdnSelectedObjectName.Text + "'"); ;
        ddlEvent.DataBind();
        if (ddlEvent.Items.Count == 0)
            divEvent.Visible = false;
        else
            divEvent.Visible = true;
        ddlEvent.Items.Insert(0, new ListItem(String.Empty, String.Empty));

        ddlType.DataSource = OSAESql.RunSQL("SELECT object_type as Text, object_type as Value FROM osae_object_type"); ;
        ddlType.DataBind();
        if (ddlType.Items.Count == 0)
            ddlType.Visible = false;
        else
            ddlType.Visible = true;
        ddlType.Items.Insert(0, new ListItem(String.Empty, String.Empty));

        ddlContainer.DataSource = OSAESql.RunSQL("SELECT object_name as Text, object_name as Value FROM osae_v_object where container = 1"); ;
        ddlContainer.DataBind();
        if (ddlContainer.Items.Count == 0)
            ddlContainer.Visible = false;
        else
            ddlContainer.Visible = true;
        ddlContainer.Items.Insert(0, new ListItem(String.Empty, String.Empty));
    }

    private void loadProperties()
    {
        gvProperties.DataSource = OSAESql.RunSQL("SELECT property_name, property_value FROM osae_v_object_property where object_name='" + hdnSelectedObjectName.Text + "'");
        gvProperties.DataBind();
    }

    private void loadDetails()
    {
        OSAEObject obj = OSAEObjectManager.GetObjectByName(gvObjects.DataKeys[Int32.Parse(hdnSelectedRow.Text)]["object_name"].ToString());
        txtName.Text = obj.Name;
        txtDescr.Text = obj.Description;
        txtAddress.Text = obj.Address;
        ddlContainer.SelectedValue = obj.Container;
        ddlType.SelectedValue = obj.Type;
        if (obj.Enabled == 1)
            chkEnabled.Checked = true;
        else
            chkEnabled.Checked = false;
    }

    protected void btnPropSave_Click(object sender, EventArgs e)
    {
        OSAEObjectPropertyManager.ObjectPropertySet(hdnSelectedObjectName.Text, hdnSelectedPropName.Text, txtPropValue.Text, "Web UI");
        loadProperties();
    }
    protected void btnAdd_Click(object sender, EventArgs e)
    {
        OSAEObjectManager.ObjectAdd(txtName.Text, txtDescr.Text, ddlType.SelectedValue, txtAddress.Text, ddlContainer.SelectedValue, chkEnabled.Checked);
    }
    protected void btnDelete_Click(object sender, EventArgs e)
    {
        OSAEObjectManager.ObjectDelete(gvObjects.DataKeys[Int32.Parse(hdnSelectedRow.Text)]["object_name"].ToString());
    }
    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        int enabled = 0;
        if(chkEnabled.Checked)
            enabled = 1;
        OSAEObjectManager.ObjectUpdate(gvObjects.DataKeys[Int32.Parse(hdnSelectedRow.Text)]["object_name"].ToString(), txtName.Text, txtDescr.Text, ddlType.SelectedValue, txtAddress.Text, ddlContainer.SelectedValue, enabled);
    }
}