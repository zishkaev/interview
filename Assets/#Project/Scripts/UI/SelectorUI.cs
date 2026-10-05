using System.Collections.Generic;
using Interview;
using UnityEngine;

public class SelectorUI : MonoBehaviour
{
    public InterviewSession interviewSession;
    public TextAsset[] textAssets;
    public InterviewData[] interviewDatas;
    public GameObject selectorRoot;
    public RectTransform buttonSelectorRoot;
    public ButtonSelector buttonHRPrefab;
    private const string JSON_FOLDER_PATH = "Interview";
    void Start()
    {
        Load();
        GenerateSelector();
    }

    private void Load()
    {
        textAssets = Resources.LoadAll<TextAsset>(JSON_FOLDER_PATH);
    }

    private void GenerateSelector()
    {
        float height = 0;
        interviewDatas = new InterviewData[textAssets.Length];
        for (int i = 0; i < textAssets.Length; i++)
        {
            InterviewData data = InterviewLoader.FromTextAsset(textAssets[i]);
            interviewDatas[i] = data;
            var spawned = Instantiate(buttonHRPrefab, buttonSelectorRoot);
            spawned.SetID(i);
            spawned.SetData(data);
            spawned.OnClick += SelectInterview;
            height += spawned.GetComponent<RectTransform>().sizeDelta.y;
        }
        //buttonSelectorRoot.sizeDelta = new Vector2(buttonSelectorRoot.sizeDelta.x, height);
    }

    private void SelectInterview(int id)
    {
        interviewSession.Load(interviewDatas[id]);
        selectorRoot.SetActive(false);
    }

    public void Show()
    {
        selectorRoot.SetActive(true);
    }
}
