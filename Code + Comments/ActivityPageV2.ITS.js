define("ActivityPageV2", [], function () {
	return {
		entitySchemaName: "Activity",
		attributes: {
			"DetailHasRecords": {
				"type": Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				"dataValueType": Terrasoft.DataValueType.BOOLEAN,
				"value": false
			},
			"OtherVisible": {
				"type": Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				"dataValueType": Terrasoft.DataValueType.BOOLEAN,
				"value": false
			},
			"ESQCompleted": {
				"type": Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				"dataValueType": Terrasoft.DataValueType.BOOLEAN,
				"value": false
			},
		},
		details: /**SCHEMA_DETAILS*/ {
			"UsrSchema4Detail7082ad87": {
				"schemaName": "UsrSchema4Detail",
				"entitySchemaName": "UsrReasonForComplaint",
				"filter": {
					"detailColumn": "UsrActivity",
					"masterColumn": "Id"
				}
			}
		}/**SCHEMA_DETAILS*/,
		messages: {
			"ChangeAnotherVis": {
				mode: Terrasoft.MessageMode.BROADCAST,
				direction: Terrasoft.MessageDirectionType.SUBSCRIBE
			}
		},
		methods: {
			onEntityInitialized: function () {
				this.callParent(arguments);

				document.thisPageScope = this; // это одно и тоже по факту

				var category = this.get("ActivityCategory");
				if (category.displayValue == "Прозвон детракторов") {
					this.set("NPSVisible", true);
					this.set("NonNPSVisible", false);
					this.TabsDelete(); // Вызов метода который удаляет вкладку Email

					var activity = this.get("Id");
					var self = this;
					var esq = this.Ext.create("Terrasoft.EntitySchemaQuery", { rootSchemaName: "UsrReasonForComplaint" });
					esq.addColumn("UsrCompReason", "UsrCompReason");
					esq.addColumn("UsrActivity", "UsrActivity");
					esq.filters.add("Filter1", this.Terrasoft.createColumnFilterWithParameter(this.Terrasoft.ComparisonType.EQUAL, "UsrActivity", activity));
					esq.filters.add("Filter2", this.Terrasoft.createColumnFilterWithParameter(this.Terrasoft.ComparisonType.EQUAL, "UsrCompReason.Name", "Другое"));
					esq.getEntityCollection(function (response) {
						if (response && response.success && response.collection.getCount() > 0) {
							self.set("OtherVisible", true);
							self.set("UsrOtherCommentIsReq", true);
						}
						else {
							self.set("UsrOtherCommentIsReq", false);
						}
					});
					this.sandbox.subscribe("ChangeAnotherVis", this.changeAnotherVisibile, this);
				}
				else {
					this.set("NPSVisible", false);
					this.set("NonNPSVisible", true);
					this.TabsAdd();
				}
			},

			setVisible: function () {
				var category = this.get("ActivityCategory");
				if (category.displayValue == "Прозвон детракторов") {
					this.set("NPSVisible", true);
					this.set("NonNPSVisible", false);
					this.TabsDelete();
				}
				else {
					this.set("NPSVisible", false);
					this.set("NonNPSVisible", true);
					this.TabsAdd();
				}
			},

			//#region Реализация метод: Удаляет вкладку Email
			TabsDelete: function () {
				var tabs = this.get("TabsCollection");
				tabs.removeByKey("EmailTab");
			},
            //#endregion

			//#region Реализация метод: Добавляет вкладку Email
			TabsAdd: function () {
				var tabs = this.get('TabsCollection');
				tabs.insert(1, 'EmailTab', tabs.createItem(
					{ Caption: "Email", Name: "EmailTab", }
				));
			},
			//#endregion

			//#region Реализация метод: Переключает флаги
			changeAnotherVisibile: function (args) {
				if (args == this.get("Id")) {
					this.set("OtherVisible", true);
					this.set("UsrOtherCommentIsReq", true);
					this.get("UsrOtherCommentIsReq").visible = true;
					this.get("UsrOtherCommentIsReq").required = true;
				}
			},
			//#endregion

			checkDetailRecords: function () {
				var activtId = this.get("Id").toUpperCase();
				var select = this.Ext.create("Terrasoft.EntitySchemaQuery",
					{
						rootSchemaName: "UsrReasonForComplaint" //название схемы объекта детали
					});
				select.filters.add("Filter111", this.Terrasoft.createColumnFilterWithParameter(this.Terrasoft.ComparisonType.EQUAL, "UsrActivity", activtId));
				select.getEntityCollection(function (response) {
					if (response.success) {
						var collection = response.collection;
						if (collection && response.collection.getCount() > 0) {
							this.set("DetailHasRecords", true);
							this.set("ESQCompleted", true);
							this.save();
						}
						else {
							this.set("DetailHasRecords", false);
							this.set("ESQCompleted", true);
							this.save();
						}
					}
				}, this);
			},

			updateDetailProperty: function () {
				var activtId = this.get("Id").toUpperCase();
				var select = this.Ext.create("Terrasoft.EntitySchemaQuery", { rootSchemaName: "UsrReasonForComplaint" });
				select.filters.add("Filter111", this.Terrasoft.createColumnFilterWithParameter(this.Terrasoft.ComparisonType.EQUAL, "UsrActivity", activtId));
				select.getEntityCollection(function (response) {
					if (response.success) {
						var collection = response.collection;
						if (collection && response.collection.getCount() > 0) {
							this.set("DetailHasRecords", true);
							this.set("ESQCompleted", true);
							this.save(11);
						}
						else {
							this.set("DetailHasRecords", false);
							this.set("ESQCompleted", true);
							this.save(11);
						}
					}
				}, this);
			},

			save: function (parameter) {
				var status = this.get("Status");
				var category = this.get("ActivityCategory");
				var result = this.get("Result");
				if ((status.displayValue == "Завершена") && (category.displayValue == "Прозвон детракторов") && (result.displayValue == "Ответ")) {
					if (parameter != 11) {
						this.updateDetailProperty()
					}
					else {
						if (this.get("DetailHasRecords")) {
							this.callParent();
						}
						else {
							this.showInformationDialog("Заполните деталь 'Причина жалобы'");
						}
					}
				}
				else {
					this.callParent();
				}
			}
		},
		rules: {},
		modules: /**SCHEMA_MODULES*/{}/**SCHEMA_MODULES*/,
		businessRules: /**SCHEMA_BUSINESS_RULES*/ {
			"UsrOtherComment": {
				"3e7ee95d-9b2d-4996-80bc-cb7f5b9ba0dc": {
					"uId": "3e7ee95d-9b2d-4996-80bc-cb7f5b9ba0dc",
					"enabled": true,
					"removed": false,
					"ruleType": 0,
					"property": 2,
					"logical": 0,
					"conditions": [
						{
							"comparisonType": 3,
							"leftExpression": {
								"type": 1,
								"attribute": "UsrOtherCommentIsReq"
							},
							"rightExpression": {
								"type": 0,
								"value": true,
								"dataValueType": 12
							}
						},
						{
							"comparisonType": 3,
							"leftExpression": {
								"type": 1,
								"attribute": "ActivityCategory"
							},
							"rightExpression": {
								"type": 0,
								"value": "cb46fb5d-0392-47fd-aaff-21e444714d07",
								"dataValueType": 10
							}
						}
					]
				},
				"f9c080ea-8d99-426f-99ed-105b9cc089b3": {
					"uId": "f9c080ea-8d99-426f-99ed-105b9cc089b3",
					"enabled": true,
					"removed": true,
					"ruleType": 0,
					"property": 2,
					"logical": 0,
					"conditions": [
						{
							"comparisonType": 3,
							"leftExpression": {
								"type": 1,
								"attribute": "UsrOtherCommentIsReq"
							},
							"rightExpression": {
								"type": 0,
								"value": true,
								"dataValueType": 12
							}
						}
					]
				},
				"5ac774bc-71d9-405b-866a-966e92b94271": {
					"uId": "5ac774bc-71d9-405b-866a-966e92b94271",
					"enabled": true,
					"removed": false,
					"ruleType": 0,
					"property": 0,
					"logical": 0,
					"conditions": [
						{
							"comparisonType": 3,
							"leftExpression": {
								"type": 1,
								"attribute": "UsrOtherCommentIsReq"
							},
							"rightExpression": {
								"type": 0,
								"value": true,
								"dataValueType": 12
							}
						},
						{
							"comparisonType": 3,
							"leftExpression": {
								"type": 1,
								"attribute": "ActivityCategory"
							},
							"rightExpression": {
								"type": 0,
								"value": "cb46fb5d-0392-47fd-aaff-21e444714d07",
								"dataValueType": 10
							}
						}
					]
				}
			},
			"Result": {
				"BindParameterRequiredResultToStatus": {
					"uId": "3e5e9408-932c-472f-a46b-2eb1149df4c9",
					"enabled": true,
					"ruleType": 0,
					"property": 2,
					"logical": 0,
					"conditions": [
						{
							"comparisonType": 3,
							"leftExpression": {
								"type": 1,
								"attribute": "Status",
								"attributePath": "Finish"
							},
							"rightExpression": {
								"type": 0,
								"value": true,
								"dataValueType": 12
							}
						},
						{
							"comparisonType": 3,
							"leftExpression": {
								"type": 1,
								"attribute": "IsProcessMode"
							},
							"rightExpression": {
								"type": 0,
								"value": true,
								"dataValueType": 1
							}
						},
						{
							"comparisonType": 4,
							"leftExpression": {
								"type": 1,
								"attribute": "ActivityCategory"
							},
							"rightExpression": {
								"type": 0,
								"value": "cb46fb5d-0392-47fd-aaff-21e444714d07",
								"dataValueType": 10
							}
						}
					]
				},
				"dbffdbc9-259c-441f-beeb-7f8c4086b6b3": {
					"uId": "dbffdbc9-259c-441f-beeb-7f8c4086b6b3",
					"enabled": true,
					"removed": false,
					"ruleType": 0,
					"property": 2,
					"logical": 0,
					"conditions": [
						{
							"comparisonType": 3,
							"leftExpression": {
								"type": 1,
								"attribute": "Status"
							},
							"rightExpression": {
								"type": 0,
								"value": "4bdbb88f-58e6-df11-971b-001d60e938c6",
								"dataValueType": 10
							}
						},
						{
							"comparisonType": 3,
							"leftExpression": {
								"type": 1,
								"attribute": "ActivityCategory"
							},
							"rightExpression": {
								"type": 0,
								"value": "cb46fb5d-0392-47fd-aaff-21e444714d07",
								"dataValueType": 10
							}
						}
					]
				}
			},
			"DetailedResult": {
				"BindParameterEnabledDetailedResultToStatus": {
					"uId": "d82b438e-a3af-45bf-8cc3-f825e758af5d",
					"enabled": true,
					"ruleType": 0,
					"property": 1,
					"logical": 0,
					"conditions": [
						{
							"comparisonType": 3,
							"leftExpression": {
								"type": 1,
								"attribute": "Status",
								"attributePath": "Finish"
							},
							"rightExpression": {
								"type": 0,
								"value": true,
								"dataValueType": 12
							}
						}
					]
				},
				"34d740d2-5101-4e0e-9f7d-d92a84e703fc": {
					"uId": "34d740d2-5101-4e0e-9f7d-d92a84e703fc",
					"enabled": true,
					"removed": false,
					"ruleType": 0,
					"property": 2,
					"logical": 0,
					"conditions": [
						{
							"comparisonType": 3,
							"leftExpression": {
								"type": 1,
								"attribute": "Status"
							},
							"rightExpression": {
								"type": 0,
								"value": "4bdbb88f-58e6-df11-971b-001d60e938c6",
								"dataValueType": 10
							}
						},
						{
							"comparisonType": 3,
							"leftExpression": {
								"type": 1,
								"attribute": "ActivityCategory"
							},
							"rightExpression": {
								"type": 0,
								"value": "cb46fb5d-0392-47fd-aaff-21e444714d07",
								"dataValueType": 10
							}
						}
					]
				}
			},
			"UsrReasonForNotRinging": {
				"44f26717-3cd0-4681-b9f8-adce62b55826": {
					"uId": "44f26717-3cd0-4681-b9f8-adce62b55826",
					"enabled": true,
					"removed": false,
					"ruleType": 0,
					"property": 0,
					"logical": 0,
					"conditions": [
						{
							"comparisonType": 3,
							"leftExpression": {
								"type": 1,
								"attribute": "ActivityCategory"
							},
							"rightExpression": {
								"type": 0,
								"value": "cb46fb5d-0392-47fd-aaff-21e444714d07",
								"dataValueType": 10
							}
						}
					]
				}
			}
		}/**SCHEMA_BUSINESS_RULES*/,
		dataModels: /**SCHEMA_DATA_MODELS*/{}/**SCHEMA_DATA_MODELS*/,
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "merge",
				"name": "StartDate",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 1
					}
				}
			},
			{
				"operation": "merge",
				"name": "Owner",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 1
					}
				}
			},
			{
				"operation": "move",
				"name": "Owner",
				"parentName": "Header",
				"propertyName": "items",
				"index": 3
			},
			{
				"operation": "merge",
				"name": "DueDate",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 2
					}
				}
			},
			{
				"operation": "merge",
				"name": "Author",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 2
					}
				}
			},
			{
				"operation": "move",
				"name": "Author",
				"parentName": "Header",
				"propertyName": "items",
				"index": 5
			},
			{
				"operation": "merge",
				"name": "Status",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 3
					}
				}
			},
			{
				"operation": "merge",
				"name": "Priority",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 3
					}
				}
			},
			{
				"operation": "move",
				"name": "Priority",
				"parentName": "Header",
				"propertyName": "items",
				"index": 7
			},
			{
				"operation": "merge",
				"name": "ShowInScheduler",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 4
					}
				}
			},
			{
				"operation": "move",
				"name": "ShowInScheduler",
				"parentName": "Header",
				"propertyName": "items",
				"index": 8
			},
			{
				"operation": "merge",
				"name": "ActivityCategory",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 4
					}
				}
			},
			{
				"operation": "merge",
				"name": "CallDirection",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 5
					}
				}
			},
			{
				"operation": "insert",
				"name": "UsrSmsState0bd8a7e6-43c4-4dc8-af3a-5a454e22f95d",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 6,
						"layoutName": "Header"
					},
					"bindTo": "UsrSmsState",
					"visible": {
						"bindTo": "NonNPSVisible"
					}
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 11
			},
			{
				"operation": "insert",
				"name": "UsrSmsStatusb24874af-f5d6-452b-b2a0-946987cf46af",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 6,
						"layoutName": "Header"
					},
					"bindTo": "UsrSmsStatus",
					"visible": {
						"bindTo": "NonNPSVisible"
					}
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 12
			},
			{
				"operation": "insert",
				"name": "UsrVibera676d580-aa39-4fa9-aba7-9fd3bf006759",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 7,
						"layoutName": "Header"
					},
					"visible": {
						"bindTo": "NonNPSVisible"
					},
					"bindTo": "UsrViber"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 13
			},
			{
				"operation": "insert",
				"name": "UsrSmsa2b3df58-e87b-4b4b-b4c7-c3a60d657dea",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 7,
						"layoutName": "Header"
					},
					"bindTo": "UsrSms",
					"visible": {
						"bindTo": "NonNPSVisible"
					}
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 14
			},
			{
				"operation": "insert",
				"name": "INTEGER3755ad1b-fa30-43a1-95f4-8d2237c8f05a",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 8,
						"layoutName": "Header"
					},
					"visible": {
						"bindTo": "NPSVisible"
					},
					"bindTo": "UsrAttempt",
					"enabled": false
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 15
			},
			{
				"operation": "insert",
				"name": "STRING1ef40c25-b420-4dc4-9c48-c3b2cfe6c396",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 8,
						"layoutName": "Header"
					},
					"bindTo": "UsrDetractorInf",
					"enabled": true,
					"visible": {
						"bindTo": "NPSVisible"
					},
					"contentType": 0
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 16
			},
			{
				"operation": "merge",
				"name": "ESNTab",
				"values": {
					"order": 5
				}
			},
			{
				"operation": "merge",
				"name": "GeneralInfoTab",
				"values": {
					"order": 0
				}
			},
			{
				"operation": "merge",
				"name": "CustomActionSelectedResultControlGroup",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 0
					}
				}
			},
			{
				"operation": "merge",
				"name": "Result",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 0
					}
				}
			},
			{
				"operation": "insert",
				"name": "UsrSchema4Detail7082ad87",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail",
					"visible": {
						"bindTo": "NPSVisible"
					}
				},
				"parentName": "GeneralInfoTab",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "GeneralInfoTabGroupd98ff745",
				"values": {
					"caption": {
						"bindTo": "Resources.Strings.GeneralInfoTabGroupd98ff745GroupCaption"
					},
					"itemType": 15,
					"markerValue": "added-group",
					"items": [],
					"visible": {
						"bindTo": "OtherVisible"
					}
				},
				"parentName": "GeneralInfoTab",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "GeneralInfoTabGridLayout5285bfd3",
				"values": {
					"itemType": 0,
					"items": [],
					"visible": {
						"bindTo": "OtherVisible"
					}
				},
				"parentName": "GeneralInfoTabGroupd98ff745",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "STRING0172d16f-61da-4160-b3b9-c5c07328959c",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 2,
						"column": 0,
						"row": 0,
						"layoutName": "GeneralInfoTabGridLayout5285bfd3"
					},
					"bindTo": "UsrOtherComment",
					"enabled": true,
					"contentType": 0,
					"visible": {
						"bindTo": "NPSVisible"
					}
				},
				"parentName": "GeneralInfoTabGridLayout5285bfd3",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "UsrEntityConnectionsControlBlock",
				"values": {
					"itemType": 15,
					"caption": "Связи",
					"items": [],
					"controlConfig": {
						"collapsed": false
					}
				},
				"parentName": "GeneralInfoTab",
				"propertyName": "items",
				"index": 3
			},
			{
				"operation": "insert",
				"name": "Order",
				"values": {
					"bindTo": "Order",
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 0
					}
				},
				"parentName": "UsrEntityConnectionsControlBlock",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "UsrEntityConnectionsResultsBlock",
				"values": {
					"itemType": 0,
					"items": []
				},
				"parentName": "UsrEntityConnectionsControlBlock",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "Contact",
				"values": {
					"bindTo": "Contact",
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 0
					}
				},
				"parentName": "UsrEntityConnectionsControlBlock",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "UsrCase",
				"values": {
					"bindTo": "Case",
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 3
					}
				},
				"parentName": "UsrEntityConnectionsControlBlock",
				"propertyName": "items",
				"index": 3
			},
			{
				"operation": "insert",
				"name": "Account",
				"values": {
					"bindTo": "Account",
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 1
					}
				},
				"parentName": "UsrEntityConnectionsControlBlock",
				"propertyName": "items",
				"index": 4
			},
			{
				"operation": "insert",
				"name": "ConnectionWithProjectControlGroup",
				"values": {
					"itemType": 0,
					"caption": null,
					"items": [],
					"wrapClass": [
						"connection-with-project-control-group"
					]
				},
				"parentName": "GeneralInfoTab",
				"propertyName": "items",
				"index": 4
			},
			{
				"operation": "insert",
				"name": "FullProjectName",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 0
					},
					"tip": {
						"content": {
							"bindTo": "Resources.Strings.FullProjectNameTip"
						}
					},
					"bindTo": "FullProjectName",
					"hasClearIcon": true,
					"showValueAsLink": true,
					"caption": {
						"bindTo": "Resources.Strings.FullProjectCaption"
					},
					"href": {
						"bindTo": "FullProjectName",
						"bindConfig": {
							"converter": "setProjectUrl"
						}
					},
					"controlConfig": {
						"className": "Terrasoft.TextEdit",
						"rightIconClasses": [
							"custom-right-item",
							"lookup-edit-right-icon"
						],
						"rightIconClick": {
							"bindTo": "openProjectLookup"
						},
						"linkclick": {
							"bindTo": "onFullProjectNameLinkClick"
						}
					}
				},
				"parentName": "ConnectionWithProjectControlGroup",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "UsrSmsControlBlock",
				"values": {
					"itemType": 15,
					"caption": "СМС Сообщение",
					"items": [],
					"controlConfig": {
						"collapsed": false
					},
					"visible": {
						"bindTo": "NonNPSVisible"
					}
				},
				"parentName": "GeneralInfoTab",
				"propertyName": "items",
				"index": 5
			},
			{
				"operation": "insert",
				"name": "UsrMessageIDV2",
				"values": {
					"bindTo": "UsrMessageIDV2",
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 0
					}
				},
				"parentName": "UsrSmsControlBlock",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "UsrSmsState",
				"values": {
					"bindTo": "UsrSmsState",
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 1
					},
					"visible": {
						"bindTo": "NonNPSVisible"
					}
				},
				"parentName": "UsrSmsControlBlock",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "UsrSmsResultsBlock",
				"values": {
					"itemType": 0,
					"items": [],
					"visible": {
						"bindTo": "NonNPSVisible"
					}
				},
				"parentName": "UsrSmsControlBlock",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "UsrSmsStatus",
				"values": {
					"bindTo": "UsrSmsStatus",
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 2
					},
					"visible": {
						"bindTo": "NonNPSVisible"
					}
				},
				"parentName": "UsrSmsControlBlock",
				"propertyName": "items",
				"index": 3
			},
			{
				"operation": "insert",
				"name": "UsrSms",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 4,
						"layoutName": "UsrSmsControlBlock"
					},
					"bindTo": "UsrSms"
				},
				"parentName": "UsrSmsControlBlock",
				"propertyName": "items",
				"index": 4
			},
			{
				"operation": "insert",
				"name": "UsrViber",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 3,
						"layoutName": "UsrSmsControlBlock"
					},
					"visible": {
						"bindTo": "NonNPSVisible"
					},
					"bindTo": "UsrViber"
				},
				"parentName": "UsrSmsControlBlock",
				"propertyName": "items",
				"index": 5
			},
			{
				"operation": "merge",
				"name": "RemindToOwner",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 0
					},
					"labelConfig": {
						"caption": {
							"bindTo": "Resources.Strings.RemindToOwnerLabelCaption"
						}
					}
				}
			},
			{
				"operation": "merge",
				"name": "RemindToOwnerDate",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 0
					},
					"labelConfig": {
						"caption": {
							"bindTo": "Resources.Strings.RemindToOwnerDateLabelCaption"
						}
					}
				}
			},
			{
				"operation": "merge",
				"name": "RemindToAuthor",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 1
					},
					"labelConfig": {
						"caption": {
							"bindTo": "Resources.Strings.RemindToAuthorLabelCaption"
						}
					}
				}
			},
			{
				"operation": "merge",
				"name": "RemindToAuthorDate",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 1
					},
					"labelConfig": {
						"caption": {
							"bindTo": "Resources.Strings.RemindToAuthorDateLabelCaption"
						}
					}
				}
			},
			{
				"operation": "merge",
				"name": "ActivityParticipantTab",
				"values": {
					"order": 1
				}
			},
			{
				"operation": "merge",
				"name": "ActivityFileNotesTab",
				"values": {
					"order": 2
				}
			},
			{
				"operation": "merge",
				"name": "EmailTab",
				"values": {
					"visible": {
						"bindTo": "NonNPSVisible"
					},
					"order": 3
				}
			},
			{
				"operation": "merge",
				"name": "CallTab",
				"values": {
					"order": 4
				}
			},
			{
				"operation": "insert",
				"name": "CallTabGroupade321fb",
				"values": {
					"caption": {
						"bindTo": "Resources.Strings.CallTabGroupade321fbGroupCaption"
					},
					"itemType": 15,
					"markerValue": "added-group",
					"items": []
				},
				"parentName": "CallTab",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "CallTabGridLayout3b9f1c0c",
				"values": {
					"itemType": 0,
					"items": []
				},
				"parentName": "CallTabGroupade321fb",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "STRINGe1c82707-ee9f-4520-8a7f-4ec02cff200e",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 0,
						"layoutName": "CallTabGridLayout3b9f1c0c"
					},
					"bindTo": "UsrReasonForNotRinging",
					"enabled": true
				},
				"parentName": "CallTabGridLayout3b9f1c0c",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "remove",
				"name": "EntityConnections"
			},
			{
				"operation": "move",
				"name": "InformationOnStepButtonContainer",
				"parentName": "Header",
				"propertyName": "items",
				"index": 0
			}
		]/**SCHEMA_DIFF*/
	};
});