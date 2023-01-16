using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Activation;
using System.Runtime.Serialization;
using System.Net;
using System.Text;
using System.IO;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Column = Terrasoft.Core.DB.Column;
using Terrasoft.Core.Entities;
using Newtonsoft;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Terrasoft.Core.Process;
using Terrasoft.Web.Common;
using EVA.Files.cs;

namespace EVA.Files
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class UsrEvaBPMService : BaseService
    {
		#region Поля для получения прав доступа

		private AppConnection appConnection;
		private UserConnection userConnection;

		#endregion

		private Response response;

		#region Конструктор с получение прав доступа SystemUserConnection

		/// <summary> Конструктор с получением прав доступа SystemUserConnection </summary>
		public UsrEvaBPMService()
		{
			appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
			userConnection = appConnection.SystemUserConnection;
		}

		#endregion

		#region Конструктор с получение прав доступа UserConnection

		/// <summary> Конструктор с получением прав доступа UserConnection </summary>
		public UsrEvaBPMService(UserConnection userConnection)
		{
			this.userConnection = userConnection;
		}

		#endregion

		#region Метод NewOrder, который получает на вход order_id. Обращаемся из внешнего мира по адрессу: {{SiteAddress}}/0/ServiceModel/UsrEvaBPMService.svc/NewOrder

		/// <summary> Метод для получения данных из Magento </summary>
		/// <param name="request"> Приходит от Magento {order_id} </param>
		/// <returns> Возвращает свойства класса Response </returns>
		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		public Response NewOrder(NewOrderRequest request)
		{
			response = new Response();
			response.success = true;

			#region Читаем SysSettings

			var url = String.Empty;
			var tokenSys = String.Empty;
			try
			{
				url = Convert.ToString(Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "UsrUrlGetImOrderId"));
			}
			catch (System.Exception ex)
			{
				response.success = false;
				response.error = "SysSetting UsrUrlGetImOrderId was not found";
				return response;
			}
			try
			{
				tokenSys = Convert.ToString(Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "UsrTokenForNewOrder"));
			}
			catch (Exception ex)
			{
				response.success = false;
				response.error = "SysSetting UsrTokenForNewOrder was not found";
				return response;
			}

			#endregion

			#region Инициируем GET запрос к Magento

			url = String.Format(url, request.order_id);
			var token = tokenSys;
			try
			{
				var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
				httpWebRequest.ContentType = "application/json";
				httpWebRequest.Headers.Add("Authorization", "Bearer " + token.Replace("\"", ""));
				httpWebRequest.Headers.Add("Cache-Control", "no-cache");
				httpWebRequest.Method = "GET";

				var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
				{
					var result = streamReader.ReadToEnd();
					Logger.Log("RESPONSE UsrEvaBPMService: " + result.ToString(), userConnection, "NewOrder Service " + request.order_id.ToString());

					var orderFromServer = JsonConvert.DeserializeObject<GetOrderResponse>(result.ToString());
					
					#region Инициируем вызов метода ProcessOrderRequest()

					ProcessOrderRequest(orderFromServer, request.order_id, ref response);

					#endregion
					
					response.success = true;
					response.error = null;
					return response;
				}
			}
			catch (WebException ex)
			{
				if (ex.Response != null)
				{
					using (var stream = ex.Response.GetResponseStream())
					using (var reader = new StreamReader(stream))
					{
						Logger.Log("RESPONSE UsrEvaBPMService: " + reader.ReadToEnd().ToString(), userConnection, "NewOrder Service error");
					}
				}
				else
				{
					Logger.Log(ex.Message, userConnection, "NewOrder UsrEvaBPMService Service error - geting response");
				}
				response.success = false;
				response.error = ex.Message;
			}
			catch (Exception ex)
			{
				response.success = false;
				response.error = ex.Message;
				Logger.Log(ex.Message, userConnection, "NewOrder UsrEvaBPMService Service error");
				return response;
			}
			return response;

			#endregion

		}

		#endregion

		#region Метод OrderUpdate

		// TODO: Шаг 10. MasterCard. Перенос OrderUpdate() [✓]

		/// <summary> Метод для получения данных из Magento и обновления данных про существующему заказу </summary>
		/// <param name="request"> Приходит от Magento {order_id} </param>
		/// <returns></returns>
		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		public Response OrderUpdate(NewOrderRequest request)
		{
			response = new Response();
			response.success = true;

			#region Читаем и записываем SysSettings

			var url = String.Empty;
			var tokenSys = String.Empty;
			var testResponce = String.Empty;

			try
			{
				url = Convert.ToString(Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "UsrUrlGetImOrderId"));
			}
			catch (Exception ex)
			{
				response.success = false;
				response.error = "SysSetting UsrUrlGetImOrderId was not found";
				return response;
			}
			try
			{
				tokenSys = Convert.ToString(Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "UsrTokenForNewOrder"));
			}
			catch (Exception ex)
			{
				response.success = false;
				response.error = "SysSetting UsrTokenForNewOrder was not found";
				return response;
			}

			#endregion

			#region Инициируем GET запрос к Magento

			url = String.Format(url, request.order_id);
			testResponce = Convert.ToString(Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "UsrTestOrderRequest"));
			var token = tokenSys;

			try
			{
				var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
				httpWebRequest.ContentType = "application/json";
				httpWebRequest.Headers.Add("Authorization", "Bearer " + token.Replace("\"", ""));
				httpWebRequest.Headers.Add("Cache-Control", "no-cache");
				httpWebRequest.Method = "GET";

				var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
				{
					var result = streamReader.ReadToEnd();
					Logger.Log("RESPONSE: " + result.ToString(), userConnection, "OrderUpdateService " + request.order_id.ToString());
					var orderFromServer = JsonConvert.DeserializeObject<GetOrderResponse>(result.ToString());
					UpdateOrder(orderFromServer, request.order_id);
					response.success = true;
					response.error = null;
					return response;
				}
			}
			catch (WebException ex)
			{
				if (ex.Response != null)
				{
					using (var stream = ex.Response.GetResponseStream())
					using (var reader = new StreamReader(stream))
					{
						Logger.Log("RESPONSE: " + reader.ReadToEnd().ToString(), userConnection, "OrderUpdate Service error");
					}
				}
				else
				{
					Logger.Log(ex.Message, userConnection, "OrderUpdate Service error");
				}
				response.success = false;
				response.error = ex.Message;
			}
			catch (Exception ex)
			{
				response.success = false;
				response.error = ex.Message;
				Logger.Log(ex.Message, userConnection, "OrderUpdate Service error");
				return response;
			}
			return response;

			#endregion
		}

		#endregion

		#region Реализация метода ProcessOrderRequest()

		/// <summary> Метод который вызывает ProcessCustomerV2() и ProcessOrder() </summary>
		/// <param name="request"> Тело ответа на GET запрос к Magento </param>
		/// <param name="orderId"> request.order_id </param>
		/// <param name="response"> Свойства класса Response </param>
		public void ProcessOrderRequest(GetOrderResponse request, long orderId, ref Response response)
		{
			try
			{
				var processError = String.Empty;
				var orderCustomerId = Guid.Empty;

				#region Инициируем вызов метода ProcessCustomerV2()

				processError = ProcessCustomerV2(request, ref orderCustomerId);
				if (!String.IsNullOrEmpty(processError))
				{
					response.error = processError;
					response.success = false;
					return;
				}

				#endregion

				#region Инициируем вызов метода ProcessOrder()

				processError = ProcessOrder(request, orderId, orderCustomerId);
				if (!String.IsNullOrEmpty(processError))
				{
					response.error = processError;
					response.success = false;
					return;
				}

				#endregion

			}
			catch (Exception ex)
			{
				Logger.Log(ex.Message, userConnection, "NewOrder UsrEvaBPMService Service - ProcessOrderRequest orderId=" + orderId.ToString());
			}

		}

		#endregion

		#region Реализация метода ProcessOrder()

		/// <summary> Метод ProcessOrder создает экземпляр Order в БД </summary>
		/// <param name="request"> Тело ответа на GET запрос к Magento </param>
		/// <param name="orderId"> request.order_id </param>
		/// <param name="contactId"> Contact.Id </param>
		/// <returns> Возвращает String.Empty или Exception </returns>
		public string ProcessOrder(GetOrderResponse request, long orderId, Guid contactId)
		{

			#region Создание строки в таблице Order

			var entity = userConnection.EntitySchemaManager.GetInstanceByName("Order").CreateEntity(userConnection);
			var orderPorductError = (string)null;
			var isForUpdate = false;
			var orderEntityId = Guid.Empty;
			var statusId = Guid.Empty;
			var orderCancelReason = Guid.Empty;
			var createdOn = DateTime.Now;
			var invoiceExpirationDate = DateTime.Now;
			var isCreatedOnValid = false;
			var isInvoiceExpirationDateValid = false;
			isInvoiceExpirationDateValid = DateTime.TryParse(request.extension_attributes.invoice_expiration_date, out invoiceExpirationDate);
			isCreatedOnValid = DateTime.TryParse(request.extension_attributes.created_at_eet, out createdOn);
			var productEntityId = Guid.Empty;
			var optReliabilityId = GetLookupBpmIdByString("UsrCustomerReliability", "Name", "ОПТ", "Id");
			var contactReliabilityId = GetLookupBpmIdByGuid("Contact", "Id", contactId, "UsrReliabilityid");
			if (contactReliabilityId == optReliabilityId)
			{
				statusId = GetLookupBpmIdByString("OrderStatus", "Name", "Отменен", "Id");
				orderCancelReason = GetLookupBpmIdByString("UsrOrderCancelReason", "Name", "Оптовый покупатель", "Id");
			}
			else
			{
				statusId = GetLookupBpmIdByString("OrderStatus", "UsrCodeIM", request.state, "Id");
			}
			var paymentStatusId = GetLookupBpmIdByString("OrderPaymentStatus", "UsrPaymentStatusCode", request.extension_attributes.wfp_transaction_status, "Id");
			var parentId = request.original_increment_id != null ? GetLookupBpmIdByString("Order", "Number", request.original_increment_id, "Id") : Guid.Empty;
			var parentUpdateId = request.relation_parent_real_id != null ? GetLookupBpmIdByString("Order", "Number", request.relation_parent_real_id, "Id") : Guid.Empty;
			var city = request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes != null ? request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes.city : String.Empty;
			var deliveryServiceId = GetLookupBpmIdByString("UsrDeliveryService", "UsrIMCode", request.extension_attributes.shipping_assignments[0].shipping.method, "Id");
			var deliveryTypeId = GetLookupBpmIdByString("UsrDeliveryService", "UsrIMCode", request.extension_attributes.shipping_assignments[0].shipping.method, "UsrDeliveryTypeId");
			var orderProductId = Guid.Empty;
			var repackagedStatusId = GetLookupBpmIdByString("OrderStatus", "UsrCode", "6", "Id");
			string webLang = request.store_name;
			var separateWebLang = webLang.Split('\n');
			var typeId = new Guid("BD36F392-3143-4251-829B-7BB5F65EBAF9");// Клиентский
			
			try
			{
				isForUpdate = entity.FetchFromDB("UsrId", orderId);
			}
			catch (Exception ex)
			{
				return "There are more than one order with the same id";
			}
			
			if (isForUpdate)
			{
				return String.Empty;
            }

			#region [✓] Шаг 4. MasterCard. Вызов IsMasterCardDiscountActive() из NewOrder()

			// TODO: [✓] Шаг 4. MasterCard. Вызов IsMasterCardDiscountActive() из NewOrder()
			decimal? discountByProduct = default;
			string isActive = IsMasterCardDiscountActive(request, orderId, ref discountByProduct);
			if (isActive == "Коэффициент не попал в рамки допустимого" && discountByProduct == null)
			{
				statusId = GetLookupBpmIdByString("OrderStatus", "Name", "Ошибка оплаты", "Id");
				entity.SetColumnValue("Notes", $"Коэффициент не попал в рамки допустимого");
				discountByProduct = default;
			}

			#endregion

			orderEntityId = Guid.NewGuid();
			entity.SetDefColumnValues();
			entity.SetColumnValue("Id", orderEntityId);
			entity.SetColumnValue("UsrId", orderId);
			entity.SetColumnValue("CreatedOn", isCreatedOnValid == true ? createdOn : DateTime.Now);
			entity.SetColumnValue("StatusId", statusId != Guid.Empty ? statusId : (Guid?)null);
			entity.SetColumnValue("UsrOrderCancelReasonId", orderCancelReason != Guid.Empty ? orderCancelReason : (Guid?)null);
			entity.SetColumnValue("UsrWebsiteCode", request.store_id.ToString());
			entity.SetColumnValue("UsrWebsiteLanguage", separateWebLang[2]);
			entity.SetColumnValue("UsrPaymentMethod", request.payment.method);
			entity.SetColumnValue("UsrInvoice", request.extension_attributes.invoice);
			entity.SetColumnValue("UsrInvoiceExpireDate", isInvoiceExpirationDateValid == true ? invoiceExpirationDate : DateTime.Now);
			entity.SetColumnValue("UsrWfpOrder", request.extension_attributes.wfp_order_id);
			entity.SetColumnValue("ContactId", contactId);
			entity.SetColumnValue("ContactNumber", request.extension_attributes.shipping_assignments[0].shipping.address.telephone);
			entity.SetColumnValue("ReceiverName", String.Join(" ", new string[] { request.extension_attributes.shipping_assignments[0].shipping.address.lastname, request.extension_attributes.shipping_assignments[0].shipping.address.firstname, request.extension_attributes.shipping_assignments[0].shipping.address.middlename }));
			//entity.SetColumnValue("PaymentStatusId", GetOrderPaymentStatus(request.items));
			entity.SetColumnValue("PaymentStatusId", paymentStatusId != Guid.Empty ? paymentStatusId : (Guid?)null);
			entity.SetColumnValue("Number", request.increment_id);
			entity.SetColumnValue("UsrParentId", parentId != Guid.Empty ? parentId : (Guid?)null);
			entity.SetColumnValue("UsrDeliveryServiceId", deliveryServiceId != Guid.Empty ? deliveryServiceId : (Guid?)null);
			entity.SetColumnValue("DeliveryTypeId", deliveryTypeId != Guid.Empty ? deliveryTypeId : (Guid?)null);
			entity.SetColumnValue("UsrWeight", request.weight);
			entity.SetColumnValue("UsrNotificationNumber", CleanPhone(request.billing_address.telephone));
			//entity.SetColumnValue("UsrAddress", GetOrderStreet(request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes != null ? request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes.street.name : String.Empty));
			//entity.SetColumnValue("UsrHouse", request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes != null ? request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes.street.house_number : String.Empty);
			//entity.SetColumnValue("UsrApartment", request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes != null ? request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes.street.apt_number : String.Empty);
			entity.SetColumnValue("UsrAddress", GetOrderStreet(request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes.street.name));
			entity.SetColumnValue("UsrHouse", request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes.street.house_number);
			entity.SetColumnValue("UsrApartment", request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes.street.apt_number);
			entity.SetColumnValue("UsrCity", city);
			entity.SetColumnValue("UsrTypeId", typeId != Guid.Empty ? typeId : (Guid?)null);
			entity.SetColumnValue("Amount", request.grand_total);
			entity.SetColumnValue("PaymentAmount", request.grand_total);
			entity.SetColumnValue("UsrCall", request.extension_attributes.call == 1 ? true : false);
			entity.SetColumnValue("UsrCityCode", request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes != null ? request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes.city_id : String.Empty);
			entity.SetColumnValue("UsrStreetCode", request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes != null ? request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes.street_id : String.Empty);
			entity.SetColumnValue("UsrRegionCode", request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes != null ? request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes.region_id : String.Empty);
			entity.SetColumnValue("UsrDeliveryDepartment", request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes != null ? request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes.warehouse_number : String.Empty);
			entity.SetColumnValue("Comment", request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes != null ? request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes.comment : String.Empty);
			entity.SetColumnValue("UsrOperationDelivery", request.operation_id);
			entity.SetColumnValue("UsrCustomerEmail", request.customer_email);

			#region [✓] Шаг 6. MasterCard. Запись discountByProduct в Order.Notes
			// TODO: [✓] Шаг 6. MasterCard. Запись discountByProduct в Order.Notes
			if (isActive == String.Empty && discountByProduct != null)
			{
				var newShippingAmount = request.shipping_amount - (request.shipping_amount * discountByProduct);
				entity.SetColumnValue("UsrCostDelivery", newShippingAmount);
				entity.SetColumnValue("Notes", "Применена скидка по акции от MasterCard");
			}
			else
			{
				entity.SetColumnValue("UsrCostDelivery", request.shipping_amount);
			}
			#endregion

			#endregion

			try
			{
				entity.Save(false);
				orderEntityId = entity.GetTypedColumnValue<Guid>("Id");

				if (parentUpdateId != Guid.Empty)
				{

					#region Инициируем вызов метода UpdateTableStringColumn() (Ставим состояние "Пересобран" родительскому заказу)

					UpdateTableLookupColumn("Order", "StatusId", parentUpdateId, repackagedStatusId); // пересобран

					#endregion

					#region Вызов бизнес-процесса: UsrInformingClientIfOrderRebuiltEVA / Информирование клиента если заказ пересобран EVA
					//var manager = userConnection.ProcessSchemaManager;
					//var processSchema = manager.GetInstanceByName("UsrInformingClientIfOrderRebuiltEVA");
					//var flowEngine = new FlowEngine(userConnection);
					//Dictionary<string, string> parameter = new Dictionary<string, string>();
					//parameter.Add("OrderId", orderEntityId.ToString());
					//flowEngine.RunProcess(processSchema, parameter);
					#endregion
				}

                foreach (var item in request.items)
				{
					var currentItem = item.parent_item != null ? item.parent_item : item;
					if (currentItem.product_type != "simple" && item.parent_item == null)
					{
						continue;
					}
					//string ruleSetNumber = String.Empty;
					//foreach (var rule in request.rule_sets.items)
					//{
					//if (item.sku == rule.sku)
					//{
					//ruleSetNumber = request.rule_sets.rule_set_number;
					//}
					//}

					#region Инициируем вызов метода ProcessOrderItem()
					// TODO: [✓] Шаг 8. MasterCard. Добавили параметр discountByProduct при вызове метода ProcessOrderItem()
					orderPorductError = ProcessOrderItem(orderEntityId, currentItem, request.extension_attributes.is_in_set, isActive, discountByProduct, ref productEntityId, ref orderProductId);
					if (orderPorductError != null)
					{
						return orderPorductError;
					}

					#endregion

					#region Инициируем вызов метода ProcessDiscounts()

					if (currentItem.ExtensionAttributes.discounts == null)
					{
						continue;
					}
					ProcessDiscounts(currentItem.ExtensionAttributes.discounts, orderEntityId, productEntityId, orderProductId, currentItem.ExtensionAttributes.order_item_number, currentItem.sku, currentItem);

					#endregion
				}
			}
			catch (Exception ex)
			{
				Logger.Log(ex.Message, userConnection, "NewOrder UsrEvaBPMService Service - ProcessOrder orderId=" + orderId.ToString());
				return ex.Message;
			}

			return null;
		}

		#endregion

		#region Реализация метода ProcessOrderItem()

		/// <summary> Создает экземпляр OrderProduct в БД </summary>
		/// <param name="orderId"> Order.Id </param>
		/// <param name="item"> item из items </param>
		/// <param name="isInSet"></param>
		/// <param name="productEntityId"> Product.Id </param>
		/// <param name="discountByProduct"> Скидка на продукт</param>
		/// <param name="orderProductId"> OrderProduct.Id </param>
		/// <returns> Возвращает null или ex.Message </returns>
		public string ProcessOrderItem(Guid orderId, OrderItem item, long? isInSet, string isActive, decimal? discountByProduct, ref Guid productEntityId, ref Guid orderProductId)
		// TODO: [✓] Шаг 7. MasterCard. Добавили параметры bool isActive и decimal discountByProduct при реализации метода ProcessOrderItem()
		{
			var entity = userConnection.EntitySchemaManager.GetInstanceByName("OrderProduct").CreateEntity(userConnection);
			productEntityId = GetLookupBpmIdByString("Product", "Code", item.sku, "Id");
			
			var format = GetLookupBpmNameByGuid("Product", "Id", productEntityId, "UsrFormat");
			orderProductId = Guid.NewGuid();
			entity.SetDefColumnValues();
			entity.SetColumnValue("Id", orderProductId);
			entity.SetColumnValue("UsrSKU", item.sku);
			entity.SetColumnValue("Name", item.name);
			entity.SetColumnValue("Quantity", item.qty_ordered);
			entity.SetColumnValue("OrderId", orderId);
			entity.SetColumnValue("ProductId", productEntityId != Guid.Empty ? productEntityId : (Guid?)null);
			entity.SetColumnValue("UsrFormat", format);
			entity.SetColumnValue("TotalAmount", item.row_total_discounted);
			entity.SetColumnValue("UsrAction", item.applied_rule_ids);
			entity.SetColumnValue("UsrIsInSet", item.ExtensionAttributes != null ? (item.ExtensionAttributes.discounts.Count != 0 ? (item.ExtensionAttributes.discounts[0].rule_set_id != null ? item.ExtensionAttributes.discounts[0].rule_set_id : 0) : 0) : 0);
			entity.SetColumnValue("UsrOrderItemNumber", item.ExtensionAttributes != null ? (item.ExtensionAttributes.order_item_number) : 0L);
			entity.SetColumnValue("UsrRowWeight", item.row_weight);
			entity.SetColumnValue("UsrOriginalPrice", item.original_price);
			entity.SetColumnValue("UsrDiscountedPrice", item.ExtensionAttributes != null ? (item.ExtensionAttributes.price_discounted) : 0m);
			entity.SetColumnValue("DiscountAmount", item.discount_amount);
			entity.SetColumnValue("UsrOperation", item.ExtensionAttributes.operation_id);
			entity.SetColumnValue("UsrAmountPrice", item.row_total);
			entity.SetColumnValue("UsrAmountPriceDiscount", item.ExtensionAttributes != null ? (item.ExtensionAttributes.row_total_discounted) : 0m);

			#region Шаг 9. MasterCard. Изменение стоимости продукта и запись OrderProduct.Notes [✓]

			// TODO: [✓] Шаг 9. MasterCard. Изменение стоимости продукта и запись OrderProduct.Notes
			if (isActive == String.Empty && discountByProduct != null)
            {
				entity.SetColumnValue("Notes", String.Format("{0:F2}", item.price)); // Записываем старую цену
				var newPrice = item.price - (item.price * discountByProduct);
				entity.SetColumnValue("Price", newPrice);                            // Записываем новую цену 
			}
			else
			{
				entity.SetColumnValue("Price", item.price);                          // Записываем текущую цену
			}                         

			#endregion

			//entity.SetColumnValue("UsrRuleSetsId", ruleSetNumber);
			try
			{
				entity.Save(false);
				orderProductId = entity.GetTypedColumnValue<Guid>("Id");
			}
			catch (Exception ex)
			{
				Logger.Log(ex.Message, userConnection, "NewOrder UsrEvaBPMService Service - ProcessOrderItem orderId=" + orderId.ToString() + " sku=" + item.sku.ToString());
				return ex.Message;
			}
			return null;
			
		}

		#endregion

		#region Реализация метода ProcessCustomerV2()

		/// <summary> Ищет или создает новый контакт </summary>
		/// <param name="request"> Тело ответа на GET запрос к Magento </param>
		/// <param name="contactId"> Guid контакта</param>
		/// <returns> Возвращает null или ex.Message</returns>
		public string ProcessCustomerV2(GetOrderResponse request, ref Guid contactId)
		{
			var cleanedPhone = CleanPhone(request.billing_address.telephone);
			var entity = userConnection.EntitySchemaManager.GetInstanceByName("Contact").CreateEntity(userConnection);
			contactId = GetLookupBpmIdByString("Contact", "UsrIMCode", request.customer_id.ToString(), "Id");
			var typeId = GetLookupBpmIdByString("ContactType", "Name", "Клиент", "Id");
			var cityId = GetLookupBpmIdByString("City", "Name", request.extension_attributes.customer_city, "Id");
			if (contactId == Guid.Empty)
			{
				contactId = GetLookupBpmIdByString("Contact", "UsrNumberActiveCard", request.extension_attributes.loyalty_card, "Id");
				if (contactId != Guid.Empty)
				{
					UpdateTableStringColumn("Contact", "UsrIMCode", contactId, request.customer_id.ToString());
				}
				else
				{
					var searchNumberLength = Convert.ToInt32(Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "SearchNumberLength"));
					var searchNumber = Reverse(cleanedPhone);
					searchNumber = searchNumber.Substring(0, searchNumberLength > searchNumber.Length ? searchNumber.Length : searchNumberLength);
					contactId = GetContactIdByReversedPhone(searchNumber, userConnection);
					if (contactId != Guid.Empty)
					{
						#region Инициируем вызов метода UpdateTableStringColumn() Contact.UsrIMCode

						UpdateTableStringColumn("Contact", "UsrIMCode", contactId, request.customer_id.ToString());

						#endregion
					}
					else
					{
						entity.SetDefColumnValues();
						entity.SetColumnValue("TypeId", typeId != Guid.Empty ? typeId : (Guid?)null);
						entity.SetColumnValue("MobilePhone", cleanedPhone);
						entity.SetColumnValue("Surname", request.billing_address.lastname);
						entity.SetColumnValue("GivenName", request.billing_address.firstname);
						entity.SetColumnValue("MiddleName", request.billing_address.middlename);
						#region В объекте Contact нет поля AccountName
						entity.SetColumnValue("AccountName", request.billing_address.lastname + " " + request.billing_address.firstname + " " + request.billing_address.middlename);
						#endregion
						entity.SetColumnValue("Email", request.customer_email);
						entity.SetColumnValue("CityId", cityId != Guid.Empty ? cityId : (Guid?)null);
						entity.SetColumnValue("BirthDate", GetDateFromString(request.customer_dob));
						entity.SetColumnValue("UsrNumberActiveCard", request.extension_attributes.loyalty_card);
						entity.SetColumnValue("Zip", request.extension_attributes.shipping_assignments[0].shipping.address.postcode);
						entity.SetColumnValue("AddressTypeId", UsrConstantsServer.AddressType.Shipping);
						entity.SetColumnValue("UsrIMCode", request.customer_id);
						entity.SetColumnValue("UsrIsCreatedFromService", true);
						try
						{
							entity.Save(false);
							contactId = entity.GetTypedColumnValue<Guid>("Id");
							Logger.Log("UsrEvaBPMService", userConnection, " ProcessCustomerV2 " + contactId.ToString());
						}
						catch (Exception ex)
						{
							Logger.Log(ex.Message, userConnection, "NewOrder UsrEvaBPMService Service - ProcessCustomerV2 contactId=" + contactId.ToString());
							return ex.Message;
						}
					}
				}
			}

			#region Инициируем вызов метода InsertMobilePhoneToContactCommunication() INSERT SET ContactCommunication

			InsertMobilePhoneToContactCommunication(contactId, cleanedPhone);

			#endregion

			return null;
		}

		#endregion

		#region Реализация метода ProcessDiscounts()

		/// <summary>
		/// Создает экземпляр UsrShares в БД
		/// </summary>
		/// <param name="discounts"> items.ExtensionAttributes.discounts </param>
		/// <param name="orderId"> Order.Id </param>
		/// <param name="productId"> OrderProduct.ProductId </param>
		/// <param name="orderProductId"> OrderProduct.Id </param>
		/// <param name="orderItemNumber"> OrderProduct.UsrOrderItemNumber </param>
		/// <param name="sku"> OrderProduct.SKU </param>
		/// <param name="item"> item из items </param>
		/// <returns> Возвращает String.Empty или ex.Message </returns>
		public string ProcessDiscounts(List<Discount> discounts, Guid orderId, Guid productId, Guid orderProductId, long? orderItemNumber, string sku, OrderItem item)
		{
			var entity = userConnection.EntitySchemaManager.GetInstanceByName("UsrShares").CreateEntity(userConnection);
			var entityId = Guid.NewGuid();
			foreach (var discount in discounts)
			{
				entity = userConnection.EntitySchemaManager.GetInstanceByName("UsrShares").CreateEntity(userConnection);
				entityId = Guid.NewGuid();
				entity.SetDefColumnValues();
				entity.SetColumnValue("Id", entityId);
				entity.SetColumnValue("UsrOrderProductId", orderProductId);
				entity.SetColumnValue("UsrRuleId", discount.rule_id);
				entity.SetColumnValue("UsrRuleName", discount.rule_name);
				entity.SetColumnValue("UsrPrice", discount.price);
				entity.SetColumnValue("UsrAmountPrice", discount.row_total);
				entity.SetColumnValue("UsrDiscountPercent", discount.discount_percent);
				entity.SetColumnValue("UsrDiscount", discount.discount_amount);
				entity.SetColumnValue("UsrPriceDiscounted", discount.price_discounted);
				entity.SetColumnValue("UsrRuleSetId", discount.rule_set_id);
				entity.SetColumnValue("UsrProductSharesId", productId != Guid.Empty ? productId : (Guid?)null);
				entity.SetColumnValue("UsrOrderSharesId", orderId);
				entity.SetColumnValue("UsrOrderItemNumber", orderItemNumber);
				entity.SetColumnValue("UsrSKU", sku);
				entity.SetColumnValue("UsrQuantity", discount.qty);
				entity.SetColumnValue("UsrDiscountAmount", discount.row_discount_amount);
				entity.SetColumnValue("UsrAmountPriceDiscounted", discount.row_total_discounted);
				entity.SetColumnValue("UsrSetId", discount.rule_set_id);
				try
				{
					entity.Save(false);
				}
				catch (Exception ex)
				{
					Logger.Log(ex.Message, userConnection, "NewOrder UsrEvaBPMService Service - ProcessDiscounts orderId=" + orderId.ToString());
					return ex.Message;
				}
			}
			return String.Empty;
		}

		#endregion

		#region Реализация метода InsertMobilePhoneToContactCommunication()

		/// <summary> INSERT данных в БД, в таблицу ContactCommunication </summary>
		/// <param name="contactId"> Contact.Id </param>
		/// <param name="phone"> Contact.MobilePhone </param>
		/// <returns> Возвращает ex.Message или String.Empty </returns>
		public string InsertMobilePhoneToContactCommunication(Guid contactId, string phone)
		{
			var entity = userConnection.EntitySchemaManager.GetInstanceByName("ContactCommunication").CreateEntity(userConnection);
			try
			{
				var entityConditions = new Dictionary<string, object>() {
					{ "Contact", contactId },
					{ "Number", phone},
				};

				if (entity.FetchFromDB(entityConditions))
				{
					return String.Empty;
				}
				entity.SetDefColumnValues();
				entity.SetColumnValue("ContactId", contactId);
				entity.SetColumnValue("Number", phone);
				entity.SetColumnValue("CommunicationTypeId", new Guid("D4A2DC80-30CA-DF11-9B2A-001D60E938C6"));
				entity.Save(false);
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
			return String.Empty;
		}

		#endregion

		#region Реализация метода GetDateFromString()

		/// <summary> Преобразовывает string в DateTime </summary>
		/// <param name="value"> string которую нужно преобразовать </param>
		/// <returns> Преобразованное значение в DateTime </returns>
		public DateTime? GetDateFromString(string value)
		{
			var date = DateTime.Now;
			var isDate = DateTime.TryParse(value, out date);
			return isDate == true ? date : (DateTime?)null;
		}

		#endregion

		#region Реализация метода Reverse()

		/// <summary> Изменяет порядок элементов на обратный </summary>
		/// <param name="s"> Order.UsrNotificationNumber </param>
		/// <returns> Возвращает значение для ContactCommunication.SearchNumber</returns>
		public string Reverse(string s)
		{
			char[] charArray = s.ToCharArray();
			Array.Reverse(charArray);
			return new string(charArray);
		}

		#endregion

		#region Реализация метода GetContactIdByReversedPhone()

		/// <summary> Поиск Contact.Id из ContactCommunication.SearchNumber </summary>
		/// <param name="phone"> Reversed(Order.UsrNotificationNumber) </param>
		/// <param name="userConnection"> Права доступа </param>
		/// <returns> Возвращает Contact.Id или Guid.Empty </returns>
		public Guid GetContactIdByReversedPhone(string phone, UserConnection userConnection)
		{
			var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, "ContactCommunication");
			esq.AddColumn("Contact");
			var startWithFilter = esq.CreateFilterWithParameters(FilterComparisonType.StartWith, "SearchNumber", phone);
			esq.Filters.Add(startWithFilter);
			EntitySchemaQueryOptions options = new EntitySchemaQueryOptions
			{
				PageableDirection = PageableSelectDirection.First,
				PageableRowCount = 1,
				PageableConditionValues = new Dictionary<string, object>()
			};
			var entities = esq.GetEntityCollection(userConnection, options);
			if (entities.Count == 1)
			{
				return entities[0].GetTypedColumnValue<Guid>("ContactId");
			}
			return Guid.Empty;
		}

		#endregion

		#region Реализация метода CleanPhone()

		/// <summary> Уберает все что не цифра </summary>
		/// <param name="phone"> request.billing_address.telephone </param>
		/// <returns> Возвращает [UI]: "Заказ.Номер для уведомлений"; [DB]: "Order.UsrNotificationNumber" </returns>
		public string CleanPhone(string phone)
		{
			Regex digitsOnly = new Regex(@"[^\d]");
			return digitsOnly.Replace(phone, "");
		}

		#endregion

		#region Реализация метода GetOrderStreet()

		/// <summary> Обрезает ненужное, и возвращает требуемое значение </summary>
		/// <param name="streetResponse"> request.extension_attributes.shipping_assignments[0].shipping.address.extension_attributes.street.name </param>
		/// <returns> Возвращает [UI]: "Заказ.Улица"; [DB]: "Order.UsrAddress" </returns>
		public string GetOrderStreet(string streetResponse)
		{
			var splitResponse = streetResponse.Split(':');
			if (splitResponse.Length > 1)
			{
				return splitResponse[1];
			}
			return splitResponse[0];
		}

		#endregion

		#region Реализация метода UpdateTableLookupColumn()

		/// <summary> UPDATE данных в БД, Guid value по Guid value </summary>
		/// <param name="table"> SET Сущность / Таблица </param>
		/// <param name="column"> WHERE Поле / Колонка </param>
		/// <param name="recordId"> Guid value </param>
		/// <param name="columnValue"> Guid value </param>
		/// <returns> Возвращает ex.Message или String.Empty </returns>
		public string UpdateTableLookupColumn(string table, string column, Guid recordId, Guid columnValue)
		{
			var entity = userConnection.EntitySchemaManager.GetInstanceByName(table).CreateEntity(userConnection);
			try
			{
				if (entity.FetchFromDB(recordId))
				{
					entity.SetColumnValue(column, columnValue);
					entity.Save(false);
				}
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
			return String.Empty;
		}

		#endregion

		#region Реализация метода UpdateTableStringColumn()

		/// <summary> UPDATE данных в БД, string value по Guid value </summary>
		/// <param name="table"> SET Сущность / Таблица </param>
		/// <param name="column"> WHERE Поле / Колонка </param>
		/// <param name="recordId"> Guid value </param>
		/// <param name="columnValue"> string value </param>
		/// <returns> Возвращает String.Empty или ex.Message </returns>
		public string UpdateTableStringColumn(string table, string column, Guid recordId, string columnValue)
		{
			var entity = userConnection.EntitySchemaManager.GetInstanceByName(table).CreateEntity(userConnection);
			try
			{
				if (entity.FetchFromDB(recordId))
				{
					entity.SetColumnValue(column, columnValue);
					entity.Save(false);
				}
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
			return String.Empty;
		}

		#endregion

		#region Реализация метода GetLookupBpmIdByString()

		/// <summary> SELECT данных из БД по string </summary>
		/// <param name="table"> FROM Сущность / Таблица </param>
		/// <param name="column"> WHERE Поле / Колонка </param>
		/// <param name="value"> string value </param>
		/// <param name="columnReturn"> SELECT string </param>
		/// <returns> Возвращает Guid </returns>
		public Guid GetLookupBpmIdByString(string table, string column, string value, string columnReturn)
		{
			if (value == String.Empty || value == null)
			{
				return Guid.Empty;
			}
			var lookupBPMId = (new Select(userConnection).Top(1)
				.Column(columnReturn)
				.From(table)
				.Where(column).IsEqual(Terrasoft.Core.DB.Column.Parameter(value)) as Select).ExecuteScalar<Guid>();
			return lookupBPMId;
		}

		#endregion

		#region Реализация метода GetLookupBpmIdByString()

		/// <summary> SELECT данных из БД по Guid </summary>
		/// <param name="table"> FROM Сущность / Таблица </param>
		/// <param name="column"> WHERE Поле / Колонка </param>
		/// <param name="value"> Guid value </param>
		/// <param name="columnReturn"> SELECT string </param>
		/// <returns> Возвращает Guid </returns>
		public Guid GetLookupBpmIdByGuid(string table, string column, Guid value, string columnReturn)
		{
			if (value == Guid.Empty || value == null)
			{
				return Guid.Empty;
			}
			var lookupBPMId = (new Select(userConnection).Top(1)
				.Column(columnReturn)
				.From(table)
				.Where(column).IsEqual(Terrasoft.Core.DB.Column.Parameter(value)) as Select).ExecuteScalar<Guid>();
			return lookupBPMId;
		}

		#endregion

		#region Реализация метода GetLookupBpmNameByGuid()

		/// <summary> SELECT данных из БД по Guid </summary>
		/// <param name="table"> FROM Сущность / Таблица </param>
		/// <param name="column"> WHERE Поле / Колонка </param>
		/// <param name="value"> Guid value </param>
		/// <param name="columnReturn"> SELECT string </param>
		/// <returns> Возвращает string </returns>
		public string GetLookupBpmNameByGuid(string table, string column, Guid value, string columnReturn)
		{
			if (value == Guid.Empty)
			{
				return String.Empty;
			}
			var lookupBPMName = (new Select(userConnection).Top(1)
				.Column(columnReturn)
				.From(table)
				.Where(column).IsEqual(Terrasoft.Core.DB.Column.Parameter(value)) as Select).ExecuteScalar<string>();
			return lookupBPMName;
		}

		#endregion

		#region Реализация метода GetLookupBpmIdByInt()

		// TODO: Шаг 12. MasterCard. Перенос GetLookupBpmIdByInt() [✓]

		/// <summary> SELECT данных из БД по int? </summary>
		/// <param name="table"> FROM Сущность / Таблица </param>
		/// <param name="column"> WHERE Поле / Колонка </param>
		/// <param name="value"> int? value </param>
		/// <param name="columnReturn"> SELECT string </param>
		/// <returns> Возвращает Guid </returns>
		public Guid GetLookupBpmIdByInt(string table, string column, int? value, string columnReturn)
		{
			if (value == 0 || value == null)
			{
				return Guid.Empty;
			}
			var lookupBPMId = (new Select(userConnection).Top(1)
				.Column(columnReturn)
				.From(table)
				.Where(column).IsEqual(Terrasoft.Core.DB.Column.Parameter(value)) as Select).ExecuteScalar<Guid>();
			return lookupBPMId;
		}

		#endregion

		#region [✓] Шаг 2. MasterCard. Реализация IsMasterCardDiscountActive()

		// TODO: [✓] Шаг 2. MasterCard. Реализация IsMasterCardDiscountActive()

		public string IsMasterCardDiscountActive(GetOrderResponse request, long orderId, ref decimal? discountByProduct)
        {

			#region Читаем SysSettings

			bool promotionFromMasterCardOnOff;
            DateTime promotionFromMasterCardLifeCycle;
			
			try
            {
                promotionFromMasterCardOnOff = Convert.ToBoolean(Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "UsrPromotionFromMasterCardOnOff"));
                promotionFromMasterCardLifeCycle = Convert.ToDateTime(Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "UsrPromotionFromMasterCardLifeCycle"));
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, userConnection, "Difficulty in system settings: [UsrPromotionFromMasterCardOnOff] и [UsrPromotionFromMasterCardLifeCycle]");
				return ex.Message;
			}

			#endregion

			#region [✓] Шаг 5. MasterCard. Вызов CalculateProductDiscount() из IsMasterCardDiscountActive()

			// TODO: [✓] Шаг 5. MasterCard. Вызов CalculateProductDiscount() из IsMasterCardDiscountActive()
			if (promotionFromMasterCardOnOff && promotionFromMasterCardLifeCycle > DateTime.Now)
            {
				string cardType;
				decimal hold;

				try
                {
					cardType = request.payment.payment_details_info.cardType.ToLower();
					hold = (decimal)request.payment.payment_details_info.amount;
				}
                catch (NullReferenceException)
                {
					Logger.Log($"Заказ не соответствует условиям Акции от MasterCard. Order.UsrId: {orderId}", userConnection, "IsMasterCardDiscountActive()");
					return "Заказ не соответствует условиям Акции от MasterCard";
				}

				if (request.payment.method.ToLower() == "wayforpay" && cardType == "mastercard")
				{
					string resultCPD = CalculateProductDiscount(request.shipping_amount, hold, request.items, orderId, ref discountByProduct);
					return resultCPD;
				}
				Logger.Log($"Заказ не соответствует условиям Акции от MasterCard. Order.UsrId: {orderId}", userConnection, "IsMasterCardDiscountActive()");
				return "Заказ не соответствует условиям Акции от MasterCard";
			}
			Logger.Log($"Акция от MasterCard отключена. Order.UsrId: {orderId}", userConnection, "IsMasterCardDiscountActive()");
			return "Акция от MasterCard отключена";

			#endregion

		}

		#endregion

		#region [✓] Шаг 3. MasterCard. Реализация CalculateProductDiscount()

		// TODO: [✓] Шаг 3. MasterCard. Реализация CalculateProductDiscount()
		public string CalculateProductDiscount(decimal costOfDelivery, decimal hold, List<OrderItem> items, long orderId, ref decimal? discountByProduct)
		{
			string resultCPD = String.Empty;
			decimal? priceOfAllItems = 0;

			foreach (var item in items)
			{
				var y = item.price * item.qty_ordered;
				priceOfAllItems += y;
			}

			var coefficient = 1 - (hold / (priceOfAllItems + costOfDelivery));

            if (coefficient > 0.00m && coefficient < 0.06m)
			{
				//int countOfProduct = 0;

				//foreach (var item in items)
				//{
				//	var x = item.qty_ordered;
				//	countOfProduct += (int)x;
				//}

				//decimal discountByOrder = (costOfDelivery + (decimal)priceOfAllItems) - hold; // Считаем скидку на заказ
				//discountByProduct = discountByOrder / countOfProduct;                         // Считаем скидку на продукт
				discountByProduct = coefficient;
				return resultCPD;
			}
			resultCPD = "Коэффициент не попал в рамки допустимого";
			Logger.Log($"{resultCPD}:{coefficient}. Order.UsrId: {orderId}, ", userConnection, "CalculateProductDiscount()");
			return resultCPD;
		}

		#endregion

		#region Реализация метода UpdateOrder()

		// TODO: Шаг 11. MasterCard. Перенос UpdateOrder() [✓]

		/// <summary> Обновляет данные по заказу </summary>
		/// <param name="request"> Разобранный json</param>
		/// <param name="orderId"> Order.UsrId </param>
		public void UpdateOrder(GetOrderResponse request, long orderId)
		{

			var entity = userConnection.EntitySchemaManager.GetInstanceByName("Order").CreateEntity(userConnection);
			var orderCheckId = GetLookupBpmIdByInt("Order", "UsrId", request.entity_id, "Id");
			var statusCheckId = GetLookupBpmIdByGuid("Order", "Id", orderCheckId, "StatusId");
			var statusName = GetLookupBpmNameByGuid("OrderStatus", "Id", statusCheckId, "Name");
			if (statusName == "Отменен")
			{
				return;
			}
			if (entity.FetchFromDB("UsrId", request.entity_id))
			{
				var invoiceExpirationDate = DateTime.Now;
				var isInvoiceExpirationDateValid = false;
				isInvoiceExpirationDateValid = DateTime.TryParse(request.extension_attributes.invoice_expiration_date, out invoiceExpirationDate);
				var statusId = GetLookupBpmIdByString("OrderStatus", "UsrCodeIM", request.state, "Id");
				var paymentStatusId = GetLookupBpmIdByString("OrderPaymentStatus", "UsrPaymentStatusCode", request.extension_attributes.wfp_transaction_status, "Id");
				entity.SetColumnValue("UsrInvoice", request.extension_attributes.invoice);
				entity.SetColumnValue("UsrWfpOrder", request.extension_attributes.wfp_order_id);
				entity.SetColumnValue("UsrInvoiceExpireDate", isInvoiceExpirationDateValid == true ? invoiceExpirationDate : DateTime.Now);
				if (statusId != Guid.Empty)
				{
					entity.SetColumnValue("StatusId", statusId);
				}
				entity.SetColumnValue("PaymentStatusId", paymentStatusId != Guid.Empty ? paymentStatusId : (Guid?)null);
				entity.SetColumnValue("UsrPaymentMethod", request.payment.method);

				#region Тестирование MasterCard. Шаг 9. Работа акции от MasterCard в методе OrderUpdate

				// TODO: Шаг 13. MasterCard. Вызов IsMasterCardDiscountActive() из UpdateOrder() [✓]
				decimal? discountByProduct = default;
				string isActive = IsMasterCardDiscountActive(request, orderId, ref discountByProduct);
                
				if (isActive == String.Empty && discountByProduct != null)
                {
					// TODO: Шаг 16. MasterCard. Вызов ChangePriceFromOrderUpdateForMasterCard() из UpdateOrder()[✓]
					string isChange = ChangePriceFromOrderUpdateForMasterCard(request.items, discountByProduct, orderId);

                    if (isChange == String.Empty)
                    {
						// TODO: Шаг 14. MasterCard. Запись discountByProduct в Order.Notes [✓]
						var newShippingAmount = request.shipping_amount - (request.shipping_amount * discountByProduct);
						entity.SetColumnValue("UsrCostDelivery", newShippingAmount);
						entity.SetColumnValue("Notes", "Применена скидка по акции от MasterCard");

						Logger.Log($"Цены изменены. Order.UsrId: {orderId}", userConnection, "ChangePriceFromOrderUpdateforMasterCard()");
					}
				}

				#endregion

				try
				{
					entity.Save(false);
				}
				catch (Exception ex)
				{
					Logger.Log(ex.Message, userConnection, "OrderUpdate Service error");
				}
			}
			else
			{
				Logger.Log("Заказ ИД: " + orderId.ToString() + " не найден!", userConnection, "OrderUpdate Service error");
			}
		}

		#endregion

		#region Реализация метода ChangePriceFromOrderUpdateForMasterCard()

		// TODO: Шаг 15. MasterCard. Реализация ChangePriceFromOrderUpdateForMasterCard() [✓]
		public string ChangePriceFromOrderUpdateForMasterCard(List<OrderItem> items, decimal? discountByProduct, long orderId)
        {
			var orderGuid = GetLookupBpmIdByString("Order", "Usrid", orderId.ToString(), "Id");

			foreach (var item in items)
            {
				var currentPosition = item.ExtensionAttributes.order_item_number;
				var newPrice = item.price - (item.price * discountByProduct);
				var update = new Update(UserConnection, "OrderProduct")
					.Set("Price", Column.Parameter(newPrice))
					.Set("Notes", Column.Parameter(String.Format("{0:F2}", item.price)))
					.Where("OrderId").IsEqual(Column.Parameter(orderGuid))
					.And("UsrOrderItemNumber").IsEqual(Column.Parameter(currentPosition)) as Update;
                try
                {
					update.Execute();
				}
                catch (Exception ex)
                {
					Logger.Log($"Цены не изменены. Order.UsrId: {orderId}", userConnection, "ChangePriceFromOrderUpdateforMasterCard()");
					return ex.Message;
                }
			}
			return String.Empty;
		}

		#endregion
	}

	#region DTO

	/// <summary> Содержит свойство order_id </summary>
	[DataContract]
	public class NewOrderRequest
	{
		/// <summary> [UI]: "ID заказа ИМ"; [DB]: "UsrId" </summary>
		[DataMember]
		public long order_id { get; set; }
	}

	/// <summary> cодержит свойства success и error для API ответа </summary>
	[DataContract]
	public class Response
	{
		/// /// <summary> Свойство success в API ответе </summary>
		[DataMember]
		public bool success { get; set; }

		/// <summary> Свойство error в API ответе </summary>
		[DataMember]
		public string error { get; set; }
	}

	/// <summary> cодержит свойства billing_address </summary>
	[DataContract]
	public class BillingAddress
	{
		/// <summary> [UI]: "Заказ.Номер для уведомлений"; [DB]: "Order.UsrNotificationNumber" </summary>
		[DataMember]
		[JsonProperty("telephone")]
		public string telephone { get; set; }

		/// <summary> [UI]: "Контакт.Имя"; [DB]: "Contact.GivenName" </summary>
		[DataMember]
		[JsonProperty("firstname")]
		public string firstname { get; set; }

		/// <summary> [UI]: "Контакт.Фамилия"; [DB]: "Contact.Surname" </summary>
		[DataMember]
		[JsonProperty("lastname")]
		public string lastname { get; set; }

		/// <summary> [UI]: "Контакт.Отчество"; [DB]: "Contact.MiddleName" </summary>
		[DataMember]
		[JsonProperty("middlename")]
		public string middlename { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("email")]
		public string email { get; set; }
	}

	/// <summary> Cодержит свойства соответствующие ответу на GET запрос к Magento </summary>
	[DataContract]
	public class GetOrderResponse
	{
		/// <summary> [UI]: "Контакт.UsrIMCode"; [DB]: "Contact.UsrIMCode" </summary>
		[DataMember]
		[JsonProperty("customer_id")]
		public long? customer_id { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("entity_id")]
		public int? entity_id { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("customer_lastname")]
		public string customer_lastname { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("customer_firstname")]
		public string customer_firstname { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("customer_middlename")]
		public string customer_middlename { get; set; }

		/// <summary>
		/// [UI]: "Контакт.Email"; [DB]: "Contact.Email";
		/// [UI]: "Заказ.UsrCustomerEmail"; [DB]: "Order.UsrCustomerEmail"
		/// </summary>
		[DataMember]
		[JsonProperty("customer_email")]
		public string customer_email { get; set; }

		/// <summary> [UI]: "Контакт.Дата рождения"; [DB]: "Contact.BirthDate" </summary>
		[DataMember]
		[JsonProperty("customer_dob")]
		public string customer_dob { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("customer_gender")]
		public long? customer_gender { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("customer_group_id")]
		public string customer_group_id { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("created_at")]
		public string created_at { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("status")]
		public string status { get; set; }

		/// <summary>
		/// [UI]: "Заказ.Итого"; [DB]: "Order.Amount";
		/// [UI]: "Заказ.Сумма оплаты"; [DB]: "Order.PaymentAmount"
		/// </summary>
		[DataMember]
		[JsonProperty("grand_total")]
		public decimal? grand_total { get; set; }

		/// <summary> [UI]: "Заказ.Сайт"; [DB]: "Order.UsrWebsiteCode" </summary>
		[DataMember]
		[JsonProperty("store_id")]
		public long? store_id { get; set; }

		/// <summary> [UI]: "Заказ.Язык сайта"; [DB]: "Order.UsrWebsiteLanguage" </summary>
		[DataMember]
		[JsonProperty("store_name")]
		public string store_name { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("applied_rule_ids")]
		public string applied_rule_ids { get; set; }

		/// <summary> Cодержит свойства items </summary>
		[DataMember]
		[JsonProperty("items")]
		public List<OrderItem> items { get; set; }

		/// <summary> Cодержит свойства billing_address </summary>
		[DataMember]
		[JsonProperty("billing_address")]
		public BillingAddress billing_address { get; set; }

		/// <summary> Cодержит свойства payment </summary>
		[DataMember]
		[JsonProperty("payment")]
		public Payment payment { get; set; }

		/// <summary> Cодержит свойства extension_attributes </summary>
		[DataMember]
		[JsonProperty("extension_attributes")]
		public ExtensionAttributes extension_attributes { get; set; }

		/// <summary> [UI]: "Заказ.Номер"; [DB]: "Order.Number" </summary>
		[DataMember]
		[JsonProperty("increment_id")]
		public string increment_id { get; set; }

		/// <summary> Свойство используется для поиска родительского заказа и проставления по нему состояния </summary>
		[DataMember]
		[JsonProperty("relation_parent_real_id")]
		public string relation_parent_real_id { get; set; }

		/// <summary> [UI]: "Заказ.Родительский заказ"; [DB]: "Order.UsrParentId" </summary>
		[DataMember]
		[JsonProperty("original_increment_id")]
		public string original_increment_id { get; set; }

		/// <summary> [UI]: "Заказ.Стоимость доставки"; [DB]: "Order.UsrCostDelivery" </summary>
		[DataMember]
		[JsonProperty("shipping_amount")]
		public decimal shipping_amount { get; set; }

		/// <summary> [UI]: "Заказ.Общий вес заказа"; [DB]: "Order.UsrWeight" </summary>
		[DataMember]
		[JsonProperty("weight")]
		public decimal weight { get; set; }

		/// <summary> [UI]: "Состояние заказа.Код ИМ"; [DB]: "OrderStatus.UsrCodeIM" </summary>
		[DataMember]
		[JsonProperty("state")]
		public string state { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("invoice")]
		public string invoice { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("invoice_expiration_date")]
		public string invoice_expiration_date { get; set; }

		/// <summary> [UI]: "Заказ.UsrOperationDelivery"; [DB]: "Order.UsrOperationDelivery" </summary>
		[DataMember]
		[JsonProperty("operation_id")]
		public string operation_id { get; set; }

		//[DataMember]
		//[JsonProperty("rule_sets")]
		//public rule_items rule_sets { get; set; }
	}

	/// <summary> Cодержит свойства extension_attributes.shipping_assignments.shipping.address </summary>
	[DataContract]
	public class ShippingAddress
	{
		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("city")]
		public string city { get; set; }

		/// <summary> Часть firstname --> [UI]: "Заказ.Имя получателя"; [DB]: "Order.ReceiverName" </summary>
		[DataMember]
		[JsonProperty("firstname")]
		public string firstname { get; set; }

		/// <summary> Часть lastname --> [UI]: "Заказ.Имя получателя"; [DB]: "Order.ReceiverName" </summary>
		[DataMember]
		[JsonProperty("lastname")]
		public string lastname { get; set; }

		/// <summary> Часть middlename --> [UI]: "Заказ.Имя получателя"; [DB]: "Order.ReceiverName" </summary>
		[DataMember]
		[JsonProperty("middlename")]
		public string middlename { get; set; }

		/// <summary> [UI]: "Заказ.Контактный телефон"; [DB]: "Order.ContactNumber" </summary>
		[DataMember]
		[JsonProperty("telephone")]
		public string telephone { get; set; }

		/// <summary> [UI]: "Контакт.Индекс"; [DB]: "Contact.Zip" </summary>
		[DataMember]
		[JsonProperty("postcode")]
		public string postcode { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("street")]
		public List<string> street { get; set; }

		/// <summary> Cодержит свойства extension_attributes.shipping_assignments.shipping.address.extension_attributes </summary>
		[DataMember]
		[JsonProperty("extension_attributes")]
		public ShippingExtensionAttributes extension_attributes { get; set; }
	}

	/// <summary> Cодержит свойства extension_attributes.shipping_assignments.shipping.address.extension_attributes </summary>
	[DataContract]
	public class ShippingExtensionAttributes
	{
		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("method")]
		public string method { get; set; }

		/// <summary> [UI]: "Заказ.Код города ИМ"; [DB]: "Order.UsrCityCode" </summary>
		[DataMember]
		[JsonProperty("city_id")]
		public string city_id { get; set; }

		/// <summary> [UI]: "Заказ.Код улицы"; [DB]: "Order.UsrStreetCode" </summary>
		[DataMember]
		[JsonProperty("street_id")]
		public string street_id { get; set; }

		/// <summary> [UI]: "Заказ.Код региона ИМ"; [DB]: "Order.UsrRegionCode" </summary>
		[DataMember]
		[JsonProperty("region_id")]
		public string region_id { get; set; }

		/// <summary> [UI]: "Заказ.Отделение доставки"; [DB]: "Order.UsrDeliveryDepartment" </summary>
		[DataMember]
		[JsonProperty("warehouse_number")]
		public string warehouse_number { get; set; }

		/// <summary> Cодержит свойства extension_attributes.shipping_assignments.shipping.address.extension_attributes.street </summary>
		[DataMember]
		[JsonProperty("street")]
		public ShippingStreet street { get; set; }

		/// <summary> [UI]: "Заказ.Комментарий"; [DB]: "Order.Comment" </summary>
		[DataMember]
		[JsonProperty("comment")]
		public string comment { get; set; }

		/// <summary> [UI]: "Заказ.Город"; [DB]: "Order.UsrCity" </summary>
		[DataMember]
		[JsonProperty("city")]
		public string city { get; set; }
	}

	/// <summary> Cодержит свойства extension_attributes.shipping_assignments.shipping.address.extension_attributes.street </summary>
	[DataContract]
	public class ShippingStreet
	{
		/// <summary> [UI]: "Заказ.Улица"; [DB]: "Order.UsrAddress" </summary>
		[DataMember]
		[JsonProperty("name")]
		public string name { get; set; }

		/// <summary> [UI]: "Заказ.Дом"; [DB]: "Order.UsrHouse" </summary>
		[DataMember]
		[JsonProperty("house_number")]
		public string house_number { get; set; }

		/// <summary> [UI]: "Заказ.Квартира"; [DB]: "Order.UsrApartment" </summary>
		[DataMember]
		[JsonProperty("apt_number")]
		public string apt_number { get; set; }
	}

	/// <summary> Содержит свойства extension_attributes.shipping_assignments.shipping </summary>
	[DataContract]
	public class Shipping
	{
		/// <summary> Cодержит свойства extension_attributes.shipping_assignments.shipping.address </summary>
		[DataMember]
		[JsonProperty("address")]
		public ShippingAddress address { get; set; }

		/// <summary> [UI]: "Сервисы доставки.ID код"; [DB]: "UsrDeliveryService.UsrIMCode" </summary>
		[DataMember]
		[JsonProperty("method")]
		public string method { get; set; }
	}

	/// <summary> Содержит свойства extension_attributes.shipping_assignments </summary>
	[DataContract]
	public class ShippingAssignment
	{
		/// <summary> Cодержит свойства extension_attributes.shipping_assignments.shipping </summary>
		[DataMember]
		[JsonProperty("shipping")]
		public Shipping shipping { get; set; }
	}

	/// <summary> Содержит свойства payment </summary>
	[DataContract]
	public class Payment
	{
		/// <summary> [UI]: "Заказ.Метод оплаты"; [DB]: "Order.UsrPaymentMethod" </summary>
		[DataMember]
		[JsonProperty("method")]
		public string method { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("status")]
		public string status { get; set; }

		#region Тестирование MasterCard. Шаг 1

		//      /// <summary> [UI]: "Заказ.Комментарий заказа"; [DB]: "Order.Notes" </summary>
		//      [DataMember]
		//[JsonProperty("amount_authorized")]
		//public decimal? amount_authorized { get; set; } // TODO: Тестирование MasterCard. Шаг 1. Добавляем [DataMember]

		#endregion

		#region [✓] Шаг 1.2. MasterCard. [DataMember]

		//TODO: [✓] Шаг 1.2. MasterCard. [DataMember]
		/// <summary> Содержит свойства payment.payment_details_info </summary>
		[DataMember]
        [JsonProperty("payment_details_info")]
        public PaymentDetailsInfo payment_details_info { get; set; }

		#endregion
	}

	#region [✓] Шаг 1.1. MasterCard. [DataContract]

	// TODO: [✓] Шаг 1.1. MasterCard. [DataContract]
	/// <summary> Содержит свойства payment.payment_details_info </summary>
	[DataContract]
    public class PaymentDetailsInfo
    {
        /// <summary> payment.payment_details_info.amount </summary>
        [DataMember]
        [JsonProperty("amount")]
        public decimal? amount { get; set; }

        /// <summary> payment.payment_details_info.cardType </summary>
        [DataMember]
        [JsonProperty("cardType")]
        public string cardType { get; set; }

    }

	#endregion

	/// <summary> Содержит свойства extension_attributes </summary>
	[DataContract]
	public class ExtensionAttributes
	{
		/// <summary> Cодержит свойства extension_attributes.shipping_assignments </summary>
		[DataMember]
		[JsonProperty("shipping_assignments")]
		public List<ShippingAssignment> shipping_assignments { get; set; }

		/// <summary> [UI]: "Город.Название"; [DB]: "City.Name" </summary>
		[DataMember]
		[JsonProperty("customer_city")]
		public string customer_city { get; set; }

		/// <summary> [UI]: "Контакт.Номер активной карты"; [DB]: "Contact.UsrNumberActiveCard" </summary>
		[DataMember]
		[JsonProperty("loyalty_card")]
		public string loyalty_card { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("is_set_present")]
		public long? is_set_present { get; set; }

		/// <summary> Передается 3-м параметром в метод ProcessOrderItem, но не используется в нем </summary>
		[DataMember]
		[JsonProperty("is_in_set")]
		public long? is_in_set { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("loylty_discount_amount")]
		public decimal? loylty_discount_amount { get; set; }

		/// <summary> [UI]: "Заказ.Не звонить мне"; [DB]: "Order.UsrCall" </summary>
		[DataMember]
		[JsonProperty("call")]
		public long? call { get; set; }

		/// <summary> [UI]: "Заказ.Дата создания"; [DB]: "Order.CreatedOn" </summary>
		[DataMember]
		[JsonProperty("created_at_eet")]
		public string created_at_eet { get; set; }

		/// <summary> [UI]: "Состояние оплаты заказа.Код"; [DB]: "OrderPaymentStatus.UsrPaymentStatusCode" </summary>
		[DataMember]
		[JsonProperty("wfp_transaction_status")]
		public string wfp_transaction_status { get; set; }

		/// <summary> [UI]: "Заказ.Инвойс"; [DB]: "Order.UsrInvoice" </summary>
		[DataMember]
		[JsonProperty("invoice")]
		public string invoice { get; set; }

		/// <summary> [UI]: "Заказ.Срок жизни инвойса"; [DB]: "Order.UsrInvoiceExpireDate" </summary>
		[DataMember]
		[JsonProperty("invoice_expiration_date")]
		public string invoice_expiration_date { get; set; }

		/// <summary> [UI]: "Заказ.WfpOrder"; [DB]: "Order.UsrWfpOrder" </summary>
		[DataMember]
		[JsonProperty("wfp_order_id")]
		public string wfp_order_id { get; set; }
	}

	/// <summary> Содержит свойства items </summary>
	[DataContract]
	public class OrderItem
	{
		/// <summary> [UI]: "Продукт в заказе.SKU"; [DB]: "OrderProduct.UsrSKU" </summary>
		[DataMember]
		[JsonProperty("sku")]
		public string sku { get; set; }

		/// <summary> [UI]: "Продукт в заказе.Название"; [DB]: "OrderProduct.Name" </summary>
		[DataMember]
		[JsonProperty("name")]
		public string name { get; set; }

		/// <summary> [UI]: "Продукт в заказе.Цена"; [DB]: "OrderProduct.Price" </summary>
		[DataMember]
		[JsonProperty("price")]
		public decimal? price { get; set; }

		/// <summary> [UI]: "Продукт в заказе.Количество"; [DB]: "OrderProduct.Quantity" </summary>
		[DataMember]
		[JsonProperty("qty_ordered")]
		public decimal? qty_ordered { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("qty_invoiced")]
		public decimal? qty_invoiced { get; set; }

		/// <summary> [UI]: "Продукт в заказе.ID акции"; [DB]: "OrderProduct.UsrAction" </summary>
		[DataMember]
		[JsonProperty("applied_rule_ids")]
		public string applied_rule_ids { get; set; }

		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("product_id")]
		public long? product_id { get; set; }

		/// <summary> Содержит свойства items.extension_attributes </summary>
		[DataMember]
		[JsonProperty("extension_attributes")]
		public ItemExtensionAttributes ExtensionAttributes { get; set; }

		/// <summary> [UI]: "Продукт в заказе.Общий вес"; [DB]: "OrderProduct.UsrRowWeight" </summary>
		[DataMember]
		[JsonProperty("row_weight")]
		public decimal? row_weight { get; set; }

		/// <summary> [UI]: "Продукт в заказе.Первоначальная цена товара"; [DB]: "OrderProduct.UsrOriginalPrice" </summary>
		[DataMember]
		[JsonProperty("original_price")]
		public decimal? original_price { get; set; }

		/// <summary> [UI]: "Продукт в заказе.Сумма скидки"; [DB]: "OrderProduct.DiscountAmount" </summary>
		[DataMember]
		[JsonProperty("discount_amount")]
		public decimal? discount_amount { get; set; }

		/// <summary> [UI]: "Продукт в заказе.Сума товара"; [DB]: "OrderProduct.UsrAmountPrice" </summary>
		[DataMember]
		[JsonProperty("row_total")]
		public decimal? row_total { get; set; }

		/// <summary> [UI]: "Продукт в заказе.Итого"; [DB]: "OrderProduct.TotalAmount" </summary>
		[DataMember]
		[JsonProperty("row_total_discounted")]
		public decimal? row_total_discounted { get; set; }
		[DataMember]

		[JsonProperty("product_type")]
		public string product_type { get; set; }

		[DataMember]
		[JsonProperty("parent_item")]
		public OrderItem parent_item { get; set; }
	}

	/// <summary> Содержит свойства items.extension_attributes </summary>
	[DataContract]
	public class ItemExtensionAttributes
	{
		/// <summary> Это свойство не используется в коде </summary>
		[DataMember]
		[JsonProperty("rule_set_id")]
		public long? rule_set_id { get; set; }

		/// <summary> Содержит свойства items.extension_attributes.discounts </summary>
		[DataMember]
		[JsonProperty("discounts")]
		public List<Discount> discounts { get; set; }

		/// <summary> [UI]: "Продукт в заказе.Код позиции"; [DB]: "OrderProduct.UsrOrderItemNumber" </summary>
		[DataMember]
		[JsonProperty("order_item_number")]
		public long? order_item_number { get; set; }

		/// <summary> [UI]: "Продукт в заказе.Цена товара после скидки"; [DB]: "OrderProduct.UsrDiscountedPrice" </summary>
		[DataMember]
		[JsonProperty("price_discounted")]
		public decimal? price_discounted { get; set; }

		/// <summary> [UI]: "Продукт в заказе.Сумма товара (после всех скидок)"; [DB]: "OrderProduct.UsrAmountPriceDiscount" </summary>
		[DataMember]
		[JsonProperty("row_total_discounted")]
		public decimal? row_total_discounted { get; set; }

		/// <summary> [UI]: "Продукт в заказе.UsrOperation"; [DB]: "OrderProduct.UsrOperation" </summary>
		[DataMember]
		[JsonProperty("operation_id")]
		public decimal? operation_id { get; set; }
	}

	//[DataContract]
	//public class rule_items
	//{
	//[DataMember]
	//[JsonProperty("items")]
	//public List<RuleItem> items { get; set; }
	//[DataMember]
	//[JsonProperty("rule_set_number")]
	//public string rule_set_number { get; set; }
	//[DataMember]
	//[JsonProperty("rule_id")]
	//public long? rule_id { get; set; }

	//}
	//[DataContract]
	//public class RuleItem
	//{
	//[DataMember]
	//[JsonProperty("sku")]
	//public string sku { get; set; }
	//}

	/// <summary> Содержит свойства items.extension_attributes.discounts </summary>
	[DataContract]
	public class Discount
	{
		/// <summary> [UI]: "Акции по продуктам.Номер акции"; [DB]: "UsrShares.UsrRuleId" </summary>
		[DataMember]
		[JsonProperty("rule_id")]
		public long rule_id { get; set; }

		/// <summary> [UI]: "Акции по продуктам.Название Акции"; [DB]: "UsrShares.UsrRuleName" </summary>
		[DataMember]
		[JsonProperty("rule_name")]
		public string rule_name { get; set; }

		/// <summary> [UI]: "Акции по продуктам.Скидка на ед.товара"; [DB]: "UsrShares.UsrDiscount" </summary>
		[DataMember]
		[JsonProperty("discount_amount")]
		public decimal discount_amount { get; set; }

		/// <summary> [UI]: "Акции по продуктам.Скидка на товар в %"; [DB]: "UsrShares.UsrDiscountPercent" </summary>
		[DataMember]
		[JsonProperty("discount_percent")]
		public decimal discount_percent { get; set; }

		/// <summary> [UI]: "Акции по продуктам.Цена товара после скидки"; [DB]: "UsrShares.UsrPriceDiscounted" </summary>
		[DataMember]
		[JsonProperty("price_discounted")]
		public decimal price_discounted { get; set; }

		/// <summary> 
		/// [UI]: "Продукт в заказе.ID комплекта"; [DB]: "OrderProduct.UsrIsInSet";
		/// [UI]: "Акции по продуктам.Акционный набор"; [DB]: "UsrShares.UsrRuleSetId";
		/// [UI]: "Акции по продуктам.Номер Комплекта"; [DB]: "UsrShares.UsrSetId"
		/// </summary>
		[DataMember]
		[JsonProperty("rule_set_id")]
		public long? rule_set_id { get; set; }

		/// <summary> [UI]: "Акции по продуктам.Количество"; [DB]: "UsrShares.UsrQuantity" </summary>
		[DataMember]
		[JsonProperty("qty")]
		public decimal? qty { get; set; }

		/// <summary> [UI]: "Акции по продуктам.Сумма скидки на товар"; [DB]: "UsrShares.UsrDiscountAmount" </summary>
		[DataMember]
		[JsonProperty("row_discount_amount")]
		public decimal? row_discount_amount { get; set; }

		/// <summary> [UI]: "Акции по продуктам.Сумма товара после скидки"; [DB]: "UsrShares.UsrAmountPriceDiscounted" </summary>
		[DataMember]
		[JsonProperty("row_total_discounted")]
		public decimal? row_total_discounted { get; set; }

		/// <summary> [UI]: "Акции по продуктам.Цена товара до скидки"; [DB]: "UsrShares.UsrPrice" </summary>
		[DataMember]
		[JsonProperty("price")]
		public decimal? price { get; set; }

		/// <summary> [UI]: "Акции по продуктам.Сумма товара до скидки"; [DB]: "UsrShares.UsrAmountPrice" </summary>
		[DataMember]
		[JsonProperty("row_total")]
		public decimal? row_total { get; set; }
	}

	#endregion

}
