//#region УДАЛЕНИЕ И ДОБАВЛЕНИЕ ВКЛАДКИ СО СТРАНИЦЫ РЕДАКТИРОВАНИЯ

define("ActivityPageV2", [], function () {
	return {
		entitySchemaName: "Activity",
		attributes: {},
		modules: /**SCHEMA_MODULES*/{}/**SCHEMA_MODULES*/,
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		businessRules: /**SCHEMA_BUSINESS_RULES*/{}/**SCHEMA_BUSINESS_RULES*/,
		methods:
		{
			//---------------

			init: function () {
				this.callParent(arguments);
			},
			onEntityInitialized: function () {
				this.callParent(arguments);
				
				var categoryTest = this.get("ActivityCategory");
				if (categoryTest.displayValue == "Прозвон детракторов") {
					this.TabDel(); // Вызов метода: Удаляет вкладку Email
				}
				else
				{
					this.TabAdd();
				}
			},
			
			//#region Реализация метод: Удаляет вкладку Email
			TabDel: function () {
				var tabDel = this.get("TabsCollection");
				tabDel.removeByKey("ActivityParticipantTab");
			},
			//#endregion

			//#region Реализация метод: Добавляет вкладку Email
			TabAdd: function () {
				var tabs = this.get('TabsCollection');
				tabs.insert(2, 'ActivityParticipantTab', tabs.createItem(
					{ Caption: "Участники", Name: "ActivityParticipantTab" }
				));
			},
			//#endregion

			//---------------
		},
		dataModels: /**SCHEMA_DATA_MODELS*/{}/**SCHEMA_DATA_MODELS*/,
		diff: /**SCHEMA_DIFF*/[]/**SCHEMA_DIFF*/
	};
});

//#endregion