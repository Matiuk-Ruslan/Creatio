# Package

## **Content of the new package, created in the configuration Creatio**

* **Assemblies** / Внешние сборки
* **Data** / Данные
* **Resourses** / Ресурсы
* **Shemas** / Схемы
* **SqlScript** / SQL сценарии
* ``descriptor.json``

## **Content of the new package created in Clio**

* **Assemblies**
* **Data**
* **Files**
* **Properties**
* **Resourses**
* **Shemas**
* **SqlScript**
* ``descriptor.json``
* NAME_PACKAGE.csproj
* NAME_PACKAGE.sln
* package.config

---

**Содержимое файла `descriptor.json`:**

**Важно:** Содержимое этого файла запрещенно редактировать **руками**.

``` JSON
{
    "Descriptor": {
    "UId": "56dded8f-7032-4546-ac15-3d4f29be8fe1", // ID_ПАКЕТА
    "PackageVersion": "7.8.0",  // ВЕРСИЯ_ПАКЕТА
    "Name": "NAME_PACKAGE",  // ИМЯ_ПАКЕТА
    "ModifiedOnUtc": "\/Date(1621867154000)\/",  //ДАТА_И_ВРЕМЯ_ПОСЛЕДНЕГО_ИЗМЕНЕНИЯ
    "Maintainer": "Customer", // СОПРОВОЖДАЮЩИЙ_ПАКЕТ_РАЗРАБОТКИ
    "Description": "Some kind of description", // ОПИСАНИЕ_ПАКЕТА
    "DependsOn": [] // ЗАВИCИТ_ОТ_ПАКЕТОВ 
    }
}
```
