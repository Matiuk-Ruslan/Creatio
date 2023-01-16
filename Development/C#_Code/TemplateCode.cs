

#region Метод для генерации пароля

var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefgijklmnopqrstuvwxyz0123456789@";
var stringChars = new char[12];
var random = new Random();

for (int i = 0; i < stringChars.Length; i++)
{
    stringChars[i] = chars[random.Next(chars.Length)];
}

var finalString = new String(stringChars);
Set("Password", finalString);
return true;

#endregion

#region Метод очистки номер от ненужных символов

string a = "+38 (050) 596-96-52";
string b = a.Replace("+", string.Empty).Replace(" ", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty);
Console.WriteLine(b);

#endregion

#region Метод разделения email и ФИО

string senderEnum = @"Матюк Руслан Александрович <matiuk.ruslan@gmail.com>;";
string mail = Regex.Match(senderEnum, "<.*>").Captures[0].Value.Replace("<", string.Empty).Replace(">", string.Empty);
string user = Regex.Match(senderEnum, "^.*?<").Captures[0].Value;
//user = user.Substring(0, user.Length - 2); // это по старому
user = user[0..^2]; // это по новому
Console.WriteLine(mail);
Console.WriteLine(user);

#endregion

#region Создание напоминания из CSharp кода 
//var reminding = userConnection.EntitySchemaManager.GetInstanceByName("Reminding").CreateEntity(userConnection);
//reminding.SetDefColumnValues();
//reminding.SetColumnValue("Id", Guid.NewGuid());
//reminding.SetColumnValue("NotificationTypeId", "685E7149-C015-4A4D-B4A6-2E5625A6314C");
//reminding.SetColumnValue("AuthorId", "410006e1-ca4e-4502-a9ec-e54d922d2c00");
//reminding.SetColumnValue("ContactId", "410006e1-ca4e-4502-a9ec-e54d922d2c00");
//reminding.SetColumnValue("SubjectCaption", "Заголовок тестового сообщения");
//reminding.SetColumnValue("PopupTitle", "Заголовок всплывающего окна тестового сообщения");
//reminding.SetColumnValue("SysEntitySchemaId", "80294582-06B5-4FAA-A85F-3323E5536B71");
//reminding.SetColumnValue("RemindTime", DateTime.Now);
//reminding.Save();
#endregion

#region Вызов бизнес-процесса из CSharp кода 
//var proccessStart = userConnection.ProcessSchemaManager.GetInstanceByName("Proccess_Name");
//var flowEngine = new FlowEngine(userConnection);
//Dictionary<string, string> parameter = new Dictionary<string, string>();
//parameter.Add("OrderId", orderId);
//flowEngine.RunProcess(proccessStart, parameter);
#endregion

#region Создание свойства
public string AccountName 
{
    get { return GetTypedColumnValue<string>("AccountName"); }
    set { SetColumnValue("AccountName", value); if (_account != null) { _account.Name = value; } }
}
#endregion