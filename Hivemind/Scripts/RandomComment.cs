using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class RandomComment : MonoBehaviour {

    GameObject commentBox;
    Text text;
    float displayTime = 5f;
    
    List<string> comments = new List<string>()
    {
        "Now, where did i put those keys.",
        "What should we eat today, something fast and simple.",
        "I must be coming sick *sneeze*, only 1 more day, i just got to hold on till then.",
        "Should i take phone, tablet or computer, and if computer what kind?",
        "Thank goodness christmas is over,small children are just impossible.",
        "Was total miracle that i was at work in time today, i really need a new alarm clock.",
        "Really cant wait the new movies hit the theaters , kids will love it.",
        "That new tv and audio system really made everyone happy in the family.",
        "Damn this flu is killing me.",
        "This might actually work.",
        "This is just purely wrong.",
        "Am i ready for this?",
        "Hope the bank is still open after my work.",
        "My alcohol problem is getting out of hand, its already on the floor.",
        "Does this plant look different from yesterday, should i report this or not?",
        "This is just not may day.",
        "I really need some extra work hours, im so behind on bills.",
        "Early mornings are not my strong sides.",
        "Strange, its so damn quiet out here, did the planet die  or something ? .",
        "Im so hungry.",
        "I just want a nice, easy life. What's wrong with that?",
        "I'm too old to start again.",
        "Going try to avoid yearly employee interview, boss might notice i dont have proper insurance.",
        "I will pack my bags and move to alaska, I'm done with this.",
        "Just crossed my mind, I have never seen my  boss or higher staff, thats odd.",
        "I didnt agree to this crap when i took the job.",
        "The pace here, its  really killing me,  what next 18 hours  in a day hmmm?",
        "I'm so bored.",
        "Na naa naa na na.",
        "As a child, i was been told dolls were for girls.",
        "That summer seemed to last forever."
    };

    void Start () {
        commentBox = transform.parent.gameObject;
        text = GetComponent<Text>();
        //commentBox.SetActive(false);
    }
    
    void Hide()
    {
        text.text = "";
        commentBox.SetActive(false);
    }

    public void NewComment(string comment, float displayTime = 5f)
    {
        if (!commentBox) commentBox = transform.parent.gameObject;
        if (commentBox.activeInHierarchy) return;
        CancelInvoke();
        commentBox.SetActive(true);
        if (!text) text = GetComponent<Text>();
        text.text = comment;
        Invoke("Hide", displayTime);
    }

    public void NewRandomComment(float displayTime = 5f)
    {
        if (!commentBox) commentBox = transform.parent.gameObject;
        if (commentBox.activeInHierarchy) return;
        CancelInvoke();
        commentBox.SetActive(true);
        string comment = GetRandomComment();
        if (!text) text = GetComponent<Text>();
        text.text = comment;
        Invoke("Hide", displayTime);
    }

    public void GetComment(int index)
    {
        if ((index < 0) || (index >= comments.Count)) return;
        if (!commentBox) commentBox = transform.parent.gameObject;
        if (!text) text = GetComponent<Text>();
        CancelInvoke();
        commentBox.SetActive(true);
        text.text = comments[index];
    }

    string GetRandomComment()
    {
        return comments[Random.Range(0, comments.Count)];
    }

    public int GetCommentCount()
    {
        return comments.Count;
    }
}
