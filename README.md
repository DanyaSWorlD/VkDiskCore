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


