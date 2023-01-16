# Clio / NuGet package [CLI interface for Creatio]

`Clio` - инструмент с помощью которого мы можем работать с пакетами Creatio через терминал.

## **Установка**

1. Установить глобально для всех пользователей

    ```BASH
    dotnet tool install clio -g
    ```

2. Справка по командам и параметрам в терминале

    ```BASH
    clio help
    ```

## **Настройка**

1. Отобразить список сред / Environment

    ```BASH
    clio apps
    ```

2. Отображение содержимого файла по адресу: `C:\Users\[ПОЛЬЗОВАТЕЛЬ_СИСТЕМЫ_WINDOWS]\AppData\Local\creatio\clio\appsettings.json`

3. Заполнить файл по примеру:

    ``` JSON
    {
      "ActiveEnvironmentKey": "Sandbox_1",  // КЛЮЧ_ОПРЕДЕЛЯЕТ_КАКАЯ_СРЕДА_АКТИВНА !!!
      "Autoupdate": false,
      "Environments": {
        "Sandbox_1": {                      // НАЗВАНИЕ_СРЕДЫ
          "Uri": "http://localhost:1001",   // АДРЕС_ПРИЛОЖЕНИЯ
          "Login": "Supervisor",            // ЛОГИН_СУПЕРПОЛЬЗОВАТЕЛЯ
          "Password": "Supervisor",         // ПАРОЛЬ_СУПЕРПОЛЬЗОВАТЕЛЯ
          "IsNetCore": false
        }//, // МОЖНО_ДОБАВЛЯТЬ_НЕСКОЛЬКО_СРЕД
        // "Sandbox_2": {                      // НАЗВАНИЕ_СРЕДЫ
        //   "Uri": "http://localhost:1002",   // АДРЕС_ПРИЛОЖЕНИЯ
        //   "Login": "Supervisor",            // ЛОГИН_СУПЕРПОЛЬЗОВАТЕЛЯ
        //   "Password": "Supervisor",         // ПАРОЛЬ_СУПЕРПОЛЬЗОВАТЕЛЯ
        //   "IsNetCore": false
        // }
      }
    }
    ```

4. Подключить `Clio` к среде которая указана в `ActiveEnvironmentKey`

    ```BASH
    clio ping
    ```

5. Установить `СlioGate` на среду которая указана в `ActiveEnvironmentKey`

    ```BASH
    clio install-gate
    ```

    Это пакет с определенными веб-сервисами, с помощью которых устанавливается связь между Clio и Creatio.
    Добавляет пакет `cliogate` по адресу `С:\inetpub\wwwroot\[Папка приложения]\Terrasoft.WebApp\Terrasoft.Configuration\Pkg\`.

6. Проверяем пакеты активной среды разработки

    ```BASH
    clio packages
    ```

7. Очистка Redis

    ```BASH
    clio flushdb
    ```

8. Перезапуск приложения

    ```BASH
    clio restart
    ```

## **Основные комманды**

  Command | Short_command | Description | Example
  --- | --- | --- | ---
  clio new-pkg | init | Create new creatio package in local file system | `clio init NAME_PACKAGE`
  clio pull-pkg | download | Download package to from web-application. | `clio download NAME_PACKAGE -e NAME_ENVIRONMENT`
  [clio restart-web-app](https://github.com/Advance-Technologies-Foundation/clio/wiki/restart-web-app) | restart | Forcible restart a web application. | `clio restart`
  [clio clear-redis-db](https://github.com/Advance-Technologies-Foundation/clio/wiki/clear-redis-db) | clio flushdb | Forcible clear a web-application cache.  Clear redis database. | `clio flashdb`
  clio execute-sql-script | sql | Executes custom SQL script on a web-application | `clio sql "select id, name from contact"`
  [clio show-web-app-list](https://github.com/Advance-Technologies-Foundation/clio/wiki/show-web-app-list) | apps | View list of all register web-application | `clio apps`
  clio open-web-app | open | Open web-application in web-browser | clio open
  [clio build-workspace](https://github.com/Advance-Technologies-Foundation/clio/wiki/build-workspace) | --- | Compile configuration assembly | `clio build-workspace`
  [clio generate-pkg-zip](https://github.com/Advance-Technologies-Foundation/clio/wiki/generate-pkg-zip) | compress | Prepare an archive of creatio package | `clio NAME_PACKAGE -d DESTINATION_PATH_FOR_RESULT_FILE\NAME_PACKAGE.zip`
  [clio push-pkg](https://github.com/Advance-Technologies-Foundation/clio/wiki/push-pkg) | install | Install a package to a web-application. | `clio install NAME_PACKAGE -e NAME_ENVIRONMENT`
  [clio set-pkg-version](https://github.com/Advance-Technologies-Foundation/clio#set-package-version) | Set package version | Install a package to a web-application. | `clio set-pkg-version <PACKAGE PATH> -v <PACKAGE VERSION>`

## **Useful links**

* [Documentation for `Clio`](https://github.com/Advance-Technologies-Foundation/clio)
