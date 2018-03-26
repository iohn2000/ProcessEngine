res = {
    GetRes: function (localizedString, parameters) {
        for (var i = 0; i < parameters.length; i++) {

            var searchString = '{' + i + '}';
            localizedString = localizedString.replace(searchString, parameters[i]);
        }
        return localizedString;
    }
}


res.Button = {

}

res.Button.Hint = {
    Edit: 'Edit',
    Configure: 'Configure'
}


res.Alert = {
    Title_Info: 'Information',
    Title_Error: 'Error',
    Title_NoDataFound: 'No data found error',
    GeneralError: 'A general error occured!',
    Timeout: 'The server didn\'t respond',
    FormValidation: 'The form couldn\'t be saved.<br>Please check the info message on top!',
    FormValidationTabError: 'The current tab has one or more errors. Please fix it for the next step!',
    FormValidationTabErrors: 'One of your tabs have errors. Please check all tabs for errors!',
    FormValidationTabErrorPrevious: 'The previous tab has one or more errors. Please fix it before onboarding!',
    Success_Default: 'The item was sucessfully created/updated',
    FormValidation_Unknown_Error: 'The form couldn\'t be saved.<br>There occured an unknown error on server.<br>The Server method couldn\t be reached.',
}

res.Confirmation = {
    Title_Delete: 'Delete',
    Detail_Delete_P_EntityName: 'Do you really want to delete the {0} {1}?',
    Linked_Entity_Warning: ' All related entities will be deleted!'
}

res.Workflow = {

}

res.Workflow.Alert = {
    Title_Default: 'Workflow',
    Title_Error: 'Workflow Error',
    Save_Success: 'The workflow was saved!',
    Checkout_Success: 'Successfully checked out',
    Checkin_Success: 'Successfully checked in',
    Checkout_Undo: 'Successfully undoed Checkout',
    Delete_Success: 'The Workflow was deleted!'
}

res.WorkflowMapping = {

}

res.WorkflowMapping.Alert = {
    Title_Default: 'Workflow Mapping',
    Title_Error: 'Workflow Mapping Error',
    Save_Success: 'The Workflow Mapping was saved!',
    Delete_Success: 'The Workflow Mapping was deleted!'
}

res.Equipment = {

}

res.Equipment.Alert = {
    Title_Default: 'Equipment',
    Title_Error: 'Equipment Error',
    Save_Success: 'The configured equipment was saved!'
}

res.User = {};

res.User.Alert = {
    Title_Default: 'User',
    Title_Error: 'User Error',
    Save_Success: 'The user was saved!',
    Delete_Success: 'The user was deleted!',
    GetUsername_Error: 'The username couldn\'t be generated'
}

res.Person = {};

res.Person.Alert = {
    Title_Default: 'Person',
    Title_Error: 'Person Error',
    Save_Success: 'The person was saved!',
    Delete_Success: 'The person was deleted!'
}

res.EquipmentDefinition = {};

res.EquipmentDefinition.Alert = {
    Title_Default: 'EquipmentDefinition',
    Title_Error: 'EquipmentDefinition Error',
    Save_Success: 'The EquipmentDefinition was saved!',
    Delete_Success: 'The EquipmentDefinition was deleted!',
    UpdatePrices_Success: 'The prices were updated!',
    UpdatePrice_Exernal_Success: 'The prices was updated!<div style="font-style: italic; margin-top: 10px">Please remember:<br>If you have created or changed the Client Reference ID, you have to save the equipment-definition separately.<div><div>Please press save to continue.</div>'
}

res.Enterprise = {};

res.Enterprise.Alert = {
    Title_Default: 'Enterprise',
    Title_Error: 'Enterprise Error',
    Save_Success: 'The Enterprise was saved!',
    Delete_Success: 'The Enterprise was deleted!'
}

res.EnterpriseLocation = {};

res.EnterpriseLocation.Alert = {
    Title_Default: 'Enterprise-Location',
    Title_Error: 'Enterprise-Location Error',
    Save_Success: 'The Enterprise-Location was saved!',
    Delete_Success: 'The Enterprise-Location was deleted!'
}

res.Location = {};

res.Location.Alert = {
    Title_Default: 'Location',
    Title_Error: 'Location Error',
    Save_Success: 'The Location was saved!',
    Delete_Success: 'The Location was deleted!'
}

res.Country = {};

res.Country.Alert = {
    Title_Default: 'Country',
    Title_Error: 'Country Error',
    Save_Success: 'The Country was saved!',
    Delete_Success: 'The Country was deleted!'
}

res.Role = {};

res.Role.Alert = {
    Title_Default: 'Role',
    Title_Error: 'Role Error',
    Save_Success: 'The Role was saved!',
    Delete_Success: 'The Role was deleted!'
}

res.Costcenter = {};

res.Costcenter.Alert = {
    Title_Default: 'Costcenter',
    Title_Error: 'Costcenter Error',
    Save_Success: 'The Costcenter was saved!',
    Delete_Success: 'The Costcenter was deleted!',
    Cleanup_Success: 'Costcenter relations are cleaned up.'
}

res.Orgunit = {};

res.Orgunit.Alert = {
    Title_Default: 'Orgunit',
    Title_Error: 'Orgunit Error',
    Save_Success: 'The Orgunit was saved!',
    Delete_Success: 'The Orgunit was deleted!',
    Cleanup_Success: 'Orgunits relations are cleaned up.'
}

res.Orgunit.Button = {
    ShowOrgunitTree: 'Show grid tree',
    ShowOrgunitFlatList: 'Show flat grid'
}

res.OrgunitRole = {};

res.OrgunitRole.Alert = {
    Title_Default: 'Orgunit Employment',
    Title_Error: 'Orgunit Employment Error',
    Save_Success: 'The Orgunit Employment was saved!',
    Delete_Success: 'The Orgunit Employment was deleted!'
}

res.WorkflowInstance = {};

res.WorkflowInstance.Confirmation = {

    Title_Rerun: 'Rerun workflow-instance',
    Detail_Rerun: 'Do you really want to rerun the workflow instance "{0}"'
}

res.WorkflowInstance.Alert = {
    Title_Default: 'Workflow instance',
    Title_Error: 'Workflow instance Error',
    Save_Success: 'The workflow instance was saved!',
    Rerun_Success: 'The workflow instance was started!'

}



res.PersonProfilePackage = {};

res.PersonProfilePackage.Alert = {
    Title_Default: 'Employment package',
    Title_Error: 'Package Error',
    Save_Success: 'The package was saved!',
    Delete_Success: 'The package was deleted!'
}

res.PersonProfileEquipment = {};

res.PersonProfileEquipment.Alert = {
    Title_Default: 'Employment equipment',
    Title_Error: 'Equipment Error',
    Save_Success: 'The equipment was saved!',
    Delete_Success: 'The equipment was deleted!'
}

res.AccountGroup = {};

res.AccountGroup.Alert = {
    Title_Default: 'Costcenter group',
    Title_Error: 'Costcenter group Error',
    Save_Success: 'The Costcenter group was saved!',
    Delete_Success: 'The Costcenter group was deleted!'
}

res.ContactData = {};

res.ContactData.Callnumber = {};

res.ContactData.Callnumber.Alert = {
    Title_Default: 'Call number',
    Title_Error: 'Call number Error',
    Save_Success: 'The call number was saved!',
    Delete_Success: 'The call number was deleted!'
}

res.Category = {};

res.Category.Alert = {
    Title_Default: 'Category',
    Title_Error: 'Category Error',
    Save_Success: 'The Category was saved!',
    Delete_Success: 'The Category was deleted!',
    Create_Success: 'The Category has been created!'
}
