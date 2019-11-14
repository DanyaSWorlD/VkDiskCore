# VkDiskCore
VkDiskCore это базовая библиотека для создания собственных приложений VkDisk.
Данная библиотека содержит все необходимые методы и функции для подключения к ВКонтакте и работе с разделом документы (и не только).
А использоваине библиотеки [**VkNet**](https://github.com/vknet/vk) делает использование VkDiskCore максимально удобным и понятным.
# Как использовать
## Авторизация

```c#
 private async void Login()
 {
      await new Auth(Login, Password)
          .LoginAsync();

      if (!VkDisk.VkApi.IsAuthorized)
          return;
          
      // Авторизация прошла успешно! 
}
```

В VkDiskCore реализован механизм сохранения токенов, поэтому можно попробовать залогиниться по сохраненному токену

```C#
private async void TryLogin()
{
    if (await Auth.TryTokenLoginAsync())
      // Успешно залогигинены!
    else
      // Надо вводить пароль
}
```

Для двухфакторной авторизации:
```C#
await new Auth(Login, Password)
      .WithTwoFactor(WaitTwoFactor)
      .LoginAsync();
```

После авторизации id пользователя и токен можно получить из статичного класса ```User```, либо обратившись к VkNet
```C#
 VkDisk.VkApi.UserId.ToString();
```

## Загрузка и отправка файлов
Отправка файла довольно примитивна: 
```C#
VkDisk.Document.Upload(path);
```
Для загрузки файла потребуется адрес файла. Причем, если файл .vkd, то он будет скачан как надо
```C#
VkDisk.Document.Download(url, new KnownFolder(KnownFolderType.Downloads).Path);
```

# План развития проекта
- добавить возможность загрузки папок
- переделать структуру .vkd файла для удобного файлообмена
- подключить телеграм
- мигрировать на .NET CORE

# Контакты
- [***Твиттор***](https://twitter.com/DiskVk)
- [***ВКонтакте***](https://vk.com/vkdisk_ru)
- [***vkdisk.ru***](https://vkdisk.ru)
