using UnityEngine;
using Yarn.Unity;

public class DialogueCharacter : MonoBehaviour
{
    public DialogueRunner dialogueRunner;
    private void Awake()
    {
        dialogueRunner.AddCommandHandler("debug", Deebug);
        dialogueRunner.AddCommandHandler("say_yes", SayYes);
        dialogueRunner.AddCommandHandler("say_no", SayNo);
    }

    private void Deebug()
    {
        Debug.Log("Deebug() func");
    }
    private void SayYes()
    {
        Debug.Log("Choose Yes!");
    }
    private void SayNo()
    {
        Debug.Log("Choose No!!");
    }
}
