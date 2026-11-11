using System;

namespace Interview
{
    /// <summary>Уровень образования (шаг 3 сценария): колледж или вуз.</summary>
    [Serializable]
    public class LevelData
    {
        public string id;
        public string title;
        public string shortTitle;
        public string description;
        public string badge;
    }

    /// <summary>Факультет / колледж (шаг 4 сценария). Принадлежит одному уровню образования.</summary>
    [Serializable]
    public class FacultyData
    {
        public string id;
        public string levelId;
        public string title;
        public string shortTitle;
        public string description;
    }

    /// <summary>Компания-партнёр (шаг 5 сценария).</summary>
    [Serializable]
    public class CompanyData
    {
        public string id;
        /// <summary>Официальное название («Банк ВТБ (ПАО)»).</summary>
        public string title;
        /// <summary>Короткое название для шапки чата и хлебных крошек («ВТБ»).</summary>
        public string shortTitle;
        public string description;
        /// <summary>Необязательная плашка на карточке («Топ-партнёр»).</summary>
        public string tag;
    }

    /// <summary>Содержимое Resources/Interview/catalog.json.</summary>
    [Serializable]
    public class CatalogData
    {
        public LevelData[] levels;
        public FacultyData[] faculties;
        public CompanyData[] companies;
    }
}
