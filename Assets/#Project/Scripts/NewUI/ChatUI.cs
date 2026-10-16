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
    public Button nextQuestion;
    [Header("Finish")]
    public GameObject finalRoot;
    public Button returnToSelector;

    void Awake()
    {
        interviewSession.QuestionStarted += SetQuestion;
        for (int i = 0; i < buttonAnswers.Length; i++)
        {
            buttonAnswers[i].OnClick += Click;
        }
        nextQuestion.onClick.AddListener(NextQuetion);
        returnToSelector.onClick.AddListener(ShowSelector);
    }

    void OnEnable()
    {
        interviewSession.Load(curData);
        resultRoot.SetActive(false);
        finalRoot.SetActive(false);
    }

    private void ShowSelector()
    {
        ClearDialog();
        finalRoot.SetActive(false);
    }

    private void SetQuestion(QuestionData data)
    {
        AddDialog(false, data.text);

        buttonAnswerRoot.SetActive(true);
        for (int i = 0; i < buttonAnswers.Length; i++)
        {
            buttonAnswers[i].SetData(data.answers[i]);
        }
    }

    private void Click(int id)
    {
        var result = interviewSession.SelectAnswer(id);

        AddDialog(true, result.answerText);

        buttonAnswerRoot.SetActive(false);
        resultRoot.SetActive(true);
        commentText.text = result.comment;
    }

    private void NextQuetion()
    {
        resultRoot.SetActive(false);
        if (interviewSession.MoveNext() == false)
        {
            finalRoot.SetActive(true);
            AddDialog(false,"Спасибо за ваши ответы! Это было интересное собеседование. Мы свяжемся с вами в течение 5 рабочих дней. Удачи!");
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
            Destroy(textRoot.GetChild(i).gameObject);
        }
    }

    public void SetData(InterviewData interviewData)
    {
        curData = interviewData;
    }
}
