using System;
using System.Collections.Generic;
using Interview;
using UnityEngine;

public class UpdateItemList : MonoBehaviour
{
    public TextAsset[] textAssets;
    public InterviewData[] interviewDatas;
    public RectTransform root;
    public GameObject buttonHRPrefab;
    public ItemListUI itemListUI;
    public ChatUI chatUI;
    public float space;
    private const string JSON_FOLDER_PATH = "Interview";
    void Start()
    {
        itemListUI.OnSelectItem += OnSelectItem;
        Load();
        GenerateSelector();
    }

    private void OnSelectItem(int id)
    {
        chatUI.SetData(interviewDatas[id]);
    }

    private void Load()
    {
        textAssets = Resources.LoadAll<TextAsset>(JSON_FOLDER_PATH);
    }

    private void GenerateSelector()
    {
        float height = 0;
        interviewDatas = new InterviewData[textAssets.Length];
        List<ItemUI> items = new();
        for (int i = 0; i < textAssets.Length; i++)
        {
            InterviewData data = InterviewLoader.FromTextAsset(textAssets[i]);
            interviewDatas[i] = data;
            var spawned = Instantiate(buttonHRPrefab, root);
            var company = spawned.GetComponent<CompanyUI>();
            company.SetData(data);
            var item = spawned.GetComponent<ItemUI>();
            items.Add(item);
            height += spawned.GetComponent<RectTransform>().sizeDelta.y + space;
        }
        itemListUI.SetList(items);
        root.sizeDelta = new Vector2(root.sizeDelta.x, height);
    }
}
