using Interview;
using TMPro;

public class ButtonSelector : ButtonAnswer
{
    public TextMeshProUGUI logoText, descText, profText;

    public void SetData(InterviewData interviewData)
    {
        var (company, position) = StringSplitter.SplitByDash(interviewData.title);
        logoText.text = company;
        profText.text = position;
        descText.text = interviewData.description;
    }
}
