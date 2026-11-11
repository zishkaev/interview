using System.Collections.Generic;
using Interview;
using TMPro;
using UnityEngine;

/// <summary>
/// Один шаг выбора: уровень → факультет → компания → вакансия.
/// При показе страницы строит карточки по данным каталога, отфильтрованным
/// по выбору на предыдущих шагах, и записывает выбор в <see cref="InterviewSelection"/>.
/// </summary>
public class SelectionStepUI : MonoBehaviour
{
    public enum Kind
    {
        Level,
        Faculty,
        Company,
        Vacancy
    }

    public Kind kind;
    public ItemListUI itemList;

    [Header("Карточки")]
    [Tooltip("Шаблон карточки. Если задан — карточки клонируются в Content. " +
             "Если пуст — используются уже лежащие в ItemListUI карточки по порядку (лишние скрываются).")]
    public CardView template;
    public RectTransform content;

    [Header("Факультеты")]
    [Tooltip("Показывать только факультеты, для которых есть хотя бы одна вакансия.")]
    public bool hideEmptyFaculties = true;

    [Header("Необязательно")]
    [Tooltip("Показывается, если подходящих вариантов нет.")]
    public GameObject emptyState;
    [Tooltip("Хлебные крошки: «ВУЗ › Финансовый › ВТБ».")]
    public TextMeshProUGUI breadcrumbText;

    private readonly List<object> options = new List<object>();
    private readonly List<GameObject> spawned = new List<GameObject>();
    private List<ItemUI> staticItems;

    private void Awake()
    {
        if (itemList == null)
        {
            itemList = GetComponent<ItemListUI>();
        }

        if (template == null)
        {
            staticItems = new List<ItemUI>(itemList.items);
        }
        else
        {
            template.gameObject.SetActive(false);
        }

        itemList.OnSelectItem += OnSelect;
    }

    private void OnEnable()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        CollectOptions();

        var items = new List<ItemUI>();

        if (template != null)
        {
            for (int i = 0; i < spawned.Count; i++)
            {
                Destroy(spawned[i]);
            }

            spawned.Clear();
            RectTransform parent = content != null ? content : (RectTransform)template.transform.parent;

            for (int i = 0; i < options.Count; i++)
            {
                CardView card = Instantiate(template, parent);
                card.gameObject.name = template.name + " " + i;
                card.gameObject.SetActive(true);
                Fill(card, options[i]);
                spawned.Add(card.gameObject);
                items.Add(card.GetComponent<ItemUI>());
            }
        }
        else
        {
            for (int i = 0; i < staticItems.Count; i++)
            {
                bool visible = i < options.Count;
                staticItems[i].gameObject.SetActive(visible);

                if (!visible)
                {
                    continue;
                }

                CardView card = staticItems[i].GetComponent<CardView>();

                if (card != null)
                {
                    Fill(card, options[i]);
                }

                items.Add(staticItems[i]);
            }
        }

        itemList.SetList(items);

        if (emptyState)
        {
            emptyState.SetActive(options.Count == 0);
        }

        if (breadcrumbText)
        {
            breadcrumbText.text = InterviewSelection.Breadcrumb((int)kind);
        }
    }

    private void CollectOptions()
    {
        options.Clear();

        switch (kind)
        {
            case Kind.Level:
                options.AddRange(InterviewCatalog.GetLevels());
                break;
            case Kind.Faculty:
                if (InterviewSelection.Level != null)
                {
                    options.AddRange(InterviewCatalog.GetFaculties(InterviewSelection.Level.id, hideEmptyFaculties));
                }

                break;
            case Kind.Company:
                if (InterviewSelection.Faculty != null)
                {
                    options.AddRange(InterviewCatalog.GetCompanies(InterviewSelection.Faculty.id));
                }

                break;
            case Kind.Vacancy:
                if (InterviewSelection.Faculty != null && InterviewSelection.Company != null)
                {
                    options.AddRange(InterviewCatalog.GetVacancies(InterviewSelection.Faculty.id, InterviewSelection.Company.id));
                }

                break;
        }
    }

    private static void Fill(CardView card, object option)
    {
        switch (option)
        {
            case LevelData level:
                card.Set(level.shortTitle, level.description, null, level.badge);
                break;
            case FacultyData faculty:
                card.Set(faculty.title, faculty.description);
                break;
            case CompanyData company:
                card.Set(company.shortTitle, company.description, company.title, company.tag);
                break;
            case InterviewData vacancy:
                CompanyData owner = InterviewCatalog.GetCompany(vacancy.companyId);
                string meta = (owner != null ? owner.shortTitle + " · " : string.Empty) + vacancy.QuestionsCount + " этапа собеседования";
                // Без «✓»: в шрифте LiberationSans SDF нет этого символа.
                string tag = string.IsNullOrEmpty(vacancy.courseRecommendation) ? null : vacancy.courseRecommendation;
                card.Set(vacancy.position, vacancy.targetAudience, meta, tag);
                break;
        }
    }

    private void OnSelect(int index)
    {
        if (index < 0 || index >= options.Count)
        {
            return;
        }

        switch (options[index])
        {
            case LevelData level:
                InterviewSelection.SetLevel(level);
                break;
            case FacultyData faculty:
                InterviewSelection.SetFaculty(faculty);
                break;
            case CompanyData company:
                InterviewSelection.SetCompany(company);
                break;
            case InterviewData vacancy:
                InterviewSelection.SetVacancy(vacancy);
                break;
        }
    }
}
