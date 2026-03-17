using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DialogueNode
{
    public string speakerName;
    [TextArea] public string dialogueText;
    public Choice[] choices;

    [Serializable]
    public class Choice
    {
        public string choiceText;
        public DialogueNode nextNode;
        public UnityEvent onChoiceSelected;
    }
}