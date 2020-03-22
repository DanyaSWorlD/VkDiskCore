[![Build Status](https://daquga.visualstudio.com/VkDiskCore/_apis/build/status/DanyaSWorlD.VkDiskCore?branchName=master)](https://daquga.visualstudio.com/VkDiskCore/_build/latest?definitionId=3&branchName=master)
# VkDiskCore
VkDiskCore это базовая библиотека для создания собственных приложений VkDisk.
Данная библиотека содержит все необходимые методы и функции для подключения к ВКонтакте и работе с разделом документы (и не только).
А использоваине библиотеки [**VkNet**](https://github.com/vknet/vk) делает использование VkDiskCore максимально удобным и понятным.
# Как использовать
## Авторизация

```c#
 private void Login()
 {
      if (!Auth.WithPass(Login, Password)
          return;
          
      // Авторизация прошла успешно! 
}
```

В VkDiskCore реализован механизм сохранения токенов, поэтому можно попробовать залогиниться по сохраненному токену

```C#
private void TryLogin()
{
    // попробовать залогиниться с сохраненным токеном
    if (Auth.WithToken())
      // Успешно залогигинены!
    
    // попробовать залогиниться с сохраненными логином и паролем
    if(Auth.WithSavedPass())
      // успешно залогинены
    
    // надо вводить пароль
}
```

Для двухфакторной авторизации:
```C#
Auth.WithPass(Login, Password, WaitTwoFactor);
```
или
```C#
Auth.WithSavedPass(WaitTwoFactor);
```
Где ```WaitTwoFactor``` возвращает вводимый пользователем код в формате ``` string ```

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
