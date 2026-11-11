using System;
using System.Collections;
using Interview;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    [Header("Interview")]
    public InterviewSession interviewSession;
    public InterviewData curData;
    [Header("Header")]
    [Tooltip("Имя в шапке чата — «Рекрутер».")]
    public TextMeshProUGUI recruiterNameText;
    [Tooltip("Строка под именем: «Банк ВТБ (ПАО) • Представитель кадровой службы».")]
    public TextMeshProUGUI recruiterInfoText;
    [Header("Dialog")]
    public RectTransform textRoot;
    public ScrollRect scrollRect;
    public BlockText blockTextPrefab;
    public float space;
    [Header("Answers")]
    public GameObject buttonAnswerRoot;
    public ButtonAnswer[] buttonAnswers;
    [Header("Next")]
    public GameObject resultRoot;
    public TextMeshProUGUI commentText;
    [Tooltip("Заголовок блока обратной связи — показывает «Сильный / Средний / Слабый ответ» цветом.")]
    public TextMeshProUGUI qualityText;
    public Button nextQuestion;
    [Tooltip("Текст кнопки «Продолжить» (на последнем этапе — «Завершить собеседование»).")]
    public TextMeshProUGUI nextQuestionText;
    [Tooltip("Цвет заголовка этапа в реплике рекрутера (rich text).")]
    public string stageColor = "#1A9BB5";
    [Header("Finish")]
    public GameObject finalRoot;
    public Button returnToSelector;

    void Awake()
    {
        interviewSession.QuestionStarted += SetQuestion;
        for (int i = 0; i < buttonAnswers.Length; i++)
        {
            // id = индекс кнопки = индекс варианта (в сцене у всех кнопок стоял id 0).
            buttonAnswers[i].SetID(i);
            buttonAnswers[i].OnClick += Click;
        }
        nextQuestion.onClick.AddListener(NextQuetion);
        returnToSelector.onClick.AddListener(ShowSelector);
    }

    void OnEnable()
    {
        if (InterviewSelection.Vacancy != null)
        {
            curData = InterviewSelection.Vacancy;
        }

        ClearDialog();
        resultRoot.SetActive(false);
        finalRoot.SetActive(false);
        UpdateHeader();

        if (curData == null)
        {
            Debug.LogWarning(InterviewLoader.LogPrefix + " вакансия не выбрана — нечего показывать в чате.");
            return;
        }

        interviewSession.Load(curData);
    }

    private void UpdateHeader()
    {
        if (recruiterNameText)
        {
            recruiterNameText.text = "Рекрутер";
        }

        if (recruiterInfoText == null || curData == null)
        {
            return;
        }

        CompanyData company = InterviewCatalog.GetCompany(curData.companyId);
        string companyName = company != null ? company.title : string.Empty;
        string role = curData.recruiterRole ?? string.Empty;
        recruiterInfoText.text = companyName + (companyName.Length > 0 && role.Length > 0 ? " • " : string.Empty) + role;
    }

    /// <summary>Буква варианта по позиции кнопки (варианты перемешаны, поэтому «A» — не обязательно сильный ответ).</summary>
    private static string Letter(int index)
    {
        return index >= 0 && index < AnswerLetters.Length ? AnswerLetters[index] : (index + 1).ToString();
    }

    private static readonly string[] AnswerLetters = { "A", "B", "C", "D", "E" };

    private void ShowSelector()
    {
        ClearDialog();
        finalRoot.SetActive(false);
    }

    private void SetQuestion(QuestionData data)
    {
        // Заголовок этапа над репликой рекрутера: «Этап 2 из 4 · Профессиональные компетенции».
        int index = interviewSession.CurrentIndex;
        string stage = "<color=" + stageColor + "><b>Этап " + (index + 1) + " из " + interviewSession.QuestionCount
                       + " · " + InterviewText.StageName(data) + "</b></color>\n";
        AddDialog(false, stage + data.text);

        buttonAnswerRoot.SetActive(true);
        if (data.answers.Length > buttonAnswers.Length)
        {
            Debug.LogWarning(InterviewLoader.LogPrefix + " «" + data.id + "»: вариантов (" + data.answers.Length + ") больше, чем кнопок (" + buttonAnswers.Length + ").");
        }
        for (int i = 0; i < buttonAnswers.Length; i++)
        {
            bool visible = i < data.answers.Length;
            buttonAnswers[i].gameObject.SetActive(visible);
            if (visible)
            {
                buttonAnswers[i].SetData(data.answers[i], Letter(i));
            }
        }
    }

    private void Click(int id)
    {
        var result = interviewSession.SelectAnswer(id);

        if (!result.isValid)
        {
            return; // повторный клик по уже отвеченному вопросу
        }

        AddDialog(true, "<b>" + Letter(id) + ".</b> " + result.answerText);

        buttonAnswerRoot.SetActive(false);
        resultRoot.SetActive(true);
        commentText.text = result.comment;

        if (qualityText)
        {
            qualityText.text = InterviewText.QualityLabel(result.quality);
            qualityText.color = InterviewText.QualityColor(result.quality);
        }

        if (nextQuestionText)
        {
            bool isLast = interviewSession.CurrentIndex >= interviewSession.QuestionCount - 1;
            nextQuestionText.text = isLast ? "Завершить собеседование →" : "Следующий вопрос →";
        }
    }

    private void NextQuetion()
    {
        resultRoot.SetActive(false);
        if (interviewSession.MoveNext() == false)
        {
            finalRoot.SetActive(true);
            buttonAnswerRoot.SetActive(false);
            AddDialog(false, "Спасибо, что уделили время! На этом собеседование завершено. " +
                             "Система оценила ваши ответы и подготовила итоговый отчёт с рекомендациями.");
        }
    }

    private void AddDialog(bool isAnswer, string text)
    {
        var spawned = Instantiate(blockTextPrefab, textRoot);
        spawned.SetSide(isAnswer: isAnswer);
        spawned.SetText(text);
        //recalculate height of root
        float height = 0;
        for (int i = 0; i < textRoot.childCount; i++)
        {
            var rect = textRoot.GetChild(i).GetComponent<RectTransform>();
            height += rect.sizeDelta.y + space;
        }
        textRoot.sizeDelta = new Vector2(textRoot.sizeDelta.x, height);
        //set down
        //scrollRect.verticalScrollbar.value = 0;
        StartCoroutine(UpdateUI(textRoot.childCount));
    }

    IEnumerator UpdateUI(int count)
    {
        yield return null;
        while (scrollRect.verticalScrollbar.value > 0)
        {
            scrollRect.verticalScrollbar.value -= Time.deltaTime / count;
            yield return null;
        }
        scrollRect.verticalScrollbar.value = 0;
    }

    private void ClearDialog()
    {
        for (int i = textRoot.childCount - 1; i >= 0; --i)
        {
            // Отцепляем сразу: Destroy отложенный, а AddDialog считает высоту по childCount.
            var child = textRoot.GetChild(i);
            child.SetParent(null, false);
            Destroy(child.gameObject);
        }
    }

    public void SetData(InterviewData interviewData)
    {
        curData = interviewData;
    }
}
