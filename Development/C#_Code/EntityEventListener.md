# Entity Event Listener

## Описание

* Основная логика валидации данных должна находится в `BaseEntityEventListener` и не в коем случае на клиентской части.

* **Пример переопределения метода-обработчика события**

    ```CSharp
    // Слушатель событий сущности "Активность".
    [EntityEventListener(SchemaName = "Activity")] 
    public class ActivityEntityEventListener : BaseEntityEventListener
    {
        // Переопределение обработчика события сохранения сущности.
        public override void OnSaved(object sender, EntityAfterEventArgs e) {
            // Вызов родительской реализации.
            base.OnSaved(sender, e);
            //...
        }
    }
    ```

* **Атрибут `EntityEventListener`**

    Атрибут `EntityEventListener` (класс `EntityEventListenerAttribute`) предназначен для регистрации слушателя. Слушатель может быть связан со всеми объектами (`IsGlobal = true`) или с конкретным объектом (например, `SchemaName = “Contact`”). Один класс-слушатель можно помечать множеством атрибутов для определения необходимого набора “прослушиваемых” сущностей.

* **Класс `BaseEntityEventListener` предоставляет методы-обработчики различных событий сущности:**

    [Method] | [Description]
    --- | ---
    OnDeleted(object sender, Entity**After**EventArgs e) | Обработчик события **после** удаления записи.
    OnDeleting(object sender, Entity**Before**EventArgs e) | Обработчик события **перед** удалением записи.
    OnInserted(object sender, Entity**After**EventArgs e) | Обработчик события **после** добавления записи.
    OnInserting(object sender, Entity**Before**EventArgs e) | Обработчик события **перед** добавлением записи.
    OnSaved(object sender, Entity**After**EventArgs e) | Обработчик события **после** сохранения записи.
    OnSaving(object sender, Entity**Before**EventArgs e) | Обработчик события **перед** сохранением записи.
    OnUpdated(object sender, Entity**After**EventArgs e) | Обработчик события **после** обновления записи.
    OnUpdating(object sender, Entity**Before**EventArgs e) | Обработчик события **перед** обновлением записи.

* **Параметры методов:**
  * `sender` - ссылка на экземпляр сущности, генерирующий событие. Тип параметра — Object.
  * `e` - аргументы события. В зависимости от момента выполнения метода-обработчика (после или до события), тип аргумента может быть или **EntityAfterEventArgs**, или **EntityBeforeEventArgs**.

* **Последовательность вызова методов-обработчиков событий приведена в таблице.**

    [Создание] | [Изменение] | [Удаление]
    --- | --- | ---
    OnSaving() | OnSaving() | OnDeleting()
    OnInserting() | OnUpdating() | OnDeleted()
    OnInserted() | OnUpdated() | ---
    OnSaved() | OnSaved() | ---

* **Получение экземпляра `UserConnection`.**

    Экземпляр `UserConnection` в обработчиках событий необходимо получать из параметра `sender`:

```CSharp
[EntityEventListener(SchemaName = "Activity")]
public class ActivityEntityEventListener : BaseEntityEventListener
{
    public override void OnSaved(object sender, EntityAfterEventArgs e) {
        base.OnSaved(sender, e);
        var entity = (Entity) sender;
        var userConnection = entity.UserConnection;
    }
}
```

* **Класс `EntityAfterEventArgs`**

    Класс предоставляет свойства с аргументами метода-обработчика, выполняемого после возникновения события:

  * `ModifiedColumnValues` — коллекция измененных колонок.
  * `PrimaryColumnValue` — идентификатор записи.

* **Класс `EntityBeforeEventArgs`**

    Класс предоставляет свойства с аргументами метода-обработчика, выполняемого до возникновения события:

  * `KeyValue` — идентификатор записи.
  * `IsCanceled` — позволяет отменить дальнейшее выполнение события.
  * `AdditionalCondition` — позволяет дополнительно описать условия фильтрации сущности перед действием.

## Useful links

[Бизнес-логика объектов](https://academy.terrasoft.ru/docs/developer/back-end_development/entity_event_layer/sobytiynyy_sloy_obekta)

[Entity Event Layer](https://academy.creatio.com/documents/technic-sdk/7-16/entity-event-layer)

[Binding Data Packages](https://academy.creatio.com/documents/technic-sdk/7-16/binding-data-packages)