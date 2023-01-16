 define("ClientMessageBridge", ["ConfigurationConstants"],
    function(ConfigurationConstants) {
        return {
            messages: {
                "SetRealtyViews": {
                    "mode": Terrasoft.MessageMode.BROADCAST,
                    "direction": Terrasoft.MessageDirectionType.PUBLISH
                }
            },
            methods: {
                init: function() {
                    this.callParent(arguments);
                    this.addMessageConfig({ sender: "SetRealtyViews", messageName: "SetRealtyViews" });
                },
                afterPublishMessage: function(sandboxMessageName, webSocketBody, result, publishConfig) {
                    if (sandboxMessageName === "SetRealtyViews") { window.console.info("Бизнес-процесс [UsrAutomaticCreationOfRecords] успешно отработал"); }
                }
            }
        };
    });