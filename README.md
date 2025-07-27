# Описание
YaSDK - плагин для Unity, предоставляющий интеграцию с SDK Яндекс игр используя колбэки. Плагин предоставляет удобный API для работы с платформой "Яндекс.Игры": реклама, покупки, таблицы лидеров и прочее.
## Содержание
- [Инициализация](#инициализация)
- [Разметка геймплея](#разметка-геймплея)
- [Данные игрока](#данные-игрока)
- [Реклама](#реклама)
- [Инап-покупки](#инап-покупки)
- [Лидерборд](#лидерборд)
- [Переменные окружения](#Переменные-окружения)
- [Серверное время](#Серверное-время)

# Инициализация 
Плагин инициализируется автоматически (при условии использования шаблона предоставляемого плагином). 
Чтобы проверить что плагин и СДК готовы к работе, используйте: 
```c#
YaSDK.IsSDKReady()
```
Метод вернет `true` либо `false`. 
##### Пример использования
```c#
public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(WaitForSDK());       
    }
    
    IEnumerator WaitForSDK()
	{
    yield return new WaitUntil(() => YaSDK.IsSDKReady() == true);
    SceneManager.LoadScene(1);
    // Или вызывайте метод старта игры, если загрузочной сцены нет
	}
}

```


# Разметка геймплея
#### GameReady

Метод `YaSDK.GameReady()` нужно вызывать, когда игра загрузила все ресурсы и готова к взаимодействию с пользователем.
#### GameplayStart
Метод `YaSDK.GameplayStart()` нужно вызывать в случаях, когда игрок начинает или возобновляет игровой процесс.
#### GameplayStop
Метод `YaSDK.GameplayStop()` нужно вызывать в случаях, когда игрок приостанавливает или завершает игровой процесс.
# Данные игрока
Данные игрока загружаются автоматически при инициализации плагина. 
В папке плагина есть скрипт `Saves.cs`. В нем вы создаете свои поля, которые хотите сохранить. Затем на протяжении игры вы изменяете данные как вам нужно, и вызываете метод сохранения, он отправит данные на сервер. При первом входе в игру, поля примут значения по умолчанию, которые вы можете установить.
#### Сохранения
##### Пример
```c#
[Serializable]
public class Saves 
{
	public int Score = 0; // 0 - по умолчанию
	public int Money = 500; // 500 - по умолчанию
	public CarData Car;	
}

[Serializable] 
public class CarData 
{
	public string Model = "Matiz";
	public float MaxSpeed = 136;
	public int Level = 1;
}
```
**Обязательно помечайте свои классы `[Serializable]`!**
Во время игры изменяете поля, как вам нужно:
```c#
public void CarUpgrade(){
	YaSDK.Saves.Car.Level += 1;
	YaSDK.Saves.Car.MaxSpeed += 5;
}
```
Затем, сохраняете данные когда посчитаете нужным. Имейте ввиду, есть ограничения со стороны Яндекс Игр. Это на вашей ответственности:
```c#
public void Loosed(int score)
{
	if(YaSDK.Saves.Score < score)
	{
		YaSDK.Saves.Score = score;
		YaSDK.SavePlayerData()
	}
}
```
После вызова `YaSDK.SavePlayerData()` всё, что у вас хранится в `YaSDK.Saves` будет сохранено на сервере Яндекс Игр.
#### Получение сохраненных данных игрока
Так как данные с сервера приходят при инициализации плагина, запрашивать данные посреди игры нужно в редких случаях ([[#Авторизация#Пример]]). Для получения данных используем метод: `YaSDK.GetPlayerData(Action<Save> onRecive = null)` 
#### Проверка авторизации 
Метод `YaSDK.IsPlayerAuthorized()` вернет `true` если игрок авторизован, `false` если нет.
#### Авторизация
Для открытия окна авторизации вызовите метод `YaSDK.OpenAuthDialog()`. Он принимает 2 необязательных параметра:
- `onAuth` - вызывается при успешной авторизации пользователя
- `onReject` - вызывается если игрок не авторизовался
**Перед показом окна авторизации проверяйте не авторизован ли игрок! `YaSDK.IsPlayerAuthorized()`.** Если вызовете окно авторизации, а игрок уже авторизован, колбэк `onReject` будет вызван сразу.
##### Пример
```C#
public void Auth() 
{
	YaSDK.OpenAuthDialog(onAuth: () => {
		// Тут игрок уже авторизован и мы хотим перенести сохранения, если надо
		YaSDK.GetPlayerData(onRecive: saves => {
			// Если счёт больше на неавторизованном, то обновляем сохранения на сервере
			if(YaSDK.Saves.Score > saves.Score)
			{
				YaSDK.SavePlayerData();
			}
			// или если на аккаунте прогресса больше, то загружаем данные с авторизованого аккаунта			
			if(YaSDK.Saves.Score < saves.Score)
			{
				YaSDK.SetSaves(saves); // подменяем сохранения полностью
			}
		});
	});
}
```
# Удаленная конфигурация (флаги)
На данный момент флаги инициализируются при старте. Чтобы получить флаг вызывается метод `YaSDK.GetFlag("flag_name")`. Вернется значение флага типа `string`.
# Реклама
### Полноэкранная реклама
Для показа полноэкранной рекламы надо вызвать метод `YaSDK.ShowInterstitial()`.
Он принимает 3 необязательных колбэка. 
##### Пример
```c#
YaSDK.ShowInterstitial(
	onClose: wasShown => {
		if(wasShown)
		{
			GiveCoin(5);
			Debug.Log("реклама была показана. Дадим 5 монеток, просто так");
		}
});
```
Полная сигнатура метода: 
```c#
ShowInterstitial(Action onOpen = null, Action<bool> onClose = null, Action<string> onError = null)
```

### Реклама за вознаграждение
Для показа рекламы за вознаграждение вызываем метод `ShowRewarded(Action<string> onRewarded`. У него 1 обязательный аргумент (колбэк вознаграждения) и 4 необязательных.

* `string rewarded` - ваш id вознаграждения 
* `Action onOpen` - вызывается при открытии рекламы
* `Action onClose` - вызывается при закрытии
* `Action<string> onError` - при ошибке, с сообщением ошибки

#### Пример 1
Будем выдавать награду сразу в колбэке, поэтому rewarded можем не указывать. Он будет пустым.
```c#
public void GetFiatForReward()
{
	YaSDK.ShowRewarded(reward => 
	{
		YaSDK.Saves.Car.Model = "Fiat";
		Debug.Log("Поздравляем!");
	});
}
```

##### Пример 2
Передадим метод, который отвечает за выдачу наград и сохраняет накопленное.
```c#
public void RewardBoss(string reward)
{
	switch(reward)
	{
		case "Money":
			YaSDK.Saves.Money += 500;
			break;
		case "Fiat":
			YaSDK.Saves.Car.Model = "Fiat";
			break;
		...
	}
	YaSDK.SavePlayerData();
}

//somewhere
YaSDK.ShowRewarded(RewardBoss, rewarded: "Fiat");
```
В этом примере нам надо обязательно передать `rewarded`, чтобы знать, что выдавать в RewardBoss.
# Инап-покупки
#### Активация процесса покупки
Для активации процесса покупки используется метод: `YaSDK.Buy(string purchaseID, Action<PurchaseData> onSuccses, Action<string> onError = null)`.
Структура `PurchaseData`:
- string productID - ид товара из консоли разработчика
- string purchaseToken - токен покупки. Используется для подтверждения(консумирования)
- string developerPayload - дополнительные данные о покупке
##### Пример
```c#
public void Buy(string productId)
{
	YaSDK.Buy(productId, product => {
		if(product.productID == "money_500")
			YaSDK.Saves.Money += 500;
		else if(product.productID == "money_5000")
			YaSDK.Saves.Money += 5000;
		// и т.д.

		// Обязательно подтверждаем что товар был выдан!
		YaSDK.ConsumePurchase(product.purchaseToken)
	});
}
// либо отдельный метод для выдачи 
YaSDK.Buy("money_500", Purchase);

private void Purchase(PurchaseData data)
{
	if(data.productID == "money_500")
		YaSDK.Saves.Money += 500;
	else if(data.productID == "money_5000")
		YaSDK.Saves.Money += 5000;
		// и т.д.

	// Обязательно подтверждаем что товар был выдан!
	YaSDK.ConsumePurchase(product.purchaseToken)
}
```

#### Проверка необработанных покупок
Может случится, что игрок не получил товар, но уже заплатил. Для этого при запуске игры вызовете метод `GetPurchased(Action<PurchaseList> onPurchased)` - который вернет вам список необработанных покупок. Лучше использовать отдельный метод выдачи товаров из предыдущего примера, в котором мы выдавали товар и подтверждали выдачу 
##### Пример 
```c#
void Start()
{
	YaSDK.GetPurchased(purchaseList =>
	{
	    foreach (var purchase in purchaseList.purchases)
	    {
	        Purchase(purchase);// метод из предыдущего примера
	    }
	});
}
```

#### Получение каталога всех товаров
Для получения всех товаров, что были созданы в консоли разработчика, надо вызвать метод `YaSDK.GetCatalog(GetCatalog(Action<ProductList> onRecive, string iconSize = "small"))`. 
Он работает с `ProductList`, внутри которого массив типа `Product`. 
Структура `Product`:
- string id - идентификатор товара.
- string title - название
- string description - описание
- string imageURI - URL изображения
- string price - стоимость товара в формате `<цена> <код валюты>`.
- string priceValue -  стоимость товара в формате `<цена>`.
- string priceCurrencyCode -  код валюты.
- string currencyIcon - адрес иконки валюты размером из аргумента метода`string iconSize = "small"`
##### Пример 
```c#
void Start() 
{
	YaSDK.GetCatalog(productList =>
	{
	    foreach (var item in productList.products)
	    {
	        Debug.Log($"{item.title} стоит {item.price}");
	        Debug.Log($"описание - {item.description}");
	    }
	});
}
```

# Лидерборд
Для начала работы надо создать лидерборд в консоли разработчика. 
#### Новый результат
Чтобы установить новый результат, используется метод `YaSDK.SetScore(string leaderboard, long score)`. 
##### Пример
```c#
YaSDK.SetScore("Richest", YaSDK.Saves.Money);
```
#### Получение рейтинга
Для получения рейтинга вызываем метод `YaSDK.GetPlayerEntry(string leaderboard, Action<LeaderboardEntry> onEntry, Action onNotPresent = null, string avatarSize = "small")`
leaderboard - название лидерборда
Класс `LeaderboardEntry` содержит все поля которые дает Яндекс. 
Колбэк `onNotPresent` вызывается если игрока нет в таблице. 
`avatarSize` может принимать значения `small`, `medium` и `large`.
##### Пример 
```c#
YaSDK.GetPlayerEntry("Richest", entry =>
{
    Debug.Log($"Вы {entry.rank} по наличию денег");
}, 
() =>
{
    Debug.Log("Вас ещё нет в списках!");
});
```

#### Записи лидерборда
Для получения записей лидерборда вызываем `YaSDK.GetLeaderboardEntries(string leaderboard, Action<LeaderboardEntries> onEntries, bool include = false, int around = 6, int top = 3, string avatarSize = "small")`
Все аргументы можно посмотреть в документации Яндекс.Игр.

##### Пример 
```c#
YaSDK.GetLeaderboardEntries("Richest", entries =>
{
    foreach (var entry in entries.entries)
    {
        Debug.Log($"На {entry.rank} месте {entry.player.publicName} - у него {entry.score} на счету!");
    }
});
```

# Переменные окружения
Все переменные окружения находятся в `YaSDK.Env`. Чаще всего оттуда нужен только язык, его можно получить так `YaSDK.Env.i18n.lang`. Там будет строка `ru` либо `en` и т.д.
# Серверное время
Для получения серверного времени вызовем `YaSDK.ServerTime()`. Он вернет количество миллисекунд с начала 1970 года типа `long`.
