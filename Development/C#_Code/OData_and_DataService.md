# OData and DataService

## DataService

### SelectQuery

#### Важные параметры

`filters` - фильтры. Если удалить из запроса, то вернется все без фильтров.
`columns` - колонки
`UseLocalization` - Определяет, будут ли использоваться локализованные данные
`UseRecordDeactivation` - Определяет, будут ли данные исключены из фильтрации. Использование деактивированных записей
`ignoreDisplayValues` - Определяет, будут ли в запросе использоваться отображаемые значения колонок

#### Можно убрать из запроса

* `rowCount` - Количество выбираемых записей
  * Максимальное количество записей, которые можно получить по запросу, задается настройкой `[MaxEntityRowCount]` (по умолчанию - 20 000). Изменить значение настройки можно в файле `.\Terrasoft.WebApp\Web.config`.
* `rowsOffset` - Количество строк, которые необходимо пропустить при возврате результата запроса (с какой записи мне это начинать)
* `allColumns` - Признак выбора всех колонок. Если значение установлено как `true`, в результате выполнения запроса будут выбраны все колонки корневой схемы.
* `isDistinct` - Признак, указывающий, убирать или нет дубли в результирующем наборе данных.
* `isPageable` - Признак постраничной выборки данных.
* `ServerESQCacheParameters` - Параметры кэширования EntitySchemaQuery на сервере
  * `CacheLevel` - Уровень кеширования,
  * `CacheGroup` - Группа кеширования,
  * `CacheItemName` - Ключ записи в хранилище
* `IsHierarchical` - Признак иерархической выборки данных
* `ConditionalValues` - Условия для построения постраничного запроса

## OData

Недостатки:

* При первом запросе строится `Entity Data Model`, потому первый запрос может быть дольше последующих
* Через OData нельза получить агрегируемые данные, как это можно сделать через DataService
* Нет возможности выбрать локализацию. Тянет по умолчанию локализацию ученой записи через которую подлючается пользователь системы

## Useful links

[DataService](https://academy.terrasoft.ru/documents/technic-sdk/7-15/dataservice)

[Доступ к данным через ORM](https://academy.terrasoft.ru/docs/7-16/developer/back-end_development/operatsii_s_dannymi_back_end/dostup_k_dannym_cherez_orm)