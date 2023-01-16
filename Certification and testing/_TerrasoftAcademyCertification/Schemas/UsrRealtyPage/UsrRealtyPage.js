define("UsrRealtyPage", ["ProcessModuleUtilities"], function(ProcessModuleUtilities) {
	return {
		entitySchemaName: "UsrRealty",
		attributes: {
			"UsrCommissionAttribute": {
				dataValueType: Terrasoft.DataValueType.FLOAT2,
				type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				value: 0,
				dependencies: [
                    {
                        columns: ["UsrPrice", "UsrOfferTypeRealty"],
                        methodName: "calculateCommission"
                    }
                ]
			},
			"UsrOfferTypeRealty": {
				lookupListConfig: {
					columns: ["UsrCommissionRate"]
				}
			}
		},
		modules: /**SCHEMA_MODULES*/{}/**SCHEMA_MODULES*/,
		details: /**SCHEMA_DETAILS*/{
			"Files": {
				"schemaName": "FileDetailV2",
				"entitySchemaName": "UsrRealtyFile",
				"filter": {
					"masterColumn": "Id",
					"detailColumn": "UsrRealty"
				}
			},
			"UsrRealtyViewDetail": {
				"schemaName": "UsrRealtyViewsDetail",
				"entitySchemaName": "UsrRealtyViews",
				"filter": {
					"detailColumn": "UsrRealty",
					"masterColumn": "Id"
				}
			}
		}/**SCHEMA_DETAILS*/,
		businessRules: /**SCHEMA_BUSINESS_RULES*/{
			"UsrNotes": {
				"96a0cb87-ae5c-4665-8123-149941997046": {
					"uId": "96a0cb87-ae5c-4665-8123-149941997046",
					"enabled": true,
					"removed": false,
					"ruleType": 0,
					"property": 2,
					"logical": 0,
					"conditions": [
						{
							"comparisonType": 7,
							"leftExpression": {
								"type": 1,
								"attribute": "UsrPrice"
							},
							"rightExpression": {
								"type": 0,
								"value": 100000,
								"dataValueType": 5
							}
						}
					]
				}
			}
		}/**SCHEMA_BUSINESS_RULES*/,
		messages: {
                "SetRealtyViews": {
                    "mode": Terrasoft.MessageMode.BROADCAST,
                    "direction": Terrasoft.MessageDirectionType.SUBSCRIBE
                }
            },
		methods: {
			onEntityInitialized: function() {
                this.callParent(arguments);
				this.calculateCommission();
				this.sandbox.subscribe("SetRealtyViews", this.updateDetailRealtyViews, this);
			},
			updateDetailRealtyViews: function(args) { 
				this.updateDetail( {detail: "UsrRealtyViewDetail"} ); 
			},
			valueValidator: function(value, field) {
				var warning = '';
				if(value < 0) { warning = this.get("Resources.Strings.WarningMessageValidator"); }
				return { invalidMessage: warning };
			},
			setValidationConfig: function() {
                this.callParent(arguments);
                this.addColumnValidator("UsrPrice", this.valueValidator);
                this.addColumnValidator("UsrArea", this.valueValidator);
            },
			calculateCommission: function() {
				var usrPrice = this.get("UsrPrice");
				if(!usrPrice) { usrPrice = 0; }
				var usrOfferTypeRealty = this.get("UsrOfferTypeRealty");
				var usrCommissionRate = 0;
				if(usrOfferTypeRealty) { usrCommissionRate = usrOfferTypeRealty.UsrCommissionRate; }
				var usrCommission = usrPrice * usrCommissionRate;
				this.set("UsrCommissionAttribute", usrCommission);
			},
			isActionEnabled: function () {
                var selectedId = this.get("Id");
                return selectedId ? true : false;
            },
			getActions: function() {
				var actionMenuItems = this.callParent(arguments);
				actionMenuItems.addItem(this.getButtonMenuItem( { Type: "Terrasoft.MenuSeparator", Caption: ""} ));
				actionMenuItems.addItem(this.getButtonMenuItem( { "Caption": "Автоматическое создание записей",
																  "Tag": "callAutomaticCreationOfRecords",
																  "Enable": {bintTo: "isActionEnabled"} } 
															  ));
				return actionMenuItems;
			},
            callAutomaticCreationOfRecords: function () {
                var realtyId = this.get("Id");
                if (realtyId) {
					var args = { sysProcessName: "UsrAutomaticCreationOfRecords", parameters: { RealtyId: realtyId } };
					ProcessModuleUtilities.executeProcess(args);
				}
            }
		},
		dataModels: /**SCHEMA_DATA_MODELS*/{}/**SCHEMA_DATA_MODELS*/,
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "insert",
				"name": "UsrName",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 0,
						"layoutName": "ProfileContainer"
					},
					"bindTo": "UsrName"
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "UsrRealtyType",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 1,
						"layoutName": "ProfileContainer"
					},
					"bindTo": "UsrRealtyType"
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "UsrOfferTypeRealty",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 2,
						"layoutName": "ProfileContainer"
					},
					"bindTo": "UsrOfferTypeRealty",
					"enabled": true,
					"contentType": 3
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "UsrArea",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 3,
						"layoutName": "ProfileContainer"
					},
					"bindTo": "UsrArea",
					"tip": {
						"content": {
							"bindTo": "Resources.Strings.UsrAreaHelp"
						}
					}
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 3
			},
			{
				"operation": "insert",
				"name": "UsrPrice",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 0,
						"layoutName": "Header"
					},
					"bindTo": "UsrPrice",
					"tip": {
						"content": {
							"bindTo": "Resources.Strings.UsrPriceHelp"
						}
					},
					"enabled": true
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "Commission",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 0,
						"layoutName": "Header"
					},
					"bindTo": "UsrCommissionAttribute",
					"tip": {
						"content": {
							"bindTo": "Resources.Strings.CommissionHelp"
						}
					},
					"enabled": true,
                    "caption": {
                        "bindTo": "Resources.Strings.CommissionCaption"
                    }
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "UsrNotes",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 1,
						"layoutName": "Header"
					},
					"bindTo": "UsrNotes"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "TabBasicInformation",
				"values": {
					"caption": {
						"bindTo": "Resources.Strings.TabBasicInformationTabCaption"
					},
					"items": [],
					"order": 0
				},
				"parentName": "Tabs",
				"propertyName": "tabs",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "UsrRealtyViewDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "TabBasicInformation",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "NotesAndFilesTab",
				"values": {
					"caption": {
						"bindTo": "Resources.Strings.NotesAndFilesTabCaption"
					},
					"items": [],
					"order": 1
				},
				"parentName": "Tabs",
				"propertyName": "tabs",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "Files",
				"values": {
					"itemType": 2
				},
				"parentName": "NotesAndFilesTab",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "NotesControlGroup",
				"values": {
					"itemType": 15,
					"caption": {
						"bindTo": "Resources.Strings.NotesGroupCaption"
					},
					"items": []
				},
				"parentName": "NotesAndFilesTab",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "Notes",
				"values": {
					"bindTo": "UsrNotes",
					"dataValueType": 1,
					"contentType": 4,
					"layout": {
						"column": 0,
						"row": 0,
						"colSpan": 24
					},
					"labelConfig": {
						"visible": false
					},
					"controlConfig": {
						"imageLoaded": {
							"bindTo": "insertImagesToNotes"
						},
						"images": {
							"bindTo": "NotesImagesCollection"
						}
					}
				},
				"parentName": "NotesControlGroup",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "merge",
				"name": "ESNTab",
				"values": {
					"order": 2
				}
			}
		]/**SCHEMA_DIFF*/
	};
});
