define("UsrRealty1Page", [], function() {
	return {
		entitySchemaName: "UsrRealty",
		attributes: {},
		modules: /**SCHEMA_MODULES*/{}/**SCHEMA_MODULES*/,
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		businessRules: /**SCHEMA_BUSINESS_RULES*/{}/**SCHEMA_BUSINESS_RULES*/,
		methods: {
			showInfo: function(){
				alert("Hello!");
			}
		},
		dataModels: /**SCHEMA_DATA_MODELS*/{}/**SCHEMA_DATA_MODELS*/,
		diff: /**SCHEMA_DIFF*/[
			{ }, // Поле 1
			{ }, // Поле 2
			{
				"operation": "insert",
				"parentName": "ActionButtonsContainer",
				"propertyName": "items",
				"name": "TestButton",
				"values": {
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"caption": "Нажмите на эту кнопку",
					"click": { "bindTo": "showInfo" },
                    "style": Terrasoft.controls.ButtonEnums.style.RED,
					"enabled": true
				}
			}
		]/**SCHEMA_DIFF*/
	};
});
