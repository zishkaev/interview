# Симулятор собеседований — данные (Interview)

Сценарий пользователя (ТЗ): уровень образования → факультет → компания → вакансия → диалог из 4 этапов → отчёт.

## Где лежат данные

```
Assets/#Project/Docs/*Диалог*.docx              исходные сценарии (источник правды для текста)
Assets/Resources/Interview/catalog.json         справочники: уровни, факультеты, компании (правится руками)
Assets/Resources/Interview/Vacancies/*.json     вакансии — генерируются из docx импортёром
Assets/#Project/Data/LegacyInterview/           старые JSON (не загружаются, оставлены для истории)
```

## Как обновить вакансии из docx

`Tools → Interview → Импорт сценариев из DOCX` (скрипт `Assets/#Project/Editor/InterviewDocxImporter.cs`).

* Из docx берутся: позиция, целевая аудитория, ключевая задача, этапы (название, цель, вопрос),
  варианты A/B/C с качеством (сильный/средний/слабый), обратная связь, рекомендации ДПО.
* Привязка вакансии (`companyId`, `faculties`, `courseRecommendation`) при первом импорте берётся
  из таблицы `Mappings` в импортёре. Если потом поправить её прямо в JSON — повторный импорт ручные
  значения **не затрёт**.
* Новый docx: добавьте строку в `Mappings` (фрагмент имени файла → id, компания, факультеты), при
  необходимости — компанию в `catalog.json`, и запустите импорт.
* Ошибки разбора и проверки (нет вопроса, нет обратной связи и т.п.) пишутся в Console с префиксом `[Interview]`.

## catalog.json

```json
{
  "levels":    [{ "id": "university", "title": "Высшее образование", "shortTitle": "ВУЗ", "description": "...", "badge": "..." }],
  "faculties": [{ "id": "fin", "levelId": "university", "title": "Финансовый факультет", "shortTitle": "Финансовый", "description": "..." }],
  "companies": [{ "id": "vtb", "title": "Банк ВТБ (ПАО)", "shortTitle": "ВТБ", "description": "...", "tag": "Топ-партнёр" }]
}
```

Уровень образования вакансии определяется через её факультеты (`faculty.levelId`).

## Вакансия (Vacancies/<id>.json)

```json
{
  "id": "vtb_financial_analyst_intern",
  "companyId": "vtb",
  "position": "Стажер финансовый аналитик в Аналитический департамент",
  "title": "ВТБ — Стажер финансовый аналитик ...",
  "description": "Ключевая задача тренажёра",
  "targetAudience": "Целевая аудитория",
  "courseRecommendation": "Выпускники 2025–2027",
  "recruiterRole": "Представитель кадровой службы",
  "faculties": ["fin"],
  "recommendations": "Рекомендации курсов/ДПО",
  "questions": [
    {
      "id": "stage1",
      "category": "Этап 1. Самопрезентация и мотивация",
      "goal": "Цель этапа",
      "text": "Вопрос рекрутера",
      "answers": [
        { "text": "...", "quality": "strong", "comment": "Обратная связь" },
        { "text": "...", "quality": "medium", "comment": "..." },
        { "text": "...", "quality": "weak",   "comment": "..." }
      ]
    }
  ]
}
```

`quality` — строка `strong` / `medium` / `weak` (также понимаются `A/B/C`, `сильный/средний/слабый`, `2/1/0`).
В коде используйте `AnswerData.Level` (enum `Quality`): `JsonUtility` не умеет читать enum из строки,
поэтому разбор делается вручную.

## Код

| Файл | Назначение |
| --- | --- |
| `InterviewData.cs` | модель вакансии / этапа / ответа, `Quality`, `QualityUtil` |
| `CatalogData.cs` | модель справочников |
| `InterviewCatalog.cs` | загрузка каталога и вакансий, фильтры `GetFaculties / GetCompanies / GetVacancies` |
| `InterviewSelection.cs` | текущий выбор пользователя; смена верхнего шага сбрасывает нижние |
| `InterviewLoader.cs` | разбор JSON + валидация |
| `InterviewSession.cs` | ход собеседования, баллы: `Points`, `MaxPoints`, `Percent` (strong 2 / medium 1 / weak 0) |
| `NewUI/SelectionStepUI.cs` | шаг выбора: строит карточки по каталогу и пишет выбор в `InterviewSelection` |
| `NewUI/CardView.cs` | тексты карточки |
| `NewUI/ChatUI.cs` | чат: берёт вакансию из `InterviewSelection`, заголовок этапа в реплике, цветная оценка ответа |
| `NewUI/ReportUI.cs` | итоговый отчёт: балл 0–100, разбор по этапам, сильные стороны / зоны роста, рекомендации |
| `NewUI/StageCardView.cs` | карточка этапа в отчёте |
| `InterviewText.cs` | общие подписи: название этапа, «Сильный ответ», цвета, вердикт по баллу |

## Итоговый отчёт

* Балл = сумма (strong 2 / medium 1 / weak 0) / максимум × 100.
* «Сильные стороны» — обратная связь на сильные ответы, «Зоны роста» — на средние и слабые.
* Рекомендации: курс «Твоя успешная карьера» — всегда; «Самопрезентация» — если этап 1 или 3
  пройден не на сильный ответ; «Программы ДПО» — всегда, с направлениями из поля `recommendations` вакансии.
* Кнопки: «На главную», «Пройти ещё раз» (та же вакансия), «Выбрать другую вакансию».

## Сцена (SampleScene)

`Canvas (MenuUI)` → страницы: Start, Step 1 (уровень), Step 2 (факультет), Step 3 (компания),
Step 4 (вакансия), Step 5 (чат), Step 6 (итоги). На Step 1–4 висит `SelectionStepUI`;
у Step 2–4 карточки клонируются из шаблона внутри `Scroll View/Viewport/Content`.
