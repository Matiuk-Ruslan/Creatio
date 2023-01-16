define("OrderSectionV2", ["ProcessModuleUtilities"], function (ProcessModuleUtilities) {
    return {
        entitySchemaName: "Order",
        details: /**SCHEMA_DETAILS*/ {} /**SCHEMA_DETAILS*/,
        diff: /**SCHEMA_DIFF*/[] /**SCHEMA_DIFF*/,
        methods: {
            isActionEnabled: function () {
                var selectedRows = this.get("SelectedRows");
                return selectedRows ? (selectedRows.length > 0) : false;
            },
            getSectionActions: function () {
                var actionMenuItems = this.callParent(arguments);
                actionMenuItems.addItem(this.getButtonMenuItem({
                    Type: "Terrasoft.MenuSeparator",
                    Caption: ""
                }));
                actionMenuItems.addItem(this.getButtonMenuItem({
                    "Caption": "Экспорт состояния заказа",
                    "Enabled": { bindTo: "isActionEnabled" },
                    "Click": { bindTo: "callProcess" },
                    "IsEnabledForSelectedAll": true
                }));
                return actionMenuItems;
            },
            callProcess: function () {
                //var contactParameter = this.get("логика выбора id из selectedRows");

                var selectedRows = this.get("SelectedRows");
                if (selectedRows.length > 0) {
                    selectedRows.forEach(function (selectedRowId) {
                        debugger;
                        var args = {
                            sysProcessName: "UsrImportOrderData" //,
                            //parameters: { ProcessSchemaContactParameter: contactParameter.value }
                        };
                        ProcessModuleUtilities.executeProcess(args);
                    }, this);
                }
            }
        }
    };
});