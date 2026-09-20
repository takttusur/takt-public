# TAKT-public

Основная часть веб-сайта клуба, открытая для публичного доступа.

[![Publish master](https://github.com/takttusur/takt-public/actions/workflows/master-publish.yml/badge.svg?branch=master)](https://github.com/takttusur/takt-public/actions/workflows/master-publish.yml)
[![Build develop](https://github.com/takttusur/takt-public/actions/workflows/develop-build.yml/badge.svg?branch=develop)](https://github.com/takttusur/takt-public/actions/workflows/develop-build.yml)

## Функциональность

* Отображение новостей
* Отображение ближайших событий
* Отображение доступного снаряжения на центральном складе

## Технологии

* [React.js](https://react.dev)
* [Chakra-UI](https://chakra-ui.com)
* [TanStack/query](https://github.com/TanStack/query)
* [Vite](https://vitejs.dev)
* [Docker](https://www.docker.com)

## Сборка

1. Установить необходимое ПО: 
   1. [Node.js](https://nodejs.org/en)
   2. [Git](https://git-scm.com)
   3. Если требуется работа с контейнерами:
      1. [Docker](https://docs.docker.com/desktop/install/windows-install/) 
      2. [Docker Compose](https://docs.docker.com/compose/install/) 
2. Склонировать проект `git clone https://github.com/takttusur/takt-public.git`
3. Открыть командную строку в папке с проектом и выполнить:
   1. `npm install` - для установки зависимостей
   2. `npm run lint` - для проверки кода
   3. `npm run test` - для запуска автотестов
   4. `npm run build` - для сборки проекта, результат будет в папке `dist`
      1. `npm run build -- --base=<youbaseurl>` - если базовый URL отличный от /
   5. `npm run dev` - для запуска дев-сервера с проектом

## Локальный запуск в Docker

1. `docker compose up --build`
2. Открыть `http://localhost:8080`
3. `/api/*` уходит в .NET backend, остальные запросы — в Vite dev server с hot reload
4. Для Aspire: `dotnet run --project src/Takt.AppHost/Takt.AppHost.csproj`

## Развертывание

Публикация выполняется при обновлении ветки `master` или создании тега `v*`,
проект собирается в docker-образ с [nginx](https://www.nginx.com) 
и публикуется в [GitHub Container Registry](https://github.com/takttusur/takt-public/pkgs/container/takt-public)

Получить актуальную версию: `docker pull ghcr.io/takttusur/takt-public:latest`

После запуска, веб-сервер принимает подключения на порт `80`
`docker run ghcr.io/takttusur/takt-public:latest -p <your_port>:80`

## Зависимости

_Планируется, что веб-приложение будет зависеть от серверной части, но пока таких зависимостей нет. 
Когда появятся, будут указаны ниже._ 
