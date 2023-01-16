define("ActivitySectionV2", ["ConfigurationConstants"],
    function (ConfigurationConstants) {
        return {
            // Название схемы раздела
            entitySchemaName: "Activity",
            // Методы модели представления раздела
            methods: {

                // Определяет, будет ли доступен пункт меню
                isCustomActionEnabled: function () {
                    // Попытка получить массив идентификаторов выбранных записей
                    var selectedRows = this.get("SelectedRows");
                    // Если массив содержит элементы (выбрана хотя бы одна запись в реестре), то возвращается true, иначе — false
                    return selectedRows ? (selectedRows.length > 0) : false;
                },

                // Метод-обработчик действия. Устанавливает для выбранных записей статус[Выполнено]
                setAllDone: function () {
                    // Получение массива идентификаторов выбранных записей.
                    var selectedRows = this.get("SelectedRows");
                    // Обработка запускается в случае, если выбрана хотя бы одна запись.
                    if (selectedRows.length > 0) {
                        // Создание экземпляра класса пакетных запросов
                        var batchQuery = this.Ext.create("Terrasoft.BatchQuery");
                        // Обновление каждой из выбранных записей
                        selectedRows.forEach(function (selectedRowId) {
                            // Создание экземпляра класса UpdateQuery с корневой схемой Activity
                            var update = this.Ext.create("Terrasoft.UpdateQuery", {
                                rootSchemaName: "Activity"
                            });
                            // Применение фильтра для определения записи для обновления
                            update.enablePrimaryColumnFilter(selectedRowId);
                            // Для колонки Status устанавливается значение "Выполнено" с помощью конфигурационной константы
                            ConfigurationConstants.Activity.Status.Done.update.setParameterValue("Status", ConfigurationConstants.Activity.Status.Done, this.Terrasoft.DataValueType.GUID);
                            // Добавление запроса на обновление записи в пакетный запрос
                            batchQuery.add(update);
                        }, this);
                        // Выполнение пакетного запроса к серверу
                        batchQuery.execute(function () {
                            // Обновление реестра
                            this.reloadGridData();
                        }, this);
                    }
                },
                // Переопределение базового виртуального метода, возвращающего коллекцию действий раздела
                getSectionActions: function () {
                    // Вызывается родительская реализация метода, возвращающая коллекцию проинициализированных действий раздела
                    var actionMenuItems = this.callParent(arguments);
                    // Добавление линии-разделителя
                    actionMenuItems.addItem(this.getButtonMenuItem({
                        Type: "Terrasoft.MenuSeparator",
                        Caption: ""
                    }));
                    // Добавление пункта меню в список действий раздела
                    actionMenuItems.addItem(this.getButtonMenuItem({
                        // Привязка заголовка пункта меню к локализуемой строке схемы
                        "Caption": { bindTo: "Resources.Strings.AllDoneCaption" },
                        // Привязка метода-обработчика действия
                        "Click": { bindTo: "setAllDone" },
                        // Привязка свойства доступности пункта меню к значению, которое возвращает метод isCustomActionEnabled
                        "Enabled": { bindTo: "isCustomActionEnabled" },
                        // Поддержка режима множественного выбора
                        "IsEnabledForSelectedAll": true
                    }));
                    // Возврат дополненной коллекции действий раздела
                    return actionMenuItems;
                }
            }
        };
    });