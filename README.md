# Описание
YaSDK - плагин для Unity, предоставляющий интеграцию с SDK Яндекс игр используя колбэки. Плагин предоставляет удобный API для работы с платформой "Яндекс.Игры": реклама, покупки, таблицы лидеров и прочее.

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
В папке плагина есть скрипт `Saves.cs`. В нем вы создаете свои поля, которые хотите сохранить. Затем на протяжении игры вы изменяете данные как вам нужно, и вызываете метод сохранения, он отправит данные на сервер. При первом входе в игру, поля примут значения по умолчанию.
#### Сохранения
##### Пример
```c#
[Serializable]
public class Saves 
{
	public int Score = 0;
	public int Money = 500;
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
Затем, во время игры изменяете поля, как вам нужно:
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
#### Авторизация
Авторизация внутри игры пока не реализована, но вы можете проверить авторизован ли игрок вызвав `YaSDK.IsPlayerAuthorized()`. Вернет `true` если авторизован и `false` если нет.
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