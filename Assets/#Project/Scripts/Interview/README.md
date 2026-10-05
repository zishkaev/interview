# Банк вопросов интервью (Interview)

Ассеты с вопросами для собеседования: **вопрос → варианты ответа → комментарий к каждому варианту**.
Данные лежат в JSON-ассетах: их удобно генерировать ИИ, править в блокноте и класть прямо в проект.

## Структура

```
Assets/#Project/Scripts/Interview/
    InterviewData.cs       модель данных: AnswerData / QuestionData / InterviewData
    InterviewLoader.cs     загрузка JSON-ассетов, нормализация и валидация
    JsonSanitizer.cs       терпимость к «почти JSON»: markdown-обёртка, комментарии, висячие запятые
    InterviewSession.cs    MonoBehaviour-сессия: текущий вопрос, выбор ответа, комментарий, статистика

Assets/#Project/Data/Interview/
    csharp_basics.json     пример готового набора вопросов
```

Сторонние пакеты не нужны: JSON читается встроенным `JsonUtility`.

## Схема JSON

Имена полей обязательно должны совпадать со классами из `InterviewData.cs`
(регистр важен, лишние поля `JsonUtility` игнорирует):

```json
{
  "title": "Название набора",
  "description": "Необязательное описание",
  "questions": [
    {
      "id": "уникальный_идентификатор",
      "category": "Раздел / тема",
      "text": "Текст вопроса?",
      "answers": [
        { "text": "Правильный вариант", "isCorrect": true,  "comment": "Почему это верно" },
        { "text": "Неверный вариант",   "isCorrect": false, "comment": "Почему это неверно" }
      ]
    }
  ]
}
```

Ключи: `title`, `description`, `questions[]`, `id`, `category`, `text`, `answers[]`, `isCorrect`, `comment`.

Правила, на которые стоит обратить внимание:

* **Лучше чистый JSON** без комментариев и висячих запятых. Если ИИ всё-таки их добавил
  или обернул ответ в markdown-блок, загрузчик вычистит это сам — см. «Терпимость к выводу ИИ».
* Файл сохраняйте в **UTF-8** (BOM допустим, он отбрасывается при загрузке).
* `isCorrect` — только `true` / `false`. В каждом вопросе должен быть **хотя бы один** правильный вариант
  (несколько правильных допустимы, если у вас мультивыбор).
* `answers` — всегда массив; для пустого списка пишите `[]`, а не `null`.
* `comment` заполняйте у **каждого** варианта — это и есть основная ценность набора.
* `id` — латиница/цифры/подчёркивания, уникален внутри набора (нужен для сохранений и статистики).
* Перенос строки внутри текста — экранированный `\n`, кавычки — `\"`.

## Как попросить ИИ сгенерировать набор

Скопируйте в чат с ИИ такой запрос и подставьте свою тему:

```
Сгенерируй 10 вопросов для собеседования на Unity C# разработчика (тема: <ТЕМА>).
Верни ТОЛЬКО валидный JSON без комментариев и markdown-обёртки, в формате:

{
  "title": "<название набора>",
  "description": "<одно предложение>",
  "questions": [
    {
      "id": "<латиница_с_подчёркиваниями>",
      "category": "<тема вопроса>",
      "text": "<вопрос>",
      "answers": [
        { "text": "<вариант>", "isCorrect": true, "comment": "<почему верно, 1-3 предложения>" },
        { "text": "<вариант>", "isCorrect": false, "comment": "<почему неверно, 1-3 предложения>" }
      ]
    }
  ]
}

Требования: у каждого вопроса 3-4 варианта ответа; ровно один вариант с "isCorrect": true;
у каждого варианта заполнен "comment" на русском; внутри строк не используй настоящие переносы,
только \n; текст ответов разной длины, дистракторы правдоподобные.
```

Полученный текст сохраните как `Assets/#Project/Data/Interview/<имя>.json` — Unity импортирует
файл как `TextAsset`, и его можно сразу перетаскивать в поле `Data File` компонента `Interview Session`.

## Как подключить

1. **Через компонент.** Добавьте `Interview Session` на объект сцены
   (`Add Component → Interview → Interview Session`) и перетащите JSON-ассет в поле `Data File`.
   Если поле пустое, ассет ищется в `Resources` по пути `resourcesPath` (по умолчанию `Interview/csharp_basics`) —
   тогда файл должен лежать в папке `Resources`, например `Assets/Resources/Interview/csharp_basics.json`.

2. **Через код.**

```csharp
using Interview;
using UnityEngine;

public class QuizScreen : MonoBehaviour
{
    [SerializeField] private InterviewSession session;
    [SerializeField] private TextAsset questions;

    private void OnEnable()
    {
        session.Answered += OnAnswered;
        session.Completed += OnCompleted;

        session.Load();                                   // данные из поля Data File компонента

        InterviewData data = InterviewLoader.FromTextAsset(questions);
        // InterviewData data = InterviewLoader.FromResources("Interview/csharp_basics");
        // InterviewData data = InterviewLoader.FromFile(@"C:\Questions\my_set.json");
        session.Load(data);
    }

    public void OnAnswerButton(int index)
    {
        AnswerResult result = session.SelectAnswer(index);
        Debug.Log(result.isCorrect ? "Верно" : "Неверно");
        Debug.Log(result.comment);   // комментарий к выбранному варианту
    }

    private void OnAnswered(AnswerResult result)
    {
        Debug.Log(result.answerText + " → " + result.comment);
    }

    private void OnCompleted()
    {
        Debug.Log("Верных ответов: " + session.CorrectCount + " из " + session.AnsweredCount);
    }
}
```

## API сессии

| Метод / свойство | Назначение |
| --- | --- |
| `Load()` / `Load(InterviewData)` | загрузить данные и начать сессию |
| `Restart()` | пройти набор заново |
| `MoveNext()` / `GoTo(int)` | следующий / произвольный вопрос |
| `CurrentQuestion`, `CurrentIndex`, `QuestionCount` | состояние навигации |
| `GetCurrentAnswerTexts()` | тексты вариантов для кнопок UI |
| `SelectAnswer(int)` → `AnswerResult` | выбрать вариант, получить `isCorrect` и `comment` |
| `GetCurrentAnswerComment(int)` | комментарий к варианту без ответа |
| `HasAnsweredCurrent`, `IsAnswered(int)` | отвечал ли игрок |
| `AnsweredCount`, `CorrectCount`, `WrongCount`, `Score` | статистика |
| события `Loaded`, `QuestionStarted`, `Answered`, `Completed` | реакция UI на ход сессии |

Настройки в инспекторе: `loadOnStart`, `shuffleQuestions`, `shuffleAnswers`, `randomSeed`
(`0` — новый случайный порядок при каждом запуске), `allowAnswerChange`.

## Терпимость к выводу ИИ

`JsonSanitizer.Sanitize` вызывается автоматически внутри `InterviewLoader.FromJson`, поэтому
скопированный из чата ответ ИИ грузится без ручной правки. Он убирает:

* BOM в начале файла;
* markdown-обёртку вокруг JSON (тройные обратные кавычки, в том числе с пометкой языка);
* строчные и блочные комментарии в стиле JavaScript;
* висячие запятые перед `}` и `]`.

Всё внутри строковых литералов не трогается: текст с `https://...`, `//` или `, }` останется как есть.

## Проверка данных

`InterviewLoader.Validate(data)` ищет пустые вопросы, отсутствие правильного варианта, дубли `id`,
пустые варианты и отсутствующие комментарии, а найденные проблемы пишет в консоль Unity
с именем ассета. Вызов `Validate` выполняется автоматически при любой загрузке JSON
(`FromTextAsset` / `FromResources` / `FromFile` / `FromJson`), поэтому битый набор сразу видно
в Console — игру при этом не ломает: некорректные поля просто становятся пустыми строками.