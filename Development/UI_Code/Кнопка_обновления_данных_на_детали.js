define("UsrPriceAndBalanceInStoreDetail", [], function() {
	return {
		entitySchemaName: "UsrPriceAndBalanceInStore",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "insert",
				"parentName": "Detail",
				"propertyName": "tools",
				"name": "RefreshButton",
				"values": {
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"click": {"bindTo": "refresh"},
					"style": Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
					"clickDebounceTimeout": 2000,
					"caption": "Обновить данные"
				}
			}
			]/**SCHEMA_DIFF*/,
		methods: 
		{
			refresh: function() {
				this.updateDetail({ reloadAll: true });
			}
		}
	};
});
