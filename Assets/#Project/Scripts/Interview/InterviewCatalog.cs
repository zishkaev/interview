using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interview
{
    /// <summary>
    /// Каталог симулятора: справочники (catalog.json) + все вакансии (Vacancies/*.json).
    /// Даёт отфильтрованные списки для шагов выбора:
    /// уровень → факультеты → компании → вакансии.
    /// </summary>
    public static class InterviewCatalog
    {
        public const string CatalogPath = "Interview/catalog";
        public const string VacanciesFolder = "Interview/Vacancies";

        private static CatalogData catalog;
        private static List<InterviewData> vacancies;

        public static CatalogData Data
        {
            get
            {
                EnsureLoaded();
                return catalog;
            }
        }

        public static IReadOnlyList<InterviewData> Vacancies
        {
            get
            {
                EnsureLoaded();
                return vacancies;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            catalog = null;
            vacancies = null;
        }

        /// <summary>Перечитать данные (например, после импорта в редакторе).</summary>
        public static void Reload()
        {
            catalog = null;
            vacancies = null;
            EnsureLoaded();
        }

        private static void EnsureLoaded()
        {
            if (catalog != null && vacancies != null)
            {
                return;
            }

            catalog = LoadCatalog();
            vacancies = new List<InterviewData>();

            TextAsset[] assets = Resources.LoadAll<TextAsset>(VacanciesFolder);
            Array.Sort(assets, (a, b) => string.CompareOrdinal(a.name, b.name));

            for (int i = 0; i < assets.Length; i++)
            {
                InterviewData data = InterviewLoader.FromTextAsset(assets[i]);

                if (data == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(data.id))
                {
                    data.id = assets[i].name;
                }

                ValidateLinks(data);
                vacancies.Add(data);
            }
        }

        private static CatalogData LoadCatalog()
        {
            TextAsset asset = Resources.Load<TextAsset>(CatalogPath);

            if (asset == null)
            {
                Debug.LogError(InterviewLoader.LogPrefix + " не найден каталог Resources/" + CatalogPath + ".json");
                return new CatalogData { levels = new LevelData[0], faculties = new FacultyData[0], companies = new CompanyData[0] };
            }

            CatalogData data = JsonUtility.FromJson<CatalogData>(JsonSanitizer.Sanitize(asset.text)) ?? new CatalogData();
            data.levels = data.levels ?? new LevelData[0];
            data.faculties = data.faculties ?? new FacultyData[0];
            data.companies = data.companies ?? new CompanyData[0];
            return data;
        }

        private static void ValidateLinks(InterviewData data)
        {
            string prefix = InterviewLoader.LogPrefix + " «" + data.id + "»: ";

            if (GetCompany(data.companyId) == null)
            {
                Debug.LogWarning(prefix + "компания «" + data.companyId + "» не найдена в каталоге.");
            }

            if (data.faculties == null || data.faculties.Length == 0)
            {
                Debug.LogWarning(prefix + "не указаны факультеты — вакансия не появится ни в одном списке.");
                return;
            }

            for (int i = 0; i < data.faculties.Length; i++)
            {
                if (GetFaculty(data.faculties[i]) == null)
                {
                    Debug.LogWarning(prefix + "факультет «" + data.faculties[i] + "» не найден в каталоге.");
                }
            }
        }

        // ---------- поиск по id ----------

        public static LevelData GetLevel(string id)
        {
            return Find(Data.levels, l => l.id == id);
        }

        public static FacultyData GetFaculty(string id)
        {
            return Find(Data.faculties, f => f.id == id);
        }

        public static CompanyData GetCompany(string id)
        {
            return Find(Data.companies, c => c.id == id);
        }

        public static InterviewData GetVacancy(string id)
        {
            return Find(Vacancies, v => v.id == id);
        }

        // ---------- списки для шагов ----------

        public static List<LevelData> GetLevels()
        {
            return new List<LevelData>(Data.levels);
        }

        /// <summary>Факультеты уровня. По умолчанию — только те, где есть хотя бы одна вакансия.</summary>
        public static List<FacultyData> GetFaculties(string levelId, bool onlyWithVacancies = false)
        {
            var result = new List<FacultyData>();

            foreach (FacultyData faculty in Data.faculties)
            {
                if (faculty.levelId != levelId)
                {
                    continue;
                }

                if (onlyWithVacancies && GetVacancies(faculty.id, null).Count == 0)
                {
                    continue;
                }

                result.Add(faculty);
            }

            return result;
        }

        /// <summary>Компании, у которых есть вакансии для факультета (в порядке каталога).</summary>
        public static List<CompanyData> GetCompanies(string facultyId)
        {
            var result = new List<CompanyData>();

            foreach (CompanyData company in Data.companies)
            {
                if (GetVacancies(facultyId, company.id).Count > 0)
                {
                    result.Add(company);
                }
            }

            return result;
        }

        /// <summary>Вакансии факультета; если companyId задан — только этой компании.</summary>
        public static List<InterviewData> GetVacancies(string facultyId, string companyId)
        {
            var result = new List<InterviewData>();

            foreach (InterviewData vacancy in Vacancies)
            {
                if (!vacancy.HasFaculty(facultyId))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(companyId) && vacancy.companyId != companyId)
                {
                    continue;
                }

                result.Add(vacancy);
            }

            return result;
        }

        private static T Find<T>(IEnumerable<T> items, Predicate<T> match) where T : class
        {
            if (items == null)
            {
                return null;
            }

            foreach (T item in items)
            {
                if (item != null && match(item))
                {
                    return item;
                }
            }

            return null;
        }
    }
}
