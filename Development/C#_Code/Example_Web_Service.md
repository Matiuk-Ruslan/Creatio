# Example_Web_Service

## Описание

* **Пример GET метода (Нужна авторизация/ Не анонимный)**

    ```CSharp
    [WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]

    public class Example_Web_Service : BaseService
    {
        [OperationContract]
        [WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]

        public string GetMethodName()
        {
            return "Hello World";
        }
    }
    ```

    Вызов через: `[Адрес приложения]/0/rest/[Имя класса]/[Имя метода]`
  
  * RequestFormat - Формат запроса
  * ResponseFormat - Формат ответа
    * Json - возвращает значение в json формате
    * Xml - возвращает значение в xml формате
  * BodyStyle - обертка ответа
    * Wrapped - Возвращает -> `[ИМЯ МЕТОДА]Result : значение`
    * Bare - Возвращает -> `значение`

## Доступ к данным в базе данных

* ORM - Быстро, но игнорирует права доступа (Используется если нужна скорость обработки)
* Entity - Долго, но учитывает все права доступа (Для не высоконагруженных операций где необходимо учивать права доступа)

## Useful links

[Пользовательские веб-сервисы](https://academy.terrasoft.ru/docs/developer/back-end_development/configuration_web_service/konfiguracionnye_veb-servisy)

[ServiceContractAttribute Class](https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.servicecontractattribute?redirectedfrom=MSDN&view=dotnet-plat-ext-5.0)

[AspNetCompatibilityRequirementsAttribute Class](https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.activation.aspnetcompatibilityrequirementsattribute?redirectedfrom=MSDN&view=netframework-4.8)

[Конечные точки: адреса, привязки и контракты](https://docs.microsoft.com/ru-ru/dotnet/framework/wcf/feature-details/endpoints-addresses-bindings-and-contracts)

[OperationContractAttribute Class](https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.operationcontractattribute?redirectedfrom=MSDN&view=dotnet-plat-ext-5.0)

[WebInvokeAttribute Class](https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.web.webinvokeattribute?redirectedfrom=MSDN&view=netframework-4.8)

[DataContractAttribute Class](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.datacontractattribute?redirectedfrom=MSDN&view=net-5.0)

[DataMemberAttribute Class](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.datamemberattribute?redirectedfrom=MSDN&view=net-5.0)

[Аутентификация](https://academy.terrasoft.ru/docs/developer/integrations_and_api/request_authentication/autentifikaciya_zaprosov#reference-2298)
