using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Core.DB;

namespace EVA.Files.cs
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class UsrEvaUnloadOrderOnFtp : IUsrEvaUnloadOrderOnFtp
    {

        #region Инициализация переменных для сервиса
        public AppConnection appConnection;
        public UserConnection userConnection;

        Response response = new Response();

        public string ftpHost = "";
        public string ftpUserName = "";
        public string ftpPassword = "";
        public string ftpFolderOutOrders = "";
        public string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss");
        public DateTime unloadDate = DateTime.Now;

        /// <summary> Заказы в состоянии [Новый] </summary>
        List<Guid> orderNewList = new List<Guid>();
        /// <summary> Заказы в состоянии [Подтвержден] </summary>
        List<Guid> orderConfirmedList = new List<Guid>();
        /// <summary> Заказы в состоянии [Отменен] </summary>
        List<Guid> orderCanceledList = new List<Guid>();
        /// <summary> Заказы в состоянии [Не оплачен] </summary>
        List<Guid> orderUnpaidList = new List<Guid>();
        /// <summary> Заказы в состоянии [Заявка на отказ от доставки] </summary>
        List<Guid> orderApplicationForRefusalOfDeliveryList = new List<Guid>();

        /// <summary> Задержки экспорта заказа на FTP (по умолчанию 3 минуты) </summary>
        private int сountMinIsExp = 3;
        /// <summary> Корректировка времени т.к. серверное время отличается на -2 часа от текущего (по умолчанию -2 часа) </summary>
        private int correctionTimeHours = -2;
        #endregion

        #region Инициализация системных настроек
        /// <summary> Инициализация системных настроек </summary>
        public UsrEvaUnloadOrderOnFtp(UserConnection userConnection)
        {
            this.userConnection = userConnection;
            ftpHost = Convert.ToString(Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "UsrFTPHost"));
            ftpUserName = Convert.ToString(Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "UsrFTPUserName"));
            ftpPassword = Convert.ToString(Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "UsrFTPPassword"));
            ftpFolderOutOrders = Convert.ToString(Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "UsrPathToFolderInOrders"));
            сountMinIsExp = Terrasoft.Core.Configuration.SysSettings.GetValue<int>(userConnection, "UsrCountMinIsExp", 3);
            correctionTimeHours = Terrasoft.Core.Configuration.SysSettings.GetValue<int>(userConnection, "UsrCorrectionTimeHours", -2);
        }
        #endregion

        #region Контроллер методов
        /// <summary> Контроллер методов </summary>
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public void UnloadOnFTP()
        {
            string countOrder = String.Empty;

            try
            {
                var ordersId = GetModifiedOrdersId();
                ordersId = this.CheсkingCreatedOnOrderProduct(ordersId, this.сountMinIsExp, this.correctionTimeHours);
                
                countOrder = ordersId.Count.ToString();
                if (ordersId.Count != 0)
                {
                    var Order = GetOrderInfo(ordersId);
                    var orderStatusCsv = GenerateOrderStatusCsv(Order);

                    var OrderProducts = GetOrderProductInfo(ordersId);
                    var orderProductsCsv = GenerateOrderProductsCsv(OrderProducts);

                    var OrderHeaders = GetOrderHeadersInfo(ordersId);
                    var orderHeadersCsv = GenerateOrderHeadersCsv(OrderHeaders);

                    var OrderDiscounts = GetOrderDiscountsInfo(ordersId);
                    var orderDiscountsCsv = GenerateOrderDiscountsCsv(OrderDiscounts);

                    UploadOrderStatusCsvFile(ftpHost, ftpUserName, ftpPassword, ftpFolderOutOrders, orderStatusCsv);
                    UploadOrderProductsCsvFile(ftpHost, ftpUserName, ftpPassword, ftpFolderOutOrders, orderProductsCsv);
                    LoggerProduct(OrderProducts);
                    UploadOrderHeadersCsvFile(ftpHost, ftpUserName, ftpPassword, ftpFolderOutOrders, orderHeadersCsv);
                    UploadOrderDiscountsCsvFile(ftpHost, ftpUserName, ftpPassword, ftpFolderOutOrders, orderDiscountsCsv);
                    InsertUnloadDate(unloadDate, ordersId);
                    UpdateUploadParam();
                }
            }
            catch (Exception ex)
            {
                InsertErrorMessage(ex.Message);
            }
            InsertSuccessMessage("Success. Loaded: " + countOrder);
        }
        #endregion

        #region Выбираем заказы которые нужно экспортировать на FTP
        /// <summary> Выбираем заказы которые нужно экспортировать на FTP </summary>
        public List<Guid> GetModifiedOrdersId()
        {
            var dateFormat = "yyyy-MM-dd HH:mm:ss";
            var startDate = DateTime.Now.AddHours(-3).AddMinutes(-5).ToString(dateFormat);
            var endDate = DateTime.Now.AddHours(-3).ToString(dateFormat);
            var orderList = new List<Guid>();

            #region Заказы в состоянии [Новый]
            Select select1 = new Select(userConnection)
                .Column("Order", "Id")
                .From("Order")
                    .Join(JoinType.LeftOuter, "OrderStatus")
                    .On("OrderStatus", "Id").IsEqual("Order", "StatusId")
                .Where("OrderStatus", "UsrCode").IsEqual(Column.Parameter(15))
                .And("UsrUploadAsNew").IsEqual(Column.Parameter(""))
                as Select;
            using (DBExecutor dbExecutor1 = userConnection.EnsureDBConnection())
            {
                using (IDataReader reader1 = select1.ExecuteReader(dbExecutor1))
                {
                    while (reader1.Read())
                    {
                        orderNewList.Add(reader1.GetColumnValue<Guid>("Id"));
                        orderList.Add(reader1.GetColumnValue<Guid>("Id"));
                    }
                }
            }
            #endregion

            #region Заказы в состоянии [Подтвержден]
            Select select2 = new Select(userConnection)
                .Column("Order", "Id")
                .From("Order")
                    .Join(JoinType.LeftOuter, "OrderStatus")
                    .On("OrderStatus", "Id").IsEqual("Order", "StatusId")
                .Where("OrderStatus", "UsrCode").IsEqual(Column.Parameter(16))
                .And("UsrUploadAsConfirmed").IsEqual(Column.Parameter(""))
                as Select;
            using (DBExecutor dbExecutor2 = userConnection.EnsureDBConnection())
            {
                using (IDataReader reader2 = select2.ExecuteReader(dbExecutor2))
                {
                    while (reader2.Read())
                    {
                        orderConfirmedList.Add(reader2.GetColumnValue<Guid>("Id"));
                        orderList.Add(reader2.GetColumnValue<Guid>("Id"));
                    }
                }
            }
            #endregion

            #region Заказы в состоянии [Не оплачен]
            Select select3 = new Select(userConnection)
                .Column("Order", "Id")
                .From("Order")
                    .Join(JoinType.LeftOuter, "OrderStatus")
                    .On("OrderStatus", "Id").IsEqual("Order", "StatusId")
                .Where("OrderStatus", "UsrCode").IsEqual(Column.Parameter(37))
                .And("UsrUploadAsUnpaid").IsEqual(Column.Parameter(""))
                as Select;
            using (DBExecutor dbExecutor3 = userConnection.EnsureDBConnection())
            {
                using (IDataReader reader3 = select3.ExecuteReader(dbExecutor3))
                {
                    while (reader3.Read())
                    {
                        orderUnpaidList.Add(reader3.GetColumnValue<Guid>("Id"));
                        orderList.Add(reader3.GetColumnValue<Guid>("Id"));
                    }
                }
            }
            #endregion

            #region Заказы в состоянии [Отменен]
            Select select4 = new Select(userConnection)
                .Column("Order", "Id")
                .From("Order")
                    .Join(JoinType.LeftOuter, "OrderStatus")
                    .On("OrderStatus", "Id").IsEqual("Order", "StatusId")
                .Where("OrderStatus", "UsrCode").IsEqual(Column.Parameter(13))
                .And("UsrUploadAsCanceled").IsEqual(Column.Parameter(""))
                as Select;
            using (DBExecutor dbExecutor4 = userConnection.EnsureDBConnection())
            {
                using (IDataReader reader4 = select4.ExecuteReader(dbExecutor4))
                {
                    while (reader4.Read())
                    {
                        orderCanceledList.Add(reader4.GetColumnValue<Guid>("Id"));
                        orderList.Add(reader4.GetColumnValue<Guid>("Id"));
                    }
                }
            }
            #endregion

            #region Заказы в состоянии [Заява на отказ от доставки]
            Select select5 = new Select(userConnection)
                .Column("Order", "Id")
                .From("Order")
                    .Join(JoinType.LeftOuter, "OrderStatus")
                    .On("OrderStatus", "Id").IsEqual("Order", "StatusId")
                .Where("OrderStatus", "UsrCode").IsEqual(Column.Parameter(64))
                .And("UsrUploadAsApplicationForRefusalOfDelivery").IsEqual(Column.Parameter(""))
                as Select;
            using (DBExecutor dbExecutor5 = userConnection.EnsureDBConnection())
            {
                using (IDataReader reader5 = select5.ExecuteReader(dbExecutor5))
                {
                    while (reader5.Read())
                    {
                        orderApplicationForRefusalOfDeliveryList.Add(reader5.GetColumnValue<Guid>("Id"));
                        orderList.Add(reader5.GetColumnValue<Guid>("Id"));
                    }
                }
            }
            #endregion

            return orderList;
        }
        #endregion

        #region Убирает из списка заказы, в которые не полностью загружены продукты
        /// <summary> Убирает из списка заказы, в которые не полностью загружены продукты </summary>
        private List<Guid> CheсkingCreatedOnOrderProduct(List<Guid> listOrderId, int сountMinIsExp = 3, int correctionTimeHours = -2)
        {
            List<Guid> newListOrderId = null;
            string codeGroupLog = "845";

            if (сountMinIsExp == 0)
            {
                return listOrderId;
            }

            try
            {
                if (listOrderId != null)
                {
                    newListOrderId = new List<Guid>();

                    foreach (Guid orderId in listOrderId)
                    {
                        DateTime createdOn;

                        try
                        {
                            createdOn = (new Select(userConnection)
                                .Column(Func.Max("OrderProduct", "CreatedOn")).As("CreatedOn")
                                .From("OrderProduct")
                                .Where("OrderProduct", "OrderId").IsEqual(Column.Parameter(orderId))
                                .GroupBy("OrderProduct", "OrderId") as Select)
                                .ExecuteScalar<DateTime>();
                        }
                        catch (Exception err)
                        {
                            this.InsertErrorMessage($"Ошибка во время запроса к базе данных, заказ OrderId = {orderId} {Environment.NewLine} {this.GetStrException(err)} codeGroupLog = {codeGroupLog}");
                            continue;
                        }

                        if (createdOn == DateTime.MinValue)
                        {
                            this.InsertSuccessMessage($"Номенклатура в заказе отсутствует, удаляем заказ из экспорта OrderId = {orderId} codeGroupLog = {codeGroupLog}");
                            continue;
                        }

                        DateTime dateNow = DateTime.Now.AddHours(correctionTimeHours);

                        if ((dateNow - createdOn).TotalMinutes < сountMinIsExp)
                        {
                            this.InsertSuccessMessage($"Между текущей датой и датой создания позиции в заказе (максимально возможная дата из всех позиций заказа) прошло меньше {сountMinIsExp} минут (DateTime.Now = {dateNow}; [OrderProduct].[createdOn] = {createdOn};), удаляем заказ из экспорта OrderId = {orderId} codeGroupLog = {codeGroupLog}");
                            continue;
                        }

                        newListOrderId.Add(orderId);
                    }

                    #region Убирает из списков заказы, которые не прошли проверку
                    this.orderNewList?.RemoveAll(ordNew => !newListOrderId.Any(ord => ord == ordNew));
                    this.orderConfirmedList?.RemoveAll(ordConfirmed => !newListOrderId.Any(ord => ord == ordConfirmed));
                    this.orderCanceledList?.RemoveAll(ordCanceled => !newListOrderId.Any(ord => ord == ordCanceled));
                    this.orderUnpaidList?.RemoveAll(ordUnpaid => !newListOrderId.Any(ord => ord == ordUnpaid));
                    this.orderApplicationForRefusalOfDeliveryList?.RemoveAll(orderApplicationForRefusalOfDelivery => !newListOrderId.Any(ord => ord == orderApplicationForRefusalOfDelivery));
                    #endregion

                }
            }
            catch (Exception err)
            {
                this.InsertErrorMessage($"EVA.Files.cs.UsrEvaUnloadOrderOnFtp.CheсkingCreatedOnOrderProduct(List<Guid> listOrderId, int сountMinIsExp = 3) codeGroupLog = {codeGroupLog} {Environment.NewLine}" + this.GetStrException(err));
            }

            return newListOrderId;
        }
        #endregion

        #region Файл [OrderStatusCsv]

        #region Читаем и записываем данные для файла [OrderStatusCsv]
        /// <summary> Читаем и записываем данные для файла [OrderStatusCsv] </summary>
        public List<Order> GetOrderInfo(List<Guid> orders)
        {
            var Order = new List<Order>();
            try
            {
                foreach (var orderId in orders)
                {
                    Select selectOrder = new Select(userConnection)
                    .Column("Number")
                    .Column("StatusId")
                    .Column("ModifiedOn")
                    .Column("UsrApproveDate")
                    .From("Order")
                    .Where("Id").IsEqual(Column.Parameter(orderId)) as Select;
                    using (DBExecutor dbExecutor = userConnection.EnsureDBConnection())
                    {
                        using (IDataReader reader = selectOrder.ExecuteReader(dbExecutor))
                        {
                            while (reader.Read())
                            {
                                var orderStatusId = reader.GetColumnValue<Guid>("StatusId");
                                var orderStatus = GetLookupBPM("OrderStatus", "Id", orderStatusId);
                                Order.Add(new Order()
                                {
                                    Number = reader.GetColumnValue<string>("Number"),
                                    Status = orderStatus,
                                    ModifiedOn = reader.GetColumnValue<DateTime>("ModifiedOn"),
                                    UsrApproveDate = reader.GetColumnValue<DateTime>("UsrApproveDate")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception e) 
            {
                InsertErrorMessage(e.Message);
            }
            return Order;
        }
        #endregion

        #region Формируем файл [OrderStatusCsv]
        /// <summary> Формируем файл [OrderStatusCsv] </summary>
        public string GenerateOrderStatusCsv(List<Order> orders)
        {
            var delimiter = ";";
            StringBuilder csvExport = new StringBuilder();
            try
            {
                foreach (var order in orders)
                {
                    csvExport.AppendLine(string.Join(delimiter, new string[] {
                    order.Number,
                    order.Status,
                    order.ModifiedOn.ToString(),
                    order.UsrApproveDate.AddHours(3).ToString()
                }));
                }
            }
            catch(Exception e)
            {
                InsertErrorMessage(e.Message);
            }
            return csvExport.ToString();
        }
        #endregion

        #region Выгружаем файл [OrderStatusCsv] на FTP
        /// <summary> Выгружаем файл [OrderStatusCsv] на FTP </summary>
        public void UploadOrderStatusCsvFile(string ftpHost, string ftpUserName, string ftpPassword, string ftpFolderOutOrders, string orderStatusCsv)
        {
            var fileName = dateTime + "Status.csv";
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpHost + ftpFolderOutOrders + fileName);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
            //var text = Encoding.UTF8.GetBytes(orderStatusCsv);
            var text = Encoding.GetEncoding("Windows-1251").GetBytes(orderStatusCsv);
            request.ContentLength = text.Length;
            request.Proxy = null;
            using (Stream body = request.GetRequestStream())
            {
                body.Write(text, 0, text.Length);
            }
            try
            {
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    // nothing, just close
                }
            }
            catch (Exception e)
            {
                InsertErrorMessage(e.Message);
            }
        }
        #endregion

        #endregion

        #region Файл [OrderProductsCsv]

        #region Читаем и записываем данные для файла [OrderProductsCsv]
        /// <summary> Читаем и записываем данные для файла [OrderProductsCsv] </summary>
        public List<Order> GetOrderProductInfo(List<Guid> orders)
        {
            var OrderProduct = new List<Order>();
            try
            {
                foreach (var orderId in orders)
                {
                    Select selectOrder = new Select(userConnection)
                        .Column("OrderProduct", "OrderId")
                        .Column("OrderProduct", "UsrOrderItemNumber")
                        .Column("Order", "Number")
                        .Column("OrderProduct", "Name")
                        .Column("OrderProduct", "UsrSKU")
                        .Column("OrderProduct", "Quantity")
                        .Column("OrderProduct", "UsrInStock")
                        .Column("Order", "UsrWeight")
                        .Column("OrderProduct", "UsrOriginalPrice")
                        .Column("OrderProduct", "Price")
                        .Column("OrderProduct", "UsrDiscountedPrice")
                        .Column("OrderProduct", "DiscountAmount")
                        .Column("OrderProduct", "Amount")
                        .Column("OrderProduct", "UsrAmountPriceDiscount")
                        .Column("OrderProduct", "UsrOperation")
                    .From("Order")
                    .Join(JoinType.Inner, "OrderProduct")
                    .On("OrderProduct", "OrderId").IsEqual("Order", "Id")
                    .Where("Order", "Id").IsEqual(Column.Parameter(orderId)) as Select;
                    using (DBExecutor dbExecutor = userConnection.EnsureDBConnection())
                    {
                        using (IDataReader reader = selectOrder.ExecuteReader(dbExecutor))
                        {
                            while (reader.Read())
                            {
                                OrderProduct.Add(new Order
                                {
                                    UsrOrderItemNumber = reader.GetColumnValue<int>("UsrOrderItemNumber"),
                                    Number = reader.GetColumnValue<string>("Number"),
                                    Name = reader.GetColumnValue<string>("Name"),
                                    UsrSKU = reader.GetColumnValue<string>("UsrSKU"),
                                    Quantity = reader.GetColumnValue<double>("Quantity"),
                                    UsrInStock = reader.GetColumnValue<double>("UsrInStock"),
                                    UsrWeightProduct = reader.GetColumnValue<double>("UsrWeight"),
                                    UsrOriginalPrice = reader.GetColumnValue<double>("UsrOriginalPrice"),
                                    Price = reader.GetColumnValue<double>("Price"),
                                    UsrDiscountedPrice = reader.GetColumnValue<double>("UsrDiscountedPrice"),
                                    DiscountAmount = reader.GetColumnValue<double>("DiscountAmount"),
                                    Amount = reader.GetColumnValue<double>("Amount"),
                                    UsrAmountPriceDiscount = reader.GetColumnValue<double>("UsrAmountPriceDiscount"),
                                    UsrOperation = reader.GetColumnValue<string>("UsrOperation")
                                });
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                InsertErrorMessage(e.Message);
            }
            return OrderProduct.OrderBy(x => x.OrderId).ToList<Order>();
        }
        #endregion

        #region Формируем файл [OrderProductsCsv]
        /// <summary> Формируем файл [OrderProductsCsv] </summary>
        public string GenerateOrderProductsCsv(List<Order> orders)
        {
            var delimiter = ";";
            StringBuilder csvExport = new StringBuilder();
            try
            {
                foreach (var orderProduct in orders)
                {
                    csvExport.AppendLine(string.Join(delimiter, new string[] {
                      orderProduct.UsrOrderItemNumber.ToString(),
                      orderProduct.Number,
                      orderProduct.Name,
                      orderProduct.UsrSKU,
                      orderProduct.Quantity.ToString(),
                      orderProduct.UsrInStock.ToString(),
                      orderProduct.UsrWeightProduct.ToString(),
                      orderProduct.UsrOriginalPrice.ToString(),
                      orderProduct.Price.ToString(),
                      orderProduct.UsrDiscountedPrice.ToString(),
                      orderProduct.DiscountAmount.ToString(),
                      orderProduct.Amount.ToString(),
                      orderProduct.UsrAmountPriceDiscount.ToString(),
                      orderProduct.UsrOperation.ToString()
                }));
                }
            }
            catch(Exception e)
            {
                InsertErrorMessage(e.Message);
            }
            return csvExport.ToString();
        }
        #endregion

        #region Выгружаем файл [OrderProductsCsv] на FTP
        /// <summary> Выгружаем файл [OrderProductsCsv] на FTP </summary>
        public void UploadOrderProductsCsvFile(string ftpHost, string ftpUserName, string ftpPassword, string ftpFolderOutOrders, string orderProductsCsv)
        {
            var fileName = dateTime + "Products.csv";
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpHost + ftpFolderOutOrders + fileName);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
            var text = Encoding.GetEncoding("Windows-1251").GetBytes(orderProductsCsv);
            //var text = Encoding.UTF8.GetBytes(orderProductsCsv);
            //var text = Encoding.GetEncoding("Windows-1251").GetBytes(orderStatusCsv);
            request.ContentLength = text.Length;
            request.Proxy = null;
            using (Stream body = request.GetRequestStream())
            {
                body.Write(text, 0, text.Length);
            }
            try
            {
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    // nothing, just close
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region Данные для файла [OrderProductsCsv] записываем в логи
        /// <summary> Данные для файла [OrderProductsCsv] записываем в логи </summary>
        public void LoggerProduct(List<Order> products)
        {
            var delimiter = ";";
            StringBuilder allProducts = new StringBuilder();
            foreach (var orderProduct in products)
            {
                allProducts.AppendLine(string.Join(delimiter, new string[] {
                      orderProduct.UsrOrderItemNumber.ToString(),
                      orderProduct.Number,
                      orderProduct.Name,
                      orderProduct.UsrSKU,
                      orderProduct.Quantity.ToString(),
                      orderProduct.UsrInStock.ToString(),
                      orderProduct.UsrWeightProduct.ToString(),
                      orderProduct.UsrOriginalPrice.ToString(),
                      orderProduct.Price.ToString(),
                      orderProduct.UsrDiscountedPrice.ToString(),
                      orderProduct.DiscountAmount.ToString(),
                      orderProduct.Amount.ToString(),
                      orderProduct.UsrAmountPriceDiscount.ToString(),
                      orderProduct.UsrOperation.ToString()
                }));
            }
            InsertSuccessMessage("Products uploaded: " + allProducts.ToString());
        }
        #endregion

        #endregion

        #region Файл [OrderHeadersCsv]

        #region Читаем и записываем данные для файла [OrderHeadersCsv]
        /// <summary> Читаем и записываем данные для файла [OrderHeadersCsv] </summary>
        public List<Order> GetOrderHeadersInfo(List<Guid> orders)
        {
            var OrderHeaders = new List<Order>();
            foreach (var orderId in orders)
            {
                Select selectOrder = new Select(userConnection)
                    .Column("Order", "Id")
                    .Column("Order", "Number")
                    .Column("Order", "UsrTypeId")
                    .Column("Order", "UsrId")
                    .Column("Order", "StatusId")
                    .Column("Order", "OwnerId")
                    .Column("Order", "PaymentStatusId")
                    .Column("Order", "UsrDeliveryDate")
                    .Column("Order", "Amount")
                    .Column("Order", "UsrCostDelivery")
                    .Column("Order", "UsrWeight")
                    .Column("Order", "UsrPaymentMethod")
                    .Column("Order", "ReceiverName")
                    .Column("Contact", "Email")
                    .Column("Order", "ContactNumber")
                    .Column("Contact", "UsrNumberActiveCard")
                    .Column("Order", "Comment")
                    .Column("Order", "UsrParentId")
                    .Column("Order", "UsrDeliveryServiceId")
                    .Column("Order", "UsrDeliveryDepartment")
                    .Column("Order", "DeliveryTypeId")
                    .Column("Order", "UsrCity")
                    .Column("Order", "UsrAddress")
                    .Column("Order", "UsrHouse")
                    .Column("Order", "UsrCorps")
                    .Column("Order", "UsrApartment")
                    .Column("Order", "UsrCityCode")
                    .Column("Order", "UsrStreetCode")
                    .Column("Order", "UsrFigureCount")
                    .Column("Order", "UsrWfpOrder")
                    .Column("Order", "UsrOperationDelivery")
                    .Column("Order", "UsrNotificationNumber")
                    .Column("Order", "UsrCustomerEmail")
                    .Column("Order", "UsrRegionCode")
                .From("Order")
                .Join(JoinType.LeftOuter, "Contact")
                .On("Contact", "Id").IsEqual("Order", "ContactId")
                .Where("Order", "Id").IsEqual(Column.Parameter(orderId)) as Select;
                using (DBExecutor dbExecutor = userConnection.EnsureDBConnection())
                {
                    using (IDataReader reader = selectOrder.ExecuteReader(dbExecutor))
                    {
                        while (reader.Read())
                        {
                            var statusId = reader.GetColumnValue<Guid>("StatusId");
                            var status = GetLookupBPM("OrderStatus", "Id", statusId);
                            var ownerId = reader.GetColumnValue<Guid>("OwnerId");
                            var owner = GetLookupBPM("Contact", "Id", ownerId);
                            var parentOrderId = reader.GetColumnValue<Guid>("UsrParentId");
                            var parentOrder = GetLookupBPM("Order", "Id", parentOrderId);
                            var deliveryServiceId = reader.GetColumnValue<Guid>("UsrDeliveryServiceId");
                            var deliveryService = GetLookupBPM("UsrDeliveryService", "Id", deliveryServiceId);
                            var deliveryTypeId = reader.GetColumnValue<Guid>("DeliveryTypeId");
                            var deliveryType = GetLookupBPM("DeliveryType", "Id", deliveryTypeId);
                            var paymentStatusId = reader.GetColumnValue<Guid>("PaymentStatusId");
                            var paymentStatus = GetLookupBPM("OrderPaymentStatus", "Id", paymentStatusId);
                            var typeId = reader.GetColumnValue<Guid>("UsrTypeId");
                            var type = GetLookupBPM("UsrOrderType", "Id", typeId);
                            OrderHeaders.Add(new Order
                            {
                                Number = reader.GetColumnValue<string>("Number"),
                                UsrType = type,
                                UsrId = reader.GetColumnValue<string>("UsrId"),
                                Status = status,
                                Owner = owner,
                                PaymentStatus = paymentStatus,
                                PaymentAmount = reader.GetColumnValue<double>("Amount"),
                                UsrWeightOrder = reader.GetColumnValue<double>("UsrWeight"),
                                UsrPaymentMethod = reader.GetColumnValue<string>("UsrPaymentMethod"),
                                UsrDeliveryDate = reader.GetColumnValue<DateTime>("UsrDeliveryDate"),
                                Contact = reader.GetColumnValue<string>("ReceiverName"),
                                Email = reader.GetColumnValue<string>("UsrCustomerEmail"),
                                ContactNumber = reader.GetColumnValue<string>("ContactNumber"),
                                UsrNumberActiveCard = reader.GetColumnValue<string>("UsrNumberActiveCard"),
                                Comment = reader.GetColumnValue<string>("Comment"),
                                UsrParent = parentOrder,
                                UsrDeliveryService = deliveryService,
                                UsrDeliveryDepartment = reader.GetColumnValue<string>("UsrDeliveryDepartment"),
                                DeliveryType = deliveryType,
                                UsrCity = reader.GetColumnValue<string>("UsrCity"),
                                UsrAddress = reader.GetColumnValue<string>("UsrAddress"),
                                UsrHouse = reader.GetColumnValue<string>("UsrHouse"),
                                UsrCorps = reader.GetColumnValue<string>("UsrCorps"),
                                UsrApartment = reader.GetColumnValue<string>("UsrApartment"),
                                UsrCostDelivery = reader.GetColumnValue<double>("UsrCostDelivery"),
                                UsrCityCode = reader.GetColumnValue<string>("UsrCityCode"),
                                UsrStreetCode = reader.GetColumnValue<string>("UsrStreetCode"),
                                UsrFigureCount = reader.GetColumnValue<int>("UsrFigureCount"),
                                UsrWfpOrder = reader.GetColumnValue<string>("UsrWfpOrder"),
                                UsrOperationDelivery = reader.GetColumnValue<string>("UsrOperationDelivery"),
                                UsrNotificationNumber = reader.GetColumnValue<string>("UsrNotificationNumber"),
                                UsrRegionCode = reader.GetColumnValue<string>("UsrRegionCode")
                            });
                        }
                    }
                }
            }
            return OrderHeaders;
        }
        #endregion

        #region Формируем файл [OrderHeadersCsv]
        /// <summary> Формируем файл [OrderHeadersCsv] </summary>
        public string GenerateOrderHeadersCsv(List<Order> orders)
        {
            var delimiter = ";";
            StringBuilder csvExport = new StringBuilder();
            foreach (var orderHeader in orders)
            {
                csvExport.AppendLine(string.Join(delimiter, new string[] {
                    orderHeader.Number,
                    orderHeader.UsrType,
                    orderHeader.UsrId,
                    orderHeader.Status,
                    orderHeader.Owner,
                    orderHeader.PaymentStatus,
                    orderHeader.PaymentAmount.ToString(),
                    orderHeader.UsrWeightOrder.ToString(),
                    orderHeader.UsrPaymentMethod,
                    orderHeader.UsrDeliveryDate.ToString(),
                    orderHeader.Contact,
                    orderHeader.Email,
                    orderHeader.ContactNumber,
                    orderHeader.UsrNumberActiveCard,
                    orderHeader.Comment,
                    orderHeader.UsrParent,
                    orderHeader.UsrDeliveryService,
                    orderHeader.UsrDeliveryDepartment,
                    orderHeader.DeliveryType,
                    orderHeader.UsrCity,
                    orderHeader.UsrAddress,
                    orderHeader.UsrHouse,
                    orderHeader.UsrCorps,
                    orderHeader.UsrApartment,
                    orderHeader.UsrCostDelivery.ToString(),
                    orderHeader.UsrCityCode,
                    orderHeader.UsrStreetCode,
                    orderHeader.UsrFigureCount.ToString(),
                    orderHeader.UsrWfpOrder,
                    orderHeader.UsrOperationDelivery.ToString(),
                    orderHeader.UsrNotificationNumber,
                    orderHeader.UsrRegionCode
                }));
            }
            return csvExport.ToString();
        }
        #endregion

        #region Выгружаем файл [OrderHeadersCsv] на FTP
        /// <summary> Выгружаем файл [OrderHeadersCsv] на FTP </summary>
        public void UploadOrderHeadersCsvFile(string ftpHost, string ftpUserName, string ftpPassword, string ftpFolderOutOrders, string orderHeadersCsv)
        {
            var fileName = dateTime + "Headers.csv";
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpHost + ftpFolderOutOrders + fileName);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
            var text = Encoding.GetEncoding("Windows-1251").GetBytes(orderHeadersCsv);
            request.ContentLength = text.Length;
            request.Proxy = null;
            using (Stream body = request.GetRequestStream())
            {
                body.Write(text, 0, text.Length);
            }
            try
            {
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    // nothing, just close
                }
            }
            catch (Exception e)
            {
              InsertErrorMessage(e.Message);
            }
        }
        #endregion

        #endregion

        #region Файл [OrderDiscountsCsv]

        #region Читаем и записываем данные для файла [OrderDiscountsCsv]
        /// <summary> Читаем и записываем данные для файла [OrderDiscountsCsv] </summary>
        public List<Order> GetOrderDiscountsInfo(List<Guid> orders)
        {
            var OrderDiscounts = new List<Order>();
            foreach (var orderId in orders)
            {
                Select selectOrder = new Select(userConnection)
                    .Column("UsrShares", "UsrOrderSharesId")
                    .Column("UsrShares", "UsrOrderItemNumber")
                    .Column("UsrShares", "UsrProductSharesId")
                    .Column("UsrShares", "UsrSKU")
                    .Column("UsrShares", "UsrQuantity")
                    .Column("UsrShares", "UsrRuleId")
                    .Column("UsrShares", "UsrRuleName")
                    .Column("UsrShares", "UsrPrice")
                    .Column("UsrShares", "UsrAmountPrice")
                    .Column("UsrShares", "UsrDiscountPercent")
                    .Column("UsrShares", "UsrDiscount")
                    .Column("UsrShares", "UsrDiscountAmount")
                    .Column("UsrShares", "UsrPriceDiscounted")
                    .Column("UsrShares", "UsrAmountPriceDiscounted")
                    .Column("UsrShares", "UsrSetId")
                .From("UsrShares")
                .Join(JoinType.Inner, "Order")
                .On("Order", "Id").IsEqual("UsrShares", "UsrOrderSharesId")
                .Join(JoinType.LeftOuter, "Product")
                .On("Product", "Id").IsEqual("UsrShares", "UsrProductSharesId")
                .Where("UsrShares", "UsrOrderSharesId").IsEqual(Column.Parameter(orderId)) as Select;
                using (DBExecutor dbExecutor = userConnection.EnsureDBConnection())
                {
                    using (IDataReader reader = selectOrder.ExecuteReader(dbExecutor))
                    {
                        while (reader.Read())
                        {
                            var orderSharesId = reader.GetColumnValue<Guid>("UsrOrderSharesId");
                            var order = GetLookupBPM("Order", "Id", orderSharesId);
                            var productSharesId = reader.GetColumnValue<Guid>("UsrProductSharesId");
                            var product = GetLookupBPM("Product", "Id", productSharesId);
                            OrderDiscounts.Add(new Order
                            {
                                Number = order,
                                UsrOrderItemNumber = reader.GetColumnValue<int>("UsrOrderItemNumber"),
                                ProductName = product,
                                UsrSKU = reader.GetColumnValue<string>("UsrSKU"),
                                UsrQuantity = reader.GetColumnValue<double>("UsrQuantity"),
                                UsrRuleId = reader.GetColumnValue<string>("UsrRuleId"),
                                UsrRuleName = reader.GetColumnValue<string>("UsrRuleName"),
                                UsrPrice = reader.GetColumnValue<double>("UsrPrice"),
                                UsrAmountPrice = reader.GetColumnValue<double>("UsrAmountPrice"),
                                UsrDiscountPercent = reader.GetColumnValue<double>("UsrDiscountPercent"),
                                UsrDiscount = reader.GetColumnValue<double>("UsrDiscount"),
                                UsrDiscountAmount = reader.GetColumnValue<double>("UsrDiscountAmount"),
                                UsrPriceDiscounted = reader.GetColumnValue<double>("UsrPriceDiscounted"),
                                UsrAmountPriceDiscounted = reader.GetColumnValue<double>("UsrAmountPriceDiscounted"),
                                UsrSetId = reader.GetColumnValue<double>("UsrSetId"),
                            });
                        }
                    }
                }
            }
            return OrderDiscounts;
        }
        #endregion

        #region Формируем файл [OrderDiscountsCsv]
        /// <summary> Формируем файл [OrderDiscountsCsv] </summary>
        public string GenerateOrderDiscountsCsv(List<Order> orders)
        {
            var delimiter = ";";
            StringBuilder csvExport = new StringBuilder();
            foreach (var orderDiscount in orders)
            {
                csvExport.AppendLine(string.Join(delimiter, new string[] {
                    orderDiscount.Number,
                    orderDiscount.UsrOrderItemNumber.ToString(),
                    orderDiscount.ProductName,
                    orderDiscount.UsrSKU,
                    orderDiscount.UsrQuantity.ToString(),
                    orderDiscount.UsrRuleId,
                    orderDiscount.UsrRuleName,
                    orderDiscount.UsrPrice.ToString(),
                    orderDiscount.UsrAmountPrice.ToString(),
                    orderDiscount.UsrDiscountPercent.ToString(),
                    orderDiscount.UsrDiscount.ToString(),
                    orderDiscount.UsrDiscountAmount.ToString(),
                    orderDiscount.UsrPriceDiscounted.ToString(),
                    orderDiscount.UsrAmountPriceDiscounted.ToString(),
                    orderDiscount.UsrSetId.ToString(),
                }));
            }
            return csvExport.ToString();
        }
        #endregion

        #region Выгружаем файл [OrderDiscountsCsv] на FTP
        /// <summary> Выгружаем файл [OrderDiscountsCsv] на FTP </summary>
        public void UploadOrderDiscountsCsvFile(string ftpHost, string ftpUserName, string ftpPassword, string ftpFolderOutOrders, string orderDiscountsCsv)
        {
            var fileName = dateTime + "Discounts.csv";
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpHost + ftpFolderOutOrders + fileName);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
            var text = Encoding.GetEncoding("Windows-1251").GetBytes(orderDiscountsCsv);
            request.ContentLength = text.Length;
            request.Proxy = null;
            using (Stream body = request.GetRequestStream())
            {
                body.Write(text, 0, text.Length);
            }
            try
            {
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    // nothing, just close
                }
            }
            catch (Exception e)
            {
                InsertErrorMessage(e.Message);
            }
        }
        #endregion

        #endregion

        #region Получение значения из базы данных
        /// <summary> Получение значения из базы данных </summary>
        public string GetLookupBPM(string table, string column, Guid value)
        {
            if (value == null)
            {
                return String.Empty;
            }
            if (table == "Order")
            {
                var orderName = (new Select(userConnection).Top(1)
                    .Column("Number")
                    .From(table)
                    .Where(column).IsEqual(Column.Parameter(value)) as Select).ExecuteScalar<string>();
                return orderName;
            }
            if (table == "OrderStatus")
            {
                var orderStatus = (new Select(userConnection).Top(1)
                    .Column("UsrCode")
                    .From(table)
                    .Where(column).IsEqual(Column.Parameter(value)) as Select).ExecuteScalar<string>();
                return orderStatus;
            }
            if (table == "UsrDeliveryService")
            {
                var deliveryService = (new Select(userConnection).Top(1)
                        .Column("UsrERPCode")
                        .From(table)
                        .Where(column).IsEqual(Column.Parameter(value)) as Select).ExecuteScalar<string>();
                return deliveryService;
            }
            if (table == "OrderPaymentStatus")
            {
                var paymentStatus = (new Select(userConnection).Top(1)
                        .Column("UsrPaymentStatusCode")
                        .From(table)
                        .Where(column).IsEqual(Column.Parameter(value)) as Select).ExecuteScalar<string>();
                return paymentStatus;
            }
            if (table == "DeliveryType")
            {
                var deliveryType = (new Select(userConnection).Top(1)
                        .Column("UsrErpCode")
                        .From(table)
                        .Where(column).IsEqual(Column.Parameter(value)) as Select).ExecuteScalar<string>();
                return deliveryType;
            }
            var data = (new Select(userConnection).Top(1)
                    .Column("Name")
                    .From(table)
                    .Where(column).IsEqual(Column.Parameter(value)) as Select).ExecuteScalar<string>();
            return data;
        }
        #endregion

        #region Запись данных в таблицу логирования
        /// <summary> Записывает ErrorMessage в таблицу логирования </summary>
        public void InsertErrorMessage(string logMessage)
        {
            Insert insert = new Insert(userConnection).Into("UsrIntegrationLogFtp")
                .Set("UsrName", Column.Parameter("Выгрузка данных на FTP EVA ошибка"))
                .Set("UsrErrorDescription", Column.Parameter(logMessage));
            insert.Execute();
        }

        /// <summary> Записывает SuccessMessage в таблицу логирования </summary>
        public void InsertSuccessMessage(string logMessage)
        {
            Insert insert = new Insert(userConnection).Into("UsrIntegrationLogFtp")
                .Set("UsrName", Column.Parameter("Выгрузка данных на FTP EVA"))
                .Set("UsrDescription", Column.Parameter(logMessage));
            insert.Execute();
        }
        #endregion

        #region Записываем признак выгрузки заказа на FTP по заказу
        /// <summary> Записываем признак выгрузки заказа на FTP по заказу </summary>
        public void UpdateUploadParam()
        {
            foreach (Guid order in orderNewList)
            {
                var update = new Update(userConnection, "Order")
                                        .Set("UsrUploadAsNew", Column.Parameter("1"))
                                        .Where("Id").IsEqual(Column.Parameter(order));
                update.Execute();
            }
            foreach (Guid order in orderConfirmedList)
            {
                var update = new Update(userConnection, "Order")
                                        .Set("UsrUploadAsConfirmed", Column.Parameter("1"))
                                        .Where("Id").IsEqual(Column.Parameter(order));
                update.Execute();
            }
            foreach (Guid order in orderCanceledList)
            {
                var update = new Update(userConnection, "Order")
                                        .Set("UsrUploadAsCanceled", Column.Parameter("1"))
                                        .Where("Id").IsEqual(Column.Parameter(order));
                update.Execute();
            }
            foreach (Guid order in orderUnpaidList)
            {
                var update = new Update(userConnection, "Order")
                                        .Set("UsrUploadAsUnpaid", Column.Parameter("1"))
                                        .Where("Id").IsEqual(Column.Parameter(order));
                update.Execute();
            }
            foreach (Guid order in orderApplicationForRefusalOfDeliveryList)
            {
                var update = new Update(userConnection, "Order")
                                        .Set("UsrUploadAsApplicationForRefusalOfDelivery", Column.Parameter("1"))
                                        .Where("Id").IsEqual(Column.Parameter(order));
                update.Execute();
            }
        }
        #endregion

        #region Записываем дату выгрузки заказа на FTP по заказу
        /// <summary> Записываем дату выгрузки заказа на FTP по заказу </summary>
        public void InsertUnloadDate(DateTime unloadDate, List<Guid> ordersId)
        {
            foreach (var orderId in ordersId)
            {
                Select s = new Select(userConnection).
                    Column("UsrUnloadDate").From("Order").
                    Where("Id").IsEqual(Column.Parameter(orderId)) as Select;
                using (var dbExecutor = userConnection.EnsureDBConnection())
                {
                    using (var dataReader = s.ExecuteReader(dbExecutor))
                    {
                        if (dataReader.Read())
                        {
                            var date = dataReader.GetColumnValue<DateTime>("UsrUnloadDate");
                            if (date == DateTime.MinValue || date == null)
                            {
                                var orderEntity = userConnection.EntitySchemaManager.GetInstanceByName("Order").CreateEntity(userConnection);
                                if (orderEntity.FetchFromDB("Id", orderId))
                                {
                                    orderEntity.SetColumnValue("UsrUnloadDate", unloadDate);
                                    try
                                    {
                                        orderEntity.Save(false);
                                    }
                                    catch (Exception ex)
                                    {
                                        InsertErrorMessage("Update Unloaddate error. " + ex.Message);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Предоставляет ошибку, которая вызвала текущее исключение
        /// <summary> Предоставляет ошибку, которая вызвала текущее исключение </summary>
        private string GetStrException(Exception err)
        {
            return err.InnerException == null ? err.Message : GetStrException(err.InnerException);
        }
        #endregion
    
    }

    public class Order
	{

        #region Заказ
        /// <summary> [UI]: "Заказ.Дата изменения"; [DB]: "Order.ModifiedOn" </summary>
		public DateTime ModifiedOn { get; set; }

        /// <summary> [UI]: "Заказ.Дата подтверждения"; [DB]: "Order.UsrApproveDate" </summary>
		public DateTime UsrApproveDate { get; set; }

        /// <summary> [UI]: "Заказ.Номер"; [DB]: "Order.Number" </summary>
        public string Number { get; set; }

        /// <summary> [UI]: "Заказ.Номер"; [DB]: "Order.Number" </summary>
		public string UsrParent { get; set; }

        /// <summary> [UI]: "Заказ.ID заказа ИМ"; [DB]: "Order.UsrId" </summary>
		public string UsrId { get; set; }

        /// <summary> [UI]: "Заказ.Стоимость доставки"; [DB]: "Order.UsrCostDelivery" </summary>
		public double UsrCostDelivery { get; set; }

        /// <summary> [UI]: "Заказ.Итого"; [DB]: "Order.Amount" </summary>
		public double PaymentAmount { get; set; }

        /// <summary> [UI]: "Заказ.Метод оплаты"; [DB]: "Order.UsrPaymentMethod" </summary>
		public string UsrPaymentMethod { get; set; }

        /// <summary> [UI]: "Заказ.Имя получателя"; [DB]: "Order.ReceiverName" </summary>
		public string Contact { get; set; }

        /// <summary> [UI]: "Заказ.Email для уведомлений"; [DB]: "Order.UsrCustomerEmail" </summary>
		public string Email { get; set; }

        /// <summary> [UI]: "Заказ.Номер активной карты"; [DB]: "Order.UsrNumberActiveCard" </summary>
		public string UsrNumberActiveCard { get; set; }

        /// <summary> [UI]: "Заказ.Контактный телефон"; [DB]: "Order.ContactNumber" </summary>
		public string ContactNumber { get; set; }

        /// <summary> [UI]: "Заказ.Комментарий"; [DB]: "Order.Comment" </summary>
		public string Comment { get; set; }

        /// <summary> [UI]: "Заказ.Отделение доставки"; [DB]: "Order.UsrDeliveryDepartmentId" </summary>
		public string UsrDeliveryDepartment { get; set; }

        /// <summary> [UI]: "Заказ.Город"; [DB]: "Order.UsrCity" </summary>
		public string UsrCity { get; set; }

        /// <summary> [UI]: "Заказ.Улица"; [DB]: "Order.UsrAddress" </summary>
		public string UsrAddress { get; set; }

        /// <summary> [UI]: "Заказ.Дом"; [DB]: "Order.UsrHouse" </summary>
		public string UsrHouse { get; set; }

        /// <summary> [UI]: "Заказ.Корпус"; [DB]: "Order.UsrCorps" </summary>
		public string UsrCorps { get; set; }

        /// <summary> [UI]: "Заказ.Квартира"; [DB]: "Order.UsrApartment" </summary>
		public string UsrApartment { get; set; }

        /// <summary> [UI]: "Заказ.Общий вес заказа"; [DB]: "Order.UsrWeight" </summary>
		public double UsrWeightProduct { get; set; }

        /// <summary> [UI]: "Заказ.Общий вес заказа"; [DB]: "Order.UsrWeight" </summary>
		public double UsrWeightOrder { get; set; }

        /// <summary> [UI]: "Заказ.Дата доставки"; [DB]: "Order.UsrDeliveryDate" </summary>
		public DateTime UsrDeliveryDate { get; set; }

        /// <summary> [UI]: "Заказ.Код города ИМ"; [DB]: "Order.UsrCityCode" </summary>
		public string UsrCityCode { get; set; }

        /// <summary> [UI]: "Заказ.Код улицы"; [DB]: "Order.UsrStreetCode" </summary>
        public string UsrStreetCode { get; set; }

        /// <summary> [UI]: "Заказ.Код региона ИМ"; [DB]: "Order.UsrRegionCode" </summary>
        public string UsrRegionCode { get; set; }

        /// <summary> [UI]: "Заказ.Количество фигурок"; [DB]: "Order.UsrFigureCount" </summary>
        public int UsrFigureCount { get; set; }

        /// <summary> [UI]: "Заказ.WfpOrder"; [DB]: "Order.UsrWfpOrder" </summary>
		public string UsrWfpOrder { get; set; }

        /// <summary> [UI]: "Заказ.Id операции доставки"; [DB]: "Order.UsrOperationDelivery" </summary>
		public string UsrOperationDelivery { get; set; }

        /// <summary> [UI]: "Заказ.Номер для уведомлений"; [DB]: "Order.UsrNotificationNumber" </summary>
        public string UsrNotificationNumber { get; set; }

        /// <summary> [UI]: "Заказ.Email для уведомлений"; [DB]: "Order.UsrCustomerEmail" </summary>
        public string UsrCustomerEmail { get; set; }

        /// <summary> [UI]: "Контакт.ФИО"; [DB]: "Contact.Name" </summary>
        public string Owner { get; set; }
        /// <summary> [UI]: "Состояние оплаты заказа.Код"; [DB]: "OrderPaymentStatus.UsrPaymentStatusCode" </summary>
		public string PaymentStatus { get; set; }
        /// <summary> [UI]: "Состояние заказа.ID код"; [DB]: "OrderStatus.UsrCode" </summary>
        public string Status { get; set; }

        /// <summary> [UI]: "Тип доставки.ERP Code"; [DB]: "DeliveryType.UsrErpCode" </summary>
		public string DeliveryType { get; set; }

        /// <summary> [UI]: "Сервисы доставки.Код"; [DB]: "UsrDeliveryService.UsrERPCode" </summary>
		public string UsrDeliveryService { get; set; }

        /// <summary> [UI]: "Тип заказа.Название"; [DB]: "UsrOrderType.Name" </summary>
		public string UsrType { get; set; }
        #endregion
        
        #region Продукт в заказе
        /// <summary> [UI]: "Продукт в заказе.Заказ"; [DB]: "OrderProduct.OrderId" </summary>
        public Guid OrderId { get; set; }

        /// <summary> [UI]: "Продукт в заказе.Название"; [DB]: "OrderProduct.Name" </summary>
		public string Name { get; set; }

        /// <summary> [UI]: "Продукт в заказе.SKU"; [DB]: "OrderProduct.UsrSKU" </summary>
		public string UsrSKU { get; set; }

        /// <summary> [UI]: "Продукт в заказе.Количество"; [DB]: "OrderProduct.Quantity" </summary>
		public double Quantity { get; set; }

        /// <summary> [UI]: "Продукт в заказе.В наличии"; [DB]: "OrderProduct.UsrInStock" </summary>
		public double UsrInStock { get; set; }

        /// <summary> [UI]: "Продукт в заказе.Первоначальная цена товара"; [DB]: "OrderProduct.UsrOriginalPrice" </summary>
		public double UsrOriginalPrice { get; set; }

        /// <summary> [UI]: "Продукт в заказе.Цена"; [DB]: "OrderProduct.Price" </summary>
		public double Price { get; set; }

        /// <summary> [UI]: "Продукт в заказе.Сумма скидки"; [DB]: "OrderProduct.DiscountAmount" </summary>
		public double DiscountAmount { get; set; }

        /// <summary> [UI]: "Продукт в заказе.Сумма"; [DB]: "OrderProduct.Amount" </summary>
		public double Amount { get; set; }

        /// <summary> [UI]: "Продукт в заказе.Сумма товара (после всех скидок)"; [DB]: "OrderProduct.UsrAmountPriceDiscount" </summary>
		public double UsrAmountPriceDiscount { get; set; }

        /// <summary> [UI]: "Продукт в заказе.Цена товара после скидки"; [DB]: "OrderProduct.UsrDiscountedPrice" </summary>
        public double UsrDiscountedPrice { get; set; }

        /// <summary> [UI]: "Продукт в заказе.Код позиции"; [DB]: "OrderProduct.UsrOrderItemNumber" </summary>
		public int UsrOrderItemNumber { get; set; }

        /// <summary> [UI]: "Продукт в заказе.UsrOperation"; [DB]: "OrderProduct.UsrOperation" </summary>
        public string UsrOperation { get; set; }

        /// <summary> [UI]: "Продукт.Название"; [DB]: "Product.Name" </summary>
        public string ProductName { get; set; }
        #endregion
        
        #region Акции по продуктам
        /// <summary> [UI]: "Акции по продуктам.Количество"; [DB]: "UsrShares.UsrQuantity" </summary>
        public double UsrQuantity { get; set; }

        /// <summary> [UI]: "Акции по продуктам.Номер акции"; [DB]: "UsrShares.UsrRuleId" </summary>
		public string UsrRuleId { get; set; }

        /// <summary> [UI]: "Акции по продуктам.Название Акции"; [DB]: "UsrShares.UsrRuleName" </summary>
		public string UsrRuleName { get; set; }

        /// <summary> [UI]: "Акции по продуктам.Цена товара до скидки"; [DB]: "UsrShares.UsrPrice" </summary>
		public double UsrPrice { get; set; }

        /// <summary> [UI]: "Акции по продуктам.Сумма товара до скидки"; [DB]: "UsrShares.UsrAmountPrice" </summary>
		public double UsrAmountPrice { get; set; }

        /// <summary> [UI]: "Акции по продуктам.Скидка на товар в %"; [DB]: "UsrShares.UsrDiscountPercent" </summary>
		public double UsrDiscountPercent { get; set; }

        /// <summary> [UI]: "Акции по продуктам.Скидка на ед.товара"; [DB]: "UsrShares.UsrDiscount" </summary>
		public double UsrDiscount { get; set; }

        /// <summary> [UI]: "Акции по продуктам.Сумма скидки на товар"; [DB]: "UsrShares.UsrDiscountAmount" </summary>
		public double UsrDiscountAmount { get; set; }

        /// <summary> [UI]: "Акции по продуктам.Цена товара после скидки"; [DB]: "UsrShares.UsrPriceDiscounted" </summary>
		public double UsrPriceDiscounted { get; set; }

        /// <summary> [UI]: "Акции по продуктам.Сумма товара после скидки"; [DB]: "UsrShares.UsrAmountPriceDiscounted" </summary>
		public double UsrAmountPriceDiscounted { get; set; }

        /// <summary> [UI]: "Акции по продуктам.Номер Комплекта"; [DB]: "UsrShares.UsrSetId" </summary>
		public double UsrSetId { get; set; }
        #endregion

        #region Не используем
        public int UsrPositionNumber { get; set; }
        public int UsrIsInSet { get; set; }
        public Guid UsrOrderSharesId { get; set; }
        public Guid UsrProductSharesId { get; set; }
        #endregion
        
    }

    public class Response
	{
		public bool Success { get; set; }
		public string Error { get; set; }
	}
}