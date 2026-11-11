using System;
using UnityEngine;

namespace Interview
{
    /// <summary>
    /// Что пользователь выбрал на шагах: уровень → факультет → компания → вакансия.
    /// Смена верхнего шага сбрасывает все нижние, чтобы не остался выбор,
    /// который не подходит под новый фильтр.
    /// </summary>
    public static class InterviewSelection
    {
        public static LevelData Level { get; private set; }
        public static FacultyData Faculty { get; private set; }
        public static CompanyData Company { get; private set; }
        public static InterviewData Vacancy { get; private set; }

        /// <summary>Выбор изменился.</summary>
        public static event Action Changed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Level = null;
            Faculty = null;
            Company = null;
            Vacancy = null;
            Changed = null;
        }

        public static void SetLevel(LevelData level)
        {
            if (Level == level)
            {
                return;
            }

            Level = level;
            Faculty = null;
            Company = null;
            Vacancy = null;
            Notify();
        }

        public static void SetFaculty(FacultyData faculty)
        {
            if (Faculty == faculty)
            {
                return;
            }

            Faculty = faculty;
            Company = null;
            Vacancy = null;
            Notify();
        }

        public static void SetCompany(CompanyData company)
        {
            if (Company == company)
            {
                return;
            }

            Company = company;
            Vacancy = null;
            Notify();
        }

        public static void SetVacancy(InterviewData vacancy)
        {
            if (Vacancy == vacancy)
            {
                return;
            }

            Vacancy = vacancy;
            Notify();
        }

        public static void Clear()
        {
            Level = null;
            Faculty = null;
            Company = null;
            Vacancy = null;
            Notify();
        }

        /// <summary>«Высшее образование › Финансовый факультет › Банк ВТБ (ПАО)» (названия дословно из сценария) — для хлебных крошек.</summary>
        public static string Breadcrumb(int depth)
        {
            var parts = new System.Collections.Generic.List<string>();

            if (depth >= 1 && Level != null)
            {
                parts.Add(Level.title);
            }

            if (depth >= 2 && Faculty != null)
            {
                parts.Add(Faculty.title);
            }

            if (depth >= 3 && Company != null)
            {
                parts.Add(Company.title);
            }

            if (depth >= 4 && Vacancy != null)
            {
                parts.Add(Vacancy.DisplayPosition);
            }

            return string.Join(" › ", parts);
        }

        private static void Notify()
        {
            Changed?.Invoke();
        }
    }
}
