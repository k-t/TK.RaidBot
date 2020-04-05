[![Build Status](https://travis-ci.org/k-t/TK.RaidBot.svg?branch=master)](https://travis-ci.org/k-t/TK.RaidBot)

Бот для [Discord](https://discordapp.com/), заточенный под организацию [WvW](https://wiki.guildwars2.com/wiki/World_versus_World)-рейдов в гильдии [Time Keepers](https://time-keepers.ru/).

#### Требования

* [.NET Core Runtime](https://dotnet.microsoft.com/download/dotnet-core/current/runtime): версия 3.1 и выше.
* [MongoDB](https://www.mongodb.com/): версия 4.2 и ниже.
* Любая ОС, для которой поддерживаются перечисленные выше продукты.

#### Установка и настройка

1. Собрать из исходников или скачать последний архив из https://github.com/k-t/TK.RaidBot/releases для нужной платформы и распаковать его в нужной папке.
2. Отредактировать настройки в файле `config.ini`.

#### Настройки

| Имя          | Описание                                                     | Обязателен |
| ------------ | ------------------------------------------------------------ | ---------- |
| databaseUrl  | Строка подключения к MongoDB.                                | Да         |
| databaseName | Имя базы данных.<br />По умолчанию используется имя `raidBotDB`. | Нет        |
| botToken     | Токен аутентификации бота.<br />Копируется из настроек бота в его профиле в Discord. | Да         |

Пример конфига:

```ini
[db]
; local database
databaseUrl=mongodb://127.0.0.1:27017/?appname=RaidBot&ssl=false
databaseName=raidBotDB

[bot]
botToken=[Copied Bot Token]
```

##### Запуск

На Windows достаточно запустить исполняемый файл `TK.RaidBot.exe`.

На Linux можно выполнить следующую команду:

```bash
$ dotnet ./TK.RaidBot.dll
```

