define("UsrGiftCertificateDetail", [], function() {
	return {
		entitySchemaName: "UsrGiftCertificate",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		diff: /**SCHEMA_DIFF*/[]/**SCHEMA_DIFF*/,
		methods: 
		{
			getAddRecordButtonVisible: function() { return false; }, // Удалить кнопку [+]
			getDeleteRecordMenuItem: function() { return false; },   // Удалить кнопку [Удалить]
			getCopyRecordMenuItem: function() { return false; },     // Удалить кнопку [Копировать]
			getEditRecordMenuItem: function() { return false; }      // Удалить кнопку [Изменить]
		}
	};
});
